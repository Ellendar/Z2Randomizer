using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class CoastalCanyonLayoutTests
{
    private static readonly Terrain[] BankTerrains =
        [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN];

    [TestMethod]
    [DataRow(64, 75, false, Terrain.PREPLACED_WATER)]
    [DataRow(64, 75, true, Terrain.PREPLACED_WATER_WALKABLE)]
    [DataRow(26, 26, false, Terrain.PREPLACED_WATER_WALKABLE)]
    [DataRow(26, 26, true, Terrain.PREPLACED_WATER)]
    public void CoastRetainsPlacementSpaceOnBothSidesAndCutsStripesOnlyOffshore(
        int columns, int rows, bool horizontal, Terrain water)
    {
        var map = new OverworldMap(rows, columns, Terrain.NONE);
        var climate = Climates.Create(Continent.WEST, ClimateEnum.CLASSIC);

        TerrainLayout.DrawCoastalIslands(map, rows, columns, water, climate, BankTerrains, horizontal, new Random(7));

        IntVector2 forward = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        IntVector2 seaward = horizontal ? IntVector2.SOUTH : IntVector2.EAST;
        int length = horizontal ? columns : rows;
        int width = horizontal ? rows : columns;
        var shore = new int[length];
        int offshoreCrossStripes = 0;

        for (int step = 1; step < length - 1; step++)
        {
            IntVector2 origin = step * forward;
            map[origin].Should().Be(Terrain.MOUNTAIN);
            map[origin + (width - 1) * seaward].Should().Be(water);
            int firstWater = Enumerable.Range(1, width - 1).First(i => map[origin + i * seaward] == water);
            shore[step] = firstWater;
            Enumerable.Range(firstWater, 4).Should().OnlyContain(i => map[origin + i * seaward] == water);
            if (Enumerable.Range(firstWater, width - firstWater).All(i => map[origin + i * seaward] == water))
            {
                offshoreCrossStripes++;
            }
        }
        offshoreCrossStripes.Should().Be(length >= 44 ? 3 : 2);

        var emptyCenters =
            from step in Enumerable.Range(2, length - 4)
            from across in Enumerable.Range(2, width - 4)
            let position = step * forward + across * seaward
            where map.AllTerrainIn3x3Equals(position, Terrain.NONE)
            select (Step: step, Across: across);
        var centers = emptyCenters.ToArray();
        centers.Should().Contain(tile => tile.Across < shore[tile.Step]);
        centers.Should().Contain(tile => tile.Across > shore[tile.Step]);
    }
}
