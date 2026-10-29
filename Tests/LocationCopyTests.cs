using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class LocationCopyTests
{
    [TestMethod]
    public void CopyPreservesContentEntranceMetadataAndOriginalCoordinatesWithoutAliasingOwnedState()
    {
        var source = new Location(LocationID.EAST_TOWN_NEW_KASUTO, 40, 7, 12, Continent.EAST, true)
        {
            TerrainType = Terrain.FOREST, EntranceNumber = 2, IsExternalWorld = true,
            MapPage = 1, IsFallInHole = true, ForceEnterRight = true,
            ConnectedContinent = Continent.WEST, VanillaContinent = Continent.WEST,
            Name = "Copied entrance", AppearsOnMap = true, CollectablesAreShufflable = true,
            AccessRequirements = new Requirements([RequirementType.HAMMER]), Reachable = true, IsHidden = true, CanShuffle = false,
            MemAddress = 1234, Pos = new(9, 14), IsPassthrough = false
        };
        source.SetCollectables([Collectable.RAFT]);
        var copy = new Location(source);
        copy.GetLocationBytes().Should().Equal(source.GetLocationBytes());
        copy.GetAllCollectables().Should().Equal(Collectable.RAFT);
        copy.AccessRequirements.Should().BeSameAs(source.AccessRequirements);
        copy.Reachable.Should().BeTrue();
        copy.IsHidden.Should().BeTrue();
        copy.IsHidden = false;
        source.IsHidden.Should().BeTrue();
        copy.CanShuffle.Should().BeFalse();
        copy.AppearsOnMap.Should().BeTrue();
        copy.CollectablesAreShufflable.Should().BeTrue();
        copy.MemAddress.Should().Be(1234);
        copy.Name.Should().Be(source.Name);
        copy.Continent.Should().Be(source.Continent);
        copy.VanillaContinent.Should().Be(source.VanillaContinent);
        copy.ConnectedContinent.Should().Be(source.ConnectedContinent);
        copy.WasPassthrough.Should().BeTrue();
        copy.IsPassthrough.Should().BeFalse();
        copy.ResetCoords(true);
        copy.Pos.Should().Be(new IntVector2(7, 10));
        copy.Disable();
        copy.ReplaceCollectable(Collectable.RAFT, Collectable.BOOTS).Should().BeTrue();
        source.Pos.Should().Be(new IntVector2(9, 14));
        source.GetAllCollectables().Should().Equal(Collectable.RAFT);
    }

    [TestMethod]
    public void CopyOfAnActiveCandidateKeepsCurrentValuesWithoutSharingItsTransactionOrPlacementAdapter()
    {
        var source = new Location(LocationID.WEST_GRASS, 40, 7, 0, Continent.WEST);
        source.BeginTerrainCandidate();
        source.Pos = new(9, 14);
        source.IsHidden = true;
        source.PlacementState = new LocationPlacementState();
        source.ReserveForTerrainGeneration();
        var copy = new Location(source);
        source.EndPlacementAttempt();
        source.EndTerrainCandidate(false);
        copy.Pos.Should().Be(new IntVector2(9, 14));
        copy.IsHidden.Should().BeTrue();
        source.IsHidden.Should().BeFalse();
        copy.CanShuffle.Should().BeFalse();
        copy.PlacementState.Should().BeNull();
        copy.BeginTerrainCandidate();
        copy.Disable();
        copy.EndTerrainCandidate(false);
        copy.Pos.Should().Be(new IntVector2(9, 14));
        copy.IsHidden.Should().BeTrue();
        source.IsHidden.Should().BeFalse();
        source.Pos.Should().Be(new IntVector2(7, 10));
    }
}
