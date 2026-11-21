using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Output;
using OTD.EnhancedOutputMode.Pointers.VMulti;
using VoiDPlugins.OutputMode;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Absolute Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class EnhancedVMultiAbsoluteMode : EnhancedAbsoluteOutputMode
    {
        private readonly DummyAbsolutePointer _dummyPointer = new();
        private VMultiAbsolutePointer? _pointer;
        private IVirtualScreen? _virtualScreen;

        [Resolved]
        public IServiceProvider ServiceProvider
        {
            set => _virtualScreen = (IVirtualScreen)value.GetService(typeof(IVirtualScreen))!;
        }

        public override TabletReference Tablet
        {
            get => base.Tablet;
            set
            {
                base.Tablet = value;
                _pointer = new VMultiAbsolutePointer(value, _virtualScreen!);
            }
        }

        public override IAbsolutePointer Pointer
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