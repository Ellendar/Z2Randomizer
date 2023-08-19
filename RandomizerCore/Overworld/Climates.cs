using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.Core;

namespace Z2Randomizer.Core.Overworld;
public class Climates
{
    public static readonly Climate Classic = new Climate
    (
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
                { Terrain.ROAD, 0 }
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
                { Terrain.ROAD, 0 }
            }, 
        30 
    );
}


//Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN