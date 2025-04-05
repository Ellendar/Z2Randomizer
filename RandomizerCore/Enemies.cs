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
    public const int Fairy = 0x00;
    public const int LockedDoor = 0x02;
    public const int Myu = 0x03;
    public const int Bot = 0x04;
}

/// <summary>
/// IDs that are shared in palace 1-6
/// 
/// It's a class of constants and not an enum, so that
/// EnemiesRegularPalace.Fairy == EnemiesPalace125.Fairy is true.
/// </summary>
public static class EnemiesRegularPalaceShared
{
    public const int Fairy = 0x00;
    public const int ItemInPalaceSprite = 0x01;
    public const int LockedDoor = 0x02;
    public const int Myu = 0x03;
    public const int Bot = 0x04;
    public const int StrikeForRedJar = 0x05; // close enough
    public const int SlowBubble = 0x06;
    public const int OrangeMoa = 0x07;
    public const int FallingBlockGenerator = 0x08;
    public const int SingleFallingBlock = 0x09;

    public const int TinsuitGenerator = 0x0B;
    public const int Tinsuit = 0x0C;
    public const int Dripper = 0x0D;
    public const int FastBubble = 0x0E;

    public const int Elevator = 0x13;
    public const int CrystalSpot = 0x14;
    public const int Crystal = 0x15;
    public const int EnergyBallShooterDownRight = 0x16;
    public const int EnergyBallShooterDownLeft = 0x17;
    public const int OrangeIronKnuckle = 0x18;
    public const int RedIronKnuckle = 0x19;
    public const int BlueIronKnuckle = 0x1A;
    public const int WolfHeadGenerator = 0x1B;
    public const int WolfHead = 0x1C;
    public const int RedStalfos = 0x1F; // close enough
    public const int BlueStalfos = 0x23; // close enough
}

/// <summary>
/// IDs that are shared in all palaces, including Great Palace.
/// 
/// It's a class of constants and not an enum, so that
/// EnemiesAnyPalace.Fairy == EnemiesPalace125.Fairy is true.
/// </summary>
public static class EnemiesPalaceShared
{
    public const int Fairy = 0x00;
    public const int LockedDoor = 0x02;
    public const int Myu = 0x03;
    public const int Bot = 0x04;
    public const int Elevator = 0x13;
}

/// <summary>
/// IDs for Palace Group 1 (Palace 1, 2, 5)
/// </summary>
public enum EnemiesPalace125
{
    Fairy = 0x00,
    ItemInPalaceSprite = 0x01,
    LockedDoor = 0x02,
    Myu = 0x03,
    Bot = 0x04,
    StrikeForRedJar = 0x05,
    SlowBubble = 0x06,
    OrangeMoa = 0x07,
    FallingBlockGenerator = 0x08,
    SingleFallingBlock = 0x09,
    BlueDragonHeadGenerator = 0x0A,
    TinsuitGenerator = 0x0B,
    Tinsuit = 0x0C,
    Dripper = 0x0D,
    FastBubble = 0x0E,
    BagoBagoGenerator = 0x0F,
    BagoBago = 0x10,
    RopeJumpingOnly = 0x11,
    RopeJumpingAndMoving = 0x12,
    Elevator = 0x13,
    CrystalSpot = 0x14,
    Crystal = 0x15,
    EnergyBallShooterDownRight = 0x16,
    EnergyBallShooterDownLeft = 0x17,
    OrangeIronKnuckle = 0x18,
    RedIronKnuckle = 0x19,
    BlueIronKnuckle = 0x1A,
    WolfHeadGenerator = 0x1B,
    WolfHead = 0x1C,
    Mago = 0x1D,
    Guma = 0x1E,
    RedStalfos = 0x1F,
    Horsehead = 0x20,
    HelmetheadOrGooma = 0x21,
    HelmetheadsFloatingHead = 0x22,
    BlueStalfos = 0x23
}

/// <summary>
/// IDs for Palace Group 2 (Palace 3, 4, 6)
/// </summary>
public enum EnemiesPalace346
{
    Fairy = 0x00,
    ItemInPalaceSprite = 0x01,
    LockedDoor = 0x02,
    Myu = 0x03,
    Bot = 0x04,
    StrikeForRedJarOrIronKnuckle = 0x05,
    SlowBubble = 0x06,
    OrangeMoa = 0x07,
    FallingBlockGenerator = 0x08,
    SingleFallingBlock = 0x09,
    RebonackPostHorse = 0x0A,
    TinsuitGenerator = 0x0B,
    Tinsuit = 0x0C,
    Dripper = 0x0D,
    FastBubble = 0x0E,
    BlueDragonHeadGenerator = 0x0F,
    Flame = 0x10,
    Rope = 0x11,
    Elevator = 0x13,
    CrystalSpot = 0x14,
    Crystal = 0x15,
    EnergyBallShooterDownRight = 0x16,
    EnergyBallShooterDownLeft = 0x17,
    OrangeIronKnuckle = 0x18,
    RedIronKnuckle = 0x19,
    BlueIronKnuckle = 0x1A,
    WolfHeadGenerator = 0x1B,
    WolfHead = 0x1C,
    Wizard = 0x1D,
    Doomknocker = 0x1E,
    ArmoredRedStalfos = 0x1F,
    Rebonack = 0x20,
    Barba = 0x21,
    Carock = 0x22,
    ArmoredBlueStalfos = 0x23
}

/// <summary>
/// IDs for Palace Group 3 (Great Palace)
/// </summary>
public enum EnemiesGreatPalace
{
    Fairy = 0x00,
    StrikeForRedJarOrFokka = 0x01,
    LockedDoor = 0x02,
    Myu = 0x03,
    Bot = 0x04,
    Bit = 0x05,
    OrangeMoa = 0x06,
    Ache = 0x07,
    Acheman = 0x0A,
    BubbleGenerator = 0x0B,
    RockGenerator = 0x0C,
    RedDeeler = 0x0D,
    BlueDeeler = 0x0E,
    FireBagoBagoGenerator = 0x0F,
    FireBagoBago = 0x10,
    RopeJumpingOnly = 0x11,
    RopeJumpingAndMoving = 0x12,
    Elevator = 0x13,
    SlowBubble = 0x14,
    FastBubble = 0x15,
    OrangeDragonHeadGenerator = 0x16,
    KingBubble = 0x17,
    OrangeFokka = 0x18,
    RedFokka = 0x19,
    BlueFokka = 0x1A,
    Fokkeru = 0x1D,
    KingBot = 0x1E,
    GreatPalaceBarrier = 0x20,
    Thunderbird = 0x22,
    DarkLinkTrigger = 0x23
}
