using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
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

    public static ServiceCollection? ServiceContainer;
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Remove built-in validation plugins
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
        
        ServiceContainer ??= new ();
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel()
                };

                ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(desktop.MainWindow));
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };

                ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(TopLevel.GetTopLevel(singleViewPlatform.MainView)));
                break;
        }

        Services = ServiceContainer.BuildServiceProvider();
        
        base.OnFrameworkInitializationCompleted();
    }

    public new static App? Current => Application.Current as App;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}