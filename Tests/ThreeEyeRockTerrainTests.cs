using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class ThreeEyeRockTerrainTests
{
    [TestMethod]
    [DataRow(Terrain.DESERT)]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.SWAMP)]
    [DataRow(Terrain.GRAVE)]
    public void OpeningHasThreeMountainEyesAndKeepsThePalaceTileHidden(Terrain terrain)
    {
        var map = new OverworldMap(19, 21, Terrain.CAVE);

        EastHyrule.DrawThreeEyeRock(map, new(9, 8), terrain);

        var cells = from y in Enumerable.Range(0, 19)
                    from x in Enumerable.Range(0, 21) select new IntVector2(x, y);
        cells.Where(p => map[p] == Terrain.MOUNTAIN).Should().BeEquivalentTo(new[] {
            new IntVector2(9, 6), new(7, 8), new(11, 8) });
        cells.Count(p => map[p] == terrain).Should().Be(46);
        cells.Where(p => p.X < 6 || p.X > 12 || p.Y < 5 || p.Y > 11)
            .Should().OnlyContain(p => map[p] == Terrain.CAVE);
        map[new IntVector2(9, 10)].Should().Be(terrain);
    }
}
