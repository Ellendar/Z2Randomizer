using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Controls.Generated;
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
    public bool IsCustom { get; }
    public RandomizerConfiguration Config { get; }
}

public sealed class PresetDiffRow : ReactiveObject
{
    public PresetDiffRow(string field, Control? control, string value)
    {
        Field = field;
        Control = control;
        this.value = value;
    }

    public string Field { get; }

    [JsonIgnore]
    public Control? Control { get; }

    private string value;
    public string Value
    {
        get => value;
        set
        {
            if (this.value == value) { return; }
            this.value = value;
            this.RaisePropertyChanged(nameof(Value));
        }
    }

    private bool isRemoving;
    public bool IsRemoving
    {
        get => isRemoving;
        set
        {
            if (isRemoving == value) { return; }
            isRemoving = value;
            this.RaisePropertyChanged(nameof(IsRemoving));
        }
    }
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
            suppressDiffAnimations = true;
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

    private bool hasDiff;
    public bool HasDiff { get => hasDiff; set => this.RaiseAndSetIfChanged(ref hasDiff, value); }

    [JsonIgnore]
    public ObservableCollection<PresetDiffRow> DiffRows { get; } = new();

    // A generated control is expensive to build (resource lookups, item lists,
    // theme resolution) and a field always maps to the same control type, so
    // build each field's control once and reuse it across rebuilds. Reuse is
    // safe: a field appears at most once per diff, rows are the only consumer,
    // and a detached control holds no config subscription.
    private readonly Dictionary<string, Control> controlCache = new();

    // Keep in sync with the Opacity DoubleTransition duration in PresetsView.axaml.
    private const int RemovalFadeMilliseconds = 250;

    // Config changes arrive one property at a time
    // (Load Preset deserializes every flag), so coalesce them into a single reconcile.
    private readonly DispatcherTimer diffTimer;
    private readonly Dictionary<PresetDiffRow, DispatcherTimer> pendingRemovals = new();

    // Set when the selected preset changes so the reconcile that follows swaps
    // the whole list at once instead of fading and collapsing every old row.
    private bool suppressDiffAnimations;

    private bool isDiffExpanded;
    public bool IsDiffExpanded
    {
        get => isDiffExpanded;
        set
        {
            this.RaiseAndSetIfChanged(ref isDiffExpanded, value);
            SetDiff();
        }
    }

    public void SetDiff()
    {
        diffTimer.Stop();
        diffTimer.Start();
    }

    private void ReconcileDiff()
    {
        bool suppressAnimations = suppressDiffAnimations;
        suppressDiffAnimations = false;

        if (SelectedPreset is not { } other)
        {
            ClearRows();
            HasDiff = false;
            return;
        }

        var d = Main.Config.Diff(other.Config);

        // Collapsed: drop rows immediately, there is nothing visible to fade.
        if (!IsDiffExpanded)
        {
            ClearRows();
            HasDiff = d.Count > 0;
            return;
        }

        // Preset switch: rebuild the whole list at once instead of fading and
        // collapsing every old row.
        if (suppressAnimations)
        {
            ClearRows();
            if (d.Count == 0)
            {
                HasDiff = false;
                return;
            }
        }

        var positionOf = new Dictionary<string, int>(d.Count);
        for (int i = 0; i < d.Count; i++)
        {
            positionOf[d[i].Field] = i;
        }

        var rowsByField = new Dictionary<string, PresetDiffRow>();
        foreach (var row in DiffRows)
        {
            rowsByField[row.Field] = row;
        }

        string arrow = OperatingSystem.IsBrowser() ? "->" : "\u2192"; // Unicode arrow doesn't draw in browser build
        var liveRows = new HashSet<PresetDiffRow>();

        for (int i = 0; i < d.Count; i++)
        {
            var t = d[i];
            if (rowsByField.TryGetValue(t.Field, out var row))
            {
                // Still different: revive a fading row and refresh its value in place.
                CancelRemoval(row);
                row.IsRemoving = false;
                row.Value = FormatRowValue(arrow, row.Control, t);
                liveRows.Add(row);
            }
            else
            {
                var control = GetControl(t.Field);
                row = new PresetDiffRow(t.Field, control, FormatRowValue(arrow, control, t));
                liveRows.Add(row);
                DiffRows.Insert(InsertionIndex(positionOf, i), row);
            }
        }

        // Anything no longer different fades out, then is dropped once invisible.
        foreach (var row in DiffRows)
        {
            if (!liveRows.Contains(row))
            {
                StartRemoval(row);
            }
        }

        // Keep the expander visible while the last rows are still fading.
        HasDiff = d.Count > 0 || DiffRows.Count > 0;
    }

    private int InsertionIndex(Dictionary<string, int> positionOf, int position)
    {
        for (int i = 0; i < DiffRows.Count; i++)
        {
            if (positionOf.TryGetValue(DiffRows[i].Field, out var p) && p > position)
            {
                return i;
            }
        }
        return DiffRows.Count;
    }

    private void StartRemoval(PresetDiffRow row)
    {
        if (pendingRemovals.ContainsKey(row)) { return; }

        row.IsRemoving = true;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(RemovalFadeMilliseconds) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            pendingRemovals.Remove(row);
            DiffRows.Remove(row);
            if (DiffRows.Count == 0)
            {
                HasDiff = false;
            }
        };
        pendingRemovals[row] = timer;
        timer.Start();
    }

    private void CancelRemoval(PresetDiffRow row)
    {
        if (pendingRemovals.Remove(row, out var timer))
        {
            timer.Stop();
        }
    }

    private void ClearRows()
    {
        foreach (var timer in pendingRemovals.Values)
        {
            timer.Stop();
        }
        pendingRemovals.Clear();
        DiffRows.Clear();
    }

    private static string FormatRowValue(string arrow, Control? control, (string Field, object? OldValue, object? NewValue) t)
    {
        return control is not null
            ? $"{arrow}    {FormatDiffValue(t.NewValue)}"
            : FormatDiffLine(t);
    }

    private Control? GetControl(string field)
    {
        if (controlCache.TryGetValue(field, out var cached))
        {
            return cached;
        }

        var control = FlagControlFactory.Create(field);
        if (control is null)
        {
            return null;
        }

        // Explicit DataContext: inside the ItemsControl template the inherited
        // one would be the row, which the resolver can't turn into a config.
        // Main resolves (value.Config), so the control shows the live config.
        control.DataContext = Main;
        controlCache.Add(field, control);
        return control;
    }

    private static string FormatDiffValue(object? value)
    {
        return value switch
        {
            Enum e => e.ToDescription().ToString(),
            true => "Enabled",
            false => "Disabled",
            null => "Random",
            _ => value.ToString() ?? "?"
        };
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
        string oldString = FormatDiffValue(t.OldValue);
        string newString = FormatDiffValue(t.NewValue);
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

        diffTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        diffTimer.Tick += (_, _) =>
        {
            diffTimer.Stop();
            ReconcileDiff();
        };

        LoadPreset = ReactiveCommand.Create(() =>
        {
            if (SelectedPreset is null) { return; }
            // Applying a preset makes every diff row disappear at once, so swap it
            // instantly instead of fading the whole list out.
            suppressDiffAnimations = true;
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
