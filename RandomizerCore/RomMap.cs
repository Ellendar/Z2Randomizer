namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// The randomizer had an absolute crapton of raw memory addresses with no documentation.
/// The goal should be to never have a raw address anywhere in the code other than in this (or similar) files
/// storing constants in a more human-readable form.
/// </summary>
class RomMap
{
    //Has item addresses
    public const int START_CANDLE = 0x17B01;
    public const int START_GLOVE = 0x17B02;
    public const int START_RAFT = 0x17B03;
    public const int START_BOOTS = 0x17B04;
    public const int START_FLUTE = 0x17B05;
    public const int START_CROSS = 0x17B06;
    public const int START_HAMMER = 0x17B07;
    public const int START_MAGICAL_KEY = 0x17B08;

    //Start of memory for vanilla locations
    public const int VANILLA_DESERT_TILE_LOCATION = 0x8649;

    //Overworld data
    public const int overworldXOffset = 0x3F;
    public const int overworldMapOffset = 0x7E;
    public const int overworldWorldOffset = 0xBD;
}
