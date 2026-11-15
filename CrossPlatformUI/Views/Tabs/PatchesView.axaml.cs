using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

public partial class PatchesView : ReactiveUserControl<MainViewModel>
{
    public PatchesView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
