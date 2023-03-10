using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;

namespace OTD.EnhancedOutputMode.Pointer
{
    public class WinInkAbsolutePointer : BasePointer<DigitizerInputReport>, IVirtualTablet
    {
        public WinInkAbsolutePointer() : base(0x05, "WindowsInk")
        {
            WinInkButtonHandler.SetReport(Report);
            WinInkButtonHandler.SetDevice(Device);
        }

        public override void SetPosition(Vector2 pos)
        {
            var newPos = Convert(pos);
            Report.X = (ushort)newPos.X;
            Report.Y = (ushort)newPos.Y;
            Device.Write(Report.ToBytes());
        }

        public void SetPressure(float percentage)
        {
            Report.Pressure = (ushort)(percentage * 8191);
        }
    }
}