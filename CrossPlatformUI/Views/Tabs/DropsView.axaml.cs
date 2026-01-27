using System.Diagnostics.CodeAnalysis;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class DropsView : ReactiveUserControl<MainViewModel>
{
    public DropsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}