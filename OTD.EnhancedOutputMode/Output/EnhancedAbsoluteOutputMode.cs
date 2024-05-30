using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Timing;
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Lib.Tools;
using OTD.EnhancedOutputMode.Tablet;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Absolute Mode")]
    public class EnhancedAbsoluteOutputMode : AbsoluteOutputMode, IPointerOutputMode<IAbsolutePointer>
    {
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private HPETDeltaStopwatch _penStopwatch = new(true);
        private IList<IFilter> filters, preFilters, postFilters;
        private bool _firstReport = true;
        private Vector2 _lastPos;
        private Vector2 min, max;

        protected Matrix3x2 touchTransformationMatrix;

        public override IAbsolutePointer Pointer => SystemInterop.AbsolutePointer;

        public IList<IGateFilter> GateFilters { get; private set; } = Array.Empty<IGateFilter>();
        public IList<IAuxFilter> AuxFilters { get; private set; } = Array.Empty<IAuxFilter>();
        public TouchSettings TouchSettings { get; private set; }

        #region Initialization

        private void Initialize()
        {
            if (Filters == null) return;

            this.filters = Filters ?? Array.Empty<IFilter>();
            TouchSettings = TouchSettings.Instance ?? TouchSettings.Default;

            // Gather custom filters
            GateFilters = Filters.OfType<IGateFilter>().ToList();
            AuxFilters = Filters.OfType<IAuxFilter>().ToList();

            // Initialize filters that require initialization
            foreach (var filter in Filters.OfType<IInitialize>())
                filter.Initialize();

            // Set pre and post filters
            if (Info.Driver.InterpolatorActive)
                this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose).ToList();
            else
                this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose || t.FilterStage == FilterStage.PreInterpolate).ToList();

            this.postFilters = filters.Where(t => t.FilterStage == FilterStage.PostTranspose).ToList();

            UpdateTouchTransformMatrix();

            // we don't want to initialize again
            _firstReport = false;
        }

        #endregion

        #region Matrix Calculation

        protected void UpdateTouchTransformMatrix()
        {
            if (Input != null && Output != null && Tablet?.Digitizer != null && TouchSettings != null)
                this.touchTransformationMatrix = CalculateTouchTransformation(Input, Output, Tablet.Digitizer);

            var halfDisplayWidth = Output?.Width / 2 ?? 0;
            var halfDisplayHeight = Output?.Height / 2 ?? 0;

            var minX = Output?.Position.X - halfDisplayWidth ?? 0;
            var maxX = Output?.Position.X + Output?.Width - halfDisplayWidth ?? 0;
            var minY = Output?.Position.Y - halfDisplayHeight ?? 0;
            var maxY = Output?.Position.Y + Output?.Height - halfDisplayHeight ?? 0;

            this.min = new Vector2(minX, minY);
            this.max = new Vector2(maxX, maxY);
        }

        protected Matrix3x2 CalculateTouchTransformation(Area input, Area output, DigitizerIdentifier tablet)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                tablet.Width / TouchSettings.MaxX,
                tablet.Height / TouchSettings.MaxY);

            // Translate to the center of input area
            res *= Matrix3x2.CreateTranslation(
                -input.Position.X, -input.Position.Y);

            // Apply rotation
            res *= Matrix3x2.CreateRotation(
                (float)(-input.Rotation * Math.PI / 180));

            // Scale millimeters to pixels
            res *= Matrix3x2.CreateScale(
                output.Width / input.Width, output.Height / input.Height);

            // Translate output to virtual screen coordinates
            res *= Matrix3x2.CreateTranslation(
                output.Position.X, output.Position.Y);

            return res;
        }

        #endregion

        #region Report Handling

        public override void Read(IDeviceReport report)
        {
            if (_firstReport && Filters != null)
                Initialize();

            if (report is ITouchReport touchReport)
            {
                if (HandleTouch(report, touchReport) == false)
                    return;
            }
            else if (report is ITabletReport tabletReport)
            {
                if (Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID) && ShouldReport(report, ref tabletReport))
                {
                    if (Pointer is IVirtualTablet pressureHandler)
                        pressureHandler.SetPressure((float)tabletReport.Pressure / (float)Tablet.Digitizer.MaxPressure);

                    _lastPos = tabletReport.Position;

                    if (Transpose(tabletReport) is Vector2 pos)
                    {
                        Pointer.SetPosition(pos);
                    }
                }
            }
            else if (report is IAuxReport auxReport)
            {
                foreach (var auxFilter in AuxFilters)
                    auxReport = auxFilter.AuxFilter(auxReport);
            }
        }

        protected virtual bool HandleTouch(IDeviceReport report, ITouchReport touchReport)
        {
            if (!TouchSettings.IsTouchToggled) return false;

            // Check if the pen was in range recently and skip report if it was
            if (TouchSettings.DisableWhenPenInRange)
                if (_penStopwatch.Elapsed < TouchSettings.PenResetTimeSpan)
                    return false;

            (_convertedReport as TouchConvertedReport).HandleReport(touchReport, _lastPos);

            if (ShouldReport(report, ref _convertedReport))
            {
                if (_convertedReport.ReportID == 0)
                    return false;

                _lastPos = _convertedReport.Position;

                if (TransposeTouch(_convertedReport) is Vector2 pos)
                    Pointer.SetPosition(pos);
            }

            return true;
        }

        protected bool ShouldReport(IDeviceReport report, ref ITabletReport tabletreport)
        {
            foreach (var gateFilter in this.GateFilters)
                if (!gateFilter.Pass(report, ref tabletreport))
                    return false;

            return true;
        }

        #endregion

        #region Touch Transposition

        protected Vector2? TransposeTouch(ITabletReport report)
        {
            var pos = new Vector2(report.Position.X, report.Position.Y);

            // Pre Filter
            foreach (IFilter filter in this.preFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            // Apply transformation
            pos = Vector2.Transform(pos, this.touchTransformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, this.min, this.max);
            if (AreaLimiting && clippedPoint != pos)
                return null;

            if (AreaClipping)
                pos = clippedPoint;

            // Post Filter
            foreach (IFilter filter in this.postFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            return pos;
        }

        #endregion
    }
}