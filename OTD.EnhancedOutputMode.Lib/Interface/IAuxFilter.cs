using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Interface
{
    public interface IAuxFilter
    {
        IAuxReport AuxFilter(IAuxReport report);
    }
}