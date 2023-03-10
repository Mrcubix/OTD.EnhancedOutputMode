using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OTD.EnhancedOutputMode.Tablet
{
    public class TouchConvertedReport: ITabletReport
    {
        public byte[] Raw { get; set; }
        public uint ReportID { get; set; }
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }

        public TouchConvertedReport(IDeviceReport report, Vector2 lastPos)
        {
            this.Raw = report.Raw;

            if (report is ITouchReport touchreport)
            {
                this.ReportID = 1;
                
                TouchPoint firstTouch = null;
                foreach(TouchPoint point in touchreport.Touches)
                    if ((firstTouch = point) != null)
                        break;

                if (firstTouch != null)
                {
                    this.Position = firstTouch.Position;
                    this.Pressure = 1;
                }
                else
                {
                    this.Position = lastPos;
                    this.Pressure = 0;
                }

                this.PenButtons = new bool[] {false};
            }
            else if (report is ITabletReport tabletreport)
            {
                this.Position = tabletreport.Position;
                this.Pressure = tabletreport.Pressure;
                this.PenButtons = tabletreport.PenButtons;
                Log.Write("OTD.EnhancedOutputMode", "Report is ITabletReport when ITouchReport is expected.\nWarning occured in OpenTabletDriver.EnhancedOutputMode.Tablet.TouchConvertedReport", LogLevel.Warning);
            }
        }
    }
}