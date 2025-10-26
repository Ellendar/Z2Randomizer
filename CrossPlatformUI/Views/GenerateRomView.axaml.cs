using Avalonia.Markup.Xaml;
using static System.ObservableExtensions;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

public partial class GenerateRomView : ReactiveUserControl<GenerateRomViewModel>
{
    public GenerateRomView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            CancelGen.WhenAnyValue(x => x.IsVisible).Subscribe(_ =>
            {
                if (CancelGen?.IsVisible ?? false)
                {
                    CancelGen.Focus();
                }
            });
            CloseGen.WhenAnyValue(x => x.IsVisible).Subscribe(_ =>
            {
                if (CloseGen?.IsVisible ?? false)
                {
                    CloseGen.Focus();
                }
            });
        });
    }
}