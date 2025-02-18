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

}