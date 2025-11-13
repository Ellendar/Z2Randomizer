using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class SaveNewPresetView : ReactiveUserControl<SaveNewPresetViewModel>
{
    public SaveNewPresetView()
    {
        this.WhenActivated(disposables =>
        {
            if (!IsVisible) return;
            TextBox? host = this.FindControl<TextBox>("PresetNameTextBox");
            host!.Focus();
        });
        
        AvaloniaXamlLoader.Load(this);
    }
}