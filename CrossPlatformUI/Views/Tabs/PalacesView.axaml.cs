using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

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
}
