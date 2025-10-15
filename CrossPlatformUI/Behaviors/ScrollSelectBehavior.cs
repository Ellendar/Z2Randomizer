using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace CrossPlatformUI.Behaviors;

public class ScrollSelectBehavior : Behavior<ComboBox>
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

    // Scroll through the list of items when rolling the mousewheel over an element.
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (AssociatedObject?.Items is not IEnumerable itemsEnum)
            return;

        var items = itemsEnum.Cast<object>().ToList();
        if (items.Count == 0)
            return;

        // Adjust for natural scroll direction
        var delta = UseNaturalScroll ? -e.Delta.Y : e.Delta.Y;

        // Accumulate the delta
        _scrollAccumulator += delta;

        // Only move when we exceed threshold
        if (Math.Abs(_scrollAccumulator) >= ScrollThreshold)
        {
            int stepCount = (int)Math.Floor(Math.Abs(_scrollAccumulator) / ScrollThreshold);
            int direction = Math.Sign(_scrollAccumulator);

            var newIndex = AssociatedObject.SelectedIndex - (direction * stepCount);

            // Clamp to valid range
            newIndex = Math.Clamp(newIndex, 0, items.Count - 1);

            AssociatedObject.SelectedIndex = newIndex;

            // Retain leftover delta (for smoothness)
            _scrollAccumulator %= ScrollThreshold;
        }

        e.Handled = true;
    }
}