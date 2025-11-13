using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Browser;
using ReactiveUI.Avalonia;
using CrossPlatformUI.Services;
using Z2Randomizer.RandomizerCore;

[assembly: SupportedOSPlatform("browser")]

namespace CrossPlatformUI.Browser;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
internal sealed partial class Program
{
    [JSImport("globalThis.window.SetTitle")]
    internal static partial void SetTitle(string title);

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

            App.ServiceContainer.AddSingleton<Hyrule.NewAssemblerFn>((opts, debug) => new BrowserJsEngine(opts));
            App.FileSystemService = new BrowserFileService();
            App.ServiceContainer.AddSingleton<IFileSystemService>(x => App.FileSystemService);

            SetTitle(App.Title);
        })
        .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
#if DEBUG
             .LogToTrace()
#endif
             .With(new SkiaOptions
             {
                 MaxGpuResourceSizeBytes = 64 * 1024 * 1024, // Default is 28 MB
             });
}
