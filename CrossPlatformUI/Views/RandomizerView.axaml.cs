using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class RandomizerView : ReactiveUserControl<RandomizerViewModel>
{
    public RandomizerView()
    {
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables => {
            if (DataContext is RandomizerViewModel vm)
            {
                var flagInputTextBox = this.FindControl<TextBox>("FlagInputTextBox");
                if (flagInputTextBox is not null)
                {
                    EventHandler<RoutedEventArgs> pasteHandler = (_, _) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            vm.FlagInput = flagInputTextBox.Text ?? "";
                            if (flagInputTextBox.Text != vm.FlagInput)
                            {
                                flagInputTextBox.Text = vm.FlagInput;
                                flagInputTextBox.CaretIndex = vm.FlagInput.Length;
                            }
                        }, DispatcherPriority.Background);
                    };

                    flagInputTextBox.PastingFromClipboard += pasteHandler;
                    Disposable.Create(() => flagInputTextBox.PastingFromClipboard -= pasteHandler)
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
