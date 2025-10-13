using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}