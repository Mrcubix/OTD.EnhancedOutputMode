using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OTD.EnhancedOutputMode.Tablet
{
    public class TouchConvertedReport: ITabletReport
    {
        public static int CurrentFirstTouchID { get; set; } = -1;

        public byte[] Raw { get; set; }
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }
        public bool InRange { get; set; }

        public TouchConvertedReport(IDeviceReport report, Vector2 lastPos)
        {
            Raw = report.Raw;
            PenButtons = new bool[] {};

            if (report is ITouchReport touchreport)
            {
                HandleReport(touchreport, lastPos);
            }
            else if (report is ITabletReport tabletreport)
            {
                Position = tabletreport.Position;
                Pressure = tabletreport.Pressure;
                PenButtons = tabletreport.PenButtons;
                Log.Write("OTD.EnhancedOutputMode", "Report is ITabletReport when ITouchReport is expected. \nWarning occured in OpenTabletDriver.EnhancedOutputMode.Tablet.TouchConvertedReport", LogLevel.Warning);
            }
        }

        public TouchConvertedReport(ITouchReport report, Vector2 lastPos)
        {
            Raw = report.Raw;
            PenButtons = new bool[] {};
            
            HandleReport(report, lastPos);
        }

        public TouchConvertedReport()
        {
            Raw = new byte[] {};
            PenButtons = new bool[] {};
        }

        public void HandleReport(ITouchReport touchReport, Vector2 lastPos)
        {
            TouchPoint firstTouch = null!;

            /*foreach(TouchPoint point in touchReport.Touches)
                if ((firstTouch = point) != null)
                    break;*/

            // The current first touch from the previous report might have been cached, we still need to check if it's still valid
            if (CurrentFirstTouchID != -1)
                firstTouch = touchReport.Touches[CurrentFirstTouchID];

            // If the cached first touch is no longer valid, we need to find a new one
            if (firstTouch == null)
            {
                int index = 0;

                while (firstTouch == null && index < touchReport.Touches.Length)
                {
                    firstTouch = touchReport.Touches[index];
                    CurrentFirstTouchID = index;
                    index++;
                }
            }

            // in case we still don't have a valid touch, we'll just use the last known position, else we can use the current touch position
            if (firstTouch != null)
            {
                Position = firstTouch.Position;
                Pressure = 1;
                InRange = true;
            }
            else
            {
                Position = lastPos;
                Pressure = 0;
                InRange = false;
            }

            PenButtons = new bool[] {false};
        }
    }
}