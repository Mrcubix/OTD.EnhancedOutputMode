# Enhanced Output Modes for OpenTabletDriver

## Features

- Allow plugins to directly access tablet and aux reports.
- Add support for touch for supported tablets
- Has support for the base OpenTabletDriver output modes as well as VoiD's (Windows Ink & Vmulti)

## Installation

1. Download the latest release from the [releases page](https://github.com/Mrcubix/OTD.EnhancedOutputMode/releases/latest)
2. Open the plugin manager in OpenTabletDriver
3. Drag the downloaded .zip file into the plugin manager

## Supported Versions

- OpenTabletDriver v0.5.3.3

## Touch Supported Tablets

- Wacom PTK-x40,
- Wacom PTH-x51,
- Probably some other wacom tablets

## How to make a plugin

Clone the repo into a git submodule with:

```bash
git submodule add .modules/OTD.EnhancedOutputMode
```

Add a reference to the plugin in your project file:

```xml
<ItemGroup>
    <ProjectReference Include=".modules/OTD.EnhancedOutputMode/OTD.EnhancedOutputMode.csproj" />
</ItemGroup>
```

Add a reference to the library in your plugin class:

```csharp
using OTD.EnhancedOutputMode.Lib;
```

Make use of any of the 2 interfaces: `IGateFilter` or `IAuxFilter`.

Example for `IGateFilter`:

```csharp
public class MyPlugin : IFilter, IGateFilter
{
    public Vector2 Filter(Vector2 report)
    {
        return report;
    }

    public bool Pass(IDeviceReport report, ref ITabletReport tabletreport)
    {
        if (tabletreport.Pressure > 0)
        {
            // Affect the input in some way
            return true;
        }
        else
        {
            return false;
        }
    }

    public FilterStage FilterStage => FilterStage.PreTranspose;
}
```

Example for `IAuxFilter`:

```csharp
public class MyPlugin : IFilter, IAuxFilter
{
    public Vector2 Filter(Vector2 report)
    {
        return report;
    }

    public IAuxReport AuxFilter(IAuxReport report)
    {
        return report;
    }

    public FilterStage FilterStage => FilterStage.PreTranspose;
}
```

Note that you cannot prevent other binds from being triggered, you can mostly only make use of the extra, unused data in the reports, such as, a report from another plugin. (e.g. IWheelReport from the WheelAddon plugin of mine)

When done, build the plugin and drag the dll in the same folder as OTD.EnhancedOutputMode.dll.
The reason is that the plugin need to be loaded in the same context as the library in order for the main plugin to make use of it
without extra logic needed.

