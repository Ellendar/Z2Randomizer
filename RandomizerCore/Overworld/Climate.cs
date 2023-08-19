using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.Core;

namespace Z2Randomizer.Core.Overworld;
public class Climate
{
    private readonly Dictionary<Terrain, float> DistanceCoefficients;
    private readonly Dictionary<Terrain, int> TerrainWeights;
    private readonly Dictionary<Terrain, int> TerrainPercentages;
    public int SeedTerrainCount { get; }
    private int totalTerrainWeight;
    private Terrain[] terrainWeightTable;
    public string Name { get; set; }

    public Climate(string name, Dictionary<Terrain, float> distanceCoefficients, Dictionary<Terrain, int> terrainWeights, int seedTerrainCount)
    {
        Name = name;
        DistanceCoefficients = distanceCoefficients;
        TerrainWeights = terrainWeights;
        SeedTerrainCount = seedTerrainCount;
        totalTerrainWeight = TerrainWeights.Values.Sum();
        //This is a lazy, not very efficient lookup method, but it has very fast lookups, and since terrain generation
        //is one of the most expensive operations and we're going to do tens of thousands of lookups easily,
        //this is probably good for now.
        terrainWeightTable = new Terrain[totalTerrainWeight];
        int terrainWeightTableIndex = 0;
        foreach(Terrain terrain in TerrainWeights.Keys)
        {
            for(int i = 0; i < TerrainWeights[terrain]; i++)
            {
                terrainWeightTable[terrainWeightTableIndex++] = terrain;
            }
        }
    }

    public float GetDistanceCoefficient(Terrain terrain) 
    { 
        if(!DistanceCoefficients.ContainsKey(terrain))
        {
            return 0;
        }
        return DistanceCoefficients[terrain];
    }

    public Terrain GetRandomTerrain(Random r)
    {
        return terrainWeightTable[r.Next(totalTerrainWeight)];
    }
}

