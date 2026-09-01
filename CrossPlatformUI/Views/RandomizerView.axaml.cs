using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class RandomizerView : ReactiveUserControl<RandomizerViewModel>
{
    public RandomizerView()
    {
        AvaloniaXamlLoader.Load(this);
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
}
