using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Tools
{
    [PluginName("Touch Settings")]
    public class TouchSettings : ITool
    {
        public bool Initialize() => true;

        public void Dispose() {}

        #region Events

        public static event EventHandler<Vector2> MaxesChanged;

        #endregion

        #region Properties

        [BooleanProperty("Toggle Touch", ""),
         DefaultPropertyValue(true),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "When Enabled, touch reports will be handled in Enhanced output modes."
                )
        ]
        public bool IsTouchToggled 
        { 
            get => istouchToggled;
            set => istouchToggled = value;
        }

        public static bool istouchToggled;

        #region Max X

        [Property("Max X"),
         DefaultPropertyValue(4095),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "The maximum X value of the touch digitizer. \n" +
                 "Check the debugger for the correct value.")]
        public int MaxX
        {
            get => maxX;
            set
            {
                maxX = Math.Max(0, value);
                MaxesChanged?.Invoke(this, Maxes);
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
            get => maxY;
            set
            {
                maxY = Math.Max(0, value);
                MaxesChanged?.Invoke(this, Maxes);
            }
        }

        public static int maxY;

        #endregion

        public static Vector2 Maxes => new(maxX, maxY);

        public FilterStage FilterStage => FilterStage.PreTranspose;

        #endregion
    }
}
