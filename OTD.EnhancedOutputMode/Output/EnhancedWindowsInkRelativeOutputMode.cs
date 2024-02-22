using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Windows Ink Relative Mode")]
    public class EnhancedWindowsInkRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly WinInkRelativePointer pointer = new();
        public override IRelativePointer Pointer => pointer;
    }
}