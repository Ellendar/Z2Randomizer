using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace CrossPlatformUI.Views;

/// <summary>
/// Custom TabControl that lets you control two rows of tabs.
/// <see cref="FirstRowCount"/> items are placed on the first row and
/// the remaining items on the second row.
/// </summary>
public class MultiRowTabsPanel : Panel
{
    private const int TrailingColumnSpan = 2;
    private const double TrailingGap = 12;
    private const double MinimumTrailingSpace = 160;

    /// <summary>How many tab items occupy the first row. Defaults to 2.</summary>
    public static readonly StyledProperty<int> FirstRowCountProperty =
        AvaloniaProperty.Register<MultiRowTabsPanel, int>(nameof(FirstRowCount), 2);

    public static readonly StyledProperty<Control?> FirstRowContentProperty =
        AvaloniaProperty.Register<MultiRowTabsPanel, Control?>(nameof(FirstRowContent));

    private double firstRowHeight;
    private double secondRowHeight;
    private bool trailingOnOwnRow;
    private readonly List<Control> visibleTabs = new();

    static MultiRowTabsPanel()
    {
        AffectsMeasure<MultiRowTabsPanel>(FirstRowCountProperty, FirstRowContentProperty);
        AffectsParentMeasure<MultiRowTabsPanel>(IsVisibleProperty);
    }

    public int FirstRowCount
    {
        get => GetValue(FirstRowCountProperty);
        set => SetValue(FirstRowCountProperty, value);
    }

    public Control? FirstRowContent
    {
        get => GetValue(FirstRowContentProperty);
        set => SetValue(FirstRowContentProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != FirstRowContentProperty) return;

        // The ItemsPresenter uses Children for its tab indices.
        if (change.OldValue is Control oldContent)
        {
            VisualChildren.Remove(oldContent);
            LogicalChildren.Remove(oldContent);
        }
        if (change.NewValue is Control newContent)
        {
            LogicalChildren.Add(newContent);
            VisualChildren.Add(newContent);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var constraint = new Size(double.PositiveInfinity, availableSize.Height);
        visibleTabs.Clear();
        foreach (var child in Children)
        {
            if (child.IsVisible)
                visibleTabs.Add(child);
        }
        int firstRowCount = Math.Clamp(FirstRowCount, 0, visibleTabs.Count);
        int secondRowCount = visibleTabs.Count - firstRowCount;
        double columnWidth = secondRowCount > 0 && double.IsFinite(availableSize.Width)
            ? availableSize.Width / secondRowCount
            : 0;
        firstRowHeight = secondRowHeight = 0;
        double firstRowWidth = 0, secondRowWidth = 0, extraRowHeight = 0;
        for (int i = 0; i < visibleTabs.Count; i++)
        {
            var tab = visibleTabs[i];
            tab.Measure(constraint);
            var desired = tab.DesiredSize;
            if (i < firstRowCount)
            {
                firstRowHeight = Math.Max(firstRowHeight, desired.Height);
                firstRowWidth += Math.Max(columnWidth, desired.Width);
            }
            else
            {
                secondRowHeight = Math.Max(secondRowHeight, desired.Height);
                secondRowWidth += desired.Width;
            }
        }
        if (FirstRowContent is { IsVisible: true } trailing)
        {
            double remainingWidth = availableSize.Width - firstRowWidth - TrailingGap;
            double trailingWidth = columnWidth * Math.Min(TrailingColumnSpan, secondRowCount);
            trailingOnOwnRow = trailingWidth <= 0 || remainingWidth < MinimumTrailingSpace || trailingWidth > remainingWidth;
            trailing.Measure(new Size(trailingOnOwnRow ? availableSize.Width : trailingWidth, availableSize.Height));
            if (trailingOnOwnRow)
            {
                extraRowHeight = trailing.DesiredSize.Height;
                firstRowWidth = Math.Max(firstRowWidth, trailing.DesiredSize.Width);
            }
            else
            {
                firstRowHeight = Math.Max(firstRowHeight, trailing.DesiredSize.Height);
                firstRowWidth = availableSize.Width;
            }
        }
        return new Size(Math.Max(firstRowWidth, secondRowWidth), firstRowHeight + secondRowHeight + extraRowHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int firstRowCount = Math.Clamp(FirstRowCount, 0, visibleTabs.Count);
        int secondRowCount = visibleTabs.Count - firstRowCount;
        double columnWidth = secondRowCount > 0 ? finalSize.Width / secondRowCount : 0;
        var trailing = FirstRowContent is { IsVisible: true } content ? content : null;
        double tabTop = trailingOnOwnRow ? trailing?.DesiredSize.Height ?? 0 : 0;

        double firstRowX = 0;
        for (int i = 0; i < firstRowCount; i++)
        {
            var child = visibleTabs[i];
            double width = Math.Max(columnWidth, child.DesiredSize.Width);
            child.Arrange(new Rect(firstRowX, tabTop, width, firstRowHeight));
            firstRowX += width;
        }

        for (int j = 0; j < secondRowCount; j++)
        {
            var child = visibleTabs[firstRowCount + j];
            child.Arrange(new Rect(j * columnWidth, tabTop + firstRowHeight, columnWidth, secondRowHeight));
        }

        if (trailing is not null)
        {
            double width = trailingOnOwnRow
                ? finalSize.Width
                : columnWidth * Math.Min(TrailingColumnSpan, secondRowCount);
            double height = trailing.DesiredSize.Height;
            double y = trailingOnOwnRow ? 0 : (firstRowHeight - height) / 2;
            trailing.Arrange(new Rect(finalSize.Width - width, y, width, height));
        }

        return finalSize;
    }
}
