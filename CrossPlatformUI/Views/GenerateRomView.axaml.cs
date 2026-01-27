using System.Diagnostics.CodeAnalysis;
using static System.ObservableExtensions;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
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