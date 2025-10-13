using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views.Tabs;

public partial class SpritePreviewView : ReactiveUserControl<SpritePreviewViewModel>
{
    public SpritePreviewView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}