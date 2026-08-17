using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated((MultipleDisposable disposables) =>
        {
            // at most one of these two checkboxes must be checked
            CheckBox noDuplicateRoomsByLayoutCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByEnemiesCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox noDuplicateRoomsByEnemiesCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByLayoutCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            IObservable<bool?> byLayoutObservable = noDuplicateRoomsByLayoutCheckbox.GetObservable(CheckBox.IsCheckedProperty);
            IObservable<bool?> byEnemiesObservable = noDuplicateRoomsByEnemiesCheckbox.GetObservable(CheckBox.IsCheckedProperty);

            // To ReactiveUI: You're gonna handle nulls and you're gonna like it
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            SubscribeExtensions.Subscribe(
                byLayoutObservable,
                byLayoutValue =>
                {
                    if (byLayoutValue ?? false)
                    {
                        noDuplicateRoomsByEnemiesCheckbox.IsChecked = false;
                    }
                })
                .DisposeWith(disposables);

            SubscribeExtensions.Subscribe(
                byEnemiesObservable,
                byEnemiesValue =>
                {
                    if (byEnemiesValue ?? false)
                    {
                        noDuplicateRoomsByLayoutCheckbox.IsChecked = false;
                    }
                })
                .DisposeWith(disposables);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        });
    }
}
