using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}
