# Enhanced Output Modes for OpenTabletDriver

## Features

- Allow plugins to directly access tablet and aux reports.
- Add support for touch for supported tablets
- Has support for the base OpenTabletDriver output modes as well as VoiD's (Windows Ink & Vmulti)

## Supported Versions

- OpenTabletDriver v0.5.3.3
- OpenTabletDriver v0.6.4.0

## Installation

1. Download the latest release from the [releases page](https://github.com/Mrcubix/OTD.EnhancedOutputMode/releases/latest)
2. Open the plugin manager in OpenTabletDriver
3. Drag the downloaded .zip file into the plugin manager

## How to Enable Touch

Touch is disabled by default, to prevent any conflict of inputs between the pen and your hand.
To enable touch, you can follow these steps:

- Go to the `Tools` tab in OpenTabletDriver (`Filters` in 0.5.3.3),
- Click on Touch Settings, tick `Enable Touch Settings` & `Toggle Touch`,

![Touch Settings](/images/Touch-Settings.png)

Just doing this might not be enough if the touch resolution of your tablet is different from the Wacom CTH tablets,
In which case you will need to set these values manually using the following steps:

- You might want to open the debugger (Tablet -> Tablet Debugger in the top menu bar),
- Move your finger slowly to the bottom right of the tablet until the touch values stop changing, 
- Take note of the X and Y values,
- Write these values in `MaxX` & `MaxY` in the `Touch Settings` plugin.
- Save & Apply and it should work as expected.

## What about Touch Gestures

For Native Gestures (Only recommended on Linux), see the [Native Gestures Plugin](https://github.com/Mrcubix/Native-Gestures/releases/tag/0.1.0)
For Touch Gestures, see the [Touch Gestures Plugin](https://github.com/Mrcubix/Touch-Gestures) (Available via the Plugin Manager)

## Touch Supported Tablets (Tested)

- Wacom PTH-x51,
- Wacom CTH-xxx,

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

