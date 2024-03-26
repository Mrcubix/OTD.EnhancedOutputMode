using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class EnhancedArtistMode : EnhancedAbsoluteOutputMode
    {
        private readonly EvdevVirtualTablet penHandler = new EvdevVirtualTablet();

        public override IAbsolutePointer Pointer => penHandler;
    }
}