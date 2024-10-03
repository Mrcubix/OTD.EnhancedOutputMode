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

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerOutputMode<IRelativePointer>
    {
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private HPETDeltaStopwatch _penStopwatch = new(true);
        private bool _firstReport = true;
        private int _lastTouchID = -1;
        private Vector2 _lastPos;

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

            // Initialize filters that require initialization
            foreach (var filter in Filters.OfType<IInitialize>())
                filter.Initialize();

            // we don't want to initialize again
            _firstReport = false;
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

                if (Transpose(_convertedReport) is Vector2 pos && _lastTouchID == TouchConvertedReport.CurrentFirstTouchID)
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
    }
}