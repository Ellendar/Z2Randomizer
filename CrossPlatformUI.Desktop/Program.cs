using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI.Avalonia;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using CrossPlatformUI.Services;
using Desktop.Common;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Desktop;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var version = Assembly.GetEntryAssembly()!.GetName().Version!;
                    var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
                    WinSparkle.win_sparkle_set_appcast_url(
                        "https://raw.githubusercontent.com/Ellendar/Z2Randomizer/refs/heads/main/appcast.xml");
                    WinSparkle.win_sparkle_set_app_details("Z2Randomizer", "Z2Randomizer",
                        versionString); // THIS CALL NOT IMPLEMENTED YET
                    WinSparkle.win_sparkle_init();
                    App.CheckUpdateService = new DesktopCheckUpdateService();
                    App.ServiceContainer.AddSingleton<ICheckUpdateService>(x => App.CheckUpdateService!);
                }
            })
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
             .With(new SkiaOptions
             {
                 MaxGpuResourceSizeBytes = 64 * 1024 * 1024, // Default is 28 MB
             })
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