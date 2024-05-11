using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class LevelsView : ReactiveUserControl<MainViewModel>
{
    public LevelsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    private void ShuffleAllExpClicked(object? sender, RoutedEventArgs e)
    {
        this.FindControl<CheckBox>("AtkExp")!.IsChecked = true;
        this.FindControl<CheckBox>("MagExp")!.IsChecked = true;
        this.FindControl<CheckBox>("LifExp")!.IsChecked = true;
    }
}