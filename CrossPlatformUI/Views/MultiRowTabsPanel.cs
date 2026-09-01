using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace CrossPlatformUI.Views;

/// <summary>
/// Custom TabControl that lets you control two rows of tabs.
/// <see cref="FirstRowCount"/> items are placed on the first row and
/// the remaining items on the second row.
/// </summary>
public class MultiRowTabsPanel : Panel
{
    /// <summary>How many tab items occupy the first row. Defaults to 2.</summary>
    public static readonly StyledProperty<int> FirstRowCountProperty =
        AvaloniaProperty.Register<MultiRowTabsPanel, int>(nameof(FirstRowCount), 2);

    public int FirstRowCount
    {
        get => GetValue(FirstRowCountProperty);
        set => SetValue(FirstRowCountProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var constraint = new Size(double.PositiveInfinity, availableSize.Height);
        double firstRowHeight = 0, secondRowHeight = 0;
        double firstRowWidth = 0, secondRowWidth = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].Measure(constraint);
            var desired = Children[i].DesiredSize;
            if (i < FirstRowCount)
            {
                firstRowHeight = Math.Max(firstRowHeight, desired.Height);
                firstRowWidth += desired.Width;
            }
            else
            {
                secondRowHeight = Math.Max(secondRowHeight, desired.Height);
                secondRowWidth += desired.Width;
            }
        }
        var height = (Children.Count > FirstRowCount ? firstRowHeight + secondRowHeight : firstRowHeight);
        return new Size(Math.Max(firstRowWidth, secondRowWidth), height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double firstRowHeight = 0;
        for (int i = 0; i < Math.Min(FirstRowCount, Children.Count); i++)
        {
            firstRowHeight = Math.Max(firstRowHeight, Children[i].DesiredSize.Height);
        }

        double secondRowHeight = 0;
        for (int i = FirstRowCount; i < Children.Count; i++)
        {
            secondRowHeight = Math.Max(secondRowHeight, Children[i].DesiredSize.Height);
        }

        int secondRowCount = Children.Count - FirstRowCount;

        // Rows share the same column widths, so they stay aligned
        double columnWidth = secondRowCount > 0 ? finalSize.Width / secondRowCount : finalSize.Width;

        for (int i = 0; i < Math.Min(FirstRowCount, secondRowCount); i++)
        {
            Children[i].Arrange(new Rect(new Point(i * columnWidth, 0), new Size(columnWidth, firstRowHeight)));
        }

        if (secondRowCount > 0)
        {
            for (int j = 0; j < secondRowCount; j++)
            {
                var child = Children[FirstRowCount + j];
                child.Arrange(new Rect(new Point(j * columnWidth, firstRowHeight), new Size(columnWidth, secondRowHeight)));
            }
        }

        return finalSize;
    }
}
