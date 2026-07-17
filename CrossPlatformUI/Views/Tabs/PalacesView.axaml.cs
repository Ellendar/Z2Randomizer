using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables =>
        {
            // at most one of these two checkboxes must be checked
            CheckBox noDuplicateRoomsByLayoutCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByEnemiesCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox noDuplicateRoomsByEnemiesCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByLayoutCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            IObservable<bool?> byLayoutObservable = noDuplicateRoomsByLayoutCheckbox.GetObservable(CheckBox.IsCheckedProperty);
            IObservable<bool?> byEnemiesObservable = noDuplicateRoomsByEnemiesCheckbox.GetObservable(CheckBox.IsCheckedProperty);
            byLayoutObservable.Subscribe(byLayoutValue =>
            {
                if (byLayoutValue ?? false)
                {
                    noDuplicateRoomsByEnemiesCheckbox.IsChecked = false;
                }
            })
            .DisposeWith(disposables);
            byEnemiesObservable.Subscribe(byEnemiesValue =>
            {
                if (byEnemiesValue ?? false)
                {
                    noDuplicateRoomsByLayoutCheckbox.IsChecked = false;
                }
            })
            .DisposeWith(disposables);


        });
    }
}
