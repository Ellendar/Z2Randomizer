using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace CrossPlatformUI.Behaviors;

public class ScrollSelectBehavior : Behavior<Control>
{
    // Accumulated delta for smooth / throttled scrolling
    private double _scrollAccumulator = 0;
    
    /// <summary>
    /// Whether to use natural scrolling (like macOS). Defaults to true on macOS.
    /// </summary>
    public static readonly StyledProperty<bool> UseNaturalScrollProperty =
        AvaloniaProperty.Register<ScrollSelectBehavior, bool>(nameof(UseNaturalScroll));
    public bool UseNaturalScroll
    {
        get => GetValue(UseNaturalScrollProperty);
        set => SetValue(UseNaturalScrollProperty, value);
    }

    /// <summary>
    /// How much scroll must accumulate before we move one item
    /// </summary>
    public static readonly StyledProperty<double> ScrollThresholdProperty =
        AvaloniaProperty.Register<ScrollSelectBehavior, double>(nameof(ScrollThreshold), 1.0);

    public double ScrollThreshold
    {
        get => GetValue(ScrollThresholdProperty);
        set => SetValue(ScrollThresholdProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        // Default to natural scroll on macOS
        if (OperatingSystem.IsMacOS() && !IsSet(UseNaturalScrollProperty))
            UseNaturalScroll = true;

        AssociatedObject?.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        base.OnDetaching();
    }

    // Scroll through the list of items or increment/decrement a number when rolling the mousewheel over an element.
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (AssociatedObject == null)
            return;

        var delta = UseNaturalScroll ? -e.Delta.Y : e.Delta.Y;
        _scrollAccumulator += delta;

        // Only act when threshold exceeded
        if (Math.Abs(_scrollAccumulator) < ScrollThreshold)
            return;

        var steps = (int)Math.Floor(Math.Abs(_scrollAccumulator) / ScrollThreshold);
        var direction = Math.Sign(_scrollAccumulator);
        _scrollAccumulator %= ScrollThreshold;

        var handled = false;
        switch (AssociatedObject)
        {
            case ComboBox combo:
                handled = HandleComboBoxScroll(combo, direction * steps);
                break;

            case NumericUpDown numeric:
                handled = HandleNumericUpDownScroll(numeric, direction * steps);
                break;
        }

        e.Handled = handled;
    }

    private bool HandleComboBoxScroll(ComboBox combo, int step)
    {
        if (combo.Items is not IEnumerable itemsEnum)
            return false;

        if (combo.IsDropDownOpen)
            return false;
        
        var items = itemsEnum.Cast<object>().ToList();
        if (items.Count == 0)
            return false;

        var newIndex = combo.SelectedIndex - step;
        newIndex = Math.Clamp(newIndex, 0, items.Count - 1);
        combo.SelectedIndex = newIndex;
        return true;
    }

    private bool HandleNumericUpDownScroll(NumericUpDown numeric, int step)
    {
        var newValue = numeric.Value + (step * numeric.Increment);
        newValue = Math.Clamp(newValue ?? 0, numeric.Minimum, numeric.Maximum);
        numeric.Value = newValue;
        return true;
    }
}
