using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CrossPlatformUI;

public sealed partial class App : Application // , IDisposable
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static ServiceCollection? ServiceContainer;

    public static IFileSystemService? FileSystemService;
    
    public static TopLevel TopLevel { get; private set; }

    // public static MainViewModel? Main { get; set; }

    private MainViewModel? main;

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceContainer ??= new ();
        var files = FileSystemService!;
        try
        {
            var json = files.OpenFileSync(IFileSystemService.RandomizerPath.Settings, "Settings.json");
            main = JsonSerializer.Deserialize(json, SerializationContext.Default.MainViewModel);
        }
        catch (Exception e)
        {
            // Could not load settings, so just use the default instead
        }

        main ??= new MainViewModel();
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (_,_) =>
                {
                    PersistStateInternal().Wait();
                };
                // isLaunchingNew.OnNext(Unit.Default);
                var context = main;
                desktop.MainWindow = new MainWindow
                {
                    DataContext = context,
                    Position = new PixelPoint(context!.WindowPosition.X, context!.WindowPosition.Y),
                    Width = context.WindowSize.Width,
                    Height = context.WindowSize.Height,
                };

                TopLevel = TopLevel.GetTopLevel(desktop.MainWindow)!;
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = main
                };

                TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
                break;
        }

        ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(TopLevel));
        Services = ServiceContainer.BuildServiceProvider();
        
        base.OnFrameworkInitializationCompleted();
    }

    public static async Task PersistState()
    {
        await Current!.PersistStateInternal();
    }
    
    private Task PersistStateInternal()
    {
        return Task.Run(async () =>
        {
            var files = Current?.Services?.GetService<IFileSystemService>()!;
            var json = JsonSerializer.Serialize(main!, SerializationContext.Default.MainViewModel);
            await files.SaveFile(IFileSystemService.RandomizerPath.Settings, "Settings.json", json);
        });
    }

    public new static App? Current => Application.Current as App;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    IgnoreReadOnlyProperties = true,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
)]
[JsonSerializable(typeof(MainViewModel))]
public partial class SerializationContext : JsonSerializerContext { }