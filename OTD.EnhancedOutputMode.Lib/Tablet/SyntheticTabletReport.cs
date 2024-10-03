using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Tablet
{
    public class SyntheticTabletReport : ITabletReport
    {
        public byte[] Raw { get; set; }
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }

        public SyntheticTabletReport(ITabletReport report)
        {
            this.Raw = report.Raw;
            this.Position = report.Position;
            this.Pressure = report.Pressure;
            this.PenButtons = report.PenButtons;
        }
    }
}