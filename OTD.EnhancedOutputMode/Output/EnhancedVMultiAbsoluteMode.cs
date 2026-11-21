using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Absolute Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly DummyAbsolutePointer _dummyPointer = new();
        private readonly VMultiAbsolutePointer pointer = new();
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