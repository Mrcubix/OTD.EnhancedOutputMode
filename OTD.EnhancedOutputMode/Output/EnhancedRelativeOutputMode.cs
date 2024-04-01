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
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Lib.Tools;
using OTD.EnhancedOutputMode.Tablet;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerOutputMode<IRelativePointer>
    {
        public override IRelativePointer Pointer => SystemInterop.RelativePointer;

        public IList<IGateFilter> GateFilters { get; set; } = Array.Empty<IGateFilter>();
        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();
        private bool _firstReport = true;
        private Vector2 _lastPos;
        private int _lastTouchID = -1;

        public void Initialize()
        {
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

            if (report is ITouchReport touchreport)
            {
                if (!TouchSettings.istouchToggled) return;
                
                ITabletReport touchConvertedReport = new TouchConvertedReport(touchreport, _lastPos);

                if (ShouldReport(report, ref touchConvertedReport))
                {
                    _lastPos = touchConvertedReport.Position;

                    if (Transpose(touchConvertedReport) is Vector2 pos && _lastTouchID == TouchConvertedReport.CurrentFirstTouchID)
                    {
                        Pointer.Translate(pos);
                    }

                    _lastTouchID = TouchConvertedReport.CurrentFirstTouchID;
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
                        Pointer.Translate(pos);
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
    }
}