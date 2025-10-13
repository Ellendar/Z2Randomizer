using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

public partial class GenerateRomView : ReactiveUserControl<GenerateRomViewModel>
{
    public GenerateRomView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}