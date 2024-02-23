using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VoiD;
using static OTD.EnhancedOutputMode.Constants.VMultiModeConstants;

namespace OTD.EnhancedOutputMode.Handlers
{
    [PluginName("VMulti Mode")]
    public class VMultiButtonHandler : IStateBinding
    {
        private VMultiInstance? _instance;

        public static Dictionary<string, int> Bindings { get; } = new()
        {
            { "Left", 1 },
            { "Right", 2 },
            { "Middle", 4 }
        };

        public static string[] ValidButtons { get; } = Bindings.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button { get; set; }

        [TabletReference]
        public TabletReference Reference { set => Initialize(value); }

        private void Initialize(TabletReference tabletReference)
        {
            try
            {
                var sharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
                var mode = sharedStore.Get<int>(MODE);
                _instance = sharedStore.Get<VMultiInstance>(mode);
            }
            catch
            {
                Log.Write("VMulti",
                          "VMulti bindings are being used without an active VMulti output mode.",
                          LogLevel.Error);
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            _instance.EnableButtonBit(Bindings[Button!]);
            _instance.Write();
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            _instance.DisableButtonBit(Bindings[Button!]);
            _instance.Write();
        }
    }
}