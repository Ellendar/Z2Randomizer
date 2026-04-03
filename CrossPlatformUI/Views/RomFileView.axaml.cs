using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Diagnostics.CodeAnalysis;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class RomFileView : ReactiveUserControl<RomFileViewModel>
{
    public RomFileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}