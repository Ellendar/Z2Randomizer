using Newtonsoft.Json.Linq;
using NLog;
using RandomizerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Z2Randomizer.Core.Overworld;
using Z2Randomizer.Core.Sidescroll;

namespace Z2Randomizer.Core;

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

    public const bool UNSAFE_DEBUG = true;

    private readonly Item[] SHUFFLABLE_STARTING_ITEMS = new Item[] { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HAMMER, Item.MAGIC_KEY };

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();


    private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
    private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

 
    //private ROM romData;
    private const int overworldXOff = 0x3F;
    private const int overworldMapOff = 0x7E;
    private const int overworldWorldOff = 0xBD;

    private static readonly List<Town> spellLocations = new List<Town>() { Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST,
        Town.MIDO_CHURCH, Town.NABOORU, Town.DARUNIA_WEST, Town.DARUNIA_ROOF, Town.NEW_KASUTO, Town.OLD_KASUTO};

    //Unused
    private Dictionary<int, int> spellEnters;
    //Unused
    private Dictionary<int, int> spellExits;
    //Unused
    public HashSet<String> reachableAreas;
    //Which vanilla spell corresponds with which shuffled spell
    private Dictionary<Town, Spell> SpellMap { get; set; }
    //Locations that contain an item
    private List<Location> itemLocs;
    //Locations that are pbags in vanilla that are turned into hearts because maxhearts - startinghearts > 4
    private List<Location> pbagHearts;
    //Continent connectors
    protected (Location, Location)[] connections = new (Location, Location)[4];

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
    public int totalReachableCheck = 0;


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
        string export = JsonSerializer.Serialize(props);
        Debug.WriteLine(export);
        Flags = config.Serialize();
        Seed = config.Seed;
        logger.Info("Started generation for " + Flags + " / " + config.Seed);

        ROMData = new ROM(props.Filename);
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

        if(props.DashAlwaysOn)
        {
            ROMData.Put(0x13C3, new Byte[] { 0x30, 0xD0 });
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
        if (props.UpARestartsAtPalaces)
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

        startRandomizeStartingValuesTimestamp = DateTime.Now;
        RandomizeStartingValues();
        startRandomizeEnemiesTimestamp = DateTime.Now;
        RandomizeEnemyStats();


        palaces = Palaces.CreatePalaces(worker, RNG, props, ROMData);
        while(palaces == null || palaces.Count != 7)
        {
            if (palaces == null)
            {
                continue;
            }
            palaces = Palaces.CreatePalaces(worker, RNG, props, ROMData);
        }

        firstProcessOverworldTimestamp = DateTime.Now;
        ProcessOverworld();
        bool f = UpdateProgress(8);
        if (!f)
        {
            return;
        }
        List<Hint> hints = ROMData.GetGameText();
        ROMData.WriteHints(Hints.GenerateHints(itemLocs, startTrophy, startMed, startKid, SpellMap, westHyrule.bagu, hints, props, RNG));
        f = UpdateProgress(9);
        if (!f)
        {
            return;
        }
        MD5 hasher = MD5.Create();
        byte[] finalRNGState = new byte[32];
        RNG.NextBytes(finalRNGState);
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(
            Flags + 
            Seed +
            //Assembly.GetExecutingAssembly().GetName().Version.Major +
            //Assembly.GetExecutingAssembly().GetName().Version.Minor +
            //Assembly.GetExecutingAssembly().GetName().Version.Revision +
            //TODO: Since the modularization split, ExecutingAssembly's version data always returns 0.0.0.0
            //Eventually we need to turn this back into a read from the assembly, but for now I'm just adding an awful hard write of the version.
            "4.2.0" +
            File.ReadAllText(config.GetRoomsFile()) +
            finalRNGState
        ));
        UpdateRom(hash);
        string newFileName = props.Filename.Substring(0, props.Filename.LastIndexOf("\\") + 1) + "Z2_" + Seed + "_" + Flags + ".nes";
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
                foreach (Room room in palace.AllRooms.OrderBy(i => i.NewMap ?? i.Map))
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
                ROMData.Put(0x1E67D + i, (byte)atk[i]);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(0x1E67D + i, (byte)192);
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

        ROMData.Put(RomMap.START_CANDLE, props.StartCandle ? (byte)1 : (byte)0);
        itemGet[Item.CANDLE] = props.StartCandle;
        ROMData.Put(RomMap.START_GLOVE, props.StartGlove ? (byte)1 : (byte)0);
        itemGet[Item.GLOVE] = props.StartGlove;
        ROMData.Put(RomMap.START_RAFT, props.StartRaft ? (byte)1 : (byte)0);
        itemGet[Item.RAFT] = props.StartRaft;
        ROMData.Put(RomMap.START_BOOTS, props.StartBoots ? (byte)1 : (byte)0);
        itemGet[Item.BOOTS] = props.StartBoots;
        ROMData.Put(RomMap.START_FLUTE, props.StartFlute ? (byte)1 : (byte)0);
        itemGet[Item.FLUTE] = props.StartFlute;
        ROMData.Put(RomMap.START_CROSS, props.StartCross ? (byte)1 : (byte)0);
        itemGet[Item.CROSS] = props.StartCross;
        ROMData.Put(RomMap.START_HAMMER, props.StartHammer ? (byte)1 : (byte)0);
        itemGet[Item.HAMMER] = props.StartHammer;
        ROMData.Put(RomMap.START_MAGICAL_KEY, props.StartKey ? (byte)1 : (byte)0);
        itemGet[Item.MAGIC_KEY] = props.StartKey;

        itemList = new List<Item> { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MEDICINE, Item.TROPHY, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MAGIC_KEY, Item.MAGIC_CONTAINER, Item.HAMMER, Item.CHILD, Item.MAGIC_CONTAINER };

        if (props.PbagItemShuffle)
        {
            westHyrule.pbagCave.Item = (Item)ROMData.GetByte(0x4FE2);
            eastHyrule.pbagCave1.Item = (Item)ROMData.GetByte(0x8ECC);
            eastHyrule.pbagCave2.Item = (Item)ROMData.GetByte(0x8FB3);
            itemList.Add(westHyrule.pbagCave.Item);
            itemList.Add(eastHyrule.pbagCave1.Item);
            itemList.Add(eastHyrule.pbagCave2.Item);

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
                        westHyrule.pbagCave.Item = Item.HEART_CONTAINER;
                        itemList.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(westHyrule.pbagCave);
                        x--;
                    }
                    if (y == 1 && !pbagHearts.Contains(eastHyrule.pbagCave1))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave1);
                        eastHyrule.pbagCave1.Item = Item.HEART_CONTAINER;
                        itemList.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(eastHyrule.pbagCave1);
                        x--;
                    }
                    if (y == 2 && !pbagHearts.Contains(eastHyrule.pbagCave2))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave2);
                        eastHyrule.pbagCave2.Item = Item.HEART_CONTAINER;
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

        if (SpellGet[SpellMap[Town.MIDO_WEST]])
        {
            itemList[9] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.MEDICINE] = true;
            startMed = true;
        }

        if(SpellGet[SpellMap[Town.RUTO]])
        {
            itemList[10] = smallItems[RNG.Next(smallItems.Count)];
            itemGet[Item.TROPHY] = true;
            startTrophy = true;
        }

        if(SpellGet[SpellMap[Town.DARUNIA_WEST]])
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
            itemLocs[i].Item = itemList[i];
        }
        foreach (Location location in itemLocs)
        {
            if (location.Item == Item.CHILD)
            {
                kidLoc = location;
            }
            else if (location.Item == Item.TROPHY)
            {
                trophyLoc = location;
            }
            else if (location.Item == Item.MEDICINE)
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
        totalReachableCheck++;
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
                //if (count > 120)
                //{
                //    debug++;
                //    PrintRoutingDebug(count, wh, eh, dm, mi);
                //    
                //   return false;
                //}
                itemGetReachableFailures++;
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
            //PrintRoutingDebug(count, wh, eh, dm, mi);
            return false;
        }
        if (heartContainers != maxHearts)
        {
            heartContainerReachableFailures++;
            //PrintRoutingDebug(count, wh, eh, dm, mi);
            return false;
        }
        if(SpellGet.Values.Any(i => i == false))
        {
            spellGetReachableFailures++;
            return false;
        }

        bool retval = (CanGet(westHyrule.Locations[Terrain.TOWN]) 
            && CanGet(eastHyrule.Locations[Terrain.TOWN]) 
            && CanGet(westHyrule.locationAtPalace1) 
            && CanGet(westHyrule.locationAtPalace2) 
            && CanGet(westHyrule.locationAtPalace3) 
            && CanGet(mazeIsland.locationAtPalace4) 
            && CanGet(eastHyrule.locationAtPalace5) 
            && CanGet(eastHyrule.locationAtPalace6) 
            && CanGet(eastHyrule.locationAtGP) 
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
            ROMData.Put(0xC611 + i, (byte)0x75);
            ROMData.Put(0xC611 + i + 1, (byte)0x70);
            ROMData.Put(0xC593 + i, (byte)0x48);
            ROMData.Put(0xC593 + i + 1, (byte)0x9B);
        }
        ROMData.Put(0xC7BB, (byte)0x07);
        ROMData.Put(0xC7BF, (byte)0x13);
        ROMData.Put(0xC7C3, (byte)0x21);
        ROMData.Put(0xC7C7, (byte)0x27);
        ROMData.Put(0xC7CB, (byte)0x37);
        ROMData.Put(0xC7CF, (byte)0x3F);
        ROMData.Put(0xC850, (byte)0xB0);
        //ROMData.put(0xC7D3, (byte)0x4D);
        ROMData.Put(0xC7D7, (byte)0x5E);
        ROMData.Put(0xC7DF, (byte)0x43);
        ROMData.Put(0xC870, (byte)0xB8);
        ROMData.Put(0xC7E3, (byte)0x49);
        ROMData.Put(0xC874, (byte)0xA8);
        ROMData.Put(0xC7D3, (byte)0x4D);
        ROMData.Put(0xC7DB, (byte)0x29);
        //ROMData.put(0xC7E3, (byte)0x49);
        // ROMData.put(0xC874, (byte)0xA8);
        //ROMData.put(0x8560, (byte)0xBC);
    }

    private bool UpdateSpells()
    {
        bool changed = false;
        List<RequirementType> requireables = GetRequireables();
        foreach (Town town in spellLocations)
        {
            if (Towns.townSpellAndItemRequirements[town].AreSatisfiedBy(requireables))
            {
                if (!SpellGet[SpellMap[town]])
                {
                    changed = true;
                }
                SpellGet[SpellMap[town]] = true;
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
        accessibleMagicContainers = 4 + AllLocationsForReal().Where(i => i.itemGet == true && i.Item == Item.MAGIC_CONTAINER).Count();
        heartContainers = startHearts;
        bool changed = false;
        
        foreach (Location location in AllLocationsForReal().Where(i => i.Item != Item.DO_NOT_USE))
        {
            bool hadItemPreviously = location.itemGet;
            bool hasItemNow;
            if (location.PalNum > 0 && location.PalNum < 7)
            {
                Palace palace = palaces[location.PalNum - 1];
                hasItemNow = CanGet(location)
                    && (SpellGet[Spell.FAIRY] || itemGet[Item.MAGIC_KEY])
                    && palace.IsTraversable(GetRequireables(), location);
                    /*
                    && (!palace.NeedDstab || (palace.NeedDstab && SpellGet[Spell.DOWNSTAB])) 
                    && (!palace.NeedFairy || (palace.NeedFairy && SpellGet[Spell.FAIRY])) 
                    && (!palace.NeedGlove || (palace.NeedGlove && itemGet[Item.GLOVE])) 
                    && (!palace.NeedJumpOrFairy || (palace.NeedJumpOrFairy && (SpellGet[Spell.JUMP]) || SpellGet[Spell.FAIRY])) 
                    && (!palace.NeedReflect || (palace.NeedReflect && SpellGet[Spell.REFLECT]));
                    */
            }
            else if (location.TownNum == Town.NEW_KASUTO)
            {
                hasItemNow = CanGet(location) && (accessibleMagicContainers >= kasutoJars) && (!location.NeedHammer || itemGet[Item.HAMMER]);
            }
            else if (location.TownNum == Town.SPELL_TOWER)
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
            itemGet[location.Item] = hasItemNow || hadItemPreviously;

            if (location.itemGet && location.Item == Item.HEART_CONTAINER)
            {
                heartContainers++;
            }
            if(!hadItemPreviously && location.itemGet)
            {
                changed = true;
            }
        }

        if(!props.PbagItemShuffle)
        {
            if(westHyrule.pbagCave.Reachable)
            {
                westHyrule.pbagCave.itemGet = true;
            }
            if (eastHyrule.pbagCave1.Reachable)
            {
                eastHyrule.pbagCave1.itemGet = true;
            }
            if (eastHyrule.pbagCave2.Reachable)
            {
                eastHyrule.pbagCave2.itemGet = true;
            }
        }

        accessibleMagicContainers = 4 + AllLocationsForReal().Where(i => i.itemGet == true && i.Item == Item.MAGIC_CONTAINER).Count();
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
                ROMData.Put(start + (i * 8) + j, (byte)(highPart + (lowPart * 2)));
            }
        }

    }

    private void RandomizeEnemyStats()
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
            ROMData.Put(0x005432, (byte)193);
            ROMData.Put(0x009432, (byte)193);
            ROMData.Put(0x11436, (byte)193);
            ROMData.Put(0x12936, (byte)193);
            ROMData.Put(0x15532, (byte)193);
            ROMData.Put(0x11437, (byte)192);
            ROMData.Put(0x1143F, (byte)192);
            ROMData.Put(0x12937, (byte)192);
            ROMData.Put(0x1293F, (byte)192);
            ROMData.Put(0x15445, (byte)192);
            ROMData.Put(0x15446, (byte)192);
            ROMData.Put(0x15448, (byte)192);
            ROMData.Put(0x15453, (byte)193);
            ROMData.Put(0x12951, (byte)227);

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

            ROMData.Put(i, (byte)newVal);
        }
    }

    public List<RequirementType> GetRequireables()
    {
        List<RequirementType> requireables = new();
        /*
        && (!palace.NeedDstab || (palace.NeedDstab && SpellGet[Spell.DOWNSTAB]))
        && (!palace.NeedFairy || (palace.NeedFairy && SpellGet[Spell.FAIRY]))
        && (!palace.NeedGlove || (palace.NeedGlove && itemGet[Item.GLOVE]))
        && (!palace.NeedJumpOrFairy || (palace.NeedJumpOrFairy && (SpellGet[Spell.JUMP]) || SpellGet[Spell.FAIRY]))
        && (!palace.NeedReflect || (palace.NeedReflect && SpellGet[Spell.REFLECT]));
        */
        if (SpellGet[Spell.DOWNSTAB])
        {
            requireables.Add(RequirementType.DOWNSTAB);
        }
        if (SpellGet[Spell.UPSTAB])
        {
            requireables.Add(RequirementType.UPSTAB);
        }
        if (SpellGet[Spell.FAIRY])
        {
            requireables.Add(RequirementType.FAIRY);
        }
        if (SpellGet[Spell.REFLECT])
        {
            requireables.Add(RequirementType.REFLECT);
        }
        if (SpellGet[Spell.JUMP])
        {
            requireables.Add(RequirementType.JUMP);
        }
        if (SpellGet[Spell.SPELL])
        {
            requireables.Add(RequirementType.SPELL);
        }
        if (itemGet[Item.GLOVE])
        {
            requireables.Add(RequirementType.GLOVE);
        }
        if (itemGet[Item.MAGIC_KEY])
        {
            requireables.Add(RequirementType.KEY);
        }
        if(accessibleMagicContainers >= 5 || props.DisableMagicRecs) 
        {
            requireables.Add(RequirementType.FIVE_CONTAINERS);
        }
        if (accessibleMagicContainers >= 6 || props.DisableMagicRecs)
        {
            requireables.Add(RequirementType.SIX_CONTAINERS);
        }
        if (accessibleMagicContainers >= 7 || props.DisableMagicRecs)
        {
            requireables.Add(RequirementType.SEVEN_CONTAINERS);
        }
        if (accessibleMagicContainers == 8 || props.DisableMagicRecs)
        {
            requireables.Add(RequirementType.EIGHT_CONTAINERS);
        }
        if (itemGet[Item.TROPHY] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.TROPHY);
        }
        if (itemGet[Item.MEDICINE] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.MEDICINE);
        }
        if (itemGet[Item.CHILD] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.CHILD);
        }
        return requireables;
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
                //This just supports town swap, which never worked, so it does nothing.
                //ShuffleTowns();

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
                    Location l1, l2;
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

                    l1 = worlds[raftw1].LoadRaft(raftw2);
                    l2 = worlds[raftw2].LoadRaft(raftw1);
                    connections[0] = (l1, l2);

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

                    l1 = worlds[bridgew1].LoadBridge(bridgew2);
                    l2 = worlds[bridgew2].LoadBridge(bridgew1);
                    connections[1] = (l1, l2);

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

                    l1 = worlds[c1w1].LoadCave1(c1w2);
                    l2 = worlds[c1w2].LoadCave1(c1w1);
                    connections[2] = (l1, l2);

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

                    l1 = worlds[c2w1].LoadCave2(c2w2);
                    l2 = worlds[c2w2].LoadCave2(c2w1);
                    connections[3] = (l1, l2);
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
                w.ShuffleOverworldEnemies(props.GeneratorsAlwaysMatch);
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
        Location l1, l2;
        if(type == 1)
        {
            l1 = worlds[w1].LoadRaft(w2);
            l2 = worlds[w2].LoadRaft(w1);
            connections[0] = (l1, l2);
        }
        else if (type == 2)
        {
            l1 = worlds[w1].LoadBridge(w2);
            l2 = worlds[w2].LoadBridge(w1);
            connections[1] = (l1, l2);
        }
        else if(type == 3)
        {
            l1 = worlds[w1].LoadCave1(w2);
            l2 = worlds[w2].LoadCave1(w1);
            connections[2] = (l1, l2);
        }
        else
        {
            l1 = worlds[w1].LoadCave2(w2);
            l2 = worlds[w2].LoadCave2(w1);
            connections[3] = (l1, l2);
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

    /*
    private void ShuffleTowns()
    {
        westHyrule.shieldTown.TownNum = Town.RAURU;
        westHyrule.jump.TownNum = Town.RUTO;
        westHyrule.lifeNorth.TownNum = Town.SARIA_NORTH;
        westHyrule.lifeSouth.TownNum = Town.SARIA_SOUTH;
        westHyrule.fairy.TownNum = Town.MIDO_WEST;
        eastHyrule.nabooru.TownNum = Town.NABOORU;
        eastHyrule.darunia.TownNum = Town.DARUNIA_WEST;
        eastHyrule.newKasuto.TownNum = Town.NEW_KASUTO;
        eastHyrule.newKasuto2.TownNum = Town.SPELL_TOWER;
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
    */

    private void ShufflePalaces()
    {

        if (props.SwapPalaceCont)
        {

            List<Location> pals = new List<Location> { westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3, mazeIsland.locationAtPalace4, eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6 };

            if (props.P7shuffle)
            {
                pals.Add(eastHyrule.locationAtGP);
            }

            //Replaced with fisher-yates
            for (int i = pals.Count() - 1; i > 0; i--)
            {
                int swap = RNG.Next(i + 1);
                Util.Swap(pals[i], pals[swap]);
            }

            westHyrule.locationAtPalace1.World = westHyrule.locationAtPalace1.World & 0xFC;
            westHyrule.locationAtPalace2.World = westHyrule.locationAtPalace2.World & 0xFC;
            westHyrule.locationAtPalace3.World = westHyrule.locationAtPalace3.World & 0xFC;

            mazeIsland.locationAtPalace4.World = mazeIsland.locationAtPalace4.World & 0xFC;
            mazeIsland.locationAtPalace4.World = mazeIsland.locationAtPalace4.World | 0x03;

            eastHyrule.locationAtPalace5.World = eastHyrule.locationAtPalace5.World & 0xFC;
            eastHyrule.locationAtPalace5.World = eastHyrule.locationAtPalace5.World | 0x02;

            eastHyrule.locationAtPalace6.World = eastHyrule.locationAtPalace6.World & 0xFC;
            eastHyrule.locationAtPalace6.World = eastHyrule.locationAtPalace6.World | 0x02;

            if (props.P7shuffle)
            {
                eastHyrule.locationAtGP.World = eastHyrule.locationAtGP.World & 0xFC;
                eastHyrule.locationAtGP.World = eastHyrule.locationAtGP.World | 0x02;
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
            if (props.PalaceStyle == PalaceStyle.RECONSTRUCTED)
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
        if (westHyrule.locationAtPalace1.PalNum != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace1);
        }
        if (westHyrule.locationAtPalace2.PalNum != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace2);
        }
        if (westHyrule.locationAtPalace3.PalNum != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace3);
        }
        if (mazeIsland.locationAtPalace4.PalNum != 7)
        {
            itemLocs.Add(mazeIsland.locationAtPalace4);
        }
        if (eastHyrule.locationAtPalace5.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.locationAtPalace5);
        }
        if (eastHyrule.locationAtPalace6.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.locationAtPalace6);
        }
        if (eastHyrule.locationAtGP.PalNum != 7)
        {
            itemLocs.Add(eastHyrule.locationAtGP);
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
        SpellMap = new();
        SpellGet.Clear();

        List<Town> unallocatedTowns = new List<Town> { Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST, 
            Town.NABOORU, Town.DARUNIA_WEST, Town.NEW_KASUTO, Town.OLD_KASUTO };

        foreach (Spell spell in Enum.GetValues(typeof(Spell)))
        {
            if(props.DashSpell && spell == Spell.FIRE 
                || !props.DashSpell && spell == Spell.DASH
                || spell == Spell.UPSTAB
                || spell == Spell.DOWNSTAB)
            {
                continue;
            }
            Town town = unallocatedTowns[RNG.Next(unallocatedTowns.Count)];
            unallocatedTowns.Remove(town);
            SpellGet.Add(spell, false);
            if(props.ShuffleSpellLocations)
            {
                SpellMap.Add(town, spell);
            }
            else
            {
                SpellMap.Add(town, town switch
                {
                    Town.RAURU => Spell.SHIELD,
                    Town.RUTO => Spell.JUMP,
                    Town.SARIA_NORTH => Spell.LIFE,
                    Town.MIDO_WEST => Spell.FAIRY,
                    Town.NABOORU => props.DashSpell ? Spell.DASH : Spell.FIRE,
                    Town.DARUNIA_WEST => Spell.REFLECT,
                    Town.NEW_KASUTO => Spell.SPELL,
                    Town.OLD_KASUTO => Spell.THUNDER,
                    _ => throw new Exception("Unrecognized vanilla spell location")
                });
            }
        }
        SpellMap.Add(Town.DARUNIA_ROOF, props.SwapUpAndDownStab ? Spell.DOWNSTAB : Spell.UPSTAB);
        SpellMap.Add(Town.MIDO_CHURCH, props.SwapUpAndDownStab ? Spell.UPSTAB : Spell.DOWNSTAB);
        SpellGet.Add(Spell.DOWNSTAB, false);
        SpellGet.Add(Spell.UPSTAB, false);

        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.SHIELD), props.StartShield ? (byte)1 : (byte)0);
        SpellGet[Spell.SHIELD] = props.StartShield;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.JUMP), props.StartJump ? (byte)1 : (byte)0);
        SpellGet[Spell.JUMP] = props.StartJump;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.LIFE), props.StartLife ? (byte)1 : (byte)0);
        SpellGet[Spell.LIFE] = props.StartLife;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.FAIRY), props.StartFairy ? (byte)1 : (byte)0);
        SpellGet[Spell.FAIRY] = props.StartFairy;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.FIRE), props.StartFire ? (byte)1 : (byte)0);
        SpellGet[Spell.FIRE] = props.StartFire;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.REFLECT), props.StartReflect ? (byte)1 : (byte)0);
        SpellGet[Spell.REFLECT] = props.StartReflect;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.SPELL), props.StartSpell ? (byte)1 : (byte)0);
        SpellGet[Spell.SPELL] = props.StartSpell;
        ROMData.Put(0x17AF7 + SpellMap.Values.ToList().IndexOf(Spell.THUNDER), props.StartThunder ? (byte)1 : (byte)0);
        SpellGet[Spell.THUNDER] = props.StartThunder;

        if (props.CombineFire)
        {
            int newFire = RNG.Next(7);
            if (newFire > 3)
            {
                newFire++;
            }
            Byte newnewFire = (byte)(0x10 | ROMData.GetByte(0xDCB + newFire));
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
            ROMData.Put(start + i, (byte)(cappedExp[i] / 256));
            ROMData.Put(start + 24 + i, (byte)(cappedExp[i] % 256));
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
            ROMData.Put(i, (byte)(part1 + part2));
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
            ROMData.Put(i, (byte)(high + low));
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
        ROMData.UpdateSprites(props.CharSprite, props.TunicColor, props.ShieldColor, props.BeamSprite);

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

        if (props.DisableBeep)
        {
            //C9 20 - EA 38
            //CMP 20 -> NOP SEC
            ROMData.Put(0x1D4E4, (byte)0xEA);
            ROMData.Put(0x1D4E5, (byte)0x38);
        }
        if (props.ShuffleLifeRefill)
        {
            int lifeRefill = RNG.Next(1, 6);
            ROMData.Put(0xE7A, (byte)(lifeRefill * 16));
        }

        if (props.ShuffleStealExpAmt)
        {
            int small = ROMData.GetByte(0x1E30E);
            int big = ROMData.GetByte(0x1E314);
            small = RNG.Next((int)(small - small * .5), (int)(small + small * .5) + 1);
            big = RNG.Next((int)(big - big * .5), (int)(big + big * .5) + 1);
            ROMData.Put(0x1E30E, (byte)small);
            ROMData.Put(0x1E314, (byte)big);
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
            ROMData.Put(0xF539, (byte)0xC9);
            ROMData.Put(0xF53A, (byte)0);
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

        ROMData.Put(0x17B10, (byte)props.StartGems);


        startHearts = props.StartHearts;
        ROMData.Put(0x17B00, (byte)startHearts);


        maxHearts = props.MaxHearts;

        heartContainersInItemPool = maxHearts - startHearts;


        ROMData.Put(0x1C369, (byte)props.StartLives);

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

        if (props.ShufflePalacePalettes)
        {
            shuffler.ShufflePalacePalettes(ROMData, RNG);
        }

        if (props.ShuffleItemDropFrequency)
        {
            int drop = RNG.Next(5) + 4;
            ROMData.Put(0x1E8B0, (byte)drop);
        }

    }

    private Byte IntToText(int x)
    {
        switch (x)
        {
            case 0:
                return (byte)0xD0;
            case 1:
                return (byte)0xD1;
            case 2:
                return (byte)0xD2;
            case 3:
                return (byte)0xD3;
            case 4:
                return (byte)0xD4;
            case 5:
                return (byte)0xD5;
            case 6:
                return (byte)0xD6;
            case 7:
                return (byte)0xD7;
            case 8:
                return (byte)0xD8;
            default:
                return (byte)0xD9;
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
            if (location.Item == Item.MEDICINE)
            {
                medicineLoc = location;
            }
            if (location.Item == Item.TROPHY)
            {
                trophyLoc = location;
            }
            if (location.Item == Item.CHILD)
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

        if (medEast && eastHyrule.locationAtPalace5.Item != Item.MEDICINE && eastHyrule.locationAtPalace6.Item != Item.MEDICINE && mazeIsland.locationAtPalace4.Item != Item.MEDICINE)
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

        if (kidWest && westHyrule.locationAtPalace1.Item != Item.CHILD && westHyrule.locationAtPalace2.Item != Item.CHILD && westHyrule.locationAtPalace3.Item != Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x23570 + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0x57);
            ROMData.Put(0x1eeb6, 0x57);
        }

        if (eastHyrule.newKasuto.Item == Item.TROPHY || eastHyrule.newKasuto2.Item == Item.TROPHY || westHyrule.lifeNorth.Item == Item.TROPHY || westHyrule.lifeSouth.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27210 + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0x21);
            ROMData.Put(0x1eeb8, 0x21);
        }

        if (eastHyrule.newKasuto.Item == Item.MEDICINE || eastHyrule.newKasuto2.Item == Item.MEDICINE || westHyrule.lifeNorth.Item == Item.TROPHY || westHyrule.lifeSouth.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27230 + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0x23);
            ROMData.Put(0x1eeba, 0x23);
        }

        if (eastHyrule.newKasuto.Item == Item.CHILD || eastHyrule.newKasuto2.Item == Item.CHILD || westHyrule.lifeNorth.Item == Item.TROPHY || westHyrule.lifeSouth.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27250 + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0x25);
            ROMData.Put(0x1eeb6, 0x25);
        }

        if (westHyrule.locationAtPalace1.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalNum] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }

        if (westHyrule.locationAtPalace1.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalNum] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }

        if (westHyrule.locationAtPalace1.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalNum] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalNum] + i, kidSprite[i]);
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


        ROMData.Put(0x1CD3A, (byte)palGraphics[westHyrule.locationAtPalace1.PalNum]);


        ROMData.Put(0x1CD3B, (byte)palGraphics[westHyrule.locationAtPalace2.PalNum]);


        ROMData.Put(0x1CD3C, (byte)palGraphics[westHyrule.locationAtPalace3.PalNum]);


        ROMData.Put(0x1CD46, (byte)palGraphics[mazeIsland.locationAtPalace4.PalNum]);


        ROMData.Put(0x1CD42, (byte)palGraphics[eastHyrule.locationAtPalace5.PalNum]);

        ROMData.Put(0x1CD43, (byte)palGraphics[eastHyrule.locationAtPalace6.PalNum]);
        ROMData.Put(0x1CD44, (byte)palGraphics[eastHyrule.locationAtGP.PalNum]);

        //if (!props.palacePalette)
        //{

        ROMData.Put(0x1FFF4, (byte)palPalettes[westHyrule.locationAtPalace1.PalNum]);

        ROMData.Put(0x1FFF5, (byte)palPalettes[westHyrule.locationAtPalace2.PalNum]);

        ROMData.Put(0x1FFF6, (byte)palPalettes[westHyrule.locationAtPalace3.PalNum]);

        ROMData.Put(0x20000, (byte)palPalettes[mazeIsland.locationAtPalace4.PalNum]);

        ROMData.Put(0x1FFFC, (byte)palPalettes[eastHyrule.locationAtPalace5.PalNum]);

        ROMData.Put(0x1FFFD, (byte)palPalettes[eastHyrule.locationAtPalace6.PalNum]);

        ROMData.Put(0x1FFFE, (byte)palPalettes[eastHyrule.locationAtGP.PalNum]);

        //}

        if (props.ShuffleDripper)
        {
            ROMData.Put(0x11927, (byte)Enemies.Palace125Enemies[RNG.Next(Enemies.Palace125Enemies.Length)]);
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

        ROMData.Put(0x4DEA, (byte)westHyrule.trophyCave.Item);
        ROMData.Put(0x502A, (byte)westHyrule.jar.Item);
        ROMData.Put(0x4DD7, (byte)westHyrule.heart2.Item);
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


        ROMData.Put(0x5069, (byte)westHyrule.medCave.Item);
        ROMData.Put(0x4ff5, (byte)westHyrule.heart1.Item);
        
        ROMData.Put(0x65C3, (byte)deathMountain.magicCave.Item);
        ROMData.Put(0x6512, (byte)deathMountain.hammerCave.Item);
        ROMData.Put(0x8FAA, (byte)eastHyrule.waterTile.Item);
        ROMData.Put(0x9011, (byte)eastHyrule.desertTile.Item);
        if (props.PalaceStyle != PalaceStyle.RECONSTRUCTED)
        {
            if (westHyrule.locationAtPalace1.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.locationAtPalace1.PalNum - 1], (byte)westHyrule.locationAtPalace1.Item);
            }
            if (westHyrule.locationAtPalace2.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.locationAtPalace2.PalNum - 1], (byte)westHyrule.locationAtPalace2.Item);
            }
            if (westHyrule.locationAtPalace3.PalNum != 7)
            {
                ROMData.Put(itemLocs2[westHyrule.locationAtPalace3.PalNum - 1], (byte)westHyrule.locationAtPalace3.Item);
            }
            if (eastHyrule.locationAtPalace5.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.locationAtPalace5.PalNum - 1], (byte)eastHyrule.locationAtPalace5.Item);
            }
            if (eastHyrule.locationAtPalace6.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.locationAtPalace6.PalNum - 1], (byte)eastHyrule.locationAtPalace6.Item);
            }
            if (mazeIsland.locationAtPalace4.PalNum != 7)
            {
                ROMData.Put(itemLocs2[mazeIsland.locationAtPalace4.PalNum - 1], (byte)mazeIsland.locationAtPalace4.Item);
            }


            if (eastHyrule.locationAtGP.PalNum != 7)
            {
                ROMData.Put(itemLocs2[eastHyrule.locationAtGP.PalNum - 1], (byte)eastHyrule.locationAtGP.Item);
            }
        }
        else
        {
            ROMData.ElevatorBossFix(props.BossItem);
            if (westHyrule.locationAtPalace1.PalNum != 7)
            {
                palaces[westHyrule.locationAtPalace1.PalNum-1].UpdateItem(westHyrule.locationAtPalace1.Item, ROMData);
            }
            if (westHyrule.locationAtPalace2.PalNum != 7)
            {
                palaces[westHyrule.locationAtPalace2.PalNum - 1].UpdateItem(westHyrule.locationAtPalace2.Item, ROMData);
            }
            if (westHyrule.locationAtPalace3.PalNum != 7)
            {
                palaces[westHyrule.locationAtPalace3.PalNum - 1].UpdateItem(westHyrule.locationAtPalace3.Item, ROMData);
            }
            if (eastHyrule.locationAtPalace5.PalNum != 7)
            {
                palaces[eastHyrule.locationAtPalace5.PalNum - 1].UpdateItem(eastHyrule.locationAtPalace5.Item, ROMData);
            }
            if (eastHyrule.locationAtPalace6.PalNum != 7)
            {
                palaces[eastHyrule.locationAtPalace6.PalNum - 1].UpdateItem(eastHyrule.locationAtPalace6.Item, ROMData);
            }
            if (mazeIsland.locationAtPalace4.PalNum != 7)
            {
                palaces[mazeIsland.locationAtPalace4.PalNum - 1].UpdateItem(mazeIsland.locationAtPalace4.Item, ROMData);
            }


            if (eastHyrule.locationAtGP.PalNum != 7)
            {
                palaces[eastHyrule.locationAtGP.PalNum - 1].UpdateItem(eastHyrule.locationAtGP.Item, ROMData);
            }

            ROMData.Put(westHyrule.locationAtPalace1.MemAddress + 0x7e, (byte)palaces[westHyrule.locationAtPalace1.PalNum - 1].Root.NewMap);
            ROMData.Put(westHyrule.locationAtPalace2.MemAddress + 0x7e, (byte)palaces[westHyrule.locationAtPalace2.PalNum - 1].Root.NewMap);
            ROMData.Put(westHyrule.locationAtPalace3.MemAddress + 0x7e, (byte)palaces[westHyrule.locationAtPalace3.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.locationAtPalace5.MemAddress + 0x7e, (byte)palaces[eastHyrule.locationAtPalace5.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.locationAtPalace6.MemAddress + 0x7e, (byte)palaces[eastHyrule.locationAtPalace6.PalNum - 1].Root.NewMap);
            ROMData.Put(eastHyrule.locationAtGP.MemAddress + 0x7e, (byte)palaces[eastHyrule.locationAtGP.PalNum - 1].Root.NewMap);
            ROMData.Put(mazeIsland.locationAtPalace4.MemAddress + 0x7e, (byte)palaces[mazeIsland.locationAtPalace4.PalNum - 1].Root.NewMap);

        }
        if (eastHyrule.newKasuto.TownNum == Town.NEW_KASUTO)
        {
            ROMData.Put(0xDB95, (byte)eastHyrule.newKasuto2.Item); //map 47

            ROMData.Put(0xDB8C, (byte)eastHyrule.newKasuto.Item); //map 46
        }
        else
        {
            ROMData.Put(0xDB95, (byte)westHyrule.lifeSouth.Item); //map 47

            ROMData.Put(0xDB8C, (byte)westHyrule.lifeNorth.Item); //map 46
        }

        //Town swap was never implemented, and when it is, it is going to work very different, so this should die.
        /*
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

            if (westHyrule.fairy.TownNum != Town.MIDO_WEST)
            {
                ROMData.Put(westHyrule.fairy.MemAddress + 0x7E, (byte)(westHyrule.fairy.Map + 0xC0));
                ROMData.Put(westHyrule.fairy.MemAddress + 0xBD, (byte)8);
                ROMData.Put(eastHyrule.oldKasuto.MemAddress + 0x7E, (byte)(eastHyrule.oldKasuto.Map + 0xC0));
                ROMData.Put(eastHyrule.oldKasuto.MemAddress + 0xBD, (byte)6);
            }
        }
        */

        ROMData.Put(0xA5A8, (byte)mazeIsland.magic.Item);
        ROMData.Put(0xA58B, (byte)mazeIsland.kid.Item);
        
        if (props.PbagItemShuffle)
        {
            ROMData.Put(0x4FE2, (byte)westHyrule.pbagCave.Item);
            ROMData.Put(0x8ECC, (byte)eastHyrule.pbagCave1.Item);
            ROMData.Put(0x8FB3, (byte)eastHyrule.pbagCave2.Item);

        }

        foreach (Location location in pbagHearts)
        {
            if (location == westHyrule.pbagCave)
            {
                ROMData.Put(0x4FE2, (byte)westHyrule.pbagCave.Item);
            }

            if (location == eastHyrule.pbagCave1)
            {
                ROMData.Put(0x8ECC, (byte)eastHyrule.pbagCave1.Item);
            }
            if (location == eastHyrule.pbagCave2)
            {
                ROMData.Put(0x8FB3, (byte)eastHyrule.pbagCave2.Item);
            }
        }

        AddCropGuideBoxesToFileSelect(ROMData);

        //Update raft animation
        bool firstRaft = false;
        foreach(World w in worlds)
        {
            if (w.raft != null)
            {
                if (!firstRaft)
                {
                    ROMData.Put(0x538, (byte)w.raft.Xpos);
                    ROMData.Put(0x53A, (byte)w.raft.Ypos);
                    firstRaft = true;
                } 
                else
                {
                    ROMData.Put(0x539, (byte)w.raft.Xpos);
                    ROMData.Put(0x53B, (byte)w.raft.Ypos);
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
                    ROMData.Put(0x565, (byte)w.bridge.Xpos);
                    ROMData.Put(0x567, (byte)w.bridge.Ypos);
                    firstRaft = true;
                }
                else
                {
                    ROMData.Put(0x564, (byte)w.bridge.Xpos);
                    ROMData.Put(0x566, (byte)w.bridge.Ypos);
                }
            }
        }

        //Update world check for p7
        if (westHyrule.locationAtPalace1.PalNum == 7 || westHyrule.locationAtPalace2.PalNum == 7 || westHyrule.locationAtPalace3.PalNum == 7)
        {
            ROMData.Put(0x1dd3b, 0x05);
        }

        if (mazeIsland.locationAtPalace4.PalNum == 7)
        {
            ROMData.Put(0x1dd3b, 0x14);
        }

        int spellNameBase = 0x1c3a, effectBase = 0x00e58, spellCostBase = 0xd8b, functionBase = 0xdcb;

        int[,] magLevels = new int[8, 8];
        int[,] magNames = new int[8, 7];
        int[] magEffects = new int[16];
        int[] magFunction = new int[8];
        ROMData.UpdateSpellText(SpellMap);

        for (int i = 0; i < magFunction.Count(); i++)
        {
            magFunction[i] = ROMData.GetByte(functionBase + (int)SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]]);
        }

        for (int i = 0; i < magEffects.Count(); i = i + 2)
        {
            magEffects[i] = ROMData.GetByte(effectBase + (int)SpellMap[Towns.STRICT_SPELL_LOCATIONS[i / 2]] * 2);
            magEffects[i + 1] = ROMData.GetByte(effectBase + (int)SpellMap[Towns.STRICT_SPELL_LOCATIONS[i / 2]] * 2 + 1);
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                magLevels[i, j] = ROMData.GetByte(spellCostBase + ((int)SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]] * 8 + j));
            }

            for (int j = 0; j < 7; j++)
            {
                magNames[i, j] = ROMData.GetByte(spellNameBase + ((int)SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]] * 0xe + j));
            }
        }

        for (int i = 0; i < magFunction.Count(); i++)
        {
            ROMData.Put(functionBase + i, (byte)magFunction[i]);
        }

        for (int i = 0; i < magEffects.Count(); i = i + 2)
        {
            ROMData.Put(effectBase + i, (byte)magEffects[i]);
            ROMData.Put(effectBase + i + 1, (byte)magEffects[i + 1]);
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ROMData.Put(spellCostBase + (i * 8) + j, (byte)magLevels[i, j]);
            }

            for (int j = 0; j < 7; j++)
            {
                ROMData.Put(spellNameBase + (i * 0xe) + j, (byte)magNames[i, j]);
            }
        }

        //fix for rope graphical glitch
        for (int i = 0; i < 16; i++)
        {
            ROMData.Put(0x32CD0 + i, ROMData.GetByte(0x34CD0 + i));
        }

        //These were commented out in 4.0.4. I have no idea what they do or why they exist.
        //if (hiddenPalace)
        //{
        //    ROMData.put(0x8664, 0);
        //}
        //if (hiddenKasuto)
        //{
        //    ROMData.put(0x8660, 0);
        //}

        long inthash = BitConverter.ToInt64(hash, 0);

        ROMData.Put(0x17C2C, (byte)(((inthash) & 0x1F) + 0xD0));
        ROMData.Put(0x17C2E, (byte)(((inthash >> 5) & 0x1F) + 0xD0));
        ROMData.Put(0x17C30, (byte)(((inthash >> 10) & 0x1F) + 0xD0));
        ROMData.Put(0x17C32, (byte)(((inthash >> 15) & 0x1F) + 0xD0));
        ROMData.Put(0x17C34, (byte)(((inthash >> 20) & 0x1F) + 0xD0));
        ROMData.Put(0x17C36, (byte)(((inthash >> 25) & 0x1F) + 0xD0));
    }

    /*
    public void ShufflePalaceEnemies(int enemyPtr, int enemyAddr, List<int> enemies, List<int> generators, List<int> shorties, List<int> tallGuys, List<int> flyingEnemies, bool isP7)
    {
        List<int> mapsNos = new List<int>();
        if(isP7)
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
        else
        {
            List<int> palacesInt = new List<int> { 1, 2, 5 };
            if (enemyPtr == palace346EnemyPtr)
            {
                palacesInt = new List<int> { 3, 4, 6 };
            }

            foreach (int palace in palacesInt)
            {
                foreach (Room r in palaces[palace - 1].AllRooms)
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
        }
        foreach(int map in mapsNos) 
        {
            int low = ROMData.GetByte(enemyPtr + map * 2);
            int high = ROMData.GetByte(enemyPtr + map * 2 + 1);
            high = high << 8;
            high = high & 0x0FFF;
            int addr = high + low + enemyAddr;
            ShuffleEnemies(high + low + enemyAddr, enemies, generators, shorties, tallGuys, flyingEnemies, isP7);
        }
    }
    */

    public void ShuffleEnemies(int addr, List<int> enemies, List<int> generators, 
        List<int> smallEnemies, List<int> largeEnemies, List<int> flyingEnemies, bool isP7)
    {
        throw new NotImplementedException();
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
                    if (false && (item == 8 || (item > 9 && item < 14) || (item > 15 && item < 19) && !addresses.Contains(addr)))
                    {
                        if(UNSAFE_DEBUG)
                        {
                            logger.Debug("Map: " + map);
                            logger.Debug("Item: " + item);
                            logger.Debug("Address: {0:X}", addr);
                        }
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
            ROMData.Put(addresses[i], (byte)items[i]);
        }
    }

    public void PrintSpoiler(LogLevel logLevel)
    {
        logger.Log(logLevel, "ITEMS:");
        foreach (Item item in SHUFFLABLE_STARTING_ITEMS) {
            logger.Log(logLevel, item.ToString() + "(" + itemGet[item] + ") : " + itemLocs.Where(i => i.Item == item).FirstOrDefault()?.Name);
        }
    }

    /// <summary>
    /// Simple conversion routine that converts from a PRG rom address to CPU address.
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    private byte[] PrgToCpuAddr(int addr)
    {
        ushort banksize = 0x4000;
        byte bank = (byte)(addr / banksize);
        // If the bank is the last bank, then its the fixed bank and all pointers start from 0xc000 instead
        ushort cpuOffset = (ushort) (bank == 7 ? 0xc000 : 0x8000);
        ushort val = (ushort)(addr - (bank * banksize) + cpuOffset);
        return new byte[] { (byte)(val & 0xff), (byte)(val >> 8) };
    }

    private void AddCropGuideBoxesToFileSelect(ROM rom)
    {
        byte hasINes = 0x10;

        var freeAddr = 0x17da2;
        var hookTitleScreenDraw = 0x1728d;
        byte whiteTile = 0xfd;
        byte blueTile = 0xfe;
        var addr = PrgToCpuAddr(freeAddr);

        // Hook into the title screen drawing to redirect it to our custom draw routine.
        rom.Put(hasINes + hookTitleScreenDraw, new byte[] { 0x20, addr[0], addr[1] }); // replace sta $0726 with jsr $BDB1 ; CustomFileSelectUpdates

        // and add the custom write code to the free space
        rom.Put(hasINes + freeAddr, new byte[]
        {
            // CustomFileSelectUpdates:
            0xc0, 0x01,       //  cpy #$02
            0xf0, 0x04,       //  beq Skip
            0x8d, 0x26, 0x07, //    sta $0726  ; perform the original hooked code before exiting
            0x60,             //    rts
            // Skip:
            0xa2, 0x20,       //  ldx #$20     ; copy 32 + 1 bytes of data from the rom (+1 for terminator)
            // Loop:
            0xbd, 0xc0, 0xbd, //  lda CustomFileSelectData,x
            0x9d, 0x02, 0x03, //  sta $0302,x
            0xca,             //  dex
            0x10, 0xf7,       //  bpl Loop    ; while x >= 0
            0xa9, 0x00,       //  lda #0    ; use the draw handler for $302
            0x8d, 0x25, 0x07, //  sta $0725 ; tell the code to draw from $302
            0xee, 0x3e, 0x07, //  inc $073E ; increase the "state" count
            0x68,             //  pla       ; we need to double return here to break out of the hook and the calling function
            0x68,             //  pla
            0x60,             //  rts
            // CustomFileSelectData:
            0x20, 0x00, 0x01, whiteTile,
            0x20, 0x1f, 0x01, whiteTile,
            0x23, 0xa0, 0x01, whiteTile,
            0x23, 0xbf, 0x01, whiteTile,
            0x20, 0x21, 0x01, blueTile,
            0x20, 0x3e, 0x01, blueTile,
            0x23, 0x81, 0x01, blueTile,
            0x23, 0x9e, 0x01, blueTile,
            0xff
        });
    }

    public List<Location> AllLocationsForReal()
    {
        List<Location> locations = westHyrule.AllLocations
            .Union(eastHyrule.AllLocations)
            .Union(mazeIsland.AllLocations)
            .Union(deathMountain.AllLocations).ToList();
        locations.Add(eastHyrule.newKasuto2);
        return locations;  
    }

    public string SpellDebug()
    {
        StringBuilder sb = new StringBuilder();
        if (!SpellGet[SpellMap[Town.RAURU]])
        {
            sb.AppendLine("Rauru: " + Enum.GetName(typeof(Spell), SpellMap[Town.RAURU]));
        }
        if (!SpellGet[SpellMap[Town.RUTO]])
        {
            sb.AppendLine("Ruto: " + Enum.GetName(typeof(Spell), SpellMap[Town.RUTO]));
        }
        if (!SpellGet[SpellMap[Town.RAURU]])
        {
            sb.AppendLine("Saria: " + Enum.GetName(typeof(Spell), SpellMap[Town.RAURU]));
        }
        if (!SpellGet[SpellMap[Town.MIDO_WEST]])
        {
            sb.AppendLine("Mido West: " + Enum.GetName(typeof(Spell), SpellMap[Town.MIDO_WEST]));
        }
        if (!SpellGet[SpellMap[Town.MIDO_CHURCH]])
        {
            sb.AppendLine("Mido Tower: " + Enum.GetName(typeof(Spell), SpellMap[Town.MIDO_CHURCH]));
        }
        if (!SpellGet[SpellMap[Town.NABOORU]])
        {
            sb.AppendLine("Nabooru: " + Enum.GetName(typeof(Spell), SpellMap[Town.NABOORU]));
        }
        if (!SpellGet[SpellMap[Town.DARUNIA_WEST]])
        {
            sb.AppendLine("Darunia West: " + Enum.GetName(typeof(Spell), SpellMap[Town.DARUNIA_WEST]));
        }
        if (!SpellGet[SpellMap[Town.DARUNIA_ROOF]])
        {
            sb.AppendLine("Darunia Roof: " + Enum.GetName(typeof(Spell), SpellMap[Town.DARUNIA_ROOF]));
        }
        if (!SpellGet[SpellMap[Town.NEW_KASUTO]])
        {
            sb.AppendLine("New Kasuto: " + Enum.GetName(typeof(Spell), SpellMap[Town.NEW_KASUTO]));
        }
        if (!SpellGet[SpellMap[Town.OLD_KASUTO]])
        {
            sb.AppendLine("Old Kasuto: " + Enum.GetName(typeof(Spell), SpellMap[Town.OLD_KASUTO]));
        }
        return sb.ToString();
    }

    public void PrintRoutingDebug(int count, int wh, int eh, int dm, int mi)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Number: " + debug);
        sb.AppendLine("Reached: " + count);
        sb.AppendLine("wh: " + wh + " / " + westHyrule.AllLocations.Count);
        sb.AppendLine("eh: " + eh + " / " + eastHyrule.AllLocations.Count);
        sb.AppendLine("dm: " + dm + " / " + deathMountain.AllLocations.Count);
        sb.AppendLine("mi: " + mi + " / " + mazeIsland.AllLocations.Count);
        sb.AppendLine("");

        //Debug.WriteLine("-" + count + "- " + accessibleMagicContainers);
        //SHUFFLABLE_STARTING_ITEMS.Where(i => itemGet[item] == false).ToList().ForEach(i => Debug.WriteLine(Enum.GetName(typeof(Item), i)));
        List<Location> allLocations = AllLocationsForReal();
        allLocations.Where(i => !i.itemGet && i.Item != Item.DO_NOT_USE).ToList()
            .ForEach(i => sb.AppendLine(i.Name + " / " + Enum.GetName(typeof(Item), i.Item)));
        sb.AppendLine(SpellDebug());
        //Debug.WriteLine("---Inaccessable Locations---");
        //allLocations.Where(i => !i.Reachable).ToList().ForEach(i => Debug.WriteLine(i.Name));


        sb.AppendLine("");
        logger.Error(sb.ToString());
    }
}
