using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Presets;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels.Tabs;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class RandomizerViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{
    [JsonIgnore]
    public IObservable<bool> CanGenerateObservable { get; private set; }

    [JsonIgnore]
    public BehaviorSubject<bool> FlagsValidSubject = new(true);

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
    public string FlagInput { get; set { field = value.Trim(); this.RaisePropertyChanged(); } } = "";

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
            Main.Config.DeserializeFlags(config.SerializeFlags());
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

        var seedValidObservable = this.WhenAnyValue(x => x.Main.Config.Seed, seed => !string.IsNullOrWhiteSpace(seed));

        CanGenerateObservable = Observable.CombineLatest(
            FlagsValidSubject, seedValidObservable, Main.RomFileViewModel.HasRomDataObservable, Main.GenerateRomViewModel.IsRunning,
            (flagsValid, seedValid, hasRom, isRunning) => flagsValid && seedValid && hasRom && !isRunning);

        Generate = ReactiveCommand.Create(() =>
        {
            Main.GenerateRomDialogOpen = true;
        }, CanGenerateObservable);

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
            var updatedPreset = new CustomPreset(name, new RandomizerConfiguration(Main.Config.SerializeFlags()));
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

    private void OnActivate(CompositeDisposable disposables)
    {
        var loadedFlags = Main.Config.SerializeFlags(); // this serializes the configuration
        var defaultFlags = new RandomizerConfiguration().SerializeFlags();
        // If the flags are entirely default, use the beginner preset
        if (loadedFlags == defaultFlags)
        {
            Main.Config.DeserializeFlags(BeginnerPreset.Preset.SerializeFlags());
        }

        // flag updates from RandomizerConfiguration always overwrites our flag input
        Main.FlagsObservable
            .Subscribe(flags => FlagInput = flags)
            .DisposeWith(disposables);

        this.WhenAnyValue(x => x.FlagInput)
            .WithLatestFrom(Main.FlagsObservable,
                (Input, Current) => (Input, Current, IsValid: Input == Current || IsFlagStringValid(Input)))
            .Do(x => FlagsValidSubject.OnNext(x.IsValid))
            .Where(x => x.IsValid && x.Input != x.Current)
            .Subscribe(x => Main.Config.DeserializeFlags(x.Input))
            .DisposeWith(disposables);

        Main.Config.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case "Seed":
                    this.RaisePropertyChanged(nameof(Seed));
                    break;
            }
        };

        this.ValidationRule(x => x.FlagInput, FlagsValidSubject, "Invalid Flags");

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

    // Unique identifier for the routable view model.
    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}
