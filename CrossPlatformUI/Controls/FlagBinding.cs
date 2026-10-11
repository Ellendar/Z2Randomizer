using System;
using System.ComponentModel;
using Avalonia.Controls;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.Controls;

/// <summary>
/// The half of a flag-bound control that does not depend on which kind of control
/// it is: resolving the config behind a DataContext, keeping control and config in
/// sync in both directions, following the flag's <c>Included</c> observable into
/// <c>IsEnabled</c>, and keeping the <c>DifficultyOnly</c> style class in sync with
/// <c>ShareSeedAcrossDifficulty</c>.
/// </summary>
internal sealed class FlagBinding
{
    private const string DifficultyOnlyClassName = "DifficultyOnly";

    private readonly Control _owner;
    private readonly Action _pullFromConfig;
    private readonly Action _pushToConfig;

    private FlagEntry? _entry;
    private RandomizerConfiguration? _config;
    private PropertyChangedEventHandler? _configPropertyChanged;
    private IDisposable? _includedSubscription;
    private bool _syncing;

    public FlagBinding(
        Control owner,
        Action pullFromConfig,
        Action pushToConfig)
    {
        _owner = owner;
        _pullFromConfig = pullFromConfig;
        _pushToConfig = pushToConfig;
    }

    /// <summary>The flag's metadata from the registry, or null before <see cref="SetEntry"/>.</summary>
    public FlagEntry? Entry => _entry;

    /// <summary>The config currently bound to, if any.</summary>
    public RandomizerConfiguration? Config => _config;

    /// <summary>Takes the flag's metadata from the registry.</summary>
    public void SetEntry(ConfigFlag? flag)
    {
        _entry = flag is { } value ? FlagRegistry.Get(value) : null;

        UpdateDifficultyOnlyClass();
    }

    public void Rebind(object? dataContext)
    {
        Unbind();

        // This flag only participates while its Included guard is on. The observable
        // is StartWith-ed, so the initial value lands synchronously and the control
        // is never briefly enabled or stale.
        if (_entry?.Included is { } included)
            _includedSubscription = included(dataContext)
                ?.Subscribe(onNext: value => _owner.IsEnabled = value);

        var config = RandomizerConfigResolver.Resolve(dataContext);
        if (config is null)
        {
            UpdateDifficultyOnlyClass();
            return;
        }

        _config = config;
        _configPropertyChanged = OnConfigPropertyChanged;
        _config.PropertyChanged += _configPropertyChanged;

        Pull();
    }

    public void Unbind()
    {
        _includedSubscription?.Dispose();
        _includedSubscription = null;

        if (_config is not null && _configPropertyChanged is not null)
        {
            _config.PropertyChanged -= _configPropertyChanged;
        }

        _config = null;
        _configPropertyChanged = null;

        // No config means no answer for ShareSeedAcrossDifficulty, so the class comes
        // off rather than staying stale from a previous DataContext.
        UpdateDifficultyOnlyClass();
    }

    /// <summary>
    /// Reads the config into the control. The control's own change handler must stay
    /// quiet while this runs, which is what the guard here is for.
    /// </summary>
    public void Pull()
    {
        if (_config is null)
            return;

        _syncing = true;
        try
        {
            _pullFromConfig();
        }
        finally
        {
            _syncing = false;
        }

        UpdateDifficultyOnlyClass();
    }

    /// <summary>Writes the control's value to the config, unless the value came from the config.</summary>
    public void Push()
    {
        if (_syncing || _config is null || _entry is null)
            return;

        _pushToConfig();
    }

    /// <summary>
    /// Runs a kind-specific write-back under the same "ignore config-driven changes"
    /// guard, for controls that have more than one value to push (a Min/Max pair).
    /// </summary>
    public void Push(Action pushToConfig)
    {
        if (_syncing || _config is null || _entry is null)
            return;

        pushToConfig();
    }

    private void OnConfigPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (_entry is null)
            return;

        if (_entry.IsDifficultyOnly &&
            e.PropertyName ==
                nameof(RandomizerConfiguration.ShareSeedAcrossDifficulty))
        {
            UpdateDifficultyOnlyClass();
            return;
        }

        // A Min/Max pair watches both of its config properties.
        if (e.PropertyName != _entry.PropertyName &&
            e.PropertyName != _entry.PartnerPropertyName)
        {
            return;
        }

        Pull();
    }

    /// <summary>
    /// Difficulty-only flags are shown read-only while the seed is shared across
    /// difficulties: App.axaml's <c>.DifficultyOnly</c> selectors do the graying,
    /// and this control only has to keep the class in sync, so no view has to
    /// repeat the binding.
    /// </summary>
    private void UpdateDifficultyOnlyClass()
    {
        var difficultyOnly = _entry is { IsDifficultyOnly: true } &&
            (_config?.ShareSeedAcrossDifficulty ?? false);

        if (difficultyOnly == _owner.Classes.Contains(DifficultyOnlyClassName))
            return;

        if (difficultyOnly)
            _owner.Classes.Add(DifficultyOnlyClassName);
        else
            _owner.Classes.Remove(DifficultyOnlyClassName);
    }
}
