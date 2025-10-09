using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;

namespace CrossPlatformUI.Views.Tabs;

public partial class SpritePreviewView : ReactiveUserControl<SpritePreviewViewModel>
{
    public SpritePreviewView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}