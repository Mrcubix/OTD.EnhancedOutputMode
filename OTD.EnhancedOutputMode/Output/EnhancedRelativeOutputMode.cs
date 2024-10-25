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
using OTD.EnhancedOutputMode.Lib.Tablet;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        private readonly ITabletReport _convertedReport = new TouchConvertedReport();
        private readonly HPETDeltaStopwatch _touchStopwatch = new(true);
        private readonly HPETDeltaStopwatch _penStopwatch = new(true);
        private TouchSettings _touchSettings = TouchSettings.Default;
        private bool _initialized = false;
        private bool skipReport = false;
        private int _lastTouchID = -1;
        private Vector2? _lastTransformedTouchPos;
        private Vector2? _lastTransformedAbsolutePos;
        private Vector2 _lastPos;

        public Matrix3x2 TouchTransformationMatrix { get; protected set; }

#pragma warning disable CS8618

        [Resolved]
        public override IRelativePointer Pointer { set; get; }

#pragma warning restore CS8618

        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();

        public TouchSettings TouchSettings { get; private set; } = TouchSettings.Default;

        #region Initialization

        public void Initialize()
        {
            if (Elements == null)
                return;

            _touchSettings = Elements.OfType<TouchSettings>().FirstOrDefault() ?? TouchSettings.Default;

            // Gather custom filters
            // TODO: someone replace this system with the IPositionedPipelineElement bullshit somehow
            AuxFilters = Elements.OfType<IAuxFilter>().ToList();

            // Initialize filters that require initialization
            foreach (var filter in Elements.OfType<IInitialize>())
                filter.Initialize();

            TouchTransformationMatrix = TransformationMatrix;

            if (TouchSettings.MatchPenSensibilityInRelativeMode)
                UpdateTouchTransformMatrix();

            _initialized = true;
        }

        private void UpdateTouchTransformMatrix()
        {
            // Pen & Touch digitizer suffer from a difference in resolution, 
            // resulting in different speeds for the same sensitivity.
            var XMultiplier = Tablet.Properties.Specifications.Digitizer.MaxX / TouchSettings.MaxX;
            var YMultiplier = Tablet.Properties.Specifications.Digitizer.MaxY / TouchSettings.MaxY;

            // This should achieve about the same speed as the pen
            TouchTransformationMatrix = TransformationMatrix * Matrix3x2.CreateScale(XMultiplier, YMultiplier);
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
                if (_touchSettings == null || !_touchSettings.IsTouchToggled) return;

                // Check if the pen was in range recently and skip report if it was
                if (_touchSettings.DisableWhenPenInRange && _penStopwatch.Elapsed < _touchSettings.PenResetTimeSpan)
                    return;

                (_convertedReport as TouchConvertedReport)!.HandleReport(touchReport, _lastPos);

                // The touch point that was moving the cursor changed, skip this report as it would cause a large delta
                if (_lastTouchID != TouchConvertedReport.CurrentFirstTouchID && TouchConvertedReport.CurrentFirstTouchID != -1)
                {
                    _lastPos = _convertedReport.Position;
                    _lastTransformedTouchPos = null;
                    skipReport = true;
                }

                _lastTouchID = TouchConvertedReport.CurrentFirstTouchID;

                // Skip the report if the pressure is 0 or if the touch point changed
                if (_convertedReport.Pressure != 0 && skipReport == false)
                    base.Read(_convertedReport); // We send another report instead of overwriting the touch report since plugins might rely on it
                else
                    skipReport = false;
            }
            else if (deviceReport is IAbsolutePositionReport) // Restart the pen stopwatch when a pen report is received
                if (_touchSettings.DisableWhenPenInRange)
                    _penStopwatch.Restart();

            base.Read(deviceReport);
        }

        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            if (report is not TouchConvertedReport)
                return base.Transform(report);

            Vector2 pos = report.Position;
            Vector2? delta;

            pos = Vector2.Transform(pos, TouchTransformationMatrix);
            delta = pos - _lastTransformedTouchPos;
            _lastTransformedTouchPos = pos;

            report.Position = delta.GetValueOrDefault();

            return report;
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