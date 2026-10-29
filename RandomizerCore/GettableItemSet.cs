using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Tracks the collectables the logic simulation has obtained, plus cached
/// results of expensive reachability queries.
///
/// Configure a builder with <see cref="Builder.Excluding"/> and <see cref="Builder.StartingWith"/>, then call <see cref="Builder.Build"/>.
/// <see cref="Reset"/> clears both the obtained items and all caches, then
/// re-seeds the obtained set with the configured starting items.
/// </summary>
public sealed class GettableItemSet
{
    private static readonly FrozenSet<Collectable> INITIAL_TRACKED_ITEMS = Enum.GetValues<Collectable>()
        .Where(item => item.IsItemGetItem()).ToFrozenSet();

    private readonly FrozenSet<Collectable> trackedItems;
    private readonly HashSet<Collectable> gettableItems = [];
    private readonly Dictionary<Palace, PalaceCacheEntry> palaceCache = [];
    private readonly FrozenSet<Collectable> startingItems;

    private GettableItemSet(FrozenSet<Collectable> trackedItems, FrozenSet<Collectable> startingItems)
    {
        this.trackedItems = trackedItems;
        this.startingItems = startingItems;
        Reset();
    }

    /// <summary>Configures the immutable item sets before creating a simulation.</summary>
    public sealed class Builder
    {
        private FrozenSet<Collectable> trackedItems = INITIAL_TRACKED_ITEMS;
        private FrozenSet<Collectable> startingItems = FrozenSet<Collectable>.Empty;

        /// <summary>Excludes items from tracking (e.g. the spell a setting replaces).</summary>
        public Builder Excluding(params IEnumerable<Collectable> excludedItems)
        {
            ArgumentNullException.ThrowIfNull(excludedItems);
            trackedItems = trackedItems.Except(excludedItems).ToFrozenSet();
            return this;
        }

        /// <summary>Copies the starting items. Untracked items are filtered when building.</summary>
        public Builder StartingWith(IEnumerable<Collectable> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            startingItems = items.ToFrozenSet();
            return this;
        }

        /// <summary>Creates an independent simulation with the configured starting items.</summary>
        public GettableItemSet Build() => new(trackedItems, startingItems.Where(trackedItems.Contains).ToFrozenSet());
    }

    /// <summary>Every item this set tracks, whether or not it has been gotten.</summary>
    public IReadOnlyCollection<Collectable> TrackedItems => trackedItems;

    public bool Tracks(Collectable item) => trackedItems.Contains(item);

    /// <summary>Whether the logic simulation has already obtained this item.</summary>
    public bool this[Collectable item]
    {
        get => gettableItems.Contains(item);
        set
        {
            if (value)
            {
                gettableItems.Add(item);
            }
            else
            {
                throw new InvalidOperationException("Obtained items cannot be removed. Use Reset() to start over.");
            }
        }
    }

    /// <summary>
    /// Clears everything acquired so far and every cached reachability result,
    /// then re-applies the items selected by <see cref="Builder.StartingWith"/>.
    /// </summary>
    public void Reset()
    {
        gettableItems.Clear();
        palaceCache.Clear();

        gettableItems.UnionWith(startingItems);
    }

    /// <summary>
    /// Gets the items obtainable at <paramref name="location"/> with
    /// <paramref name="requireables"/>. Solved palaces reuse their cached items;
    /// incomplete palaces are retried only when the requireables count grows.
    /// </summary>
    /// <remarks>
    /// Requireables must only grow between resets. Call <see cref="Reset"/> before
    /// removing or replacing requireables, or changing palace items, topology, or
    /// room requirements.
    /// </remarks>
    public IReadOnlyList<Collectable> GetGettableItemsAt(
        Location location,
        IReadOnlySet<RequirementType> requireables)
    {
        if (location.Palace is not Palace palace)
        {
            return location.GetGettableItems(requireables);
        }

        List<Collectable> palaceItems = palace.GetAllCollectables();
        if (!palaceItems.Any(item => !item.IsMinorItem()))
        {
            return [];
        }

        if (palaceCache.TryGetValue(palace, out PalaceCacheEntry? cached)
            && (cached.Solved || requireables.Count <= cached.Requireables.Count))
        {
            return cached.GettableItems;
        }

        List<Collectable> gettableItemsAtPalace = palace.GetGettableItems(requireables, false);
        palaceCache[palace] = new(
            gettableItemsAtPalace.Count == palace.ItemRooms.Count,
            requireables.ToFrozenSet(),
            gettableItemsAtPalace);

        return gettableItemsAtPalace;
    }

    private sealed record PalaceCacheEntry(
        bool Solved,
        FrozenSet<RequirementType> Requireables,
        List<Collectable> GettableItems);
}
