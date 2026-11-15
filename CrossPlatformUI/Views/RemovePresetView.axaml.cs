using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

public partial class RemovePresetView : ReactiveUserControl<RemovePresetViewModel>
{
    public RemovePresetView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
