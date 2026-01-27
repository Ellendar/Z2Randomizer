using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Browser;
using ReactiveUI.Avalonia;
using CrossPlatformUI.Services;
using js65;
using Z2Randomizer.RandomizerCore;

[assembly: SupportedOSPlatform("browser")]

namespace CrossPlatformUI.Browser;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
internal sealed partial class Program
{
    [JSImport("globalThis.window.SetTitle")]
    internal static partial void SetTitle(string title);

    private static string LoadTextFileCallback(string basePath, string relPath)
    {
        return _assembly.ReadResource(relPath);
    }
    private static byte[] LoadBinaryFileCallback(string basePath, string relPath)
    {
        return _assembly.ReadBinaryResource(relPath);
    }

    private static readonly Assembly _assembly = typeof(RandomizerConfiguration).Assembly;

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

            App.ServiceContainer.AddSingleton<Hyrule.NewAssemblerFn>(MakeBrowserAssembler);
            App.FileSystemService = new BrowserFileService();
            App.ServiceContainer.AddSingleton<IFileSystemService>(x => App.FileSystemService);

            SetTitle(App.Title);
        })
        .StartBrowserAppAsync("out");
    }


    public static Assembler MakeBrowserAssembler(Js65Options? options = null, bool debugJavaScript = false)
    {
        var callbacks = new Js65Callbacks
        {
            OnFileReadBinary = LoadBinaryFileCallback,
            OnFileReadText = LoadTextFileCallback
        };
        return new BrowserJsEngine(options, callbacks);
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

internal static class AssemblyExtensions
{
    public static string ReadResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    public static async Task<string> ReadResourceAsync(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        await using var stream = assembly.GetManifestResourceStream(name)!;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
    public static byte[] ReadBinaryResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new BinaryReader(stream);
        return reader.ReadBytes((int)stream.Length);
    }
}
