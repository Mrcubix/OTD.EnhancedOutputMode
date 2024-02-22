using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Windows Ink Absolute Mode")]
    public class EnhancedWindowsInkAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly WinInkAbsolutePointer pointer = new();
        public override IAbsolutePointer Pointer => pointer;
    }
}