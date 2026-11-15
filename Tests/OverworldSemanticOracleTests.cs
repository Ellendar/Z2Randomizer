using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using static Z2Randomizer.Tests.OverworldSemanticAssertions;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class OverworldSemanticOracleTests
{
    [TestMethod]
    public void Decoder_AcceptsExactCapacityAndMapsPreplacedTerrain()
    {
        var world = new TestWorld(1, 64, Terrain.PREPLACED_WATER);
        MapMatches(world, [0xFC, 0xFC, 0xFC, 0xFC]).Should().Be(4);
    }

    [TestMethod]
    public void Decoder_RejectsTruncationAndOverflow()
    {
        Action check = () => Decode([0xF5, 0xF5, 0xF5], 1);
        check.Should().Throw<InvalidOperationException>().WithMessage("map-capacity-or-truncation:*");
    }

    [TestMethod]
    public void Decoder_RejectsRowOvershoot()
    {
        Action check = () => Decode([0xF5, 0xF5, 0xF5, 0x05, 0xF5], 1);
        check.Should().Throw<InvalidOperationException>().WithMessage("map-row-overshoot:*");
    }

    [TestMethod]
    public void MapOracle_RejectsWrongTerrainWithValidLengths()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS);
        Action check = () => MapMatches(world, [0xF5, 0xF5, 0xF4, 0xF5]);
        check.Should().Throw<InvalidOperationException>().WithMessage("map-terrain-mismatch:*");
    }

    [TestMethod]
    public void MapOracle_UsesLogicalPaddingAndRejectsUnseededTiles()
    {
        var world = new TestWorld(1, 8, Terrain.GRASS);
        MapMatches(world, [0x75, 0xFC, 0xFC, 0xFC, 0x7C]).Should().Be(5);
        world.map[0, 2] = Terrain.NONE;
        Action check = () => MapMatches(world, [0x75, 0xFC, 0xFC, 0xFC, 0x7C]);
        check.Should().Throw<InvalidOperationException>().WithMessage("unseeded-or-invalid-terrain:*");
    }

    [TestMethod]
    public void MapOracle_RequiresAOneTileForestRunAtTheHammerReveal()
    {
        var world = new TestWorld(1, 64, Terrain.FOREST);
        var position = new IntVector2(16, 0);
        world.map[position] = Terrain.TOWN;

        MapMatches(world, [0xF6, 0x06, 0xE6, 0xF6, 0xF6], hiddenKasuto: position).Should().Be(5);
        Action mergedRun = () => MapMatches(world, [0xF6, 0xF6, 0xF6, 0xF6], hiddenKasuto: position);
        mergedRun.Should().Throw<InvalidOperationException>().WithMessage("hammer-reveal-token:*");
    }

    [TestMethod]
    public void MapOracle_ChecksPalaceConcealmentExplicitly()
    {
        var world = new TestWorld(1, 64, Terrain.DESERT);
        world.map[0, 20] = Terrain.PALACE;
        byte[] region = [0xF4, 0x34, 0x04, 0xF4, 0xF4, 0xA4];
        var rock = new IntVector2(20, 0);
        MapMatches(world, region, hiddenPalace: rock, threeEyeRockUnrevealedTerrain: Terrain.DESERT).Should().Be(region.Length);
        Action mergedRun = () => MapMatches(world, [0xF4, 0xF4, 0xF4, 0xF4], hiddenPalace: rock,
            threeEyeRockUnrevealedTerrain: Terrain.DESERT);
        mergedRun.Should().Throw<InvalidOperationException>().WithMessage("flute-reveal-token:*");
        Action check = () => MapMatches(world, region);
        check.Should().Throw<InvalidOperationException>().WithMessage("map-terrain-mismatch:*");
    }

    [TestMethod]
    public void RequiredOracle_DoesNotTrustAllReachedLatchOrIgnoreDisabledTargets()
    {
        var world = new TestWorld(5, 5) { AllReached = true };
        var start = At(LocationID.WEST_NORTH_PALACE, 1, 1);
        var target = At(LocationID.WEST_GRASS, 3, 3);
        world.AddLocation(start); world.AddLocation(target);
        world.map[start.Pos] = world.map[target.Pos] = Terrain.GRASS;
        world.SetStart(start);
        world.UpdateVisit(RequirementTypeExtensions.ALL);
        Action reach = () => RequiredReachable([target]);
        reach.Should().Throw<InvalidOperationException>().WithMessage("required-unreachable:*");
        target.Disable();
        Action placement = () => Placements(world, [target]);
        placement.Should().Throw<InvalidOperationException>().WithMessage("required-disabled-or-outside:*");
    }

    [TestMethod]
    public void ReachabilityOracle_AcceptsAConnectedTarget()
    {
        var world = new TestWorld(3, 5, Terrain.GRASS);
        var start = At(LocationID.WEST_NORTH_PALACE, 1, 1);
        var target = At(LocationID.WEST_GRASS, 3, 1);
        world.AddLocation(start); world.AddLocation(target); world.SetStart(start);
        world.UpdateVisit(RequirementTypeExtensions.ALL);
        RequiredReachable([target]);
        Placements(world, [target]);
    }

    [TestMethod]
    public void ConnectorOracle_AcceptsShoreLandingsAndRejectsMissingApproach()
    {
        var world = new TestWorld(5, 9, Terrain.GRASS);
        for (int y = 0; y < 5; y++) world.map[y, 0] = Terrain.WATER;
        var raft = At(LocationID.WEST_RAFT_TO_EAST, 1, 2);
        world.map[raft.Pos] = Terrain.BRIDGE;
        ContinentConnector(world, raft, true);
        world.map[2, 2] = world.map[1, 1] = world.map[3, 1] = Terrain.MOUNTAIN;
        Action approach = () => ContinentConnector(world, raft, true);
        approach.Should().Throw<InvalidOperationException>().WithMessage("connector-approach:*");
        raft.Pos = new IntVector2(9, 2);
        Action bounds = () => ContinentConnector(world, raft, true);
        bounds.Should().Throw<InvalidOperationException>().WithMessage("connector-outside:*");
    }

    [TestMethod]
    public void ConnectorOracle_RequiresABridgeRouteToShore()
    {
        var world = new TestWorld(5, 9, Terrain.GRASS);
        var bridge = At(LocationID.WEST_BRIDGE_NORTH_OF_SARIA, 6, 2);
        world.map[2, 6] = world.map[2, 7] = Terrain.BRIDGE;
        Action route = () => ContinentConnector(world, bridge, false);
        route.Should().Throw<InvalidOperationException>().WithMessage("bridge-shore:*");
        world.map[2, 8] = Terrain.BRIDGE;
        ContinentConnector(world, bridge, false);
    }

    [TestMethod]
    public void HiddenOracle_RejectsSameColumnsOnlyWhenBothFeaturesAreEnabled()
    {
        var palace = At(LocationID.EAST_PALACE6, 3, 1);
        var kasuto = At(LocationID.EAST_TOWN_NEW_KASUTO, 3, 2);
        HiddenColumns(palace, kasuto, false);
        Action check = () => HiddenColumns(palace, kasuto, true);
        check.Should().Throw<InvalidOperationException>().WithMessage("hidden-columns:*");
        kasuto.Xpos = 4;
        HiddenColumns(palace, kasuto, true);
    }

    private static Location At(LocationID id, int x, int y) => new(id, y + 30, x, 0, id.GetContinent());
}
