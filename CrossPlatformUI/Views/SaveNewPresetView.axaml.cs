using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

public partial class SaveNewPresetView : ReactiveUserControl<SaveNewPresetViewModel>
{
    public SaveNewPresetView()
    {
        this.WhenActivated((MultipleDisposable disposables) =>
        {
            if (!IsVisible) return;
            TextBox? host = this.FindControl<TextBox>("PresetNameTextBox");
            host!.Focus();
        });

        AvaloniaXamlLoader.Load(this);
    }
}
