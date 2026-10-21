using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using CrossPlatformUI.ViewModels.Tabs;

namespace CrossPlatformUI.Behaviors;

/// <summary>
/// Collapses a diff row's height to zero as it leaves the diff so the rows
/// below close the gap smoothly. If the row is later diffed again it revives.
/// An auto-sized row cannot animate height from XAML because keyframes need a
/// concrete start value.
/// </summary>
public class CollapseOnRemovingBehavior : Behavior<Control>
{
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<CollapseOnRemovingBehavior, TimeSpan>(
            nameof(Duration), TimeSpan.FromMilliseconds(200));

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    private PresetDiffRow? row;
    private CancellationTokenSource? collapseCts;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not { } control)
        {
            return;
        }
        control.DataContextChanged += OnDataContextChanged;
        AttachTo(control.DataContext);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is { } control)
        {
            control.DataContextChanged -= OnDataContextChanged;
        }
        DetachFromRow();
        base.OnDetaching();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject is { } control)
        {
            AttachTo(control.DataContext);
        }
    }

    private void AttachTo(object? dataContext)
    {
        DetachFromRow();
        if (dataContext is PresetDiffRow r)
        {
            row = r;
            r.PropertyChanged += OnRowPropertyChanged;
            Apply(r.IsRemoving);
        }
    }

    private void DetachFromRow()
    {
        if (row is null)
        {
            return;
        }
        row.PropertyChanged -= OnRowPropertyChanged;
        collapseCts?.Cancel();
        collapseCts?.Dispose();
        collapseCts = null;
        row = null;
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PresetDiffRow.IsRemoving) && row is not null)
        {
            Apply(row.IsRemoving);
        }
    }

    private void Apply(bool removing)
    {
        if (AssociatedObject is not { } control)
        {
            return;
        }

        collapseCts?.Cancel();
        collapseCts?.Dispose();
        collapseCts = null;

        if (!removing)
        {
            // Revived: hand height back to auto layout.
            control.Height = double.NaN;
            control.RenderTransform = null;
            return;
        }

        var cts = collapseCts = new CancellationTokenSource();
        _ = CollapseAsync(control, cts.Token);
    }

    private async Task CollapseAsync(Control control, CancellationToken token)
    {
        double height = control.Bounds.Height;
        if (double.IsNaN(height) || height <= 0)
        {
            return;
        }

        control.Height = height;

        // Drive both properties from one animation with the same easing so the
        // content stays in sync with the collapse: it sinks 1px for every 2px
        // the row shrinks (a growing "top margin").
        var translate = new TranslateTransform();
        control.RenderTransform = translate;

        var animation = new Animation
        {
            Duration = Duration,
            FillMode = FillMode.Forward,
            Easing = new QuadraticEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(Control.HeightProperty, height),
                        new Setter(TranslateTransform.YProperty, 0d),
                    },
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(Control.HeightProperty, 0d),
                        new Setter(TranslateTransform.YProperty, height * 2 / 3),
                    },
                },
            },
        };

        try
        {
            await animation.RunAsync(control, token);
        }
        catch (OperationCanceledException)
        {
            // Revived mid-collapse; Apply resets the height and transform.
        }
    }
}
