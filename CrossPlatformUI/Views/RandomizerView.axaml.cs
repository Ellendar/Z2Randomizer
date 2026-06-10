using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class RandomizerView : ReactiveUserControl<RandomizerViewModel>
{
    public RandomizerView()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(disposables =>
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
                    Disposable.Create(() => flagInputTextBox.PastingFromClipboard -= PasteHandler)
                        .DisposeWith(disposables);
                }

                vm.SaveAsPreset.Subscribe(_ =>
                {
                    Button presetsButton = this.FindControl<Button>("PresetsButton") ?? throw new System.Exception("Missing Required Validation Element");
                    // close menu after saving preset
                    presetsButton?.Flyout?.Hide();
                })
                .DisposeWith(disposables);
            }
        });
    }
}
