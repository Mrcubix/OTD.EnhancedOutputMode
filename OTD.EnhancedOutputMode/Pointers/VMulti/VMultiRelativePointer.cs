using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static OTD.EnhancedOutputMode.Constants.VMultiModeConstants;

namespace OTD.EnhancedOutputMode.Pointers.VMulti
{
    public unsafe class VMultiRelativePointer : VMultiBasePointer, IRelativePointer
    {
        private readonly RelativeInputReport* _rawPointer;
        private readonly VMultiInstance<RelativeInputReport> _relativeInstance;
        private Vector2 _error;
        private Vector2 _delta;
        private bool _dirty;

        public VMultiRelativePointer(TabletReference tabletReference)
        {
            var sharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
            _relativeInstance = sharedStore.GetOrUpdate(REL_INSTANCE, createInstance, out var updated);
            Instance = _relativeInstance;
            _rawPointer = _relativeInstance.Pointer;

            sharedStore.SetOrAdd(MODE, REL_INSTANCE);

            static VMultiInstance<RelativeInputReport> createInstance()
            {
                return new VMultiInstance<RelativeInputReport>("VMultiRel", new RelativeInputReport());
            }
        }

        public void SetPosition(Vector2 delta)
        {
            if (delta == Vector2.Zero)
                return;
            _dirty = true;
            _delta = delta;
        }

        public override void Flush()
        {
            if (_dirty && Instance is not null)
            {
                _dirty = false;
                Send(_delta);
            }
        }

        private void Send(Vector2 delta)
        {
            var remaining = delta + _error;
            while (Math.Abs(remaining.X) > 127 || Math.Abs(remaining.Y) > 127)
            {
                var partialDelta = new Vector2(Math.Clamp(remaining.X, -127, 127), Math.Clamp(remaining.Y, -127, 127));
                _rawPointer->X = (byte)partialDelta.X;
                _rawPointer->Y = (byte)partialDelta.Y;
                Instance!.Write();
                remaining -= partialDelta;
            }

            _error = new Vector2(remaining.X % 1, remaining.Y % 1);
            _rawPointer->X = (byte)remaining.X;
            _rawPointer->Y = (byte)remaining.Y;
            Instance!.Write();
        }
    }
}