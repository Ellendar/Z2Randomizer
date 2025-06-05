using System.Linq;

namespace Z2Randomizer.RandomizerCore.Enemy;

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

    public static readonly EnemiesWest[] WestSmallEnemies = [
        EnemiesWest.MYU,
        EnemiesWest.BOT,
        EnemiesWest.BIT,
        EnemiesWest.OCTOROC_STATIONARY,
        EnemiesWest.OCTOROC_MOVING,
        EnemiesWest.LOWDER,
        EnemiesWest.MEGMET,
    ];
    public static readonly EnemiesWest[] WestLargeEnemies = [
        EnemiesWest.ORANGE_MOBLIN,
        EnemiesWest.RED_MOBLIN,
        EnemiesWest.BLUE_MOBLIN,
        EnemiesWest.ORANGE_DIARA,
        EnemiesWest.RED_DIARA,
        EnemiesWest.ORANGE_GORYIA,
        EnemiesWest.RED_GORYIA,
        EnemiesWest.BLUE_GORYIA,
        EnemiesWest.GELDARM,
    ];
    public static readonly EnemiesWest[] WestGroundEnemies = [.. WestSmallEnemies, .. WestLargeEnemies];
    public static readonly EnemiesWest[] WestFlyingEnemies = [
        EnemiesWest.MOA,
        EnemiesWest.ACHE,
        EnemiesWest.ACHEMAN,
        EnemiesWest.RED_DEELER,
        EnemiesWest.BLUE_DEELER,
    ];
    public static readonly EnemiesWest[] WestGenerators = [
        EnemiesWest.BUBBLE_GENERATOR,
        EnemiesWest.ROCK_GENERATOR,
        EnemiesWest.BAGO_BAGO_GENERATOR,
        EnemiesWest.MOBY_GENERATOR,
    ];

    public static readonly EnemiesEast[] EastSmallEnemies = [
        EnemiesEast.MYU,
        EnemiesEast.BOT,
        EnemiesEast.BIT,
        EnemiesEast.BLUE_OCTOROC_STATIONARY,
        EnemiesEast.BLUE_OCTOROC_MOVING,
        EnemiesEast.LEEVER,
    ];
    public static readonly EnemiesEast[] EastLargeEnemies = [
        EnemiesEast.TEKTITE,
        EnemiesEast.BASILISK,
        EnemiesEast.SCORPION,
        EnemiesEast.RED_LIZALFOS,
        EnemiesEast.ORANGE_LIZALFOS,
        EnemiesEast.BLUE_LIZALFOS,
    ];
    public static readonly EnemiesEast[] EastGroundEnemies = [.. EastSmallEnemies, .. EastLargeEnemies];
    public static readonly EnemiesEast[] EastFlyingEnemies = [
        EnemiesEast.MOA,
        EnemiesEast.ACHE,
        EnemiesEast.ACHEMAN,
        EnemiesEast.RED_DEELER,
        EnemiesEast.BLUE_DEELER,
        EnemiesEast.GIRUBOKKU,
    ];
    public static readonly EnemiesEast[] EastGenerators = [
        EnemiesEast.BUBBLE_GENERATOR,
        EnemiesEast.BAGO_BAGO_GENERATOR,
        EnemiesEast.BOON_GENERATOR,
    ];

    public static readonly EnemiesPalace125[] Palace125SmallEnemies = [
        EnemiesPalace125.MYU,
        EnemiesPalace125.BOT,
        EnemiesPalace125.ROPE_STATIONARY,
        EnemiesPalace125.ROPE_MOVING,
    ];
    public static readonly EnemiesPalace125[] Palace125LargeEnemies = [
        EnemiesPalace125.TINSUIT,
        EnemiesPalace125.ORANGE_IRON_KNUCKLE,
        EnemiesPalace125.RED_IRON_KNUCKLE,
        EnemiesPalace125.BLUE_IRON_KNUCKLE,
        EnemiesPalace125.MAGO,
        EnemiesPalace125.GUMA,
        EnemiesPalace125.RED_STALFOS,
        EnemiesPalace125.BLUE_STALFOS,
    ];
    public static readonly EnemiesPalace125[] Palace125GroundEnemies = [.. Palace125SmallEnemies, .. Palace125LargeEnemies];
    public static readonly EnemiesPalace125[] Palace125FlyingEnemies = [
        EnemiesPalace125.SLOW_BUBBLE,
        EnemiesPalace125.ORANGE_MOA,
        EnemiesPalace125.FAST_BUBBLE,
    ];
    public static readonly EnemiesPalace125[] Palace125Generators = [
        EnemiesPalace125.TINSUIT_GENERATOR,
        EnemiesPalace125.BAGO_BAGO_GENERATOR,
        EnemiesPalace125.WOLF_HEAD_GENERATOR,
        EnemiesPalace125.BLUE_DRAGON_HEAD_GENERATOR,
    ];

    public static readonly EnemiesPalace346[] Palace346SmallEnemies = [
        EnemiesPalace346.MYU,
        EnemiesPalace346.BOT,
        EnemiesPalace346.ROPE_STATIONARY,
    ];
    public static readonly EnemiesPalace346[] Palace346LargeEnemies = [
        EnemiesPalace346.TINSUIT,
        EnemiesPalace346.ORANGE_IRON_KNUCKLE,
        EnemiesPalace346.RED_IRON_KNUCKLE,
        EnemiesPalace346.BLUE_IRON_KNUCKLE,
        EnemiesPalace346.WIZARD,
        EnemiesPalace346.DOOMKNOCKER,
        EnemiesPalace346.RED_STALFOS,
        EnemiesPalace346.BLUE_STALFOS,
    ];
    public static readonly EnemiesPalace346[] Palace346GroundEnemies = [.. Palace346SmallEnemies, .. Palace346LargeEnemies];
    public static readonly EnemiesPalace346[] Palace346FlyingEnemies = [
        EnemiesPalace346.SLOW_BUBBLE,
        EnemiesPalace346.ORANGE_MOA,
        EnemiesPalace346.FAST_BUBBLE,
    ];
    public static readonly EnemiesPalace346[] Palace346Generators = [
        EnemiesPalace346.TINSUIT_GENERATOR,
        EnemiesPalace346.BLUE_DRAGON_HEAD_GENERATOR,
        EnemiesPalace346.WOLF_HEAD_GENERATOR,
    ];

    public static readonly EnemiesGreatPalace[] GPSmallEnemies = [
        EnemiesGreatPalace.MYU,
        EnemiesGreatPalace.BOT,
        EnemiesGreatPalace.ROPE_STATIONARY,
        EnemiesGreatPalace.ROPE_MOBILE,
    ];
    public static readonly EnemiesGreatPalace[] GPLargeEnemies = [
        EnemiesGreatPalace.ORANGE_FOKKA,
        EnemiesGreatPalace.RED_FOKKA,
        EnemiesGreatPalace.BLUE_FOKKA,
        EnemiesGreatPalace.FOKKERU,
    ];
    public static readonly EnemiesGreatPalace[] GPGroundEnemies = [.. GPSmallEnemies, .. GPLargeEnemies];
    public static readonly EnemiesGreatPalace[] GPFlyingEnemies = [
        EnemiesGreatPalace.ORANGE_MOA,
        EnemiesGreatPalace.SLOW_BUBBLE,
        EnemiesGreatPalace.FAST_BUBBLE,
        EnemiesGreatPalace.BIG_BUBBLE,
        EnemiesGreatPalace.KING_BOT, // Not actually a flying enemy
    ];
    public static readonly EnemiesGreatPalace[] GPGenerators = [
        EnemiesGreatPalace.BUBBLE_GENERATOR,
        EnemiesGreatPalace.ROCK_GENERATOR,
        EnemiesGreatPalace.FIRE_BAGO_BAGO_GENERATOR,
        EnemiesGreatPalace.ORANGE_DRAGON_HEAD_GENERATOR,
    ];

    public static readonly int[] StandardPalaceGroundEnemies = Palace125GroundEnemies.Select(e => (int)e).Union(Palace346GroundEnemies.Select(e => (int)e)).ToArray();
    public static readonly int[] StandardPalaceFlyingEnemies = Palace125FlyingEnemies.Select(e => (int)e).Union(Palace346FlyingEnemies.Select(e => (int)e)).ToArray();
    public static readonly int[] StandardPalaceGenerators = Palace125Generators.Select(e => (int)e).Union(Palace346Generators.Select(e => (int)e)).ToArray();
    public static readonly int[] StandardPalaceSmallEnemies = Palace125SmallEnemies.Select(e => (int)e).Union(Palace346SmallEnemies.Select(e => (int)e)).ToArray();
    public static readonly int[] StandardPalaceLargeEnemies = Palace125LargeEnemies.Select(e => (int)e).Union(Palace346LargeEnemies.Select(e => (int)e)).ToArray();
}

/*
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
/// IDs for the west continent and Death Mountain
/// </summary>
public enum EnemiesWest
{
    FAIRY = 0x00,
    RED_JAR = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    BIT = 0x05,
    MOA = 0x06,
    ACHE = 0x07,
    ACHEMAN = 0x0A,
    BUBBLE_GENERATOR = 0x0B,
    ROCK_GENERATOR = 0x0C,
    RED_DEELER = 0x0D,
    BLUE_DEELER = 0x0E,
    BAGO_BAGO_GENERATOR = 0x0F,
    BAGO_BAGO = 0x10,
    OCTOROC_STATIONARY = 0x11,
    OCTOROC_MOVING = 0x12,
    ELEVATOR = 0x13,
    ORANGE_MOBLIN = 0x14,
    RED_MOBLIN = 0x15,
    BLUE_MOBLIN = 0x16,
    ORANGE_DIARA = 0x17,
    RED_DIARA = 0x18,
    ORANGE_GORYIA = 0x19,
    RED_GORYIA = 0x1A,
    BLUE_GORYIA = 0x1B,
    LOWDER = 0x1C,
    MOBY_GENERATOR = 0x1D,
    MOBY = 0x1E,
    MEGMET = 0x1F,
    GELDARM = 0x20,
    DUMB_MOBLIN_GENERATOR = 0x21,
    DUMB_MOBLIN = 0x22,
}

/// <summary>
/// IDs for the east continent and Maze Island
/// </summary>
public enum EnemiesEast
{
    FAIRY = 0x00,
    RED_JAR = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    BIT = 0x05,
    MOA = 0x06,
    ACHE = 0x07,
    ACHEMAN = 0x0A,
    BUBBLE_GENERATOR = 0x0B,
    RED_DEELER = 0x0D,
    BLUE_DEELER = 0x0E,
    BAGO_BAGO_GENERATOR = 0x0F,
    BAGO_BAGO = 0x10,
    BLUE_OCTOROC_STATIONARY = 0x11,
    BLUE_OCTOROC_MOVING = 0x12,
    ELEVATOR = 0x13,
    TEKTITE = 0x14,
    GIRUBOKKU = 0x15,
    LEEVER = 0x16,
    BOON_GENERATOR = 0x17,
    BASILISK = 0x18,
    SCORPION = 0x19,
    RED_LIZALFOS = 0x1A,
    ORANGE_LIZALFOS = 0x1B,
    BLUE_LIZALFOS = 0x1C,
    BOULDER_TOSSING_LIZALFOS = 0x1D,
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

    public const int ROPE_STATIONARY = 0x11;
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
    ROPE_STATIONARY = 0x11,
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
    RED_STALFOS = 0x1F,
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
    ITEM_IN_PALACE_SPRITE = 0x01,
    LOCKED_DOOR = 0x02,
    MYU = 0x03,
    BOT = 0x04,
    STRIKE_FOR_RED_JAR_OR_FOKKA = 0x05,
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
