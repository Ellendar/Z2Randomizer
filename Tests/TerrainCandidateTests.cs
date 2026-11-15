using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using static Z2Randomizer.Tests.WorldCharacterizationTests;

namespace Z2Randomizer.Tests;

[TestClass]
public class TerrainCandidateTests
{
    [TestMethod]
    public void RejectionDiscardsLocationEditsRepairsAndRomWrites()
    {
        var world = new TestWorld(3, 5, Terrain.GRASS);
        var location = Location();
        world.AddLocation(location);
        var originalMap = world.map;
        var requirements = location.AccessRequirements;
        var destination = Destination();
        using (var candidate = world.BeginTerrainCandidate(destination))
        {
            location.Pos = new(3, 2);
            location.TerrainType = Terrain.FOREST;
            location.AccessRequirements = requirements.WithHardRequirement(RequirementType.HAMMER);
            location.Reachable = true;
            location.IsHidden = true;
            location.IsExternalWorld = false;
            location.EntranceNumber = 0;
            location.Disable();
            world.map[1, 2] = Terrain.ROAD;
            world.MarkTileVisited(new(2, 1));
            candidate.Writes.Put(4, 0x11);
        }
        world.map.Should().BeSameAs(originalMap);
        world.map[1, 2].Should().Be(Terrain.GRASS);
        world.TileVisited(new(2, 1)).Should().BeFalse();
        location.Pos.Should().Be(new IntVector2(1, 1));
        location.TerrainType.Should().Be(Terrain.GRASS);
        location.AccessRequirements.Should().BeSameAs(requirements);
        location.IsExternalWorld.Should().BeTrue();
        location.EntranceNumber.Should().Be(2);
        location.CanShuffle.Should().BeTrue();
        location.Reachable.Should().BeFalse();
        location.IsHidden.Should().BeFalse();
        destination.rawdata.Should().OnlyContain(value => value == 0xCC);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CandidateOutcomeControlsThePlacementFilterUsedByLaterPlacement(bool accept)
    {
        var world = new TestWorld(7, 7, Terrain.NONE);
        var location = Location();
        location.TerrainType = Terrain.PALACE;
        location.AppearsOnMap = true;
        world.AddLocation(location);
        var climate = new Climate("Candidate placement filter", new(),
            new TableWeightedRandom<Terrain>(new[] { (Terrain.GRASS, 1), (Terrain.FOREST, 1) }), 1);
        world.UsePlacementClimate(climate, new[] { Terrain.GRASS });
        using (var candidate = world.BeginTerrainCandidate(Destination()))
        {
            world.KeepOnlyPlacementTerrain(Terrain.FOREST);
            if (accept) candidate.Commit();
        }

        world.PlaceOrdinaryLocations().Should().BeTrue();

        world.map[location.Pos].Should().Be(Terrain.PALACE);
        foreach (var offset in IntVector2.DIRECTIONS)
            world.map[location.Pos + offset].Should().Be(accept ? Terrain.FOREST : Terrain.GRASS,
                "later placement must use the committed filter or the restored original");
    }

    [TestMethod]
    public void AcceptedCandidateRetainsIdentityEntranceMetadataAndRequirements()
    {
        var world = new TestWorld(3, 5, Terrain.GRASS);
        var location = Location();
        world.AddLocation(location);
        location.SetCollectables([Collectable.RAFT]);
        var destination = Destination();
        using (var candidate = world.BeginTerrainCandidate(destination))
        {
            location.Pos = new(3, 2);
            location.CanShuffle = false;
            location.AccessRequirements = location.AccessRequirements.WithHardRequirement(RequirementType.FLUTE);
            location.IsHidden = true;
            world.map[2, 3] = Terrain.PALACE;
            candidate.Writes.Put(4, 0x11);
            candidate.Commit();
        }
        world.AllLocations.Should().ContainSingle().Which.Should().BeSameAs(location);
        location.Pos.Should().Be(new IntVector2(3, 2));
        location.CanShuffle.Should().BeFalse();
        location.AccessRequirements.HasHardRequirement(RequirementType.FLUTE).Should().BeTrue();
        location.IsHidden.Should().BeTrue();
        location.GetAllCollectables().Should().Equal(Collectable.RAFT);
        location.Map.Should().Be(12);
        location.MapPage.Should().Be(1);
        location.EntranceNumber.Should().Be(2);
        location.ConnectedContinent.Should().Be(Continent.EAST);
        world.map[location.Pos].Should().Be(Terrain.PALACE);
        destination.rawdata[4].Should().Be(0x11);
        destination.rawdata.Where((_, i) => i != 4).Should().OnlyContain(value => value == 0xCC);
    }

    [TestMethod]
    public void InvalidDestinationCannotPartiallyCommitEarlierWritesOrLocations()
    {
        var world = new TestWorld(3, 5, Terrain.GRASS);
        var location = Location();
        world.AddLocation(location);
        var destination = Destination();
        using (var candidate = world.BeginTerrainCandidate(destination))
        {
            location.Pos = new(3, 2);
            candidate.Writes.Put(0, 0x11);
            candidate.Writes.Put(32, 0x22);
            Action commit = candidate.Commit;
            commit.Should().Throw<ArgumentOutOfRangeException>();
        }
        location.Pos.Should().Be(new IntVector2(1, 1));
        destination.rawdata.Should().OnlyContain(value => value == 0xCC);
    }

    [TestMethod]
    public void BudgetCheckAndWriteUseTheSameImmutableEncoding()
    {
        var map = new OverworldMap(1, 64, Terrain.GRASS);
        var encoded = new EncodedWorldMap(map);
        var patch = new TerrainRomPatch();
        var destination = Destination();
        map[0, 0] = Terrain.FOREST;
        encoded.TryStage(patch, 2, encoded.Length + 2).Should().BeTrue();
        patch.Apply(destination);
        destination.GetBytes(0, 9).Should().Equal(0xCC, 0xCC, 0xF5, 0xF5, 0xF5, 0xF5, 0x0B, 0x0B, 0xCC);
    }

    [TestMethod]
    public void OversizedMapDoesNotStageAnyTerrainOrFiller()
    {
        var encoded = new EncodedWorldMap(new OverworldMap(1, 64, Terrain.GRASS));
        var patch = new TerrainRomPatch();
        var destination = Destination();
        patch.Put(12, 0x11);
        encoded.TryStage(patch, 0, encoded.Length - 1).Should().BeFalse();
        patch.Apply(destination);
        destination.rawdata[12].Should().Be(0x11);
        destination.rawdata.Where((_, i) => i != 12).Should().OnlyContain(value => value == 0xCC);
    }

    [TestMethod]
    public void NextCandidateCannotCommitHiddenWritesFromAnOversizedAttempt()
    {
        var world = new TestWorld(1, 64, Terrain.GRASS);
        var location = Location();
        world.AddLocation(location);
        var destination = Destination();
        var profile = BiomeTerrainProfile.Resolve(Continent.WEST, Biome.VANILLALIKE, false,
            Climates.Create(Continent.WEST, ClimateEnum.CLASSIC), false);
        using (var candidate = world.BeginTerrainCandidate(destination))
        {
            using (world.BeginGeneratedTerrainAttempt(profile))
            {
                location.AccessRequirements = Requirements.NONE.WithHardRequirement(RequirementType.HAMMER);
                location.Reachable = true;
            location.IsHidden = true;
                candidate.Writes.Put(20, 0x11);
                var encoded = new EncodedWorldMap(world.map);
                encoded.TryStage(candidate.Writes, 0, 3).Should().BeFalse();
            }
            using (world.BeginGeneratedTerrainAttempt(profile))
            {
                location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER).Should().BeFalse();
                location.Reachable.Should().BeFalse();
        location.IsHidden.Should().BeFalse();
                candidate.Writes.Put(21, 0x22);
            }
            candidate.Commit();
        }
        destination.rawdata[20].Should().Be(0xCC);
        destination.rawdata[21].Should().Be(0x22);
    }

    private static Location Location() => new(LocationID.WEST_GRASS, 31, 1, 12, Continent.WEST)
    {
        TerrainType = Terrain.GRASS, IsExternalWorld = true, EntranceNumber = 2,
        MapPage = 1, ConnectedContinent = Continent.EAST
    };

    // Byte destination only; this is not a fabricated playable ROM.
    private static ROM Destination() => new(Enumerable.Repeat((byte)0xCC, 32).ToArray());
}
