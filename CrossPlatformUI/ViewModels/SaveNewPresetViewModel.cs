using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

public class CustomPreset : ReactiveObject
{
    /// name of preset
    private string preset = "";
    public string Preset { get => preset; set => this.RaiseAndSetIfChanged(ref preset, value); }

    private RandomizerConfiguration? config;

    // presets might be created in different versions with options that
    // are not known to this version. keep the original raw preset JSON
    // to avoid changing presets unexpectedly.
    [JsonPropertyName("Config")]
    public JsonElement? RawConfig { get; set; }
    [JsonIgnore]
    public RandomizerConfiguration Config
    {
        get
        {
            if (config == null)
            {
                if (RawConfig.HasValue)
                {
                    // Deserialize with safe options to handle modified Enums affecting presets
                    var parsed = JsonSerializer.Deserialize<RandomizerConfiguration>(RawConfig.Value, SerializationContext.CreateSafeOptions());
                    if (parsed != null)
                    {
                        config = parsed;
                        return config;
                    }
                }
                config = new RandomizerConfiguration();
            }
            return config;
        }
        set
        {
            RawConfig = JsonSerializer.SerializeToElement(value, SerializationContext.Default.RandomizerConfiguration);
            this.RaiseAndSetIfChanged(ref config, value);
        }
    }

    /// empty constructor for serialization only
    public CustomPreset()
    {
    }

    public CustomPreset(string preset, RandomizerConfiguration config)
    {
        Preset = preset;
        Config = config;
    }
}

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class SaveNewPresetViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

#pragma warning disable CS8618
    [JsonConstructor]
    public SaveNewPresetViewModel() {}
#pragma warning restore CS8618
    
    public SaveNewPresetViewModel(MainViewModel main)
    {
        Main = main;
        HostScreen = Main;
        PresetName = string.Empty;
        Activator = new();
        SavePreset = ReactiveCommand.Create(() => {
            Main.SaveNewPresetDialogOpen = false;
            // Setting the preset config through the flags creates a deep clone instead of a reference
            var preset = new CustomPreset(PresetName, new RandomizerConfiguration(Main.Config.SerializeFlags()));
            SavedPresets.Add(preset);
        });
        CancelPreset = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetDialogOpen = false;
        });
        this.WhenActivated((CompositeDisposable disposables) =>
        {
            PresetName = string.Empty;
        });
        SavedPresets
            .ToObservableChangeSet()
            .Select(_ => SavedPresets.Count > 0)
            .StartWith(SavedPresets.Count > 0)
            .ToProperty(this, x => x.HasSavedPresets, out hasSavedPresets);
    }
    
    private ObservableCollection<CustomPreset> savedPresets = new ();
    public ObservableCollection<CustomPreset> SavedPresets { get => savedPresets; set => this.RaiseAndSetIfChanged(ref savedPresets, value); }

    [JsonIgnore]
    private readonly ObservableAsPropertyHelper<bool> hasSavedPresets;
    [JsonIgnore]
    public bool HasSavedPresets => hasSavedPresets.Value;

    private string presetName = "";
    [JsonIgnore]
    public string PresetName { get => presetName; set => this.RaiseAndSetIfChanged(ref presetName, value); }
    
    [JsonIgnore]
    private MainViewModel Main { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> SavePreset { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CancelPreset { get; }


    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}
