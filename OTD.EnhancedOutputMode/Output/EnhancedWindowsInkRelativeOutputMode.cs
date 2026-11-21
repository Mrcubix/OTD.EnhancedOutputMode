using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Windows Ink Relative Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedWindowsInkRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly DummyRelativePointer _dummyPointer = new();
        private readonly WinInkRelativePointer pointer = new();
        public override IRelativePointer Pointer
        {
            get
            {
                if (pointer == null)
                {
                    Log.Write("Enhanced Output Mode", "Pointer not available, falling back to dummy pointer for now...");
                    return _dummyPointer;
                }
                else
                    return pointer;
            }
        }
    }
}