using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using CrossPlatformUI.Presets;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels.Tabs;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
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
        get => Main.Config.Seed ?? "";
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

    [JsonIgnore]
    public string AppVersion
    {
        get => $"Z2R {App.Version}";
    }

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

        CheckForUpdates = ReactiveCommand.CreateFromTask(async () =>
        {
            var checkUpdateService = App.Current?.Services?.GetService<ICheckUpdateService>();
            if(checkUpdateService == null)
            {
                throw new Exception("Unable to load update service");
            }
            await checkUpdateService.CheckUpdate();
        });

        ToggleTheme = ReactiveCommand.Create(() =>
        {
            if(App.Current!.ActualThemeVariant == ThemeVariant.Dark)
            {
                App.Current!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (App.Current!.ActualThemeVariant == ThemeVariant.Light)
            {
                App.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            ThemeVariantName = App.Current.RequestedThemeVariant?.Key.ToString() ?? "Default";
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

        VisitDiscord = ReactiveCommand.CreateFromTask<Control>(async control =>
        {
            var topLevel = TopLevel.GetTopLevel(control);
            if (topLevel is not null)
            {
                await topLevel.Launcher.LaunchUriAsync(new Uri("https://discord.com/invite/BsK47Nsrde"));
            }
        });

        VisitWiki = ReactiveCommand.CreateFromTask<Control>(async control =>
        {
            var topLevel = TopLevel.GetTopLevel(control);
            if (topLevel is not null)
            {
                await topLevel.Launcher.LaunchUriAsync(new Uri("https://github.com/Ellendar/Z2Randomizer/wiki"));
            }
        });

        SaveNewPreset = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetDialogOpen = true;
        });
        SaveAsPreset = ReactiveCommand.Create((string name) =>
        {
            var updatedPreset = new CustomPreset
            {
                Preset = name,
                Config =
                {
                    Flags = Main.Config.Flags
                }
            };
            var collection = Main.SaveNewPresetViewModel.SavedPresets;
            // makeshift FindIndex since ObservableCollection doesn't have one
            int presetIndex = -1;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Preset == name) { presetIndex = i; break; }
            }
            if (presetIndex == -1) { throw new Exception("Trying to overwrite preset that does not exist"); }
            // the entire item has to be set so the ObservableCollection works correctly
            collection[presetIndex] = updatedPreset;
        });
        ClearSavedPreset = ReactiveCommand.Create((string name) =>
        {
            var item = Main.SaveNewPresetViewModel.SavedPresets
                .FirstOrDefault(x => x.Preset == name);
            if (item != null)
            {
                Main.SaveNewPresetViewModel.SavedPresets.Remove(item);
            }
        });
        this.WhenActivated(OnActivate);
    }

    private void OnActivate(CompositeDisposable disposable)
    {
        // If the Flags are entirely default, use the beginner preset
        Flags = Main.Config.Flags == new RandomizerConfiguration().Flags
            ? BeginnerPreset.Preset.Flags
            : Main.Config.Flags.Trim() ?? "";

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
                Main.Config.DripperEnemyOption = DripperEnemyOption.ONLY_BOTS;
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

        // If shuffle palaces is off, then don't allow shuffling GP
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
    public ReactiveCommand<Unit, Unit> CheckForUpdates { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> ToggleTheme { get; }
    [JsonIgnore]
    public ReactiveCommand<Control, Unit> VisitDiscord { get; }
    [JsonIgnore]
    public ReactiveCommand<Control, Unit> VisitWiki { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> SaveNewPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<string, Unit> SaveAsPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<string, Unit> ClearSavedPreset { get; }
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
