/*
 * MIT License
 *
 * Copyright (c) 2026 initsu
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this source file (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace CrossPlatformUI.Views;

/// <summary>
/// Lays out visible children from top to bottom in columns, preserving their order.
/// Available width limits the number of columns. Available height, including the
/// viewport of an enclosing scroll viewer, determines when sections may need to split.
/// </summary>
/// <remarks>
/// <para>
/// Each direct child moves as a unit. Put related controls in a <see cref="StackPanel"/>
/// when they must stay in the same column.
/// </para>
/// <para>
/// A direct child starts a logical section if it is or contains a visible
/// <see cref="TextBlock"/> with <c>Classes="SectionHeader"</c>. That entire child
/// and every following visible direct child belong to the section until the next
/// section starts. For example, a headed stack followed by two loose checkboxes
/// puts all three children in one section. If the heading is itself a direct child,
/// it stays with the next visible child even when the rest of the section splits.
/// </para>
/// <para>
/// Children before the first heading form an unlabeled section. With no headings,
/// the panel balances individual children.
/// </para>
/// <para>
/// The panel fits the content within the viewport when possible and minimizes the
/// scroll height otherwise. Among layouts within that height, it first minimizes
/// section splits, then balances column heights by minimizing their sum of squares.
/// Ties favor keeping more children in the earlier column.
/// </para>
/// </remarks>
public class FlexColumnsPanel : Panel
{
    private const string SectionHeaderClass = "SectionHeader";

    public static readonly StyledProperty<double> MaxColumnWidthProperty =
        AvaloniaProperty.Register<FlexColumnsPanel, double>(nameof(MaxColumnWidth), 310);

    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<FlexColumnsPanel, double>(nameof(ColumnSpacing), 16);

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<FlexColumnsPanel, double>(nameof(ItemSpacing), 0);

    static FlexColumnsPanel()
    {
        AffectsMeasure<FlexColumnsPanel>(MaxColumnWidthProperty, ColumnSpacingProperty, ItemSpacingProperty);
    }

    public double MaxColumnWidth { get => GetValue(MaxColumnWidthProperty); set => SetValue(MaxColumnWidthProperty, value); }
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    private readonly List<Control> _items = new();
    private readonly List<Rect> _rects = new();
    private ScrollViewer? _scrollViewer;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (_scrollViewer is not null)
        {
            _scrollViewer.PropertyChanged += OnScrollViewerPropertyChanged;
            InvalidateMeasure();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_scrollViewer is not null)
        {
            _scrollViewer.PropertyChanged -= OnScrollViewerPropertyChanged;
        }
        _scrollViewer = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ScrollViewer.ViewportProperty &&
            ((Size)e.OldValue!).Height != ((Size)e.NewValue!).Height)
        {
            InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size available)
    {
        double columnSpacing = ColumnSpacing, itemSpacing = ItemSpacing, maxWidth = MaxColumnWidth;

        // How many columns could fit by width?
        int widthCols;
        double colW;
        if (double.IsInfinity(available.Width))
        {
            widthCols = 1;
            colW = maxWidth;
        }
        else
        {
            widthCols = Math.Max((int)Math.Floor((available.Width + columnSpacing) / (maxWidth + columnSpacing)), 1);
            colW = Math.Min(maxWidth, available.Width);
        }

        // Each visible child is indivisible. A StackPanel keeps related controls together.
        _items.Clear();
        _rects.Clear();
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            child.Measure(new Size(colW, double.PositiveInfinity));
            _items.Add(child);
        }

        int maxColumns = Math.Min(widthCols, _items.Count);
        if (maxColumns == 0)
        {
            return default;
        }

        double heightLimit = available.Height;
        double viewportHeight = _scrollViewer?.Viewport.Height ?? 0;
        if (viewportHeight > 0)
        {
            heightLimit = Math.Min(heightLimit, viewportHeight - Margin.Top - Margin.Bottom);
        }

        var columnEnds = FindColumnEnds(maxColumns, itemSpacing, heightLimit);
        int columnCount = columnEnds.Count;
        double tallest = 0;
        int start = 0;
        for (int column = 0; column < columnCount; column++)
        {
            double x = column * (colW + columnSpacing);
            double y = 0;
            int end = columnEnds[column];
            for (int i = start; i < end; i++)
            {
                if (i > start)
                {
                    y += itemSpacing;
                }
                double height = _items[i].DesiredSize.Height;
                _rects.Add(new Rect(x, y, colW, height));
                y += height;
            }
            tallest = Math.Max(tallest, y);
            start = end;
        }

        return new Size(columnCount * colW + (columnCount - 1) * columnSpacing, tallest);
    }

    private List<int> FindColumnEnds(int maxColumns, double spacing, double heightLimit)
    {
        int itemCount = _items.Count;
        if (maxColumns == 1)
        {
            return [itemCount];
        }
        var offsets = new double[itemCount + 1];
        var startsSection = new bool[itemCount + 1];
        var canBreakBefore = new bool[itemCount + 1];
        bool hasSections = false;
        for (int i = 0; i < itemCount; i++)
        {
            offsets[i + 1] = offsets[i] + _items[i].DesiredSize.Height + spacing;
            startsSection[i] = ContainsSectionHeader(_items[i]);
            hasSections |= startsSection[i];
            // Keep a standalone header with at least the first group below it.
            canBreakBefore[i] = i == 0 || !IsSectionHeader(_items[i - 1]);
        }
        startsSection[itemCount] = true;
        canBreakBefore[itemCount] = true;

        // If scrolling is unavoidable, use the smallest possible scroll extent as the height limit.
        if (double.IsFinite(heightLimit))
        {
            heightLimit = Math.Max(heightLimit, MinimumLayoutHeight(maxColumns, offsets, canBreakBefore, spacing));
        }

        // For each suffix, use up to the available columns. Prefer fewer section splits,
        // then minimize the sum of squared column heights to balance the columns.
        var scores = new (int Splits, double HeightScore)[maxColumns + 1, itemCount + 1];
        var nextEnds = new int[maxColumns + 1, itemCount];
        for (int start = 0; start < itemCount; start++)
        {
            scores[0, start] = (int.MaxValue, double.PositiveInfinity);
        }

        for (int columns = 1; columns <= maxColumns; columns++)
        {
            for (int start = 0; start < itemCount; start++)
            {
                var best = (Splits: int.MaxValue, HeightScore: double.PositiveInfinity);
                // Try later breaks first so ties keep more children in the earlier column.
                for (int end = itemCount; end > start; end--)
                {
                    var suffix = scores[columns - 1, end];
                    if (!canBreakBefore[end] || suffix.Splits == int.MaxValue)
                    {
                        continue;
                    }
                    double height = offsets[end] - offsets[start] - spacing;
                    if (height > heightLimit)
                    {
                        continue;
                    }
                    int splits = suffix.Splits + (hasSections && !startsSection[end] ? 1 : 0);
                    double heightScore = height * height + suffix.HeightScore;
                    if (splits < best.Splits || (splits == best.Splits && heightScore < best.HeightScore - 0.001))
                    {
                        best = (splits, heightScore);
                        nextEnds[columns, start] = end;
                    }
                }
                scores[columns, start] = best;
            }
        }

        var columnEnds = new List<int>(maxColumns);
        for (int start = 0, columns = maxColumns; start < itemCount; columns--)
        {
            start = nextEnds[columns, start];
            columnEnds.Add(start);
        }
        return columnEnds;
    }

    private static double MinimumLayoutHeight(int maxColumns, double[] offsets, bool[] canBreakBefore, double spacing)
    {
        int itemCount = offsets.Length - 1;
        var heights = new double[maxColumns + 1, itemCount + 1];
        for (int start = 0; start < itemCount; start++)
        {
            heights[0, start] = double.PositiveInfinity;
        }

        for (int columns = 1; columns <= maxColumns; columns++)
        {
            for (int start = 0; start < itemCount; start++)
            {
                double shortest = double.PositiveInfinity;
                for (int end = start + 1; end <= itemCount; end++)
                {
                    if (!canBreakBefore[end])
                    {
                        continue;
                    }
                    double height = offsets[end] - offsets[start] - spacing;
                    shortest = Math.Min(shortest, Math.Max(height, heights[columns - 1, end]));
                }
                heights[columns, start] = shortest;
            }
        }
        return heights[maxColumns, 0];
    }

    private static bool ContainsSectionHeader(ILogical item)
        => item is not Visual { IsVisible: false }
        && (IsSectionHeader(item) || item.LogicalChildren.Any(ContainsSectionHeader));

    private static bool IsSectionHeader(ILogical item)
        => item is TextBlock text && text.Classes.Contains(SectionHeaderClass);

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].Arrange(_rects[i]);
        }
        return finalSize;
    }
}
