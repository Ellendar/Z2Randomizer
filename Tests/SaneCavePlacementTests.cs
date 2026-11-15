using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using static Z2Randomizer.Tests.WorldCharacterizationTests;

namespace Z2Randomizer.Tests;

[TestClass]
public class SaneCavePlacementTests
{
    [TestMethod]
    public void PairedEntrancesCanUseDifferentWalkableTerrains()
    {
        bool foundDifferentApproaches = false;
        for (int seed = 0; seed < 8; seed++)
        {
            var world = Fixture(Biome.VOLCANO, new(10, 10), new(22, 10));
            var (first, second) = Locations(world);
            world.SeedRandom(seed);

            world.PlaceCavePair(first, Terrain.MOUNTAIN).Should().BeTrue();

            var firstTerrain = IntVector2.CARDINALS.Select(d => world.map[first.Pos + d]).Single(t => t != Terrain.MOUNTAIN);
            var secondTerrain = IntVector2.CARDINALS.Select(d => world.map[second.Pos + d]).Single(t => t != Terrain.MOUNTAIN);
            firstTerrain.IsWalkable(true).Should().BeTrue();
            secondTerrain.IsWalkable(true).Should().BeTrue();
            foundDifferentApproaches |= firstTerrain != secondTerrain;
        }
        foundDifferentApproaches.Should().BeTrue("West/East choose terrain separately for each entrance, unlike DM's shared terrain");
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(false, 1)]
    [DataRow(true, 0)]
    [DataRow(true, 1)]
    public void PlacementPreservesSideviewOrderAndOpposingEntrances(bool vertical, int firstPage)
    {
        var a = new IntVector2(10, 10);
        var b = vertical ? new IntVector2(10, 22) : new IntVector2(22, 10);
        var world = Fixture(Biome.VANILLALIKE, a, b);
        var (first, second) = Locations(world, firstPage);
        GenerateWithinBudget(world, first, Terrain.MOUNTAIN).Should().BeTrue();
        new[] { first.Pos, second.Pos }.Should().BeEquivalentTo([a, b]);
        bool firstBeforeSecond = vertical ? first.Y < second.Y : first.Xpos < second.Xpos;
        firstBeforeSecond.Should().Be(firstPage == 0);
        first.CanShuffle.Should().BeFalse();
        second.CanShuffle.Should().BeFalse();
        var forward = vertical ? IntVector2.SOUTH : IntVector2.EAST;
        AssertEntrance(world, a, forward);
        AssertEntrance(world, b, -forward);
        first.MapPage.Should().Be(firstPage);
        second.MapPage.Should().Be(1 - firstPage);
        first.AccessRequirements.HasHardRequirement(RequirementType.FAIRY).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(Biome.VANILLALIKE, 6, true)]
    [DataRow(Biome.VANILLALIKE, 17, true)]
    [DataRow(Biome.VANILLALIKE, 5, false)]
    [DataRow(Biome.VANILLALIKE, 18, false)]
    [DataRow(Biome.ISLANDS, 10, true)]
    [DataRow(Biome.ISLANDS, 19, true)]
    [DataRow(Biome.ISLANDS, 9, false)]
    [DataRow(Biome.ISLANDS, 20, false)]
    [DataRow(Biome.CALDERA, 5, true)]
    [DataRow(Biome.CALDERA, 19, true)]
    [DataRow(Biome.CALDERA, 4, false)]
    [DataRow(Biome.CALDERA, 20, false)]
    public void PlacementRetainsBiomeSpacingLimits(Biome biome, int spacing, bool expected)
    {
        var world = Fixture(biome, new(10, 10), new(10 + spacing, 10));
        var (first, _) = Locations(world);
        GenerateWithinBudget(world, first, Terrain.MOUNTAIN).Should().Be(expected);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CrossingUsesTheExistingBoundingRectangleRatherThanAStraightLine(bool inside)
    {
        var world = Fixture(Biome.VANILLALIKE, new(10, 10), new(22, 13));
        var (first, _) = Locations(world);
        world.map[inside ? 12 : 14, 16] = Terrain.WATER;
        GenerateWithinBudget(world, first, Terrain.WATER).Should().Be(inside);
    }

    [TestMethod]
    [DataRow(Biome.VOLCANO, true)]
    [DataRow(Biome.VANILLALIKE, false)]
    public void OnlyVolcanoWaivesTheCrossingTerrainRequirement(Biome biome, bool expected)
    {
        var world = Fixture(biome, new(10, 10), new(22, 10));
        var (first, _) = Locations(world);
        GenerateWithinBudget(world, first, Terrain.WATER).Should().Be(expected);
    }

    [TestMethod]
    public void FailureDoesNotPaintTerrainOrMoveLocations()
    {
        var world = Fixture(Biome.VANILLALIKE, new IntVector2(10, 10));
        var (first, second) = Locations(world);
        var before = Tiles(world);
        var positions = new[] { first.Pos, second.Pos };
        world.PlaceCavePair(first, Terrain.MOUNTAIN).Should().BeFalse();
        Tiles(world).Should().Equal(before);
        new[] { first.Pos, second.Pos }.Should().Equal(positions);
        first.CanShuffle.Should().BeTrue();
        second.CanShuffle.Should().BeTrue();
    }

    [TestMethod]
    public void PartnerMayUseTheOuterMarginWhileTheOriginRetainsItsDirectionClearance()
    {
        var world = Fixture(Biome.VANILLALIKE, new(32, 10), new(48, 10));
        var (first, second) = Locations(world);
        GenerateWithinBudget(world, first, Terrain.MOUNTAIN).Should().BeTrue();
        new[] { first.Pos, second.Pos }.Should().Contain(new IntVector2(48, 10));

        var blockedDirection = new TestWorld(30, 30) { biome = Biome.VANILLALIKE };
        blockedDirection.UseClassicClimate();
        EmptyArea(blockedDirection, new(22, 10));
        EmptyArea(blockedDirection, new(28, 10));
        var (blockedFirst, _) = Locations(blockedDirection);
        GenerateWithinBudget(blockedDirection, blockedFirst, Terrain.MOUNTAIN).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(42)]
    public void AnUnmatchableStartingAreaCannotHideAnotherViablePair(int seed)
    {
        var world = Fixture(Biome.VANILLALIKE, new(7, 10), new(19, 10), new(35, 27));
        world.SeedRandom(seed);
        var (first, second) = Locations(world);
        world.PlaceCavePair(first, Terrain.MOUNTAIN).Should().BeTrue("the map has a compatible pair despite an isolated decoy area");
        new[] { first.Pos, second.Pos }.Should().BeEquivalentTo([new IntVector2(7, 10), new IntVector2(19, 10)]);
        world.map[27, 35].Should().Be(Terrain.NONE, "an unusable area must not be painted");
    }

    [TestMethod]
    [DataRow(3, 10, 3, true)]
    [DataRow(3, 10, 9, true)]
    [DataRow(3, 10, 10, false)]
    [DataRow(5, 12, 4, false)]
    [DataRow(5, 12, 5, true)]
    [DataRow(5, 12, 11, true)]
    [DataRow(5, 12, 12, false)]
    public void DeathMountainSpacingLimitsApplyWithoutRequiringACrossing(int minimum, int maximum, int spacing, bool fits)
    {
        var world = Fixture(Biome.CANYON, new(10, 10), new(10 + spacing, 10));
        var before = Tiles(world);
        for (int seed = 0; seed < 16; seed++)
        {
            var pair = SaneCavePlacement.Find(world.map, world.MapRows, world.MapColumns,
                new Z2Randomizer.RandomizerCore.Random(seed), minimum, maximum, null);
            pair.HasValue.Should().Be(fits);
            if (pair is { } placement)
                new[] { placement.First, placement.Second }.Should().BeEquivalentTo([new IntVector2(10, 10), new IntVector2(10 + spacing, 10)]);
        }
        Tiles(world).Should().Equal(before, "search must not paint even when it finds a pair");
    }

    [TestMethod]
    public void BackingMapPaddingCannotSupplyAnOutOfBoundsPartner()
    {
        var world = new TestWorld(40, 50) { biome = Biome.ISLANDS };
        world.UseClassicClimate();
        world.map = new OverworldMap(40, 64, Terrain.MOUNTAIN);
        EmptyArea(world, new(32, 10));
        // This partner is nineteen tiles away, valid island spacing, but lies
        // in backing storage beyond the world's logical right edge.
        EmptyArea(world, new(51, 10));
        var (first, _) = Locations(world);
        GenerateWithinBudget(world, first, Terrain.MOUNTAIN).Should().BeFalse();
    }

    private static TestWorld Fixture(Biome biome, params IntVector2[] centers)
    {
        var world = new TestWorld(40, 50) { biome = biome };
        world.UseClassicClimate();
        foreach (var pos in centers) EmptyArea(world, pos);
        return world;
    }

    private static (Location First, Location Second) Locations(TestWorld world, int firstPage = 0)
    {
        var first = new Location(LocationID.WEST_DM_ENTRANCE, 32, 2, 0, Continent.WEST)
            { TerrainType = Terrain.CAVE, MapPage = firstPage, AccessRequirements = new Requirements([RequirementType.FAIRY]) };
        var second = new Location(LocationID.WEST_DM_EXIT, 33, 3, 0, Continent.WEST)
            { TerrainType = Terrain.CAVE, MapPage = 1 - firstPage };
        world.AddCavePair(first, second);
        return (first, second);
    }

    private static bool GenerateWithinBudget(TestWorld world, Location first, Terrain crossing)
    {
        for (int attempt = 0; attempt < 100; attempt++) if (world.PlaceCavePair(first, crossing)) return true;
        return false;
    }

    private static void EmptyArea(TestWorld world, IntVector2 center)
    {
        world.map[center] = Terrain.NONE;
        foreach (var delta in IntVector2.DIRECTIONS) world.map[center + delta] = Terrain.NONE;
    }

    private static void AssertEntrance(TestWorld world, IntVector2 pos, IntVector2 forward)
    {
        world.map[pos].Should().Be(Terrain.CAVE);
        world.map[pos + forward].Should().Be(Terrain.MOUNTAIN);
        foreach (int lateral in new[] { -1, 0, 1 })
            world.map[pos - forward + lateral * forward.Perpendicular()].IsWalkable(true).Should().BeTrue();
    }

    private static Terrain[] Tiles(TestWorld world) => Enumerable.Range(0, world.MapRows)
        .SelectMany(y => Enumerable.Range(0, world.MapColumns).Select(x => world.map[y, x])).ToArray();
}
