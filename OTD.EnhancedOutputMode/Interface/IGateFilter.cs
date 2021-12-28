using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Interface
{
    public interface IGateFilter
    {
        bool Pass(IDeviceReport report, ref ITabletReport tabletreport);
    }
}
