using System;
using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace CrossPlatformUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };

                services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainWindowViewModel()
                };

                services.AddSingleton<IFilesService>(x => new FilesService(TopLevel.GetTopLevel(singleViewPlatform.MainView)));
                break;
        }

        services.AddSingleton<RandomizerConfigService>(x => new RandomizerConfigService());
        Services = services.BuildServiceProvider();
        
        base.OnFrameworkInitializationCompleted();
    }

    public new static App? Current => Application.Current as App;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}