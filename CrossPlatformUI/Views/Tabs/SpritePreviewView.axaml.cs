using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels.Tabs;

namespace CrossPlatformUI.Views.Tabs;

public partial class SpritePreviewView : ReactiveUserControl<SpritePreviewViewModel>
{
    public SpritePreviewView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}
