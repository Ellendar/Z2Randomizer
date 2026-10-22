using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views.Tabs;

namespace CrossPlatformUI.Views;

public partial class RandomizerView : ReactiveUserControl<RandomizerViewModel>
{
    public RandomizerView()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(PointerPressedEvent, OnSearchInputPressed, RoutingStrategies.Tunnel);
        this.WhenActivated((MultipleDisposable disposables) =>
        {
            if (DataContext is RandomizerViewModel vm)
            {
                var flagInputTextBox = this.FindControl<TextBox>("FlagInputTextBox");
                if (flagInputTextBox is not null)
                {
                    EventHandler<RoutedEventArgs> PasteHandler = async (s, e) =>
                    {
                        var clipboard = App.TopLevel?.Clipboard;
                        if (clipboard is null) { return; }
                        var text = await clipboard.TryGetTextAsync();
                        if (string.IsNullOrEmpty(text)) { return; }
                        var (extractedFlags, extractedSeed) = FlagPasteParser.Parse(text);
                        if (string.IsNullOrEmpty(extractedFlags)) { return; }
                        if (extractedFlags != null)
                        {
                            vm.FlagInput = extractedFlags;
                        }
                        if (extractedSeed != null)
                        {
                            vm.Seed = extractedSeed;
                        }
                        e.Handled = true;
                    };
                    flagInputTextBox.PastingFromClipboard += PasteHandler;

                    new ActionDisposable(() => flagInputTextBox.PastingFromClipboard -= PasteHandler)
                        .DisposeWith(disposables);
                }
            }
        });
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        ResolveSearchTabIndex();
    }

    private void OnSearchInputPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Visual hit)
        {
            return;
        }

        var input = hit.GetSelfAndVisualAncestors().OfType<TextBox>().FirstOrDefault();
        if (input?.Name != "SearchInputTextBox" || !e.GetCurrentPoint(input).Properties.IsLeftButtonPressed)
        {
            return;
        }
        if (input.InnerRightContent is Visual clearButton && hit.GetSelfAndVisualAncestors().Contains(clearButton))
        {
            return;
        }

        (input.DataContext as RandomizerViewModel)?.OpenSearch();
    }

    private void OnSearchInputFocused(object? sender, FocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && sender is TextBox { DataContext: RandomizerViewModel vm })
        {
            vm.OpenSearch();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        ResolveSearchTabIndex();
    }

    /// <summary>
    /// Tells the view model the index of the hidden search page, so it
    /// can switch to it when a search starts. It is resolved from the strip
    /// itself rather than hard-coded, so reordering the tabs in XAML cannot
    /// desynchronize the view model.
    /// </summary>
    private void ResolveSearchTabIndex()
    {
        if (DataContext is not RandomizerViewModel vm)
        {
            return;
        }

        var tabs = this.FindControl<TabControl>("Tabs");
        if (tabs is null)
        {
            return;
        }

        var index = tabs.Items.OfType<TabItem>().ToList()
            .FindIndex(item => item.Content is SearchView);
        if (index < 0)
        {
            return;
        }

        vm.SearchTabIndex = index;
    }
}
