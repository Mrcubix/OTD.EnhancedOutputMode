using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;
using OpenTabletDriver.Plugin;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Relative Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly VMultiRelativePointer pointer = new();
        public override IRelativePointer Pointer => pointer;
    }
}