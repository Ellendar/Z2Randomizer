using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.Controls;

/// <summary>
/// Builds the preset-diff control for a numeric flag: the shared box, plus the
/// flag's optional caption above it. Both Min/Max pair ends show as their own
/// single box, so the caption (keyed off the field name) is what makes either end
/// readable away from its tab's heading.
/// </summary>
internal static class NumericDiffControl
{
    public static Control Build(ConfigFlag flag)
    {
        var box = new Z2NumericUpDown { Flag = flag };

        var caption = FlagRegistry.Get(flag)?.Caption?.Invoke();
        if (string.IsNullOrEmpty(caption))
            return box;

        var panel = new StackPanel { Orientation = Orientation.Vertical };
        panel.Children.Add(new TextBlock
        {
            Text = caption,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(2, 0, 0, 4),
        });
        panel.Children.Add(box);
        return panel;
    }
}
