using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using RandomizerCore;
using Avalonia.Styling;

namespace CrossPlatformUI.ViewModels;

public class RandomizerViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{
    private bool IsFlagStringValid(string flags)
    {
        try
        {
            _ = new RandomizerConfiguration(flags);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [JsonIgnore]
    public string Seed
    {
        get => Main.Config.Seed;
        set
        {
            Main.Config.Seed = value.Trim();
            this.RaisePropertyChanged();
        }
    }

    private string themeVariantName = "";
    public string ThemeVariantName 
    {
        get
        {
            return themeVariantName;
        }
        set
        {
            App.Current!.RequestedThemeVariant = value switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
            themeVariantName = value;
        }
    }
    private int currentTabIndex;
    public int CurrentTabIndex { get => currentTabIndex; set => this.RaiseAndSetIfChanged(ref currentTabIndex, value); }
    
    [JsonConstructor]
#pragma warning disable CS8618 
    public RandomizerViewModel() {}
#pragma warning restore CS8618
    public RandomizerViewModel(MainViewModel main)
    {
        Main = main;
        HostScreen = Main;
        CustomizeViewModel = new(Main);
        Activator = new ViewModelActivator();

        RerollSeed = ReactiveCommand.Create(() =>
        {
            Main.Config.Seed = new Random().Next(0, 999999999).ToString();
        });
        
        LoadPreset = ReactiveCommand.Create<RandomizerConfiguration>(config =>
        {
            // By writing the flags like this, it will update all the reactive elements watching each
            // individual fields.
            Main.Config.Flags = config.Flags;
        });
        
        LoadRom = ReactiveCommand.CreateFromObservable(
            () => Main.Router.Navigate.Execute(Main.RomFileViewModel)
        );

        SaveFolder = ReactiveCommand.CreateFromTask(async () =>
        {
            var fileDialog = App.Current?.Services?.GetService<IFileDialogService>()!;
            var folder = await fileDialog.OpenFolderAsync();
            Main.OutputFilePath = folder?.Path.LocalPath ?? "";
        });

        ToggleTheme = ReactiveCommand.CreateFromTask(async () =>
        {
            if(App.Current!.ActualThemeVariant == ThemeVariant.Dark)
            {
                App.Current!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (App.Current!.ActualThemeVariant == ThemeVariant.Light)
            {
                App.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            ThemeVariantName = App.Current.RequestedThemeVariant.Key.ToString();
        });
        CanGenerate = this.WhenAnyValue(
            x => x.Flags,
            x => x.Main.Config.Seed,
            x => x.Main.RomFileViewModel.HasRomData,
            (flags, seed, hasRomData) =>
                IsFlagStringValid(flags) && !string.IsNullOrWhiteSpace(seed) && hasRomData
        );
        Generate = ReactiveCommand.Create(() =>
        {
            Main.GenerateRomDialogOpen = true;
        }, CanGenerate);

        SaveNewPreset = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetDialogOpen = true;
        });
        SaveAsPreset = ReactiveCommand.Create((string name) =>
        {
            Main.SaveNewPresetViewModel.SavedPresets
                .First(x => x.Preset == name)
                .Config = Main.Config;
        });
        ClearSavedPresets = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetViewModel.SavedPresets.Clear();
        });
        this.WhenActivated(OnActivate);
    }

    private void OnActivate(CompositeDisposable disposable)
    {
        Flags = string.IsNullOrEmpty(Main.Config.Flags)
            ? BuiltinPreset.BeginnerPreset.Flags
            : Main.Config.Flags?.Trim() ?? "";

        Main.Config.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case "Flags":
                    Flags = ((RandomizerConfiguration)sender!).Flags;
                    break;
                case "Seed":
                    this.RaisePropertyChanged(nameof(Seed));
                    break;
            }
        };
        Main.RomFileViewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case "HasRomData":
                    this.RaisePropertyChanged(nameof(CanGenerate));
                    break;
            }
        };
        var flagsValidation = this.WhenAnyValue(
            x => x.Flags,
            IsFlagStringValid
            );
        this.ValidationRule(
            x => x.Flags,
            flagsValidation,
            "Invalid Flags");

        AddValidationRules();
    }

    private void AddValidationRules()
    {
        Main.Config.WhenAnyValue(
            x => x.ShuffleOverworldEnemies,
            x => x.ShufflePalaceEnemies,
            (a,b) => (a ?? true) || (b ?? true)
        ).Subscribe(_ =>
        {
            var overworldEnemyShuffle = Main.Config.ShuffleOverworldEnemies ?? true;
            var palaceEnemyShuffle = Main.Config.ShufflePalaceEnemies ?? true;
            if (!overworldEnemyShuffle && !palaceEnemyShuffle)
            {
                Main.Config.MixLargeAndSmallEnemies = false;
            }
            if (!palaceEnemyShuffle)
            {
                Main.Config.ShuffleDripperEnemy = false;
                Main.Config.GeneratorsAlwaysMatch = false;
            }
        });

        // When PalaceItems and OverworldItems are off, then don't allow MixingOverworldAndPalaceItems
        Main.Config.WhenAnyValue(
            x => x.ShufflePalaceItems,
            x => x.ShuffleOverworldItems,
            (palaceItems, overworldItems) =>
                !(palaceItems ?? true) || !(overworldItems ?? true)
        ).Subscribe(_ =>
        {
            if(!(Main.Config.ShufflePalaceItems ?? true) || !(Main.Config.ShufflePalaceItems ?? true))
            {
                Main.Config.MixOverworldAndPalaceItems = false;
            }
        });


        // If shuffle overworld items is off, turn off pbag cave item shuffle too
        Main.Config.ObservableForProperty(x => x.ShuffleOverworldItems)
        .Subscribe(x =>
        {
            if (x.Value ?? true) return;
            Main.Config.IncludePBagCavesInItemShuffle = false;
        });
        
        // If Palaces can't swap continents 
        Main.Config.ObservableForProperty(x => x.ShuffleEncounters)
        .Subscribe(x =>
        {
            if (x.Value ?? true) return;
            Main.Config.IncludeLavaInEncounterShuffle = false;
            Main.Config.AllowUnsafePathEncounters = false;
        });

        // If shuffle encounters is off, then don't allow shuffling GP
        Main.Config.ObservableForProperty(x => x.PalacesCanSwapContinents)
            .Subscribe(x =>
            {
                if (x.Value ?? true) return;
                Main.Config.ShuffleGP = false;
            });

        Main.ObservableForProperty(x => x.ShuffleAllExpState).Subscribe(x =>
        {
            if (!x.Value) return;
            Main.Config.ShuffleAttackExperience = true;
            Main.Config.ShuffleMagicExperience = true;
            Main.Config.ShuffleLifeExperience = true;
        });
    }

    private string validatedFlags = "";

    [JsonIgnore]
    public string Flags
    {
        get => validatedFlags;
        set
        {
            string trimmedValue = value.Trim() ?? "";
            if (IsFlagStringValid(trimmedValue) && value != Main.Config.Flags)
            {
                Main.Config.Flags = trimmedValue;
            }
            this.RaiseAndSetIfChanged(ref validatedFlags, trimmedValue);
        }
    }

    [JsonIgnore]
    public bool IsDesktop { get; } = !OperatingSystem.IsBrowser();

    [JsonIgnore]
    public MainViewModel Main { get; }
    public CustomizeViewModel CustomizeViewModel { get; }
    
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> RerollSeed { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> Generate { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> SaveFolder { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> ToggleTheme { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> SaveNewPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<string, Unit> SaveAsPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> ClearSavedPresets { get; }
    [JsonIgnore]
    public ReactiveCommand<RandomizerConfiguration, Unit> LoadPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, IRoutableViewModel> LoadRom { get; }
    [JsonIgnore]
    public IObservable<bool> CanGenerate { get; private set; }

    // Unique identifier for the routable view model.
    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}
