using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
