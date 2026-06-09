using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// The randomizer had an absolute crapton of raw memory addresses with no documentation.
/// The goal should be to never have a raw address anywhere in the code other than in this (or similar) files
/// storing constants in a more human-readable form.
/// </summary>
class RomMap
{
    public const int SIDEVIEW_PTR_TABLE_P125 = 0x10533;
    public const int SIDEVIEW_PTR_TABLE_P346 = 0x12010;
    public const int SIDEVIEW_PTR_TABLE_GP = 0x14533;

    //Start with item bytes
    public const int START_CANDLE = 0x17B01;
    public const int START_GLOVE = 0x17B02;
    public const int START_RAFT = 0x17B03;
    public const int START_BOOTS = 0x17B04;
    public const int START_FLUTE = 0x17B05;
    public const int START_CROSS = 0x17B06;
    public const int START_HAMMER = 0x17B07;
    public const int START_MAGICAL_KEY = 0x17B08;

    // Base offsets for continent location tables.
    // Add overworldYOffset et al to these.
    public const int WEST_LOCATIONS_BASE = 0x462f;
    public const int DM_LOCATIONS_BASE = 0x610c;
    public const int EAST_LOCATIONS_BASE = 0x862f;
    public const int MI_LOCATIONS_BASE = 0xa10c;

    public static readonly int[] ContinentLocationBaseArray = [
        WEST_LOCATIONS_BASE,
        DM_LOCATIONS_BASE,
        EAST_LOCATIONS_BASE,
        MI_LOCATIONS_BASE,
    ];

    public static readonly IReadOnlyDictionary<Continent, int> ContinentLocationBases = Enum.GetValues<Continent>()
        .ToDictionary(c => c, c => ContinentLocationBaseArray[(int)c]);

    //Overworld data
    public const int overworldYOffset = 0;
    public const int overworldXOffset = 0x3F;
    public const int overworldMapOffset = 0x7E;
    public const int overworldWorldOffset = 0xBD;

    //Vanilla non-palace collectable ID bytes
    public const int WEST_GRASS_TILE_COLLECTABLE = 0x4dd7;
    public const int WEST_TROPHY_CAVE_COLLECTABLE = 0x4dea;
    public const int WEST_PBAG_CAVE_COLLECTABLE = 0x4fe2;
    public const int WEST_HEART_CONTAINER_CAVE_COLLECTABLE = 0x4ff5;
    public const int WEST_MAGIC_CONTAINER_CAVE_COLLECTABLE = 0x502a;
    public const int WEST_MEDICINE_CAVE_COLLECTABLE = 0x5069;
    public const int DM_HAMMER_CAVE_COLLECTABLE = 0x6512;
    public const int DM_SPECTACLE_ROCK_COLLECTABLE = 0x65c3;
    public const int EAST_PBAG_CAVE1_COLLECTABLE = 0x8ecc;
    public const int EAST_WATER_TILE_COLLECTABLE = 0x8faa;
    public const int EAST_PBAG_CAVE2_COLLECTABLE = 0x8fb3;
    public const int EAST_DESERT_TILE_COLLECTABLE = 0x9011;
    public const int EAST_NEW_KASUTO_BASEMENT_COLLECTABLE = 0xDb8c;
    public const int EAST_SPELL_TOWER_COLLECTABLE = 0xDb95;
    public const int MI_CHILD_DROP_COLLECTABLE = 0xA58b;
    public const int MI_MAGIC_CONTAINER_DROP_COLLECTABLE = 0xA5a8;
    // int[] VANILLA_PALACE_COLLECTABLE_BYTES = [0x10E91, 0x10E9A, 0x1252D, 0x12538, 0x10EA3, 0x12774];

    public const int WEST_ENEMY_HP_TABLE = 0x5431;
    public const int EAST_ENEMY_HP_TABLE = 0x9431;
    public const int PALACE125_ENEMY_HP_TABLE = 0x11431;
    public const int PALACE346_ENEMY_HP_TABLE = 0x12931;
    public const int GP_ENEMY_HP_TABLE = 0x15431;
    public const int DRIPPER_ID = 0x11927;
    public const int WEST_ENEMY_STATS_TABLE = 0x54e5;
    public const int EAST_ENEMY_STATS_TABLE = 0x94e5;
    public const int PALACE125_ENEMY_STATS_TABLE = 0x114e5;
    public const int PALACE346_ENEMY_STATS_TABLE = 0x129e5;
    public const int GP_ENEMY_STATS_TABLE = 0x154e5;

    public const int ATTACK_EFFECTIVENESS_TABLE = 0x1E67D;
    public const int LIFE_EFFECTIVENESS_TABLE = 0x1E2BF;
    public const int MAGIC_EFFECTIVENESS_TABLE = 0xD8B;

    public const int EXPERIENCE_TO_LEVEL_TABLE = 0x1669;
    public const int EXPERIENCE_TO_LEVEL_TEXT_TABLE = 0x1e42;

    public const int NORTH_SOUTH_SEPARATOR_WEST = 0x1cb42;
    public const int NORTH_SOUTH_SEPARATOR_DM   = 0x1cb43;
    public const int NORTH_SOUTH_SEPARATOR_EAST = 0x1cb44;

    public static readonly List<int> bossHpAddresses = [
        0x11451, // Horsehead          (regular Palace 125 enemy table)
        0x13C86, // Helmethead         ("bank4_Table_for_Helmethead_Gooma")
        0x12951, // Rebonack           (regular Palace 346 enemy table)
        0x13041, // Unhorsed Rebonack  (hardcoded, not in a table)
        0x12953, // Carock             (regular Palace 346 enemy table)
        0x13C87, // Gooma              ("bank4_Table_for_Helmethead_Gooma")
        0x12952, // Barba              (regular Palace 346 enemy table)
        // These are bank 5 enemies so we should make a separate table for them
        // but we can deal with these when we start randomizing their hp
        // 0x15453, // Thunderbird
        // 0x15454, // Dark Link
    ];
    public static readonly List<int> bossExpAddresses = [
        0x11505, // Horsehead          (regular Palace 125 enemy table)
        0x13C88, // Helmethead         ("bank4_Table_for_Helmethead_Gooma")
        0x12A05, // Rebonack           (regular Palace 346 enemy table)
        0x129EF, // Unhorsed Rebonack  (regular Palace 346 enemy table)
        0x12A07, // Carock             (regular Palace 346 enemy table)
        0x13C89, // Gooma              ("bank4_Table_for_Helmethead_Gooma")
        0x12A06, // Barba              (regular Palace 346 enemy table)
        0x15507, // Thunderbird        (regular GP enemy table)
    ];
    public static readonly List<(int, int)> bossHpDivisorMap = [
        (bossHpAddresses[0], 0x13b80), // Horsehead
        (bossHpAddresses[1], 0x13ae2), // Helmethead
        (bossHpAddresses[2], 0x12fd2), // Rebonack
        (bossHpAddresses[3], 0x1325c), // Unhorsed Rebonack
        (bossHpAddresses[4], 0x12e92), // Carock
        (bossHpAddresses[5], 0x134cf), // Gooma
        (bossHpAddresses[6], 0x13136), // Barba
        // 0x13ae9 - unknown; who is this? I'm guessing its a helmet mini boss thing?
        // (0x15453, 0x16406), // Thunderbird
        // (0x15454, 0x158aa), // Dark Link
    ];

    public const int WEST_PALETTE_TABLE = 0x401e;
    public const int EAST_PALETTE_TABLE = 0x801e;
    public const int TOWN_PALETTE_TABLE = 0xc01e;
    public const int PALACE_PALETTE_TABLE_MAJOR = 0x1001e;
    public const int PALACE_PALETTE_TABLE_ENTRANCES = 0x10480;
    public const int PALACE_PALETTE_TABLE_PER_PALACE = 0x13f10;
    public const int GP_PALETTE_TABLE_MAJOR = 0x1401e;
}
