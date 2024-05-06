using System;
using Avalonia;
using Avalonia.ReactiveUI;
using CommandLine;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using RandomizerCore.Asm;

namespace CrossPlatformUI.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {

        BuildAvaloniaApp()
            .AfterSetup(_ =>
            {
                App.ServiceContainer ??= new ();
                App.ServiceContainer.AddSingleton<IAsmEngine>(x => new DesktopJsEngine());
                App.SuspensionDriver = new LocalFilePersistenceService();
                // App.ServiceContainer.AddSingleton<IPersistenceService>(x => new LocalFilePersistenceService());
            })
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}