using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class OverworldView : ReactiveUserControl<MainViewModel>
{
    public OverworldView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    private void ShuffleEncountersUnchecked(object sender, RoutedEventArgs args)
    {
        CheckBox allowLavaInShuffleCheckbox = this.FindControl<CheckBox>("AllowLavaInShuffleCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        allowLavaInShuffleCheckbox.IsChecked = false;
        CheckBox allowUnsafePathEncountersCheckbox = this.FindControl<CheckBox>("AllowUnsafePathEncountersCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        allowUnsafePathEncountersCheckbox.IsChecked = false;
    }

    private void PalacesSwapContinentsUnchecked(object sender, RoutedEventArgs args)
    {
        CheckBox includeGPInShuffleCheckbox = this.FindControl<CheckBox>("IncludeGPInShuffleCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        includeGPInShuffleCheckbox.IsChecked = false;
    }
}