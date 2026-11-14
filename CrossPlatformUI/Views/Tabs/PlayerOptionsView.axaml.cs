using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

public partial class PlayerOptionsView : ReactiveUserControl<MainViewModel>
{
    public PlayerOptionsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
