using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class MountainBandWalkTests
{
    [TestMethod]
    [DataRow(0, 0, 3, 1, new[] { 1, 0, 2, 0, 3, 0 })]
    [DataRow(0, 0, 1, 3, new[] { 0, 1, 0, 2, 1, 2 })]
    [DataRow(0, 0, 2, 2, new[] { 1, 0, 1, 1, 2, 1 })]
    [DataRow(2, 2, 0, 0, new[] { 1, 2, 1, 1, 0, 1 })]
    [DataRow(0, 0, 4, 0, new[] { 1, 0, 2, 0, 3, 0 })]
    [DataRow(0, 0, 0, -3, new[] { 0, -1, 0, -2 })]
    [DataRow(4, 7, 4, 7, new int[] { })]
    [DataRow(4, 7, 5, 7, new int[] { })]
    public void InteriorUsesCardinalStepsWithXPriorityAndOmitsBothEndpoints(int startX, int startY, int endX, int endY, int[] expected)
    {
        var start = new IntVector2(startX, startY);
        var end = new IntVector2(endX, endY);
        var positions = expected.Chunk(2).Select(p => new IntVector2(p[0], p[1]));

        TerrainLayout.MountainBandInterior(start, end).Should().Equal(positions);
    }
}
