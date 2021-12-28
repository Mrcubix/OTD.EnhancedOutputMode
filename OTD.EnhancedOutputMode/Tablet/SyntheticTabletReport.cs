using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Tablet
{
    public class SyntheticTabletReport : ITabletReport
    {
        public byte[] Raw { get; set; }
        public uint ReportID { get; set; }
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }

        public SyntheticTabletReport(ITabletReport report)
        {
            this.Raw = report.Raw;
            this.ReportID = report.ReportID;
            this.Position = report.Position;
            this.Pressure = report.Pressure;
            this.PenButtons = report.PenButtons;
        }
    }
}