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
using OTD.EnhancedOutputMode.Interface;
using OTD.EnhancedOutputMode.Tablet;
using OTD.EnhancedOutputMode.Tool;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Absolute Mode")]
    public class EnhancedAbsoluteOutputMode : AbsoluteOutputMode, IPointerOutputMode<IAbsolutePointer>
    {
        public override IAbsolutePointer Pointer => SystemInterop.AbsolutePointer;
        private IList<IGateFilter> gateFilters = Array.Empty<IGateFilter>();
        public IList<IGateFilter> GateFilters
        {
            set => gateFilters = value ?? Array.Empty<IGateFilter>();
            get => this.gateFilters;
        }
        public bool firstReport = true;
        public Vector2 lastPos;
        
        public override void Read(IDeviceReport report)
        {
            if (firstReport)
            {
                firstReport = false;
                GateFilters = Filters.OfType<IGateFilter>().ToList();
            }

            if (report is ITouchReport touchReport)
            {
                if (!TouchToggle.istouchToggled) return;
                 
                TouchConvertedReport touchConvertedReport = new TouchConvertedReport(report, lastPos);

                lastPos = touchConvertedReport.Position;

                if (Transpose(touchConvertedReport) is Vector2 pos)
                {
                    Pointer.SetPosition(pos);
                }
            }
            else if (report is ITabletReport tabletReport)
            {
                if (Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID) && ShouldReport(report, ref tabletReport))
                {
                    if (Pointer is IVirtualTablet pressureHandler)
                        pressureHandler.SetPressure((float)tabletReport.Pressure / (float)Tablet.Digitizer.MaxPressure);

                    lastPos = tabletReport.Position;

                    if (Transpose(tabletReport) is Vector2 pos)
                    {
                        Pointer.SetPosition(pos);
                    }
                }
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