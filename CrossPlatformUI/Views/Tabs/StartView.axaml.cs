using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

public partial class StartView : ReactiveUserControl<MainViewModel>
{
    public StartView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
