using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Pointers.VMulti;
using VoiDPlugins.OutputMode;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Relative Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly DummyRelativePointer _dummyPointer = new();
        private VMultiRelativePointer? _pointer;

        public override TabletReference Tablet
        {
            get => base.Tablet;
            set
            {
                base.Tablet = value;
                _pointer = new VMultiRelativePointer(value);
            }
        }

        public override IRelativePointer Pointer
        {
            get
            {
                if (_pointer == null)
                {
                    Log.Write("Enhanced Output Mode", "Pointer not available, falling back to dummy pointer for now...");
                    return _dummyPointer;
                }
                else
                    return _pointer;
            }
            set { }
        }
    }
}