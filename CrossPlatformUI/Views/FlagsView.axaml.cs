using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views;

public partial class FlagsView : ReactiveUserControl<FlagsViewModel>
{
    public FlagsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}