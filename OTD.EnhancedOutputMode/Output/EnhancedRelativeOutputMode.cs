using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Timing;
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Lib.Tools;
using OTD.EnhancedOutputMode.Lib.Tablet;
using OpenTabletDriver.Plugin;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerOutputMode<IRelativePointer>
    {
        private IList<IFilter> _filters, preFilters, postFilters;
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private HPETDeltaStopwatch _Touchstopwatch = new(true);
        private HPETDeltaStopwatch _penStopwatch = new(true);
        private bool _firstReport = true;
        private int _lastTouchID = -1;
        private Vector2 _lastTransformedPos;
        private Vector2 _lastPos;
        private bool _skipReport = false;

        public Matrix3x2 TransformationMatrix { get; private set; }
        public Matrix3x2 TouchTransformationMatrix { get; private set; }

        public override IRelativePointer Pointer => SystemInterop.RelativePointer;

        public IList<IGateFilter> GateFilters { get; set; } = Array.Empty<IGateFilter>();
        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();
        public TouchSettings TouchSettings { get; private set; } = TouchSettings.Default;

        public void Initialize()
        {
            if (Filters == null) return;

            TouchSettings = TouchSettings.Instance ?? TouchSettings.Default;

            if (TouchSettings.Instance == null)
                Console.WriteLine("TouchSettings instance is null");

            GateFilters = Filters.OfType<IGateFilter>().ToList();
            AuxFilters = Filters.OfType<IAuxFilter>().ToList();

            FetchFilters();

            // Initialize filters that require initialization
            foreach (var filter in Filters.OfType<IInitialize>())
                filter.Initialize();

            this._skipReport = true;

            UpdateTransformMatrix();

            // Someone asked for a feature to match the pen's speed in relative mode
            if (TouchSettings.MatchPenSensibilityInRelativeMode)
                UpdateTouchTransformMatrix();

            // we don't want to initialize again
            _firstReport = false;
        }

        private void FetchFilters()
        {
            if (Info.Driver.InterpolatorActive)
                this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose).ToList();
            else
                this.preFilters = Filters.Where(t => t.FilterStage == FilterStage.PreTranspose || t.FilterStage == FilterStage.PreInterpolate).ToList();
            
            this.postFilters = Filters.Where(t => t.FilterStage == FilterStage.PostTranspose).ToList();
        }

        private void UpdateTransformMatrix()
        {
            TransformationMatrix = Matrix3x2.CreateRotation(
                (float)(-Rotation * System.Math.PI / 180));

            TransformationMatrix *= Matrix3x2.CreateScale(
                Sensitivity.X * ((Tablet?.Digitizer?.Width / Tablet?.Digitizer?.MaxX) ?? 0.01f),
                Sensitivity.Y * ((Tablet?.Digitizer?.Height / Tablet?.Digitizer?.MaxY) ?? 0.01f));

            TouchTransformationMatrix = TransformationMatrix;
        }

        // This is only used when 
        private void UpdateTouchTransformMatrix()
        {
            // Pen & Touch digitizer suffer from a difference in resolution, 
            // resulting in different speeds for the same sensitivity.
            var XMultiplier = Tablet.Digitizer.MaxX / TouchSettings.MaxX;
            var YMultiplier = Tablet.Digitizer.MaxY / TouchSettings.MaxY;

            // This should achieve about the same speed as the pen
            TouchTransformationMatrix = TransformationMatrix * Matrix3x2.CreateScale(XMultiplier, YMultiplier);
        }

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
                        Pointer.Translate(pos);

                    // Restart the stopwatch since the pen is in range
                    if (TouchSettings.DisableWhenPenInRange)
                        _penStopwatch.Restart();
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
            if (TouchSettings == null || !TouchSettings.IsTouchToggled) return false;

            // Check if the pen was in range recently and skip report if it was
            if (TouchSettings.DisableWhenPenInRange)
                if (_penStopwatch.Elapsed < TouchSettings.PenResetTimeSpan)
                    return false;

            (_convertedReport as TouchConvertedReport).HandleReport(touchReport, _lastPos);

            if (ShouldReport(report, ref _convertedReport))
            {
                _lastPos = _convertedReport.Position;

                if (TransposeTouch(_convertedReport) is Vector2 pos && _lastTouchID == TouchConvertedReport.CurrentFirstTouchID)
                    Pointer.Translate(pos);

                _lastTouchID = TouchConvertedReport.CurrentFirstTouchID;
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

        public Vector2? TransposeTouch(ITabletReport report)
        {
            var deltaTime = _Touchstopwatch.Restart();

            var pos = report.Position;

            // Pre Filter
            foreach (IFilter filter in this.preFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            pos = Vector2.Transform(pos, this.TouchTransformationMatrix);

            // Post Filter
            foreach (IFilter filter in this.postFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            var delta = pos - this._lastTransformedPos;
            this._lastTransformedPos = pos;

            if (_skipReport)
            {
                _skipReport = false;
                return null;
            }
            return (deltaTime > ResetTime) ? null : delta;
        }
    }
}