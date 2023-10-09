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
            Raw = report.Raw;
            ReportID = 1;

            if (report is ITouchReport touchreport)
            {
                Initialize(touchreport, lastPos);
            }
            else if (report is ITabletReport tabletreport)
            {
                Position = tabletreport.Position;
                Pressure = tabletreport.Pressure;
                PenButtons = tabletreport.PenButtons;
                Log.Write("OTD.EnhancedOutputMode", "Report is ITabletReport when ITouchReport is expected.\nWarning occured in OpenTabletDriver.EnhancedOutputMode.Tablet.TouchConvertedReport", LogLevel.Warning);
            }
        }

        public TouchConvertedReport(ITouchReport report, Vector2 lastPos)
        {
            Raw = report.Raw;
            ReportID = 1;
            
            Initialize(report, lastPos);
        }

        private void Initialize(ITouchReport touchReport, Vector2 lastPos)
        {
            TouchPoint firstTouch = null;

            foreach(TouchPoint point in touchReport.Touches)
                if ((firstTouch = point) != null)
                    break;

            if (firstTouch != null)
            {
                Position = firstTouch.Position;
                Pressure = 1;
            }
            else
            {
                Position = lastPos;
                Pressure = 0;
            }

            PenButtons = new bool[] {false};
        }
    }
}