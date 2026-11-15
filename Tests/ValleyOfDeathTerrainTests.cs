using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class ValleyOfDeathTerrainTests
{
    [TestMethod]
    [DataRow(4, 4)]
    [DataRow(11, 8)]
    public void OpeningPlacesThePalaceInLavaWithAMountainRimAndInsetCorners(int x, int y)
    {
        var center = new IntVector2(x, y);
        var map = new OverworldMap(y + 5, x + 6, Terrain.GRASS);
        EastHyrule.DrawValleyOfDeathOpening(map, center);
        map[center].Should().Be(Terrain.PALACE);
        foreach (var corner in new IntVector2[] { new(-3, -3), new(-3, 3), new(3, -3), new(3, 3) })
            map[center + corner].Should().Be(Terrain.MOUNTAIN);
        foreach (var direction in IntVector2.CARDINALS)
        {
            map[center + 4 * direction].Should().Be(Terrain.MOUNTAIN);
            map[center + 3 * direction].Should().Be(Terrain.LAVA);
        }
        var counts = new Dictionary<Terrain, int>();
        for (int row = 0; row < map.GetLength(0); row++)
            for (int column = 0; column < map.GetLength(1); column++)
            {
                var terrain = map[row, column];
                counts[terrain] = counts.GetValueOrDefault(terrain) + 1;
                if (Math.Abs(column - x) > 4 || Math.Abs(row - y) > 4)
                    terrain.Should().Be(Terrain.GRASS, "the opening must preserve everything outside its footprint");
            }
        counts[Terrain.PALACE].Should().Be(1);
        counts[Terrain.LAVA].Should().Be(44);
        counts[Terrain.MOUNTAIN].Should().Be(36);
    }
}
