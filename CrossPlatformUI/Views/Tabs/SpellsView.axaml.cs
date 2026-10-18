using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;

namespace CrossPlatformUI.Views.Tabs;

public partial class SpellsView : ReactiveUserControl<MainViewModel>
{
    public SpellsView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
