using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Services;
using Desktop.Common;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using Microsoft.Extensions.DependencyInjection;
using RandomizerCore;

namespace CrossPlatformUI.Desktop;

public static class Program
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
                App.ServiceContainer.AddSingleton<Hyrule.NewAssemblerFn>((opts, debug) => new DesktopJsEngine(opts, debug));
                App.ServiceContainer.AddSingleton<IFileSystemService>(x => App.FileSystemService!);
                App.FileSystemService = new DesktopFileService();
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
            .AfterSetup(_ =>
            {
                if (Design.IsDesignMode)
                {
                    // This can be before or after InitializeComponent.
                    App.Current!.LocateMaterialTheme<MaterialTheme>().BaseTheme = BaseThemeMode.Dark;
                }
            })
            .UseReactiveUI();
}