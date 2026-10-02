using System;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;

namespace CrossPlatformUI;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class AppViewLocator : IViewLocator
{
    private IViewFor? FindView(object? viewModel)
    {
        if (viewModel is null) { return null; }

        return viewModel switch
        {
            MainViewModel context => new MainView { ViewModel = context },
            RomFileViewModel context => new RomFileView { ViewModel = context },
            GenerateRomViewModel context => new GenerateRomView { ViewModel = context },
            RandomizerViewModel context => new RandomizerView { ViewModel = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
        };
    }

    // ReactiveUI has three methods that do the same thing with varying degrees of type information
    // for AOT optimization compatability
    public IViewFor? ResolveView<TViewModel>(TViewModel? viewModel, string? contract = null) where TViewModel : class
    {
        return FindView(viewModel);
    }

    public IViewFor? ResolveView(object? viewModel, string? contract)
    {
        return FindView(viewModel);
    }

    public IViewFor? ResolveViewUnsafe(object? viewModel, string? contract = null)
    {
        return FindView(viewModel);
    }

    /*
        public IViewFor? ResolveView(object? viewModel, string? contract = null)
            => viewModel is null ? null : CreateView(viewModel);

        public IViewFor? ResolveView(object? viewModel)
            => viewModel is null ? null : CreateView(viewModel);
        public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null) where TViewModel : class
        {
            throw new NotImplementedException();
        }
    */
}
