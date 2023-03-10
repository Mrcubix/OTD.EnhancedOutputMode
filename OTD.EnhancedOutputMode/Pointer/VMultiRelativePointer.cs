using OpenTabletDriver.Plugin.Platform.Pointer;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;

namespace OTD.EnhancedOutputMode.Pointer
{
    public class VMultiRelativePointer : BasePointer<RelativeInputReport>, IRelativePointer
    {
        public VMultiRelativePointer() : base(0x04, "VMultiRel")
        {
            ButtonHandler.SetReport(Report);
        }
    }
}