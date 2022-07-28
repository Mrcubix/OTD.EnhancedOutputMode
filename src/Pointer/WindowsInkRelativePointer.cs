using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;

namespace OTD.EnhancedOutputMode.Pointer
{
    public class WinInkRelativePointer : BasePointer<DigitizerInputReport>, IVirtualTablet
    {
        private Vector2 nullPoint = new(0, 0);
        private Vector2 maxPoint;
        private Vector2 currentPoint;

        public WinInkRelativePointer() : base(0x05, "WindowsInk")
        {
            WinInkButtonHandler.SetReport(Report);
            WinInkButtonHandler.SetDevice(Device);
            maxPoint = new Vector2(VirtualScreen.Width, VirtualScreen.Height);
            currentPoint = maxPoint / 2;
        }

        public void SetPressure(float percentage)
        {
            Report.Pressure = (ushort)(percentage * 8191);
        }

        public override void Translate(Vector2 delta)
        {
            currentPoint = Vector2.Clamp(currentPoint + delta, nullPoint, maxPoint);
            base.SetPosition(currentPoint);
        }
    }
}