using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore;


public static class CHR
{
    public static readonly Dictionary<Terrain, int> TERRAIN_TILE_ADDRS = new()
    {
        { Terrain.TOWN, 0x115c0 },
        { Terrain.CAVE, 0x11f40 },
        { Terrain.PALACE, 0x11600 },
        { Terrain.BRIDGE, 0x115a0 },
        { Terrain.DESERT, 0x116c0 },
        { Terrain.GRASS, 0x116d0 },
        { Terrain.FOREST, 0x11680 },
        { Terrain.SWAMP, 0x116f0 },
        { Terrain.GRAVE, 0x11700 },
        { Terrain.ROAD, 0x11fe0},
        { Terrain.LAVA, 0x116e0 },
        { Terrain.MOUNTAIN, 0x11640 },
        { Terrain.WATER, 0x116e0 },
        { Terrain.PREPLACED_WATER, 0x116e0 },
        { Terrain.WALKABLEWATER, 0x116e0 },
        { Terrain.PREPLACED_WATER_WALKABLE, 0x116e0 },
        { Terrain.ROCK, 0x11560 },
        { Terrain.RIVER_DEVIL, 0x11400 },
    };

    // Palace objects
    public static int BRICKS_P2 = 0x9640;
    public static int BRICKS_GP = 0xd640;
    public static int ELEVATOR = 0x8ac0;
    public static int LOCKED_DOOR = 0x8740;
    public static int STEEL_BRICK = 0x95c0;
    public static int BRIDGE = 0x97c0;
    public static int CURTAIN = 0x9840;
    public static int PILLAR_TOP = 0x98e0;
    public static int PILLAR_FILL = 0x9a50;
    public static int CRUMBLE_BRIDGE = 0x9920;
    public static int LAVA_TOP = 0x9980;
    public static int LAVA_FILL = 0x9fe0;
    public static int BREAKABLE_BLOCK = 0x9ba0;
    public static int FINAL_BOSS_CANOPY = 0xd780;
    public static int NORTH_CASTLE_BRICK = 0xd9a0;

    public static readonly SpriteTile[] LockedDoorTiles = [
        new(LOCKED_DOOR, 1, 2, [(0, 0), (0, 2)], false),
        new(LOCKED_DOOR + 0x20, 1, 2, [(0, 1)]),
    ];

    public static readonly SpriteTile[] WindowTiles = [
        new(0x96a0, 2, 1, [(0, 0)], false),
        new(0x96c0, 2, 1, [(0, 1), (0, 2)], false),
        new(0x96e0, 2, 1, [(0, 3)], false),
    ];

    public static readonly SpriteTile[] CrystalStatueTiles = [
        new(0x9700, 2, 1, [(3, 1)]),
        new(0x9720, 4, 1, [(2, 2)]),
        new(0x9760, 1, 1, [(2, 5)]),
        new(0x9770, 1, 1, [(3, 3), (3, 4), (3, 5), (3, 6), (3, 8), (3, 9), (4, 8), (4, 9)]),
        new(0x9780, 1, 1, [(4, 3)]),
        new(0x9790, 1, 1, [(4, 4), (4, 5), (4, 6)]),
        new(0x97a0, 2, 1, [(3, 7)]),
        new(0x9880, 1, 1, [(2, 0), (0, 6), (6, 6)]),
        new(0x98a0, 1, 1, [(3, 0), (4, 0)]),
        new(0x98c0, 1, 1, [(5, 0), (1, 6), (7, 6)]),
        new(0x9a80, 1, 1, [(0, 7), (0, 8), (0, 9), (2, 1), (2, 3), (2, 4), (2, 6), (2, 7), (2, 8), (2, 9), (6, 7), (6, 8), (6, 9)]),
        new(0x9aa0, 1, 1, [(1, 7), (1, 8), (1, 9), (5, 1), (5, 3), (5, 4), (5, 5), (5, 6), (5, 7), (5, 8), (5, 9), (7, 7), (7, 8), (7, 9)]),
    ];

    public static readonly SpriteTile[] LargeCloudTiles = [
        new(0x9b30, 1, 2, [(0, 0)]),
        new(0x9b50, 1, 2, [(1, 0), (2, 0)]),
        new(0x9b70, 1, 2, [(3, 0)]),
    ];

    public static readonly SpriteTile[] SmallCloudTiles = [
        new(0x9b30, 1, 2, [(0, 0)]),
        new(0x9b50, 1, 2, [(1, 0)]),
        new(0x9b70, 1, 2, [(2, 0)]),
    ];

    // Enemies
    public static int FLAME = 0x8520;
    public static int RA_HEAD = 0x9340;
    public static int MAU_HEAD = 0x9380;
    public static int IRON_KNUCKLE = 0x9400;
    public static int DRIPPER = 0xb8e0; // P2 pillar top
    public static int FOKKERU = 0xce00;
    public static int FOKKA = 0xd400;
    public static int DOOMKNOCKER = 0x12e00;

    public static readonly SpriteTile[] BotTiles = [new(0x8b40, 1, 2, [(0, 0), (1, 0, flipH: true)])];

    public static readonly SpriteTile[] MoaTiles = [new(0x8b60, 2, 2, [(0, 0)])];

    public static readonly SpriteTile[] BagoBagoTiles = [new(0x8c00, 2, 2, [(0, 0)])];

    public static readonly SpriteTile[] IronKnuckleTiles = [
        new(IRON_KNUCKLE, 2, 2, [(0, 0)]),
        new(IRON_KNUCKLE + 0x80, 2, 2, [(0, 2)]),
    ];

    public static readonly SpriteTile[] BubbleTiles = [new(0x9660, 1, 2, [(0, 0), (1, 0, flipH: true)])];

    public static readonly SpriteTile[] KingBotTiles = [new(0xcf00, 3, 4, [(0, 0), (3, 0, flipH: true)])];

    public static readonly SpriteTile[] FokkaTiles = [
        new(FOKKA, 2, 2, [(0, 0)]),
        new(FOKKA + 0x80, 2, 2, [(0, 2)]),
    ];

    // Items
    public static int HEART_CONTAINER = 0x1800;
    public static int MAGIC_CONTAINER = 0x1820;
    public static int TROPHY = 0x32e0;
    public static int MIRROR = 0x1a260;
    public static int BAGUS_NOTE = 0x1a280;
    public static int MEDICINE = 0x3300;
    public static int WATER = 0x1a2c0;
    public static int CHILD = 0x5300;
    public static int KEY = 0x8660;
    public static int FAIRY = 0x86a0;
    public static int PBAG = 0x8720;
    public static int JAR = 0x88a0;
    public static int CANDLE = 0x88c0;
    public static int GLOVE = 0x88e0;
    public static int RAFT = 0x8900;
    public static int BOOTS = 0x8920;
    public static int FLUTE = 0x8940;
    public static int CROSS = 0x8960;
    public static int HAMMER = 0x8980;
    public static int MAGIC_KEY = 0x89a0;
    public static int ONEUP = 0x8a80;

    public static readonly SpriteTile[] HeartContainerTiles = [new(HEART_CONTAINER, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] MagicContainerTiles = [new(MAGIC_CONTAINER, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] TrophyTiles = [new(TROPHY, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] MirrorTiles = [new(MIRROR, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] BagusNoteTiles = [new(BAGUS_NOTE, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] MedicineTiles = [new(MEDICINE, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] WaterTiles = [new(WATER, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] ChildTiles = [new(CHILD, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] KeyTiles = [new(KEY, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] FairyTiles = [new(FAIRY, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] PBagTiles = [new(PBAG, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] JarTiles = [new(JAR, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] ShieldTiles = [new(0x1a3a0, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] JumpTiles = [new(0x1a140, 1, 2, [(0, 0)]), new(0x1a120, 1, 2, [(1, 0)])];
    public static readonly SpriteTile[] LifeTiles = [new(0x1a160, 1, 2, [(0, 0)]), new(0x1a180, 1, 2, [(1, 0, flipH: true)])];
    public static readonly SpriteTile[] FireTiles = [new(0x520, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] ReflectTiles = [new(0x1a1a0, 1, 2, [(0, 0)]), new(0x1a1c0, 1, 2, [(1, 0, flipH: true)])];
    public static readonly SpriteTile[] SpellTiles = [new(0x1a300, 1, 2, [(0, 0), (1, 0, flipH: true)])];
    public static readonly SpriteTile[] ThunderTiles = [new(0x1a360, 1, 2, [(0, 0)]), new(0x1a380, 1, 2, [(1, 0, flipH: true)])];
    public static readonly SpriteTile[] DashTiles = [new(0x1a0e0, 1, 2, [(0, 0)]), new(0x1a140, 1, 2, [(1, 0)])];
    public static readonly SpriteTile[] UpstabTiles = [new(0x1a200, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] DownstabTiles = [new(0x1a220, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] CandleTiles = [new(CANDLE, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] GloveTiles = [new(GLOVE, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] RaftTiles = [new(RAFT, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] BootsTiles = [new(BOOTS, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] FluteTiles = [new(FLUTE, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] CrossTiles = [new(CROSS, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] HammerTiles = [new(HAMMER, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] MagicKeyTiles = [new(MAGIC_KEY, 1, 2, [(0, 0)])];
    public static readonly SpriteTile[] OneUpTiles = [new(ONEUP, 1, 2, [(0, 0)])];

    public static readonly Dictionary<Collectable, SpriteTile[]> COLLECTABLE_TILES = new()
    {
        { Collectable.CANDLE, CandleTiles },
        { Collectable.GLOVE, GloveTiles },
        { Collectable.RAFT, RaftTiles },
        { Collectable.BOOTS, BootsTiles },
        { Collectable.FLUTE, FluteTiles },
        { Collectable.CROSS, CrossTiles },
        { Collectable.HAMMER, HammerTiles },
        { Collectable.MAGIC_KEY, MagicKeyTiles },
        { Collectable.KEY, KeyTiles },
        { Collectable.SMALL_BAG, PBagTiles },
        { Collectable.MEDIUM_BAG, PBagTiles },
        { Collectable.LARGE_BAG, PBagTiles },
        { Collectable.XL_BAG, PBagTiles },
        { Collectable.MAGIC_CONTAINER, MagicContainerTiles },
        { Collectable.HEART_CONTAINER, HeartContainerTiles },
        { Collectable.BLUE_JAR, JarTiles },
        { Collectable.RED_JAR, JarTiles },
        { Collectable.ONEUP, OneUpTiles },
        { Collectable.CHILD, ChildTiles },
        { Collectable.TROPHY, TrophyTiles },
        { Collectable.MEDICINE, MedicineTiles },
        { Collectable.UPSTAB, UpstabTiles },
        { Collectable.DOWNSTAB, DownstabTiles },
        { Collectable.BAGUS_NOTE, BagusNoteTiles },
        { Collectable.MIRROR, MirrorTiles },
        { Collectable.WATER, WaterTiles },
        { Collectable.SHIELD_SPELL, ShieldTiles },
        { Collectable.JUMP_SPELL, JumpTiles },
        { Collectable.LIFE_SPELL, LifeTiles },
        { Collectable.FAIRY_SPELL, FairyTiles },
        { Collectable.FIRE_SPELL, FireTiles },
        { Collectable.REFLECT_SPELL, ReflectTiles },
        { Collectable.SPELL_SPELL, SpellTiles },
        { Collectable.THUNDER_SPELL, ThunderTiles },
        { Collectable.DASH_SPELL, DashTiles },
    };
}

public static class Palettes
{
    public static int ORANGE = ROM.RomHdrSize + 0x100a2;

    public static readonly Dictionary<Terrain, int> TERRAIN_ADDRS = new()
    {
        { Terrain.TOWN, 0x1c463 },
        { Terrain.CAVE, 0x1c45f },
        { Terrain.PALACE, 0x1c463 },
        { Terrain.BRIDGE, 0x1c45f },
        { Terrain.DESERT, 0x1c467 },
        { Terrain.GRASS, 0x1c45b },
        { Terrain.FOREST, 0x1c45b },
        { Terrain.SWAMP, 0x1c45b },
        { Terrain.GRAVE, 0x1c45f },
        { Terrain.ROAD, 0x1c45f },
        { Terrain.LAVA, 0x1c45f },
        { Terrain.MOUNTAIN, 0x1c45f },
        { Terrain.WATER, 0x100aa },
        { Terrain.PREPLACED_WATER, 0x100aa },
        { Terrain.WALKABLEWATER, 0x1c467 },
        { Terrain.PREPLACED_WATER_WALKABLE, 0x1c467 },
        { Terrain.ROCK, 0x1c45f },
        { Terrain.RIVER_DEVIL, 0x1c45f },
    };
}

public readonly record struct SpriteTilePlacement(
    /// X offset in 8-pixel units from sprite origin
    int X,
    /// Y offset in 8-pixel units from sprite origin
    int Y,
    /// Whether to mirror the tile horizontally
    bool FlipH)
{
    public static implicit operator SpriteTilePlacement((int x, int y) t)
        => new(t.x, t.y, false);

    public static implicit operator SpriteTilePlacement((int x, int y, bool flipH) t)
        => new(t.x, t.y, t.flipH);
}

public readonly record struct SpriteTile(
    /// CHR ROM address of the tile data
    int Addr,
    /// Number of tiles to read from address horizontally
    int W,
    /// Number of tiles to read from address vertically
    int H,
    /// Positions where this tile is drawn
    SpriteTilePlacement[] Placement,
    /// Whether to keep the alpha channel (false = opaque)
    bool Alpha = true
);
