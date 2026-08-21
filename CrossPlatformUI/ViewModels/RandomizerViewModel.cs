using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Presets;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels.Tabs;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class RandomizerViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    [JsonIgnore]
    public IObservable<bool> CanGenerateObservable { get; private set; }

    [JsonIgnore]
    public IObservable<IBrush> FlagInputUnderlineObservable { get; }

    private bool flagsValid = true;
    private bool FlagsValid { get => flagsValid; set => this.RaiseAndSetIfChanged(ref flagsValid, value); }

    private static bool IsFlagStringValid(string flags) => FlagPasteParser.IsValidFlagString(flags);

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
            ThemeHelper.SetTheme(value);
            this.RaiseAndSetIfChanged(ref themeVariantName, value);
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
        BiomesViewModel = new(Main);
        PalacesViewModel = new(Main);
        SpellsViewModel = new(Main);
        ItemsViewModel = new(Main);
        HintsViewModel = new(Main, this);
        CustomizeViewModel = new(Main);
        Activator = new ViewModelActivator();

        RerollSeed = ReactiveCommand.Create(() =>
        {
            Main.Config.Seed = new System.Random().Next(0, 999999999).ToString();
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
            Main.OutputFilePath = await SelectSaveFolder() ?? "";
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
            if(ThemeHelper.IsDark(ThemeVariantName))
            {
                ThemeVariantName = "Light";
            }
            else
            {
                ThemeVariantName = "Dark";
            }
        });

        var seedValidObservable = this.WhenAnyValue(x => x.Main.Config.Seed, seed => !string.IsNullOrWhiteSpace(seed));

        CanGenerateObservable = this.WhenAnyValue(x => x.FlagsValid).CombineLatest(
            seedValidObservable,
            Main.RomFileViewModel.HasRomDataObservable,
            Main.GenerateRomViewModel.WhenAnyValue(x => x.IsRunning),
            (flagsValid, seedValid, hasRom, isRunning) =>
                flagsValid && seedValid && hasRom && !isRunning);

        FlagInputUnderlineObservable = this.WhenAnyValue(x => x.ThemeVariantName)
            .CombineLatest(this.WhenAnyValue(x => x.FlagsValid), (theme, valid) =>
                ThemeHelper.GetFlagUnderlineBrush(theme, valid));

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

    public static async Task<string?> SelectSaveFolder()
    {
        var fileDialog = App.Current?.Services?.GetService<IFileDialogService>()!;
        var folder = await fileDialog.OpenFolderAsync();
        Uri? path = folder?.Path;
        // both LocalPath and TryGetLocalPath() throw for non-absoloute URIs
        string? localPath = path?.IsAbsoluteUri == true
            ? path.LocalPath
            : path?.OriginalString;
        return localPath;
    }

    private void OnActivate(MultipleDisposable disposables)
    {
        var loadedFlags = Main.Config.SerializeFlags(); // this serializes the configuration
        var defaultFlags = new RandomizerConfiguration().SerializeFlags();
        // If the flags are entirely default, use the beginner preset
        if (loadedFlags == defaultFlags)
        {
            Main.Config.DeserializeFlags(BeginnerPreset.Preset.SerializeFlags());
        }

        // flag updates from RandomizerConfiguration always overwrites our flag input
        SubscribeExtensions.Subscribe(Main.FlagsObservable, flags =>
        {
            FlagsValid = true;
            FlagInput = flags;
        })
            .DisposeWith(disposables);

        SubscribeExtensions.Subscribe(
            this.WhenAnyValue(viewModel => viewModel.FlagInput)
                .WithLatestFrom(Main.FlagsObservable, (Input, Current) => (Input, Current)),
            tuple =>
            {
                var isNew = tuple.Input != tuple.Current;
                if (isNew)
                {
                    bool isValid = IsFlagStringValid(tuple.Input);
                    FlagsValid = isValid;
                    if (isValid)
                    {
                        Main.Config.DeserializeFlags(tuple.Input);
                    }
                }
                else if (!FlagsValid)
                {
                    // The input matches the currently loaded flags (always valid), but the flag is
                    // stale after a prior invalid paste left it false.
                    FlagsValid = true;
                }
            })
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

        // ReactiveUI.Validation (latest 7.1.0) is not updated to ReactiveUI 24 and can't be used
        // this.ValidationRule(viewModel => viewModel.FlagsValid, this.WhenAnyValue(x => x.FlagsValid), "Invalid Flags");

        AddValidationRules();
    }

    private void AddValidationRules()
    {
        SubscribeExtensions.Subscribe(
            Main.Config.WhenAnyValue(
                x => x.ShuffleOverworldEnemies,
                x => x.ShufflePalaceEnemies,
                (a,b) => (a ?? true) || (b ?? true)
            ),
            _ =>
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
        SubscribeExtensions.Subscribe(
            Main.Config.WhenAnyValue(
                x => x.ShufflePalaceItems,
                x => x.ShuffleOverworldItems,
                (palaceItems, overworldItems) =>
                    !(palaceItems ?? true) || !(overworldItems ?? true)
            ),
            _ =>
        {
            if(!(Main.Config.ShufflePalaceItems ?? true) || !(Main.Config.ShufflePalaceItems ?? true))
            {
                Main.Config.MixOverworldAndPalaceItems = false;
            }
        });

        // If shuffle overworld items is off, turn off pbag cave item shuffle too
        SubscribeExtensions.Subscribe(Main.Config.ObservableForProperty(x => x.ShuffleOverworldItems), x =>
        {
            if (x.Value ?? true) return;
            Main.Config.IncludePBagCavesInItemShuffle = false;
        });
        
        // If Palaces can't swap continents 
        SubscribeExtensions.Subscribe(Main.Config.ObservableForProperty(x => x.ShuffleEncounters), x =>
        {
            if (x.Value ?? true) return;
            Main.Config.IncludeLavaInEncounterShuffle = false;
            Main.Config.AllowUnsafePathEncounters = false;
        });

        // If shuffle palaces is off, then don't allow shuffling GP
        SubscribeExtensions.Subscribe(Main.Config.ObservableForProperty(x => x.PalacesCanSwapContinents), x =>
            {
                if (x.Value ?? true) return;
                Main.Config.ShuffleGP = false;
            });

        SubscribeExtensions.Subscribe(Main.ObservableForProperty(x => x.ShuffleAllExpState), x =>
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
    [JsonIgnore]
    public BiomesViewModel BiomesViewModel { get; }
    public PalacesViewModel PalacesViewModel { get; }
    public SpellsViewModel SpellsViewModel { get; }
    public ItemsViewModel ItemsViewModel { get; }
    public HintsViewModel HintsViewModel { get; }
    public CustomizeViewModel CustomizeViewModel { get; }
    
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> RerollSeed { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> Generate { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> SaveFolder { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> CheckForUpdates { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> ToggleTheme { get; }
    [JsonIgnore]
    public ReactiveCommand<Control, RxVoid> VisitDiscord { get; }
    [JsonIgnore]
    public ReactiveCommand<Control, RxVoid> VisitWiki { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> SaveNewPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<string, RxVoid> SaveAsPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<string, RxVoid> ClearSavedPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<RandomizerConfiguration, RxVoid> LoadPreset { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, IRoutableViewModel> LoadRom { get; }

    // Unique identifier for the routable view model.
    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}
