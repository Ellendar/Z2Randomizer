using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using YamlDotNet.Core.Tokens;

namespace CrossPlatformUI.Views.Tabs;

public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables =>
        {
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

    /*
    private void TBirdRequiredChecked(object sender, RoutedEventArgs args)
    {
        CheckBox removeTbirdCheckbox = this.FindControl<CheckBox>("RemoveThunderbirdCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        removeTbirdCheckbox.IsChecked = false;
    }
    private void NoDuplicateRoomsByLayoutChecked(object sender, RoutedEventArgs args)
    {
        CheckBox noDuplicateRoomsByLayoutCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByEnemiesCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        noDuplicateRoomsByLayoutCheckbox.IsChecked = false;
    }
    private void NoDuplicateRoomsByEnemiesChecked(object sender, RoutedEventArgs args)
    {
        CheckBox noDuplicateRoomsByEnemiesCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByLayoutCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        noDuplicateRoomsByEnemiesCheckbox.IsChecked = false;
    }
    */
}
