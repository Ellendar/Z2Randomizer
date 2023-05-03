using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Core;

public class Enemies
{
    public static readonly int NormalPalaceEnemyAddr = 0x108B0;
    public static readonly int GPEnemyAddr = 0x148B0;
    //Palaces 1,2,5
    public static readonly int Palace125EnemyPtr = 0x105B1;
    //Palaces 3,4,6
    public static readonly int Palace346EnemyPtr = 0x1208E;
    //GP
    public static readonly int GPEnemyPtr = 0x145B1;

	public const int MAXIMUM_ENEMY_BYTES = 0x400;
	public const int MAXIMUM_ENEMY_BYTES_GP = 681;



    public static readonly int[] Palace125Enemies = new int[] { 3, 4, 12, 17, 18, 24, 25, 26, 29, 0x1E, 0x1F, 0x23 };
    public static readonly int[] Palace125FlyingEnemies = new int[] { 0x06, 0x07, 0x0E };
    public static readonly int[] Palace125Generators = new int[] { 0x0B, 0x0F, 0x1B, 0x0A };
    public static readonly int[] Palace125SmallEnemies = new int[] { 0x03, 0x04, 0x11, 0x12 };
    public static readonly int[] Palace125LargeEnemies = new int[] { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1E, 0x1F, 0x23 };
    public static readonly int[] Palace346Enemies = new int[] { 3, 4, 12, 17, 24, 25, 26, 29, 0x1F, 0x1E, 0x23 };
    public static readonly int[] Palace346FlyingEnemies = new int[] { 0x06, 0x07, 0x0E };
    public static readonly int[] Palace346Generators = new int[] { 0x0B, 0x1B, 0x0F };
    public static readonly int[] Palace346SmallEnemies = new int[] { 0x03, 0x04, 0x11 };
    public static readonly int[] Palace346LargeEnemies = new int[] { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1F, 0x1E, 0x23 };
    public static readonly int[] GPEnemies = new int[] { 3, 4, 17, 18, 24, 25, 26, 0x1D };
    public static readonly int[] GPFlyingEnemies = new int[] { 0x06, 0x14, 0x15, 0x17, 0x1E };
    public static readonly int[] GPGenerators = new int[] { 0x0B, 0x0C, 0x0F, 0x16 };
    public static readonly int[] GPSmallEnemies = new int[] { 0x03, 0x04, 0x11, 0x12 };
    public static readonly int[] GPLargeEnemies = new int[] { 0x18, 0x19, 0x1A, 0x1D };

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
Palace 1,2,5
	00 - Fairy
	01 - Item in Palace Sprite
	02 - Locked Door
	03 - Myu
	04 - Bot
	05 - Strike for Red Jar
	06 - Slow Magic Stealing Skull
	07 - Orange Moa
	08 - Falling Block Generator
	09 - Single falling block
	0A - Blue Skull Head
	0B - Rathead Generator
	0C - Rathead
	0D - Dripping Column
	0E - Fast Magic Stealing Skull
	0F - Bago Bago Generator
	10 - Bago Bago
	11 - Rope Jumping Only
	12 - Rope Jumping and Moving
	13 - Elevator
	14 - Crystal Slot
	15 - Crystal
	16 - Energy ball shooter (to down-right)
	17 - Energy ball shooter (to down-left)
	18 - Orange Iron Knuckle
	19 - Red Iron Knuckle
	1A - Blue Iron Knuckle
	1B - Wolf Head Generator
	1C - Wolf Head
	1D - Mago
	1E - Hammer Thrower
	1F - Red Stalfos
	20 - Horse Head
	21 - Helmethead/Gooma (5th Palace Boss) 
	22 - Helmethead's floating head
	23 - Blue Stalfos
	24-FF crash
Palace 3,4,6
	00 - Fairy
	01 - Item in Palace sprite
	02 - Locked Door
	03 - Myu
	04 - Bot
	05 - Strike for Red Jar/Ironknuckle
	06 - Slow Magic Stealing Skulls
	07 - Orange Moa
	08 - Falling Blocks Generator
	09 - Single falling block
	0A - Blue Skull Head
	0B - Rathead Generator
	0C - Rathead
	0D - Dripping Column
	0E - Fast Magic Stealing Skulls
	0F - ???
	10 - Flame
	11 - Rope
	12 - ???
	13 - Elevator
	14 - Crystal Slot
	15 - Crystal
	16 - Energy ball shooter (to down-right)
	17 - Energy ball shooter (to down-left)
	18 - Orange Iron Knuckle
	19 - Red Iron Knuckle
	1A - Blue Iron Knuckle
	1B - Wolf Head Generator
	1C - Wolf Head
	1D - Wizrobe
	1E - Red Stalfos Knight
	1F - Doomknocker
	20 - Blue Iron Knuckle on horse
	21 - Dragon (6th palace boss)
	22 - Wizard (4th palace boss)
	23 - Blue Stalfos Knight

	00 - Fairy
	01 - Candle
	02 - Door
	03 - Myu
	04 - Bot
	05 - Strike for Red Jar/Ironknuckle
	06 - Bubble
	07 - Orange Moa
	08 - Falling blocks
	09 - Single falling block
	0A - Unicorn Head
	0B - Rat Head generator
	0C - Rat Head
	0D - Column dripper
	0E - Fast bubble
	0F - ? nothing
	10 - Fire
	11 - snake/octorock thingy
	12 - ? nothing
	13 - elevator
	14 - crystal return
	15 - crystal
	16 - unicorn head shooter (to down-right)
	17 - unicorn head shooter (to down-left)
	18 - Orange Ironknuckle
	19 - Red Ironknuckle
	1A - Blue Ironknuckle
	1B - red wolf head generator
	1C - wolf head
	1D - reflect wizrobe
	1E - Red stalfos
	1F - blue boomerang-club thing
	20 - Ironknuckle on horse
	21 - 6th palace boss
	22 - 4th palace boss
	23 - Blue stalfos
	24-FF crash
Great Palace (NOT VERIFED!)
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
	0C - Rock Generator
	0D - Red Deeler
	0E - Blue Deeler
	0F - Fire Bago Bago Generator
	10 - Fire Bago Bago
	11 - Fire Rope Jumping Only
	12 - Fire Rope Jumping and Moving
	13 - Elevator
	14 - Slow Magic Stealing Skull
	15 - Fast Magic Stealing Skull
	16 - Orange Skull Head
	17 - Large Magic Stealing Skull
	18 - Orange Dreadhawk
	19 - Red Dreadhawk
	1A - Blue Dreadhawk
	1B - ???
	1C - ???
	1D - Firebird
	1E - King Bot
	1F - ???
	20 - Barrier for Grand Palace
	21 - ???
	22 - Thunder Bird Boss
	23 - Trigger for Dark Link battle 
*/
}
