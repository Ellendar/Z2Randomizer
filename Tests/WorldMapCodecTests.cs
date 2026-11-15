using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class WorldMapCodecTests
{
    [TestMethod]
    [DataRow(1, "06F5F5F5E5")]
    [DataRow(15, "E6F5F5F505")]
    [DataRow(16, "F6F5F5F5")]
    [DataRow(17, "F606F5F5E5")]
    public void Encode_TerrainTransitionsAroundTheRunLimitMatchTheStoredFormat(int forestTiles, string expectedHex)
    {
        var map = new OverworldMap(1, 64);
        for (var x = 0; x < 64; x++) map[0, x] = x < forestTiles ? Terrain.FOREST : Terrain.GRASS;

        var encoded = WorldMapCodec.Encode(map);

        encoded.Should().Equal(Convert.FromHexString(expectedHex));
    }

    [TestMethod]
    public void Encode_PadsANarrowMapToTheStoredWidth()
    {
        var map = new OverworldMap(1, 8, Terrain.GRASS);

        var encoded = WorldMapCodec.Encode(map);

        encoded.Should().Equal(0x75, 0xFC, 0xFC, 0xFC, 0x7C);
    }

    [TestMethod]
    [DataRow(0, "06F6F6F6E6")]
    [DataRow(20, "F63606F6F6A6")]
    [DataRow(63, "F6F6F6E606")]
    public void Encode_IsolatesTheHammerRevealInsideAForestRun(int x, string expectedHex)
    {
        var map = new OverworldMap(1, 64, Terrain.FOREST);
        var options = new MapEncodingOptions(HiddenForestTilePosition: new IntVector2(x, 0));

        WorldMapCodec.Encode(map, options).Should().Equal(Convert.FromHexString(expectedHex));
    }

    [TestMethod]
    [DataRow(-1, 0)]
    [DataRow(0, -1)]
    [DataRow(8, 0)]
    [DataRow(0, 2)]
    public void Encode_RejectsAHammerRevealOutsideTheLogicalMap(int x, int y)
    {
        var map = new OverworldMap(2, 8, Terrain.FOREST);
        var options = new MapEncodingOptions(HiddenForestTilePosition: new IntVector2(x, y));

        Action encode = () => WorldMapCodec.Encode(map, options);
        encode.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*hidden forest tile*");
    }

    [TestMethod]
    [DataRow(-1, 0)]
    [DataRow(0, -1)]
    [DataRow(8, 0)]
    [DataRow(0, 2)]
    public void Encode_RejectsThreeEyeRockOutsideTheLogicalMap(int x, int y)
    {
        var map = new OverworldMap(2, 8, Terrain.DESERT);
        var options = new MapEncodingOptions(ThreeEyeRockPosition: new IntVector2(x, y));

        Action encode = () => WorldMapCodec.Encode(map, options);
        encode.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Three Eye Rock*");
    }

    [TestMethod]
    public void Encode_ThreeEyeRockUsesTheSuppliedUnrevealedTerrainWithoutExtendingTheRow()
    {
        var map = new OverworldMap(1, 64, Terrain.GRASS);
        map[0, 20] = Terrain.PALACE;
        for (int x = 21; x < 64; x++) map[0, x] = Terrain.FOREST;
        var options = new MapEncodingOptions(ThreeEyeRockPosition: new IntVector2(20, 0),
            ThreeEyeRockUnrevealedTerrain: Terrain.DESERT);

        WorldMapCodec.Encode(map, options).Should().Equal(0xF5, 0x35, 0x04, 0xF6, 0xF6, 0xA6);
    }

    [TestMethod]
    public void Encode_CoincidentHiddenLocationsSplitTheTileOnlyOnce()
    {
        var map = new OverworldMap(52, 64);
        for (var y = 0; y < 52; y++)
            for (var x = 0; x < 64; x++) map[y, x] = Terrain.GRASS;

        var encoded = WorldMapCodec.Encode(map,
            new MapEncodingOptions(ThreeEyeRockPosition: new IntVector2(61, 51), HiddenForestTilePosition: new IntVector2(61, 51)));

        encoded.Should().HaveCount(210);
        encoded.Skip(204).Should().Equal(0xF5, 0xF5, 0xF5, 0xC5, 0x06, 0x15);
        map[51, 61].Should().Be(Terrain.GRASS);
    }
}
