using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using ReactiveUI;

namespace CrossPlatformUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        // Prevent the previewer's DataContext from being set when the application is run.
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
    }
}