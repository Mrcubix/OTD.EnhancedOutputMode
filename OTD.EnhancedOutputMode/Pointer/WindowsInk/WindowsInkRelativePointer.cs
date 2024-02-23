using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Enums;

namespace OTD.EnhancedOutputMode.Pointer.WindowsInk
{
    public unsafe class WindowsInkRelativePointer : WinInkBasePointer, IRelativePointer
    {
        private Vector2 _maxPoint;
        private Vector2 _currentPoint;

        public WindowsInkRelativePointer(TabletReference tabletReference, IVirtualScreen screen)
            : base("Windows Ink", tabletReference, screen)
        {
            _maxPoint = new Vector2(screen.Width, screen.Height);
            _currentPoint = _maxPoint / 2;
        }

        public void SetPosition(Vector2 delta)
        {
            if (delta == Vector2.Zero)
                return;

            _currentPoint = Vector2.Clamp(_currentPoint + delta, Vector2.Zero, _maxPoint);

            SetInternalPosition(_currentPoint);
            Instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
            var pos = Convert(_currentPoint);
            RawPointer->X = (ushort)pos.X;
            RawPointer->Y = (ushort)pos.Y;
            Dirty = true;
        }
    }
}