using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.RandomizerCore.Overworld;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Location
{
    Logger logger = LogManager.GetCurrentClassLogger();

    public LocationID ID { get; set; }

    private List<Collectable> NonPalaceTownCollectables { get; set; }
    public bool CollectablesAreShufflable { get; set; }
    public bool AppearsOnMap { get; set; }
    /// <summary>Suppresses the normal entrance in ROM until revealed; the model retains the revealed coordinates.</summary>
    public bool IsHidden { get => TerrainState.Hidden; set => TerrainState.Hidden = value; }
    public bool Reachable { get => TerrainState.Reachable; set => TerrainState.Reachable = value; }

    public Town? Town { get; set; }
    public Palace? Palace { get; set; }
    //public TownEnum? ActualTown { get; set; }
    public Continent Continent { get; set; }
    public Continent? VanillaContinent { get; set; }
    public Continent? ConnectedContinent { get; set; }

    public Terrain TerrainType { get => TerrainState.Terrain; set => TerrainState.Terrain = value; }

    //public byte[] LocationBytes { get; set; }

    public int MemAddress { get; set; }

    public int Map { get; set; }

    private LocationTerrainState committedTerrain = new();
    private LocationTerrainState? candidateTerrain;
    private LocationTerrainState TerrainState => candidateTerrain ?? committedTerrain;

    internal void BeginTerrainCandidate()
    {
        if (candidateTerrain is not null) throw new InvalidOperationException("Location already has a terrain candidate.");
        candidateTerrain = committedTerrain with { };
    }

    internal void RestartTerrainCandidate() => candidateTerrain = committedTerrain with { };

    internal void EndTerrainCandidate(bool accept)
    {
        if (accept) committedTerrain = candidateTerrain!;
        candidateTerrain = null;
    }

    /// int vector position starting at Y=0 for the top row
    public IntVector2 Pos
    {
        get => TerrainState.Position;
        set { TerrainState.Position = value; }
    }

    /// Y position starting at Y=0 for the top row
    public int Y
    {
        get => TerrainState.Position.Y;
        set { Pos = new(TerrainState.Position.X, value); }
    }
    /// Y position starting at Y=30 for the top row
    public int YRaw
    {
        get => Y + 30;
        set { Y = value - 30; }
    }

    public int Xpos
    {
        get => TerrainState.Position.X;
        set { Pos = new(value, TerrainState.Position.Y); }
    }

    /// Y,X position with Y starting at 30 for the top row
    public (int, int) CoordsY30Offset
    {
        get
        {
            return (YRaw, Xpos);
        }

        set
        {
            YRaw = value.Item1;
            Xpos = value.Item2;
        }
    }

    public (int, int) VanillaCoords { get; }

    //List of requirements to access this location. Required before the other requirements are evaluated,
    //and to pass through the location on the map, even if the collectable is not gettable.
    public Requirements AccessRequirements { get => TerrainState.Requirements; set => TerrainState.Requirements = value; }

    internal LocationPlacementState? PlacementState { get; set; }

    /// <summary>
    /// Legacy availability flag. Generated West/East/DM attempts adapt it to separate
    /// eligibility/progress state; vanilla shuffling and gameplay retain a plain flag.
    /// </summary>
    public bool CanShuffle
    {
        get => PlacementState?.CanPlace ?? TerrainState.CanShuffle;
        set
        {
            if (PlacementState is { } state) state.SetAvailable(value);
            else TerrainState.CanShuffle = value;
        }
    }

    internal void ReserveForTerrainGeneration() => PlacementState!.Reserve();

    internal void EndPlacementAttempt()
    {
        TerrainState.CanShuffle = CanShuffle;
        PlacementState = null;
    }

    /// <summary>
    /// Page you enter this location from, 0 means left. 1/2/3 will enter from the right on areas that have
    /// that many pages total or midscren on other areas.
    /// </summary>
    public int MapPageRaw { get; set; }
    public int MapPage { get => MapPageRaw >> 6; set => MapPageRaw = value << 6; }

    public bool IsExternalWorld { get => TerrainState.ExternalWorld; set => TerrainState.ExternalWorld = value; } //0:0
    public int EntranceNumber { get => TerrainState.EntranceNumber; set => TerrainState.EntranceNumber = value; } //1:0-1
    public bool IsFallInHole { get; set; }//3:0
    internal bool WasPassthrough { get; }
    public bool IsPassthrough { get => TerrainState.Passthrough; set => TerrainState.Passthrough = value; }//3:1
    public bool ForceEnterRight { get; set; }//3:2

    public string Name { get; set; }

    public const int CONNECTOR_BRIDGE_ID = 40;
    public const int CONNECTOR_RAFT_ID = 41;
    public const int CONNECTOR_CAVE1_ID = 42;
    public const int CONNECTOR_CAVE2_ID = 43;

    /*
    Byte 0

    .xxx xxxx - Y position
    x... .... - External to this world

    Byte 1 (offset 3F bytes from Byte 0)

    ..xx xxxx - X position
    xx.. .... - Entrance index. Subtract from overworld location index to get side-scrolling location index.

    Byte 2 (offset 7E bytes from Byte 0)

    ..xx xxxx - Map number
    xx.. .... - Horizontal position to enter within map
        0 = enter from the left
        1 = enter at x=256 or from the right for 2 screens maps
        2 = enter at x=512 or from the right for 3 screens maps
        3 = enter from the right for 4 screens maps

    Byte 3 (offset BD bytes from Byte 0)

    ...x xxxx - World number
    ..x. .... - Forced enter from the right edge of screen
    .x.. .... - Pass through
    x... .... - Fall in hole
    */
    public Location(LocationID lid, int yPos, int xPos, int map, Continent continent, bool isPassthrough = false)
    {
        ID = lid;
        Map = map;
        YRaw = yPos;
        Xpos = xPos;
        VanillaCoords = (Y, Xpos);
        MemAddress = lid.GetRomOffset();
        EntranceNumber = 0;
        CanShuffle = true;
        NonPalaceTownCollectables = [];
        Reachable = false;
        Palace = null;
        Continent = continent;
        VanillaContinent = lid.GetContinent();
        ConnectedContinent = null;
        AccessRequirements = Requirements.NONE;
        IsPassthrough = isPassthrough;
        WasPassthrough = isPassthrough;

        Name = ID.GetName() ?? "Unknown (" + Continent.GetName(Continent) + ")";
        if (Town != null)
        {
            Town.Type = ID.GetTown();
        }

        if (Name.StartsWith("Unknown") && Xpos != 0 && IsEnabled())
        {
            logger.Info("Missing location name on " + Continent.GetName(Continent) + " (" + Y + ", " + Xpos + ") Map: " + Map);
        }
        if (Name.Contains("FAKE"))
        {
            logger.Debug("Fake location encountered");
        }
    }

    public Location(Location clone)
    {
        ArgumentNullException.ThrowIfNull(clone);
        ID = clone.ID;
        NonPalaceTownCollectables = [.. clone.NonPalaceTownCollectables];
        CollectablesAreShufflable = clone.CollectablesAreShufflable;
        AppearsOnMap = clone.AppearsOnMap;
        Town = clone.Town;
        Palace = clone.Palace;
        Continent = clone.Continent;
        VanillaContinent = clone.VanillaContinent;
        ConnectedContinent = clone.ConnectedContinent;
        MemAddress = clone.MemAddress;
        Map = clone.Map;
        VanillaCoords = clone.VanillaCoords;
        MapPageRaw = clone.MapPageRaw;
        IsFallInHole = clone.IsFallInHole;
        WasPassthrough = clone.WasPassthrough;
        ForceEnterRight = clone.ForceEnterRight;
        Name = clone.Name;
        committedTerrain = clone.TerrainState with { CanShuffle = clone.CanShuffle };
    }

    public byte[] GetLocationBytes()
    {
        byte[] bytes = new byte[4];
        // Hidden locations are written with raw Y zero in the ROM. Logic keeps
        // their actual revealed coordinates. The hidden-location ASM and tables
        // supply the Y coordinate when the game reveals the entrance.
        if (IsHidden)
        {
            bytes[0] = 0;
        }
        else
        {
            bytes[0] = (byte)((IsExternalWorld ? 128 : 0) + YRaw);
        }
        bytes[1] = (byte)((EntranceNumber << 6) + Xpos);
        bytes[2] = (byte)(MapPageRaw + Map);
        bytes[3] = (byte)((IsFallInHole ? 128 : 0) + (IsPassthrough ? 64 : 0) + (ForceEnterRight ? 32 : 0) + GetWorld());
        return bytes;
    }

    public string GetDebuggerDisplay()
    {
        return Continent.ToString()
            + " " + TerrainType.ToString()
            + " " + Name
            + " (" + (Y) + "," + (Xpos) + ") _"
            + (Reachable ? "Reachable " : "Unreachable ")
            + '[' + string.Join(", ", GetAllCollectables().Select(i => i.ToString())) + ']';
    }

    /// <summary>
    /// Restores vanilla coordinates, or moves the location off-map (raw Y zero)
    /// for generated placement. Does not change placement availability or other state.
    /// </summary>
    public void ResetCoords(bool isVanilla)
    {
        if (isVanilla)
        {
            (Y, Xpos) = VanillaCoords;
        }
        else
        {
            Pos = IntVector2.NOT_ON_MAP;
        }
    }

    /// The TownNumber as used in game code
    public int GetTownNumber()
    {
        return ID switch
        {
            LocationID.WEST_TOWN_RAURU => 0,
            LocationID.WEST_TOWN_RUTO => 1,
            LocationID.WEST_TOWN_SARIA_NORTH => 2,
            LocationID.WEST_TOWN_MIDO => 3,
            LocationID.EAST_TOWN_NABOORU => 4,
            LocationID.EAST_TOWN_DARUNIA => 5,
            LocationID.EAST_TOWN_NEW_KASUTO => 6,
            LocationID.EAST_TOWN_OLD_KASUTO => 7,
            _ => throw new Exception("Not a town location"),
        };
    }

    public int GetWorld()
    {
        //Towns reference their banks
        if (Town?.Type != null)
        {
            return Continent == Continent.WEST ? 4 : 10;
        }
        //king's tomb - Get a better signal for this later
        if (ID == LocationID.WEST_KINGS_TOMB)
        {
            return 4;
        }
        //Connectors use the world bits to indicate which continent they take you to
        if (ConnectedContinent != null)
        {
            return ConnectedContinent switch
            {
                Continent.WEST => 0,
                Continent.DM => 1,
                Continent.EAST => 2,
                Continent.MAZE => 3,
                _ => throw new ImpossibleException("Invalid connected continent value")
            };
        }
        //Palaces... have world numbers that kind... of... make sense?
        //This is what they are.
        if (Palace != null)
        {
            return Palace.Number switch
            {
                1 => 0b00001100 + (int)Continent, //12,
                2 => 0b00001100 + (int)Continent, //12,
                3 => 0b00010000 + (int)Continent, //16,
                4 => 0b00010000 + (int)Continent, //19,
                5 => 0b00001100 + (int)Continent, //14,
                6 => 0b00010000 + (int)Continent, //18,
                7 => 0b00010100 + (int)Continent, //22,
                _ => throw new Exception("Invalid palace number in Location.GetWorld()")
            };
        }
        //Otherwise the world doesn't matter, so 0
        return 0;
    }

    /// <summary>
    /// Marks the location off-map and unavailable for shuffling, retaining its entrance metadata.
    /// The inactive marker is YRaw == 0.
    /// </summary>
    public void Disable()
    {
        YRaw = 0;
        Xpos = 0;
        CanShuffle = false;
        PlacementState?.Disable();
    }

    /// <summary>
    /// Tests the model's YRaw == 0 disabled marker, independently of ROM encoding.
    /// Does not establish placement, map bounds or reachability.
    /// </summary>
    public bool IsEnabled()
    {
        return YRaw != 0;
    }

    public List<Collectable> GetAllCollectables()
    {
        if (Palace != null)
        {
            // For palace locations, the collectables are always equal to the palaces' collectables
            return Palace.GetAllCollectables();
        }
        // Towns (Saria) may be split into multiple locations
        // Keeping previous logic for the rest
        return GetGettableItems(RequirementTypeExtensions.ALL);
    }

    public List<Collectable> GetShufflableCollectables()
    {
        if (Palace != null)
        {
            return Palace.GetShufflableCollectables();
        }
        return GetGettableItems(RequirementTypeExtensions.ALL, ForceEnterRight ? Direction.EAST : Direction.WEST, true);
    }

    public List<Collectable> GetGettableItems(IReadOnlySet<RequirementType> requireables, RandomizerProperties? props = null)
    {
        return GetGettableItems(requireables, ForceEnterRight ? Direction.EAST : Direction.WEST, false, props);
    }

    public List<Collectable> GetGettableItems(IReadOnlySet<RequirementType> requireables, Direction entranceDirection, bool shufflableItemsOnly = false, RandomizerProperties? props = null)
    {
        if (Town != null)
        {
            return Town.GetGettableItems(entranceDirection, requireables, shufflableItemsOnly);
        }
        if (Palace != null)
        {
            ArgumentNullException.ThrowIfNull(props, "Location.GetGettableItems for palace location must pass props");
            return Palace.GetGettableItems(props, requireables, shufflableItemsOnly);
        }
        if (shufflableItemsOnly && !CollectablesAreShufflable)
        {
            return [];
        }
        return AccessRequirements.AreSatisfiedBy(requireables) ? NonPalaceTownCollectables : [];
    }

    public void SetCollectables(IEnumerable<Collectable> collectables, bool shufflableOnly = false)
    {
        Debug.Assert(Town == null || Palace == null);
        if (Town != null)
        {
            Town.SetCollectables(collectables, shufflableOnly);
        }
        else if (Palace != null)
        {
            Palace.SetCollectables(collectables, shufflableOnly);
        }
        else if (!shufflableOnly || CollectablesAreShufflable)
        {
            NonPalaceTownCollectables = [.. collectables];
        }
    }

    /// <summary>
    /// Replaces an instance of a given collectable at a location with the indicated collectable.
    /// If random is specified, replaces a random indicated collectable at the location if multiple exist
    /// otherwise, replaces the "first" such collectable, where first is defined separately for each location type
    /// </summary>
    /// <param name="toReplace"></param>
    /// <param name="replacement"></param>
    /// <param name="r"></param>
    public bool ReplaceCollectable(Collectable toReplace, Collectable replacement, Random? r = null)
    {
        Debug.Assert(Town == null || Palace == null);
        if (Town != null)
        {
            return Town.ReplaceCollectable(toReplace, replacement, r);
        }
        else if (Palace != null)
        {
            return Palace.ReplaceCollectable(toReplace, replacement, r);
        }
        else if (NonPalaceTownCollectables.Contains(toReplace))
        {
            List<int> replacementIndexCandidates =
                Enumerable.Range(0, NonPalaceTownCollectables.Count)
                .Where(i => NonPalaceTownCollectables[i] == toReplace).ToList();
            if (replacementIndexCandidates.Count == 0)
            {
                return false;
            }
            if (r != null)
            {
                replacementIndexCandidates.FisherYatesShuffle(r);
            }
            NonPalaceTownCollectables[replacementIndexCandidates[0]] = replacement;
            return true;
        }
        return false;
    }

    public int GetCollectableCount(bool shufflableOnly = false)
    {
        if (Town != null)
        {
            return Town.TownMaps.Count(i => i.Collectable != null && (!shufflableOnly || i.CollectableIsShufflable));
        }
        else if (Palace != null)
        {
            return Palace.ItemRooms.Count;
        }
        return (CollectablesAreShufflable || !shufflableOnly) ? NonPalaceTownCollectables.Count : 0;
    }

}
