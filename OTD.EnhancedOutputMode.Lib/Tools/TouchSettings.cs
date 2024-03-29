using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OTD.EnhancedOutputMode.Lib.Tools
{
    [PluginName("Touch Settings")]
    public class TouchSettings : ITool
    {
        public bool Initialize()
        {
            return true;
        }

	    public void Dispose() {}

        #region Properties

        #region Max X

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
                maxX = Math.Max(0, value);
            }
        }

        public static int maxX;

        #endregion

        #region Max Y

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
                maxY = Math.Max(0, value);
            }
        }

        public static int maxY;

        #endregion

        #endregion

        public static Vector2 Maxes => new(maxX, maxY);
    }
}
