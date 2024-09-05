using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
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
    [PluginName("Enhanced Absolute Mode")]
    public class EnhancedAbsoluteOutputMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        private readonly ITabletReport _convertedReport = new TouchConvertedReport();
        private readonly HPETDeltaStopwatch _penStopwatch = new(true);
        private TouchSettings _touchSettings = TouchSettings.Default;
        private DigitizerSpecifications _touchDigitizer = new();
        private bool _initialized = false;
        private Vector2 min, max;
        private Vector2 _lastPos;

        protected Matrix3x2 _touchTransformationMatrix;

#pragma warning disable CS8618

        [Resolved]
        public override IAbsolutePointer Pointer { set; get; }

        [Resolved]
        public IDriver _driver { set; get; }

#pragma warning restore CS8618

        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();

        public TouchSettings TouchSettings { get; private set; } = TouchSettings.Default;

        #region Initialization

        private void InitializeTouch(TabletReference tabletReference)
        {
            var digitizer = tabletReference.Properties.Specifications.Digitizer;
            var touch = tabletReference.Properties.Specifications.Touch;

            Vector2 maxes;

            // TODO, currently, TouchSettings isn't getting its values set early enough, we might want to load them from elsewhere
            if (touch != null)
                maxes = new Vector2(touch.MaxX, touch.MaxY);
            else if (_touchSettings.Maxes != Vector2.Zero) 
                maxes = _touchSettings.Maxes;
            else
                maxes = new Vector2(4095, 4095);

            // Set touch digitizer specifications
            _touchDigitizer = new DigitizerSpecifications
            {
                Width = digitizer.Width,
                Height = digitizer.Height,
                MaxX = (int)maxes.X,
                MaxY = (int)maxes.Y,
            };
        }

        public void Initialize()
        {
            // Gather custom filters
            // TODO: someone replace this system with the IPositionedPipelineElement bullshit somehow
            if (Elements == null)
                return;

            _touchSettings = Elements.OfType<TouchSettings>().FirstOrDefault() ?? TouchSettings.Default;

            // Initialize touch digitizer
            InitializeTouch(Tablet);

            // Gather custom filters
            // TODO: someone replace this system with the IPositionedPipelineElement bullshit somehow
            AuxFilters = Elements.OfType<IAuxFilter>().ToList();

            // Initialize filters that require initialization
            foreach (var filter in Elements.OfType<IInitialize>())
                filter.Initialize();

            if (_driver is Driver driver)
            {
                var device = driver.InputDevices.Where(dev => dev?.OutputMode == this).FirstOrDefault();

                if (device is not InputDeviceTree inputDevice)
                    return;

                if (device.OutputMode is not AbsoluteOutputMode absoluteMode)
                    return;

                if (absoluteMode.Input == null || absoluteMode.Output == null)
                    return;

                // Calculate transformation matrix for touch
                _touchTransformationMatrix = CreateTouchTransformationMatrix(absoluteMode);

                _initialized = true;
            }
        }

        protected virtual Matrix3x2 CreateTouchTransformationMatrix(AbsoluteOutputMode output)
        {
            var transform = CalculateTransformation(output.Input, output.Output, _touchDigitizer);

            var halfDisplayWidth = Output?.Width / 2 ?? 0;
            var halfDisplayHeight = Output?.Height / 2 ?? 0;

            var minX = Output?.Position.X - halfDisplayWidth ?? 0;
            var maxX = Output?.Position.X + Output?.Width - halfDisplayWidth ?? 0;
            var minY = Output?.Position.Y - halfDisplayHeight ?? 0;
            var maxY = Output?.Position.Y + Output?.Height - halfDisplayHeight ?? 0;

            this.min = new Vector2(minX, minY);
            this.max = new Vector2(maxX, maxY);

            return transform;
        }

        #endregion

        #region Report Handling

        public override void Read(IDeviceReport deviceReport)
        {
            if (!_initialized)
                Initialize();

            if (deviceReport is ITabletReport) // Restart the pen stopwatch when a pen report is received
                if (_touchSettings.DisableWhenPenInRange)
                    _penStopwatch.Restart();

            if (deviceReport is ITouchReport touchReport)
            {
                if (TouchSettings == null || !TouchSettings.IsTouchToggled) return;

                // Check if the pen was in range recently and skip report if it was
                if (_touchSettings.DisableWhenPenInRange && _penStopwatch.Elapsed < _touchSettings.PenResetTimeSpan)
                    return;

                (_convertedReport as TouchConvertedReport)!.HandleReport(touchReport, _lastPos);

                if (_convertedReport.Pressure != 0)
                {
                    _lastPos = _convertedReport.Position;
                    base.Read(_convertedReport); // We send another report instead of overwriting the touch report since plugins might rely on it
                }
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

        /// <summary>
        /// Transposes, transforms, and performs all absolute positioning calculations to a <see cref="IAbsolutePositionReport"/>.
        /// </summary>
        /// <param name="report">The <see cref="IAbsolutePositionReport"/> in which to transform.</param>
        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            Vector2 pos = Vector2.Zero;

            // Apply transformation
            if (report is TouchConvertedReport)
                pos = Vector2.Transform(report.Position, this._touchTransformationMatrix);
            else
                pos = Vector2.Transform(report.Position, this.TransformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, this.min, this.max);
            if (AreaLimiting && clippedPoint != pos)
                return null!;

            if (AreaClipping)
                pos = clippedPoint;

            report.Position = pos;

            return report;
        }

        #endregion
    }
}