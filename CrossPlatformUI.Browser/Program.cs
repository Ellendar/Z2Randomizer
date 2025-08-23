using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore;

[assembly: SupportedOSPlatform("browser")]

namespace CrossPlatformUI.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
#if DEBUG
        AppContext.SetSwitch("System.Runtime.InteropServices.EnableDebugLogging", true);
#endif

        return BuildAvaloniaApp()
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(_ =>
        {
            App.ServiceContainer ??= new();
            // not sure if this will be possible using js65.BrowserJsEngine, but using it for now
            App.ServiceContainer.AddSingleton<Hyrule.NewAssemblerFn>((opts, debug) => new BrowserJsEngine(opts));
            App.FileSystemService = new BrowserFileService();
            App.ServiceContainer.AddSingleton<IFileSystemService>(x => App.FileSystemService);
            // App.SyncSuspensionDriver = new LocalStoragePersistenceService();
        })
        .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}