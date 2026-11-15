using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class VanillaShuffleAreasTests
{
    [TestMethod]
    public void GroupsCurrentVanillaCoordinatesInLocationOrder()
    {
        var north = At(0x34, 0x17);
        var parapa = At(0x34, 0x17);
        var otherNorth = At(0x20, 0x1D);
        // Grouping follows current coordinates, as required after vanilla entry preparation.
        parapa.CoordsY30Offset = (0x39, 0x3D);

        var areas = VanillaShuffleAreas.GroupLocations(Continent.WEST, [otherNorth, parapa, north]);

        areas["north"].Should().Equal(otherNorth, north);
        areas["parapa"].Should().ContainSingle().Which.Should().BeSameAs(parapa);
        parapa.CoordsY30Offset.Should().Be((0x39, 0x3D));
        parapa.CanShuffle.Should().BeTrue();
    }

    [TestMethod]
    public void SharedCoordinatesRetainTheFirstLocationForEachEntry()
    {
        var first = At(0x34, 0x17);
        var other = At(0x20, 0x1D);
        var collocated = At(0x34, 0x17);

        var areas = VanillaShuffleAreas.GroupLocations(Continent.WEST, [first, other, collocated]);

        areas["north"].Should().Equal(first, other, first);
        collocated.CoordsY30Offset.Should().Be(first.CoordsY30Offset);
    }

    [TestMethod]
    public void SameCoordinatesCanBelongToDifferentRegionsOnDifferentContinents()
    {
        var location = At(0x4C, 0x15);

        VanillaShuffleAreas.GroupLocations(Continent.WEST, [location])["mid"]
            .Should().ContainSingle().Which.Should().BeSameAs(location);
        VanillaShuffleAreas.GroupLocations(Continent.EAST, [location])["south"]
            .Should().ContainSingle().Which.Should().BeSameAs(location);
    }

    [TestMethod]
    [DataRow(Continent.WEST)]
    [DataRow(Continent.EAST)]
    public void EmptyInputKeepsNamedAreasAvailable(Continent continent)
    {
        var areas = VanillaShuffleAreas.GroupLocations(continent, []);

        areas.Should().HaveCount(9);
        areas.Values.Should().OnlyContain(locations => locations.Count == 0);
        var requiredCaveAreas = continent == Continent.WEST
            ? new[] { "parapa", "lifesouth", "island", "dmexit" }
            : new[] { "kasuto", "vod", "gp" };
        areas.Keys.Should().Contain(requiredCaveAreas);
    }

    [TestMethod]
    [DataRow(Continent.WEST)]
    [DataRow(Continent.EAST)]
    public void UnknownCoordinateFailsWithoutChangingLocations(Continent continent)
    {
        var location = At(0, 0);

        Action group = () => VanillaShuffleAreas.GroupLocations(continent, [location]);

        group.Should().Throw<KeyNotFoundException>();
        location.CoordsY30Offset.Should().Be((0, 0));
        location.CanShuffle.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(Continent.DM)]
    [DataRow(Continent.MAZE)]
    public void OtherContinentsDoNotUseTheseAreas(Continent continent)
    {
        Action group = () => VanillaShuffleAreas.GroupLocations(continent, []);

        group.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("continent");
    }

    private static Location At(int yRaw, int x)
        => new(LocationID.WEST_NORTH_PALACE, yRaw, x, 0, Continent.WEST);
}
