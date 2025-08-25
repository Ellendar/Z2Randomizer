using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace CrossPlatformUI.Views;

public class TabContentPreservationBehavior : Behavior<TabControl>
{
    private readonly Dictionary<TabItem, object> contentCache = new();

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }
        base.OnDetaching();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Cache content from tabs being deselected
        foreach (TabItem item in e.RemovedItems.OfType<TabItem>())
        {
            if (item.Content != null)
            {
                contentCache[item] = item.Content;
            }
        }

        // Restore content for newly selected tab
        foreach (TabItem item in e.AddedItems.OfType<TabItem>())
        {
            if (contentCache.TryGetValue(item, out var cachedContent))
            {
                item.Content = cachedContent;
            }
        }
    }
}