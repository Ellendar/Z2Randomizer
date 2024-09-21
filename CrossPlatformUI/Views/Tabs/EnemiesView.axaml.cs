using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class EnemiesView : ReactiveUserControl<MainViewModel>
{
    public EnemiesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    private void EnemyShuffleChecked(object? sender, RoutedEventArgs e)
    {
        bool overworldEnemyShuffle = this.FindControl<CheckBox>("ShufflePalaceEnemiesCheckbox")?.IsChecked ?? true;
        bool palaceEnemyShuffle = this.FindControl<CheckBox>("ShuffleOverworldEnemiesCheckbox")?.IsChecked ?? true;
        CheckBox mixLargeAndSmallEnemiesCheckbox = this.FindControl<CheckBox>("MixLargeAndSmallEnemiesCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox shuffleDripperCheckbox = this.FindControl<CheckBox>("ShuffleDripperCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        CheckBox generatorsMatchCheckbox = this.FindControl<CheckBox>("GeneratorsMatchCheckbox") ?? throw new System.Exception("Missing Required Validation Element");


        if (!overworldEnemyShuffle && !palaceEnemyShuffle)
        {
            mixLargeAndSmallEnemiesCheckbox.IsChecked = false;
        }

        if(!palaceEnemyShuffle)
        {
            shuffleDripperCheckbox.IsChecked = false;
            generatorsMatchCheckbox.IsChecked = false;
        }
    }

}