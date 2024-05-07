using System;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;
using ReactiveUI;

namespace CrossPlatformUI;

public class AppViewLocator : ReactiveUI.IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        MainViewModel context => new MainView { DataContext = context },
        MainWindowViewModel context => new MainWindow { DataContext = context },
        RomFileViewModel context => new RomFileView { DataContext = context },
        GenerateRomViewModel context => new GenerateRomView { DataContext = context },
        HeaderViewModel context => new HeaderView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}