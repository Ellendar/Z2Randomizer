using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using static Z2Randomizer.Tests.WorldCharacterizationTests;

namespace Z2Randomizer.Tests;

[TestClass]
public class GeneratedTerrainAttemptTests
{
    [TestMethod]
    public void ReopeningPlacementCannotUndoAReservationOrDisabledLocation()
    {
        var reserved = LocationAt(1);
        var disabled = LocationAt(2);
        var placed = LocationAt(3);
        var plan = new GeneratedLocationPlan([reserved, disabled, placed], []);
        using (plan.BeginAttempt())
        {
            reserved.ReserveForTerrainGeneration();
            disabled.Disable();
            placed.CanShuffle = false;
            foreach (var location in new[] { reserved, disabled, placed }) location.CanShuffle = true;
            reserved.CanShuffle.Should().BeFalse();
            disabled.CanShuffle.Should().BeFalse();
            disabled.YRaw.Should().Be(0);
            placed.CanShuffle.Should().BeTrue();
        }
        // Outside terrain generation the legacy property retains its original contract.
        reserved.CanShuffle = true;
        reserved.CanShuffle.Should().BeTrue();
        using (plan.BeginAttempt())
        {
            reserved.CanShuffle.Should().BeTrue();
            disabled.CanShuffle.Should().BeTrue();
        }
    }

    [TestMethod]
    public void FailedPlacementDoesNotMakeTheNextAttemptSkipLocations()
    {
        var world = new TestWorld(15, 15);
        world.UseClassicClimate();
        var first = LocationAt(1);
        var second = LocationAt(2);
        world.AddLocation(first);
        world.AddLocation(second);
        var plan = new GeneratedLocationPlan(world.AllLocations, []);
        using (plan.BeginAttempt())
        {
            world.PlaceOrdinaryLocations().Should().BeFalse("there are no empty placement tiles");
            first.Disable();
        }
        using (plan.BeginAttempt())
        {
            world.StartGeneratedMap();
            world.PlaceOrdinaryLocations().Should().BeTrue();
            foreach (var location in world.AllLocations)
            {
                location.IsEnabled().Should().BeTrue();
                world.map[location.Pos].Should().Be(Terrain.GRASS);
                location.CanShuffle.Should().BeFalse();
            }
            first.Pos.Should().NotBe(second.Pos);
        }
    }

    [TestMethod]
    public void RetryRestoresTerrainRequirementsAndEntranceMetadataButRetainsIdentityAndGameplayState()
    {
        var location = LocationAt(1);
        location.AccessRequirements = new Requirements([RequirementType.GLOVE]);
        location.IsExternalWorld = true;
        location.EntranceNumber = 2;
        location.Reachable = true;
        location.SetCollectables([Collectable.RAFT]);
        var plan = new GeneratedLocationPlan([location], [location]);
        using (plan.BeginAttempt())
        {
            location.AccessRequirements = location.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
            location.IsHidden = true;
            location.TerrainType = Terrain.FOREST;
            location.IsPassthrough = true;
            location.IsExternalWorld = false;
            location.EntranceNumber = 0;
            location.Disable();
        }
        using (plan.BeginAttempt())
        {
            location.CanShuffle.Should().BeTrue();
            location.IsEnabled().Should().BeFalse("a fresh generated attempt starts off-map but remains eligible for placement");
            location.TerrainType.Should().Be(Terrain.GRASS);
            location.IsPassthrough.Should().BeFalse();
            location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER).Should().BeFalse();
            location.IsHidden.Should().BeFalse();
            location.AccessRequirements.HasHardRequirement(RequirementType.GLOVE).Should().BeTrue();
            location.IsExternalWorld.Should().BeTrue();
            location.EntranceNumber.Should().Be(2);
            location.Reachable.Should().BeTrue();
            location.GetAllCollectables().Should().Equal(Collectable.RAFT);
            plan.CreateMinorLocations().Should().ContainSingle().Which.Should().BeSameAs(location);
            plan.CreateMinorLocations().Clear();
            plan.CreateMinorLocations().Should().ContainSingle();
        }
    }

    [TestMethod]
    public void CandidateResetDoesNotRewindRandomOrClearAllReached()
    {
        var world = new TestWorld(15, 15) { AllReached = true };
        var location = LocationAt(1);
        world.AddLocation(location);
        var profile = BiomeTerrainProfile.Resolve(Continent.WEST, Biome.VANILLALIKE, false,
            Climates.Create(Continent.WEST, ClimateEnum.CLASSIC), false);
        using (world.BeginGeneratedTerrainAttempt(profile))
        {
            location.Disable();
            world.NextRandom();
        }
        var control = new Z2Randomizer.RandomizerCore.Random(42);
        control.Next();
        using (world.BeginGeneratedTerrainAttempt(profile)) world.NextRandom().Should().Be(control.Next());
        world.AllReached.Should().BeTrue();
    }

    private static Location LocationAt(int x) => new(LocationID.WEST_GRASS, 32, x, 0, Continent.WEST)
        { TerrainType = Terrain.GRASS, AppearsOnMap = true };
}
