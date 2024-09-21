using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class ItemsView : ReactiveUserControl<MainViewModel>
{
    public ItemsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    private void ShuffleItemChecked(object? sender, RoutedEventArgs e)
    {
        CheckBox mixOverworldAndPalaceItemsCheckbox = this.FindControl<CheckBox>("MixOverworldAndPalaceItemsCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
        mixOverworldAndPalaceItemsCheckbox.IsChecked = false;

        if(sender != null && ((CheckBox)sender).Name == "ShuffleOverworldItemsCheckbox" && !(((CheckBox)sender).IsChecked ?? false))
        {
            CheckBox pBagsInShuffleCheckbox = this.FindControl<CheckBox>("PBagsInShuffleCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            pBagsInShuffleCheckbox.IsChecked = false;
        }
    }
}