namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// The randomizer had an absolute crapton of raw memory addresses with no documentation.
/// The goal should be to never have a raw address anywhere in the code other than in this (or similar) files
/// storing constants in a more human-readable form.
/// </summary>
class RomMap
{
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
    public const int VANILLA_DESERT_TILE_LOCATION = 0x8649;

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
