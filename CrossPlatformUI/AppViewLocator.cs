using System;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;

namespace CrossPlatformUI;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class AppViewLocator : IViewLocator
{
    private static IViewFor? CreateView(object viewModel) => viewModel switch
    {
        MainViewModel context => new MainView { ViewModel = context },
        RomFileViewModel context => new RomFileView { ViewModel = context },
        GenerateRomViewModel context => new GenerateRomView { ViewModel = context },
        RandomizerViewModel context => new RandomizerView { ViewModel = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };

    public IViewFor? ResolveView(object? viewModel, string? contract = null)
        => viewModel is null ? null : CreateView(viewModel);

    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null) where TViewModel : class
    {
        throw new NotImplementedException();
    }
}
