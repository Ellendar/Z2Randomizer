using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

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