using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

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