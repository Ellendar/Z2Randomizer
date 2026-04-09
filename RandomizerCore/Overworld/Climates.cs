using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;
public static class Climates
{
    private static readonly Climate CLASSIC = new(
        "Classic",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 1 ),
            ( Terrain.GRASS, 1 ),
            ( Terrain.FOREST, 1 ),
            ( Terrain.SWAMP, 1 ),
            ( Terrain.GRAVE, 1 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 1 ),
            ( Terrain.ROAD, 1 ),
        ]),
        30,
        ClimateEnum.CLASSIC
    );

    private static readonly Climate VANILLA_WEIGHTED_WEST = new(
        "Vanilla-Weighted West",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new LinearWeightedRandom<Terrain>([
            ( Terrain.DESERT, 615 ),
            ( Terrain.GRASS, 647 ),
            ( Terrain.FOREST, 359 ),
            ( Terrain.SWAMP, 300 ),
            ( Terrain.GRAVE, 256 ),
            ( Terrain.LAVA, 0 ),
            ( Terrain.WALKABLEWATER, 712 ),
            ( Terrain.WATER, 712 ),
            ( Terrain.MOUNTAIN, 1546 ),
            ( Terrain.ROAD, 328 ),
        ]),
        30,
        ClimateEnum.VANILLA_WEIGHTED_WEST
    );

    private static readonly Climate VANILLA_WEIGHTED_EAST = new(
        "Vanilla-Weighted East",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new LinearWeightedRandom<Terrain>([
            ( Terrain.DESERT, 542 ),
            ( Terrain.GRASS, 401 ),
            ( Terrain.FOREST, 556 ),
            ( Terrain.SWAMP, 90 ),
            ( Terrain.GRAVE, 45 ),
            ( Terrain.LAVA, 131 ),
            ( Terrain.WALKABLEWATER, 1359 ), /* 350 walkable */
            ( Terrain.WATER, 1359 ),
            ( Terrain.MOUNTAIN, 1514 ),
            ( Terrain.ROAD, 128 ),
        ]),
        30,
        ClimateEnum.VANILLA_WEIGHTED_EAST
    );

    private static readonly Climate CHAOS = new(
        "Chaos",
        //Coefficients
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, .2f }
        },
        //weights
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 5 ),
            ( Terrain.GRASS, 5 ),
            ( Terrain.FOREST, 5 ),
            ( Terrain.SWAMP, 5 ),
            ( Terrain.GRAVE, 5 ),
            ( Terrain.LAVA, 5 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 5 ),
            ( Terrain.ROAD, 5 ),
        ]),
        300,
        ClimateEnum.CHAOS
    );

    private static readonly Climate WETLANDS = new(
        "Wetlands",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 2.5f },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 0 ),
            ( Terrain.GRASS, 2 ),
            ( Terrain.FOREST, 2 ),
            ( Terrain.SWAMP, 2 ),
            ( Terrain.GRAVE, 2 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 3 ),
            ( Terrain.WATER, 3 ),
            ( Terrain.MOUNTAIN, 2 ),
            ( Terrain.ROAD, 3 ),
        ]),
        30,
        ClimateEnum.WETLANDS
    );

    private static readonly Climate SCRUBLAND = new(
        "Scrubland",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, .5f },
            { Terrain.SWAMP, .25f },
            { Terrain.GRAVE, .35f },
            { Terrain.LAVA, .5f },
            { Terrain.WALKABLEWATER, .5f },
            { Terrain.PREPLACED_WATER_WALKABLE, 1f },
            { Terrain.PREPLACED_WATER, 1f },
            { Terrain.WATER, .25f },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 2 ),
            ( Terrain.GRASS, 3 ),
            ( Terrain.FOREST, 1 ),
            ( Terrain.SWAMP, 0 ),
            ( Terrain.GRAVE, 4 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 1 ),
            ( Terrain.ROAD, 2 ),
        ]),
        30,
        ClimateEnum.SCRUBLAND
    );

    //Scrubland is basically the most degenerate case for DM. Lots of low-growing terrain creates a bunch of tiny,
    //not-connected sections that don't create a routeable whole. So for DM specifcally, use a toned-down version
    //with higher walkable growth and lower grave frequency
    private static readonly Climate DM_SCRUBLAND = new(
        "Scrubland",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, .75f },
            { Terrain.SWAMP, .45f },
            { Terrain.GRAVE, .60f },
            { Terrain.LAVA, .5f },
            { Terrain.WALKABLEWATER, .5f },
            { Terrain.PREPLACED_WATER_WALKABLE, 1f },
            { Terrain.PREPLACED_WATER, 1f },
            { Terrain.WATER, .25f },
            { Terrain.MOUNTAIN, .8f },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 2 ),
            ( Terrain.GRASS, 3 ),
            ( Terrain.FOREST, 1 ),
            ( Terrain.SWAMP, 0 ),
            ( Terrain.GRAVE, 2 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 1 ),
            ( Terrain.ROAD, 2 ),
        ]),
        30,
        ClimateEnum.DM_SCRUBLAND
    );

    private static readonly Climate GREAT_LAKES = new(
       "GreatLakes",
       //Size
       new Dictionary<Terrain, float>
       {
            { Terrain.DESERT, .5f },
            { Terrain.GRASS, 1.5f },
            { Terrain.FOREST, 1.5f },
            { Terrain.SWAMP, .75f },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 2.5f },
            { Terrain.PREPLACED_WATER_WALKABLE, 1.5f },
            { Terrain.PREPLACED_WATER, 1.5f },
            { Terrain.WATER, 2.5f },
            { Terrain.MOUNTAIN, .75f },
            { Terrain.ROAD, 1 }
       },
       //Frequency
       new TableWeightedRandom<Terrain>([
           ( Terrain.DESERT, 0 ),
            ( Terrain.GRASS, 4 ),
            ( Terrain.FOREST, 4 ),
            ( Terrain.SWAMP, 4 ),
            ( Terrain.GRAVE, 3 ),
            ( Terrain.LAVA, 3 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 4 ),
            ( Terrain.ROAD, 5 ),
       ]),
       30,
       ClimateEnum.GREAT_LAKES
   );

    public static Climate Create(ClimateEnum climate)
    {
        return climate switch
        {
            ClimateEnum.CLASSIC => CLASSIC.CloneWithInvertedDistances(),
            ClimateEnum.VANILLA_WEIGHTED_WEST => VANILLA_WEIGHTED_WEST.Clone(),
            ClimateEnum.VANILLA_WEIGHTED_EAST => VANILLA_WEIGHTED_EAST.Clone(),
            ClimateEnum.CHAOS => CHAOS.CloneWithInvertedDistances(),
            ClimateEnum.WETLANDS => WETLANDS.CloneWithInvertedDistances(),
            ClimateEnum.GREAT_LAKES => GREAT_LAKES.CloneWithInvertedDistances(),
            ClimateEnum.SCRUBLAND => SCRUBLAND.CloneWithInvertedDistances(),
            ClimateEnum.DM_SCRUBLAND => DM_SCRUBLAND.CloneWithInvertedDistances(),
            _ => throw new NotImplementedException()
        };
    }
}
