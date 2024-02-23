using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Windows Ink Absolute Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedWindowsInkAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly WinInkAbsolutePointer pointer = new();
        public override IAbsolutePointer Pointer => pointer;
    }
}