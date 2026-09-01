using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Presets;

namespace CrossPlatformUI.ViewModels.Tabs;

public class PresetItem
{
    public PresetItem(string name, RandomizerConfiguration config, string? description = null, bool isCustom = false)
    {
        Name = name;
        Config = config;
        Description = description;
        IsCustom = isCustom;
    }

    public string Name { get; }
    public string? Description { get; }
    public string Diff { get; set; } = "";
    public bool IsCustom { get; }
    public RandomizerConfiguration Config { get; }
}

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class PresetsViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public ObservableCollection<PresetItem> Presets { get; } = new();

    // Guards that the session-saved preset name isn't overwritten by the ListBox's initial
    // auto-selection of the first item before the saved selection has been restored.
    private bool hasRestoredSelection;

    private PresetItem? selectedPreset;
    public PresetItem? SelectedPreset
    {
        get => selectedPreset;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedPreset, value);
            SetDiff();
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(HasDescription));
            this.RaisePropertyChanged(nameof(Title));
        }
    }

    public string Title => SelectedPreset?.Name ?? "";

    public string? Description => SelectedPreset?.Description;

    public bool HasDescription => !string.IsNullOrEmpty(SelectedPreset?.Description);

    public string Diff => SelectedPreset?.Diff ?? "";

    private bool hasDiff;
    public bool HasDiff { get => hasDiff; set => this.RaiseAndSetIfChanged(ref hasDiff, value); }

    public void SetDiff()
    {
        if (SelectedPreset is { } other)
        {
            var d = Main.Config.Diff(other.Config);
            HasDiff = d.Count > 0;
            other.Diff = d.Count == 0 ? "" : string.Join("\n", d.Select(t => FormatDiffLine(t)));
        }
        this.RaisePropertyChanged(nameof(Diff));
    }

    private static string FormatDiffLine((string Field, object? OldValue, object? NewValue) t)
    {
#if false
        // useful to create new built-in presets
        var value = t.NewValue switch
        {
            null => "null",
            bool b => b.ToString().ToLowerInvariant(),
            Enum e => $"{e.GetType().FullName}.{e}",
            _ => t.NewValue.ToString()
        };
        return $"{t.Field} = {value},";
#else
        string oldString = t.OldValue is Enum oldEnum ? oldEnum.ToDescription().ToString() : t.OldValue?.ToString() ?? "?";
        string newString = t.NewValue is Enum newEnum ? newEnum.ToDescription().ToString() : t.NewValue?.ToString() ?? "?";
        string arrow = OperatingSystem.IsBrowser() ? "->" : "\u2192"; // Unicode arrow doesn't draw in browser build
        return $"{t.Field}: {oldString} {arrow} {newString}";
#endif
    }

    public ReactiveCommand<RxVoid, RxVoid> LoadPreset { get; }
    public ReactiveCommand<RxVoid, RxVoid> SaveNewPreset { get; }
    public ReactiveCommand<RxVoid, RxVoid> UpdatePreset { get; }
    public ReactiveCommand<RxVoid, RxVoid> RemovePreset { get; }

    public PresetsViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        LoadPreset = ReactiveCommand.Create(() =>
        {
            if (SelectedPreset is null) { return; }
            // By writing the flags like this, it will update all the reactive elements watching each
            // individual field.
            Main.Config.DeserializeFlags(SelectedPreset.Config.SerializeFlags());
        });

        SaveNewPreset = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetDialogOpen = true;
        });

        RemovePreset = ReactiveCommand.Create(
            () =>
            {
                if (SelectedPreset is null || !SelectedPreset.IsCustom) { return; }
                Main.RemovePresetViewModel.TargetName = SelectedPreset.Name;
                Main.RemovePresetDialogOpen = true;
            },
            this.WhenAnyValue(x => x.SelectedPreset, p => p?.IsCustom == true));

        UpdatePreset = ReactiveCommand.Create(
            () =>
            {
                if (SelectedPreset is null || !SelectedPreset.IsCustom) { return; }
                if (!Main.SaveNewPresetViewModel.HasSavedPresets) { return; }
                Main.UpdatePresetViewModel.TargetName = SelectedPreset.Name;
                Main.UpdatePresetDialogOpen = true;
            },
            this.WhenAnyValue(x => x.SelectedPreset, p => p?.IsCustom == true));

        AddBuiltInPresets();

        this.WhenActivated(OnActivate);
    }

    private void AddBuiltInPresets()
    {
        Presets.Add(new(VanillaPreset.Name, VanillaPreset.Preset, VanillaPreset.Description));
        Presets.Add(new(BeginnerPreset.Name, BeginnerPreset.Preset, BeginnerPreset.Description));
        Presets.Add(new(NormalPreset.Name, NormalPreset.Preset, NormalPreset.Description));
        Presets.Add(new(FullShufflePreset.Name, FullShufflePreset.Preset, FullShufflePreset.Description));
        Presets.Add(new(Upstarts2026Week1Preset.Name, Upstarts2026Week1Preset.Preset, Upstarts2026Week1Preset.Description));
        Presets.Add(new(Upstarts2026Week2Preset.Name, Upstarts2026Week2Preset.Preset, Upstarts2026Week2Preset.Description));
        Presets.Add(new(Upstarts2026Week3Preset.Name, Upstarts2026Week3Preset.Preset, Upstarts2026Week3Preset.Description));
        Presets.Add(new(Upstarts2026Week4Preset.Name, Upstarts2026Week4Preset.Preset, Upstarts2026Week4Preset.Description));
        Presets.Add(new(Upstarts2026Week5Preset.Name, Upstarts2026Week5Preset.Preset, Upstarts2026Week5Preset.Description));
        Presets.Add(new(Upstarts2026Week6Preset.Name, Upstarts2026Week6Preset.Preset, Upstarts2026Week6Preset.Description));
        Presets.Add(new(MaxRandoPreset.Name, MaxRandoPreset.Preset, MaxRandoPreset.Description));
        Presets.Add(new(StandardSwissPreset.Name, StandardSwissPreset.Preset, StandardSwissPreset.Description));
        Presets.Add(new(StandardPreset.Name, StandardPreset.Preset, StandardPreset.Description));
        Presets.Add(new(Sgl2025Preset.Name, Sgl2025Preset.Preset, Sgl2025Preset.Description));
        Presets.Add(new(Upstarts2025TournamentPreset.Name, Upstarts2025TournamentPreset.Preset, Upstarts2025TournamentPreset.Description));
        Presets.Add(new(MaxRando2025Preset.Name, MaxRando2025Preset.Preset, MaxRando2025Preset.Description));
        Presets.Add(new(RandomPercentPreset.Name, RandomPercentPreset.Preset, RandomPercentPreset.Description));
    }

    private void RefreshCustomPresets()
    {
        var previousName = hasRestoredSelection
            ? SelectedPreset?.Name
            : Main.RandomizerViewModel.SelectedPresetName;

        hasRestoredSelection = true;

        var customItems = (Main.SaveNewPresetViewModel?.SavedPresets ?? new ObservableCollection<CustomPreset>())
            .Select(c => new PresetItem(c.Preset, c.Config, isCustom: true))
            .Reverse()
            .ToList();

        Presets.Clear();
        foreach (var custom in customItems)
        {
            Presets.Add(custom);
        }
        AddBuiltInPresets();

        if (previousName is not null)
        {
            SelectedPreset = Presets.FirstOrDefault(p => p.Name == previousName);
        }
        if (SelectedPreset is null && Presets.Count > 0)
        {
            SelectedPreset = Presets[0];
        }
    }

    internal void OnActivate(MultipleDisposable disposables)
    {
        if (Main.SaveNewPresetViewModel is not null)
        {
            // Keep the custom preset list in sync as presets are saved or removed.
            var savedPresets = Main.SaveNewPresetViewModel.SavedPresets;
            NotifyCollectionChangedEventHandler handler = (_, _) => RefreshCustomPresets();
            savedPresets.CollectionChanged += handler;
            new ActionDisposable(() => savedPresets.CollectionChanged -= handler)
                .DisposeWith(disposables);
        }

        System.ComponentModel.PropertyChangedEventHandler configHandler = (_, _) => SetDiff();
        Main.Config.PropertyChanged += configHandler;
        new ActionDisposable(() => Main.Config.PropertyChanged -= configHandler)
            .DisposeWith(disposables);

        // Remember which preset was selected so it can be restored next time the app starts.
        // Ignore changes until the initial restore has happened, otherwise the ListBox's
        // auto-selection of the first item could overwrite the saved preset name.
        SubscribeExtensions.Subscribe(
            this.WhenAnyValue(x => x.SelectedPreset, p => p?.Name ?? ""),
            name =>
            {
                if (hasRestoredSelection)
                {
                    Main.RandomizerViewModel.SelectedPresetName = name == "" ? null : name;
                }
            })
            .DisposeWith(disposables);

        RefreshCustomPresets();
    }
}
