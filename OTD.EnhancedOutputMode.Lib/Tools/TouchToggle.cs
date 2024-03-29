using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OTD.EnhancedOutputMode.Lib.Tools
{
    [PluginName("Touch Toggle")]
    public class TouchToggle : ITool
    {
        public bool Initialize()
        {
            return true;
        }

	    public void Dispose() {}

        [BooleanProperty("Toggle Touch", ""),
         DefaultPropertyValue(true),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "When Enabled, touch reports will be handled in Enhanced output modes."
                )
        ]
        public bool _isTouchToggled 
        { 
            get
            {
                return istouchToggled;
            }
            set
            {
                istouchToggled = value;
            }
        }

        public static bool istouchToggled;
    }
}
