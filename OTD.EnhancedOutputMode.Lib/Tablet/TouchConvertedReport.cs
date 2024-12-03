using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OTD.EnhancedOutputMode.Lib.Tablet
{
    public class TouchConvertedReport : ITabletReport//, ITouchReport
    {
        public int CurrentFirstTouchID { get; set; } = -1;

        public byte[] Raw { get; set; } = Array.Empty<byte>();
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; } = Array.Empty<bool>();
        public bool InRange { get; set; }

        //public TouchPoint[] Touches { get; set; }

        public TouchConvertedReport(IDeviceReport report, Vector2 lastPos)
        {
            Raw = report.Raw;
            PenButtons = new bool[] {};

            if (report is ITouchReport touchreport)
                HandleReport(touchreport, lastPos);
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
            HandleReport(report, lastPos);
        }

        public TouchConvertedReport()
        {
        }

        /// <summary>
        ///   Handles the touch report and sets the position accordingly.
        /// </summary>
        /// <param name="touchReport">The touch report to handle.</param>
        /// <param name="lastPos">The last known position.</param>
        /// <remarks>
        ///   This method works without issues in 0.5, however, further handling is required in 0.6 as a large delta is being caused by something.
        /// </remarks>
        public void HandleReport(ITouchReport touchReport, Vector2 lastPos)
        {
            TouchPoint firstTouch = null;

            // Touch ID stays the same until the touch is released
            if (CurrentFirstTouchID != -1)
                firstTouch = touchReport.Touches.FirstOrDefault(point => point != null && point.TouchID == CurrentFirstTouchID);

            // If the cached first touch is no longer valid, we need to find a new one
            if (firstTouch == null)
            {
                int index = 0;

                while (firstTouch == null && index < touchReport.Touches.Length)
                {
                    firstTouch = touchReport.Touches[index];
                    CurrentFirstTouchID = firstTouch?.TouchID ?? -1;
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
        }
    }
}