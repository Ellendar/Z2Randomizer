using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CrossPlatformUI.Views;

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
