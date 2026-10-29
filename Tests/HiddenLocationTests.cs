using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class HiddenLocationTests
{
    [TestMethod]
    public void HiddenEntryKeepsTheWorldLocationReferenceAndSeparateTerrainValues()
    {
        var location = new Location(LocationID.EAST_PALACE6, 82, 37, 0, Continent.EAST)
            { IsExternalWorld = true, TerrainType = Terrain.PALACE, EntranceNumber = 2 };
        var reveal = HiddenLocation.FromLocation(location, HiddenLocationTrigger.Flute, unrevealedTerrain: Terrain.FOREST);
        location.Disable();
        location.TerrainType = Terrain.FOREST;
        location.IsExternalWorld = false;
        location.EntranceNumber = 0;
        reveal.Location.Should().BeSameAs(location);
        reveal.Location.Xpos.Should().Be(0);
        reveal.Location.YRaw.Should().Be(0);
        reveal.AreaByte.Should().Be(0);
        reveal.LocationIndex.Should().Be(0x35);
        reveal.Location.EntranceNumber.Should().Be(0);
        reveal.UnrevealedTerrain.Should().Be(Terrain.FOREST);
        reveal.RevealedTerrain.Should().Be(Terrain.PALACE);
    }

}
