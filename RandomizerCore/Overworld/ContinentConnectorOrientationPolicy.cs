using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>Defines allowed shores from continent, biome and connection rules, without consulting the map or RNG.</summary>
internal sealed record ContinentConnectorOrientationPolicy(IReadOnlyList<Direction> RaftEdges, IReadOnlyList<Direction> BridgeEdges)
{
    private static readonly Direction[] ALL_EDGES = [Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST];

    internal static ContinentConnectorOrientationPolicy Resolve(Continent continent, Biome biome, bool horizontal, ContinentConnectionType connections)
    {
        switch (continent)
        {
            case Continent.WEST:
            case Continent.EAST:
            case Continent.DM:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(continent));
        }

        return new(DetermineRaftEdges(continent, biome, horizontal, connections),
                   DetermineBridgeEdges());
    }

    private static IReadOnlyList<Direction> DetermineRaftEdges(Continent continent, Biome biome,
        bool horizontal, ContinentConnectionType connections)
    {
        switch (biome)
        {
            // Raft tile will be placed on one of the edges in the biome's open axis
            case Biome.CANYON:
            case Biome.DRY_CANYON:
            case Biome.CALDERA:
            case Biome.VOLCANO:
                return horizontal ? [Direction.WEST, Direction.EAST] : [Direction.NORTH, Direction.SOUTH];
            case Biome.COASTAL_ISLANDS when continent != Continent.DM:
                return connections switch
                {
                    ContinentConnectionType.NORMAL => horizontal ? [Direction.SOUTH] : [Direction.EAST],
                    _ => ALL_EDGES,
                };
            case Biome.VANILLA:
            case Biome.VANILLA_SHUFFLE:
            case Biome.VANILLALIKE:
            case Biome.ISLANDS:
            case Biome.COASTAL_ISLANDS:
            case Biome.MOUNTAINOUS:
                return connections switch
                {
                    ContinentConnectionType.NORMAL => continent switch
                    {
                        Continent.WEST => [Direction.EAST],
                        Continent.EAST => [Direction.WEST],
                        Continent.DM => ALL_EDGES,
                        _ => throw new ArgumentOutOfRangeException(nameof(continent))
                    },
                    ContinentConnectionType.TRANSPORTATION_SHUFFLE or ContinentConnectionType.ANYTHING_GOES => ALL_EDGES,
                    _ => throw new ArgumentOutOfRangeException(nameof(connections))
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome, "Resolve configuration biomes before choosing shores.");
        }
    }

    private static IReadOnlyList<Direction> DetermineBridgeEdges() => ALL_EDGES;
}
