using System.ComponentModel;
using ModelContextProtocol.Server;
using pdc.DisplayApi;

namespace pdc.Mcp;

/// <summary>
/// MCP tools that let an agent inspect displays and change their brightness.
/// These wrap <see cref="DisplayApiService"/>; none of them write to stdout
/// (which is reserved for the MCP protocol), so they build/return strings instead.
/// </summary>
[McpServerToolType]
public static class BrightnessTools
{
    [McpServerTool(Name = "list_displays")]
    [Description("List connected displays with their index and name. " +
        "Philips monitors (name contains 'PHL') support precise DDC/CI brightness; " +
        "use the returned index with set_brightness.")]
    public static string ListDisplays()
    {
        DisplayApiService.InitDisplayInfo();

        var names = DisplayApiService.displayNames;
        if (names.Count == 0)
        {
            return "No displays were detected.";
        }

        var lines = names.Select((name, index) => $"index {index}: {name}");
        return string.Join(Environment.NewLine, lines);
    }

    [McpServerTool(Name = "set_brightness")]
    [Description("Set the brightness of a single display by its index (from list_displays). " +
        "Brightness is a percentage from 0 to 100; out-of-range values are clamped.")]
    public static string SetBrightness(
        [Description("Display index as reported by list_displays")] int displayIndex,
        [Description("Target brightness percentage, 0-100")] int brightness)
    {
        var clamped = Math.Clamp(brightness, 0, 100);
        DisplayApiService.SetDisplayBrightnessIndividually(new[] { $"{displayIndex}:{clamped}" });
        return $"Set display {displayIndex} brightness to {clamped}%.";
    }

    [McpServerTool(Name = "set_all_brightness")]
    [Description("Set the same brightness on all displays at once. Philips monitors are set " +
        "via DDC/CI; other displays via the OS backlight control. " +
        "Brightness is a percentage from 0 to 100; out-of-range values are clamped.")]
    public static string SetAllBrightness(
        [Description("Target brightness percentage, 0-100")] int brightness)
    {
        var clamped = Math.Clamp(brightness, 0, 100);
        DisplayApiService.SetAllDisplayBrightness(clamped);
        return $"Set all displays to {clamped}%.";
    }
}
