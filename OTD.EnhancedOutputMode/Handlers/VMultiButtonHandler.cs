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
    [PluginName("Enhanced VMulti Mode")]
    public class VMultiButtonHandler : IStateBinding
    {
        private VMultiInstance? _instance;

        private string? _button = "Left";
        private int _finalButton;

        public static Dictionary<string, int> Bindings { get; } = new()
        {
            { "Left", 1 },
            { "Right", 2 },
            { "Middle", 4 }
        };

        public static string[] ValidButtons { get; } = Bindings.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button
        {
            set
            {
                _button = value;
                
                Bindings.TryGetValue(value!, out _finalButton);

                // Fallback to Left if an invalid button is selected
                if (_finalButton == 0) 
                {
                    _finalButton = 1;
                    Log.Write("Enhanced VMulti Mode", $"Invalid value '{value}' for Button property. Falling back to 'Left' instead.", LogLevel.Warning);
                }
            }
            get => _button;
        }
        

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
                Log.Write("Enhanced VMulti Bindings",
                          "VMulti bindings are being used without an active Enhanced VMulti Output Mode.",
                          LogLevel.Error);
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            _instance.EnableButtonBit(_finalButton);
            _instance.Write();
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            _instance.DisableButtonBit(_finalButton);
            _instance.Write();
        }
    }
}