using NLog;
using NLog.Fluent;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z2Randomizer.Overworld;
using Z2Randomizer.Sidescroll;

namespace Z2Randomizer;

/*
 * 
Change List:
    Removed items are no longer replaced with a pbag, they can be replaced with any small item
    Implemented a level cap, along with an option to scale xp requirements to the level cap
    Added an option for town signs to tell you what spell is in the town (thanks, Thirwolf!)
    Added an option to reduce the encounter spawn rate or turn them off entirely
    Introduced new exp shuffling levels
    Added an option to allow users to select their starting atk/mag/life level
    Added options to shuffle how continents are connected
    Added new overworld biomes (Islands, Canyon, Caldera, Volcano, Mountainous)
    Added an option to create new palaces
    Added the ability to use community based rooms in the palaces (thanks GTM, Scorpion__Max, eonHck, aaron2u2, TKnightCrawler, Duster, Link_7777)
    Reintroduced vanila maps
    Removed a few more extraneous rooms from spell houses
    Added an option to remap Up+A to controller 1
    Added an option to allow less important locations to be hidden on the overworld
    Added an option to restrict how connection caves are placed
    Added an option to allow connection caves to be boulder blocked
    Added an option to randomize which locations are hidden in New Kasuto/Hidden Palace spots
    Added an option to remove the flashing death screen
    Added an option to randomly select character sprite
    Added an option to randomize the item dropped by bosses
    Added an option to generate Bagu's Woods
    Fixed softlock when dying while talking (thanks eon!)
    Fixed a bug that robbed you of 1 attack power at attack 5 in rare situations
    Added the Yoshi, Dragonlord, Miria, Crystalis sprites (thanks TKnightCrawler!)
    Added the Pyramid sprite (thanks Plan!)
    Added the GliitchWiitch sprite (thanks RandomSaliance!)
    Added the Lady Link sprite (thanks eonHck!)
    Added the Hoodie Link sprite (thanks gtm!)
    Added the Taco sprite (thanks Warlock!)

    Double clicking on the box with the flags selects all of the text automatically
    Removed options to shuffle xp of bosses and enemies seperately
    Changed UI for level effectiveness
    Continued my never ending quest to get tooltips right
    UI now updates progress of seed generation


Bug List:
    funky grass encounter in east hyrule
    death mountain encounters

Todo List for version 4.0:
    Mess with drop rooms
    Add more rooms
    Update tooltips / Make sure UI is reasonable
    Update documentation

Bugs in 4.0
   

Feature List:

    Item shuffling
        More extreme item shuffling
        Duplicate items? (maybe a bad idea...)
    Move towns accross continents (tried this and it is very hard)
        Notes:
            Must swap map and world bytes for this to work
            Must update sanity checker
            How to deal with life town?
            How to deal with new kasuto?
    Palace columns tell you info about the palace
    New experience amounts *
    Shorten Towns
    More extreme enemy shuffle
        Slider? (easy, medium, crazy)
    Overworld Generation improvements
        Continent sizes (maybe just for maze island?)
    Randomize overworld caves
    Swap up/downstab
    Allow jars to restore health
    Allow fairies to restore magic
    New graphics/tile sets/etc.
    Spells as items?
    Tri-state checkboxes?
    Random%        
*/

public class Hyrule
{
    //IMPORTANT: Tuning these factors can have a big impact on generation times.
    //In general, the non-terrain shuffle features are much cheaper than the cost of generating terrain or verifying the seed.
    
    //This controls how many attempts will be made to shuffle the non-terrain features like towns, spells, and items.
    //The higher you set it, the more likely a given terrain is to find a set of items that works, resulting in fewer terrain generations.
    //It will also increase the number of seeds that have more arcane solutions, where only a specific item route works.
    //This was originally set to 10, but increasing it to 100 massively reduces the number of extremely degenerate caldera and mountain generation times
    private const int NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT = 40;

    //This controls how many times 
    private const int NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT = 10;

    public const bool UNSAFE_DEBUG = false;

    private readonly Item[] SHUFFLABLE_STARTING_ITEMS = new Item[] { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HAMMER, Item.MAGIC_KEY };

    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly int[] fireLocs = { 0x20850, 0x22850, 0x24850, 0x26850, 0x28850, 0x2a850, 0x2c850, 0x2e850, 0x36850, 0x32850, 0x34850, 0x38850 };

    private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
    private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

 
    //private ROM romData;
    private const int overworldXOff = 0x3F;
    private const int overworldMapOff = 0x7E;
    private const int overworldWorldOff = 0xBD;

    //Unused
    private Dictionary<int, int> spellEnters;
    //Unused
    private Dictionary<int, int> spellExits;
    //Unused
    public HashSet<String> reachableAreas;
    //Which vanilla spell corresponds with which shuffled spell
    private Dictionary<Spell, Spell> spellMap; //key is location, value is spell (this is a bad implementation)
    //Locations that contain an item
    private List<Location> itemLocs;
    //Locations that are pbags in vanilla that are turned into hearts because maxhearts - startinghearts > 4
    private List<Location> pbagHearts;
    //Continent connectors
    protected Dictionary<Location, List<Location>> connections;
    //public SortedDictionary<String, List<Location>> areasByLocation;
    //public Dictionary<Location, String> section;
    private int accessibleMagicContainers;
    private int heartContainers;
    private int startHearts;
    private int maxHearts;
    private int heartContainersInItemPool;
    private int kasutoJars;
    private BackgroundWorker worker;
    
    //private Character character;

    public Dictionary<Item, bool> itemGet = new Dictionary<Item, bool>();
    //private bool[] spellGet;
    private readonly int enemyAddr1 = 0x108B0;
    private readonly int enemyAddr2 = 0x148B0;
    private readonly int enemyPtr1 = 0x105B1;
    private readonly int enemyPtr2 = 0x1208E;
    private readonly int enemyPtr3 = 0x145B1;
    private readonly List<int> enemies1 = new List<int> { 3, 4, 12, 17, 18, 24, 25, 26, 29, 0x1E, 0x1F, 0x23 };
    private readonly List<int> flyingEnemies1 = new List<int> { 0x06, 0x07, 0x0E };
    private readonly List<int> generators1 = new List<int> { 0x0B, 0x0F, 0x1B, 0x0A };
    private readonly List<int> shorties1 = new List<int> { 0x03, 0x04, 0x11, 0x12 };
    private readonly List<int> tallGuys1 = new List<int> { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1E, 0x1F, 0x23 };
    private readonly List<int> enemies2 = new List<int> { 3, 4, 12, 17, 24, 25, 26, 29, 0x1F, 0x1E, 0x23 };
    private readonly List<int> flyingEnemies2 = new List<int> { 0x06, 0x07, 0x0E };
    private readonly List<int> generators2 = new List<int> { 0x0B, 0x1B, 0x0F };
    private readonly List<int> shorties2 = new List<int> { 0x03, 0x04, 0x11 };
    private readonly List<int> tallGuys2 = new List<int> { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1F, 0x1E, 0x23 };
    private readonly List<int> enemies3 = new List<int> { 3, 4, 17, 18, 24, 25, 26, 0x1D };
    private readonly List<int> flyingEnemies3 = new List<int> { 0x06, 0x14, 0x15, 0x17, 0x1E };
    private readonly List<int> generators3 = new List<int> { 0x0B, 0x0C, 0x0F, 0x16 };
    private readonly List<int> shorties3 = new List<int> { 0x03, 0x04, 0x11, 0x12 };
    private readonly List<int> tallGuys3 = new List<int> { 0x18, 0x19, 0x1A, 0x1D };
    private List<int> visitedEnemies;

    public WestHyrule westHyrule;
    public EastHyrule eastHyrule;
    private MazeIsland mazeIsland;
    private DeathMountain deathMountain;

    private Shuffler shuffler;
    private RandomizerProperties props;
    public List<World> worlds;
    public List<Palace> palaces;
    public List<Room> rooms;
    public bool startKid;
    public bool startTrophy;
    public bool startMed;

    //DEBUG/STATS
    public DateTime startTime = DateTime.Now;
    public DateTime startRandomizeStartingValuesTimestamp;
    public DateTime startRandomizeEnemiesTimestamp;
    public DateTime firstProcessOverworldTimestamp;

    public int totalReachabilityOverworldAttempts = 0;
    public int totalContinentConnectionOverworldAttempts = 0;
    public int totalWestGenerationAttempts = 0;
    public int totalEastGenerationAttempts = 0;
    public int totalMazeIslandGenerationAttempts = 0;
    public int totalDeathMountainGenerationAttempts = 0;
    public int isEverythingReachableFailures = 0;

    public int itemGetReachableFailures = 0;
    public int spellGetReachableFailures = 0;
    public int heartContainerReachableFailures = 0;
    public int magicContainerReachableFailures = 0;
    public int logicallyRequiredLocationsReachableFailures = 0;

    public int timeSpentBuildingWest = 0;
    public int timeSpentBuildingEast = 0;
    public int timeSpentBuildingDM = 0;
    public int timeSpentBuildingMI = 0;

    public int debug = 0;


    private readonly SortedDictionary<int, int> palaceConnectionLocs = new SortedDictionary<int, int>
    {
        {1, 0x1072B},
        {2, 0x1072B},
        {3, 0x12208},
        {4, 0x12208},
        {5, 0x1072B},
        {6, 0x12208},
        {7, 0x1472B},
    };

    private readonly Dictionary<int, int> palaceAddr = new Dictionary<int, int>
    {
        {1, 0x4663 },
        {2, 0x4664 },
        {3, 0x4665 },
        {4, 0xA140 },
        {5, 0x8663 },
        {6, 0x8664 },
        {7, 0x8665 }
    };

    public ROM ROMData { get; set; }
    public Dictionary<Spell, bool> SpellGet { get; set; }
    public Random RNG { get; set; }
    public string Flags { get; private set; }
    public int Seed { get; private set; }
    public RandomizerProperties Props
    {
        get
        {
            return props;
        }

        set
        {
            props = value;
        }
    }

    public Hyrule(RandomizerConfiguration config, BackgroundWorker worker, bool saveRom = true)
    {
        WestHyrule.ResetStats();
        RNG = new Random(config.Seed);
        props = config.Export(RNG);
        props.saveRom = saveRom;
        Flags = config.Serialize();
        Seed = config.Seed;
        logger.Info("Started generation for " + Flags + " / " + config.Seed);

        ROMData = new ROM(props.Filename);
        //ROMData.dumpAll("glitch");
        //ROMData.dumpSamus();
        //Palace.DumpMaps(ROMData);
        this.worker = worker;


        //character = new Character(props);
        shuffler = new Shuffler(props);

        palaces = new List<Palace>();
        itemGet = new Dictionary<Item, bool>();
        foreach(Item item in Enum.GetValues(typeof(Item)))
        {
            itemGet.Add(item, false);
        }
        SpellGet = new Dictionary<Spell, bool>();
        foreach(Spell spell in Enum.GetValues(typeof(Spell)))
        {
            SpellGet.Add(spell, false);
        }
        reachableAreas = new HashSet<string>();
        //areasByLocation = new SortedDictionary<string, List<Location>>();

        kasutoJars = shuffler.ShuffleKasutoJars(ROMData, RNG);
        //ROMData.moveAfterGem();

        //Allows casting magic without requeueing a spell
        if (props.FastCast)
        {
            ROMData.WriteFastCastMagic();
        }

        if (props.DisableMusic)
        {
            ROMData.DisableMusic();
        }

        ROMData.DoHackyFixes();
        shuffler.ShuffleDrops(ROMData, RNG);
        shuffler.ShufflePbagAmounts(ROMData, RNG);

        ROMData.FixSoftLock();
        ROMData.ExtendMapSize();
        ROMData.DisableTurningPalacesToStone();
        ROMData.UpdateMapPointers();
        ROMData.FixContinentTransitions();

        if (props.DashSpell)
        {
            ROMData.DashSpell();
        }

        /*
        Up + A:
        1cbba(cbaa): insert jump to d39a (1d3aa) (209ad3)
        1d3aa(d39a): store 707(8D0707) compare to 3(c903) less than 2 jump(3012) Load FB1 (ADb10f)compare with zero(c901) branch if zero(f00B) Load 561(AD6105) store accumulator into side memory(8Db00f) load accumulator with 1(a901) store to fb1(8db10f) return (60)
        d3bc(1d3cc): Load accumulator with fbo (adb00f)store to 561(8d6105) load 707(AD0707) return (60)
        feb3(1fec3): Store y into 707(8c0707) load 0(a900) stor into fb1(8db10f) return (60)
        CAD0(1CAE0): (20bcd3) c902 10
        CAE3(1CAF3): NOP NOP NOP(EAEAEA)
        CF92: (1CFA2): Jump to feb3(20b3fe)

        */

        if(props.UpAC1)
        {
            ROMData.UpAController1();
        }
        if (props.UpaBox)
        {
            ROMData.Put(0x1cbba, 0x20);
            ROMData.Put(0x1cbbb, 0x9a);
            ROMData.Put(0x1cbbc, 0xd3);

            ROMData.Put(0x1d3aa, 0x8d);
            ROMData.Put(0x1d3ab, 0x07);
            ROMData.Put(0x1d3ac, 0x07);
            ROMData.Put(0x1d3ad, 0xad);
            ROMData.Put(0x1d3ae, 0x07);
            ROMData.Put(0x1d3af, 0x07);
            ROMData.Put(0x1d3b0, 0xc9);
            ROMData.Put(0x1d3b1, 0x03);
            ROMData.Put(0x1d3b2, 0x30);
            ROMData.Put(0x1d3b3, 0x12);
            ROMData.Put(0x1d3b4, 0xad);
            ROMData.Put(0x1d3b5, 0xb0);
            ROMData.Put(0x1d3b6, 0x0f);
            ROMData.Put(0x1d3b7, 0xc9);
            ROMData.Put(0x1d3b8, 0x01);
            ROMData.Put(0x1d3b9, 0xf0);
            ROMData.Put(0x1d3ba, 0x0b);
            ROMData.Put(0x1d3bb, 0xad);
            ROMData.Put(0x1d3bc, 0x61);
            ROMData.Put(0x1d3bd, 0x05);
            ROMData.Put(0x1d3be, 0x8d);
            ROMData.Put(0x1d3bf, 0xb1);
            ROMData.Put(0x1d3c0, 0x0f);
            ROMData.Put(0x1d3c1, 0xa9);
            ROMData.Put(0x1d3c2, 0x01);
            ROMData.Put(0x1d3c3, 0x8D);
            ROMData.Put(0x1d3c4, 0xB0);
            ROMData.Put(0x1d3c5, 0x0F);
            ROMData.Put(0x1d3c6, 0xad);
            ROMData.Put(0x1d3c7, 0x07);
            ROMData.Put(0x1d3c8, 0x07);
            ROMData.Put(0x1d3c9, 0x29);
            ROMData.Put(0x1d3ca, 0x07);
            ROMData.Put(0x1d3cb, 0x60);
            ROMData.Put(0x1d3cc, 0xad);
            ROMData.Put(0x1d3cd, 0xb1);
            ROMData.Put(0x1d3ce, 0x0f);
            ROMData.Put(0x1d3cf, 0x8d);
            ROMData.Put(0x1d3d0, 0x61);
            ROMData.Put(0x1d3d1, 0x05);
            ROMData.Put(0x1d3d2, 0x20);
            ROMData.Put(0x1d3d3, 0x57);
            ROMData.Put(0x1d3d4, 0xa0);
            ROMData.Put(0x1d3d5, 0xad);
            ROMData.Put(0x1d3d6, 0x07);
            ROMData.Put(0x1d3d7, 0x07);
            ROMData.Put(0x1d3d8, 0x60);

            //feb3(1fec3): Store y into 707(8c0707) load 0(a900) stor into fb1(8db10f) return (60)
            ROMData.Put(0x1feca, 0x8c);
            ROMData.Put(0x1fecb, 0x07);
            ROMData.Put(0x1fecc, 0x07);
            ROMData.Put(0x1fecd, 0xa9);
            ROMData.Put(0x1fece, 0x00);
            ROMData.Put(0x1fecf, 0x8d);
            ROMData.Put(0x1fed0, 0xb0);
            ROMData.Put(0x1fed1, 0x0f);
            ROMData.Put(0x1fed2, 0x60);

            //CAD0(1CAE0): (20b7d3) c902 10
            ROMData.Put(0x1cae0, 0x20);
            ROMData.Put(0x1cae1, 0xbc);
            ROMData.Put(0x1cae2, 0xd3);
            ROMData.Put(0x1cae3, 0xc9);
            ROMData.Put(0x1cae4, 0x03);
            ROMData.Put(0x1cae5, 0x10);

            //CAE3(1CAF3): NOP NOP NOP(EAEAEA)
            ROMData.Put(0x1caf3, 0xea);
            ROMData.Put(0x1caf4, 0xea);
            ROMData.Put(0x1caf5, 0xea);

            //CF92: (1CFA2): Jump to feba(20bafe)
            ROMData.Put(0x1cfa2, 0x20);
            ROMData.Put(0x1cfa3, 0xba);
            ROMData.Put(0x1cfa4, 0xfe);
        }

        if (props.PermanentBeam)
        {
            ROMData.Put(0x186c, 0xEA);
            ROMData.Put(0x186d, 0xEA);
        }

        if (props.StandardizeDrops)
        {
            ROMData.Put(0x1e8bd, 0x20);
            ROMData.Put(0x1e8be, 0x4c);
            ROMData.Put(0x1e8bf, 0xff);

            ROMData.Put(0x1ff5c, 0xc0);
            ROMData.Put(0x1ff5d, 0x02);
            ROMData.Put(0x1ff5e, 0xd0);
            ROMData.Put(0x1ff5f, 0x07);
            ROMData.Put(0x1ff60, 0xad);
            ROMData.Put(0x1ff61, 0xfe);
            ROMData.Put(0x1ff62, 0x06);
            ROMData.Put(0x1ff63, 0xee);
            ROMData.Put(0x1ff64, 0xfe);
            ROMData.Put(0x1ff65, 0x06);
            ROMData.Put(0x1ff66, 0x60);
            ROMData.Put(0x1ff67, 0xad);
            ROMData.Put(0x1ff68, 0xff);
            ROMData.Put(0x1ff69, 0x06);
            ROMData.Put(0x1ff6a, 0xee);
            ROMData.Put(0x1ff6b, 0xff);
            ROMData.Put(0x1ff6c, 0x06);
            ROMData.Put(0x1ff6d, 0x60);

        }

        //load 706 (3) (AD0607)
        //cmp 03 (2) (c903)
        //bne 2 (2) (d0 03)
        //store 01 in 706 (3) (
        //do the math (3)
        //push to stack (1)
        //load 70a (3)
        //store to 706 (3)
        //pop stack (1)
        //rts (1)

        spellEnters = new Dictionary<int, int>();
        spellEnters.Add(0, 0x90);
        spellEnters.Add(1, 0x94);
        spellEnters.Add(2, 0x98);
        spellEnters.Add(3, 0x9C);
        spellEnters.Add(4, 0xA0);
        spellEnters.Add(5, 0xA4);
        spellEnters.Add(6, 0x4D);
        spellEnters.Add(7, 0xAC);
        spellEnters.Add(8, 0xB0);
        spellEnters.Add(9, 0xB4);

        spellExits = new Dictionary<int, int>();
        spellExits.Add(0, 0xC1);
        spellExits.Add(1, 0xC5);
        spellExits.Add(2, 0xC9);
        spellExits.Add(3, 0xCD);
        spellExits.Add(4, 0xD1);
        spellExits.Add(5, 0xD5);
        spellExits.Add(6, 0x6A);
        spellExits.Add(7, 0xDD);
        spellExits.Add(8, 0xE1);
        spellExits.Add(9, 0xE5);

        ShortenWizards();
        accessibleMagicContainers = 4;
        visitedEnemies = new List<int>();

        startRandomizeStartingValuesTimestamp = DateTime.Now;
        RandomizeStartingValues();
        startRandomizeEnemiesTimestamp = DateTime.Now;
        RandomizeEnemies();


        palaces = Palaces.CreatePalaces(worker, RNG, props, ROMData);
        while(palaces == null || palaces.Count != 7)
        {
            if (palaces == null)
            {
                return;
            }
            palaces = Palaces.CreatePalaces(worker, RNG, props, ROMData);
            
        }
        if (props.ShufflePalaceEnemies)
        {
            ShuffleEnemies(enemyPtr1, enemyAddr1, enemies1, generators1, shorties1, tallGuys1, flyingEnemies1, false);
            ShuffleEnemies(enemyPtr2, enemyAddr1, enemies2, generators2, shorties2, tallGuys2, flyingEnemies2, false);
            ShuffleEnemies(enemyPtr3, enemyAddr2, enemies3, generators3, shorties3, tallGuys3, flyingEnemies3, true);
        }

        firstProcessOverworldTimestamp = DateTime.Now;
        ProcessOverworld();
        bool f = UpdateProgress(8);
        if (!f)
        {
            return;
        }
        List<Hint> hints = ROMData.GetGameText();
        ROMData.WriteHints(Hints.GenerateHints(itemLocs, startTrophy, startMed, startKid, spellMap, westHyrule.bagu, hints, props, RNG));
        f = UpdateProgress(9);
        if (!f)
        {
            return;
        }
        MD5 hasher = MD5.Create();
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(
            Flags + 
            Seed + 
            typeof(MainUI).Assembly.GetName().Version.Major + 
            typeof(MainUI).Assembly.GetName().Version.Minor +
            File.ReadAllText(config.GetRoomsFile())
        ));
        UpdateRom(hash);
        String newFileName = props.Filename.Substring(0, props.Filename.LastIndexOf("\\") + 1) + "Z2_" + Seed + "_" + Flags + ".nes";
        if(props.saveRom)
        {
            ROMData.Dump(newFileName);
        }

        if (UNSAFE_DEBUG)
        {
            PrintSpoiler(LogLevel.Info);
            //DEBUG
            StringBuilder sb = new();
            foreach (Palace palace in palaces)
            {
                sb.AppendLine("Palace: " + palace.Number);
                foreach (Room room in palace.AllRooms)
                {
                    sb.AppendLine(room.Debug());
                }
            }
            File.WriteAllText("rooms.log", sb.ToString());
        }
    }



    /*
        Text Notes:
        
        Community Text Changes
        ----------------------
        Shield Spell    15  43
        Cannot Help     16  35
        Jump Spell      24  34
        Life Spell      35  37
        You know..?     37  42
        Fairy           46  37
        Downstab        47  38
        Bagu            48  44
        Fire            70  43
        You know        71  34
        Reflect         81  37
        Upstab          82  32
        Spell           93  25
        Thunder         96  36 
    */
    

    private void ShuffleAttackEffectiveness(bool ohko)
    {
        if (!ohko)
        {
            int[] atk = new int[8];
            for (int i = 0; i < 8; i++)
            {
                atk[i] = ROMData.GetByte(0x1E67D + i);
            }

            for (int i = 0; i < atk.Length; i++)
            {
                int minAtk = (int)Math.Ceiling(atk[i] - atk[i] * .333);
                int maxAtk = (int)(atk[i] + atk[i] * .5);
                int next = atk[i];

                if (props.AttackEffectiveness == StatEffectiveness.AVERAGE)
                {
                    next = RNG.Next(minAtk, maxAtk);
                }
                else if (props.AttackEffectiveness == StatEffectiveness.HIGH)
                {
                    next = (int)(atk[i] + (atk[i] * .4));
                }
                else if (props.AttackEffectiveness == StatEffectiveness.LOW)
                {
                    next = (int)(atk[i] * .5);
                }

                if (props.AttackEffectiveness == StatEffectiveness.AVERAGE)
                {
                    if (i == 0)
                    {
                        atk[i] = Math.Max(next, 2);
                    }
                    else
                    {
                        if (next < atk[i - 1])
                        {
                            atk[i] = atk[i - 1];
                        }
                        else
                        {
                            atk[i] = next;
                        }
                    }
                }
                else
                {
                    atk[i] = next;
                }
            }


            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(0x1E67D + i, (Byte)atk[i]);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(0x1E67D + i, (Byte)192);
            }
        }
    }

    private void ShuffleItems()
    {
        List<Item> itemList = new List<Item> { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MEDICINE, Item.TROPHY, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MAGIC_KEY, Item.MAGIC_CONTAINER, Item.HAMMER, Item.CHILD, Item.MAGIC_CONTAINER };
        List<Item> smallItems = new List<Item> { Item.BLUE_JAR, Item.RED_JAR, Item.SMALL_BAG, Item.MEDIUM_BAG, Item.LARGE_BAG, Item.XL_BAG, Item.ONEUP, Item.KEY };
        Location kidLoc = mazeIsland.kid;
        Location medicineLoc = westHyrule.medCave;
        Location trophyLoc = westHyrule.trophyCave;
        heartContainersInItemPool = maxHearts - startHearts;
        
        foreach(Item item in itemGet.Keys.ToList())
        {
            itemGet[item] = false;
        }
        foreach (Location location in itemLocs)
        {
            location.itemGet = false;
        }
        westHyrule.pbagCave.itemGet = false;
        eastHyrule.pbagCave1.itemGet = false;
        eastHyrule.pbagCave2.itemGet = false;

        ROMData.Put(RomMap.START_CANDLE, props.StartCandle ? (Byte)1 : (Byte)0);
        itemGet[Item.CANDLE] = props.StartCandle;
        ROMData.Put(RomMap.START_GLOVE, props.StartGlove ? (Byte)1 : (Byte)0);
        itemGet[Item.GLOVE] = props.StartGlove;
        ROMData.Put(RomMap.START_RAFT, props.StartRaft ? (Byte)1 : (Byte)0);
        itemGet[Item.RAFT] = props.StartRaft;
        ROMData.Put(RomMap.START_BOOTS, props.StartBoots ? (Byte)1 : (Byte)0);
        itemGet[Item.BOOTS] = props.StartBoots;
        ROMData.Put(RomMap.START_FLUTE, props.StartFlute ? (Byte)1 : (Byte)0);
        itemGet[Item.FLUTE] = props.StartFlute;
        ROMData.Put(RomMap.START_CROSS, props.StartCross ? (Byte)1 : (Byte)0);
        itemGet[Item.CROSS] = props.StartCross;
        ROMData.Put(RomMap.START_HAMMER, props.StartHammer ? (Byte)1 : (Byte)0);
        itemGet[Item.HAMMER] = props.StartHammer;
        ROMData.Put(RomMap.START_MAGICAL_KEY, props.StartKey ? (Byte)1 : (Byte)0);
        itemGet[Item.MAGIC_KEY] = props.StartKey;

        itemList = new List<Item> { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MEDICINE, Item.TROPHY, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MAGIC_KEY, Item.MAGIC_CONTAINER, Item.HAMMER, Item.CHILD, Item.MAGIC_CONTAINER };

        if (props.PbagItemShuffle)
        {
            westHyrule.pbagCave.item = (Item)ROMData.GetByte(0x4FE2);
            eastHyrule.pbagCave1.item = (Item)ROMData.GetByte(0x8ECC);
            eastHyrule.pbagCave2.item = (Item)ROMData.GetByte(0x8FB3);
            itemList.Add(westHyrule.pbagCave.item);
            itemList.Add(eastHyrule.pbagCave1.item);
            itemList.Add(eastHyrule.pbagCave2.item);

        }
        pbagHearts = new List<Location>();
        //Replace any unused heart containers with small items
        if (heartContainersInItemPool < 4)
        {
            int heartContainersToAdd = 4 - heartContainersInItemPool;
            while (heartContainersToAdd > 0)
            {
                int remove = RNG.Next(itemList.Count);
                if (itemList[remove] == Item.HEART_CONTAINER)
                {
                    itemList[remove] = smallItems[RNG.Next(smallItems.Count)];
                    heartContainersToAdd--;
                }
            }
        }

        if (heartContainersInItemPool > 4)
        {
            if (props.PbagItemShuffle)
            {
                int heartContainersToAdd = heartContainersInItemPool - 4;
                while (heartContainersToAdd > 0)
                {
                    itemList[22 - heartContainersToAdd] = Item.HEART_CONTAINER;
                    heartContainersToAdd--;
                }
            }
            else
            {
                int x = heartContainersInItemPool - 4;
                while (x > 0)
                {
                    int y = RNG.Next(3);
                    if (y == 0 && !pbagHearts.Contains(westHyrule.pbagCave))
                    {
                        pbagHearts.Add(westHyrule.pbagCave);
                        westHyrule.pbagCave.item = Item.HEART_CONTAINER;
                        itemList.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(westHyrule.pbagCave);
                        x--;
                    }
                    if (y == 1 && !pbagHearts.Contains(eastHyrule.pbagCave1))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave1);
                        eastHyrule.pbagCave1.item = Item.HEART_CONTAINER;
                        itemList.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(eastHyrule.pbagCave1);
                        x--;
                    }
                    if (y == 2 && !pbagHearts.Contains(eastHyrule.pbagCave2))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave2);
                        eastHyrule.pbagCave2.item = Item.HEART_CONTAINER;
                        itemList.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(eastHyrule.pbagCave2);
                        x--;
                    }
                }
            }
        }

        if(props.RemoveSpellItems)
        {
            itemList[9] = smallItems[RNG.Next(smallItems.Count)];
            itemList[10] = smallItems[RNG.Next(smallItems.Count)];
            itemList[17] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.TROPHY] = true;
            itemGet[Item.MEDICINE] = true;
            itemGet[Item.CHILD] = true;

        }

        if (SpellGet[spellMap[Spell.FAIRY]])
        {
            itemList[9] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.MEDICINE] = true;
            startMed = true;
        }

        if(SpellGet[spellMap[Spell.JUMP]])
        {
            itemList[10] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.TROPHY] = true;
            startTrophy = true;
        }

        if(SpellGet[spellMap[Spell.REFLECT]])
        {
            itemList[17] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.CHILD] = true;
            startKid = true;
        }

        //TODO: Clean up the readability of this logic
        if (itemGet[(Item)0])
        {
            itemList[0] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)1])
        {
            itemList[1] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)2])
        {
            itemList[2] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)3])
        {
            itemList[3] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)4])
        {
            itemList[4] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)5])
        {
            itemList[5] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)7])
        {
            itemList[14] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (itemGet[(Item)6])
        {
            itemList[16] = smallItems[RNG.Next(smallItems.Count)];
        }


        if (props.MixOverworldPalaceItems)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                int s = RNG.Next(i, itemList.Count);
                Item sl = itemList[s];
                itemList[s] = itemList[i];
                itemList[i] = sl;
            }
        }
        else
        {
            if (props.ShufflePalaceItems)
            {
                for (int i = 0; i < 6; i++)
                {
                    int s = RNG.Next(i, 6);
                    Item sl = itemList[s];
                    itemList[s] = itemList[i];
                    itemList[i] = sl;
                }
            }
            else
            {
                //Was this place intentionally left blank? If so why the else?
            }

            if (props.ShuffleOverworldItems)
            {
                for (int i = 6; i < itemList.Count; i++)
                {
                    int s = RNG.Next(i, itemList.Count);
                    Item sl = itemList[s];
                    itemList[s] = itemList[i];
                    itemList[i] = sl;
                }
            }
        }
        for (int i = 0; i < itemList.Count; i++)
        {
            itemLocs[i].item = itemList[i];
        }
        foreach (Location location in itemLocs)
        {
            if (location.item == Item.CHILD)
            {
                kidLoc = location;
            }
            else if (location.item == Item.TROPHY)
            {
                trophyLoc = location;
            }
            else if (location.item == Item.MEDICINE)
            {
                medicineLoc = location;
            }
        }

        for (int i = 0; i < 64; i++)
        {
            Byte heartByte = ROMData.GetByte(0x27810 + i);
            ROMData.Put(0x29810 + i, heartByte);
            ROMData.Put(0x2B810 + i, heartByte);
            ROMData.Put(0x2D810 + i, heartByte);
            ROMData.Put(0x33810 + i, heartByte);
            ROMData.Put(0x35810 + i, heartByte);
            ROMData.Put(0x37810 + i, heartByte);
            ROMData.Put(0x39810 + i, heartByte);
        }


    }

    private bool IsEverythingReachable()
    {
        //return true;
        int dm = 0;
        int mi = 0;
        int wh = 0;
        int eh = 0;
        int count = 1;
        int prevCount = 0;
        int loopCount = 0;

        int totalLocationsCount = westHyrule.AllLocations.Count + eastHyrule.AllLocations.Count + deathMountain.AllLocations.Count + mazeIsland.AllLocations.Count;
        //logger.Debug("Locations count: West-" + westHyrule.AllLocations.Count + " East-" + eastHyrule.AllLocations.Count +
        //   " DM-" + deathMountain.AllLocations.Count + " MI-" + mazeIsland.AllLocations.Count + " Total-" + totalLocationsCount);
        bool updateItemsResult = false;
        bool updateSpellsResult = false;
        while (prevCount != count || updateItemsResult || updateSpellsResult)
        {
            prevCount = count;
            westHyrule.UpdateVisit();
            deathMountain.UpdateVisit();
            eastHyrule.UpdateVisit();
            mazeIsland.UpdateVisit();

            foreach(World world in worlds)
            {
                if(world.raft != null && CanGet(world.raft) && itemGet[Item.RAFT])
                {
                    worlds.ForEach(i => i.VisitRaft());
                }

                if (world.bridge != null && CanGet(world.bridge))
                {
                    worlds.ForEach(i => i.VisitBridge());
                }

                if (world.cave1 != null && CanGet(world.cave1))
                {
                    worlds.ForEach(i => i.VisitCave1());
                }

                if (world.cave2 != null && CanGet(world.cave2))
                {
                    worlds.ForEach(i => i.VisitCave2());
                }
            }
            updateItemsResult = UpdateItemGets();
            updateSpellsResult = UpdateSpells();

            westHyrule.UpdateVisit();
            deathMountain.UpdateVisit();
            eastHyrule.UpdateVisit();
            mazeIsland.UpdateVisit();

            updateItemsResult = UpdateItemGets();
            updateSpellsResult = UpdateSpells();


            count = 0;
            dm = 0;
            mi = 0;
            wh = 0;
            eh = 0;

            wh = westHyrule.AllLocations.Count(i => i.Reachable);
            eh = eastHyrule.AllLocations.Count(i => i.Reachable);
            dm = deathMountain.AllLocations.Count(i => i.Reachable);
            mi = mazeIsland.AllLocations.Count(i => i.Reachable);
            count = wh + eh + dm + mi;

            //logger.Debug("Starting reachable main loop(" + loopCount++ + ". prevCount:" + prevCount + " count:" + count
            //+ " updateItemsResult:" + updateItemsResult + " updateSpellsResult:" + updateSpellsResult);
        }
       /* logger.Debug("Reached: " + count);
        logger.Debug("wh: " + wh + " / " + westHyrule.AllLocations.Count);
        logger.Debug("eh: " + eh + " / " + eastHyrule.AllLocations.Count);
        logger.Debug("dm: " + dm + " / " + deathMountain.AllLocations.Count);
        logger.Debug("mi: " + mi + " / " + mazeIsland.AllLocations.Count);
       */

        //return true;
        /*
        if(eastHyrule.AllLocations.Where(i => i.Reachable == false).ToList().Count <= 6)
        {
            if(debug++ >= 1)
            {
                int i = 2;
                logger.Debug("wh: " + wh + " / " + westHyrule.AllLocations.Count);
                logger.Debug("eh: " + eh + " / " + eastHyrule.AllLocations.Count);
                logger.Debug("dm: " + dm + " / " + deathMountain.AllLocations.Count);
                logger.Debug("mi: " + mi + " / " + mazeIsland.AllLocations.Count);
                UpdateItemGets();
                return true;
            } 
        }
        */
        //logger.Debug("-");

        foreach (Item item in SHUFFLABLE_STARTING_ITEMS)
        {
            if(itemGet[item] == false)
            {
                /*
                if (count > 150)
                {
                    Debug.WriteLine("-" + count + "- " + accessibleMagicContainers);
                    //SHUFFLABLE_STARTING_ITEMS.Where(i => itemGet[item] == false).ToList().ForEach(i => Debug.WriteLine(Enum.GetName(typeof(Item), i)));
                    westHyrule.AllLocations.Union(eastHyrule.AllLocations).Union(deathMountain.AllLocations).Union(mazeIsland.AllLocations)
                        .Where(i => !i.itemGet && i.item != Item.DO_NOT_USE).ToList().ForEach(i => Debug.WriteLine(i.Name + " / " + Enum.GetName(typeof(Item), i.item)));
                    Debug.WriteLine("");
                    itemGetReachableFailures++;
                    return true;
                }
                */
                return false;
            }
        }

        for (int i = 19; i < 22; i++)
        {
            if (itemGet[(Item)i] == false)
            {
                itemGetReachableFailures++;
                return false;
            }
        }
        if (accessibleMagicContainers != 8)
        {
            magicContainerReachableFailures++;
            return false;
        }
        if (heartContainers != maxHearts)
        {
            heartContainerReachableFailures++;
            return false;
        }
        if(SpellGet.Values.Any(i => i == false))
        {
            spellGetReachableFailures++;
            return false;
        }

        bool retval = (CanGet(westHyrule.Locations[Terrain.TOWN]) 
            && CanGet(eastHyrule.Locations[Terrain.TOWN]) 
            && CanGet(westHyrule.palace1) 
            && CanGet(westHyrule.palace2) 
            && CanGet(westHyrule.palace3) 
            && CanGet(mazeIsland.palace4) 
            && CanGet(eastHyrule.palace5) 
            && CanGet(eastHyrule.palace6) 
            && CanGet(eastHyrule.gp) 
            && CanGet(itemLocs) 
            && CanGet(westHyrule.bagu) 
            && (!Props.HiddenKasuto || (CanGet(eastHyrule.hiddenKasutoLocation))) 
            && (!Props.HiddenPalace || (CanGet(eastHyrule.hiddenPalaceLocation))));
        if(retval == false)
        {
            logicallyRequiredLocationsReachableFailures++;
        }
        return retval;
    }

    private bool CanGet(List<Location> l)
    {
        return l.All(i => i.Reachable);
    }
    private bool CanGet(Location location)
    {
        return location.Reachable;
    }

    /// <summary>
    /// Removes the intermediate room between entering the spell house and reaching the old man's basement
    /// </summary>
    private void ShortenWizards()
    {
        /*
        Spell swap notes:
        Shield exit: 0xC7BB, 0xC1; enter: 0xC7EC, 0x90 //change map 48 pointer to map 40 pointer
        Jump exit: 0xC7BF, 0xC5; enter: 0xC7F0, 0x94 //change map 49 pointer to map 41 pointer
        Life exit: 0xC7C3, 0xC9; enter 0xC7F4, 0x98 //change map 50 pointer to map 42 pointer
        Fairy exit: 0xC7C7, 0xCD; enter 0xC7F8, 0x9C //change map 51 pointer to map 43 pointer
        Fire exit: 0xC7Cb, 0xD1; enter 0xC7FC, 0xA0 //change map 52 pointer to map 44 pointer
        Reflect exit: 0xC7Cf, 0xD5; enter 0xC800, 0xA4 //change map 53 pointer to map 45 pointer
        Spell exit: 0xC7D3, 0x6A; enter 0xC795, 0xC796, 0x4D //new kasuto item?
        Thunder exit: 0xC7D7, 0xDD; enter 0xC808, 0xAC
        Downstab exit: 0xC7DB, 0xE1; enter 0xC80C, 0xB0
        Upstab exit: 0xC7DF, 0xE5; enter 0xC810, 0xB4
        */
        for (int i = 0; i < 16; i = i + 2)
        {
            ROMData.Put(0xC611 + i, (Byte)0x75);
            ROMData.Put(0xC611 + i + 1, (Byte)0x70);
            ROMData.Put(0xC593 + i, (Byte)0x48);
            ROMData.Put(0xC593 + i + 1, (Byte)0x9B);
        }
        ROMData.Put(0xC7BB, (Byte)0x07);
        ROMData.Put(0xC7BF, (Byte)0x13);
        ROMData.Put(0xC7C3, (Byte)0x21);
        ROMData.Put(0xC7C7, (Byte)0x27);
        ROMData.Put(0xC7CB, (Byte)0x37);
        ROMData.Put(0xC7CF, (Byte)0x3F);
        ROMData.Put(0xC850, (Byte)0xB0);
        //ROMData.put(0xC7D3, (Byte)0x4D);
        ROMData.Put(0xC7D7, (Byte)0x5E);
        ROMData.Put(0xC7DF, (Byte)0x43);
        ROMData.Put(0xC870, (Byte)0xB8);
        ROMData.Put(0xC7E3, (Byte)0x49);
        ROMData.Put(0xC874, (Byte)0xA8);
        ROMData.Put(0xC7D3, (Byte)0x4D);
        ROMData.Put(0xC7DB, (Byte)0x29);
        //ROMData.put(0xC7E3, (Byte)0x49);
        // ROMData.put(0xC874, (Byte)0xA8);
        //ROMData.put(0x8560, (Byte)0xBC);
    }

    private bool UpdateSpells()
    {
        //Location[] townLocations = new Location[11];
        Dictionary<Town, Location> townLocations = new Dictionary<Town, Location>();
        townLocations[westHyrule.shieldTown.TownNum] = westHyrule.shieldTown;
        townLocations[westHyrule.jump.TownNum] = westHyrule.jump;
        townLocations[westHyrule.lifeNorth.TownNum] = westHyrule.lifeNorth;
        townLocations[westHyrule.lifeSouth.TownNum] = westHyrule.lifeSouth;
        townLocations[westHyrule.fairy.TownNum] = westHyrule.fairy;
        townLocations[eastHyrule.nabooru.TownNum] = eastHyrule.nabooru;
        townLocations[eastHyrule.darunia.TownNum] = eastHyrule.darunia;
        townLocations[eastHyrule.newKasuto.TownNum] = eastHyrule.newKasuto;
        townLocations[eastHyrule.newKasuto2.TownNum] = eastHyrule.newKasuto2;
        townLocations[eastHyrule.oldKasuto.TownNum] = eastHyrule.oldKasuto;

        bool changed = false;
        foreach (Spell s in spellMap.Keys)
        {
            if (s == Spell.FAIRY && (((itemGet[Item.MEDICINE] || props.RemoveSpellItems) && westHyrule.fairy.TownNum == Town.MIDO) || (westHyrule.fairy.TownNum == Town.OLD_KASUTO && (accessibleMagicContainers >= 8 || props.DisableMagicRecs))) && CanGet(westHyrule.fairy))
            {
                if(!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.JUMP && (((itemGet[Item.TROPHY] || props.RemoveSpellItems) && westHyrule.jump.TownNum == Town.RUTO) || (westHyrule.jump.TownNum == Town.DARUNIA && (accessibleMagicContainers >= 6 || props.DisableMagicRecs) && (itemGet[Item.CHILD] || props.RemoveSpellItems))) && CanGet(westHyrule.jump))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.DOWNSTAB && (SpellGet[Spell.JUMP] || SpellGet[Spell.FAIRY]) && CanGet(townLocations[Town.MIDO]))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.UPSTAB && (SpellGet[Spell.JUMP]) && CanGet(townLocations[Town.DARUNIA]))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.LIFE && (CanGet(westHyrule.lifeNorth)) && (((accessibleMagicContainers >= 7 || props.DisableMagicRecs) && westHyrule.lifeNorth.TownNum == Town.NEW_KASUTO) || westHyrule.lifeNorth.TownNum == Town.SARIA_NORTH))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.SHIELD && (CanGet(westHyrule.shieldTown)) && (((accessibleMagicContainers >= 5 || props.DisableMagicRecs) && westHyrule.shieldTown.TownNum == Town.NABOORU) || westHyrule.shieldTown.TownNum == Town.RAURU))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.REFLECT && ((eastHyrule.darunia.TownNum == Town.RUTO && (itemGet[Item.TROPHY] || props.RemoveSpellItems)) || ((itemGet[Item.CHILD] || props.RemoveSpellItems) && eastHyrule.darunia.TownNum == Town.DARUNIA && (accessibleMagicContainers >= 6 || props.DisableMagicRecs))) && CanGet(eastHyrule.darunia))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.FIRE && (CanGet(eastHyrule.nabooru)) && (((accessibleMagicContainers >= 5 || props.DisableMagicRecs) && eastHyrule.nabooru.TownNum == Town.NABOORU) || eastHyrule.nabooru.TownNum == Town.RAURU))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.SPELL && (CanGet(eastHyrule.newKasuto)) && (((accessibleMagicContainers >= 7 || props.DisableMagicRecs) && eastHyrule.newKasuto.TownNum == Town.NEW_KASUTO) || eastHyrule.newKasuto.TownNum == Town.SARIA_NORTH))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
            else if (s == Spell.THUNDER && (CanGet(eastHyrule.oldKasuto)) && (((accessibleMagicContainers >= 8 || props.DisableMagicRecs) && eastHyrule.oldKasuto.TownNum == Town.OLD_KASUTO) || (eastHyrule.oldKasuto.TownNum == Town.MIDO && (itemGet[Item.MEDICINE] || props.RemoveSpellItems))))
            {
                if (!SpellGet[spellMap[s]])
                {
                    changed = true;
                }
                SpellGet[spellMap[s]] = true;
            }
        }
        return changed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether any items were marked accessable</returns>
    private bool UpdateItemGets()
    {
        accessibleMagicContainers = 4 + westHyrule.AllLocations.Union(eastHyrule.AllLocations).Union(deathMountain.AllLocations).Union(mazeIsland.AllLocations)
            .Where(i => i.itemGet == true && i.item == Item.MAGIC_CONTAINER).Count();
        heartContainers = startHearts;
        bool changed = false;
        foreach (Location location in itemLocs)
        {
            bool hadItemPreviously = location.itemGet;
            bool hasItemNow;
            if (location.PalNum > 0 && location.PalNum < 7)
            {
                Palace palace = palaces[location.PalNum - 1];
                hasItemNow = CanGet(location) 
                    && (SpellGet[Spell.FAIRY] || itemGet[Item.MAGIC_KEY]) 
                    && (!palace.NeedDstab || (palace.NeedDstab && SpellGet[Spell.DOWNSTAB])) 
                    && (!palace.NeedFairy || (palace.NeedFairy && SpellGet[Spell.FAIRY])) 
                    && (!palace.NeedGlove || (palace.NeedGlove && itemGet[Item.GLOVE])) 
                    && (!palace.NeedJumpOrFairy || (palace.NeedJumpOrFairy && (SpellGet[Spell.JUMP]) || SpellGet[Spell.FAIRY])) 
                    && (!palace.NeedReflect || (palace.NeedReflect && SpellGet[Spell.REFLECT]));
            }
            else if (location.TownNum == Town.NEW_KASUTO)
            {
                hasItemNow = CanGet(location) && (accessibleMagicContainers >= kasutoJars) && (!location.NeedHammer || itemGet[Item.HAMMER]);
            }
            else if (location.TownNum == Town.NEW_KASUTO_2)
            {
                hasItemNow = (CanGet(location) && SpellGet[Spell.SPELL]) && (!location.NeedHammer || itemGet[Item.HAMMER]);
            }
            else
            {
                hasItemNow = CanGet(location) && (!location.NeedHammer || itemGet[Item.HAMMER]) && (!location.NeedRecorder || itemGet[Item.FLUTE]);
            }

            //Issue #3: Previously running UpdateItemGets multiple times could produce different results based on the sequence of times it ran
            //For items that were blocked by MC requirements, different orders of parsing the same world could check the MCs at different times
            //producing different results. Now it's not possible to "go back" in logic and call previously accessed items inaccesable.
            location.itemGet = hasItemNow || hadItemPreviously;
            itemGet[location.item] = hasItemNow || hadItemPreviously;

            /*
            if (location.itemGet && location.item == Item.MAGIC_CONTAINER)
            {
                accessibleMagicContainers++;
            }
            */
            if (location.itemGet && location.item == Item.HEART_CONTAINER)
            {
                heartContainers++;
            }
            if(!hadItemPreviously && location.itemGet)
            {
                changed = true;
            }
        }

        accessibleMagicContainers = 4 + westHyrule.AllLocations.Union(eastHyrule.AllLocations).Union(deathMountain.AllLocations).Union(mazeIsland.AllLocations)
            .Where(i => i.itemGet == true && i.item == Item.MAGIC_CONTAINER).Count();
        return changed;
    }
    private void ShuffleLifeEffectiveness(bool isMag)
    {

        int numBanks = 7;
        int start = 0x1E2BF;
        if (isMag)
        {
            numBanks = 8;
            start = 0xD8B;
        }
        int[,] life = new int[numBanks, 8];
        for (int i = 0; i < numBanks; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int lifeVal = ROMData.GetByte(start + (i * 8) + j);
                int highPart = (lifeVal & 0xF0) >> 4;
                int lowPart = lifeVal & 0x0F;
                life[i, j] = highPart * 8 + lowPart / 2;
            }
        }

        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < numBanks; i++)
            {
                int nextVal = life[i, j];
                if ((props.LifeEffectiveness == StatEffectiveness.AVERAGE && !isMag) || (props.MagicEffectiveness == StatEffectiveness.AVERAGE && isMag))
                {
                    int max = (int)(life[i, j] + life[i, j] * .5);
                    int min = (int)(life[i, j] - life[i, j] * .5);
                    if(!isMag)
                    {
                        min = (int)(life[i, j] - life[i, j] * .25);
                    }
                    if (j == 0)
                    {
                        nextVal = RNG.Next(min, Math.Min(max, 120));
                    }
                    else
                    {
                        nextVal = RNG.Next(min, Math.Min(max, 120));
                        if (nextVal > life[i, j - 1])
                        {
                            nextVal = life[i, j - 1];
                        }
                    }
                }
                else if (props.MagicEffectiveness == StatEffectiveness.LOW && isMag)
                {
                    nextVal = (int)(life[i, j] + (life[i, j] * .5));
                }
                else if (props.LifeEffectiveness == StatEffectiveness.HIGH && !isMag || props.MagicEffectiveness == StatEffectiveness.HIGH && isMag)
                {
                    nextVal = (int)(life[i, j] * .5);
                }

                if (isMag && nextVal > 120)
                {
                    nextVal = 120;
                }
                life[i, j] = nextVal;
            }
        }

        for (int i = 0; i < numBanks; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int highPart = (life[i, j] / 8) << 4;
                int lowPart = (life[i, j] % 8);
                ROMData.Put(start + (i * 8) + j, (Byte)(highPart + (lowPart * 2)));
            }
        }

    }

    private void RandomizeEnemies()
    {
        if (props.ShuffleEnemyHP)
        {
            ShuffleHP(0x5434, 0x5453);
            ShuffleHP(0x9434, 0x944E);
            ShuffleHP(0x11435, 0x11435);
            ShuffleHP(0x11437, 0x11454);
            ShuffleHP(0x13C86, 0x13C87);
            ShuffleHP(0x15434, 0x15438);
            ShuffleHP(0x15440, 0x15443);
            ShuffleHP(0x15445, 0x1544B);
            ShuffleHP(0x1544E, 0x1544E);
            ShuffleHP(0x12935, 0x12935);
            ShuffleHP(0x12937, 0x12954);
        }

        if (props.AttackEffectiveness == StatEffectiveness.MAX)
        {
            ShuffleAttackEffectiveness(true);
            ROMData.Put(0x005432, (Byte)193);
            ROMData.Put(0x009432, (Byte)193);
            ROMData.Put(0x11436, (Byte)193);
            ROMData.Put(0x12936, (Byte)193);
            ROMData.Put(0x15532, (Byte)193);
            ROMData.Put(0x11437, (Byte)192);
            ROMData.Put(0x1143F, (Byte)192);
            ROMData.Put(0x12937, (Byte)192);
            ROMData.Put(0x1293F, (Byte)192);
            ROMData.Put(0x15445, (Byte)192);
            ROMData.Put(0x15446, (Byte)192);
            ROMData.Put(0x15448, (Byte)192);
            ROMData.Put(0x15453, (Byte)193);
            ROMData.Put(0x12951, (Byte)227);

        }
    }

    private void ShuffleHP(int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            int newVal = 0;
            int val = (int)ROMData.GetByte(i);

            newVal = RNG.Next((int)(val * 0.5), (int)(val * 1.5));
            if (newVal > 255)
            {
                newVal = 255;
            }

            ROMData.Put(i, (Byte)newVal);
        }
    }

    private void ProcessOverworld()
    {
        if (props.ShuffleSmallItems)
        {
            ShuffleSmallItems(1, true);
            ShuffleSmallItems(1, false);
            ShuffleSmallItems(2, true);
            ShuffleSmallItems(2, false);
            ShuffleSmallItems(3, true);
            //shuffleSmallItems(4, true);
            //shuffleSmallItems(4, false);
        }
        do //while (!EverythingReachable()) ;
        {
            totalReachabilityOverworldAttempts++;
            worlds = new List<World>();
            westHyrule = new WestHyrule(this);
            deathMountain = new DeathMountain(this);
            eastHyrule = new EastHyrule(this);
            mazeIsland = new MazeIsland(this);
            worlds.Add(westHyrule);
            worlds.Add(deathMountain);
            worlds.Add(eastHyrule);
            worlds.Add(mazeIsland);
            ShuffleTowns();

            //Generate a pathable set of continents
            do // } while (!AllContinentsHaveConnection(worlds));
            {
                totalContinentConnectionOverworldAttempts++;
                worlds = new List<World>();
                westHyrule = new WestHyrule(this);
                deathMountain = new DeathMountain(this);
                eastHyrule = new EastHyrule(this);
                mazeIsland = new MazeIsland(this);
                worlds.Add(westHyrule);
                worlds.Add(deathMountain);
                worlds.Add(eastHyrule);
                worlds.Add(mazeIsland);
                ShuffleTowns();

                if (props.ContinentConnections == ContinentConnectionType.NORMAL || props.ContinentConnections == ContinentConnectionType.RB_BORDER_SHUFFLE)
                {
                    westHyrule.LoadCave1(1);
                    westHyrule.LoadCave2(1);
                    westHyrule.LoadRaft(2);

                    deathMountain.LoadCave1(0);
                    deathMountain.LoadCave2(0);

                    eastHyrule.LoadRaft(0);
                    eastHyrule.LoadBridge(3);

                    mazeIsland.LoadBridge(2);
                }
                else if (props.ContinentConnections == ContinentConnectionType.TRANSPORTATION_SHUFFLE)
                {
                    List<int> chosen = new List<int>();
                    int type = RNG.Next(4);
                    if (props.WestBiome == Biome.VANILLA
                        || props.WestBiome == Biome.VANILLA_SHUFFLE 
                        || props.DmBiome == Biome.VANILLA
                        || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        type = 3;
                    }

                    SetTransportation(0, 1, type);
                    chosen.Add(type);
                    if (props.WestBiome == Biome.VANILLA
                        || props.WestBiome == Biome.VANILLA_SHUFFLE
                        || props.DmBiome == Biome.VANILLA
                        || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        type = 0;
                    }
                    else
                    {
                        do
                        {
                            type = RNG.Next(4);
                        } while (chosen.Contains(type));
                    }
                    SetTransportation(0, 1, type);
                    chosen.Add(type);
                    if (props.WestBiome == Biome.VANILLA
                        || props.WestBiome == Biome.VANILLA_SHUFFLE
                        || props.EastBiome == Biome.VANILLA
                        || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        type = 1;
                    }
                    else
                    {
                        do
                        {
                            type = RNG.Next(4);
                        } while (chosen.Contains(type));
                    }
                    SetTransportation(0, 2, type);
                    chosen.Add(type);
                    if (props.EastBiome == Biome.VANILLA
                        || props.EastBiome == Biome.VANILLA_SHUFFLE
                        || props.MazeBiome == Biome.VANILLA
                        || props.MazeBiome == Biome.VANILLA_SHUFFLE)
                    {
                        type = 2;
                    }
                    else
                    {
                        do
                        {
                            type = RNG.Next(4);
                        } while (chosen.Contains(type));
                    }
                    SetTransportation(2, 3, type);
                }
                else
                {
                    List<int> doNotPick = new List<int>();
                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        doNotPick.Add(0);
                    }
                    if (props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        doNotPick.Add(2);
                    }
                    if (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        doNotPick.Add(1);
                    }
                    if (props.MazeBiome == Biome.VANILLA || props.MazeBiome == Biome.VANILLA_SHUFFLE)
                    {
                        doNotPick.Add(3);
                    }

                    int raftw1 = RNG.Next(worlds.Count);

                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        raftw1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(raftw1))
                        {
                            raftw1 = RNG.Next(worlds.Count);
                        }
                    }


                    int raftw2 = RNG.Next(worlds.Count);
                    if (props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        raftw2 = 2;
                    }
                    else
                    {
                        do
                        {
                            raftw2 = RNG.Next(worlds.Count);
                        } while (raftw1 == raftw2 || doNotPick.Contains(raftw2));
                    }

                    worlds[raftw1].LoadRaft(raftw2);
                    worlds[raftw2].LoadRaft(raftw1);

                    int bridgew1 = RNG.Next(worlds.Count);
                    if (props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        bridgew1 = 2;
                    }
                    else
                    {
                        while (doNotPick.Contains(bridgew1))
                        {
                            bridgew1 = RNG.Next(worlds.Count);
                        }
                    }
                    int bridgew2 = RNG.Next(worlds.Count);
                    if (props.MazeBiome == Biome.VANILLA || props.MazeBiome == Biome.VANILLA_SHUFFLE)
                    {
                        bridgew2 = 3;
                    }
                    else
                    {
                        do
                        {
                            bridgew2 = RNG.Next(worlds.Count);
                        } while (bridgew1 == bridgew2 || doNotPick.Contains(bridgew2));
                    }

                    worlds[bridgew1].LoadBridge(bridgew2);
                    worlds[bridgew2].LoadBridge(bridgew1);

                    int c1w1 = RNG.Next(worlds.Count);
                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c1w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c1w1))
                        {
                            c1w1 = RNG.Next(worlds.Count);
                        }
                    }
                    int c1w2 = RNG.Next(worlds.Count);
                    if (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c1w2 = 1;
                    }
                    else
                    {
                        do
                        {
                            c1w2 = RNG.Next(worlds.Count);
                        } while (c1w1 == c1w2 || doNotPick.Contains(c1w2));
                    }

                    worlds[c1w1].LoadCave1(c1w2);
                    worlds[c1w2].LoadCave1(c1w1);

                    int c2w1 = RNG.Next(worlds.Count);
                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c2w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c2w1))
                        {
                            c2w1 = RNG.Next(worlds.Count);
                        }
                    }
                    int c2w2 = RNG.Next(worlds.Count);
                    if (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c2w2 = 1;
                    }
                    else
                    {
                        do
                        {
                            c2w2 = RNG.Next(worlds.Count);
                        } while (c2w1 == c2w2 || doNotPick.Contains(c2w2));
                    }

                    worlds[c2w1].LoadCave2(c2w2);
                    worlds[c2w2].LoadCave2(c2w1);
                }
            } while (!AllContinentsHaveConnection(worlds));

            int nonContinentGenerationAttempts = 0;
            int nonTerrainShuffleAttempt = 0;
            DateTime timestamp;
            //Shuffle everything else
            do //while (wtries < 10 && !EverythingReachable());
            {
                //GENERATE WEST
                bool shouldContinue = UpdateProgress(2);
                if (!shouldContinue)
                {
                    return;
                }
                nonContinentGenerationAttempts++;
                timestamp = DateTime.Now;
                if (!westHyrule.AllReached)
                {
                    while (!westHyrule.Terraform()) { totalWestGenerationAttempts++; }
                    totalWestGenerationAttempts++;
                }
                westHyrule.ResetVisitabilityState();
                timeSpentBuildingWest += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE DM
                shouldContinue = UpdateProgress(3);
                if (!shouldContinue)
                {
                    return;
                }
                timestamp = DateTime.Now;
                if (!deathMountain.AllReached)
                {
                    while (!deathMountain.Terraform()) { totalDeathMountainGenerationAttempts++; }
                    totalDeathMountainGenerationAttempts++;
                }
                deathMountain.ResetVisitabilityState();
                timeSpentBuildingDM += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE EAST
                shouldContinue = UpdateProgress(4);
                if (!shouldContinue)
                {
                    return;
                }
                timestamp = DateTime.Now;
                if (!eastHyrule.AllReached)
                {
                    while (!eastHyrule.Terraform()) { totalEastGenerationAttempts++; }
                    totalEastGenerationAttempts++;
                }
                eastHyrule.ResetVisitabilityState();
                timeSpentBuildingEast += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE MAZE ISLAND
                shouldContinue = UpdateProgress(5);
                if (!shouldContinue)
                {
                    return;
                }
                timestamp = DateTime.Now;
                if (!mazeIsland.AllReached)
                {
                    while (!mazeIsland.Terraform()) { totalMazeIslandGenerationAttempts++; }
                    totalMazeIslandGenerationAttempts++;
                }
                mazeIsland.ResetVisitabilityState();
                timeSpentBuildingMI += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                shouldContinue = UpdateProgress(6);
                if (!shouldContinue)
                {
                    return;
                }


                //Then perform non-terrain shuffles looking for one that works.
                nonTerrainShuffleAttempt = 0;
                do
                {
                    foreach (Location location in westHyrule.AllLocations)
                    {
                        location.Reachable = false;
                    }

                    foreach (Location location in eastHyrule.AllLocations)
                    {
                        location.Reachable = false;
                    }

                    foreach (Location location in mazeIsland.AllLocations)
                    {
                        location.Reachable = false;
                    }
                    foreach (Location location in deathMountain.AllLocations)
                    {
                        location.Reachable = false;
                    }

                    eastHyrule.newKasuto2.Reachable = false;
                    //eastHyrule.bridge.Reachable = false;
                    startMed = false;
                    startTrophy = false;
                    startKid = false;
                    westHyrule.ResetVisitabilityState();
                    eastHyrule.ResetVisitabilityState();
                    mazeIsland.ResetVisitabilityState();
                    

                    ShuffleSpells();
                    LoadItemLocs();
                    deathMountain.ResetVisitabilityState();


                    westHyrule.SetStart();
                    ShuffleItems();

                    ShufflePalaces();
                    LoadItemLocs();


                    westHyrule.UpdateAllReached();
                    eastHyrule.UpdateAllReached();
                    mazeIsland.UpdateAllReached();
                    deathMountain.UpdateAllReached();

                    nonTerrainShuffleAttempt++;
                } while (nonTerrainShuffleAttempt < NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT && !IsEverythingReachable());

                if (nonTerrainShuffleAttempt != NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT)
                {
                    break;
                }

                /*
                int west = westHyrule.AllLocations.Count(i => i.Reachable);
                int east = eastHyrule.AllLocations.Count(i => i.Reachable);
                int maze = mazeIsland.AllLocations.Count(i => i.Reachable);
                int dm = deathMountain.AllLocations.Count(i => i.Reachable);

                logger.Trace("wr: " + west + " / " + westHyrule.AllLocations.Count);
                logger.Trace("er: " + east + " / " + eastHyrule.AllLocations.Count);
                logger.Trace("dm: " + dm + " / " + deathMountain.AllLocations.Count);
                logger.Trace("maze: " + maze + " / " + mazeIsland.AllLocations.Count);
                */
            } while (nonContinentGenerationAttempts < NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT);
        } while (!IsEverythingReachable());

        if (props.ShuffleOverworldEnemies)
        {
            foreach (World w in worlds)
            {
                w.ShuffleEnemies();
            }
        }

        //WRITE LOCATIONS HERE
    }

    private bool UpdateProgress(int v)
    {
        if (worker != null)
        {
            if (worker.CancellationPending)
            {
                return false;
            }
            worker.ReportProgress(v);
        }
        return true;
    }

    private void SetTransportation(int w1, int w2, int type)
    {
        if(type == 1)
        {
            worlds[w1].LoadRaft(w2);
            worlds[w2].LoadRaft(w1);
        }
        else if (type == 2)
        {
            worlds[w1].LoadBridge(w2);
            worlds[w2].LoadBridge(w1);
        }
        else if(type == 3)
        {
            worlds[w1].LoadCave1(w2);
            worlds[w2].LoadCave1(w1);

        }
        else
        {
            worlds[w1].LoadCave2(w2);
            worlds[w2].LoadCave2(w1);
        }
    }

    private bool AllContinentsHaveConnection(List<World> worlds)
    {
        foreach (World w in worlds)
        {
            if (!w.HasConnections())
            {
                return false;
            }
        }
        return true;
    }

    private void ShuffleTowns()
    {
        westHyrule.shieldTown.TownNum = Town.RAURU;
        westHyrule.jump.TownNum = Town.RUTO;
        westHyrule.lifeNorth.TownNum = Town.SARIA_NORTH;
        westHyrule.lifeSouth.TownNum = Town.SARIA_SOUTH;
        westHyrule.fairy.TownNum = Town.MIDO;
        eastHyrule.nabooru.TownNum = Town.NABOORU;
        eastHyrule.darunia.TownNum = Town.DARUNIA;
        eastHyrule.newKasuto.TownNum = Town.NEW_KASUTO;
        eastHyrule.newKasuto2.TownNum = Town.NEW_KASUTO_2;
        eastHyrule.oldKasuto.TownNum = Town.OLD_KASUTO;

        if(props.TownSwap)
        {
            if(RNG.NextDouble() > .5)
            {
                Util.Swap(westHyrule.shieldTown, eastHyrule.nabooru);
            }
            if (RNG.NextDouble() > .5)
            {
                Util.Swap(westHyrule.jump, eastHyrule.darunia);
            }
            if (RNG.NextDouble() > .5)
            {
                Util.Swap(westHyrule.lifeNorth, eastHyrule.newKasuto);
                Util.Swap(westHyrule.lifeSouth, eastHyrule.newKasuto2);

                eastHyrule.newKasuto.NeedBagu = true;
                eastHyrule.newKasuto2.NeedBagu = true;

                westHyrule.lifeNorth.NeedBagu = false;
                westHyrule.lifeSouth.NeedBagu = false;

                westHyrule.connections.Remove(westHyrule.lifeNorth);
                westHyrule.connections.Remove(westHyrule.lifeSouth);

                eastHyrule.connections.Add(eastHyrule.newKasuto, eastHyrule.newKasuto2);
                eastHyrule.connections.Add(eastHyrule.newKasuto2, eastHyrule.newKasuto);

                westHyrule.AllLocations.Remove(westHyrule.lifeSouth);

                eastHyrule.AllLocations.Add(eastHyrule.newKasuto2);



            }
            if (RNG.NextDouble() > .5)
            {
                Util.Swap(westHyrule.fairy, eastHyrule.oldKasuto);
            }
        }
    }

    private void ShufflePalaces()
    {

        if (props.SwapPalaceCont)
        {

            List<Location> pals = new List<Location> { westHyrule.palace1, westHyrule.palace2, westHyrule.palace3, mazeIsland.palace4, eastHyrule.palace5, eastHyrule.palace6 };

            if (props.P7shuffle)
            {
                pals.Add(eastHyrule.gp);
            }

            for (int i = 0; i < pals.Count; i++)
            {
                int swapp = RNG.Next(i, pals.Count);
                Util.Swap(pals[i], pals[swapp]);
            }

            westHyrule.palace1.World = westHyrule.palace1.World & 0xFC;
            westHyrule.palace2.World = westHyrule.palace2.World & 0xFC;
            westHyrule.palace3.World = westHyrule.palace3.World & 0xFC;

            mazeIsland.palace4.World = mazeIsland.palace4.World & 0xFC;
            mazeIsland.palace4.World = mazeIsland.palace4.World | 0x03;

            eastHyrule.palace5.World = eastHyrule.palace5.World & 0xFC;
            eastHyrule.palace5.World = eastHyrule.palace5.World | 0x02;

            eastHyrule.palace6.World = eastHyrule.palace6.World & 0xFC;
            eastHyrule.palace6.World = eastHyrule.palace6.World | 0x02;

            if (props.P7shuffle)
            {
                eastHyrule.gp.World = eastHyrule.gp.World & 0xFC;
                eastHyrule.gp.World = eastHyrule.gp.World | 0x02;
            }

            /*
            subroutine start bf60(13f70)

            instruction: 20 60 bf

            subroutine:
                load 22 into accumulator    A9 22
                xor with $561               4D 61 05
                return                      60


            Gooma / helmet head fix (CHECK THESE):
                13c96 = d0--hitbox / exp / hp
                13d88 = d0--sprite info
                13ad6 = d0--behavior
                11b2d = d0(don't need?)
                */

            //write subroutine
            ROMData.Put(0x13f70, 0xA9);
            byte helmetRoom = 0x22;
            if (props.palaceStyle == PalaceStyle.RECONSTRUCTED)
            {
                helmetRoom = (byte)palaces[1].BossRoom.NewMap;
            }
            ROMData.Put(0x13f71, helmetRoom);
            ROMData.Put(0x13f72, 0x4D);
            ROMData.Put(0x13f73, 0x61);
            ROMData.Put(0x13f74, 0x05);
            ROMData.Put(0x13f75, 0x60);

            //jump to subroutine
            ROMData.Put(0x13c93, 0x20);
            ROMData.Put(0x13c94, 0x60);
            ROMData.Put(0x13c95, 0xBF);

            ROMData.Put(0x13d85, 0x20);
            ROMData.Put(0x13d86, 0x60);
            ROMData.Put(0x13d87, 0xBF);

            ROMData.Put(0x13ad3, 0x20);
            ROMData.Put(0x13ad4, 0x60);
            ROMData.Put(0x13ad5, 0xBF);

            //fix for key glitch
            ROMData.Put(0x11b37, 0xea);
            ROMData.Put(0x11b38, 0xea);
            ROMData.Put(0x11b39, 0xea);
        }

    }

    private List<Location> LoadItemLocs()
    {
        itemLocs = new List<Location>();
        if (westHyrule.palace1.PalNum != 7)
        {
            itemLocs.Add(westHyrule.palace1);
        }
        if (westHyrule.palace2.PalNum != 7)
        {
            itemLocs.Add(westHyrule.palace2);
        }
        if (westHyrule.palace3.PalNum != 7)
        {
            itemLocs.Add(westHyrule.palace3);
        }
        if (mazeIsland.palace4.PalNum != 7)
        {
            itemLocs.Add(mazeIsland.palace4);
        }
        if (eastHyrule.palace5.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.palace5);
        }
        if (eastHyrule.palace6.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.palace6);
        }
        if (eastHyrule.gp.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.gp);
        }
        itemLocs.Add(westHyrule.heart1);
        itemLocs.Add(westHyrule.heart2);
        itemLocs.Add(westHyrule.jar);
        itemLocs.Add(westHyrule.medCave);
        itemLocs.Add(westHyrule.trophyCave);
        itemLocs.Add(eastHyrule.waterTile);
        itemLocs.Add(eastHyrule.desertTile);
        if (eastHyrule.newKasuto.TownNum == Town.NEW_KASUTO)
        {
            itemLocs.Add(eastHyrule.newKasuto);
            itemLocs.Add(eastHyrule.newKasuto2);
        } 
        else
        {
            itemLocs.Add(westHyrule.lifeNorth);
            itemLocs.Add(westHyrule.lifeSouth);
        }
        itemLocs.Add(deathMountain.magicCave);
        itemLocs.Add(deathMountain.hammerCave);
        itemLocs.Add(mazeIsland.kid);
        itemLocs.Add(mazeIsland.magic);


        if (props.PbagItemShuffle)
        {
            itemLocs.Add(westHyrule.pbagCave);
            itemLocs.Add(eastHyrule.pbagCave1);
            itemLocs.Add(eastHyrule.pbagCave2);
        }

        return itemLocs;
    }

    private void ShuffleSpells()
    {
        spellMap = new Dictionary<Spell, Spell>();
        List<int> shuffleThis = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
        SpellGet.Clear();
        foreach (Spell spell in Enum.GetValues(typeof(Spell)))
        {
            SpellGet.Add(spell, false);
        }
        /*for (int i = 0; i < spellGet.Count(); i++)
        {
            spellGet[i] = false;
        }*/
        if (props.ShuffleSpellLocations)
        {
            for (int i = 0; i < shuffleThis.Count; i++)
            {

                int s = RNG.Next(i, shuffleThis.Count);
                int sl = shuffleThis[s];
                shuffleThis[s] = shuffleThis[i];
                shuffleThis[i] = sl;
            }
        }
        for (int i = 0; i < shuffleThis.Count; i++)
        {
            spellMap.Add((Spell)i, (Spell)shuffleThis[i]);
        }
        spellMap.Add(Spell.UPSTAB, Spell.UPSTAB);
        spellMap.Add(Spell.DOWNSTAB, Spell.DOWNSTAB);


        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.SHIELD), props.StartShield ? (Byte)1 : (Byte)0);
        SpellGet[Spell.SHIELD] = props.StartShield;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.JUMP), props.StartJump ? (Byte)1 : (Byte)0);
        SpellGet[Spell.JUMP] = props.StartJump;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.LIFE), props.StartLife ? (Byte)1 : (Byte)0);
        SpellGet[Spell.LIFE] = props.StartLife;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.FAIRY), props.StartFairy ? (Byte)1 : (Byte)0);
        SpellGet[Spell.FAIRY] = props.StartFairy;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.FIRE), props.StartFire ? (Byte)1 : (Byte)0);
        SpellGet[Spell.FIRE] = props.StartFire;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.REFLECT), props.StartReflect ? (Byte)1 : (Byte)0);
        SpellGet[Spell.REFLECT] = props.StartReflect;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.SPELL), props.StartSpell ? (Byte)1 : (Byte)0);
        SpellGet[Spell.SPELL] = props.StartSpell;
        ROMData.Put(0x17AF7 + spellMap.Values.ToList().IndexOf(Spell.THUNDER), props.StartThunder ? (Byte)1 : (Byte)0);
        SpellGet[Spell.THUNDER] = props.StartThunder;

        if (props.CombineFire)
        {
            int newFire = RNG.Next(7);
            if (newFire > 3)
            {
                newFire++;
            }
            Byte newnewFire = (Byte)(0x10 | ROMData.GetByte(0xDCB + newFire));
            ROMData.Put(0xDCF, newnewFire);
        }




    }

    private void ShuffleExp(int start, int cap)
    {
        int[] exp = new int[8];

        for (int i = 0; i < exp.Length; i++)
        {
            exp[i] = ROMData.GetByte(start + i) * 256;
            exp[i] = exp[i] + ROMData.GetByte(start + 24 + i);
        }

        for (int i = 0; i < exp.Length; i++)
        {
            int nextMin = (int)(exp[i] - exp[i] * 0.25);
            int nextMax = (int)(exp[i] + exp[i] * 0.25);
            if (i == 0)
            {
                exp[i] = RNG.Next(Math.Max(10, nextMin), nextMax);
            }
            else
            {
                exp[i] = RNG.Next(Math.Max(exp[i - 1], nextMin), Math.Min(nextMax, 9990));
            }
        }

        for (int i = 0; i < exp.Length; i++)
        {
            exp[i] = exp[i] / 10 * 10; //wtf is this line of code? -digshake, 2020
        }

        int[] cappedExp = new int[8];
        if (props.ScaleLevels)
        {
            for (int i = 0; i < exp.Length; i++)
            {
                if (i >= cap)
                {
                    cappedExp[i] = exp[i]; //shouldn't matter, just wanna put something here
                }
                else if (i == cap - 1)
                {
                    cappedExp[i] = exp[7]; //exp to get a 1up
                }
                else
                {
                    int index = (int)(8 * ((i + 1.0) / (cap - 1)));
                    cappedExp[i] = exp[(int)(6 * ((i + 1.0) / (cap - 1)))]; //cap = 3, level 4, 8, 
                }
            }
        }
        else
        {
            cappedExp = exp;
        }    

        for (int i = 0; i < exp.Length; i++)
        {
            ROMData.Put(start + i, (Byte)(cappedExp[i] / 256));
            ROMData.Put(start + 24 + i, (Byte)(cappedExp[i] % 256));
        }

        for (int i = 0; i < exp.Length; i++)
        {

            ROMData.Put(start + 2057 + i, IntToText(cappedExp[i] / 1000));
            cappedExp[i] = cappedExp[i] - ((cappedExp[i] / 1000) * 1000);
            ROMData.Put(start + 2033 + i, IntToText(cappedExp[i] / 100));
            cappedExp[i] = cappedExp[i] - ((cappedExp[i] / 100) * 100);
            ROMData.Put(start + 2009 + i, IntToText(cappedExp[i] / 10));
        }
    }

    private void ShuffleBits(List<int> addr, bool fire)
    {
        int mask = 0x10;
        int notMask = 0xEF;
        if (fire)
        {
            mask = 0x20;
            notMask = 0xDF;
        }

        double count = 0;
        foreach (int i in addr)
        {
            if ((ROMData.GetByte(i) & mask) > 0)
            {
                count++;
            }
        }

        double fraction = count / addr.Count;

        foreach (int i in addr)
        {
            int part1 = 0;
            int part2 = ROMData.GetByte(i) & notMask;
            bool havethis = RNG.NextDouble() <= fraction;
            if (havethis)
            {
                part1 = mask;
            }
            ROMData.Put(i, (Byte)(part1 + part2));
        }
    }

    private void ShuffleEnemyExp(List<int> addr)
    {
        foreach (int i in addr)
        {
            Byte exp = ROMData.GetByte(i);
            int high = exp & 0xF0;
            int low = exp & 0x0F;

            if(props.ExpLevel == StatEffectiveness.HIGH)
            {
                low++;
            } else if(props.ExpLevel == StatEffectiveness.LOW) {
                low--;
            } else if(props.ExpLevel == StatEffectiveness.NONE) {
                low = 0;
            }

            if (props.ExpLevel != StatEffectiveness.NONE)
            {
                low = RNG.Next(low - 2, low + 3);
            }
            if (low < 0)
            {
                low = 0;
            }
            else if (low > 15)
            {
                low = 15;
            }
            ROMData.Put(i, (Byte)(high + low));
        }
    }

    //Updated to use fisher-yates. Eventually i'll catch all of these. N is small enough here it REALLY makes a difference
    private void ShuffleEncounters(List<int> addr)
    {
        for (int i = addr.Count - 1; i > 0; --i)
        {
            int swap = RNG.Next(i + 1);

            Byte temp = ROMData.GetByte(addr[i]);
            ROMData.Put(addr[i], ROMData.GetByte(addr[swap]));
            ROMData.Put(addr[swap], temp);
        }
    }
    private void RandomizeStartingValues()
    {

        ROMData.Put(0x17AF3, (byte)props.StartAtk);
        ROMData.Put(0x17AF4, (byte)props.StartMag);
        ROMData.Put(0x17AF5, (byte)props.StartLifeLvl);

        if(props.RemoveFlashing)
        {
            ROMData.DisableFlashing();
        }

        if(props.SpellEnemy)
        {
            List<int> enemies = new List<int> { 3, 4, 6, 7, 14, 16, 17, 18, 24, 25, 26 };
            ROMData.Put(0x11ef, (byte)enemies[RNG.Next(enemies.Count())]);
        }
        if(props.BossItem)
        {
            shuffler.ShuffleBossDrop(ROMData, RNG);
        }

        if (props.RemoveSpellItems)
        {
            ROMData.Put(0xF584, 0xA9);
            ROMData.Put(0xF585, 0x01);
            ROMData.Put(0xF586, 0xEA);
        }
        ROMData.UpdateSprites(props.CharSprite);

        Dictionary<String, int> colorMap = new Dictionary<String, int> { { "Green", 0x2A }, { "Dark Green", 0x0A }, { "Aqua", 0x3C }, { "Dark Blue", 0x02 }, { "Purple", 0x04 }, { "Pink", 0x24 }, { "Red", 0x16 }, { "Orange", 0x27 }, { "Turd", 0x18 } };

        /*colors to include
            Green (2A)
            Dark Green (0A)
            Aqua (3C)
            Dark Blue (02)
            Purple (04)
            Pink (24)
            Red (16)
            Orange (27)
            Turd (08)
        */
        int c2 = 0;
        int c1 = 0;

        if(props.TunicColor.Equals("Default"))
        {
            if(props.CharSprite == CharacterSprite.LINK)
            {
                c2 = colorMap["Green"];
            }
            else if(props.CharSprite == CharacterSprite.IRON_KNUCKLE)
            {
                c2 = colorMap["Dark Blue"];
            }
            else if(props.CharSprite == CharacterSprite.ERROR)
            {
                c2 = 0x13;
            }
            else if(props.CharSprite == CharacterSprite.SAMUS)
            {
                c2 = 0x27;
            }
            else if(props.CharSprite == CharacterSprite.SIMON)
            {
                c2 = 0x27;
            }
            else if(props.CharSprite == CharacterSprite.STALFOS)
            {
                c2 = colorMap["Red"];
            }
            else if(props.CharSprite == CharacterSprite.VASE_LADY)
            {
                c2 = 0x13;
            }
            else if(props.CharSprite == CharacterSprite.RUTO)
            {
                c2 = 0x30;
            }
            else if(props.CharSprite == CharacterSprite.YOSHI)
            {
                c2 = 0x2a;
            }
            else if(props.CharSprite == CharacterSprite.DRAGONLORD)
            {
                c2 = 0x01;
            }
            else if(props.CharSprite == CharacterSprite.MIRIA)
            {
                c2 = 0x16;
            }
            else if(props.CharSprite == CharacterSprite.CRYSTALIS)
            {
                c2 = 0x14;
            }
            else if(props.CharSprite == CharacterSprite.TACO)
            {
                c2 = 0x2a;
            }
            else if(props.CharSprite == CharacterSprite.PYRAMID)
            {
                c2 = 0x32;
            }
            /* NOT CURRENTLY ACCESSABLE
            else if (props.charSprite.Equals("Faxanadu"))
            {
                c2 = 0x2a;
            }
            */
            else if (props.CharSprite == CharacterSprite.LADY_LINK)
            {
                c2 = 0x2a;
            }
            else if (props.CharSprite == CharacterSprite.HOODIE_LINK)
            {
                c2 = 0x2a;
            }
            else if(props.CharSprite == CharacterSprite.GLITCH_WITCH)
            {
                c2 = 0x0c;
            }
        }
        else if (!props.TunicColor.Equals("Random"))
        {
            c2 = colorMap[props.TunicColor];
        }

        if(props.ShieldColor.Equals("Default"))
        {
            if (props.CharSprite == CharacterSprite.LINK)
            {
                c1 = colorMap["Red"];
            }
            else if (props.CharSprite == CharacterSprite.IRON_KNUCKLE)
            {
                c1 = colorMap["Red"];
            }
            else if (props.CharSprite == CharacterSprite.ERROR)
            {
                c1 = colorMap["Red"];
            }
            else if(props.CharSprite == CharacterSprite.SAMUS)
            {
                c1 = 0x37;
            }
            else if(props.CharSprite == CharacterSprite.SIMON)
            {
                c1 = 0x16;
            }
            else if(props.CharSprite == CharacterSprite.STALFOS)
            {
                c1 = colorMap["Dark Blue"];
            }
            else if(props.CharSprite == CharacterSprite.VASE_LADY)
            {
                c1 = colorMap["Red"];
            }
            else if(props.CharSprite == CharacterSprite.RUTO)
            {
                c1 = 0x3c;
            }
            else if(props.CharSprite == CharacterSprite.YOSHI)
            {
                c1 = 0x0F;
            }
            else if(props.CharSprite == CharacterSprite.DRAGONLORD)
            {
                c1 = 0x03;
            }
            else if(props.CharSprite == CharacterSprite.MIRIA)
            {
                c1 = 0x15;
            }
            else if (props.CharSprite == CharacterSprite.CRYSTALIS)
            {
                c1 = 0x1B;
            }
            else if (props.CharSprite == CharacterSprite.TACO)
            {
                c1 = 0x16;
            }
            else if(props.CharSprite == CharacterSprite.PYRAMID)
            {
                c1 = 0x02;
            }
            else if (props.CharSprite == CharacterSprite.LADY_LINK)
            {
                c1 = 0x16;
            }
            else if (props.CharSprite == CharacterSprite.HOODIE_LINK)
            {
                c1 = 0x16;
            }
            else if(props.CharSprite == CharacterSprite.GLITCH_WITCH)
            {
                c1 = 0x25;
            }

        }
        else if (!props.ShieldColor.Equals("Random"))
        {
            c1 = colorMap[props.ShieldColor];
        }
        if (props.TunicColor.Equals("Random"))
        {
            Random r2 = new Random();

            int c2p1 = r2.Next(3);
            int c2p2 = r2.Next(1, 13);
            c2 = c2p1 * 16 + c2p2;

            while (c1 == c2)
            {
                c2p1 = r2.Next(3);
                c2p2 = r2.Next(1, 13);
                c2 = c2p1 * 16 + c2p2;
            }
        }

        if (props.ShieldColor.Equals("Random"))
        {
            Random r2 = new Random();



            int c1p1 = r2.Next(3);
            int c1p2 = r2.Next(1, 13);

            c1 = c1p1 * 16 + c1p2;

            while (c1 == c2)
            {
                c1p1 = r2.Next(3);
                c1p2 = r2.Next(1, 13);
                c1 = c1p1 * 16 + c1p2;
            }
        }

        if(props.EncounterRate == EncounterRate.NONE)
        {
            ROMData.Put(0x294, 0x60); //skips the whole routine
        }

        if(props.EncounterRate == EncounterRate.HALF)
        {
            //terrain timers
            ROMData.Put(0x250, 0x40);
            ROMData.Put(0x251, 0x30);
            ROMData.Put(0x252, 0x30);
            ROMData.Put(0x253, 0x40);
            ROMData.Put(0x254, 0x12);
            ROMData.Put(0x255, 0x06);

            //initial overworld timer
            ROMData.Put(0x88A, 0x10);

            /*
             * insert jump to a8aa at 2a3 (4c AA A8)
             * 
             * At a8aa
             * Load $26 (A5 26)
             * bne to end (2 bytes) (D0 0D)
             * inc new step counter (where?) EE E0 06
             * Load 1 to accumulator (A9 01)
             * xor new step counter with 1 (2D E0 06)
             * bne to end (D0 03)
             * jump to encounter spawn 8298 (4C 98 82)
             * jump to rts 829f (4C 93 82)
             */
            ROMData.Put(0x29f, new byte[] { 0x4C, 0xAA, 0xA8 });

            ROMData.Put(0x28ba, new byte[] { 0xA5, 0x26, 0xD0, 0x0D, 0xEE, 0xE0, 0x06, 0xA9, 0x01, 0x2D, 0xE0, 0x06, 0xD0, 0x03, 0x4C, 0x98, 0x82, 0x4C, 0x93, 0x82 });
        }

        int[] tunicLocs = { 0x285C, 0x40b1, 0x40c1, 0x40d1, 0x80e1, 0x80b1, 0x80c1, 0x80d1, 0x80e1, 0xc0b1, 0xc0c1, 0xc0d1, 0xc0e1, 0x100b1, 0x100c1, 0x100d1, 0x100e1, 0x140b1, 0x140c1, 0x140d1, 0x140e1, 0x17c1b, 0x1c466, 0x1c47e };

        foreach (int l in tunicLocs)
        {
            ROMData.Put(0x10ea, (byte)c2);
            if (props.CharSprite == CharacterSprite.IRON_KNUCKLE)
            {
                ROMData.Put(0x10ea, (byte)0x30);
                ROMData.Put(0x2a0a, 0x0D);
                ROMData.Put(0x2a10, (byte)c2);
                ROMData.Put(l, 0x20);
                ROMData.Put(l - 1, (byte)c2);
                ROMData.Put(l - 2, 0x0D);
            }
            else if(props.CharSprite == CharacterSprite.SAMUS)
            {
                ROMData.Put(0x2a0a, 0x16);
                ROMData.Put(0x2a10, 0x1a);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x1a);
                ROMData.Put(l - 2, 0x16);
            }
            else if (props.CharSprite == CharacterSprite.ERROR || props.CharSprite == CharacterSprite.VASE_LADY)
            {
                ROMData.Put(0x2a0a, 0x0F);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 2, 0x0F);
            }
            else if(props.CharSprite == CharacterSprite.SIMON)
            {
                ROMData.Put(0x2a0a, 0x07);
                ROMData.Put(0x2a10, 0x37);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x37);
                ROMData.Put(l - 2, 0x07);
            }
            else if(props.CharSprite == CharacterSprite.STALFOS)
            {
                ROMData.Put(0x2a0a, 0x08);
                ROMData.Put(0x2a10, 0x20);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x20);
                ROMData.Put(l - 2, 0x08);
            }
            else if(props.CharSprite == CharacterSprite.RUTO)
            {
                ROMData.Put(0x2a0a, 0x0c);
                ROMData.Put(0x2a10, 0x1c);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x1c);
                ROMData.Put(l - 2, 0x0c);
            }
            else if(props.CharSprite == CharacterSprite.YOSHI)
            {
                ROMData.Put(0x2a0a, 0x16);
                ROMData.Put(0x2a10, 0x20);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x20);
                ROMData.Put(l - 2, 0x16);
            }
            else if(props.CharSprite == CharacterSprite.DRAGONLORD)
            {
                ROMData.Put(0x2a0a, 0x28);
                ROMData.Put(0x2a10, 0x11);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x11);
                ROMData.Put(l - 2, 0x28);
            }
            else if (props.CharSprite == CharacterSprite.MIRIA)
            {
                ROMData.Put(0x2a0a, 0x0D);
                ROMData.Put(0x2a10, 0x30);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x30);
                ROMData.Put(l - 2, 0x0D);
                
            }
            else if (props.CharSprite == CharacterSprite.CRYSTALIS)
            {
                ROMData.Put(0x2a0a, 0x0D);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x0D);

            }
            else if (props.CharSprite == CharacterSprite.TACO)
            {
                ROMData.Put(0x2a0a, 0x18);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x18);

            }
            else if (props.CharSprite == CharacterSprite.PYRAMID)
            {
                ROMData.Put(0x2a0a, 0x12);
                ROMData.Put(0x2a10, 0x22);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x22);
                ROMData.Put(l - 2, 0x12);

            }
            /*
            else if (props.charSprite == CharacterSprite.FAXANADU)
            {
                ROMData.Put(0x2a0a, 0x18);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x18);

            }
            */
            else if (props.CharSprite == CharacterSprite.LADY_LINK)
            {
                ROMData.Put(0x2a0a, 0x18);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x18);

            }
            else if (props.CharSprite == CharacterSprite.HOODIE_LINK)
            {
                ROMData.Put(0x2a0a, 0x18);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x18);

            }
            else if (props.CharSprite == CharacterSprite.GLITCH_WITCH)
            {
                ROMData.Put(0x2a0a, 0x08);
                ROMData.Put(0x2a10, 0x36);
                ROMData.Put(l, (byte)c2);
                ROMData.Put(l - 1, 0x36);
                ROMData.Put(l - 2, 0x08);

            }
            else
            {
                ROMData.Put(0x10ea, (byte)c2);
                ROMData.Put(l, (byte)c2);
            }
        }

        ROMData.Put(0xe9e, (byte)c1);



        int beamType = -1;
        if (props.BeamSprite.Equals("Random"))
        {

            Random r2 = new Random();
            beamType = r2.Next(6);
        }
        else if (props.BeamSprite.Equals("Fire"))
        {
            beamType = 0;
        }
        else if (props.BeamSprite.Equals("Bubble"))
        {
            beamType = 1;
        }
        else if (props.BeamSprite.Equals("Rock"))
        {
            beamType = 2;
        }
        else if (props.BeamSprite.Equals("Axe"))
        {
            beamType = 3;
        }
        else if (props.BeamSprite.Equals("Hammer"))
        {
            beamType = 4;
        }
        else if (props.BeamSprite.Equals("Wizzrobe Beam"))
        {
            beamType = 5;
        }
        byte[] newSprite = new Byte[32];

        if (beamType == 0 || beamType == 3 || beamType == 4)
        {
            ROMData.Put(0x18f5, 0xa9);
            ROMData.Put(0x18f6, 0x00);
            ROMData.Put(0x18f7, 0xea);
        }
        else if(beamType != -1)
        {
            ROMData.Put(0X18FB, 0x84);
        }

        if (beamType == 1)//bubbles
        {
            for (int i = 0; i < 32; i++)
            {
                Byte next = ROMData.GetByte(0x20ab0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 2)//rocks
        {
            for (int i = 0; i < 32; i++)
            {
                Byte next = ROMData.GetByte(0x22af0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 3)//axes
        {
            for (int i = 0; i < 32; i++)
            {
                Byte next = ROMData.GetByte(0x22fb0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 4)//hammers
        {
            for (int i = 0; i < 32; i++)
            {
                Byte next = ROMData.GetByte(0x32ef0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 5)//wizzrobe beam
        {
            for (int i = 0; i < 32; i++)
            {
                Byte next = ROMData.GetByte(0x34dd0 + i);
                newSprite[i] = next;
            }
        }


        if (beamType != 0 && beamType != -1)
        {
            foreach (int loc in fireLocs)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.Put(loc + i, newSprite[i]);
                }
            }
        }


        if (props.DisableBeep)
        {
            //C9 20 - EA 38
            //CMP 20 -> NOP SEC
            ROMData.Put(0x1D4E4, (Byte)0xEA);
            ROMData.Put(0x1D4E5, (Byte)0x38);
        }
        if (props.ShuffleLifeRefill)
        {
            int lifeRefill = RNG.Next(1, 6);
            ROMData.Put(0xE7A, (Byte)(lifeRefill * 16));
        }

        if (props.ShuffleStealExpAmt)
        {
            int small = ROMData.GetByte(0x1E30E);
            int big = ROMData.GetByte(0x1E314);
            small = RNG.Next((int)(small - small * .5), (int)(small + small * .5) + 1);
            big = RNG.Next((int)(big - big * .5), (int)(big + big * .5) + 1);
            ROMData.Put(0x1E30E, (Byte)small);
            ROMData.Put(0x1E314, (Byte)big);
        }

        List<int> addr = new List<int>();
        for (int i = 0x54E8; i < 0x54ED; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x54EF; i < 0x54F8; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x54F9; i < 0x5508; i++)
        {
            addr.Add(i);
        }

        if (props.ShuffleEnemyStealExp)
        {
            ShuffleBits(addr, false);
        }

        if (props.ShuffleSwordImmunity)
        {
            ShuffleBits(addr, true);
        }

        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            ShuffleEnemyExp(addr);
        }
        addr = new List<int>();
        for (int i = 0x94E8; i < 0x94ED; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x94EF; i < 0x94F8; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x94F9; i < 0x9502; i++)
        {
            addr.Add(i);
        }
        if (props.ShuffleEnemyStealExp)
        {
            ShuffleBits(addr, false);
        }

        if (props.ShuffleSwordImmunity)
        {
            ShuffleBits(addr, true);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            ShuffleEnemyExp(addr);
        }

        addr = new List<int>();
        for (int i = 0x114E8; i < 0x114EA; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x114EB; i < 0x114ED; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x114EF; i < 0x114F8; i++)
        {
            addr.Add(i);
        }
        for (int i = 0x114FD; i < 0x11505; i++)
        {
            addr.Add(i);
        }
        addr.Add(0x11508);

        if (props.ShuffleEnemyStealExp)
        {
            ShuffleBits(addr, false);
        }

        if (props.ShuffleSwordImmunity)
        {
            ShuffleBits(addr, true);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            ShuffleEnemyExp(addr);
        }

        addr = new List<int>();
        for (int i = 0x129E8; i < 0x129EA; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x129EB; i < 0x129ED; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x129EF; i < 0x129F4; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x129F5; i < 0x129F7; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x129FD; i < 0x12A05; i++)
        {
            addr.Add(i);
        }

        addr.Add(0x12A08);

        if (props.ShuffleEnemyStealExp)
        {
            ShuffleBits(addr, false);
        }

        if (props.ShuffleSwordImmunity)
        {
            ShuffleBits(addr, true);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            ShuffleEnemyExp(addr);
        }

        addr = new List<int>();
        for (int i = 0x154E9; i < 0x154ED; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x154F2; i < 0x154F8; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x154F9; i < 0x15500; i++)
        {
            addr.Add(i);
        }

        for (int i = 0x15502; i < 15504; i++)
        {
            addr.Add(i);
        }

        if (props.ShuffleEnemyStealExp)
        {
            ShuffleBits(addr, false);
        }

        if (props.ShuffleSwordImmunity)
        {
            ShuffleBits(addr, true);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            ShuffleEnemyExp(addr);
        }

        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            addr = new List<int>();
            addr.Add(0x11505);
            addr.Add(0x13C88);
            addr.Add(0x13C89);
            addr.Add(0x12A05);
            addr.Add(0x12A06);
            addr.Add(0x12A07);
            addr.Add(0x15507);
            ShuffleEnemyExp(addr);
        }

        if (props.ShuffleEncounters)
        {
            addr = new List<int>();
            addr.Add(0x441b); // 0x62: West northern grass
            addr.Add(0x4419); // 0x5D: West northern desert
            addr.Add(0x441D); // 0x67: West northern forest
            addr.Add(0x4420); // 0x70: West swamp
            addr.Add(0x441C); // 0x63: West south grass
            addr.Add(0x441A); // 0x5E: West south desert
            addr.Add(0x4422); // 0x75: West grave
            addr.Add(0x441E); // 0x68: West south forest

            if (props.AllowPathEnemies)
            {
                addr.Add(0x4424);
                addr.Add(0x4423);
            }

            ShuffleEncounters(addr);

            addr = new List<int>();
            addr.Add(0x841B); // 0x62: East grass
            addr.Add(0x8419); // 0x5D: East desert
            addr.Add(0x841D); // 0x67: East forest
            addr.Add(0x8422); // 0x75: East grave
            addr.Add(0x8420); // 0x70: East swamp
            addr.Add(0x841A); // 0x5E: East south desert
            addr.Add(0x841E); // 0x68: East south forest
            if (props.IncludeLavaInEncounterShuffle)
            {
                addr.Add(0x8426); // 0x7C: Valley of death
            }

            if (props.AllowPathEnemies)
            {
                addr.Add(0x8423);
                addr.Add(0x8424);
            }

            ShuffleEncounters(addr);
        }

        if (props.JumpAlwaysOn)
        {
            ROMData.Put(0x1482, ROMData.GetByte(0x1480));
            ROMData.Put(0x1483, ROMData.GetByte(0x1481));
            ROMData.Put(0x1486, ROMData.GetByte(0x1484));
            ROMData.Put(0x1487, ROMData.GetByte(0x1485));

        }

        if (props.DisableMagicRecs)
        {
            ROMData.Put(0xF539, (Byte)0xC9);
            ROMData.Put(0xF53A, (Byte)0);
        }

        if (props.ShuffleAtkExp)
        {
            ShuffleExp(0x1669, props.AttackCap);
        }

        if (props.ShuffleMagicExp)
        {
            ShuffleExp(0x1671, props.MagicCap);
        }

        if (props.ShuffleLifeExp)
        {
            ShuffleExp(0x1679, props.LifeCap);
        }

        ROMData.SetLevelCap(props.AttackCap, props.MagicCap, props.LifeCap);

        ShuffleAttackEffectiveness(false);

        ShuffleLifeEffectiveness(true);

        ShuffleLifeEffectiveness(false);

        ROMData.Put(0x17B10, (Byte)props.StartGems);


        startHearts = props.StartHearts;
        ROMData.Put(0x17B00, (Byte)startHearts);


        maxHearts = props.MaxHearts;

        heartContainersInItemPool = maxHearts - startHearts;


        ROMData.Put(0x1C369, (Byte)props.StartLives);

        ROMData.Put(0x17B12, (byte)((props.StartWithUpstab ? 0x04 : 0) + (props.StartWithDownstab ? 0x10 : 0)));

        //Swap up and Downstab
        if(props.SwapUpAndDownStab)
        {
            //Swap the ORAs that determine which stab to give you
            ROMData.Put(0xF4DF, 0x04);
            ROMData.Put(0xF4F7, 0x10);
            //Swap the ANDs that check whether or not you have the stab
            ROMData.Put(0xF4D3, 0x04);
            ROMData.Put(0xF4EB, 0x10);
        }

        if (props.LifeEffectiveness == StatEffectiveness.MAX)
        {
            for (int i = 0x1E2BF; i < 0x1E2BF + 56; i++)
            {
                ROMData.Put(i, 0);
            }
        }

        if (props.LifeEffectiveness == StatEffectiveness.NONE)
        {
            for (int i = 0x1E2BF; i < 0x1E2BF + 56; i++)
            {
                ROMData.Put(i, 0xFF);
            }
        }

        if (props.MagicEffectiveness == StatEffectiveness.MAX)
        {
            for (int i = 0xD8B; i < 0xD8b + 64; i++)
            {
                ROMData.Put(i, 0);
            }
        }

        if (props.PalacePalette)
        {
            shuffler.ShufflePalacePalettes(ROMData, RNG);
        }

        if (props.ShuffleItemDropFrequency)
        {
            int drop = RNG.Next(5) + 4;
            ROMData.Put(0x1E8B0, (Byte)drop);
        }

    }

    private Byte IntToText(int x)
    {
        switch (x)
        {
            case 0:
                return (Byte)0xD0;
            case 1:
                return (Byte)0xD1;
            case 2:
                return (Byte)0xD2;
            case 3:
                return (Byte)0xD3;
            case 4:
                return (Byte)0xD4;
            case 5:
                return (Byte)0xD5;
            case 6:
                return (Byte)0xD6;
            case 7:
                return (Byte)0xD7;
            case 8:
                return (Byte)0xD8;
            default:
                return (Byte)0xD9;
        }
    }

    private void UpdateRom(byte[] hash)
    {
        foreach (World world in worlds)
        {
            List<Location> locs = world.AllLocations;
            foreach (Location location in locs)
            {
                location.UpdateBytes();
                ROMData.Put(location.MemAddress, location.LocationBytes[0]);
                ROMData.Put(location.MemAddress + overworldXOff, location.LocationBytes[1]);
                ROMData.Put(location.MemAddress + overworldMapOff, location.LocationBytes[2]);
                ROMData.Put(location.MemAddress + overworldWorldOff, location.LocationBytes[3]);
            }
            world.RemoveUnusedConnectors();
        }


        Location medicineLoc = null;
        Location trophyLoc = null;
        Location kidLoc = null;
        foreach (Location location in itemLocs)
        {
            if (location.item == Item.MEDICINE)
            {
                medicineLoc = location;
            }
            if (location.item == Item.TROPHY)
            {
                trophyLoc = location;
            }
            if (location.item == Item.CHILD)
            {
                kidLoc = location;
            }
        }

        byte[] medSprite = new Byte[32];
        byte[] trophySprite = new Byte[32];
        byte[] kidSprite = new Byte[32];

        for (int i = 0; i < 32; i++)
        {
            medSprite[i] = ROMData.GetByte(0x23310 + i);
            trophySprite[i] = ROMData.GetByte(0x232f0 + i);
            kidSprite[i] = ROMData.GetByte(0x25310 + i);
        }
        bool medEast = (eastHyrule.AllLocations.Contains(medicineLoc) || mazeIsland.AllLocations.Contains(medicineLoc));
        bool trophyEast = (eastHyrule.AllLocations.Contains(trophyLoc) || mazeIsland.AllLocations.Contains(trophyLoc));
        bool kidWest = (westHyrule.AllLocations.Contains(kidLoc) || deathMountain.AllLocations.Contains(kidLoc));
        Dictionary<int, int> palaceMems = new Dictionary<int, int>();
        palaceMems.Add(1, 0x29AD0);
        palaceMems.Add(2, 0x2BAD0);
        palaceMems.Add(3, 0x33AD0);
        palaceMems.Add(4, 0x35AD0);
        palaceMems.Add(5, 0x37AD0);
        palaceMems.Add(6, 0x39AD0);

        if (medEast && eastHyrule.palace5.item != Item.MEDICINE && eastHyrule.palace6.item != Item.MEDICINE && mazeIsland.palace4.item != Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x25430 + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0x43);
            ROMData.Put(0x1eeba, 0x43);
        }

        if (trophyEast)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x25410 + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0x41);
            ROMData.Put(0x1eeb8, 0x41);
        }

        if (kidWest && westHyrule.palace1.item != Item.CHILD && westHyrule.palace2.item != Item.CHILD && westHyrule.palace3.item != Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x23570 + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0x57);
            ROMData.Put(0x1eeb6, 0x57);
        }

        if (eastHyrule.newKasuto.item == Item.TROPHY || eastHyrule.newKasuto2.item == Item.TROPHY || westHyrule.lifeNorth.item == Item.TROPHY || westHyrule.lifeSouth.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27210 + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0x21);
            ROMData.Put(0x1eeb8, 0x21);
        }

        if (eastHyrule.newKasuto.item == Item.MEDICINE || eastHyrule.newKasuto2.item == Item.MEDICINE || westHyrule.lifeNorth.item == Item.TROPHY || westHyrule.lifeSouth.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27230 + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0x23);
            ROMData.Put(0x1eeba, 0x23);
        }

        if (eastHyrule.newKasuto.item == Item.CHILD || eastHyrule.newKasuto2.item == Item.CHILD || westHyrule.lifeNorth.item == Item.TROPHY || westHyrule.lifeSouth.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27250 + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0x25);
            ROMData.Put(0x1eeb6, 0x25);
        }

        if (westHyrule.palace1.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace1.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.palace2.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace2.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.palace3.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace3.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (mazeIsland.palace4.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.palace4.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.palace5.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace5.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.palace6.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace6.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.gp.item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.gp.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }

        if (westHyrule.palace1.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace1.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.palace2.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace2.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.palace3.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace3.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (mazeIsland.palace4.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.palace4.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.palace5.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace5.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.palace6.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace6.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.gp.item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.gp.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }

        if (westHyrule.palace1.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace1.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.palace2.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace2.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.palace3.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.palace3.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (mazeIsland.palace4.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.palace4.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.palace5.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace5.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.palace6.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.palace6.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.gp.item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.gp.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }

        ROMData.AddCredits();
        //fixes improper exit from p6/new kasuto
        //if (eastHyrule.palace6.PalNum != 7)
        //{
        //    ROMData.put(0x1ccd3, 0xea);
        //    ROMData.put(0x1ccd4, 0xea);
        //    ROMData.put(0x1ccd5, 0xea);
        //    ROMData.put(0x1ccd6, 0xc0);
        //    ROMData.put(0x1ccd7, 0x31);
        //}


        ROMData.Put(0x1CD3A, (Byte)palGraphics[westHyrule.palace1.PalNum]);


        ROMData.Put(0x1CD3B, (Byte)palGraphics[westHyrule.palace2.PalNum]);


        ROMData.Put(0x1CD3C, (Byte)palGraphics[westHyrule.palace3.PalNum]);


        ROMData.Put(0x1CD46, (Byte)palGraphics[mazeIsland.palace4.PalNum]);


        ROMData.Put(0x1CD42, (Byte)palGraphics[eastHyrule.palace5.PalNum]);

        ROMData.Put(0x1CD43, (Byte)palGraphics[eastHyrule.palace6.PalNum]);
        ROMData.Put(0x1CD44, (Byte)palGraphics[eastHyrule.gp.PalNum]);

        //if (!props.palacePalette)
        //{

        ROMData.Put(0x1FFF4, (Byte)palPalettes[westHyrule.palace1.PalNum]);

        ROMData.Put(0x1FFF5, (Byte)palPalettes[westHyrule.palace2.PalNum]);

        ROMData.Put(0x1FFF6, (Byte)palPalettes[westHyrule.palace3.PalNum]);

        ROMData.Put(0x20000, (Byte)palPalettes[mazeIsland.palace4.PalNum]);

        ROMData.Put(0x1FFFC, (Byte)palPalettes[eastHyrule.palace5.PalNum]);

        ROMData.Put(0x1FFFD, (Byte)palPalettes[eastHyrule.palace6.PalNum]);

        ROMData.Put(0x1FFFE, (Byte)palPalettes[eastHyrule.gp.PalNum]);

        //}

        if (props.ShuffleDripper)
        {
            ROMData.Put(0x11927, (Byte)enemies1[RNG.Next(enemies1.Count)]);
        }

        if (props.ShuffleEnemyPalettes)
        {
            List<int> doubleLocs = new List<int> { 0x40b4, 0x80b4, 0x100b4, 0x100b8, 0x100bc, 0x140b4, 0x140b8, 0x140bc };
            List<int> singleLocs = new List<int> { 0x40b8, 0x40bc, 0x80b8, 0x80bc };

            foreach (int i in doubleLocs)
            {
                int low = RNG.Next(12) + 1;
                int high = (RNG.Next(2) + 1) * 16;
                int color = high + low;
                ROMData.Put(i, (byte)color);
                ROMData.Put(i + 16, (byte)color);
                ROMData.Put(i - 1, (byte)(color - 15));
                ROMData.Put(i + 16 - 1, (byte)(color - 15));
            }
            foreach (int i in singleLocs)
            {
                int low = RNG.Next(13);
                int high = (RNG.Next(3)) * 16;
                int color = high + low;
                ROMData.Put(i, (byte)color);
                ROMData.Put(i + 16, (byte)color);
                ROMData.Put(i + 16 - 1, (byte)(color - 15));
            }

            for (int i = 0x54e8; i < 0x5508; i++)
            {
                if (i != 0x54f8)
                {
                    int b = ROMData.GetByte(i);
                    int p = b & 0x3F;
                    int n = RNG.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }

            for (int i = 0x94e8; i < 0x9508; i++)
            {
                if (i != 0x94f8)
                {
                    int b = ROMData.GetByte(i);
                    int p = b & 0x3F;
                    int n = RNG.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }
            for (int i = 0x114e8; i < 0x11508; i++)
            {
                if (i != 0x114f8)
                {
                    int b = ROMData.GetByte(i);
                    int p = b & 0x3F;
                    int n = RNG.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }
            for (int i = 0x129e8; i < 0x12a09; i++)
            {
                if (i != 0x129f8)
                {
                    int b = ROMData.GetByte(i);
                    int p = b & 0x3F;
                    int n = RNG.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }
            for (int i = 0x154e8; i < 0x15508; i++)
            {
                if (i != 0x154f8)
                {
                    int b = ROMData.GetByte(i);
                    int p = b & 0x3F;
                    int n = RNG.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }
        }

        ROMData.Put(0x4DEA, (Byte)westHyrule.trophyCave.item);
        ROMData.Put(0x502A, (Byte)westHyrule.jar.item);
        ROMData.Put(0x4DD7, (Byte)westHyrule.heart2.item);
        //logger.WriteLine(westHyrule.heart1.item);
        //logger.WriteLine(westHyrule.heart2.item);
        //logger.WriteLine(westHyrule.medCave.item);
        //logger.WriteLine(westHyrule.trophyCave.item);
        //logger.WriteLine(westHyrule.jar.item);
        //logger.WriteLine(deathMountain.magicCave.item);
        //logger.WriteLine(deathMountain.hammerCave.item);
        //logger.WriteLine(westHyrule.palace1.PalNum + " " + westHyrule.palace1.item);
        //logger.WriteLine(westHyrule.palace2.PalNum + " " + westHyrule.palace2.item);
        //logger.WriteLine(westHyrule.palace3.PalNum + " " + westHyrule.palace3.item);
        //logger.WriteLine(mazeIsland.palace4.PalNum + " " + mazeIsland.palace4.item);
        //logger.WriteLine(eastHyrule.palace5.PalNum + " " + eastHyrule.palace5.item);
        //logger.WriteLine(eastHyrule.palace6.PalNum + " " + eastHyrule.palace6.item);
        //logger.WriteLine(eastHyrule.gp.PalNum + " " + eastHyrule.gp.item);
        //logger.WriteLine(eastHyrule.heart1.item);
        //logger.WriteLine(eastHyrule.heart2.item);
        //logger.WriteLine(mazeIsland.magic.item);
        //logger.WriteLine(mazeIsland.kid.item);
        //logger.WriteLine(eastHyrule.newKasuto.item);
        //logger.WriteLine(eastHyrule.newKasuto2.item);
        //logger.WriteLine(eastHyrule.pbagCave1.item);
        //logger.WriteLine(eastHyrule.pbagCave2.item);
        //logger.WriteLine(eastHyrule.gp.item);
        //logger.WriteLine(spellMap[spells.life]);
        //logger.WriteLine(spellMap[spells.shield]);
        //logger.WriteLine(spellMap[spells.fire]);
        //logger.WriteLine(spellMap[spells.reflect]);
        //logger.WriteLine(spellMap[spells.jump]);
        //logger.WriteLine(spellMap[spells.thunder]);
        //logger.WriteLine(spellMap[spells.fairy]);
        //logger.WriteLine(spellMap[spells.spell]);

        int[] itemLocs2 = { 0x10E91, 0x10E9A, 0x1252D, 0x12538, 0x10EA3, 0x12774 };


        ROMData.Put(0x5069, (Byte)westHyrule.medCave.item);
        ROMData.Put(0x4ff5, (Byte)westHyrule.heart1.item);
        
        ROMData.Put(0x65C3, (Byte)deathMountain.magicCave.item);
        ROMData.Put(0x6512, (Byte)deathMountain.hammerCave.item);
        ROMData.Put(0x8FAA, (Byte)eastHyrule.waterTile.item);
        ROMData.Put(0x9011, (Byte)eastHyrule.desertTile.item);
        if (props.palaceStyle != PalaceStyle.RECONSTRUCTED)
        {
            if (westHyrule.palace1.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.palace1.PalNum - 1], (Byte)westHyrule.palace1.item);
            }
            if (westHyrule.palace2.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.palace2.PalNum - 1], (Byte)westHyrule.palace2.item);
            }
            if (westHyrule.palace3.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.palace3.PalNum - 1], (Byte)westHyrule.palace3.item);
            }
            if (eastHyrule.palace5.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.palace5.PalNum - 1], (Byte)eastHyrule.palace5.item);
            }
            if (eastHyrule.palace6.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.palace6.PalNum - 1], (Byte)eastHyrule.palace6.item);
            }
            if (mazeIsland.palace4.PalNum != 7)
            {
                ROMData.Put(itemLocs2[mazeIsland.palace4.PalNum - 1], (Byte)mazeIsland.palace4.item);
            }


            if (eastHyrule.gp.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.gp.PalNum - 1], (Byte)eastHyrule.gp.item);
            }
        }
        else
        {
            ROMData.ElevatorBossFix(props.BossItem);
            if (westHyrule.palace1.PalNum != 7)
            {
                palaces[westHyrule.palace1.PalNum-1].UpdateItem(westHyrule.palace1.item, ROMData);
            }
            if (westHyrule.palace2.PalNum != 7)
            {
                palaces[westHyrule.palace2.PalNum - 1].UpdateItem(westHyrule.palace2.item, ROMData);
            }
            if (westHyrule.palace3.PalNum != 7)
            {
                palaces[westHyrule.palace3.PalNum - 1].UpdateItem(westHyrule.palace3.item, ROMData);
            }
            if (eastHyrule.palace5.PalNum != 7)
            {
                palaces[eastHyrule.palace5.PalNum - 1].UpdateItem(eastHyrule.palace5.item, ROMData);
            }
            if (eastHyrule.palace6.PalNum != 7)
            {
                palaces[eastHyrule.palace6.PalNum - 1].UpdateItem(eastHyrule.palace6.item, ROMData);
            }
            if (mazeIsland.palace4.PalNum != 7)
            {
                palaces[mazeIsland.palace4.PalNum - 1].UpdateItem(mazeIsland.palace4.item, ROMData);
            }


            if (eastHyrule.gp.PalNum != 7)
            {
                palaces[eastHyrule.gp.PalNum - 1].UpdateItem(eastHyrule.gp.item, ROMData);
            }

            ROMData.Put(westHyrule.palace1.MemAddress + 0x7e, (byte)palaces[westHyrule.palace1.PalNum - 1].Root.NewMap);
            ROMData.Put(westHyrule.palace2.MemAddress + 0x7e, (byte)palaces[westHyrule.palace2.PalNum - 1].Root.NewMap);
            ROMData.Put(westHyrule.palace3.MemAddress + 0x7e, (byte)palaces[westHyrule.palace3.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.palace5.MemAddress + 0x7e, (byte)palaces[eastHyrule.palace5.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.palace6.MemAddress + 0x7e, (byte)palaces[eastHyrule.palace6.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.gp.MemAddress + 0x7e, (byte)palaces[eastHyrule.gp.PalNum - 1].Root.NewMap);
            ROMData.Put(mazeIsland.palace4.MemAddress + 0x7e, (byte)palaces[mazeIsland.palace4.PalNum - 1].Root.NewMap);

        }
        if (eastHyrule.newKasuto.TownNum == Town.NEW_KASUTO)
        {
            ROMData.Put(0xDB95, (Byte)eastHyrule.newKasuto2.item); //map 47

            ROMData.Put(0xDB8C, (Byte)eastHyrule.newKasuto.item); //map 46
        }
        else
        {
            ROMData.Put(0xDB95, (Byte)westHyrule.lifeSouth.item); //map 47

            ROMData.Put(0xDB8C, (Byte)westHyrule.lifeNorth.item); //map 46
        }

        if (props.TownSwap)
        {
            if (westHyrule.shieldTown.TownNum != Town.RAURU)
            {
                ROMData.Put(westHyrule.shieldTown.MemAddress + 0x7E, (byte)(westHyrule.shieldTown.Map + 0xC0));
                ROMData.Put(westHyrule.shieldTown.MemAddress + 0xBD, (byte)8);
                ROMData.Put(eastHyrule.nabooru.MemAddress + 0x7E, (byte)(eastHyrule.nabooru.Map + 0xC0));
                ROMData.Put(eastHyrule.nabooru.MemAddress + 0xBD, (byte)6);
            }

            if (westHyrule.jump.TownNum != Town.RUTO)
            {
                ROMData.Put(westHyrule.jump.MemAddress + 0x7E, (byte)(westHyrule.jump.Map + 0xC0));
                ROMData.Put(westHyrule.jump.MemAddress + 0xBD, (byte)8);
                ROMData.Put(eastHyrule.darunia.MemAddress + 0x7E, (byte)(eastHyrule.darunia.Map + 0xC0));
                ROMData.Put(eastHyrule.darunia.MemAddress + 0xBD, (byte)6);
            }

            if (westHyrule.lifeNorth.TownNum != Town.SARIA_NORTH)
            {
                ROMData.Put(westHyrule.lifeNorth.MemAddress + 0x7E, (byte)(westHyrule.lifeNorth.Map));
                ROMData.Put(westHyrule.lifeNorth.MemAddress + 0xBD, (byte)8);
                ROMData.Put(westHyrule.lifeSouth.MemAddress, (byte)0);
                ROMData.Put(eastHyrule.newKasuto.MemAddress + 0x7E, (byte)(eastHyrule.newKasuto.Map + 0xC0));
                ROMData.Put(eastHyrule.newKasuto.MemAddress + 0xBD, (byte)6);
                ROMData.Put(eastHyrule.newKasuto2.MemAddress + 0x7F, (byte)(eastHyrule.newKasuto2.Map));
                ROMData.Put(eastHyrule.newKasuto2.MemAddress + 0xBE, (byte)6);
            }

            if (westHyrule.fairy.TownNum != Town.MIDO)
            {
                ROMData.Put(westHyrule.fairy.MemAddress + 0x7E, (byte)(westHyrule.fairy.Map + 0xC0));
                ROMData.Put(westHyrule.fairy.MemAddress + 0xBD, (byte)8);
                ROMData.Put(eastHyrule.oldKasuto.MemAddress + 0x7E, (byte)(eastHyrule.oldKasuto.Map + 0xC0));
                ROMData.Put(eastHyrule.oldKasuto.MemAddress + 0xBD, (byte)6);
            }
        }

        ROMData.Put(0xA5A8, (Byte)mazeIsland.magic.item);
        ROMData.Put(0xA58B, (Byte)mazeIsland.kid.item);
        
        if (props.PbagItemShuffle)
        {
            ROMData.Put(0x4FE2, (Byte)westHyrule.pbagCave.item);
            ROMData.Put(0x8ECC, (Byte)eastHyrule.pbagCave1.item);
            ROMData.Put(0x8FB3, (Byte)eastHyrule.pbagCave2.item);

        }

        foreach (Location location in pbagHearts)
        {
            if (location == westHyrule.pbagCave)
            {
                ROMData.Put(0x4FE2, (Byte)westHyrule.pbagCave.item);
            }

            if (location == eastHyrule.pbagCave1)
            {
                ROMData.Put(0x8ECC, (Byte)eastHyrule.pbagCave1.item);
            }
            if (location == eastHyrule.pbagCave2)
            {
                ROMData.Put(0x8FB3, (Byte)eastHyrule.pbagCave2.item);
            }
        }

        //Make a hash for select screen
        long inthash = BitConverter.ToInt64(hash, 0);

        ROMData.Put(0x17C2C, (byte)(((inthash) & 0x1F) + 0xD0));
        ROMData.Put(0x17C2E, (byte)(((inthash >> 5) & 0x1F) + 0xD0));
        ROMData.Put(0x17C30, (byte)(((inthash >> 10) & 0x1F) + 0xD0));
        ROMData.Put(0x17C32, (byte)(((inthash >> 15) & 0x1F) + 0xD0));
        ROMData.Put(0x17C34, (byte)(((inthash >> 20) & 0x1F) + 0xD0));
        ROMData.Put(0x17C36, (byte)(((inthash >> 25) & 0x1F) + 0xD0));

        //Update raft animation
        bool firstRaft = false;
        foreach(World w in worlds)
        {
            if (w.raft != null)
            {
                if (!firstRaft)
                {
                    ROMData.Put(0x538, (Byte)w.raft.Xpos);
                    ROMData.Put(0x53A, (Byte)w.raft.Ypos);
                    firstRaft = true;
                } 
                else
                {
                    ROMData.Put(0x539, (Byte)w.raft.Xpos);
                    ROMData.Put(0x53B, (Byte)w.raft.Ypos);
                }
            } 
        }

        firstRaft = false;
        //Fix Maze Island Bridge music bug
        foreach (World w in worlds)
        {
            if (w.bridge != null)
            {
                if (!firstRaft)
                {
                    ROMData.Put(0x565, (Byte)w.bridge.Xpos);
                    ROMData.Put(0x567, (Byte)w.bridge.Ypos);
                    firstRaft = true;
                }
                else
                {
                    ROMData.Put(0x564, (Byte)w.bridge.Xpos);
                    ROMData.Put(0x566, (Byte)w.bridge.Ypos);
                }
            }
        }

        //Update world check for p7
        if (westHyrule.palace1.PalNum == 7 || westHyrule.palace2.PalNum == 7 || westHyrule.palace3.PalNum == 7)
        {
            ROMData.Put(0x1dd3b, 0x05);
        }

        if (mazeIsland.palace4.PalNum == 7)
        {
            ROMData.Put(0x1dd3b, 0x14);
        }

        int spellNameBase = 0x1c3a, effectBase = 0x00e58, spellCostBase = 0xd8b, functionBase = 0xdcb;

        int[,] magLevels = new int[8, 8];
        int[,] magNames = new int[8, 7];
        int[] magEffects = new int[16];
        int[] magFunction = new int[8];
        ROMData.UpdateSpellText(spellMap);

        for (int i = 0; i < magFunction.Count(); i++)
        {
            magFunction[i] = ROMData.GetByte(functionBase + (int)spellMap[(Spell)i]);
        }

        for (int i = 0; i < magEffects.Count(); i = i + 2)
        {
            magEffects[i] = ROMData.GetByte(effectBase + (int)spellMap[(Spell)(i / 2)] * 2);
            magEffects[i + 1] = ROMData.GetByte(effectBase + (int)spellMap[(Spell)(i / 2)] * 2 + 1);
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                magLevels[i, j] = ROMData.GetByte(spellCostBase + ((int)spellMap[(Spell)i] * 8 + j));
            }

            for (int j = 0; j < 7; j++)
            {
                magNames[i, j] = ROMData.GetByte(spellNameBase + ((int)spellMap[(Spell)i] * 0xe + j));
            }
        }

        for (int i = 0; i < magFunction.Count(); i++)
        {
            ROMData.Put(functionBase + i, (Byte)magFunction[i]);
        }

        for (int i = 0; i < magEffects.Count(); i = i + 2)
        {
            ROMData.Put(effectBase + i, (Byte)magEffects[i]);
            ROMData.Put(effectBase + i + 1, (Byte)magEffects[i + 1]);
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ROMData.Put(spellCostBase + (i * 8) + j, (Byte)magLevels[i, j]);
            }

            for (int j = 0; j < 7; j++)
            {
                ROMData.Put(spellNameBase + (i * 0xe) + j, (Byte)magNames[i, j]);
            }
        }

        //fix for rope graphical glitch
        for (int i = 0; i < 16; i++)
        {
            ROMData.Put(0x32CD0 + i, ROMData.GetByte(0x34CD0 + i));
        }

        //if (hiddenPalace)
        //{
        //    ROMData.put(0x8664, 0);
        //}

        //if (hiddenKasuto)
        //{
        //    ROMData.put(0x8660, 0);
        //}

    }

    public void ShuffleEnemies(int enemyPtr, int enemyAddr, List<int> enemies, List<int> generators, List<int> shorties, List<int> tallGuys, List<int> flyingEnemies, bool p7)
    {
        //refactor this to use enemy arrays in rooms
        int maps = 0;
        List<int> mapsNos = new List<int>();
        if(!p7)
        {
            List<int> palacesInt = new List<int> { 1, 2, 5 };
            if (enemyPtr == enemyPtr2)
            {
                palacesInt = new List<int> { 3, 4, 6 };
            }

            foreach(int palace in palacesInt) 
            { 
                foreach(Room r in palaces[palace-1].AllRooms)
                {
                    if(r.NewMap == 0)
                    {
                        mapsNos.Add(r.Map);
                    }
                    else
                    {
                        mapsNos.Add(r.NewMap);
                    }
                }
            }
        }
        else
        {
            foreach (Room r in palaces[6].AllRooms)
            {
                if (r.NewMap == 0)
                {
                    mapsNos.Add(r.Map);
                }
                else
                {
                    mapsNos.Add(r.NewMap);
                }
            }
        }
        foreach(int map in mapsNos) 
        {
            int low = ROMData.GetByte(enemyPtr + map * 2);
            int high = ROMData.GetByte(enemyPtr + map * 2 + 1);
            high = high << 8;
            high = high & 0x0FFF;
            int addr = high + low + enemyAddr;
            ShuffleEnemies(high + low + enemyAddr, enemies, generators, shorties, tallGuys, flyingEnemies, p7);
        }
    }

    public void ShuffleEnemies(int addr, List<int> enemies, List<int> generators, List<int> shorties, List<int> tallGuys, List<int> flyingEnemies, bool p7)
    {
        if (!visitedEnemies.Contains(addr))
        {
            int numBytes = ROMData.GetByte(addr);
            for (int j = addr + 2; j < addr + numBytes; j = j + 2)
            {
                int enemy = ROMData.GetByte(j) & 0x3F;
                int highPart = ROMData.GetByte(j) & 0xC0;
                if (props.MixEnemies)
                {
                    if (enemies.Contains(enemy))
                    {
                        int swap = enemies[RNG.Next(0, enemies.Count)];
                        int ypos = ROMData.GetByte(j - 1) & 0xF0;
                        int xpos = ROMData.GetByte(j - 1) & 0x0F;
                        if (shorties.Contains(enemy) && tallGuys.Contains(swap))
                        {
                            ypos = ypos - 16;
                            while (swap == 0x1D && ypos != 0x70 && !p7)
                            {
                                swap = tallGuys[RNG.Next(0, tallGuys.Count)];
                            }
                        }
                        else
                        {
                            while (swap == 0x1D && ypos != 0x70 && !p7)
                            {
                                swap = enemies[RNG.Next(0, enemies.Count)];
                            }
                        }


                        ROMData.Put(j - 1, (Byte)(ypos + xpos));
                        ROMData.Put(j, (Byte)(swap + highPart));
                    }
                }
                else
                {
                    if (tallGuys.Contains(enemy))
                    {
                        int swap = RNG.Next(0, tallGuys.Count);
                        int ypos = ROMData.GetByte(j - 1) & 0xF0;
                        while (tallGuys[swap] == 0x1D && ypos != 0x70 && !p7)
                        {
                            swap = RNG.Next(0, tallGuys.Count);
                        }
                        ROMData.Put(j, (Byte)(tallGuys[swap] + highPart));
                    }

                    if (shorties.Contains(enemy))
                    {
                        int swap = RNG.Next(0, shorties.Count);
                        ROMData.Put(j, (Byte)(shorties[swap] + highPart));
                    }
                }


                if (flyingEnemies.Contains(enemy))
                {
                    int swap = RNG.Next(0, flyingEnemies.Count);
                    while (enemy == 0x07 && (flyingEnemies[swap] == 0x06 || flyingEnemies[swap] == 0x0E))
                    {
                        swap = RNG.Next(0, flyingEnemies.Count);
                    }
                    ROMData.Put(j, (Byte)(flyingEnemies[swap] + highPart));
                }

                if (generators.Contains(enemy))
                {
                    int swap = RNG.Next(0, generators.Count);
                    ROMData.Put(j, (Byte)(generators[swap] + highPart));
                }

                if (enemy == 0x0B)
                {
                    int swap = RNG.Next(0, generators.Count + 1);
                    if (swap != generators.Count)
                    {
                        ROMData.Put(j, (Byte)(generators[swap] + highPart));
                    }
                }
            }
            visitedEnemies.Add(addr);
        }

    }
    public void ShuffleSmallItems(int world, bool first)
    {
        logger.Debug("World: " + world);
        List<int> addresses = new List<int>();
        List<int> items = new List<int>();
        int startAddr;
        if (first)
        {
            startAddr = 0x8523 - 0x8000 + (world * 0x4000) + 0x10;
        }
        else
        {
            startAddr = 0xA000 - 0x8000 + (world * 0x4000) + 0x10;
        }
        int map = 0;
        for (int i = startAddr; i < startAddr + 126; i = i + 2)
        {

            map++;
            int low = ROMData.GetByte(i);
            int hi = ROMData.GetByte(i + 1) * 256;
            int numBytes = ROMData.GetByte(hi + low + 16 - 0x8000 + (world * 0x4000));
            for (int j = 4; j < numBytes; j = j + 2)
            {
                int yPos = ROMData.GetByte(hi + low + j + 16 - 0x8000 + (world * 0x4000)) & 0xF0;
                yPos = yPos >> 4;
                if (ROMData.GetByte(hi + low + j + 1 + 16 - 0x8000 + (world * 0x4000)) == 0x0F && yPos < 13)
                {
                    int addr = hi + low + j + 2 + 16 - 0x8000 + (world * 0x4000);
                    int item = ROMData.GetByte(addr);
                    if (UNSAFE_DEBUG && (item == 8 || (item > 9 && item < 14) || (item > 15 && item < 19) && !addresses.Contains(addr)))
                    {
                        logger.Debug("Map: " + map);
                        logger.Debug("Item: " + item);
                        logger.Debug("Address: {0:X}", addr);
                        addresses.Add(addr);
                        items.Add(item);
                    }
                    j++;
                }
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            int swap = RNG.Next(i, items.Count);
            int temp = items[swap];
            items[swap] = items[i];
            items[i] = temp;
        }
        for (int i = 0; i < addresses.Count; i++)
        {
            ROMData.Put(addresses[i], (Byte)items[i]);
        }
    }

    public void PrintSpoiler(LogLevel logLevel)
    {
        logger.Log(logLevel, "ITEMS:");
        foreach (Item item in SHUFFLABLE_STARTING_ITEMS) {
            logger.Log(logLevel, item.ToString() + "(" + itemGet[item] + ") : " + itemLocs.Where(i => i.item == item).FirstOrDefault()?.Name);
        }
    }

}
