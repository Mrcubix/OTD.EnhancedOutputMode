using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OTD.EnhancedOutputMode.Pointer
{
    public class DummyAbsolutePointer : IAbsolutePointer
    {
        public DummyAbsolutePointer()
        {
        }

        public void SetPosition(Vector2 pos)
        {
        }
    }

    public class DummyRelativePointer : IRelativePointer
    {
        public DummyRelativePointer()
        {
        }

        public void Translate(Vector2 pos)
        {
        }
    }
}