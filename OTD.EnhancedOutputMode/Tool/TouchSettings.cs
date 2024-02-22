using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OTD.EnhancedOutputMode.Tool
{
    [PluginName("Touch Settings")]
    public class TouchSettings : ITool
    {
        public bool Initialize()
        {
            return true;
        }

	    public void Dispose() {}

        [Property("Max X"),
         DefaultPropertyValue(4095),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "The maximum X value of the touch digitizer. \n" +
                 "Check the debugger for the correct value.")]
        public int MaxX
        {
            get
            {
                return maxX;
            }
            set
            {
                maxX = value;
            }
        }

        public static int maxX;

        [Property("Max Y"),
         DefaultPropertyValue(4095),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "The maximum Y value of the touch digitizer. \n" +
                 "Check the debugger for the correct value.")]
        public int MaxY
        {
            get
            {
                return maxY;
            }
            set
            {
                maxY = value;
            }
        }

        public static int maxY;
    }
}
