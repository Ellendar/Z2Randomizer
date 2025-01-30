using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;

[assembly: SupportedOSPlatform("browser")]

namespace CrossPlatformUI.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .UseReactiveUI()
        .AfterSetup(_ =>
        {
            App.ServiceContainer ??= new ();
            //XXX: We broke the web version. We'll fix it later.
            //App.ServiceContainer.AddSingleton<IAsmEngine>(x => new BrowserJsEngine());
            App.ServiceContainer.AddSingleton<IFileSystemService>(x => App.FileSystemService!);
            App.FileSystemService = new BrowserFileService();
            // App.SyncSuspensionDriver = new LocalStoragePersistenceService();
        })
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}