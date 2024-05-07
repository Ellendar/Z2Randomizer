using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views;

public partial class HeaderView : ReactiveUserControl<HeaderViewModel>
{
    public HeaderView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}