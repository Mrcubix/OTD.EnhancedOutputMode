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
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Tablet;
using OTD.EnhancedOutputMode.Tool;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Absolute Mode")]
    public class EnhancedAbsoluteOutputMode : AbsoluteOutputMode, IPointerOutputMode<IAbsolutePointer>
    {
        private IList<IFilter> filters, preFilters, postFilters;
        private Vector2 min, max;
        private bool _firstReport = true;
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private Vector2 _lastPos;

        protected Matrix3x2 touchTransformationMatrix;

        public override IAbsolutePointer Pointer => SystemInterop.AbsolutePointer;

        public IList<IGateFilter> GateFilters { get; set; } = Array.Empty<IGateFilter>();
        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();

        #region Initialization

        private void Initialize()
        {
            this.filters = Filters ?? Array.Empty<IFilter>();

            // Gather custom filters
            GateFilters = Filters.OfType<IGateFilter>().ToList();
            AuxFilters = Filters.OfType<IAuxFilter>().ToList();

            // Initialize filters that require initialization
            var initializationDependantFilters = Filters.OfType<IInitialize>().ToList();

            foreach (var filter in initializationDependantFilters)
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
            if (Input != null && Output != null && Tablet?.Digitizer != null)
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

        protected static Matrix3x2 CalculateTouchTransformation(Area input, Area output, DigitizerIdentifier tablet)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                tablet.Width / TouchSettings.maxX,
                tablet.Height / TouchSettings.maxY);

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
                if (!TouchToggle.istouchToggled) return;

                (_convertedReport as TouchConvertedReport).HandleReport(touchReport, _lastPos);

                if (ShouldReport(report, ref _convertedReport))
                {
                    if (_convertedReport.ReportID == 0)
                        return;

                    _lastPos = _convertedReport.Position;

                    if (TransposeTouch(_convertedReport) is Vector2 pos)
                    {
                        Pointer.SetPosition(pos);
                    }
                }
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