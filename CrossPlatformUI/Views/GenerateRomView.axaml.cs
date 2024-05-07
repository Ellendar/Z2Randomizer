using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views;

public partial class GenerateRomView : ReactiveUserControl<GenerateRomViewModel>
{
    public GenerateRomView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}