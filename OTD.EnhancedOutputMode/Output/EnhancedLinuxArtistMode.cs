using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OTD.EnhancedOutputMode.Output
{
    [PluginName("Enhanced Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class EnhancedLinuxArtistMode : EnhancedAbsoluteOutputMode
    {
#pragma warning disable CS8618

        [Resolved]
        public IPressureHandler VirtualTablet { get; set; }

#pragma warning restore CS8618

        public override IAbsolutePointer Pointer
        {
            set => throw new NotSupportedException();
            get => (IAbsolutePointer)VirtualTablet;
        }
    }
}