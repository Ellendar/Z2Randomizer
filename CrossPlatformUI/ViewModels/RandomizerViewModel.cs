using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels.Tabs;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using RandomizerCore;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

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
        
        LoadPreset = ReactiveCommand.Create<string>((flags) =>
        {
            Main.Config.Flags = flags;
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
        CanGenerate = this.WhenAnyValue(
            x => x.Flags,
            x => x.Main.Config.Seed,
            x => x.Main.RomFileViewModel.HasRomData,
            (flags, seed, hasRomData) =>
                IsFlagStringValid(flags) && !string.IsNullOrWhiteSpace(seed) && hasRomData
        );
        Generate = ReactiveCommand.CreateFromTask(async () =>
        {
            //NYI
            await Task.CompletedTask;
            //await DialogHost.Show("GenerateRomDialog");
        }, CanGenerate);
        this.WhenActivated(OnActivate);
    }

    private void OnActivate(CompositeDisposable disposable)
    {
        Flags = string.IsNullOrEmpty(Main.Config.Flags)
            ? MainViewModel.BeginnerPreset
            : Main.Config.Flags;

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
                (palaceItems ?? true) || (overworldItems ?? true)
        ).Subscribe(_ =>
        {
            Main.Config.MixOverworldAndPalaceItems = false;            
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
            if (IsFlagStringValid(value) && value != Main.Config.Flags)
            {
                Main.Config.Flags = value;
            }
            this.RaiseAndSetIfChanged(ref validatedFlags, value);
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
    public ReactiveCommand<string, Unit> LoadPreset { get; }
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
