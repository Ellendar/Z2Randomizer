using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using DialogHostAvalonia;
using ReactiveUI;

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