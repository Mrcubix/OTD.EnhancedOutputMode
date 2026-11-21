using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Windows Ink Absolute Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedWindowsInkAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly DummyAbsolutePointer _dummyPointer = new();
        private readonly WinInkAbsolutePointer pointer = new();
        public override IAbsolutePointer Pointer
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