using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class GenerateRomView : ReactiveUserControl<GenerateRomViewModel>
{
    public GenerateRomView()
    {
        InitializeComponent();
        this.WhenActivated((MultipleDisposable disposables) =>
        {
            SubscribeExtensions.Subscribe(
                CancelGen.WhenAnyValue(x => x.IsVisible),
                _ =>
                {
                    if (CancelGen?.IsVisible ?? false)
                    {
                        CancelGen.Focus();
                    }
                })
                .DisposeWith(disposables);

            SubscribeExtensions.Subscribe(
                CloseGen.WhenAnyValue(x => x.IsVisible),
                _ =>
                {
                    if (CloseGen?.IsVisible ?? false)
                    {
                        CloseGen.Focus();
                    }
                })
                .DisposeWith(disposables);
        });
    }
}