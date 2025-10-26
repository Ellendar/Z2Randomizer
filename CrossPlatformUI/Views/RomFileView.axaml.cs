using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

public partial class RomFileView : ReactiveUserControl<RomFileViewModel>
{
    public RomFileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}