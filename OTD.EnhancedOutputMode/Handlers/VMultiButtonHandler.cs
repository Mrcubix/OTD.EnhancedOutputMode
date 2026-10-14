using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
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
                    Log.Write("Enhanced VMulti Bindings", $"Invalid value '{value}' for Button property. Falling back to 'Left' instead.", LogLevel.Warning);
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
                          "Enhanced VMulti Bindings are being used without an active Enhanced VMulti Output Mode.",
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

        // Adaptive Binding Support
        public static void Activate(PenAction action, VMultiInstance instance)
        {
            if (instance == null)
                return;

            if (GetCode(action) is { } code)
            {
                instance.EnableButtonBit(code);
                instance.Write();
            }
        }

        public static void MouseUp(MouseButton button, VMultiInstance? Instance)
        {
            if (Instance == null)
                return;

            if (GetCode(button) is { } code)
            {
                Instance.EnableButtonBit(code);
                Instance.Write();
            }
        }

        public static void Deactivate(PenAction action, VMultiInstance instance)
        {
            if (instance == null)
                return;

            if (GetCode(action) is { } code)
            {
                instance.DisableButtonBit(code);
                instance.Write();
            }
        }

        public static void MouseDown(MouseButton button, VMultiInstance? instance)
        {
            if (instance == null)
                return;

            if (GetCode(button) is { } code)
            {
                instance.DisableButtonBit(code);
                instance.Write();
            }
        }

        private static int? GetCode(PenAction button) => button switch
        {
            PenAction.Tip => Bindings["Left"],
            PenAction.Eraser => Bindings["Left"],
            PenAction.BarrelButton1 => Bindings["Right"],
            PenAction.BarrelButton2 => Bindings["Middle"],
            PenAction.BarrelButton3 => null,
            _ => null,
        };

        private static int? GetCode(MouseButton button) => button switch
        {
            MouseButton.Left => Bindings["Left"],
            MouseButton.Middle => Bindings["Middle"],
            MouseButton.Right => Bindings["Right"],
            _ => null,
        };
    }
}