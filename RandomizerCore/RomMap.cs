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

    //Addresses for vanilla locations. These addresses holds Y coordinate
    //bytes. The X coord bytes are at the same address + 0x3F.
    public const int WEST_NORTH_PALACE_TILE_LOCATION = 0x462f;
    public const int WEST_CAVE_TROPHY_TILE_LOCATION = 0x4630;
    public const int WEST_MINOR_FOREST_TILE_AT_START_LOCATION = 0x4631;
    public const int WEST_CAVE_MAGIC_CONTAINER_TILE_LOCATION = 0x4632;
    public const int WEST_MINOR_FOREST_TILE_BY_SARIA_LOCATION = 0x4633;
    public const int WEST_GRASS_TILE_LOCATION = 0x4634;
    public const int WEST_BAGU_WOODS_TILE_LOCATION1 = 0x4635;
    public const int WEST_TRAP_ROAD_TILE_LOCATION = 0x4636;
    public const int WEST_MINOR_SWAMP_TILE_LOCATION1 = 0x4637;
    public const int WEST_MINOR_GRAVE_TILE_LOCATION1 = 0x4638; //09: Red jar
    public const int WEST_CAVE_PARAPA_NORTH_TILE_LOCATION = 0x4639;
    public const int WEST_CAVE_PARAPA_SOUTH_TILE_LOCATION = 0x463a;
    public const int WEST_CAVE_JUMP_NORTH_TILE_LOCATION = 0x463b;
    public const int WEST_CAVE_JUMP_SOUTH_TILE_LOCATION = 0x463c;
    public const int WEST_CAVE_PBAG_TILE_LOCATION = 0x463d;
    public const int WEST_CAVE_MEDICINE_TILE_LOCATION = 0x463e;
    public const int WEST_CAVE_HEART_CONTAINER_TILE_LOCATION = 0x463f;
    public const int WEST_FAIRY_CAVE_DROP_TILE_LOCATION = 0x4640;
    public const int WEST_FAIRY_CAVE_EXIT_TILE_LOCATION = 0x4641;
    public const int WEST_BRIDGE_TILE_NORTH_OF_SARIA_LOCATION = 0x4642;
    public const int WEST_BRIDGE_TILE_EAST_OF_SARIA_LOCATION = 0x4643;
    public const int WEST_BRIDGE_AFTER_DM_WEST_LOCATION = 0x4644;
    public const int WEST_BRIDGE_AFTER_DM_EAST_LOCATION = 0x4645;
    public const int WEST_MINOR_FOREST_TILE_BY_JUMP_CAVE_LOCATION = 0x4646;
    public const int WEST_MINOR_SWAMP_TILE_LOCATION2 = 0x4647;
    public const int WEST_MINOR_FOREST_TILE_EAST_OF_SARIA_LOCATION = 0x4648;
    public const int WEST_BAGU_WOODS_TILE_LOCATION2 = 0x4649;
    public const int WEST_BAGU_WOODS_TILE_LOCATION3 = 0x464a;
    public const int WEST_BAGU_WOODS_TILE_LOCATION4 = 0x464b;
    public const int WEST_BAGU_WOODS_TILE_LOCATION5 = 0x464c;
    public const int WEST_MINOR_ROAD_TILE_LOCATION = 0x464d;
    public const int WEST_MINOR_DESERT_TILE_LOCATION = 0x464f;
    public const int WEST_KINGS_TOMB_TILE_LOCATION = 0x465b;
    public const int WEST_TOWN_RAURO_TILE_LOCATION = 0x465c;
    public const int WEST_TOWN_RUTO_TILE_LOCATION = 0x465e;
    public const int WEST_TOWN_SARIA_SOUTH_TILE_LOCATION = 0x465f;
    public const int WEST_TOWN_SARIA_NORTH_TILE_LOCATION = 0x4660;
    public const int WEST_BAGU_HOUSE_TILE_LOCATION = 0x4661;
    public const int WEST_TOWN_MIDO_TILE_LOCATION = 0x4662;
    public const int WEST_PALACE1_TILE_LOCATION = 0x4663;
    public const int WEST_PALACE2_TILE_LOCATION = 0x4664;
    public const int WEST_PALACE3_TILE_LOCATION = 0x4665;

    public const int EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION = 0x862f;
    public const int EAST_MINOR_FOREST_TILE_BY_P6_LOCATION = 0x8630;
    public const int EAST_TRAP_ROAD_TILE_LOCATION1 = 0x8631;
    public const int EAST_TRAP_ROAD_TILE_LOCATION2 = 0x8632;
    public const int EAST_TRAP_ROAD_TILE_LOCATION3 = 0x8633;
    public const int EAST_TRAP_ROAD_TILE_TO_VOD_LOCATION = 0x8634;
    public const int EAST_BRIDGE_TILE_TO_P6_LOCATION = 0x8635;
    public const int EAST_BRIDGE_TILE_TO_KASUTO_LOCATION = 0x8636;
    public const int EAST_TRAP_DESERT_TILE_LOCATION1 = 0x8637;
    public const int EAST_TRAP_DESERT_TILE_LOCATION2 = 0x8638;
    public const int EAST_WATER_TILE_LOCATION = 0x8639;
    public const int EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION = 0x863a;
    public const int EAST_CAVE_NABOORU_PASSTHROUGH_NORTH_LOCATION = 0x863b;
    public const int EAST_CAVE_PBAG1_LOCATION = 0x863c;
    public const int EAST_CAVE_PBAG2_LOCATION = 0x863d;
    public const int EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION = 0x863e;
    public const int EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST_LOCATION = 0x863f;
    public const int EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION = 0x8640;
    public const int EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION = 0x8641;
    public const int EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION = 0x8642;
    public const int EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION = 0x8643;
    public const int EAST_MINOR_SWAMP_TILE_LOCATION = 0x8644;
    public const int EAST_BUGGED_MINOR_LAVA_TILE_LOCATION = 0x8645; // in vanilla this is hidden behind another tile
    public const int EAST_MINOR_DESERT_TILE_LOCATION1 = 0x8646;
    public const int EAST_MINOR_DESERT_TILE_LOCATION2 = 0x8647;
    public const int EAST_MINOR_DESERT_TILE_LOCATION3 = 0x8648;
    public const int EAST_DESERT_TILE_LOCATION = 0x8649;
    public const int EAST_MINOR_FOREST_TILE_LOCATION2 = 0x864a;
    public const int EAST_MINOR_LAVA_TILE_LOCATION1 = 0x864b;
    public const int EAST_MINOR_LAVA_TILE_LOCATION2 = 0x864c;
    public const int EAST_TRAP_LAVA_TILE_LOCATION1 = 0x864d;
    public const int EAST_TRAP_LAVA_TILE_LOCATION2 = 0x864e;
    public const int EAST_TRAP_LAVA_TILE_LOCATION3 = 0x864f;
    public const int EAST_BRIDGE_TILE_TO_MI_LOCATION = 0x8657;
    public const int EAST_RAFT_TILE_TO_WEST_LOCATION = 0x8658;
    public const int EAST_TOWN_OF_NABOORU_TILE_LOCATION = 0x865c;
    public const int EAST_TOWN_OF_DARUNIA_TILE_LOCATION = 0x865e;
    public const int EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION = 0x8660;
    public const int EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION = 0x8662;
    public const int EAST_PALACE5_TILE_LOCATION = 0x8663;
    public const int EAST_PALACE6_TILE_LOCATION = 0x8664;
    public const int EAST_GREAT_PALACE_TILE_LOCATION = 0x8665;

    //Overworld data
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

}
