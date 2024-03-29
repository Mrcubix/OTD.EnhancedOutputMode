using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Absolute Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly VMultiAbsolutePointer pointer = new();
        public override IAbsolutePointer Pointer => pointer;
    }
}