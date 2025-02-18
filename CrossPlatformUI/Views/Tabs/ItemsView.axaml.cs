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
}