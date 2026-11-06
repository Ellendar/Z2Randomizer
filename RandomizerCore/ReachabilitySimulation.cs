using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SD.Tools.BCLExtensions.CollectionsRelated;
using Z2Randomizer.RandomizerCore.Diagnostics;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

/// <summary>Simulates travel and item collection across the four continents until neither can expand.</summary>
internal sealed class ReachabilitySimulation(Func<IEnumerable<Location>> itemLocations)
{
    private int accessibleMagicContainers;
    private int accessibleHeartContainers;

#if Z2R_DIAGNOSTICS
    private string? lastReachabilityFailure;
    private long consecutiveReachabilityFailures;
    private long reachabilityCalls, itemUpdateCalls;
#endif

    [Conditional("Z2R_DIAGNOSTICS")]
    internal void ResetDiagnostics()
    {
#if Z2R_DIAGNOSTICS
        lastReachabilityFailure = null;
        consecutiveReachabilityFailures = 0;
        reachabilityCalls = itemUpdateCalls = 0;
#endif
    }

    internal bool IsEverythingReachable(List<World> worlds, RandomizerProperties props,
        GettableItemSet gettableItems, ref int failures)
    {
        foreach (var world in worlds)
        {
            world.VisitStart();
        }

        var west = worlds[(int)Continent.WEST];
        var mountain = worlds[(int)Continent.DM];
        var east = worlds[(int)Continent.EAST];
        var maze = worlds[(int)Continent.MAZE];
        int dm = 0;
        int mi = 0;
        int wh = 0;
        int eh = 0;
        int reachableLocationsCount = 1;
        int previousReachableLocationsCount = 0;
        int gettableItemsCount = 1;
        int previousGettableItemsCount = 0;

#if Z2R_DIAGNOSTICS
        long reachCall = ++reachabilityCalls;
        long reachPasses = 0;
#endif
        while (reachableLocationsCount != previousReachableLocationsCount || gettableItemsCount != previousGettableItemsCount)
        {
#if Z2R_DIAGNOSTICS
            reachPasses++;
            if (Diag.IsSample(reachCall) || reachPasses >= 1000)
                Diag.Retry("reach.loop", reachPasses, $"reachable={reachableLocationsCount} previous={previousReachableLocationsCount} items={gettableItemsCount} previousItems={previousGettableItemsCount}");
#endif
            previousReachableLocationsCount = reachableLocationsCount;
            previousGettableItemsCount = gettableItemsCount;
            gettableItemsCount = UpdateGettableItems(props, gettableItems);
            IReadOnlySet<RequirementType> requireables = GetRequireables(props, gettableItems);
            foreach (var world in worlds) world.UpdateVisit(requireables);

            foreach (World world in worlds)
            {
                if (world.raft != null && world.raft.Reachable && gettableItems[Collectable.RAFT])
                {
                    worlds.ForEach(i => i.VisitRaft());
                }

                if (world.bridge != null && world.bridge.Reachable)
                {
                    worlds.ForEach(i => i.VisitBridge());
                }

                if (world.cave1 != null && world.cave1.Reachable)
                {
                    worlds.ForEach(i => i.VisitCave1());
                }

                if (world.cave2 != null && world.cave2.Reachable)
                {
                    worlds.ForEach(i => i.VisitCave2());
                }
            }
            gettableItemsCount = UpdateGettableItems(props, gettableItems);

            // Revisit terrain with the items found after crossing between continents.
            requireables = GetRequireables(props, gettableItems);
            foreach (var world in worlds) world.UpdateVisit(requireables);

            gettableItemsCount = UpdateGettableItems(props, gettableItems);

            reachableLocationsCount = 0;
            dm = 0;
            mi = 0;
            wh = 0;
            eh = 0;

            wh = west.AllLocations.Count(i => i.Reachable);
            eh = east.AllLocations.Count(i => i.Reachable);
            dm = mountain.AllLocations.Count(i => i.Reachable);
            mi = maze.AllLocations.Count(i => i.Reachable);
            reachableLocationsCount = wh + eh + dm + mi;
        }

        if (gettableItems.TrackedItems.Any(item => !gettableItems[item]))
        {
            failures++;
            EmitReachabilityFailure(worlds, props, gettableItems, failures, "items", reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }
        if (accessibleMagicContainers != props.MaxMagicContainers)
        {
            failures++;
            EmitReachabilityFailure(worlds, props, gettableItems, failures, "magic", reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }
        if (accessibleHeartContainers < props.MaxHearts)
        {
            failures++;
            EmitReachabilityFailure(worlds, props, gettableItems, failures, "hearts", reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }
        if (accessibleHeartContainers > props.MaxHearts)
        {
            throw new Exception("More hearts found than should exist in the seed.");
        }

        bool requiredLocationsReachable =
            CanGet(west.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(east.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(mountain.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(maze.RequiredLocations(props.HiddenPalace, props.HiddenKasuto));
        if (!requiredLocationsReachable)
        {
            failures++;
            EmitReachabilityFailure(worlds, props, gettableItems, failures, "locations", reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }

#if Z2R_DIAGNOSTICS
        lastReachabilityFailure = null;
        consecutiveReachabilityFailures = 0;
#endif
        Diag.Trace("reach", $"event=success reachable={reachableLocationsCount}/"
            + $"{west.AllLocations.Count + east.AllLocations.Count + mountain.AllLocations.Count + maze.AllLocations.Count}");
        return true;
    }

    /// <summary>
    /// Emits why <see cref="IsEverythingReachable"/> failed as a failure reason plus
    /// the routing snapshot that used to sit behind a reachability threshold.
    /// Compiled out (call site and all LINQ arguments removed) unless
    /// <c>Z2R_DIAGNOSTICS</c> is defined.
    /// </summary>
    [Conditional("Z2R_DIAGNOSTICS")]
    private void EmitReachabilityFailure(IReadOnlyList<World> worlds, RandomizerProperties props, GettableItemSet gettableItems,
        int failures, string failureClass, int reachableLocations, int wh, int eh, int dm, int mi)
    {
#if Z2R_DIAGNOSTICS
        if (lastReachabilityFailure != failureClass)
        {
            lastReachabilityFailure = failureClass;
            consecutiveReachabilityFailures = 0;
        }
        if (!Diag.IsSample(++consecutiveReachabilityFailures)) return;
        Diag.Trace("reach", $"failed class={failureClass} consecutive={consecutiveReachabilityFailures} attempt={failures} "
            + $"reachable={reachableLocations} "
            + $"wh={wh}/{worlds[(int)Continent.WEST].AllLocations.Count} "
            + $"eh={eh}/{worlds[(int)Continent.EAST].AllLocations.Count} "
            + $"dm={dm}/{worlds[(int)Continent.DM].AllLocations.Count} "
            + $"mi={mi}/{worlds[(int)Continent.MAZE].AllLocations.Count} "
            + $"hearts={accessibleHeartContainers}/{props.MaxHearts} magic={accessibleMagicContainers}/{props.MaxMagicContainers}");

        foreach (Collectable item in gettableItems.TrackedItems.Where(i => !gettableItems[i] && !i.IsMinorItem()))
        {
            Diag.Trace("reach", $"ungotten {item}");
        }

        foreach (Location location in itemLocations()
            .Where(l => l.GetAllCollectables().Any(i => !i.IsMinorItem() && !gettableItems[i])))
        {
            string palace = location.Palace is null ? string.Empty : $"({location.Palace.Number})";
            Diag.Trace("reach", $"held-by {location.Name}{palace} "
                + $"[{string.Join(", ", location.GetAllCollectables().Where(i => !i.IsMinorItem() && !gettableItems[i]))}]");
        }

        foreach (Location location in itemLocations().Where(l => !l.Reachable))
        {
            Diag.Trace("reach", $"unreachable-item-location {location.Name}");
        }
        if (failureClass == "locations") EmitUnreachableRequiredLocations(worlds, props);
#endif
    }

    [Conditional("Z2R_DIAGNOSTICS")]
    private static void EmitUnreachableRequiredLocations(IReadOnlyList<World> worlds, RandomizerProperties props)
    {
        IEnumerable<Location> requiredLocations =
        [
            .. worlds[(int)Continent.WEST].RequiredLocations(props.HiddenPalace, props.HiddenKasuto),
            .. worlds[(int)Continent.EAST].RequiredLocations(props.HiddenPalace, props.HiddenKasuto),
            .. worlds[(int)Continent.MAZE].RequiredLocations(props.HiddenPalace, props.HiddenKasuto),
            .. worlds[(int)Continent.DM].RequiredLocations(props.HiddenPalace, props.HiddenKasuto),
        ];

        foreach (Location location in requiredLocations.Where(l => !l.Reachable))
        {
            Diag.Trace("reach", $"unreachable-required {location.GetDebuggerDisplay()}");
        }
    }

    private static bool CanGet(IEnumerable<Location> locations)
    {
        return locations.All(i => i.Reachable);
    }

    /// <summary>
    /// Marks every item reachable at currently reachable locations, repeating
    /// until no new items become available.
    /// </summary>
    /// <returns>The number of items obtained on the final pass.</returns>
    private int UpdateGettableItems(RandomizerProperties props, GettableItemSet gettableItems)
    {
        IReadOnlySet<RequirementType> requireables;
        accessibleMagicContainers = props.StartMagicContainers;
        accessibleHeartContainers = props.StartHearts;
        List<Collectable> gottenItems = [], lastIterationGottenItems = [];
        List<Location> reachableLocations = itemLocations().Where(i => i.Reachable).ToList();
        List<Location> heartContainerGetLocations = [], magicContainerGetLocations = [];
        bool stalled = false;
        //No more delayed evaluation because too many locations could give you containers or require containers.
        //Just iterate until we find as many items as possible
#if Z2R_DIAGNOSTICS
        long itemCall = ++itemUpdateCalls;
        long itemPasses = 0;
#endif
        do
        {
#if Z2R_DIAGNOSTICS
            itemPasses++;
            if (Diag.IsSample(itemCall) || itemPasses >= 1000)
                Diag.Retry("items.loop", itemPasses, $"event=begin previousItems={lastIterationGottenItems.Count}");
#endif
            accessibleHeartContainers = props.StartHearts + heartContainerGetLocations.Count;
            accessibleMagicContainers = props.StartMagicContainers + magicContainerGetLocations.Count;
            requireables = GetRequireables(props, gettableItems);
            gottenItems = [];
            foreach (Location location in reachableLocations)
            {
                IReadOnlyList<Collectable> itemsAtLocation = gettableItems.GetGettableItemsAt(props, location, requireables);

                foreach (Collectable item in itemsAtLocation)
                {
                    gettableItems[item] = true;
                    gottenItems.Add(item);

                    if (item == Collectable.HEART_CONTAINER && !heartContainerGetLocations.Contains(location))
                    {
                        heartContainerGetLocations.Add(location);
                    }
                    if (item == Collectable.MAGIC_CONTAINER && !magicContainerGetLocations.Contains(location))
                    {
                        magicContainerGetLocations.Add(location);
                    }
                }
            }
            stalled = lastIterationGottenItems.SequenceEqual(gottenItems);
            lastIterationGottenItems = new(gottenItems);
        } while (!stalled);

        accessibleHeartContainers = props.StartHearts + heartContainerGetLocations.Count;
        accessibleMagicContainers = props.StartMagicContainers + magicContainerGetLocations.Count;
        return gottenItems.Count;
    }

    internal IReadOnlySet<RequirementType> GetRequireables(RandomizerProperties props, GettableItemSet gettableItems)
    {
        HashSet<RequirementType> requireables = [];

        foreach (Collectable item in gettableItems.TrackedItems)
        {
            RequirementType? requirement = item.AsRequirement();
            if (requirement != null
                && (gettableItems[item] || props.StartsWithCollectable(item)))
            {
                requireables.Add(requirement.Value);
            }
        }

        //Mario always runs fast and jumps high, so assume he always has access to jump and dash
        //(even though they aren't collectable)
        //Also mario implicitly has upstab as a requirementType because it means "can break blocks upward"
        //but not in itemget because he can still get the upstab upgrade, which lets him POW
        if (props.MarioMode)
        {
            requireables.Add(RequirementType.JUMP);
            requireables.Add(RequirementType.DASH);
            requireables.Add(RequirementType.UPSTAB);
        }

        requireables.AddRange(RequirementTypeExtensions.UpToXContainers(accessibleMagicContainers));

        // Now there are KEY requirements in palace rooms, and that responsibility is handled by
        // AccessRequirements in the old mode - we just add KEY here
        if (props.KeyLogic == KeyLogicOption.FAIRY_OR_MAGICKEY)
        {
            requireables.Add(RequirementType.KEY);
        }

        return requireables;
    }
}
