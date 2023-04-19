using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer;

/// <summary>
/// The randomizer had an absolute crapton of raw memory addresses with no documentation.
/// The goal should be to never have a raw address anywhere in the code other than in this (or similar) files
/// storing constants in a more human-readable form.
/// </summary>
class RomMap
{
    //Has item addresses
    public static readonly int START_CANDLE = 0x17B01;
    public static readonly int START_GLOVE = 0x17B02;
    public static readonly int START_RAFT = 0x17B03;
    public static readonly int START_BOOTS = 0x17B04;
    public static readonly int START_FLUTE = 0x17B05;
    public static readonly int START_CROSS = 0x17B06;
    public static readonly int START_HAMMER = 0x17B07;
    public static readonly int START_MAGICAL_KEY = 0x17B08;

    //Start of memory for vanilla locations
    public static readonly int VANILLA_DESERT_TILE_LOCATION = 0x8649;
}
