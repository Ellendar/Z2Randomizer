using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Material.Styles.Assists;
using Microsoft.Extensions.DependencyInjection;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;

namespace CrossPlatformUI;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public sealed partial class App : Application // , IDisposable
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var version = Assembly.GetExecutingAssembly().GetName().Version;

#pragma warning disable CS0162 // Unreachable code detected
        if (Z2Randomizer.GitInfo.Branch == "main")
        {
            Version = $"v{version?.ToString(version.Revision > 0 ? 4 : 3)}";
        }
        else
        {
            Version = $"[{Z2Randomizer.GitInfo.Branch}:{Z2Randomizer.GitInfo.Commit}]";
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

    public static TopLevel? TopLevel { get; private set; }

    // public static MainViewModel? Main { get; set; }

    private MainViewModel? main;

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceContainer ??= new();
        ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(TopLevel));
        ServiceContainer.AddSingleton<SpriteLoaderService>();
        Services = ServiceContainer.BuildServiceProvider();
        var files = FileSystemService!;
        try
        {
            var json = files.OpenFileSync(IFileSystemService.RandomizerPath.Settings, "Settings.json");
            main = JsonSerializer.Deserialize(json, new SerializationContext(true).MainViewModel)!;
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

            spriteLoaderService.Sprites
                .Where(sprites => sprites.Count > 0)
                .Take(1)
                .Subscribe(sprites =>
                {
                    var sprite = sprites.First(loaded => loaded.DisplayName == main.Config.SpriteName);
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

/// If deserialization fails, we lose ROM data & we lose all created presets.
/// We must do our best to avoid this failing.
///
/// A safer enum parser, that uses the default value instead of failing
/// has been added.
[JsonSourceGenerationOptions(
    WriteIndented = false,
    IgnoreReadOnlyProperties = true,
    UseStringEnumConverter = true,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
)]
[JsonSerializable(typeof(MainViewModel))]
public partial class SerializationContext : JsonSerializerContext
{
    public SerializationContext(bool _) // added an argument to avoid constructors colliding
        : base(CreateOptions())
    {
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = Default.GeneratedSerializerOptions!;

        var enumNamespacePrefix = "Z2Randomizer";

        foreach (var enumType in AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                try { return a.GetTypes(); }
#pragma warning restore IL2026
                catch { return Enumerable.Empty<Type>(); }
            })
            .Where(t =>
                t.IsEnum &&
                t.IsPublic &&
                !t.IsGenericTypeDefinition &&
                t.Namespace != null && t.Namespace.StartsWith(enumNamespacePrefix)
            ))
        {
            try
            {
#pragma warning disable IL2076 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
                var converterType = typeof(SafeStringEnumConverter<>).MakeGenericType(enumType);
#pragma warning restore IL2076
                var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
                options.Converters.Add(converter);
            }
            catch
            {
                // we try our best to add the custom parsing, but proceed regardless
            }
        }

        return options;
    }
}

/// Custom StringEnumConverter that returns default instead of failing for unknown values
public class SafeStringEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number &&
                 reader.TryGetInt32(out var intValue) &&
                 Enum.IsDefined(typeof(T), intValue))
        {
            return (T)Enum.ToObject(typeof(T), intValue);
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
