using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Material.Styles.Assists;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Primitives;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;

namespace CrossPlatformUI;

public sealed partial class App : Application // , IDisposable
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version?.ToString(version.Revision > 0 ? 4 : 3);

#pragma warning disable CS0162 // Unreachable code detected
        if (versionString == Z2Randomizer.GitInfo.Tag && !Z2Randomizer.GitInfo.IsDirty)
        {
            Version = $"v{versionString}";
        }
        else if (!Z2Randomizer.GitInfo.IsDirty)
        {
            Version = $"[{Z2Randomizer.GitInfo.Branch}:{Z2Randomizer.GitInfo.Commit}]";
        }
        else
        {
            Version = $"[{Z2Randomizer.GitInfo.Branch}]";
        }
#pragma warning restore CS0162 // Unreachable code detected
#if DEBUG
        Version += " (Debug)";
#endif

        Title = $"Zelda II Randomizer {Version}";
    }

    public static ServiceCollection? ServiceContainer;

    public static IFileSystemService? FileSystemService;
    public static ICheckUpdateService? CheckUpdateService;

    public static string Version = "";
    public static string Title = "";
    public const string SETTINGS_FILENAME = "Settings_v5_1.json";

    public static TopLevel? TopLevel { get; private set; }

    private MainViewModel? main;

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceContainer ??= new();
        ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(TopLevel));
        ServiceContainer.AddSingleton(services => new RoomLoaderService(services.GetRequiredService<IFileSystemService>()));
        ServiceContainer.AddSingleton(services => new SpriteLoaderService(services.GetRequiredService<IFileSystemService>()));
        Services = ServiceContainer.BuildServiceProvider();
        var files = FileSystemService!;
        try
        {
            var json = files.OpenFileSync(IFileSystemService.RandomizerPath.Settings, SETTINGS_FILENAME);
            main = JsonSerializer.Deserialize(json, SerializationContext.SettingsReadTypeInfo)!;
        }
        catch (System.IO.FileNotFoundException) { /* No settings file exists */ }
        catch (Exception)
        {
            // Could not load settings, so just use the default instead
            // TODO: We need to do something to try and recover if the settings failed so we don't lose the rom data
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) { throw; }
#endif
        }

        main ??= new MainViewModel();

        if (main.Config.SpriteName != "Link")
        {
            var spriteLoaderService = App.Current?.Services?.GetService<SpriteLoaderService>();
            Debug.Assert(spriteLoaderService != null);

            SubscribeExtensions.Subscribe(
                spriteLoaderService.Sprites
                    .Where(sprites => sprites.Count > 0)
                    .Take(1),
                sprites =>
                {
                    var sprite = sprites.FirstOrDefault(loaded => loaded.DisplayName == main.Config.SpriteName);
                    if (sprite != null)
                    {
                        main.Config.Sprite = sprite;
                    }
                });
        }

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (_, _) =>
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
                    Title = Title,
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

        // Turn of window animations that were making the comboboxes glitch out when changing tabs.
        var state = !TransitionAssist.GetDisableTransitions(TopLevel!);
        TransitionAssist.SetDisableTransitions(TopLevel!, state);

        ThemeHelper.SetTheme(main.RandomizerViewModel.ThemeVariantName);

        // If system theme changes, call SetTheme again with the saved theme.
        // If light or dark has been set specifically, this will change nothing.
        // If it's unset, it will change to the new system theme.
        ActualThemeVariantChanged += (_, _) =>
            ThemeHelper.SetTheme(main.RandomizerViewModel.ThemeVariantName);

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
            var json = JsonSerializer.Serialize(main!, SerializationContext.SettingsWriter.MainViewModel);
            await files.SaveFile(IFileSystemService.RandomizerPath.Settings, SETTINGS_FILENAME, json);
        });
    }

    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}
