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

    private void ShuffleAllExpChecked(object? sender, RoutedEventArgs e)
    {
        CheckBox AtkExpCheckbox = this.FindControl<CheckBox>("AtkExp") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox MagExpCheckbox = this.FindControl<CheckBox>("MagExp") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox LifExpCheckbox = this.FindControl<CheckBox>("LifExp") ?? throw new System.Exception("Missing Required Validation Element");

        AtkExpCheckbox.IsChecked = true;
        AtkExpCheckbox.IsEnabled = false;
        MagExpCheckbox.IsChecked = true;
        MagExpCheckbox.IsEnabled = false;
        LifExpCheckbox.IsChecked = true;
        LifExpCheckbox.IsEnabled = false;
    }

    private void ShuffleAllExpUnchecked(object? sender, RoutedEventArgs e)
    {
        CheckBox AtkExpCheckbox = this.FindControl<CheckBox>("AtkExp") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox MagExpCheckbox = this.FindControl<CheckBox>("MagExp") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox LifExpCheckbox = this.FindControl<CheckBox>("LifExp") ?? throw new System.Exception("Missing Required Validation Element");

        AtkExpCheckbox.IsEnabled = true;
        MagExpCheckbox.IsEnabled = true;
        LifExpCheckbox.IsEnabled = true;
    }
}