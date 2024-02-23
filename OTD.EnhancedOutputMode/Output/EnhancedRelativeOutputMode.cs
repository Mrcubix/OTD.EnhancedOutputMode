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
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.EnhancedOutputMode.Tablet;
using OTD.EnhancedOutputMode.Tool;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Relative Mode")]
    public class EnhancedRelativeOutputMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        private ITabletReport _convertedReport = new TouchConvertedReport();
        private Vector2 _lastPos;
        private bool _initialized = false;

        protected Matrix3x2 _touchTransformationMatrix;

        public IList<IAuxFilter> AuxFilters { get; set; } = Array.Empty<IAuxFilter>();

#pragma warning disable CS8618

        [Resolved]
        public override IRelativePointer Pointer { set; get; }

#pragma warning restore CS8618

        #region Initialization

        public void Initialize()
        {
            // Gather custom filters
            // TODO: someone replace this system with the IPositionedPipelineElement bullshit somehow
            AuxFilters = Elements.OfType<IAuxFilter>().ToList();

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

                if (_convertedReport.Pressure == 0)
                    return;

                _lastPos = _convertedReport.Position;
                deviceReport = _convertedReport;
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