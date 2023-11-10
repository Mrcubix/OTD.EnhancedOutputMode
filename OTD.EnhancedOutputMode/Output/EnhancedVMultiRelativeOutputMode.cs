using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OTD.EnhancedOutputMode.Tablet;
using OTD.EnhancedOutputMode.Pointer;
using OTD.EnhancedOutputMode.Tool;
using System.Collections.Generic;
using System;
using OTD.EnhancedOutputMode.Lib.Interface;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Relative Mode")]
    public class EnhancedVMultiRelativeOutputMode : RelativeOutputMode
    {
        private readonly VMultiRelativePointer pointer = new();
        public override IRelativePointer Pointer => pointer;

        public IList<IGateFilter> GateFilters { get; set; } = Array.Empty<IGateFilter>();
        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();
        private bool _firstReport = true;
        private Vector2 _lastPos;
        private int _lastTouchID = -1;
        
        public override void Read(IDeviceReport report)
        {
            if (_firstReport && Filters != null)
            {
                GateFilters = Filters.OfType<IGateFilter>().ToList();
                AuxFilters = Filters.OfType<IAuxFilter>().ToList();
                _firstReport = false;
            }

            if (report is ITouchReport touchReport)
            {
                if (!TouchToggle.istouchToggled) return;

                ITabletReport touchConvertedReport = new TouchConvertedReport(report, _lastPos);

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

        private bool ShouldReport(IDeviceReport report, ref ITabletReport tabletreport)
        {
            foreach (var gateFilter in this.GateFilters)
                if (!gateFilter.Pass(report, ref tabletreport))
                    return false;
                    
            return true;
        }
    }
}