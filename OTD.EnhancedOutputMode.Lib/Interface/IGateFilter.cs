using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Interface
{
    // NOTE: Deprecated as fuck
    public interface IGateFilter
    {
        bool Pass(IDeviceReport report, ref ITabletReport tabletreport);
    }
}
