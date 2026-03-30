using NLog;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Location
{
    Logger logger = LogManager.GetCurrentClassLogger();

    public LocationID ID { get; set; }

    public int EntranceNumber { get; set; }

    public List<Collectable> Collectables { get; set; }
    public Collectable VanillaCollectable { get; set; }
    public bool AppearsOnMap { get; set; }

    public Terrain TerrainType { get; set; }

    //public byte[] LocationBytes { get; set; }
    public List<Location> Children { get; set; } = [];

    public int MemAddress { get; set; }

    //This is really stupidly implemented. It _should_ be a boolean, which then gets written to the appropriate bit on the
    //encounter data when it's written to the ROM. Instead, this has the value 0 if the area is not a passthrough
    //and 64 if the area is a passthrough. This shouldn't be too hard to refactor, but I am lazy right now.
    //Also probably refactoring this should be a part of removing the LocationBytes structure alltogether, so we only
    //actually care about the structure of the ROM when we're reading or writing
    public int PassThrough { get; set; }

    public int Map { get; set; }

    private IntVector2 pos = IntVector2.ZERO;

    /// int vector position starting at Y=0 for the top row
    public IntVector2 Pos
    {
        get => pos;
        set { pos = value; }
    }

    /// Y position starting at Y=0 for the top row
    public int Y
    {
        get => pos.Y;
        set { Pos = new(pos.X, value); }
    }
    /// Y position starting at Y=30 for the top row
    public int YRaw
    {
        get => Y + 30;
        set { Y = value - 30; }
    }

    public int Xpos
    {
        get => pos.X;
        set { Pos = new(value, pos.Y); }
    }

    /// X,Y position with Y starting at 30 for the top row
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

    //List of requirements to access this location. Required before the other requirements are evaluated,
    //and to pass through the location on the map, even if the collectable is not gettable.
    public Requirements AccessRequirements;
    //Requirements to get the thing(s) here. For palaces, this does not (yet) consider the layout of the palace or the rooms in it
    //only the general "need fairy or key" requirements for palaces generally
    public Requirements CollectableRequirements;
    //If this location is a connector, these are the requirements for the other side of the connection to be reached
    public Requirements ConnectionRequirements;

    //This does 2 things and should only do 1. It both tracks whether the location is a location that should be possible to shuffle,
    //and tracks whether this location has actually been shuffled already. This persistence doesn't always get reset properly,
    //which causes a bunch of bugs. That said, tracking 100 references and reworking it everywhere is beyond what I want to mess with
    //so for now i'm going to keep this crap design but I am mad about it.
    public bool CanShuffle { get; set; }
	
    /// <summary>
    /// Page you enter this location from, 0 means left. 1/2/3 will enter from the right on areas that have
    /// that many pages total or midscren on other areas.
    /// </summary>
    public int MapPageRaw { get; set; }
    public int MapPage { get => MapPageRaw >> 6; set => MapPageRaw = value << 6; }

    public int ExternalWorld { get; set; }

    public bool Reachable { get; set; }

    public int? PalaceNumber { get; set; }

    public Town? ActualTown { get; set; }
    public Continent Continent { get; set; }
    public Continent? VanillaContinent { get; set; }
    public Continent? ConnectedContinent { get; set; }
    public int FallInHole { get; set; }
    public int ForceEnterRight { get; set; }

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
    public Location(LocationID lid, int yPos, int xPos, int map, Continent continent)
    {
        /*
        ExternalWorld = bytes[0] & 128;
        appear2loweruponexit = bytes[1] & 128;
        Secondpartofcave = bytes[1] & 64;
        Xpos = bytes[1] & 63;
        MapPage = bytes[2] & 192;
        Map = bytes[2] & 63;
        FallInHole = bytes[3] & 128;
        PassThrough = bytes[3] & 64;
        ForceEnterRight = bytes[3] & 32;
        World = bytes[3] & 31;
        TerrainType = t;
        */
        ID = lid;
        Map = map;
        YRaw = yPos;
        Xpos = xPos;
        MemAddress = lid.GetRomOffset(); ;
        EntranceNumber = 0;
        CanShuffle = true;
        Collectables = [];
        Reachable = false;
        PalaceNumber = null;
        Continent = continent;
        VanillaContinent = lid.GetContinent();
        ConnectedContinent = null;
        AccessRequirements = Requirements.NONE;
        CollectableRequirements = Requirements.NONE;
        ConnectionRequirements = Requirements.NONE;

        Name = ID.GetName() ?? "Unknown (" + Continent.GetName(Continent) + ")";
        ActualTown = ID.GetTown();

        if (Name.StartsWith("Unknown") && Xpos != 0 && YRaw != 0)
        {
            logger.Info("Missing location name on " + Continent.GetName(Continent) + " (" + Y + ", " + Xpos + ") Map: " + Map);
        }
        if (Name.Contains("FAKE"))
        {
            logger.Debug("Fake location encountered");
        }
    }

    public Location(Location clone) : this(clone.ID, clone.YRaw, clone.Xpos, clone.Map, clone.Continent)
    {
        ExternalWorld = clone.ExternalWorld;
        EntranceNumber = clone.EntranceNumber;
        Xpos = clone.Xpos;
        MapPageRaw = clone.MapPageRaw;
        FallInHole = clone.FallInHole;
        PassThrough = clone.PassThrough;
        ForceEnterRight = clone.ForceEnterRight;
        TerrainType = clone.TerrainType;
    }

    public byte[] GetLocationBytes()
    {
        byte[] bytes = new byte[4];
        if(AccessRequirements.HasHardRequirement(RequirementType.HAMMER) || AccessRequirements.HasHardRequirement(RequirementType.FLUTE))
        {
            bytes[0] = 0;
        }
        else
        {
            bytes[0] = (byte)(ExternalWorld + YRaw);
        }
        bytes[1] = (byte)((EntranceNumber << 6) + Xpos);
        bytes[2] = (byte)(MapPageRaw + Map);
        bytes[3] = (byte)(FallInHole + PassThrough + ForceEnterRight + GetWorld());
        return bytes;
    }

    public string GetDebuggerDisplay()
    {
        return Continent.ToString()
            + " " + TerrainType.ToString()
            + " " + Name
            + " (" + (Y) + "," + (Xpos) + ") _"
            + (Reachable ? "Reachable " : "Unreachable ")
            + '[' + string.Join(", ", Collectables.Select(i => i.ToString())) + ']';
    }

    public int GetWorld()
    {
        //Towns reference their banks
        if (ActualTown != null)
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
        if (PalaceNumber != null)
        {
            return PalaceNumber switch
            {
                //North palace
                null => 0,
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
    /// Zero out some bytes for this Location, which makes it an unused location in-game.
    /// </summary>
    public void Clear()
    {
        ExternalWorld = 0;
        YRaw = 0;
        Xpos = 0;
        EntranceNumber = 0;
        CanShuffle = false;
    }
}
