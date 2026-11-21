using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;
using OpenTabletDriver.Plugin;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Relative Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly DummyRelativePointer _dummyPointer = new();
        private readonly VMultiRelativePointer pointer = new();
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