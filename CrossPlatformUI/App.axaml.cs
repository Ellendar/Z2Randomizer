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
using Z2Randomizer.RandomizerCore;
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
        ServiceContainer.AddSingleton<SpriteLoaderService>();
        Services = ServiceContainer.BuildServiceProvider();
        var files = FileSystemService!;
        try
        {
            var json = files.OpenFileSync(IFileSystemService.RandomizerPath.Settings, SETTINGS_FILENAME);
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
            await files.SaveFile(IFileSystemService.RandomizerPath.Settings, SETTINGS_FILENAME, json);
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
[JsonSerializable(typeof(RandomizerConfiguration))]
public partial class SerializationContext : JsonSerializerContext
{
    public SerializationContext(bool _) // added an argument to avoid constructors colliding
        : base(InitSafeOptions())
    {
    }

    /// modifies original context - can't be called after init
    private static JsonSerializerOptions InitSafeOptions()
    {
        var options = Default.GeneratedSerializerOptions!;
        options.Converters.Add(new SafeStringEnumConverterFactory());
        return options;
    }

    /// returns a new options copy with safe serialization
    public static JsonSerializerOptions CreateSafeOptions()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = SerializationContext.Default
        };
        options.Converters.Add(new SafeStringEnumConverterFactory());
        return options;
    }
}

public sealed class SafeStringEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum;

    [RequiresUnreferencedCode("Uses reflection to construct generic enum converters at runtime.")]
    [SuppressMessage("Trimming", "IL2046:'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.", Justification = " Justification = \"Enum converters are created dynamically; enum metadata is preserved.\")]")]
    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var converterType = typeof(SafeStringEnumConverter<>)
            .MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// Custom StringEnumConverter that returns default instead of failing for unknown values
public sealed class SafeStringEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();

                if (!string.IsNullOrEmpty(value) &&
                    Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
                {
                    return parsed;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out var raw) && Enum.IsDefined(typeof(T), raw))
                {
                    return (T)Enum.ToObject(typeof(T), raw);
                }
            }
        }
        catch // we prefer returning the default over failing
        {
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override T ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        try
        {
            var value = reader.GetString();

            if (!string.IsNullOrEmpty(value) &&
                Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
            {
                return parsed;
            }
        }
        catch // we prefer returning the default over failing
        {
        }

        return default;
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
