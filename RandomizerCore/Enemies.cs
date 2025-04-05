using System.Linq;

namespace RandomizerCore;

public class Enemies
{
    public const int NormalPalaceEnemyAddr = 0x108B0;
    public const int GPEnemyAddr = 0x148B0;
    public const int OverworldEnemyAddr = 0x88B0;
    //Palaces 1,2,5
    public const int Palace125EnemyPtr = 0x105B1;
    //Palaces 3,4,6
    public const int Palace346EnemyPtr = 0x1208E;
    //GP
    public const int GPEnemyPtr = 0x145B1;
    public const int OverworldEnemyPtr = 0xA08E;


    public const int MAXIMUM_ENEMY_BYTES = 0x400;
	public const int MAXIMUM_ENEMY_BYTES_GP = 681;




	public static readonly int[] Palace125Enemies = new int[] { 0x03, 0x04, 0x0C, 0x11, 0x12, 0x18, 0x19, 0x1A, 0x1D, 0x1E, 0x1F, 0x23 };
    public static readonly int[] Palace125FlyingEnemies = new int[] { 0x06, 0x07, 0x0E };
    public static readonly int[] Palace125Generators = new int[] { 0x0B, 0x0F, 0x1B, 0x0A };
    public static readonly int[] Palace125SmallEnemies = new int[] { 0x03, 0x04, 0x11, 0x12 };
    public static readonly int[] Palace125LargeEnemies = new int[] { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1E, 0x1F, 0x23 };

    public static readonly int[] Palace346Enemies = new int[] { 0x03, 0x04, 0x0C, 0x11, 0x18, 0x19, 0x1A, 0x1D, 0x1F, 0x1E, 0x23 };
    public static readonly int[] Palace346FlyingEnemies = new int[] { 0x06, 0x07, 0x0E };
    public static readonly int[] Palace346Generators = new int[] { 0x0B, 0x0F, 0x1B  };
    public static readonly int[] Palace346SmallEnemies = new int[] { 0x03, 0x04, 0x11 };
    public static readonly int[] Palace346LargeEnemies = new int[] { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1F, 0x1E, 0x23 };

    public static readonly int[] GPEnemies = new int[] { 0x03, 0x04, 0x11, 0x12, 0x18, 0x19, 0x1A, 0x1D };
    public static readonly int[] GPFlyingEnemies = new int[] { 0x06, 0x14, 0x15, 0x17, 0x1E };
    public static readonly int[] GPGenerators = new int[] { 0x0B, 0x0C, 0x0F, 0x16 };
    public static readonly int[] GPSmallEnemies = new int[] { 03, 0x04, 0x11, 0x12 };
    public static readonly int[] GPLargeEnemies = new int[] { 0x18, 0x19, 0x1A, 0x1D };

	public static readonly int[] StandardPalaceEnemies = Palace125Enemies.Union(Palace346Enemies).ToArray();
    public static readonly int[] StandardPalaceFlyingEnemies = Palace125FlyingEnemies.Union(Palace346FlyingEnemies).ToArray();
    public static readonly int[] StandardPalaceGenerators = Palace125Generators.Union(Palace346Generators).ToArray();
    public static readonly int[] StandardPalaceSmallEnemies = Palace125SmallEnemies.Union(Palace346SmallEnemies).ToArray();
    public static readonly int[] StandardPalaceLargeEnemies = Palace125LargeEnemies.Union(Palace346LargeEnemies).ToArray();

    //TODO: Turn this into constants and a lookup, then replace the above nonsense with const arrays of constants instead of garbage
    /*
West Hyrule, Death Mountain:
	00 - Fairy
	01 - Red Jar
	02 - Locked Door
	03 - Myu
	04 - Bot (blue)
	05 - Bit (red)
	06 - Moa
	07 - Ache
	08 - crash
	09 - crash
	0A - Acheman
	0B - Bubble generator
	0C - Rock Generator
	0D - Red Deeler
	0E - Blue Deeler
	0F - Bago Bago Generator
	10 - Bago Bago
	11 - Red Octoroc (Jumping Only)
	12 - Red Octorock (Moving and Jumping)
	13 - Elevator
	14 - Orange Moblin
	15 - Red Moblin
	16 - Blue Moblin
	17 - Orange Diara
	18 - Red Diara
	19 - Orange Goryia
	1A - Red Goryia
	1B - Blue Goryia
	1C - Lowder
	1D - Moby generator
	1E - Moby
	1F - Megmet
	20 - Geldarm
	21 - Dumb Moblin Generator
	22 - Dumb Moblin
	23-FF crash
East Hyrule, Maze Island
	00 - Fairy
	01 - Red Magic Jar
	02 - Locked Door
	03 - Myu
	04 - Bot (blue)
	05 - Bit (red)
	06 - Moa
	07 - Ache
	08 - ???
	09 - ???
	0A - Acheman
	0B - Bubble Generator
	0C - Rock Generator (messed up link-doll sprite in east hyrule)
	0D - Red Deeler
	0E - Blue Deeler
	0F - Bago Bago Generator
	10 - Bago Bago
	11 - Blue Octoroc (Jumping Only)
	12 - Blue Octorock (Moving and Jumping)
	13 - Elevator
	14 - Tektite
	15 - Eye
	16 - Leever
	17 - Boon
	18 - Basilisk
	19 - Scorpion
	1A - Red Lizalfo
	1B - Orange Lizalfo
	1C - Blue Lizalfo
	1D - Boulder Tossing Lizalfos 
	1E-FF - Crash
Towns
	00 - Fairy
	01 - Candle
	02 - Locked Door
	03 - Purple Kees
	04 - Purple Bot
	05 - Bit
	06 - Purple Moa
	07 - Purple Kees (again?)
	08 - Gold colored girl, crashes game if you talk to her
	09-24 - Various townspeople
*/
}

/// <summary>
/// IDs that are shared everywhere in the game.
/// 
/// It's a class of constants and not an enum, so that
/// EnemiesAnywhere.Fairy == EnemiesPalace125.Fairy is true.
/// </summary>
public static class EnemiesShared
{
    public const int FAIRY = 0x00;
    public const int LOCKED_DOOR = 0x02;
    public const int MYU = 0x03;
    public const int BOT = 0x04;
}

/// <summary>
/// IDs that are shared in palace 1-6
/// 
/// It's a class of constants and not an enum, so that
/// EnemiesRegularPalace.Fairy == EnemiesPalace125.Fairy is true.
/// </summary>
public static class EnemiesRegularPalaceShared
{
    public const int FAIRY = 0x00;
    public const int ITEM_IN_PALACE_SPRITE = 0x01;
    public const int LOCKED_DOOR = 0x02;
    public const int MYU = 0x03;
    public const int BOT = 0x04;
    public const int STRIKE_FOR_RED_JAR = 0x05; // close enough
    public const int SLOW_BUBBLE = 0x06;
    public const int ORANGE_MOA = 0x07;
    public const int FALLING_BLOCK_GENERATOR = 0x08;
    public const int SINGLE_FALLING_BLOCK = 0x09;

    public const int TINSUIT_GENERATOR = 0x0B;
    public const int TINSUIT = 0x0C;
    public const int DRIPPER = 0x0D;
    public const int FAST_BUBBLE = 0x0E;

    public const int ELEVATOR = 0x13;
    public const int CRYSTAL_SPOT = 0x14;
    public const int CRYSTAL = 0x15;
    public const int ENERGY_BALL_SHOOTER_DOWN_RIGHT = 0x16;
    public const int ENERGY_BALL_SHOOTER_DOWN_LEFT = 0x17;
    public const int ORANGE_IRON_KNUCKLE = 0x18;
    public const int RED_IRON_KNUCKLE = 0x19;
    public const int BLUE_IRON_KNUCKLE = 0x1A;
    public const int WOLF_HEAD_GENERATOR = 0x1B;
    public const int WOLF_HEAD = 0x1C;
    public const int RED_STALFOS = 0x1F; // close enough
    public const int BLUE_STALFOS = 0x23; // close enough
}

/// <summary>
/// IDs that are shared in all palaces, including Great Palace.
/// 
/// It's a class of constants and not an enum, so that
/// EnemiesAnyPalace.Fairy == EnemiesPalace125.Fairy is true.
/// </summary>
public static class EnemiesPalaceShared
{
    public const int FAIRY = 0x00;
    public const int LOCKED_DOOR = 0x02;
    public const int MYU = 0x03;
    public const int BOT = 0x04;
    public const int ELEVATOR = 0x13;
}

/// <summary>
/// IDs for Palace Group 1 (Palace 1, 2, 5)
/// </summary>
public enum EnemiesPalace125
{
    FAIRY = 0x00,
    ITEM_IN_PALACE_SPRITE = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    STRIKE_FOR_RED_JAR = 0x05,
    SLOW_BUBBLE = 0x06,
    ORANGE_MOA = 0x07,
    FALLING_BLOCK_GENERATOR = 0x08,
    SINGLE_FALLING_BLOCK = 0x09,
    BLUE_DRAGON_HEAD_GENERATOR = 0x0A,
    TINSUIT_GENERATOR = 0x0B,
    TINSUIT = 0x0C,
    DRIPPER = 0x0D,
    FAST_BUBBLE = 0x0E,
    BAGO_BAGO_GENERATOR = 0x0F,
    BAGO_BAGO = 0x10,
    ROPE_STATIONARY = 0x11,
    ROPE_MOVING = 0x12,
    ELEVATOR = 0x13,
    CRYSTAL_SPOT = 0x14,
    CRYSTAL = 0x15,
    ENERGY_BALL_SHOOTER_DOWN_RIGHT = 0x16,
    ENERGY_BALL_SHOOTER_DOWN_LEFT = 0x17,
    ORANGE_IRON_KNUCKLE = 0x18,
    RED_IRON_KNUCKLE = 0x19,
    BLUE_IRON_KNUCKLE = 0x1A,
    WOLF_HEAD_GENERATOR = 0x1B,
    WOLF_HEAD = 0x1C,
    MAGO = 0x1D,
    GUMA = 0x1E,
    RED_STALFOS = 0x1F,
    HORSEHEAD = 0x20,
    HELMETHEAD_OR_GOOMA = 0x21,
    HELMETHEAD_HEAD = 0x22,
    BLUE_STALFOS = 0x23
}

/// <summary>
/// IDs for Palace Group 2 (Palace 3, 4, 6)
/// </summary>
public enum EnemiesPalace346
{
    FAIRY = 0x00,
    ITEM_IN_PALACE_SPRITE = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    STRIKE_FOR_RED_JAR_OR_IRON_KNUCKLE = 0x05,
    SLOW_BUBBLE = 0x06,
    ORANGE_MOA = 0x07,
    FALLING_BLOCK_GENERATOR = 0x08,
    SINGLE_FALLING_BLOCK = 0x09,
    REBO_UNHORSED = 0x0A,
    TINSUIT_GENERATOR = 0x0B,
    TINSUIT = 0x0C,
    DRIPPER = 0x0D,
    FAST_BUBBLE = 0x0E,
    BLUE_DRAGON_HEAD_GENERATOR = 0x0F,
    FLAME = 0x10,
    ROPE = 0x11,
    ELEVATOR = 0x13,
    CRYSTAL_SPOT = 0x14,
    CRYSTAL = 0x15,
    ENERGY_BALL_SHOOTER_DOWN_RIGHT = 0x16,
    ENERGY_BALL_SHOOTER_DOWN_LEFT = 0x17,
    ORANGE_IRON_KNUCKLE = 0x18,
    RED_IRON_KNUCKLE = 0x19,
    BLUE_IRON_KNUCKLE = 0x1A,
    WOLF_HEAD_GENERATOR = 0x1B,
    WOLF_HEAD = 0x1C,
    WIZARD = 0x1D,
    DOOMKNOCKER = 0x1E,
    ARMORED_RED_STALFOS = 0x1F,
    REBONAK = 0x20,
    BARBA = 0x21,
    CAROCK = 0x22,
    BLUE_STALFOS = 0x23
}

/// <summary>
/// IDs for Palace Group 3 (Great Palace)
/// </summary>
public enum EnemiesGreatPalace
{
    FAIRY = 0x00,
    STRIKE_FOR_RED_JAR_OR_FOKKA = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    BIT = 0x05,
    ORANGE_MOA = 0x06,
    ACHE = 0x07,
    ACHEMAN = 0x0A,
    BUBBLE_GENERATOR = 0x0B,
    ROCK_GENERATOR = 0x0C,
    RED_DEELER = 0x0D,
    BLUE_DEELER = 0x0E,
    FIRE_BAGO_BAGO_GENERATOR = 0x0F,
    FIRE_BAGO_BAGO = 0x10,
    ROPE_STATIONARY = 0x11,
    ROPE_MOBILE = 0x12,
    ELEVATOR = 0x13,
    SLOW_BUBBLE = 0x14,
    FAST_BUBBLE = 0x15,
    ORANGE_DRAGON_HEAD_GENERATOR = 0x16,
    BIG_BUBBLE = 0x17,
    ORANGE_FOKKA = 0x18,
    RED_FOKKA = 0x19,
    BLUE_FOKKA = 0x1A,
    FOKKERU = 0x1D,
    KING_BOT = 0x1E,
    GP_BARRIER = 0x20,
    THUNDERBIRD = 0x22,
    DARK_LINK = 0x23
}
