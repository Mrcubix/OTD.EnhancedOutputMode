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

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerOutputMode<IRelativePointer>
    {
        public override IRelativePointer Pointer => SystemInterop.RelativePointer;
        public Vector2 lastPos;
        public override void Read(IDeviceReport report)
        {
            if (report is ITouchReport touchReport)
            {
                Log.Debug("OTD.EnhancedOutputMode", $"Last Recorded Position: {lastPos}");
                
                TouchConvertedReport touchConvertedReport = new TouchConvertedReport(report, lastPos);
                
                lastPos = touchConvertedReport.Position;

                Log.Debug("OTD.EnhancedOutputMode", $"TouchConvertedReport: {touchConvertedReport.Position}");

                if (Transpose(touchConvertedReport) is Vector2 pos)
                {
                    Pointer.Translate(pos);
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
                        Pointer.Translate(pos);
                    }
                }
            }
        }

        private bool ShouldReport(IDeviceReport report, ref ITabletReport tabletreport)
        {
            foreach (var gateFilter in Filters.OfType<IGateFilter>())
                if (!gateFilter.Pass(report, ref tabletreport))
                    return false;
                    
            return true;
        }
    }
}