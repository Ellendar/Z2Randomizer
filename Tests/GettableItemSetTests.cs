using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.Tests;

[TestClass]
public class GettableItemSetTests
{
    // These cache fixtures have no generated key layout. Keep the traversal-only mode explicit.
    private readonly RandomizerProperties props = new() { KeyLogic = KeyLogicOption.FAIRY_OR_MAGICKEY };

    [TestMethod]
    public void Reset_RestoresStartingItemsAndClearsUntrackedItems()
    {
        GettableItemSet set = new GettableItemSet.Builder()
            .Excluding(Collectable.DASH_SPELL, Collectable.TANOOKI)
            .StartingWith([Collectable.GLOVE])
            .Build();

        set.Tracks(Collectable.CANDLE).Should().BeTrue();
        set.Tracks(Collectable.DASH_SPELL).Should().BeFalse();
        set.Tracks(Collectable.TANOOKI).Should().BeFalse();

        set[Collectable.CANDLE] = true;
        set[Collectable.HEART_CONTAINER] = true;

        set.Reset();

        set[Collectable.CANDLE].Should().BeFalse();
        set[Collectable.HEART_CONTAINER].Should().BeFalse();
        set[Collectable.GLOVE].Should().BeTrue();
    }

    [TestMethod]
    public void Reset_RestoresSnapshotOfStartingItems()
    {
        List<Collectable> startingItems = [Collectable.GLOVE];
        GettableItemSet set = new GettableItemSet.Builder()
            .Excluding()
            .StartingWith(startingItems)
            .Build();

        startingItems.Clear();
        startingItems.Add(Collectable.CANDLE);
        set.Reset();

        set[Collectable.GLOVE].Should().BeTrue();
        set[Collectable.CANDLE].Should().BeFalse();
    }

    [TestMethod]
    public void GetGettableItemsAt_SolvedPalaceIsCachedUntilReset()
    {
        Palace palace = BuildPalace((Collectable.CANDLE, [RequirementType.GLOVE]));
        Location location = new(LocationID.WEST_PALACE1, 0, 0, 0, Continent.WEST)
        {
            Palace = palace
        };
        GettableItemSet set = new GettableItemSet.Builder().Build();
        HashSet<RequirementType> requireables = [RequirementType.GLOVE];

        IReadOnlyList<Collectable> cached = set.GetGettableItemsAt(props, location, requireables);
        cached.Should().Equal(Collectable.CANDLE);
        requireables.Add(RequirementType.JUMP);
        set.GetGettableItemsAt(props, location, requireables).Should().BeSameAs(cached);

        set.Reset();
        set.GetGettableItemsAt(props, location, requireables).Should().NotBeSameAs(cached);

        palace.ItemRooms[0].Collectable = Collectable.RAFT;
        set.Reset();
        requireables.Clear();
        set.GetGettableItemsAt(props, location, requireables).Should().BeEmpty();
        requireables.Add(RequirementType.GLOVE);
        set.GetGettableItemsAt(props, location, requireables).Should().Equal(Collectable.RAFT);
    }

    [TestMethod]
    public void GetGettableItemsAt_IncompletePalaceIsCachedForMatchingRequireables()
    {
        Palace palace = BuildPalace(
            (Collectable.CANDLE, []),
            (Collectable.RAFT, [RequirementType.GLOVE]));
        Location location = new(LocationID.WEST_PALACE1, 0, 0, 0, Continent.WEST)
        {
            Palace = palace
        };
        GettableItemSet set = new GettableItemSet.Builder().Build();
        IReadOnlySet<RequirementType> withGlove = new HashSet<RequirementType> { RequirementType.GLOVE };
        IReadOnlySet<RequirementType> withoutGlove = new HashSet<RequirementType>();

        IReadOnlyList<Collectable> cached = set.GetGettableItemsAt(props, location, withoutGlove);
        cached.Should().Equal(Collectable.CANDLE);
        set.GetGettableItemsAt(props, location, new HashSet<RequirementType>()).Should().BeSameAs(cached);

        // More requireables must invalidate the partial result.
        set.GetGettableItemsAt(props, location, withGlove).Should().Equal(Collectable.CANDLE, Collectable.RAFT);
    }

    [TestMethod]
    public void Builder_BuildsIndependentSetsWithFrozenConfiguration()
    {
        List<Collectable> exclusions = [Collectable.DASH_SPELL];
        Collectable[] startingItems = [Collectable.GLOVE, Collectable.DASH_SPELL];
        GettableItemSet.Builder builder = new GettableItemSet.Builder().Excluding(exclusions)
            .StartingWith(startingItems);

        exclusions[0] = Collectable.CANDLE;
        startingItems[0] = Collectable.CANDLE;
        GettableItemSet first = builder.Build();
        GettableItemSet second = builder.Build();
        builder.StartingWith([Collectable.RAFT]);
        GettableItemSet third = builder.Build();

        first[Collectable.CANDLE] = true;
        second[Collectable.CANDLE].Should().BeFalse();
        second.Tracks(Collectable.CANDLE).Should().BeTrue();
        second.Tracks(Collectable.DASH_SPELL).Should().BeFalse();
        second[Collectable.DASH_SPELL].Should().BeFalse();

        first.Reset();
        first[Collectable.CANDLE].Should().BeFalse();
        first[Collectable.GLOVE].Should().BeTrue();
        second[Collectable.GLOVE].Should().BeTrue();
        third[Collectable.GLOVE].Should().BeFalse();
        third[Collectable.RAFT].Should().BeTrue();
    }

    [TestMethod]
    public void Builder_ExcludingAfterStartingWithFiltersStartingItems()
    {
        GettableItemSet set = new GettableItemSet.Builder()
            .StartingWith([Collectable.GLOVE, Collectable.CANDLE])
            .Excluding(Collectable.GLOVE)
            .Build();

        set.Tracks(Collectable.GLOVE).Should().BeFalse();
        set[Collectable.GLOVE].Should().BeFalse();
        set[Collectable.CANDLE].Should().BeTrue();
        set.Reset();
        set[Collectable.GLOVE].Should().BeFalse();
        set[Collectable.CANDLE].Should().BeTrue();
    }

    [TestMethod]
    public void GetGettableItemsAt_IncompleteCacheSnapshotsGrowingRequireables()
    {
        Palace palace = BuildPalace(
            (Collectable.CANDLE, [RequirementType.GLOVE]),
            (Collectable.RAFT, [RequirementType.HAMMER]));
        Location location = new(LocationID.WEST_PALACE1, 0, 0, 0, Continent.WEST)
        {
            Palace = palace
        };
        GettableItemSet set = new GettableItemSet.Builder().Build();
        HashSet<RequirementType> requireables = [];

        IReadOnlyList<Collectable> empty = set.GetGettableItemsAt(props, location, requireables);
        empty.Should().BeEmpty();
        set.GetGettableItemsAt(props, location, requireables).Should().BeSameAs(empty);

        requireables.Add(RequirementType.GLOVE);
        IReadOnlyList<Collectable> partial = set.GetGettableItemsAt(props, location, requireables);
        partial.Should().Equal(Collectable.CANDLE);
        set.GetGettableItemsAt(props, location, new HashSet<RequirementType> { RequirementType.GLOVE })
            .Should().BeSameAs(partial);

        requireables.Add(RequirementType.JUMP);
        IReadOnlyList<Collectable> retried = set.GetGettableItemsAt(props, location, requireables);
        retried.Should().Equal(Collectable.CANDLE);
        retried.Should().NotBeSameAs(partial);
        set.GetGettableItemsAt(props, location, new HashSet<RequirementType> { RequirementType.JUMP, RequirementType.GLOVE })
            .Should().BeSameAs(retried);

        requireables.Add(RequirementType.HAMMER);
        IReadOnlyList<Collectable> solved = set.GetGettableItemsAt(props, location, requireables);
        solved.Should().Equal(Collectable.CANDLE, Collectable.RAFT);
        requireables.Add(RequirementType.BOOTS);
        set.GetGettableItemsAt(props, location, requireables).Should().BeSameAs(solved);
    }

    [TestMethod]
    public void GetGettableItemsAt_PalaceWithOnlyMinorItemsIsSkipped()
    {
        Palace palace = BuildPalace((Collectable.SMALL_BAG, []));
        Location location = new(LocationID.WEST_PALACE1, 0, 0, 0, Continent.WEST)
        {
            Palace = palace
        };
        GettableItemSet set = new GettableItemSet.Builder().Build();
        set.GetGettableItemsAt(props, location, new HashSet<RequirementType>()).Should().BeEmpty();
    }

    private static Palace BuildPalace(
        params (Collectable Item, RequirementType[] Requirements)[] itemRooms)
    {
        Palace palace = new(1, false);
        Room entrance = BuildRoom("Entrance", isEntrance: true);
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        for (int i = 0; i < itemRooms.Length; i++)
        {
            Room itemRoom = BuildRoom($"Item{i}");
            itemRoom.Collectable = itemRooms[i].Item;
            itemRoom.LinkRequirements = new Requirements(itemRooms[i].Requirements);
            palace.AllRooms.Add(itemRoom);
            palace.ItemRooms.Add(itemRoom);

            switch (i)
            {
                case 0:
                    entrance.Right = itemRoom;
                    itemRoom.Left = entrance;
                    break;
                case 1:
                    entrance.Up = itemRoom;
                    itemRoom.Down = entrance;
                    break;
                default:
                    entrance.Left = itemRoom;
                    itemRoom.Right = entrance;
                    break;
            }
        }

        return palace;
    }

    private static Room BuildRoom(string name, bool isEntrance = false)
    {
        return new Room
        {
            Name = name,
            Group = RoomGroup.VANILLA,
            Enabled = true,
            IsEntrance = isEntrance,
            SideView = [0x04, 0x60, 0x00, 0x08],
            Enemies = [0x01],
            ItemPresenceBits = [0x0F],
            Connections = [0xFC, 0xFC, 0xFC, 0xFC],
        };
    }
}
