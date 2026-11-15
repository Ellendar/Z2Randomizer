using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class OverworldMapTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    [DataRow(7)]
    public void SquareQueryChecksTheWholeRequestedFootprint(int radius)
    {
        var center = new IntVector2(radius + 2, radius + 3);
        var map = new OverworldMap(2 * radius + 7, 2 * radius + 9, Terrain.MOUNTAIN);
        map[0, 0] = Terrain.GRASS;
        map.AllTerrainInSquareEquals(center, Terrain.MOUNTAIN, radius).Should().BeTrue();
        var corner = center + new IntVector2(radius, -radius);
        map[corner] = Terrain.CAVE;
        map.AllTerrainInSquareEquals(center, Terrain.MOUNTAIN, radius).Should().BeFalse();
        map[corner].Should().Be(Terrain.CAVE);
    }

    [TestMethod]
    public void SquareQueryRejectsClippedAndExcessivelyLargeAreas()
    {
        var map = new OverworldMap(9, 11, Terrain.MOUNTAIN);
        map.AllTerrainInSquareEquals(new(4, 4), Terrain.MOUNTAIN, 4).Should().BeTrue();
        map.AllTerrainInSquareEquals(new(3, 4), Terrain.MOUNTAIN, 4).Should().BeFalse();
        map.AllTerrainInSquareEquals(new(4, 4), Terrain.MOUNTAIN, int.MaxValue).Should().BeFalse();
    }

    [TestMethod]
    public void SquareQueryRejectsNegativeRadius()
    {
        var map = new OverworldMap(9, 11, Terrain.MOUNTAIN);
        Action query = () => map.AllTerrainInSquareEquals(new(4, 4), Terrain.MOUNTAIN, -1);
        query.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    [DataRow(Terrain.NONE)]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.WATER)]
    public void Matching3x3IncludesTheCenterAndIgnoresTerrainOutsideTheArea(Terrain terrain)
    {
        var map = new OverworldMap(5, 7, terrain);
        map[0, 0] = Terrain.MOUNTAIN;
        map.AllTerrainIn3x3Equals(new(3, 2), terrain).Should().BeTrue();
        map[0, 0].Should().Be(Terrain.MOUNTAIN);
    }

    [TestMethod]
    [DataRow(-1, -1)]
    [DataRow(0, -1)]
    [DataRow(1, -1)]
    [DataRow(-1, 0)]
    [DataRow(0, 0)]
    [DataRow(1, 0)]
    [DataRow(-1, 1)]
    [DataRow(0, 1)]
    [DataRow(1, 1)]
    public void AnyMismatchedTileRejectsThe3x3WithoutRepainting(int dx, int dy)
    {
        var map = new OverworldMap(5, 7, Terrain.NONE);
        var changed = new IntVector2(3 + dx, 2 + dy);
        map[changed] = Terrain.CAVE;
        map.AllTerrainIn3x3Equals(new(3, 2), Terrain.NONE).Should().BeFalse();
        map[changed].Should().Be(Terrain.CAVE);
    }

    [TestMethod]
    [DataRow(0, 2)]
    [DataRow(6, 2)]
    [DataRow(3, 0)]
    [DataRow(3, 4)]
    [DataRow(-1, 2)]
    [DataRow(7, 2)]
    [DataRow(int.MinValue, 2)]
    [DataRow(3, int.MaxValue)]
    public void IncompleteOrOutside3x3AreasReturnFalse(int x, int y)
    {
        var map = new OverworldMap(5, 7, Terrain.NONE);
        map.AllTerrainIn3x3Equals(new(x, y), Terrain.NONE).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 1)]
    [DataRow(2, 4)]
    [DataRow(4, 2)]
    public void MapsTooSmallForA3x3AreaReturnFalse(int rows, int columns)
    {
        var map = new OverworldMap(rows, columns, Terrain.NONE);
        map.AllTerrainIn3x3Equals(new(1, 1), Terrain.NONE).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(Terrain.NONE)]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.WATER)]
    public void FilledConstructor_InitializesEveryTileOfARectangularMap(Terrain terrain)
    {
        var map = new OverworldMap(3, 5, terrain);

        map.GetLength(0).Should().Be(3);
        map.GetLength(1).Should().Be(5);
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                map[y, x].Should().Be(terrain);
                map[new IntVector2(x, y)].Should().Be(terrain);
            }
        }
    }

    [TestMethod]
    public void TwoArgumentConstructor_RetainsDefaultTerrainInitialization()
    {
        var map = new OverworldMap(2, 3);

        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 3; x++)
                map[y, x].Should().Be(default(Terrain));
    }
}
