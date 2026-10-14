using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Enums;
using VoiDPlugins.Library.VMulti;
using VoiDPlugins.Library.VMulti.Device;
using VoiDPlugins.Library.VoiD;
using static OTD.EnhancedOutputMode.Constants.WindowsInkConstants;

namespace OTD.EnhancedOutputMode.Handlers
{
    [PluginName("Enhanced Windows Ink")]
    public unsafe partial class WindowsInkButtonHandler : IStateBinding
    {
        private VMultiInstance _instance = null!;
        private SharedStore _sharedStore = null!;
        private string? _button = "Pen Tip";
        private WinInkButtonBindings _finalButton;

        public static Dictionary<string, WinInkButtonBindings> Bindings { get; } = new()
        {
            { "Pen Tip", WinInkButtonBindings.PenTip },
            { "Pen Button", WinInkButtonBindings.PenButton },
            { "Eraser (Toggle)", WinInkButtonBindings.EraserToggle },
            { "Eraser (Hold)", WinInkButtonBindings.EraserHold }
        };

        public static string[] ValidButtons { get; } = Bindings.Keys.ToArray();

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string? Button
        {
            get => _button;
            set
            {
                _button = value;
                
                Bindings.TryGetValue(value!, out _finalButton);

                if (_finalButton == WinInkButtonBindings.None)
                {
                    _finalButton = WinInkButtonBindings.PenTip;
                    Log.WriteNotify("Enhanced Windows Ink Bindings", $"Invalid value '{value}' for Button property. Falling back to 'Pen Tip' instead.", LogLevel.Error);
                }
            }
        }

        [TabletReference]
        public TabletReference Reference { set => Initialize(value); }

        private void Initialize(TabletReference tabletReference)
        {
            try
            {
                _sharedStore = SharedStore.GetStore(tabletReference, STORE_KEY);
                _instance = _sharedStore.Get<VMultiInstance>(INSTANCE);
            }
            catch
            {
                Log.WriteNotify("Enhanced Windows Ink Bindings",
                                "Enhanced Windows Ink Bindings are being used without an active Enhanced Windows Ink Output Mode.",
                                LogLevel.Error);
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            var eraserState = _sharedStore.Get<bool>(ERASER_STATE);
            switch (_finalButton)
            {
                case WinInkButtonBindings.PenTip:
                    _sharedStore.Set(TIP_PRESSED, true);
                    _instance.EnableButtonBit((int)(eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
                    break;

                case WinInkButtonBindings.PenButton:
                    _instance.EnableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;

                case WinInkButtonBindings.EraserToggle:
                    _sharedStore.Set(MANUAL_ERASER, !eraserState);
                    EraserStateTransition(_sharedStore, _instance, !eraserState);
                    break;

                case WinInkButtonBindings.EraserHold:
                    _sharedStore.Set(MANUAL_ERASER, true);
                    EraserStateTransition(_sharedStore, _instance, true);
                    break;
            }
            _instance.Write();
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_instance == null)
                return;

            switch (_finalButton)
            {
                case WinInkButtonBindings.PenTip:
                    _sharedStore.Set(TIP_PRESSED, false);
                    _instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
                    break;

                case WinInkButtonBindings.PenButton:
                    _instance.DisableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;

                case WinInkButtonBindings.EraserHold:
                    _sharedStore.Set(MANUAL_ERASER, false);
                    EraserStateTransition(_sharedStore, _instance, false);
                    break;
            }
            _instance.Write();
        }

        internal static void EraserStateTransition(SharedStore store, VMultiInstance instance, bool isEraser)
        {
            var eraserState = store.Get<bool>(ERASER_STATE);
            if (eraserState != isEraser)
            {
                store.Set(ERASER_STATE, isEraser);
                eraserState = isEraser;
                var report = (DigitizerInputReport*)instance.Header;
                var buttons = report->Header.Buttons;
                var pressure = report->Pressure;

                // Send In-Range but no tips
                instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
                report->Pressure = 0;
                instance.Write();

                // Send Out-Of-Range
                report->Header.Buttons = 0;
                instance.Write();

                // Send In-Range but no tips
                instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
                if (eraserState)
                    instance.EnableButtonBit((int)WindowsInkButtonFlags.Invert);

                instance.Write();

                // Set Proper Report
                if (VMultiInstance.HasBit(buttons, (int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser)))
                    instance.EnableButtonBit((int)(eraserState ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
                report->Pressure = pressure;
            }
        }
    }
}