using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Interface
{
    public interface IGateFilter
    {
        bool Pass(IDeviceReport report, ref ITabletReport tabletreport);
    }
}
