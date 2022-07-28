using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Interface;

namespace yep
{
    public class SomeFilter : IFilter, IGateFilter
    {
        public Vector2 Filter(Vector2 input) 
        {
            return input;
        }

        public bool Pass(IDeviceReport report, ref ITabletReport tabletreport)
        {
            
            if (tabletreport.Pressure > 0)
            {
                // Affect the input in some way
                return true;
            }
            else
            {
                return false;
            }
        }

        public FilterStage FilterStage => FilterStage.PreTranspose;
    }
}