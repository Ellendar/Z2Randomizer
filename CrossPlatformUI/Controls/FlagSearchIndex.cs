using System;
using System.Collections.Generic;
using System.Linq;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.Controls;

/// <summary>
/// One flag control the search box can find: the flag it addresses, the caption
/// it shows, and the text a query is matched against (the caption, the tooltip,
/// the combo options, and any synonym expansion).
/// </summary>
/// <param name="Flag">The config flag the control edits.</param>
/// <param name="PropertyName">The config property behind the flag.</param>
/// <param name="Caption">The text shown on the control.</param>
/// <param name="SearchText">The caption, followed by its tooltip, options, and
/// synonyms.</param>
internal sealed record FlagSearchEntry(
    ConfigFlag Flag,
    string PropertyName,
    string Caption,
    string SearchText)
{
    /// <summary>
    /// Whether every word of the query starts a word in the caption, the tooltip,
    /// an option, or a synonym expansion. Starting, not merely appearing: "lag"
    /// must not match "flag", while "recon" still finds "Reconstructed".
    /// </summary>
    public bool Matches(string[] words)
    {
        foreach (var word in words)
        {
            if (!StartsWord(SearchText, word))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Whether <paramref name="word"/> starts a word in <paramref name="text"/>.</summary>
    internal static bool StartsWord(string text, string word)
    {
        for (var start = 0; ;)
        {
            var index = text.IndexOf(word, start, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return false;
            }

            if (index == 0 || !char.IsLetterOrDigit(text[index - 1]))
            {
                return true;
            }

            start = index + 1;
        }
    }
}

/// <summary>
/// Every flag that has a generated control, with the caption its control shows and
/// the text a query matches: that caption, the flag's tooltip, its combo options, and
/// any synonym group the text touches.
/// </summary>
/// <remarks>
/// Built from the same registry the shared controls read, so the index covers
/// exactly the controls the views contain and there is no second list to keep
/// in sync. The registry is the union of every config type's control fields, so
/// a flag that no view shows would still be searchable; today every entry in
/// it is used by a view.
/// </remarks>
internal static class FlagSearchIndex
{
    /// <summary>
    /// Groups of words that mean the same thing to players. If any word from a
    /// group appears in the searchable text, all forms are added so either can be
    /// used to find the setting.
    /// </summary>
    private static readonly string[][] SynonymGroups =
    [
        ["gem", "gems", "crystal", "crystals"],
    ];

    private static readonly Lazy<FlagSearchEntry[]> entries = new(BuildEntries);

    /// <summary>All searchable flags, ordered by caption.</summary>
    public static IReadOnlyList<FlagSearchEntry> Entries => entries.Value;

    /// <summary>The entries whose text matches the query.</summary>
    public static List<FlagSearchEntry> Search(string query)
    {
        var results = new List<FlagSearchEntry>();
        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        var words = query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries.Value)
        {
            if (entry.Matches(words))
            {
                results.Add(entry);
            }
        }

        return results;
    }

    private static FlagSearchEntry[] BuildEntries()
    {
        var result = new List<FlagSearchEntry>();

        foreach (var flag in Enum.GetValues<ConfigFlag>())
        {
            var entry = FlagRegistry.Get(flag);
            if (entry is null)
            {
                continue;
            }

            var caption = entry.Label() ?? string.Empty;

            // A Min/Max pair's caption ("Heart Containers") names the group its
            // box hint ("Start Min") belongs to, so the two read as one caption.
            var groupCaption = entry.Caption?.Invoke() ?? string.Empty;
            if (!string.IsNullOrEmpty(groupCaption) &&
                !caption.Contains(groupCaption, StringComparison.OrdinalIgnoreCase))
            {
                caption = $"{groupCaption} {caption}";
            }

            var searchText = caption;

            // The tooltip spells the setting out in the players' words ("the number
            // of gems you start the game with"), so it is searchable even though the
            // result row never shows it.
            var tooltip = entry.ToolTip();
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                searchText = $"{searchText} {tooltip}";
            }

            // A combo's options are their own words: searching "Reconstructed"
            // should find the palace style, even though no caption says it.
            if (entry.Items is not null)
            {
                foreach (var item in entry.Items())
                {
                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        searchText = $"{searchText} {item.Description}";
                    }

                    if (!string.IsNullOrWhiteSpace(item.Info))
                    {
                        searchText = $"{searchText} {item.Info}";
                    }
                }
            }

            // Expand synonyms for any group that appears anywhere in the searchable text.
            foreach (var group in SynonymGroups)
            {
                var matched = false;
                foreach (var word in group)
                {
                    if (FlagSearchEntry.StartsWord(searchText, word))
                    {
                        matched = true;
                        break;
                    }
                }

                if (matched)
                {
                    foreach (var word in group)
                    {
                        if (!FlagSearchEntry.StartsWord(searchText, word))
                        {
                            searchText = $"{searchText} {word}";
                        }
                    }
                }
            }

            result.Add(new FlagSearchEntry(
                flag,
                entry.PropertyName,
                caption,
                searchText));
        }

        // Results read better alphabetically, and the flag name breaks ties so
        // the order does not depend on the registry's.
        result.Sort((a, b) =>
        {
            var byCaption = string.Compare(
                a.Caption, b.Caption, StringComparison.OrdinalIgnoreCase);
            return byCaption != 0
                ? byCaption
                : string.Compare(a.PropertyName, b.PropertyName, StringComparison.Ordinal);
        });

        return result.ToArray();
    }
}
