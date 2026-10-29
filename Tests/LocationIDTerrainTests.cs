using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class LocationIDTerrainTests
{
    [TestMethod]
    [DataRow(LocationID.WEST_NORTH_PALACE, Terrain.PALACE)]
    [DataRow(LocationID.WEST_FAIRY_CAVE_DROP, Terrain.GRAVE)]
    [DataRow(LocationID.DM_SPEC_ROCK, Terrain.CAVE)]
    [DataRow(LocationID.EAST_WATER, Terrain.WALKABLEWATER)]
    [DataRow(LocationID.EAST_TOWN_NEW_KASUTO, Terrain.TOWN)]
    [DataRow(LocationID.MI_CHILD_DROP, Terrain.ROAD)]
    [DataRow(LocationID.MI_CONNECTOR_BRIDGE, Terrain.BRIDGE)]
    public void LoadingDefaultsRetainSpecialLocationSemantics(LocationID id, Terrain expected)
    {
        id.GetDefaultTerrain().Should().Be(expected);
        id.TryGetDefaultTerrain(out var terrain).Should().BeTrue();
        terrain.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0x3f)]
    [DataRow(0x7f)]
    [DataRow(0xbf)]
    [DataRow(0xff)]
    [DataRow(0x100)]
    public void UnusedAndInvalidSlotsHaveNoDefaultTerrain(int value)
    {
        var id = (LocationID)value;
        id.TryGetDefaultTerrain(out var terrain).Should().BeFalse();
        terrain.Should().Be(Terrain.NONE);
        Action lookup = () => id.GetDefaultTerrain();
        lookup.Should().Throw<KeyNotFoundException>();
    }
}
