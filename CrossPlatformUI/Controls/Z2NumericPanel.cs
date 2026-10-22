using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.Controls;

/// <summary>
/// Builds the control for a StackPanel holding holding a Z2NumericUpDown
/// plus it's caption if available.
/// </summary>
internal static class Z2NumericPanel
{
    public static Control Build(ConfigFlag flag)
    {
        var box = new Z2NumericUpDown { Flag = flag };

        // Every numeric row is a StackPanel, so it picks up the same margins as
        // the rest of the app (App.axaml styles StackPanel and its children).
        // A bare box would sit flush against the row edge and be wider than the
        // other rows.
        var panel = new StackPanel { Orientation = Orientation.Vertical };
        panel.Children.Add(box);

        // A Min/Max pair end carries the group's caption above its box; a plain
        // numeric has no caption, and its box hint already names the setting.
        var caption = FlagRegistry.Get(flag)?.Caption?.Invoke();
        if (!string.IsNullOrEmpty(caption))
        {
            panel.Children.Insert(0, new TextBlock
            {
                Text = caption,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2, 0, 0, 4),
            });
        }

        return panel;
    }
}
