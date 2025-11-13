using System;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;

namespace CrossPlatformUI;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        MainViewModel context => new MainView { DataContext = context },
        RomFileViewModel context => new RomFileView { DataContext = context },
        GenerateRomViewModel context => new GenerateRomView { DataContext = context },
        RandomizerViewModel context => new RandomizerView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}