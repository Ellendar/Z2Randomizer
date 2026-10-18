using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

public partial class BiomesView : ReactiveUserControl<MainViewModel>
{
    public BiomesView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
