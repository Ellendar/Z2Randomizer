using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class WorldMapEncodingCharacterizationTests
{
    // These are small destination buffers for the encoder, not playable ROMs or
    // substitutes for the real base ROM needed for continent/integration coverage.
    private static ROM Destination() => new(Enumerable.Repeat((byte)0xCC, 32).ToArray());

    private static int WriteMap(TestWorld world, ROM destination, MapEncodingOptions options = default)
    {
        using var candidate = world.BeginTerrainCandidate(destination);
        var encodedMap = world.EncodeMap(options);
        world.StageEncodedMap(encodedMap).Should().BeTrue();
        candidate.Commit();
        return encodedMap.Length;
    }

    [TestMethod]
    public void EncodeMap_SplitsRunsAtSixteenTilesAndPadsTheReservedSpace()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS, mapRomOffset: 2, mapRomCapacity: 8);
        var destination = Destination();

        var encodedLength = WriteMap(world, destination);

        encodedLength.Should().Be(4);
        destination.GetBytes(0, 11).Should().Equal(0xCC, 0xCC, 0xF5, 0xF5, 0xF5, 0xF5, 0x0B, 0x0B, 0x0B, 0x0B, 0xCC);
    }

    [TestMethod]
    public void EncodeMap_SmallMapsStillEncodeSixtyFourColumnsWithWaterAtTheRight()
    {
        var world = new TestWorld(1, 8, Terrain.GRASS, mapRomCapacity: 5);
        var destination = Destination();

        var encodedLength = WriteMap(world, destination);

        encodedLength.Should().Be(5);
        destination.GetBytes(0, 6).Should().Equal(0x75, 0xFC, 0xFC, 0xFC, 0x7C, 0xCC);
    }

    [TestMethod]
    public void EncodeMap_SmallMapsUseMountainPaddingWhenTheTopRightTileIsMountain()
    {
        var world = new TestWorld(1, 8, Terrain.GRASS, mapRomCapacity: 5);
        world.map[0, 7] = Terrain.MOUNTAIN;
        var destination = Destination();

        WriteMap(world, destination).Should().Be(5);

        destination.GetBytes(0, 6).Should().Equal(0x65, 0xFB, 0xFB, 0xFB, 0x8B, 0xCC);
    }

    [TestMethod]
    public void EncodeMap_EndsEachRowEvenWhenTheNextRowHasTheSameTerrain()
    {
        var world = new TestWorld(2, 8, Terrain.GRASS, mapRomCapacity: 10);
        var destination = Destination();

        WriteMap(world, destination).Should().Be(10);

        destination.GetBytes(0, 10).Should().Equal(0x75, 0xFC, 0xFC, 0xFC, 0x7C, 0x75, 0xFC, 0xFC, 0xFC, 0x7C);
    }

    [TestMethod]
    [DataRow(Terrain.PREPLACED_WATER, (byte)0xFC)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE, (byte)0xFD)]
    public void EncodeMap_ConvertsPreplacedWaterToGameTerrain(Terrain terrain, byte token)
    {
        var world = new TestWorld(1, 64, terrain, mapRomCapacity: 4);
        var destination = Destination();

        WriteMap(world, destination).Should().Be(4);

        destination.GetBytes(0, 4).Should().Equal(token, token, token, token);
    }

    [TestMethod]
    public void EncodeMap_HiddenPalaceSplitsItsTileIntoASeparateToken()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS, mapRomCapacity: 6);
        var destination = Destination();
        var hiddenPalace = new Location(LocationID.EAST_PALACE6, 30, 20, 0, Continent.EAST);

        WriteMap(world, destination, options: new(ThreeEyeRockPosition: hiddenPalace.Pos)).Should().Be(6);

        destination.GetBytes(0, 7).Should().Equal(0xF5, 0x35, 0x05, 0xF5, 0xF5, 0xA5, 0xCC);
    }

    [TestMethod]
    public void EncodeMap_KasutoConcealsTheSuppliedLocationInTheRom()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS, mapRomCapacity: 7) { biome = Biome.VANILLA_SHUFFLE };
        var destination = Destination();
        var hiddenKasuto = new Location(LocationID.EAST_TOWN_NEW_KASUTO, 30, 20, 0, Continent.EAST);
        world.map[hiddenKasuto.Pos] = Terrain.TOWN;

        WriteMap(world, destination, options: new(HiddenForestTilePosition: hiddenKasuto.Pos)).Should().Be(6);
        destination.GetBytes(0, 8).Should().Equal(0xF5, 0x35, 0x06, 0xF5, 0xF5, 0xA5, 0x0B, 0xCC);
        world.map[hiddenKasuto.Pos].Should().Be(Terrain.TOWN);
    }

    [TestMethod]
    public void StageEncodedMap_RejectsAnOversizedEncodingWithoutWriting()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS, mapRomOffset: 2, mapRomCapacity: 3);
        var destination = Destination();

        var encodedMap = world.EncodeMap();
        encodedMap.Length.Should().Be(4);
        using var candidate = world.BeginTerrainCandidate(destination);
        world.StageEncodedMap(encodedMap).Should().BeFalse();
        candidate.Commit();

        destination.rawdata.Should().OnlyContain(value => value == 0xCC);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(63)]
    public void EncodeMap_ThreeEyeRockAtRowBoundaryUsesExactlyOneTile(int x)
    {
        var world = new TestWorld(1, 64, Terrain.GRASS, mapRomCapacity: 5);
        var destination = Destination();
        var hidden = new Location(LocationID.EAST_PALACE6, 30, x, 0, Continent.EAST);

        WriteMap(world, destination, new(ThreeEyeRockPosition: hidden.Pos)).Should().Be(5);

        destination.GetBytes(0, 6).Should().Equal(x == 0
            ? new byte[] { 0x05, 0xF5, 0xF5, 0xF5, 0xE5, 0xCC }
            : new byte[] { 0xF5, 0xF5, 0xF5, 0xE5, 0x05, 0xCC });
    }

    [TestMethod]
    public void EncodeMap_MeasureOnlyLeavesTheEntireDestinationUntouched()
    {
        // Measurement must not change encoded bytes, padding, or unrelated destination bytes.
        var world = new TestWorld(1, 64, Terrain.GRASS);
        var destination = Destination();

        using (var candidate = world.BeginTerrainCandidate(destination))
        {
            world.EncodeMap().Length.Should().Be(4);
            candidate.Commit();
        }

        destination.rawdata.Should().OnlyContain(value => value == 0xCC);
    }
}
