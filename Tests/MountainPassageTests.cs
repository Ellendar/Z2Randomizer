using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class MountainPassageTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void LongPassagesHaveVisibleFunnelsAndDoubleNecks(bool vertical)
    {
        const int length = 11;
        bool sawFlaredFunnel = false;
        bool sawDoubleNeckWithShiftingShoulder = false;

        for (int seed = 0; seed < 32; seed++)
        {
            var world = MountainBand(vertical, length);
            world.SeedRandom(seed);

            world.PlaceCrossings(MountainCrossing).PlacedCrossings.Should().Be(1);

            var rows = Enumerable.Range(4, length)
                .Select(along => Enumerable.Range(0, 13)
                    .Where(across => world.map[Cell(along, across, vertical)] == Terrain.FOREST)
                    .ToArray())
                .ToArray();
            var widths = rows.Select(row => row.Length).ToArray();
            widths.Should().OnlyContain(width => width >= 1 && width <= 3);
            widths.Should().Contain(2);
            bool narrowStart = widths.Take(2).All(width => width == 1);
            bool narrowEnd = widths.TakeLast(2).All(width => width == 1);
            (narrowStart || narrowEnd).Should().BeTrue("a long neck should span more than one tile");

            if (narrowStart && narrowEnd && widths.Contains(3))
            {
                var firstShoulder = rows.First(row => row.Length == 2);
                var lastShoulder = rows.Last(row => row.Length == 2);
                firstShoulder.Should().NotEqual(lastShoulder);
                sawDoubleNeckWithShiftingShoulder = true;
            }
            if (narrowStart ^ narrowEnd)
            {
                int[] neckToMouth = narrowStart ? widths : widths.Reverse().ToArray();
                if (neckToMouth[^1] == 3)
                {
                    neckToMouth.Should().BeInAscendingOrder();
                    sawFlaredFunnel = true;
                }
            }
        }

        sawFlaredFunnel.Should().BeTrue();
        sawDoubleNeckWithShiftingShoulder.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(Terrain.ROCK)]
    [DataRow(Terrain.RIVER_DEVIL)]
    public void ARequiredBlockerCreatesAnHourglassWithOpenBanks(Terrain blocker)
    {
        var world = MountainBand(vertical: false, length: 7);
        world.SeedRandom(0);
        var options = MountainCrossing with
        {
            RockBlocker = blocker == Terrain.ROCK,
            RiverDevilBlocker = blocker == Terrain.RIVER_DEVIL
        };

        world.PlaceCrossings(options).PlacedCrossings.Should().Be(1);

        int midpointRow = Enumerable.Range(1, 11).Single(y => world.map[y, 7] == blocker);
        world.map[midpointRow - 1, 7].Should().Be(Terrain.MOUNTAIN);
        world.map[midpointRow + 1, 7].Should().Be(Terrain.MOUNTAIN);

        for (int x = 4; x <= 10; x++)
        {
            world.map[midpointRow, x].Should().Be(x == 7 ? blocker : Terrain.FOREST);
        }

        var widths = Enumerable.Range(4, 7).Select(x =>
            1 + (world.map[midpointRow - 1, x] == Terrain.FOREST ? 1 : 0)
            + (world.map[midpointRow + 1, x] == Terrain.FOREST ? 1 : 0));
        widths.Should().Equal(3, 2, 2, 1, 2, 2, 3);
    }

    [TestMethod]
    public void DaruniaWidensBetweenItsTwoEncounterNecks()
    {
        var world = MountainBand(vertical: false, length: 11);
        world.SeedRandom(0);
        var first = AddDaruniaEncounter(world, LocationID.EAST_TRAP_DESERT1);
        var second = AddDaruniaEncounter(world, LocationID.EAST_TRAP_DESERT2);

        var result = world.PlaceCrossings(MountainCrossing with { Features = CrossingFeatures.DaruniaEncounters });

        result.PlacedFeatures.Should().Be(CrossingFeatures.DaruniaEncounters);
        first.Y.Should().Be(second.Y);
        first.Xpos.Should().Be(14);
        second.Xpos.Should().Be(4);
        foreach (var encounter in new[] { first, second })
        {
            world.map[encounter.Pos].Should().Be(Terrain.DESERT);
            world.map[encounter.Pos + IntVector2.NORTH].Should().Be(Terrain.MOUNTAIN);
            world.map[encounter.Pos + IntVector2.SOUTH].Should().Be(Terrain.MOUNTAIN);
        }

        int row = first.Pos.Y;
        var widths = Enumerable.Range(4, 11).Select(x =>
            Enumerable.Range(row - 1, 3).Count(y => world.map[y, x] == Terrain.DESERT)).ToArray();
        widths.Take(2).Should().Equal(1, 1);
        widths.TakeLast(2).Should().Equal(1, 1);
        widths.Should().Contain(3);
    }

    private static Location AddDaruniaEncounter(TestWorld world, LocationID id)
    {
        var location = new Location(id, 30, 0, 0, Continent.EAST)
        {
            Pos = IntVector2.NOT_ON_MAP,
            TerrainType = Terrain.DESERT,
            CanShuffle = true
        };
        world.AddLocation(location);
        return location;
    }

    [TestMethod]
    public void AnObstructionShortensTheSecondLaneWithoutPreventingWidening()
    {
        var world = SingleMountainPass();

        // Either side would open into another area if widened at column 5.
        world.map[2, 5] = Terrain.FOREST;
        world.map[6, 5] = Terrain.FOREST;

        world.PlaceCrossings(MountainCrossing).PlacedCrossings.Should().Be(1);

        for (int x = 4; x <= 10; x++)
        {
            world.map[4, x].Should().Be(Terrain.FOREST);
        }
        world.map[3, 5].Should().Be(Terrain.MOUNTAIN);
        world.map[5, 5].Should().Be(Terrain.MOUNTAIN);
        bool hasLaneAbove = world.map[3, 6] == Terrain.FOREST && world.map[3, 7] == Terrain.FOREST;
        bool hasLaneBelow = world.map[5, 6] == Terrain.FOREST && world.map[5, 7] == Terrain.FOREST;
        (hasLaneAbove || hasLaneBelow).Should().BeTrue();
    }

    [TestMethod]
    public void ALongCandidateWithNoSafeSecondLaneIsRejectedBeforePainting()
    {
        var world = SingleMountainPass();
        for (int x = 4; x <= 10; x++)
        {
            world.map[2, x] = Terrain.WALKABLEWATER;
            world.map[6, x] = Terrain.WALKABLEWATER;
        }

        world.PlaceCrossings(MountainCrossing).PlacedCrossings.Should().Be(0);

        for (int y = 3; y <= 5; y++)
        {
            for (int x = 4; x <= 10; x++)
            {
                world.map[y, x].Should().Be(Terrain.MOUNTAIN);
            }
        }
    }

    private static TestWorld SingleMountainPass()
    {
        // Only the center row has a bank tile at each end of the span.
        var world = new TestWorld(9, 15);
        for (int y = 3; y <= 5; y++)
        {
            world.map[y, 2] = Terrain.FOREST;
            world.map[y, 12] = Terrain.FOREST;
        }
        world.map[4, 3] = Terrain.FOREST;
        world.map[4, 11] = Terrain.FOREST;
        return world;
    }

    private static RegionCrossingOptions MountainCrossing =>
        SimpleCrossingTests.Options(Biome.MOUNTAINOUS, Terrain.MOUNTAIN, boots: true, maximum: 1);

    private static TestWorld MountainBand(bool vertical, int length = 5)
    {
        int alongSize = length + 8;
        var world = vertical ? new TestWorld(alongSize, 13) : new TestWorld(13, alongSize);
        for (int along = 1; along < alongSize - 1; along++)
        {
            if (along >= 4 && along < 4 + length) continue;
            for (int across = 1; across <= 11; across++)
            {
                world.map[Cell(along, across, vertical)] = Terrain.FOREST;
            }
        }
        return world;
    }

    private static IntVector2 Cell(int along, int across, bool vertical) =>
        vertical ? new IntVector2(across, along) : new IntVector2(along, across);
}
