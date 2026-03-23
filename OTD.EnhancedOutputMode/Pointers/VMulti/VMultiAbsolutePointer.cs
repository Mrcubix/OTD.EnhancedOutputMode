using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static OTD.EnhancedOutputMode.Constants.VMultiModeConstants;

namespace OTD.EnhancedOutputMode.Pointers.VMulti
{
    public unsafe class VMultiAbsolutePointer : VMultiBasePointer, IAbsolutePointer
    {
        private readonly AbsoluteInputReport* _rawPointer;
        private readonly VMultiInstance<AbsoluteInputReport> _absoluteInstance;
        private Vector2 _conversionFactor;
        private Vector2 _prev;
        private bool _dirty;

        public VMultiAbsolutePointer(TabletReference tabletReference, IVirtualScreen virtualScreen)
        {
            var sharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
            _absoluteInstance = sharedStore.GetOrUpdate(ABS_INSTANCE, createInstance, out _);
            Instance = _absoluteInstance;
            _rawPointer = _absoluteInstance.Pointer;
            _conversionFactor = new Vector2(32767, 32767) / new Vector2(virtualScreen.Width, virtualScreen.Height);

            sharedStore.SetOrAdd(MODE, ABS_INSTANCE);

            static VMultiInstance<AbsoluteInputReport> createInstance()
            {
                return new VMultiInstance<AbsoluteInputReport>("VMultiAbs", new AbsoluteInputReport());
            }
        }

        public void SetPosition(Vector2 pos)
        {
            if (pos == _prev)
                return;

            pos *= _conversionFactor;
            _rawPointer->X = (ushort)pos.X;
            _rawPointer->Y = (ushort)pos.Y;
            _dirty = true;
            _prev = pos;
        }

        public override void Flush()
        {
            if (_dirty && Instance is not null)
            {
                _dirty = false;
                Instance.Write();
            }
        }
    }
}