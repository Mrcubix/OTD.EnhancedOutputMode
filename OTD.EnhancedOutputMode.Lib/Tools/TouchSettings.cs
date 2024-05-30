using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OTD.EnhancedOutputMode.Lib.Tools
{
    [PluginName("Touch Settings")]
    public class TouchSettings : IPositionedPipelineElement<IDeviceReport>
    {
        #region Fields

        private TimeSpan penResetTime; 
        private int maxX;
        private int maxY;

        #endregion

        public void Consume(IDeviceReport value) => Emit?.Invoke(value);

        #region Events

        public static event EventHandler<Vector2> MaxesChanged;
        public event Action<IDeviceReport> Emit;

        #endregion

        #region Properties

        public Vector2 Maxes => new(maxX, maxY);

        public PipelinePosition Position => PipelinePosition.None;

        #endregion

        #region Plugin Properties

        [BooleanProperty("Toggle Touch", ""),
         DefaultPropertyValue(true),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "When Enabled, touch reports will be handled in Enhanced output modes.")]
        public bool IsTouchToggled { get; set; }

        [BooleanProperty("Disable When Pen in Range", ""),
         DefaultPropertyValue(false),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "When Enabled, touch will be disabled when the pen is in range.")]
        public bool DisableWhenPenInRange { get; set; }

        [Property("Pen in Range Reset Time"),
         DefaultPropertyValue(100),
         Unit("ms"),
         ToolTip("OTD.EnhancedOutputMode:\n\n" +
                 "The time in milliseconds since the last pen report, before the pen is considered out of range.")]
        public int PenResetTime
        {
            get => penResetTime.Milliseconds;
            set => penResetTime = TimeSpan.FromMilliseconds(value);
        }

        public TimeSpan PenResetTimeSpan => penResetTime;

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

        #endregion

        #region Static Properties

        public static TouchSettings Default => new()
        {
            IsTouchToggled = true,
            DisableWhenPenInRange = false,
            PenResetTime = 100,
            MaxX = 4095,
            MaxY = 4095
        };

        #endregion
    }
}
