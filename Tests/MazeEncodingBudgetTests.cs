using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
namespace Z2Randomizer.Tests;
[TestClass]
public class MazeEncodingBudgetTests
{
    [TestMethod]
    [DataRow(MazeSizeOption.LARGE, 598)]
    [DataRow(MazeSizeOption.MEDIUM, 418)]
    [DataRow(MazeSizeOption.SMALL, 285)]
    public void AlternatingTerrainMaximizesRunsAndStillFitsTheMazeBudget(MazeSizeOption size, int maximumBytes)
    {
        var dimensions = size.GetMeta();
        var map = new OverworldMap(dimensions.Height, dimensions.Width);
        for (int y = 0; y < dimensions.Height; y++)
            for (int x = 0; x < dimensions.Width; x++)
                map[y, x] = x % 2 == 0 ? Terrain.GRASS : Terrain.FOREST;
        var bytes = WorldMapCodec.Encode(map);
        bytes.Should().HaveCount(maximumBytes);
        bytes.Length.Should().BeLessThan(RomMap.OVERWORLD_MAP_CAPACITY, "every logical tile already occupies its own run; remaining columns are constant padding");
    }
}
