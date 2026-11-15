using System;
using System.Linq;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    /// <summary>Places within-continent passages using this call's quota; reports termination and successful feature placement.</summary>
    /// <remarks>
    /// Clears requested feature locations before searching. A blocked encounter entrance
    /// ends the search before painting that span. Earlier crossings remain on the map.
    /// Callers decide which features are required and validate final routing.
    /// </remarks>
    private protected RegionCrossingResult PlaceRegionCrossings(RegionCrossingOptions options)
    {
        ValidateCrossingDeadZone(options.DeadZone);

        const CrossingFeatures supportedFeatures = CrossingFeatures.Saria | CrossingFeatures.LongBridge | CrossingFeatures.DaruniaEncounters;
        var requestedFeatures = options.Features & supportedFeatures;
        var placedFeatures = CrossingFeatures.None;
        var unavailableFeatures = CrossingFeatures.None;
        int placedCrossings = 0;

        ResetCrossingFeatureLocations(requestedFeatures);
        var search = new RegionCrossingSearch(this, options);
        var crossingTerrains = search.CrossingTerrains;
        var simpleCrossing = new SimpleCrossingPainter(this, options, crossingTerrains);
        int remainingCrossings = options.MaximumCrossings;

        for (int attempt = 0; remainingCrossings > 0 && attempt < search.MaximumAttempts; attempt++)
        {
            var pendingFeatures = requestedFeatures & ~(placedFeatures | unavailableFeatures);
            var feature = NextCrossingFeature(pendingFeatures);
            var candidate = search.Sample();
            if (search.TryMeasure(candidate, feature == CrossingFeatures.Saria, out var span))
            {
                if (feature == CrossingFeatures.None && !simpleCrossing.TryPaint(span, RNG))
                {
                    search.Discard(candidate);
                    continue;
                }

                if (feature == CrossingFeatures.None)
                {
                    search.RecordConnection(span);
                    placedCrossings++;
                }
                else if (feature == CrossingFeatures.Saria)
                {
                    search.RecordConnection(span);
                    PlaceSariaCrossing(span, crossingTerrains);
                    placedFeatures |= feature;
                    placedCrossings++;
                }
                else
                {
                    var placement = PlaceEncounterCrossing(span, crossingTerrains, feature, options.ShapesMountainPassages);
                    if (placement == EncounterCrossingPlacement.TooNarrow)
                    {
                        search.Discard(candidate);
                        continue;
                    }

                    search.RecordConnection(span);
                    switch (placement)
                    {
                        case EncounterCrossingPlacement.EntranceBlocked:
                            var blocked = feature == CrossingFeatures.LongBridge
                                ? RegionCrossingTermination.LongBridgeEntranceBlocked
                                : RegionCrossingTermination.DaruniaEntranceBlocked;
                            return Finish(blocked);

                        case EncounterCrossingPlacement.Placed:
                            placedFeatures |= feature;
                            placedCrossings++;
                            break;

                        case EncounterCrossingPlacement.Unavailable:
                            unavailableFeatures |= feature;
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown encounter crossing placement: {placement}.");
                    }
                }

                remainingCrossings--;
            }

            search.Discard(candidate);
        }

        var termination = (remainingCrossings, search.RemainingCandidates) switch
        {
            (<= 0, _) => RegionCrossingTermination.CrossingLimitReached,
            (_, 0) => RegionCrossingTermination.CandidatePoolExhausted,
            _ => RegionCrossingTermination.AttemptBudgetExhausted
        };
        return Finish(termination);

        RegionCrossingResult Finish(RegionCrossingTermination termination)
        {
            var result = new RegionCrossingResult(termination, placedCrossings, requestedFeatures, placedFeatures, unavailableFeatures);
            logger.Trace($"Region crossings: {result}; pending: {result.PendingFeatures}");
            return result;
        }
    }

    private static CrossingFeatures NextCrossingFeature(CrossingFeatures pendingFeatures)
    {
        if ((pendingFeatures & CrossingFeatures.Saria) != 0) return CrossingFeatures.Saria;
        if ((pendingFeatures & CrossingFeatures.LongBridge) != 0) return CrossingFeatures.LongBridge;
        if ((pendingFeatures & CrossingFeatures.DaruniaEncounters) != 0) return CrossingFeatures.DaruniaEncounters;
        return CrossingFeatures.None;
    }

    private static void ValidateCrossingDeadZone(RegionCrossingDeadZone? deadZone)
    {
        if (deadZone is not { } bounds) return;
        // The all-sentinel value also represents an omitted dead zone.
        if (bounds is { MinimumX: 999, MaximumX: -1, MinimumY: 999, MaximumY: -1 }) return;

        if (bounds.MinimumX == 999 || bounds.MaximumX == -1 || bounds.MinimumY == 999 || bounds.MaximumY == -1)
        {
            throw new ArgumentException("Supply all four dead-zone bounds or omit the dead zone.", nameof(deadZone));
        }
    }

    private void ResetCrossingFeatureLocations(CrossingFeatures features)
    {
        // Clear old positions so the shore search can consider those tiles.
        if ((features & CrossingFeatures.Saria) != 0)
        {
            foreach (Location location in AllLocations.Where(location => location.Town?.Type == TownType.SARIA))
            {
                location.Pos = IntVector2.NOT_ON_MAP;
            }
        }

        if ((features & CrossingFeatures.DaruniaEncounters) != 0)
        {
            ResetEncounterPairPositions(CrossingFeatures.DaruniaEncounters);
        }

        if ((features & CrossingFeatures.LongBridge) != 0)
        {
            ResetEncounterPairPositions(CrossingFeatures.LongBridge);
        }
    }

    private void ResetEncounterPairPositions(CrossingFeatures feature)
    {
        var (first, second, _) = GetEncounterCrossingLayout(feature);
        if (first is not null) first.Pos = IntVector2.NOT_ON_MAP;
        if (second is not null) second.Pos = IntVector2.NOT_ON_MAP;
    }

    private readonly record struct RegionCrossingSpan(IntVector2 Start, IntVector2 End, IntVector2 Forward, int Length, int StartRegion, int EndRegion)
    {
        public IntVector2 Side => Forward.Perpendicular();
    }

    private enum EncounterCrossingPlacement { Placed, Unavailable, EntranceBlocked, TooNarrow }

    private void PlaceSariaCrossing(RegionCrossingSpan span, Terrain[] crossingTerrains)
    {
        // Saria's two town entrances sit on opposite banks.
        map[span.End] = Terrain.TOWN;
        GetLocation(LocationID.WEST_TOWN_SARIA_SOUTH).Pos = span.End;

        var northEntrance = span.End - span.Forward;
        while (crossingTerrains.Contains(map[northEntrance])) northEntrance -= span.Forward;
        map[northEntrance] = Terrain.TOWN;
        GetLocation(LocationID.WEST_TOWN_SARIA_NORTH).Pos = northEntrance;
    }

    private (Location? First, Location? Second, Terrain Terrain) GetEncounterCrossingLayout(CrossingFeatures feature)
    {
        var (firstId, secondId, terrain) = feature switch
        {
            CrossingFeatures.LongBridge => (LocationID.WEST_BRIDGE_AFTER_DM_WEST, LocationID.WEST_BRIDGE_AFTER_DM_EAST, Terrain.BRIDGE),
            CrossingFeatures.DaruniaEncounters => (LocationID.EAST_TRAP_DESERT2, LocationID.EAST_TRAP_DESERT1, Terrain.DESERT),
            _ => throw new ArgumentOutOfRangeException(nameof(feature))
        };

        return (AllLocations.Find(location => location.ID == firstId),
            AllLocations.Find(location => location.ID == secondId), terrain);
    }

    /// <summary>Paints the long bridge or Darunia path together with its encounter records.</summary>
    /// <remarks>Checks the far entrance before painting. Darunia requires both encounter records to be available.</remarks>
    private EncounterCrossingPlacement PlaceEncounterCrossing(RegionCrossingSpan span, Terrain[] crossingTerrains, CrossingFeatures feature, bool shapeMountainPassages)
    {
        var (first, second, terrain) = GetEncounterCrossingLayout(feature);
        if (first is null || second is null)
            throw new InvalidOperationException($"Cannot place {feature} without both encounter locations.");

        bool isLongBridge = feature == CrossingFeatures.LongBridge;
        if (!isLongBridge && (!first.CanShuffle || !second.CanShuffle))
            return EncounterCrossingPlacement.Unavailable;

        // Start just inside the far bank. The encounter needs crossing terrain on at least one side.
        var firstTileFromFarBank = span.End - span.Forward;
        var farPosition = firstTileFromFarBank;
        var side = span.Side;
        while (!crossingTerrains.Contains(map[farPosition + side]) && !crossingTerrains.Contains(map[farPosition - side]))
        {
            if (span.Start.IsBehind(farPosition, span.Forward))
            {
                logger.Warn($"Unable to place {feature} encounter at a jagged entrance");
                return EncounterCrossingPlacement.EntranceBlocked;
            }
            farPosition -= span.Forward;
        }

        MountainLane[] mountainLanes = [];
        if (shapeMountainPassages && !isLongBridge)
        {
            int farTile = (span.End - farPosition).ManhattanLength - 1;
            mountainLanes = PlanDaruniaMountainLanes(span, farPosition, farTile);
            int encounterSpanLength = span.Length - farTile;
            if (encounterSpanLength >= MinimumLengthRequiringWidePassage && mountainLanes.Length == 0)
            {
                return EncounterCrossingPlacement.TooNarrow;
            }
        }

        if (!walkableTerrains.Contains(map[firstTileFromFarBank])) map[firstTileFromFarBank] = terrain;

        // The long bridge can advance its far encounter by one more tile.
        if (isLongBridge && crossingTerrains.Contains(map[farPosition + span.Forward]))
            farPosition += span.Forward;

        bool positiveDirection = span.Forward.X > 0 || span.Forward.Y > 0;
        var (nearEncounter, farEncounter) = positiveDirection ? (first, second) : (second, first);
        farEncounter.Pos = farPosition;
        MatchEncounterSideTerrain(farPosition, side);

        for (var position = farPosition; position != span.Start; position -= span.Forward)
        {
            map[position] = terrain;
        }

        foreach (var lane in mountainLanes)
        {
            for (int tileIndex = lane.FirstTile; tileIndex <= lane.LastTile; tileIndex++)
            {
                map[MountainCrossingTile(span, tileIndex) + lane.Side] = terrain;
            }
        }

        // Darunia restores water at the near bank. The long bridge restores any crossing terrain there.
        Terrain nearBankTerrain = map[span.Start];
        bool restoreBank = isLongBridge ? crossingTerrains.Contains(nearBankTerrain) : nearBankTerrain.IsWater();
        if (restoreBank) map[span.Start] = map[span.Start - span.Forward];

        var nearPosition = span.Start + span.Forward;
        MatchEncounterSideTerrain(nearPosition, side);
        map[nearPosition] = terrain;
        nearEncounter.Pos = nearPosition;
        first.CanShuffle = false;
        second.CanShuffle = false;
        return EncounterCrossingPlacement.Placed;
    }

    /// <summary>Closes the open side of an encounter when the opposite side is blocked.</summary>
    private void MatchEncounterSideTerrain(IntVector2 position, IntVector2 side)
    {
        var left = position - side;
        var right = position + side;
        bool leftBlocked = map[left] == Terrain.MOUNTAIN || map[left].IsWater();
        bool rightBlocked = map[right] == Terrain.MOUNTAIN || map[right].IsWater();

        if (leftBlocked == rightBlocked) return;
        if (leftBlocked) map[right] = map[left];
        else map[left] = map[right];
    }
}
