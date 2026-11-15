using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using CrossPlatformUI.Controls;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.ViewModels.Tabs;

/// <summary>
/// One search hit: the flag it names and the live control that edits it.
/// </summary>
public sealed class SearchResultRow
{
    public SearchResultRow(
        ConfigFlag flag,
        string propertyName,
        string caption,
        Control control)
    {
        Flag = flag;
        PropertyName = propertyName;
        Caption = caption;
        Control = control;
    }

    /// <summary>The config flag this row edits.</summary>
    public ConfigFlag Flag { get; }

    /// <summary>The config property behind the flag.</summary>
    public string PropertyName { get; }

    /// <summary>The caption the search matched.</summary>
    public string Caption { get; }

    /// <summary>The live control, bound to the current config.</summary>
    public Control Control { get; }
}

/// <summary>
/// Searches the flag controls by caption and shows every match as the live
/// control that edits it, the same way the preset diff shows the controls of
/// the flags it differs on.
/// </summary>
public class SearchViewModel : ReactiveObject
{
    /// <summary>How long to wait after the last keystroke before searching.</summary>
    private const int SearchDelayMilliseconds = 150;

    public MainViewModel Main { get; }

    /// <summary>The matches, newest search first.</summary>
    [JsonIgnore]
    public ObservableCollection<SearchResultRow> Results { get; } = new();

    private readonly DispatcherTimer searchTimer;

    private string query = string.Empty;

    /// <summary>The text typed into the search box.</summary>
    public string Query
    {
        get => query;
        set
        {
            value ??= string.Empty;
            if (query == value)
            {
                return;
            }

            var wasSearching = HasQuery;
            query = value;

            this.RaisePropertyChanged(nameof(Query));
            this.RaisePropertyChanged(nameof(HasQuery));

            if (!wasSearching && HasQuery)
            {
                SearchStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (wasSearching && !HasQuery)
            {
                SearchEnded?.Invoke(this, EventArgs.Empty);
            }

            ScheduleSearch();
        }
    }

    /// <summary>Whether the search box holds anything to search for.</summary>
    public bool HasQuery => !string.IsNullOrWhiteSpace(query);

    /// <summary>How many controls matched.</summary>
    public int ResultCount => Results.Count;

    public bool HasResults => Results.Count > 0;

    /// <summary>The line above the results explaining what was found.</summary>
    public string StatusText
    {
        get
        {
            if (!HasQuery)
            {
                return "Type to search the settings.";
            }

            if (Results.Count == 0)
            {
                return $"No setting matches \"{query.Trim()}\".";
            }

            var count = Results.Count;
            return count == 1
                ? $"1 setting matches \"{query.Trim()}\"."
                : $"{count} settings match \"{query.Trim()}\".";
        }
    }

    /// <summary>Raised when the search box goes from empty to holding a query.</summary>
    public event EventHandler? SearchStarted;

    /// <summary>Raised when the search box is emptied again.</summary>
    public event EventHandler? SearchEnded;

    /// <summary>Empties the search box.</summary>
    public ReactiveCommand<RxVoid, RxVoid> Clear { get; }

    public SearchViewModel(MainViewModel main)
    {
        Main = main;

        searchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(SearchDelayMilliseconds),
        };
        searchTimer.Tick += (_, _) =>
        {
            searchTimer.Stop();
            Refresh();
        };

        Clear = ReactiveCommand.Create(() => { Query = string.Empty; });
    }

    private void ScheduleSearch()
    {
        searchTimer.Stop();
        if (!HasQuery)
        {
            // Nothing to search for, so drop the old results right away.
            Results.Clear();
            RaiseResultChanged();
            return;
        }

        searchTimer.Start();
    }

    private void Refresh()
    {
        var matches = FlagSearchIndex.Search(query);

        Results.Clear();
        foreach (var match in matches)
        {
            var control = FlagControlFactory.Create(match.Flag);
            if (control is null)
            {
                continue;
            }

            // Explicit DataContext: inside the ItemsControl template the inherited
            // one would be the row, which the resolver can't turn into a config.
            // Main resolves (value.Config), so the control shows the live config.
            control.DataContext = Main;

            Results.Add(new SearchResultRow(
                match.Flag,
                match.PropertyName,
                match.Caption,
                control));
        }

        RaiseResultChanged();
    }

    private void RaiseResultChanged()
    {
        this.RaisePropertyChanged(nameof(ResultCount));
        this.RaisePropertyChanged(nameof(HasResults));
        this.RaisePropertyChanged(nameof(StatusText));
    }
}
