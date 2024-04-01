using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Timing;
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Lib.Tools;
using OTD.EnhancedOutputMode.Tablet;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        //private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private Vector2 _lastPos;
        private bool _initialized = false;
        private bool skipReport = false;
        private int _lastTouchID = -1;

        protected Matrix3x2 _touchTransformationMatrix;

        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();

#pragma warning disable CS8618

        [Resolved]
        public override IRelativePointer Pointer { set; get; }

#pragma warning restore CS8618

        #region Initialization

        public void Initialize()
        {
            if (Elements == null)
                return;

            // Gather custom filters
            // TODO: someone replace this system with the IPositionedPipelineElement bullshit somehow
            AuxFilters = Elements.OfType<IAuxFilter>().ToList();

            // Initialize filters that require initialization
            foreach (var filter in Elements.OfType<IInitialize>())
                filter.Initialize();

            _initialized = true;
        }

        #endregion

        #region Report Handling

        public override void Consume(IDeviceReport report)
        {
            if(!_initialized)
                Initialize();

            base.Consume(report);
        }

        public override void Read(IDeviceReport deviceReport)
        {
            if (deviceReport is ITouchReport touchReport)
            {
                if (!TouchToggle.istouchToggled) return;

                (_convertedReport as TouchConvertedReport)!.HandleReport(touchReport, _lastPos);

                // The touch point that was moving the cursor changed, skip this report as it would cause a large delta
                if (_lastTouchID != TouchConvertedReport.CurrentFirstTouchID && TouchConvertedReport.CurrentFirstTouchID != -1)
                {
                    _lastPos = _convertedReport.Position;
                    skipReport = true;
                }

                _lastTouchID = TouchConvertedReport.CurrentFirstTouchID;

                // Skip the report if the pressure is 0 or if the touch point changed
                if (_convertedReport.Pressure != 0 && skipReport == false)
                    base.Read(_convertedReport); // We send another report instead of overwriting the touch report since plugins might rely on it
                else
                    skipReport = false;

                _lastPos = _convertedReport.Position;
            }

            base.Read(deviceReport);
        }

        protected override void OnOutput(IDeviceReport report)
        {
            if (report is IAuxReport auxReport)
            {
                foreach (var auxFilter in AuxFilters)
                    auxReport = auxFilter.AuxFilter(auxReport);
            }

            base.OnOutput(report);
        }

        #endregion
    }
}