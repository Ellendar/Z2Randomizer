using Assembler;
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
    private const int NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT = 500;

    //This controls how many times 
    private const int NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT = 10;

    public const bool UNSAFE_DEBUG = false;

    private readonly Item[] SHUFFLABLE_STARTING_ITEMS = new Item[] { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HAMMER, Item.MAGIC_KEY };

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();


    private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
    private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };


    //private ROM romData;
    private const int overworldXOff = 0x3F;
    private const int overworldMapOff = 0x7E;
    private const int overworldWorldOff = 0xBD;

    private static readonly List<Town> spellTowns = new List<Town>() { Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST,
        Town.MIDO_CHURCH, Town.NABOORU, Town.DARUNIA_WEST, Town.DARUNIA_ROOF, Town.NEW_KASUTO, Town.OLD_KASUTO};

    //Unused
    //private Dictionary<int, int> spellEnters;
    //Unused
    //private Dictionary<int, int> spellExits;
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
    private readonly BackgroundWorker worker;

    //private Character character;

    public Dictionary<Item, bool> ItemGet { get; set; }
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
    private static int DEBUG_THRESHOLD = 130;
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

    private Engine _engine;

    public Hyrule(RandomizerConfiguration config, BackgroundWorker worker, bool saveRom = true)
    {
        _engine = new Engine();

        World.ResetStats();
        RNG = new Random(config.Seed);
        props = config.Export(RNG);
        props.saveRom = saveRom;
        string export = JsonSerializer.Serialize(props);
        Debug.WriteLine(export);
        Flags = config.Serialize();
        Seed = config.Seed;
        logger.Info("Started generation for " + Flags + " / " + config.Seed);

        this.worker = worker;


        //character = new Character(props);
        shuffler = new Shuffler(props);

        palaces = new List<Palace>();
        ItemGet = new Dictionary<Item, bool>();
        foreach (Item item in Enum.GetValues(typeof(Item)))
        {
            ItemGet.Add(item, false);
        }
        accessibleMagicContainers = 4;
        SpellGet = new Dictionary<Spell, bool>();
        reachableAreas = new HashSet<string>();
        //areasByLocation = new SortedDictionary<string, List<Location>>();


        ROMData = new ROM(props.Filename);
        if (props.KasutoJars)
        {
            kasutoJars = RNG.Next(5, 8);
        }

        bool raftIsRequired = IsRaftAlwaysRequired(props);
        bool passedValidation = false;
        while (palaces == null || palaces.Count != 7 || passedValidation == false)
        {
            palaces = Palaces.CreatePalaces(worker, RNG, props, raftIsRequired);
            if(palaces == null)
            {
                continue;
            }

            //Randomize Enemies
            if (props.ShufflePalaceEnemies)
            {
                foreach (Palace palace in palaces)
                {
                    do
                    {
                        palace.RandomizeEnemies(props, RNG);
                    }
                    while (palace.AllRooms.Sum(i => i.Enemies.Length) > (palace.Number == 7 ? 0x2A9 : 0x400));
                }
            }

            if (props.ShuffleSmallItems || props.ExtraKeys)
            {
                palaces[0].ShuffleSmallItems(4, true, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[1].ShuffleSmallItems(4, true, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[2].ShuffleSmallItems(4, false, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[3].ShuffleSmallItems(4, false, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[4].ShuffleSmallItems(4, true, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[5].ShuffleSmallItems(4, false, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
                palaces[6].ShuffleSmallItems(5, true, RNG, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            }

            Assembler.Assembler sideview_module = new();
            Assembler.Assembler gp_sideview_module = new();
            //Assembler.Assembler validation_sideview_module = new();
            //Assembler.Assembler validation_gp_sideview_module = new();

            //This is an awful hack. We need to make a determination about whether the sideviews can fit in the available space,
            //but there is (at present) no way to test whether that is possible without rendering the entire engine into an irrecoverably
            //broken state, so we'll just run it twice. As long as this is the first modification that gets made on the engine, this is
            //guaranteed to succeed iff running on the original engine would succeed.
            //Jrowe feel free to engineer a less insane fix here. 
            Engine validationEngine = new Engine();

            int i = 0;
            //In Reconstructed, enemy pointers aren't separated between 125 and 346, they're just all in 1 big pile,
            //so we just start at the 125 pointer address
            int enemyAddr = Enemies.NormalPalaceEnemyAddr;
            Dictionary<byte[], List<Room>> sideviews = new(new Util.StandardByteArrayEqualityComparer());
            Dictionary<byte[], List<Room>> sideviewsgp = new(new Util.StandardByteArrayEqualityComparer());
            foreach (Room room in palaces.Where(i => i.Number < 7).SelectMany(i => i.AllRooms))
            {
                if (sideviews.ContainsKey(room.SideView))
                {
                    sideviews[room.SideView].Add(room);
                }
                else
                {
                    List<Room> l = new List<Room> { room };
                    sideviews.Add(room.SideView, l);
                }
            }
            foreach (Room room in palaces.Where(i => i.Number == 7).SelectMany(i => i.AllRooms))
            {
                if (sideviewsgp.ContainsKey(room.SideView))
                {
                    sideviewsgp[room.SideView].Add(room);
                }
                else
                {
                    List<Room> l = new List<Room> { room };
                    sideviewsgp.Add(room.SideView, l);
                }
            }

            foreach (byte[] sv in sideviews.Keys)
            {
                var name = "Sideview_" + i++;
                sideview_module.Segment("PRG4");
                sideview_module.Reloc();
                sideview_module.Label(name);
                sideview_module.Byt(sv);
                List<Room> rooms = sideviews[sv];
                foreach (Room room in rooms)
                {
                    room.WriteSideViewPtr(sideview_module, name);
                    room.UpdateItemGetBits(ROMData);
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.NewEnemies.Length;
                    room.UpdateConnectors();
                }
            }


            i = 0;
            //GP Reconstructed
            enemyAddr = Enemies.GPEnemyAddr;
            foreach (byte[] sv in sideviewsgp.Keys)
            {
                var name = "SideviewGP_" + i++;

                gp_sideview_module.Segment("PRG5", "PRG7");
                gp_sideview_module.Reloc();
                gp_sideview_module.Label(name);
                gp_sideview_module.Byt(sv);
                List<Room> rooms = sideviewsgp[sv];
                foreach (Room room in rooms)
                {
                    room.WriteSideViewPtr(gp_sideview_module, name);
                    room.UpdateItemGetBits(ROMData);
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.NewEnemies.Length;
                    room.UpdateConnectors();
                }
            }
            
            try
            {
                validationEngine.Modules.Add(sideview_module.Actions);
                validationEngine.Modules.Add(gp_sideview_module.Actions);
                ApplyAsmPatches(props, validationEngine, RNG);
                ROM testRom = new(ROMData);
                testRom.ApplyAsm(validationEngine);
            }
            catch(Exception e)
            {
                logger.Warn("Room packing failed. Retrying.", e);
                continue;
            }

            passedValidation = true;
            _engine.Modules.Add(sideview_module.Actions);
            _engine.Modules.Add(gp_sideview_module.Actions);
            ApplyAsmPatches(props, _engine, RNG);
            ROMData.ApplyAsm(_engine);
        }

        //Allows casting magic without requeueing a spell
        if (props.FastCast)
        {
            ROMData.WriteFastCastMagic();
        }

        if (props.DisableMusic)
        {
            ROMData.DisableMusic();
        }

        ROMData.WriteKasutoJarAmount(kasutoJars);
        ROMData.DoHackyFixes();
        shuffler.ShuffleDrops(ROMData, RNG);
        shuffler.ShufflePbagAmounts(ROMData, RNG);

        ROMData.ExtendMapSize();
        ROMData.DisableTurningPalacesToStone();
        ROMData.UpdateMapPointers();

        if (props.DashAlwaysOn)
        {
            ROMData.Put(0x13C3, new byte[] { 0x30, 0xD0 });
        }

        if (props.PermanentBeam)
        {
            ROMData.Put(0x186c, 0xEA);
            ROMData.Put(0x186d, 0xEA);
        }

        ShortenWizards();

        // startRandomizeStartingValuesTimestamp = DateTime.Now;
        startRandomizeEnemiesTimestamp = DateTime.Now;
        RandomizeEnemyStats();

        firstProcessOverworldTimestamp = DateTime.Now;
        ProcessOverworld();
        bool f = UpdateProgress(8);
        if (!f)
        {
            return;
        }


        List<Text> hints = ROMData.GetGameText();
        ROMData.WriteHints(CustomTexts.GenerateTexts(itemLocs, startTrophy, startMed, startKid, SpellMap, westHyrule.bagu, hints, props, RNG));
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
            "4.3.2" +
            Util.ReadAllTextFromFile(config.GetRoomsFile()) +
            finalRNGState
        ));
        UpdateRom(hash);
        char os_sep = Path.DirectorySeparatorChar;
        string newFileName = props.Filename.Substring(0, props.Filename.LastIndexOf(os_sep) + 1) + "Z2_" + Seed + "_" + Flags + ".nes";
        if (props.saveRom)
        {
            ROMData.Dump(newFileName);
        }

        Room search = palaces[6].AllRooms.FirstOrDefault(i => i.Name.Contains("Previously void elevator GP sloped inverse T room", StringComparison.OrdinalIgnoreCase));
        if (search != null)
        {
            Debug.WriteLine(newFileName);
            Debug.WriteLine(search.GetDebuggerDisplay());
        }

        if (UNSAFE_DEBUG)
        {
            PrintSpoiler(LogLevel.Error);
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


    private void RandomizeAttackEffectiveness(StatEffectiveness attackEffectiveness)
    {
        int[] attackValues = new int[8];
        for (int i = 0; i < 8; i++)
        {
            attackValues[i] = ROMData.GetByte(0x1E67D + i);
        }

        int[] newAttackValues = new int[8];
        for (int i = 0; i < 8; i++)
        {
            double minAtk = attackValues[i] - attackValues[i] * .333;
            double maxAtk = attackValues[i] + attackValues[i] * .5;

            double attack;
            if (attackEffectiveness == StatEffectiveness.AVERAGE)
            {
                attack = RNG.NextDouble() * (maxAtk - minAtk) + minAtk;
                if (i == 0)
                {
                    attack = (int)Math.Round(Math.Max(attack, 2));
                }
                else
                {
                    if (attack < newAttackValues[i - 1])
                    {
                        attack = newAttackValues[i - 1];
                    }
                    else
                    {
                        attack = (int)Math.Round(attack);
                    }
                }
                attack = (int)Math.Min(attack, maxAtk);
                attack = (int)Math.Max(attack, minAtk);
            }
            else if (attackEffectiveness == StatEffectiveness.HIGH)
            {
                attack = (int)(attackValues[i] + (attackValues[i] * .5));
            }
            else if (attackEffectiveness == StatEffectiveness.LOW)
            {
                attack = (int)(attackValues[i] - (attackValues[i] * .5));
            }
            else
            {
                throw new Exception("Invalid Attack Effectiveness");
            }

            newAttackValues[i] = (int)attack;
        }


        for (int i = 0; i < 8; i++)
        {
            ROMData.Put(0x1E67D + i, (byte)newAttackValues[i]);
        }
    }

    private void ShuffleItems()
    {
        List<Item> shufflableItems = new List<Item> { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MEDICINE, Item.TROPHY, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MAGIC_KEY, Item.MAGIC_CONTAINER, Item.HAMMER, Item.CHILD, Item.MAGIC_CONTAINER };
        List<Item> smallItems = new List<Item> { Item.BLUE_JAR, Item.RED_JAR, Item.SMALL_BAG, Item.MEDIUM_BAG, Item.LARGE_BAG, Item.XL_BAG, Item.ONEUP, Item.KEY };
        Location kidLoc = mazeIsland.childDrop;
        Location medicineLoc = westHyrule.medicineCave;
        Location trophyLoc = westHyrule.trophyCave;
        heartContainersInItemPool = maxHearts - startHearts;

        foreach (Item item in ItemGet.Keys.ToList())
        {
            ItemGet[item] = false;
        }
        foreach (Location location in itemLocs)
        {
            location.ItemGet = false;
        }
        westHyrule.pbagCave.ItemGet = false;
        eastHyrule.pbagCave1.ItemGet = false;
        eastHyrule.pbagCave2.ItemGet = false;

        ROMData.Put(RomMap.START_CANDLE, props.StartCandle ? (byte)1 : (byte)0);
        ItemGet[Item.CANDLE] = props.StartCandle;
        ROMData.Put(RomMap.START_GLOVE, props.StartGlove ? (byte)1 : (byte)0);
        ItemGet[Item.GLOVE] = props.StartGlove;
        ROMData.Put(RomMap.START_RAFT, props.StartRaft ? (byte)1 : (byte)0);
        ItemGet[Item.RAFT] = props.StartRaft;
        ROMData.Put(RomMap.START_BOOTS, props.StartBoots ? (byte)1 : (byte)0);
        ItemGet[Item.BOOTS] = props.StartBoots;
        ROMData.Put(RomMap.START_FLUTE, props.StartFlute ? (byte)1 : (byte)0);
        ItemGet[Item.FLUTE] = props.StartFlute;
        ROMData.Put(RomMap.START_CROSS, props.StartCross ? (byte)1 : (byte)0);
        ItemGet[Item.CROSS] = props.StartCross;
        ROMData.Put(RomMap.START_HAMMER, props.StartHammer ? (byte)1 : (byte)0);
        ItemGet[Item.HAMMER] = props.StartHammer;
        ROMData.Put(RomMap.START_MAGICAL_KEY, props.StartKey ? (byte)1 : (byte)0);
        ItemGet[Item.MAGIC_KEY] = props.StartKey;

        //itemList = new List<Item> { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MEDICINE, Item.TROPHY, Item.HEART_CONTAINER, Item.HEART_CONTAINER, Item.MAGIC_CONTAINER, Item.MAGIC_KEY, Item.MAGIC_CONTAINER, Item.HAMMER, Item.CHILD, Item.MAGIC_CONTAINER };

        if (props.PbagItemShuffle)
        {
            westHyrule.pbagCave.Item = (Item)ROMData.GetByte(0x4FE2);
            eastHyrule.pbagCave1.Item = (Item)ROMData.GetByte(0x8ECC);
            eastHyrule.pbagCave2.Item = (Item)ROMData.GetByte(0x8FB3);
            shufflableItems.Add(westHyrule.pbagCave.Item);
            shufflableItems.Add(eastHyrule.pbagCave1.Item);
            shufflableItems.Add(eastHyrule.pbagCave2.Item);

        }
        pbagHearts = new List<Location>();
        //Replace any unused heart containers with small items
        if (heartContainersInItemPool < 4)
        {
            int heartContainersToAdd = 4 - heartContainersInItemPool;
            while (heartContainersToAdd > 0)
            {
                int remove = RNG.Next(shufflableItems.Count);
                if (shufflableItems[remove] == Item.HEART_CONTAINER)
                {
                    shufflableItems[remove] = smallItems[RNG.Next(smallItems.Count)];
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
                    shufflableItems[22 - heartContainersToAdd] = Item.HEART_CONTAINER;
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
                        shufflableItems.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(westHyrule.pbagCave);
                        x--;
                    }
                    if (y == 1 && !pbagHearts.Contains(eastHyrule.pbagCave1))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave1);
                        eastHyrule.pbagCave1.Item = Item.HEART_CONTAINER;
                        shufflableItems.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(eastHyrule.pbagCave1);
                        x--;
                    }
                    if (y == 2 && !pbagHearts.Contains(eastHyrule.pbagCave2))
                    {
                        pbagHearts.Add(eastHyrule.pbagCave2);
                        eastHyrule.pbagCave2.Item = Item.HEART_CONTAINER;
                        shufflableItems.Add(Item.HEART_CONTAINER);
                        itemLocs.Add(eastHyrule.pbagCave2);
                        x--;
                    }
                }
            }
        }

        if (props.RemoveSpellItems)
        {
            shufflableItems[9] = smallItems[RNG.Next(smallItems.Count)];
            shufflableItems[10] = smallItems[RNG.Next(smallItems.Count)];
            shufflableItems[17] = smallItems[RNG.Next(smallItems.Count)];
            ItemGet[Item.TROPHY] = true;
            ItemGet[Item.MEDICINE] = true;
            ItemGet[Item.CHILD] = true;

        }

        if (SpellGet[SpellMap[Town.MIDO_WEST]])
        {
            shufflableItems[9] = smallItems[RNG.Next(smallItems.Count)];
            ItemGet[Item.MEDICINE] = true;
            startMed = true;
        }

        if (SpellGet[SpellMap[Town.RUTO]])
        {
            shufflableItems[10] = smallItems[RNG.Next(smallItems.Count)];
            ItemGet[Item.TROPHY] = true;
            startTrophy = true;
        }

        if (SpellGet[SpellMap[Town.DARUNIA_WEST]])
        {
            shufflableItems[17] = smallItems[RNG.Next(smallItems.Count)];
            ItemGet[Item.CHILD] = true;
            startKid = true;
        }

        //TODO: Clean up the readability of this logic
        if (ItemGet[Item.CANDLE])
        {
            shufflableItems[0] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.GLOVE])
        {
            shufflableItems[1] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.RAFT])
        {
            shufflableItems[2] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.BOOTS])
        {
            shufflableItems[3] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.FLUTE])
        {
            shufflableItems[4] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.CROSS])
        {
            shufflableItems[5] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.MAGIC_KEY])
        {
            shufflableItems[14] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (ItemGet[Item.HAMMER])
        {
            shufflableItems[16] = smallItems[RNG.Next(smallItems.Count)];
        }

        if (props.MixOverworldPalaceItems)
        {
            /*
            for (int i = itemList.Count - 1; i > 0; i--)
            {
                int s = RNG.Next(i, itemList.Count);
                (itemList[i], itemList[s]) = (itemList[s], itemList[i]);
            }*/
            shufflableItems.FisherYatesShuffle(RNG);
        }
        else
        {
            if (props.ShufflePalaceItems)
            {
                for (int i = 5; i > 0; i--)
                {
                    int s = RNG.Next(i + 1);
                    (shufflableItems[i], shufflableItems[s]) = (shufflableItems[s], shufflableItems[i]);
                }
            }

            if (props.ShuffleOverworldItems)
            {
                for (int i = shufflableItems.Count - 1; i > 6; i--)
                {
                    int s = RNG.Next(6, i + 1);
                    (shufflableItems[i], shufflableItems[s]) = (shufflableItems[s], shufflableItems[i]);
                }
            }
        }
        for (int i = 0; i < shufflableItems.Count; i++)
        {
            itemLocs[i].Item = shufflableItems[i];
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
            byte heartByte = ROMData.GetByte(0x27810 + i);
            ROMData.Put(0x29810 + i, heartByte);
            ROMData.Put(0x2B810 + i, heartByte);
            ROMData.Put(0x2D810 + i, heartByte);
            ROMData.Put(0x33810 + i, heartByte);
            ROMData.Put(0x35810 + i, heartByte);
            ROMData.Put(0x37810 + i, heartByte);
            ROMData.Put(0x39810 + i, heartByte);
        }


    }

    private bool IsEverythingReachable(Dictionary<Item, bool> ItemGet, Dictionary<Spell, bool> spellGet)
    {
        totalReachableCheck++;
        int dm = 0;
        int mi = 0;
        int wh = 0;
        int eh = 0;
        int count = 1;
        int prevCount = 0;
        int loopCount = 0;
        debug++;
        Dictionary<Spell, Location> spellLocations = GetSpellLocations();

        int totalLocationsCount = westHyrule.AllLocations.Count + eastHyrule.AllLocations.Count + deathMountain.AllLocations.Count + mazeIsland.AllLocations.Count;
        //logger.Debug("Locations count: West-" + westHyrule.AllLocations.Count + " East-" + eastHyrule.AllLocations.Count +
        //   " DM-" + deathMountain.AllLocations.Count + " MI-" + mazeIsland.AllLocations.Count + " Total-" + totalLocationsCount);
        bool updateItemsResult = false;
        bool updateSpellsResult = false;
        while (prevCount != count || updateItemsResult || updateSpellsResult)
        {
            prevCount = count;
            westHyrule.UpdateVisit(ItemGet, spellGet);
            deathMountain.UpdateVisit(ItemGet, spellGet);
            eastHyrule.UpdateVisit(ItemGet, spellGet);
            mazeIsland.UpdateVisit(ItemGet, spellGet);

            foreach (World world in worlds)
            {
                if (world.raft != null && CanGet(world.raft) && ItemGet[Item.RAFT])
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
            updateSpellsResult = UpdateSpells(spellLocations);

            //This 2nd pass is weird and may not need to exist, eventually I should run some stats on whether it helps or not
            westHyrule.UpdateVisit(ItemGet, spellGet);
            deathMountain.UpdateVisit(ItemGet, spellGet);
            eastHyrule.UpdateVisit(ItemGet, spellGet);
            mazeIsland.UpdateVisit(ItemGet, spellGet);

            updateItemsResult |= UpdateItemGets();
            updateSpellsResult |= UpdateSpells(spellLocations);



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

        foreach (Item item in SHUFFLABLE_STARTING_ITEMS)
        {
            if (ItemGet[item] == false)
            {
                if (UNSAFE_DEBUG && count >= DEBUG_THRESHOLD)
                {
                    Debug.WriteLine("Failed on critical item");
                    PrintRoutingDebug(count, wh, eh, dm, mi);

                    return false;
                }
                itemGetReachableFailures++;
                return false;
            }
        }

        for (int i = 19; i < 22; i++)
        {
            if (ItemGet[(Item)i] == false)
            {
                itemGetReachableFailures++;
                if (UNSAFE_DEBUG && count > DEBUG_THRESHOLD)
                {
                    Debug.WriteLine("Failed on items");
                    PrintRoutingDebug(count, wh, eh, dm, mi);
                    return false;
                }
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
        if (SpellGet.Values.Any(i => i == false))
        {
            spellGetReachableFailures++;
            if (UNSAFE_DEBUG && count > DEBUG_THRESHOLD)
            {
                Debug.WriteLine("Failed on spells");
                debug++;
                PrintRoutingDebug(count, wh, eh, dm, mi);
                return false;
            }
            return false;
        }

        bool retval =
            CanGet(westHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(eastHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(deathMountain.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(mazeIsland.RequiredLocations(props.HiddenPalace, props.HiddenKasuto));
            /*(CanGet(westHyrule.Locations[Terrain.TOWN])
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
            */
        if (retval == false)
        {
            logicallyRequiredLocationsReachableFailures++;
            if(UNSAFE_DEBUG)
            {
                List<Location> missingLocations = new();
                missingLocations.AddRange(westHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable));
                missingLocations.AddRange(eastHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable));
                missingLocations.AddRange(mazeIsland.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable));

                List<Location> nonDmLocations = new(missingLocations);
                missingLocations.AddRange(deathMountain.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable));

                Debug.WriteLine("Unreachable locations:");
                Debug.WriteLine(string.Join("\n", missingLocations.Select(i => i.GetDebuggerDisplay())));

                if(nonDmLocations.Count == 0 && UNSAFE_DEBUG)
                {
                    return false;
                }
                return false;
            }
        }
        if (UNSAFE_DEBUG && retval)
        {
            PrintRoutingDebug(count, wh, eh, dm, mi);
        }
        return retval;
    }

    private bool CanGet(IEnumerable<Location> locations)
    {
        return locations.All(i => i.Reachable);
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

    private bool UpdateSpells(Dictionary<Spell, Location> spellLocations)
    {
        bool changed = false;
        List<RequirementType> requireables = GetRequireables();

        foreach (Spell spell in spellLocations.Keys)
        {
            Town town = spellLocations[spell].ActualTown;
            if (spellLocations[spell].Reachable && Towns.townSpellAndItemRequirements[town].AreSatisfiedBy(requireables))
            {
                if (!SpellGet[spell])
                {
                    SpellGet[spell] = true;
                    changed = true;
                }
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
        accessibleMagicContainers = 4 + AllLocationsForReal().Where(i => i.ItemGet == true && i.Item == Item.MAGIC_CONTAINER).Count();
        heartContainers = startHearts;
        bool changed = false;

        foreach (Location location in AllLocationsForReal().Where(i => i.Item != Item.DO_NOT_USE))
        {
            bool hadItemPreviously = location.ItemGet;
            bool hasItemNow;
            if (location.PalaceNumber > 0 && location.PalaceNumber < 7)
            {
                Palace palace = palaces[location.PalaceNumber - 1];
                hasItemNow = CanGet(location)
                    && (SpellGet[Spell.FAIRY] || ItemGet[Item.MAGIC_KEY])
                    && palace.CanGetItem(GetRequireables());
                /*
                && (!palace.NeedDstab || (palace.NeedDstab && SpellGet[Spell.DOWNSTAB])) 
                && (!palace.NeedFairy || (palace.NeedFairy && SpellGet[Spell.FAIRY])) 
                && (!palace.NeedGlove || (palace.NeedGlove && ItemGet[Item.GLOVE])) 
                && (!palace.NeedJumpOrFairy || (palace.NeedJumpOrFairy && (SpellGet[Spell.JUMP]) || SpellGet[Spell.FAIRY])) 
                && (!palace.NeedReflect || (palace.NeedReflect && SpellGet[Spell.REFLECT]));
                */
            }
            else if (location.ActualTown == Town.NEW_KASUTO)
            {
                hasItemNow = CanGet(location) && (accessibleMagicContainers >= kasutoJars) && (!location.NeedHammer || ItemGet[Item.HAMMER]);
            }
            else if (location.ActualTown == Town.SPELL_TOWER)
            {
                hasItemNow = (CanGet(location) && SpellGet[Spell.SPELL]) && (!location.NeedHammer || ItemGet[Item.HAMMER]);
            }
            else
            {
                hasItemNow = CanGet(location) && (!location.NeedHammer || ItemGet[Item.HAMMER]) && (!location.NeedRecorder || ItemGet[Item.FLUTE]);
            }

            //Issue #3: Previously running UpdateItemGets multiple times could produce different results based on the sequence of times it ran
            //For items that were blocked by MC requirements, different orders of parsing the same world could check the MCs at different times
            //producing different results. Now it's not possible to "go back" in logic and call previously accessed items inaccesable.
            location.ItemGet = hasItemNow || hadItemPreviously;
            ItemGet[location.Item] = hasItemNow || hadItemPreviously;

            if (location.ItemGet && location.Item == Item.HEART_CONTAINER)
            {
                heartContainers++;
            }
            if (!hadItemPreviously && location.ItemGet)
            {
                changed = true;
            }
        }

        if (!props.PbagItemShuffle)
        {
            if (westHyrule.pbagCave.Reachable)
            {
                westHyrule.pbagCave.ItemGet = true;
            }
            if (eastHyrule.pbagCave1.Reachable)
            {
                eastHyrule.pbagCave1.ItemGet = true;
            }
            if (eastHyrule.pbagCave2.Reachable)
            {
                eastHyrule.pbagCave2.ItemGet = true;
            }
        }

        accessibleMagicContainers = 4 + AllLocationsForReal().Where(i => i.ItemGet == true && i.Item == Item.MAGIC_CONTAINER).Count();
        return changed;
    }
    private void RandomizeLifeOrMagicEffectiveness(bool isMag)
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

        StatEffectiveness currentStatEffectiveness = isMag ? props.MagicEffectiveness : props.LifeEffectiveness;

        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < numBanks; i++)
            {
                int nextVal = life[i, j];
                if (currentStatEffectiveness == StatEffectiveness.AVERAGE)
                {
                    int max = (int)(life[i, j] + life[i, j] * .5);
                    int min = (int)(life[i, j] - life[i, j] * .5);
                    if (!isMag)
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
            RandomizeHP(0x5434, 0x5453);
            RandomizeHP(0x9434, 0x944E);
            RandomizeHP(0x11435, 0x11435);
            RandomizeHP(0x11437, 0x11454);
            RandomizeHP(0x13C86, 0x13C87);
            RandomizeHP(0x15434, 0x15438);
            RandomizeHP(0x15440, 0x15443);
            RandomizeHP(0x15445, 0x1544B);
            RandomizeHP(0x1544E, 0x1544E);
            RandomizeHP(0x12935, 0x12935);
            RandomizeHP(0x12937, 0x12954);
        }

        if (props.AttackEffectiveness == StatEffectiveness.MAX)
        {
            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(0x1E67D + i, (byte)192);
            }
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

    private void RandomizeHP(int start, int end)
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
        && (!palace.NeedGlove || (palace.NeedGlove && ItemGet[Item.GLOVE]))
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
        if (ItemGet[Item.GLOVE])
        {
            requireables.Add(RequirementType.GLOVE);
        }
        if (ItemGet[Item.MAGIC_KEY])
        {
            requireables.Add(RequirementType.KEY);
        }
        if (accessibleMagicContainers >= 5 || props.DisableMagicRecs)
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
        if (ItemGet[Item.TROPHY] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.TROPHY);
        }
        if (ItemGet[Item.MEDICINE] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.MEDICINE);
        }
        if (ItemGet[Item.CHILD] || props.RemoveSpellItems)
        {
            requireables.Add(RequirementType.CHILD);
        }
        return requireables;
    }

    private void ProcessOverworld()
    {
        if (props.ShuffleSmallItems)
        {
            RandomizeSmallItems(1, true);
            RandomizeSmallItems(1, false);
            RandomizeSmallItems(2, true);
            RandomizeSmallItems(2, false);
            RandomizeSmallItems(3, true);
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
                westHyrule = new WestHyrule(props, RNG, ROMData);
                deathMountain = new DeathMountain(props, RNG, ROMData);
                eastHyrule = new EastHyrule(props, RNG, ROMData);
                mazeIsland = new MazeIsland(props, RNG, ROMData);
                worlds.Add(westHyrule);
                worlds.Add(deathMountain);
                worlds.Add(eastHyrule);
                worlds.Add(mazeIsland);
                ResetTowns();

                if (props.ContinentConnections == ContinentConnectionType.NORMAL || props.ContinentConnections == ContinentConnectionType.RB_BORDER_SHUFFLE)
                {
                    westHyrule.LoadCave1(ROMData, 1);
                    westHyrule.LoadCave2(ROMData, 1);
                    westHyrule.LoadRaft(ROMData, 2);

                    deathMountain.LoadCave1(ROMData, 0);
                    deathMountain.LoadCave2(ROMData, 0);

                    eastHyrule.LoadRaft(ROMData, 0);
                    eastHyrule.LoadBridge(ROMData, 3);

                    mazeIsland.LoadBridge(ROMData, 2);
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

                    l1 = worlds[raftw1].LoadRaft(ROMData, raftw2);
                    l2 = worlds[raftw2].LoadRaft(ROMData, raftw1);
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

                    l1 = worlds[bridgew1].LoadBridge(ROMData, bridgew2);
                    l2 = worlds[bridgew2].LoadBridge(ROMData, bridgew1);
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

                    l1 = worlds[c1w1].LoadCave1(ROMData, c1w2);
                    l2 = worlds[c1w2].LoadCave1(ROMData, c1w1);
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

                    l1 = worlds[c2w1].LoadCave2(ROMData, c2w2);
                    l2 = worlds[c2w2].LoadCave2(ROMData, c2w1);
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
                    while (!westHyrule.Terraform(props, ROMData)) { totalWestGenerationAttempts++; }
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
                    while (!deathMountain.Terraform(props, ROMData)) { totalDeathMountainGenerationAttempts++; }
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
                    while (!eastHyrule.Terraform(props, ROMData)) { totalEastGenerationAttempts++; }
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
                    while (!mazeIsland.Terraform(props, ROMData)) { totalMazeIslandGenerationAttempts++; }
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

                    eastHyrule.spellTower.Reachable = false;
                    //eastHyrule.bridge.Reachable = false;
                    startMed = false;
                    startTrophy = false;
                    startKid = false;
                    westHyrule.ResetVisitabilityState();
                    eastHyrule.ResetVisitabilityState();
                    mazeIsland.ResetVisitabilityState();
                    deathMountain.ResetVisitabilityState();

                    ShuffleSpells();
                    LoadItemLocs();
                    westHyrule.SetStart();

                    ShufflePalaces();
                    LoadItemLocs();
                    ShuffleItems();


                    westHyrule.UpdateAllReached();
                    eastHyrule.UpdateAllReached();
                    mazeIsland.UpdateAllReached();
                    deathMountain.UpdateAllReached();

                    nonTerrainShuffleAttempt++;
                } while (nonTerrainShuffleAttempt < NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT && !IsEverythingReachable(ItemGet, SpellGet));

                if (nonTerrainShuffleAttempt != NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT)
                {
                    break;
                }
            } while (nonContinentGenerationAttempts < NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT);
        } while (!IsEverythingReachable(ItemGet, SpellGet));

        if (props.ShuffleOverworldEnemies)
        {
            foreach (World world in worlds)
            {
                world.ShuffleOverworldEnemies(ROMData, props.GeneratorsAlwaysMatch, props.MixLargeAndSmallEnemies);
            }
        }
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
        if (type == 1)
        {
            l1 = worlds[w1].LoadRaft(ROMData, w2);
            l2 = worlds[w2].LoadRaft(ROMData, w1);
            connections[0] = (l1, l2);
        }
        else if (type == 2)
        {
            l1 = worlds[w1].LoadBridge(ROMData, w2);
            l2 = worlds[w2].LoadBridge(ROMData, w1);
            connections[1] = (l1, l2);
        }
        else if (type == 3)
        {
            l1 = worlds[w1].LoadCave1(ROMData, w2);
            l2 = worlds[w2].LoadCave1(ROMData, w1);
            connections[2] = (l1, l2);
        }
        else
        {
            l1 = worlds[w1].LoadCave2(ROMData, w2);
            l2 = worlds[w2].LoadCave2(ROMData, w1);
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

    private void ResetTowns()
    {
        westHyrule.locationAtRauru.ActualTown = Town.RAURU;
        westHyrule.locationAtRuto.ActualTown = Town.RUTO;
        westHyrule.locationAtSariaNorth.ActualTown = Town.SARIA_NORTH;
        westHyrule.locationAtSariaSouth.ActualTown = Town.SARIA_SOUTH;
        westHyrule.locationAtMido.ActualTown = Town.MIDO_WEST;
        eastHyrule.townAtNabooru.ActualTown = Town.NABOORU;
        eastHyrule.townAtDarunia.ActualTown = Town.DARUNIA_WEST;
        eastHyrule.townAtNewKasuto.ActualTown = Town.NEW_KASUTO;
        eastHyrule.spellTower.ActualTown = Town.SPELL_TOWER;
        eastHyrule.townAtOldKasuto.ActualTown = Town.OLD_KASUTO;
    }

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
        }

    }

    private List<Location> LoadItemLocs()
    {
        itemLocs = new List<Location>();
        if (westHyrule.locationAtPalace1.PalaceNumber != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace1);
        }
        if (westHyrule.locationAtPalace2.PalaceNumber != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace2);
        }
        if (westHyrule.locationAtPalace3.PalaceNumber != 7)
        {
            itemLocs.Add(westHyrule.locationAtPalace3);
        }
        if (mazeIsland.locationAtPalace4.PalaceNumber != 7)
        {
            itemLocs.Add(mazeIsland.locationAtPalace4);
        }
        if (eastHyrule.locationAtPalace5.PalaceNumber != 7)
        {
            itemLocs.Add(eastHyrule.locationAtPalace5);
        }
        if (eastHyrule.locationAtPalace6.PalaceNumber != 7)
        {
            itemLocs.Add(eastHyrule.locationAtPalace6);
        }
        if (eastHyrule.locationAtGP.PalaceNumber != 7)
        {
            itemLocs.Add(eastHyrule.locationAtGP);
        }
        itemLocs.Add(westHyrule.grassTile);
        itemLocs.Add(westHyrule.heartContainerCave);
        itemLocs.Add(westHyrule.magicContainerCave);
        itemLocs.Add(westHyrule.medicineCave);
        itemLocs.Add(westHyrule.trophyCave);
        itemLocs.Add(eastHyrule.waterTile);
        itemLocs.Add(eastHyrule.desertTile);
        itemLocs.Add(eastHyrule.townAtNewKasuto);
        itemLocs.Add(eastHyrule.spellTower);
        itemLocs.Add(deathMountain.specRock);
        itemLocs.Add(deathMountain.hammerCave);
        itemLocs.Add(mazeIsland.childDrop);
        itemLocs.Add(mazeIsland.magicContainerDrop);


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
            if (props.ReplaceFireWithDash && spell == Spell.FIRE
                || !props.ReplaceFireWithDash && spell == Spell.DASH
                || spell == Spell.UPSTAB
                || spell == Spell.DOWNSTAB)
            {
                continue;
            }
            Town town = unallocatedTowns[RNG.Next(unallocatedTowns.Count)];
            unallocatedTowns.Remove(town);
            if (props.ShuffleSpellLocations)
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
                    Town.NABOORU => props.ReplaceFireWithDash ? Spell.DASH : Spell.FIRE,
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

        int i = 0;
        foreach (Town town in Towns.STRICT_SPELL_LOCATIONS)
        {
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + i++, props.StartWithSpell(SpellMap[town]) ? (byte)1 : (byte)0);
            SpellGet[SpellMap[town]] = props.StartWithSpell(SpellMap[town]);
        }
        /*
        ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + SpellMap.Values.ToList().IndexOf(Spell.SHIELD), props.StartShield ? (byte)1 : (byte)0);
        SpellGet[Spell.SHIELD] = props.StartShield;
        */

        if (props.CombineFire)
        {
            int newFire = RNG.Next(7);
            if (newFire > 3)
            {
                newFire++;
            }
            byte newnewFire = (byte)(0x10 | ROMData.GetByte(0xDCB + newFire));
            ROMData.Put(0xDCF, newnewFire);
        }
    }

    private void RandomizeExperience(int start, int cap)
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

    /// <summary>
    /// For a given set of addresses, set a masked portion of the value of each address on or off at a rate
    /// equal to the proportion of values at the addresses that have that masked portion set to a nonzero value.
    /// In effect, turn some values in a range on or off randomly in the proportion of the number of such values that are on in vanilla.
    /// </summary>
    /// <param name="addr">Addresses to randomize.</param>
    /// <param name="mask">What part of the byte value at each address contains the configuration we care about.</param>
    /// <exception cref="ArgumentException">Iff there are 0 addresses in the space to shuffle, as this would cause a divide by zero
    /// while determining the proportion.</exception>
    private void RandomizeBits(List<int> addr, int mask = 0b00010000)
    {
        if(addr.Count == 0)
        {
            throw new ArgumentException("Cannot shuffle 0 bits");
        }
        int notMask = mask ^ 0xFF;

        double count = 0;
        foreach (int i in addr)
        {
            if ((ROMData.GetByte(i) & mask) > 0)
            {
                count++;
            }
        }

        //proportion of the bytes that have nonzero values in the masked portion
        double fraction = count / addr.Count;

        foreach (int i in addr)
        {
            int part1 = 0;
            int part2 = ROMData.GetByte(i) & notMask;
            if (RNG.NextDouble() <= fraction)
            {
                part1 = mask;
            }
            ROMData.Put(i, (byte)(part1 + part2));
        }
    }

    private void RandomizeEnemyExp(List<int> addr)
    {
        foreach (int i in addr)
        {
            byte exp = ROMData.GetByte(i);
            int high = exp & 0xF0;
            int low = exp & 0x0F;

            if (props.ExpLevel == StatEffectiveness.HIGH)
            {
                low++;
            }
            else if (props.ExpLevel == StatEffectiveness.LOW)
            {
                low--;
            }
            else if (props.ExpLevel == StatEffectiveness.NONE)
            {
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

            byte temp = ROMData.GetByte(addr[i]);
            ROMData.Put(addr[i], ROMData.GetByte(addr[swap]));
            ROMData.Put(addr[swap], temp);
        }
    }
    private void RandomizeStartingValues(Engine engine)
    {

        ROMData.Put(0x17AF3, (byte)props.StartAtk);
        ROMData.Put(0x17AF4, (byte)props.StartMag);
        ROMData.Put(0x17AF5, (byte)props.StartLifeLvl);

        if (props.RemoveFlashing)
        {
            ROMData.DisableFlashing();
        }

        if (props.SpellEnemy)
        {
            //3, 4, 6, 7, 14, 16, 17, 18, 24, 25, 26
            List<int> enemies = new List<int> { 3, 4, 6, 7, 0x0E, 0x10, 0x11, 0x12, 0x18, 0x19, 0x1A };
            ROMData.Put(0x11ef, (byte)enemies[RNG.Next(enemies.Count())]);
        }
        if (props.BossItem)
        {
            shuffler.ShuffleBossDrop(ROMData, RNG, engine);
        }

        if (props.RemoveSpellItems)
        {
            ROMData.Put(0xF584, 0xA9);
            ROMData.Put(0xF585, 0x01);
            ROMData.Put(0xF586, 0xEA);
        }
        ROMData.UpdateSprites(props.CharSprite, props.TunicColor, props.ShieldColor, props.BeamSprite);

        if (props.EncounterRate == EncounterRate.NONE)
        {
            ROMData.Put(0x294, 0x60); //skips the whole routine
        }

        if (props.EncounterRate == EncounterRate.HALF)
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

        //CMP      #$20                      ; 0x1d4e4 $D4D4 C9 20
        ROMData.Put(0x1d4e5, props.BeepThreshold);
        if (props.BeepFrequency == 0)
        {
            //C9 20 - EA 38
            //CMP 20 -> NOP SEC
            ROMData.Put(0x1D4E4, (byte)0xEA);
            ROMData.Put(0x1D4E5, (byte)0x38);
        }
        else
        {
            //LDA      #$30                      ; 0x193c1 $93B1 A9 30
            ROMData.Put(0x193c2, props.BeepFrequency);
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
            RandomizeBits(addr);
        }

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(addr, 0x20);
        }

        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(addr);
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
            RandomizeBits(addr);
        }

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(addr, 0x20);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(addr);
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
            RandomizeBits(addr);
        }

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(addr, 0x20);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(addr);
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
            RandomizeBits(addr);
        }

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(addr, 0x20);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(addr);
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
            RandomizeBits(addr);
        }

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(addr, 0x20);
        }
        if (props.ExpLevel != StatEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(addr);
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
            RandomizeEnemyExp(addr);
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
            RandomizeExperience(0x1669, props.AttackCap);
        }

        if (props.ShuffleMagicExp)
        {
            RandomizeExperience(0x1671, props.MagicCap);
        }

        if (props.ShuffleLifeExp)
        {
            RandomizeExperience(0x1679, props.LifeCap);
        }

        ROMData.SetLevelCap(props.AttackCap, props.MagicCap, props.LifeCap);

        RandomizeAttackEffectiveness(props.AttackEffectiveness);

        RandomizeLifeOrMagicEffectiveness(true);

        RandomizeLifeOrMagicEffectiveness(false);

        ROMData.Put(0x17B10, (byte)props.StartGems);


        startHearts = props.StartHearts;
        ROMData.Put(0x17B00, (byte)startHearts);


        maxHearts = props.MaxHearts;

        heartContainersInItemPool = maxHearts - startHearts;


        ROMData.Put(0x1C369, (byte)props.StartLives);

        ROMData.Put(0x17B12, (byte)((props.StartWithUpstab ? 0x04 : 0) + (props.StartWithDownstab ? 0x10 : 0)));

        //Swap up and Downstab
        if (props.SwapUpAndDownStab)
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

    private byte IntToText(int x)
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
            ROMData.RemoveUnusedConnectors(world);
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

        byte[] medSprite = new byte[32];
        byte[] trophySprite = new byte[32];
        byte[] kidSprite = new byte[32];

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

        if (eastHyrule.townAtNewKasuto.Item == Item.TROPHY || eastHyrule.spellTower.Item == Item.TROPHY || westHyrule.locationAtSariaNorth.Item == Item.TROPHY || westHyrule.locationAtSariaSouth.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27210 + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0x21);
            ROMData.Put(0x1eeb8, 0x21);
        }

        if (eastHyrule.townAtNewKasuto.Item == Item.MEDICINE || eastHyrule.spellTower.Item == Item.MEDICINE || westHyrule.locationAtSariaNorth.Item == Item.TROPHY || westHyrule.locationAtSariaSouth.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(0x27230 + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0x23);
            ROMData.Put(0x1eeba, 0x23);
        }

        if (eastHyrule.townAtNewKasuto.Item == Item.CHILD || eastHyrule.spellTower.Item == Item.CHILD || westHyrule.locationAtSariaNorth.Item == Item.TROPHY || westHyrule.locationAtSariaSouth.Item == Item.TROPHY)
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
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.TROPHY)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalaceNumber] + i, trophySprite[i]);
            }
            ROMData.Put(0x1eeb7, 0xAD);
            ROMData.Put(0x1eeb8, 0xAD);
        }

        if (westHyrule.locationAtPalace1.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.MEDICINE)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalaceNumber] + i, medSprite[i]);
            }
            ROMData.Put(0x1eeb9, 0xAD);
            ROMData.Put(0x1eeba, 0xAD);
        }

        if (westHyrule.locationAtPalace1.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace1.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.locationAtPalace2.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace2.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (westHyrule.locationAtPalace3.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[westHyrule.locationAtPalace3.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (mazeIsland.locationAtPalace4.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[mazeIsland.locationAtPalace4.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtPalace5.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace5.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtPalace6.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtPalace6.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }
        if (eastHyrule.locationAtGP.Item == Item.CHILD)
        {
            for (int i = 0; i < 32; i++)
            {
                ROMData.Put(palaceMems[eastHyrule.locationAtGP.PalaceNumber] + i, kidSprite[i]);
            }
            ROMData.Put(0x1eeb5, 0xAD);
            ROMData.Put(0x1eeb6, 0xAD);
        }

        foreach (Palace palace in palaces)
        {
            palace.ValidateRoomConnections();
            palace.UpdateRom(ROMData);
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


        ROMData.Put(0x1CD3A, (byte)palGraphics[westHyrule.locationAtPalace1.PalaceNumber]);


        ROMData.Put(0x1CD3B, (byte)palGraphics[westHyrule.locationAtPalace2.PalaceNumber]);


        ROMData.Put(0x1CD3C, (byte)palGraphics[westHyrule.locationAtPalace3.PalaceNumber]);


        ROMData.Put(0x1CD46, (byte)palGraphics[mazeIsland.locationAtPalace4.PalaceNumber]);


        ROMData.Put(0x1CD42, (byte)palGraphics[eastHyrule.locationAtPalace5.PalaceNumber]);

        ROMData.Put(0x1CD43, (byte)palGraphics[eastHyrule.locationAtPalace6.PalaceNumber]);
        ROMData.Put(0x1CD44, (byte)palGraphics[eastHyrule.locationAtGP.PalaceNumber]);

        //if (!props.palacePalette)
        //{

        ROMData.Put(0x1FFF4, (byte)palPalettes[westHyrule.locationAtPalace1.PalaceNumber]);

        ROMData.Put(0x1FFF5, (byte)palPalettes[westHyrule.locationAtPalace2.PalaceNumber]);

        ROMData.Put(0x1FFF6, (byte)palPalettes[westHyrule.locationAtPalace3.PalaceNumber]);

        ROMData.Put(0x20000, (byte)palPalettes[mazeIsland.locationAtPalace4.PalaceNumber]);

        ROMData.Put(0x1FFFC, (byte)palPalettes[eastHyrule.locationAtPalace5.PalaceNumber]);

        ROMData.Put(0x1FFFD, (byte)palPalettes[eastHyrule.locationAtPalace6.PalaceNumber]);

        ROMData.Put(0x1FFFE, (byte)palPalettes[eastHyrule.locationAtGP.PalaceNumber]);

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
        ROMData.Put(0x502A, (byte)westHyrule.magicContainerCave.Item);
        ROMData.Put(0x4DD7, (byte)westHyrule.heartContainerCave.Item);

        int[] itemLocs2 = { 0x10E91, 0x10E9A, 0x1252D, 0x12538, 0x10EA3, 0x12774 };


        ROMData.Put(0x5069, (byte)westHyrule.medicineCave.Item);
        ROMData.Put(0x4ff5, (byte)westHyrule.grassTile.Item);

        ROMData.Put(0x65C3, (byte)deathMountain.specRock.Item);
        ROMData.Put(0x6512, (byte)deathMountain.hammerCave.Item);
        ROMData.Put(0x8FAA, (byte)eastHyrule.waterTile.Item);
        ROMData.Put(0x9011, (byte)eastHyrule.desertTile.Item);
        //if (props.NormalPalaceStyle == PalaceStyle.RECONSTRUCTED)
        //{
        ROMData.ElevatorBossFix(props.BossItem);
        if (westHyrule.locationAtPalace1.PalaceNumber != 7)
        {
            palaces[westHyrule.locationAtPalace1.PalaceNumber - 1].UpdateItem(westHyrule.locationAtPalace1.Item, ROMData);
        }
        if (westHyrule.locationAtPalace2.PalaceNumber != 7)
        {
            palaces[westHyrule.locationAtPalace2.PalaceNumber - 1].UpdateItem(westHyrule.locationAtPalace2.Item, ROMData);
        }
        if (westHyrule.locationAtPalace3.PalaceNumber != 7)
        {
            palaces[westHyrule.locationAtPalace3.PalaceNumber - 1].UpdateItem(westHyrule.locationAtPalace3.Item, ROMData);
        }
        if (eastHyrule.locationAtPalace5.PalaceNumber != 7)
        {
            palaces[eastHyrule.locationAtPalace5.PalaceNumber - 1].UpdateItem(eastHyrule.locationAtPalace5.Item, ROMData);
        }
        if (eastHyrule.locationAtPalace6.PalaceNumber != 7)
        {
            palaces[eastHyrule.locationAtPalace6.PalaceNumber - 1].UpdateItem(eastHyrule.locationAtPalace6.Item, ROMData);
        }
        if (mazeIsland.locationAtPalace4.PalaceNumber != 7)
        {
            palaces[mazeIsland.locationAtPalace4.PalaceNumber - 1].UpdateItem(mazeIsland.locationAtPalace4.Item, ROMData);
        }


        if (eastHyrule.locationAtGP.PalaceNumber != 7)
        {
            palaces[eastHyrule.locationAtGP.PalaceNumber - 1].UpdateItem(eastHyrule.locationAtGP.Item, ROMData);
        }

        Room root = palaces[westHyrule.locationAtPalace1.PalaceNumber - 1].Root;
        ROMData.Put(westHyrule.locationAtPalace1.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[westHyrule.locationAtPalace2.PalaceNumber - 1].Root;
        ROMData.Put(westHyrule.locationAtPalace2.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[westHyrule.locationAtPalace3.PalaceNumber - 1].Root;
        ROMData.Put(westHyrule.locationAtPalace3.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[eastHyrule.locationAtPalace5.PalaceNumber - 1].Root;
        ROMData.Put(eastHyrule.locationAtPalace5.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[eastHyrule.locationAtPalace6.PalaceNumber - 1].Root;
        ROMData.Put(eastHyrule.locationAtPalace6.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[eastHyrule.locationAtGP.PalaceNumber - 1].Root;
        ROMData.Put(eastHyrule.locationAtGP.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));
        root = palaces[mazeIsland.locationAtPalace4.PalaceNumber - 1].Root;
        ROMData.Put(mazeIsland.locationAtPalace4.MemAddress + 0x7e, (byte)(root.NewMap == null ? root.Map : root.NewMap));

        ROMData.Put(0xDB95, (byte)eastHyrule.spellTower.Item); //map 47
        ROMData.Put(0xDB8C, (byte)eastHyrule.townAtNewKasuto.Item); //map 46

        ROMData.Put(0xA5A8, (byte)mazeIsland.magicContainerDrop.Item);
        ROMData.Put(0xA58B, (byte)mazeIsland.childDrop.Item);

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

        //Update raft animation
        bool firstRaft = false;
        foreach (World w in worlds)
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
        if (westHyrule.locationAtPalace1.PalaceNumber == 7 || westHyrule.locationAtPalace2.PalaceNumber == 7 || westHyrule.locationAtPalace3.PalaceNumber == 7)
        {
            ROMData.Put(0x1dd3b, 0x05);
        }

        if (mazeIsland.locationAtPalace4.PalaceNumber == 7)
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
            magFunction[i] = ROMData.GetByte(functionBase + SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]].VanillaSpellOrder());
        }

        for (int i = 0; i < magEffects.Count(); i = i + 2)
        {
            magEffects[i] = ROMData.GetByte(effectBase + SpellMap[Towns.STRICT_SPELL_LOCATIONS[i / 2]].VanillaSpellOrder() * 2);
            magEffects[i + 1] = ROMData.GetByte(effectBase + SpellMap[Towns.STRICT_SPELL_LOCATIONS[i / 2]].VanillaSpellOrder() * 2 + 1);
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                magLevels[i, j] = ROMData.GetByte(spellCostBase + (SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]].VanillaSpellOrder() * 8 + j));
            }

            for (int j = 0; j < 7; j++)
            {
                magNames[i, j] = ROMData.GetByte(spellNameBase + (SpellMap[Towns.STRICT_SPELL_LOCATIONS[i]].VanillaSpellOrder() * 0xe + j));
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

        long inthash = BitConverter.ToInt64(hash, 0);

        ROMData.Put(0x17C2C, (byte)(((inthash) & 0x1F) + 0xD0));
        ROMData.Put(0x17C2E, (byte)(((inthash >> 5) & 0x1F) + 0xD0));
        ROMData.Put(0x17C30, (byte)(((inthash >> 10) & 0x1F) + 0xD0));
        ROMData.Put(0x17C32, (byte)(((inthash >> 15) & 0x1F) + 0xD0));
        ROMData.Put(0x17C34, (byte)(((inthash >> 20) & 0x1F) + 0xD0));
        ROMData.Put(0x17C36, (byte)(((inthash >> 25) & 0x1F) + 0xD0));
    }

    public void RandomizeSmallItems(int world, bool first)
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
                    if (item == 8 || (item > 9 && item < 14) || (item > 15 && item < 19) && !addresses.Contains(addr))
                    {
                        if (UNSAFE_DEBUG)
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
        foreach (Item item in SHUFFLABLE_STARTING_ITEMS)
        {
            logger.Log(logLevel, item.ToString() + "(" + ItemGet[item] + ") : " + itemLocs.Where(i => i.Item == item).FirstOrDefault()?.Name);
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
        ushort cpuOffset = (ushort)(bank == 7 ? 0xc000 : 0x8000);
        ushort val = (ushort)(addr - (bank * banksize) + cpuOffset);
        return new byte[] { (byte)(val & 0xff), (byte)(val >> 8) };
    }

    private void AddCropGuideBoxesToFileSelect(Engine engine)
    {
        Assembler.Assembler assembler = new();
        assembler.Code("""
.segment "PRG5"
.org $b28d
    jsr CustomFileSelectUpdates

.reloc
CustomFileSelectUpdates:
    cpy #$01
    beq @Skip
        sta $0726  ; perform the original hooked code before exiting
        rts
@Skip:
    ldx #$20     ; copy 32 + 1 bytes of data from the rom (+1 for terminator)
@Loop:
        lda CustomFileSelectData,x
        sta $0302,x
        dex
        bpl @Loop    ; while x >= 0
    lda #0    ; use the draw handler for $302
    sta $0725 ; tell the code to draw from $302
    inc $073E ; increase the "state" count
    pla       ; we need to double return here to break out of the hook and the calling function
    pla
    rts

.define whiteTile $fd
.define blueTile $fe
CustomFileSelectData:
    .byte $20, $00, $01, whiteTile
    .byte $20, $1f, $01, whiteTile
    .byte $23, $a0, $01, whiteTile
    .byte $23, $bf, $01, whiteTile
    .byte $20, $21, $01, blueTile
    .byte $20, $3e, $01, blueTile
    .byte $23, $81, $01, blueTile
    .byte $23, $9e, $01, blueTile
    .byte $ff
""", "crop_guides.s");
        engine.Modules.Add(assembler.Actions);
    }

    public List<Location> AllLocationsForReal()
    {
        List<Location> locations = westHyrule.AllLocations
            .Union(eastHyrule.AllLocations)
            .Union(mazeIsland.AllLocations)
            .Union(deathMountain.AllLocations).ToList();
        locations.Add(eastHyrule.spellTower);
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
        //SHUFFLABLE_STARTING_ITEMS.Where(i => ItemGet[item] == false).ToList().ForEach(i => Debug.WriteLine(Enum.GetName(typeof(Item), i)));
        List<Location> allLocations = AllLocationsForReal();
        allLocations.Where(i => !i.ItemGet && i.Item != Item.DO_NOT_USE).ToList()
            .ForEach(i => sb.AppendLine(i.Name + " / " + Enum.GetName(typeof(Item), i.Item)));
        sb.AppendLine(SpellDebug());
        //Debug.WriteLine("---Inaccessable Locations---");
        //allLocations.Where(i => !i.Reachable).ToList().ForEach(i => Debug.WriteLine(i.Name));


        sb.AppendLine("");
        logger.Error(sb.ToString());
    }

    private bool IsRaftAlwaysRequired(RandomizerProperties props)
    {
        //If continent connections are vanilla, you for sure need the raft.
        if (props.ContinentConnections == ContinentConnectionType.NORMAL)
        {
            return true;
        }
        //If west and DM are both vanilla, the caves have to connect between west and DM, and vanilla connections are forced
        if ((props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
            && (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE))
        {
            return true;
        }
        //If east is vanilla, you can only get to it from raft or bridge, so if MI is vanilla, raft is required
        if ((props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
            && (props.MazeBiome == Biome.VANILLA || props.MazeBiome == Biome.VANILLA_SHUFFLE))
        {
            return true;
        }
        return false;
    }

    private Dictionary<Spell, Location> GetSpellLocations()
    {
        Dictionary<Spell, Location> spellLocations = new();
        foreach (Location location in AllLocationsForReal())
        {
            if (location.ActualTown > 0 && location.ActualTown != Town.SARIA_SOUTH && location.ActualTown != Town.SPELL_TOWER)
            {
                spellLocations.Add(SpellMap[location.ActualTown], location);
            }
            if(location.ActualTown == Town.MIDO_WEST)
            {
                spellLocations.Add(SpellMap[Town.MIDO_CHURCH], location);
            }
            if (location.ActualTown == Town.DARUNIA_WEST)
            {
                spellLocations.Add(SpellMap[Town.DARUNIA_ROOF], location);
            }
        }

        return spellLocations;
    }

    private void FixHelmetheadItemRoomDespawn(Engine engine)
    {
        byte helmetRoom = 0x22;
        if (props.NormalPalaceStyle.IsReconstructed())
        {
            helmetRoom = (byte)palaces[1].BossRoom.NewMap;
        }
        Assembler.Assembler assembler = new();
        assembler.Assign("HelmetRoom", helmetRoom);
        assembler.Code("""
.segment "PRG4"

.reloc
HelmetHeadGoomaFix:
    lda #<HelmetRoom
    eor $561
    rts

.org $bac3
    jsr HelmetHeadGoomaFix
.org $bc83
    jsr HelmetHeadGoomaFix
.org $bd75
    jsr HelmetHeadGoomaFix

.org $9b27
; Also fix a key glitch
    nop
    nop
    nop

""", "helmethead_gooma_fix.s");
        engine.Modules.Add(assembler.Actions);
    }

    private void RestartWithPalaceUpA(Engine engine) {
        
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

        Assembler.Assembler a = new();
        a.Code("""
update_next_level_exp = $a057

;(0=caves, enemy encounters...; 1=west hyrule towns; 2=east hyrule towns; 3=palace 1,2,5 ; 4=palace 3,4,6 ; 5=great palace)
world_state = $707
area_code = $561

.segment "PRG7"

.org $cbaa
    jsr PalacePatch

.reloc
PalacePatch:
    sta world_state
    ; lda $0707 ; don't need to reload 707 here
    ; if < 3 do the original patch
    cmp #$03
    bmi @Exit
    ; or if our temp flag is already set, do the original code
    lda $07b0
    cmp #$01
    beq ReloadExpForReset
        lda area_code
        sta $07b1 ; store the area code into a temp ram location
        lda #$01
        sta $07b0 ; set a flag in another empty ram location
        lda world_state
@Exit:
        rts
ReloadExpForReset:
    lda $07b1
    sta area_code
    jsr update_next_level_exp
    lda world_state
    rts

.org $cad0
    jsr ReloadExpForReset
    cmp #3
    bpl $cade ; *+9

.org $cae3
    ; Don't clear area code on reset
    nop
    nop
    nop

.org $cf92
    jsr SaveWorldStateAndClearFlag

.reloc
SaveWorldStateAndClearFlag:
    ; Precondition y = 0
    sty world_state
    sty $07b0
    rts

""", "restart_palace_upa.s");
        engine.Modules.Add(a.Actions);

    }

    /// <summary>
    /// I assume this fixes the XP on screen transition softlock, but who knows with all these magic bytes.
    /// </summary>
    private void FixSoftLock(Engine engine)
    {
        Assembler.Assembler a = new();
        a.Code("""
.segment "PRG7"
.org $e18a
    jsr FixSoftlock

.reloc
FixSoftlock:
    inc $0726
    lda $074c ; branch if dialog type is not "talking"
    cmp #$02
    beq +
        ldx #$00  ; otherwise close the talking dialog
        stx $074c
+   rts
""", "fix_softlock.s");
        engine.Modules.Add(a.Actions);
    }

    public void ApplyHudFixes(Engine engine)
    {
        Assembler.Assembler a = new();
        a.Assign("PREVENT_HUD_FLASH_ON_LAG", 0);
        a.Code(Assembly.GetExecutingAssembly().ReadResource("RandomizerCore.Asm.FixedHud.s"), "fixed_hud.s");
        engine.Modules.Add(a.Actions);
    }
    
    public void StandardizeDrops(Engine engine)
    {
        Assembler.Assembler a = new();
        a.Code("""
.segment "PRG7"
.org $e8ad
    jsr StandardizeDrops
.reloc
StandardizeDrops:
    cpy #$02
    bne +
        lda $06fe
        inc $06fe
        rts
+   lda $06ff
    inc $06ff
    rts
""", "standardize_drops.s");
        engine.Modules.Add(a.Actions);
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

    private void ApplyAsmPatches(RandomizerProperties props, Engine engine, Random RNG)
    {
        ROMData.ChangeMapperToMMC5(engine);
        AddCropGuideBoxesToFileSelect(engine);
        FixHelmetheadItemRoomDespawn(engine);

        if (props.RandomizeKnockback)
        {
            ROMData.RandomizeKnockback(engine, RNG);
        }

        if (props.HardBosses)
        {
            ROMData.BuffCarrock(engine);
        }

        if (props.ReplaceFireWithDash)
        {
            ROMData.DashSpell(engine);
        }

        if (props.UpAC1)
        {
            ROMData.UpAController1(engine);
        }
        
        if (props.UpARestartsAtPalaces)
        {
            RestartWithPalaceUpA(engine);
        }

        if (props.StandardizeDrops)
        {
        }
        FixSoftLock(engine);
        ApplyHudFixes(engine);
        RandomizeStartingValues(engine);
        
        ROMData.FixContinentTransitions(engine);
    }
}