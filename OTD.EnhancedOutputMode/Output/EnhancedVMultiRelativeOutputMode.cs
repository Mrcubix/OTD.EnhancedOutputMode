using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OTD.EnhancedOutputMode.Tablet;
using OTD.EnhancedOutputMode.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced VMulti Relative Mode")]
    public class EnhancedVMultiRelativeOutputMode : EnhancedRelativeOutputMode
    {
        private readonly VMultiRelativePointer pointer = new();
        public override IRelativePointer Pointer => pointer;
    }
}