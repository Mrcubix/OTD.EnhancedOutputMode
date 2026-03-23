using OpenTabletDriver.Plugin.Platform.Pointer;
using VoiDPlugins.Library.VMulti;

namespace OTD.EnhancedOutputMode.Pointers.VMulti
{
    public abstract class VMultiBasePointer : IPenActionHandler, ISynchronousPointer
    {
        protected VMultiInstance? Instance { get; init; }

        public void Activate(PenAction action)
        {
            if (GetCode(action) is int code)
                Instance?.EnableButtonBit(code);
        }

        public void Deactivate(PenAction action)
        {
            if (GetCode(action) is int code)
                Instance?.DisableButtonBit(code);
        }

        public int? GetCode(PenAction button) => button switch
        {
            PenAction.Tip => 0x01,
            PenAction.Eraser => null, // eraser doesn't exist in vmulti mouse spec
            PenAction.BarrelButton1 => 0x02, // right mouse button
            PenAction.BarrelButton2 => 0x04, // middle mouse button
            PenAction.BarrelButton3 => null, // there are no backward or forward buttons in vmulti mouse spec
            _ => null
        };

        public void Reset()
        {
        }

        public abstract void Flush();
    }
}