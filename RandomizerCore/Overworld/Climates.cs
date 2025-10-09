using System;
using System.Collections.Generic;
using System.Linq;

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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
            },
        new Dictionary<Terrain, int> 
            {
                { Terrain.DESERT, 1 },
                { Terrain.GRASS, 1 },
                { Terrain.FOREST, 1 },
                { Terrain.SWAMP, 1 },
                { Terrain.GRAVE, 1 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
            }, 
        30 
    );

    public static Climate Classic { get => CLASSIC.Clone(); }

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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, .2f }
           },
       //weights
       new Dictionary<Terrain, int>
           {
                { Terrain.DESERT, 5 },
                { Terrain.GRASS, 5 },
                { Terrain.FOREST, 5 },
                { Terrain.SWAMP, 5 },
                { Terrain.GRAVE, 5 },
                { Terrain.LAVA, 5 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 5 },
                { Terrain.ROAD, 5 }
           },
       220
    );

    public static Climate Chaos { get => CHAOS.Clone(); }

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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 0 },
                { Terrain.GRASS, 2 },
                { Terrain.FOREST, 2 },
                { Terrain.SWAMP, 2 },
                { Terrain.GRAVE, 2 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 3 },
                { Terrain.WATER, 3 },
                { Terrain.MOUNTAIN, 2 },
                { Terrain.ROAD, 3 }
        },
        30
    );

    public static Climate Wetlands { get => WETLANDS.Clone(); }

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
                { Terrain.WATER, .25f },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 2 },
                { Terrain.GRASS, 3 },
                { Terrain.FOREST, 1 },
                { Terrain.SWAMP, 0 },
                { Terrain.GRAVE, 4 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 2 }
        },
        30
    );

    public static Climate Scrubland { get => SCRUBLAND.Clone(); }

    private static readonly Climate GREAT_LAKES = new(
        "GreatLakes",
        //Size
        new Dictionary<Terrain, float>
        {
                { Terrain.DESERT, .5f },
                { Terrain.GRASS, 1 },
                { Terrain.FOREST, 1.5f },
                { Terrain.SWAMP, .75f },
                { Terrain.GRAVE, 1 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 5 },
                { Terrain.WATER, 5 },
                { Terrain.MOUNTAIN, .75f },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 0 },
                { Terrain.GRASS, 2 },
                { Terrain.FOREST, 2 },
                { Terrain.SWAMP, 2 },
                { Terrain.GRAVE, 2 },
                { Terrain.LAVA, 2 },
                { Terrain.WALKABLEWATER, 2 },
                { Terrain.WATER, 2 },
                { Terrain.MOUNTAIN, 2 },
                { Terrain.ROAD, 3 }
        },
        30
    );

    public static Climate GreatLakes { get => GREAT_LAKES.Clone(); }

    public static Climate ByName(string name)
    {
        //This could probably all be done with reflection but quick fix for now.
        return name switch
        {
            "Classic" => Classic,
            "Chaos" => Chaos,
            "Wetlands" => Wetlands,
            "Scrubland" => Scrubland,
            "GreatLakes" => GreatLakes,
            _ => throw new Exception("Unable to map Climate: " + name + " by name.")
        };
    }
    
    public static IEnumerable<Climate> ClimateList =
    [
        CLASSIC,
        CHAOS,
        WETLANDS,
        GREAT_LAKES,
        SCRUBLAND,
    ];

    public static IEnumerable<string> ClimateNameList = ClimateList.Select(i => i.Name);
}


//Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN