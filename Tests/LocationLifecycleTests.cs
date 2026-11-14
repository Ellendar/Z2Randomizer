using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class LocationLifecycleTests
{
    [TestMethod]
    [DataRow(RequirementType.GLOVE, 0x80, false)]
    [DataRow(RequirementType.HAMMER, 0x00, true)]
    [DataRow(RequirementType.FLUTE, 0x00, true)]
    public void DisablingPreservesContentAndPackedEntranceMetadata(RequirementType requirement, int areaByte, bool hidden)
    {
        var location = Entrance();
        location.AccessRequirements = new Requirements([requirement]);
        location.IsHidden = hidden;
        var requirements = location.AccessRequirements;
        location.SetCollectables([Collectable.RAFT]);

        location.Disable();

        // Raw Y and X are zero. External-world and entrance bits are packed into
        // those same bytes; disabled does not mean the entire record is zeroed.
        location.GetLocationBytes().Should().Equal((byte)areaByte, 0x80, 0x45, 0xE0);
        location.YRaw.Should().Be(0);
        location.IsEnabled().Should().BeFalse();
        location.IsHidden.Should().Be(hidden);
        location.CanShuffle.Should().BeFalse();
        location.AccessRequirements.Should().BeSameAs(requirements);
        location.GetAllCollectables().Should().Equal(Collectable.RAFT);
    }

    [TestMethod]
    public void RestoringCoordinatesCanReuseADisabledEntrance()
    {
        var location = Entrance();
        location.Disable();

        location.ResetCoords(isVanilla: true);
        location.CanShuffle = true;

        location.GetLocationBytes().Should().Equal(0xA8, 0x87, 0x45, 0xE0);
        location.IsEnabled().Should().BeTrue();
        location.CanShuffle.Should().BeTrue();
    }

    [TestMethod]
    public void GeneratedRetryCanPlaceADisabledEntranceAgain()
    {
        var world = new TestWorld(15, 15, Terrain.NONE);
        world.UseClassicClimate();
        var location = Entrance();
        location.IsPassthrough = false;
        world.AddLocation(location);
        var plan = new GeneratedLocationPlan(world.AllLocations, []);
        using (plan.BeginAttempt())
        {
            location.Disable();
            location.CanShuffle = true;
            location.CanShuffle.Should().BeFalse("a disabled location cannot reopen within its current attempt");
        }

        using (plan.BeginAttempt())
        {
            world.PlaceOrdinaryLocations().Should().BeTrue();

            location.YRaw.Should().BeGreaterThan(30);
            world.map[location.Pos].Should().Be(Terrain.GRASS);
            var bytes = location.GetLocationBytes();
            (bytes[0] & 0x80).Should().Be(0x80, "the retried entrance keeps its original external-world routing");
            (bytes[1] & 0xC0).Should().Be(0x80, "the retried entrance keeps its original entrance number");
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CandidateOutcomeControlsWhetherRemovalIsPublished(bool accept)
    {
        var world = new TestWorld(15, 15, Terrain.GRASS);
        var location = Entrance();
        world.AddLocation(location);
        var before = location.GetLocationBytes();
        byte[] removed;
        // Byte destination only; terrain candidates do not require a playable ROM.
        using (var candidate = world.BeginTerrainCandidate(new ROM(new byte[32])))
        {
            location.Disable();
            removed = location.GetLocationBytes();
            if (accept) candidate.Commit();
        }

        location.GetLocationBytes().Should().Equal(accept ? removed : before);
        location.CanShuffle.Should().Be(!accept);
        world.AllLocations.Should().ContainSingle().Which.Should().BeSameAs(location);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WorldRemovalAndMinorRemovalPreserveEntranceMetadataButHaveDifferentMembership(bool removeFromWorld)
    {
        var world = new TestWorld(15, 15, Terrain.GRASS);
        var location = Entrance();
        world.AddMinorLocation(location);

        if (removeFromWorld) world.RemoveWorldLocations(location);
        else world.PlaceHiddenLocations(LessImportantLocationsOption.REMOVE);

        location.GetLocationBytes().Should().Equal(0x80, 0x80, 0x45, 0xE0);
        location.CanShuffle.Should().BeFalse();
        world.AllLocations.Contains(location).Should().Be(!removeFromWorld);
        world.Locations[Terrain.GRASS].Contains(location).Should().Be(!removeFromWorld);
        world.RemovedLocations.Contains(location).Should().Be(removeFromWorld);
    }

    [TestMethod]
    public void MinorRemovalDoesNotAlterALocationThatCannotShuffle()
    {
        var world = new TestWorld(15, 15, Terrain.GRASS);
        var location = Entrance();
        location.CanShuffle = false;
        world.AddMinorLocation(location);
        var before = location.GetLocationBytes();

        world.PlaceHiddenLocations(LessImportantLocationsOption.REMOVE);

        location.GetLocationBytes().Should().Equal(before);
        world.AllLocations.Should().ContainSingle().Which.Should().BeSameAs(location);
        world.RemovedLocations.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(RequirementType.HAMMER)]
    [DataRow(RequirementType.FLUTE)]
    public void HiddenEntranceRemainsEnabledDespiteItsSuppressedRomCoordinate(RequirementType requirement)
    {
        var location = Entrance();
        location.AccessRequirements = new Requirements([requirement]);
        location.IsHidden = true;

        location.GetLocationBytes()[0].Should().Be(0);
        location.IsEnabled().Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void NonVanillaCoordinateResetLeavesLocationOffMapWithoutChangingOtherState(bool canShuffle, bool hidden)
    {
        var location = Entrance();
        location.CanShuffle = canShuffle;
        location.IsHidden = hidden;
        location.Reachable = true;
        location.SetCollectables([Collectable.RAFT]);

        location.ResetCoords(isVanilla: false);

        location.YRaw.Should().Be(0);
        location.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        location.IsEnabled().Should().BeFalse();
        location.CanShuffle.Should().Be(canShuffle, "resetting coordinates must not disable placement");
        location.IsHidden.Should().Be(hidden);
        location.Reachable.Should().BeTrue("coordinate reset does not clear cached gameplay visitation");
        location.GetAllCollectables().Should().Equal(Collectable.RAFT);
        location.GetLocationBytes().Should().Equal((byte)(hidden ? 0 : 0x80), 0x80, 0x45, 0xE0);
    }

    [TestMethod]
    [DataRow(RequirementType.GLOVE, false, false)]
    [DataRow(RequirementType.HAMMER, false, false)]
    [DataRow(RequirementType.FLUTE, false, false)]
    [DataRow(RequirementType.GLOVE, true, false)]
    [DataRow(RequirementType.HAMMER, true, false)]
    [DataRow(RequirementType.FLUTE, true, false)]
    [DataRow(RequirementType.GLOVE, false, true)]
    [DataRow(RequirementType.HAMMER, false, true)]
    [DataRow(RequirementType.FLUTE, false, true)]
    [DataRow(RequirementType.GLOVE, true, true)]
    [DataRow(RequirementType.HAMMER, true, true)]
    [DataRow(RequirementType.FLUTE, true, true)]
    public void EntranceVisibilityIsIndependentOfAccessRequirements(RequirementType requirement, bool hidden, bool external)
    {
        var location = Entrance();
        location.IsHidden = hidden;
        location.IsExternalWorld = external;
        location.AccessRequirements = new Requirements([requirement]);
        var position = location.Pos;
        location.SetCollectables([Collectable.RAFT]);

        location.GetLocationBytes().Should().Equal((byte)(hidden ? 0 : external ? 0xA8 : 40), 0x87, 0x45, 0xE0);
        location.Pos.Should().Be(position, "ROM suppression must not erase the revealed coordinates from logic");
        location.IsEnabled().Should().BeTrue();
        location.GetGettableItems(new HashSet<RequirementType>()).Should().BeEmpty();
        location.GetGettableItems(new HashSet<RequirementType> { requirement }).Should().Equal(Collectable.RAFT);
    }

    private static Location Entrance() => new(LocationID.WEST_GRASS, 40, 7, 5, Continent.WEST)
    {
        TerrainType = Terrain.GRASS,
        AppearsOnMap = true,
        IsExternalWorld = true,
        EntranceNumber = 2,
        MapPage = 1,
        IsFallInHole = true,
        IsPassthrough = true,
        ForceEnterRight = true
    };
}
