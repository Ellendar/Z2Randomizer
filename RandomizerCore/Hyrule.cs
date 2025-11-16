using DynamicData;
using FtRandoLib.Importer;
using js65;
using NLog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

public readonly record struct RandomizerResult(
    bool success,
    byte[]? romdata = null,
    string? debuginfo = null,
    string? messages = null);

public class Hyrule
{
    public delegate Assembler NewAssemblerFn(Js65Options? options = null, bool debugJavaScript = false);

    //IMPORTANT: Tuning these factors can have a big impact on generation times.
    //In general, the non-terrain shuffle features are much cheaper than the cost of generating terrain or verifying the seed.

    //This controls how many attempts will be made to shuffle the non-terrain features like towns, spells, and items.
    //The higher you set it, the more likely a given terrain is to find a set of items that works, resulting in fewer terrain generations.
    //It will also increase the number of seeds that have more arcane solutions, where only a specific item route works.
    //This was originally set to 10, but increasing it to 100 massively reduces the number of extremely degenerate caldera and mountain generation times
    private const int NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT = 200;

    //This controls how many times
    private const int NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT = 10;

    //This controls how many times
    private const int TERRAIN_GENERATION_ATTEMPT_LIMIT = 50000;

    //private readonly Item[] SHUFFLABLE_STARTING_ITEMS = new Item[] { Item.CANDLE, Item.GLOVE, Item.RAFT, Item.BOOTS, Item.FLUTE, Item.CROSS, Item.HAMMER, Item.MAGIC_KEY };

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private MusicRandomizer? musicRandomizer = null;

    private readonly int[] palPalettes = [0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60];
    private readonly int[] palGraphics = [0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06];

    /*
    private static readonly Collectable[] SHUFFLABLE_STARTING_ITEMS = [
        Collectable.CANDLE, Collectable.GLOVE,
        Collectable.RAFT,
        Collectable.BOOTS,
        Collectable.FLUTE,
        Collectable.CROSS,
        Collectable.HAMMER,
        Collectable.MAGIC_KEY
    ];
    */

    //private ROM romData;
    private const int overworldXOff = 0x3F;
    private const int overworldMapOff = 0x7E;
    private const int overworldWorldOff = 0xBD;

    /*
    private static readonly List<Town> spellTowns = new List<Town>() { Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST,
        Town.MIDO_CHURCH, Town.NABOORU, Town.DARUNIA_WEST, Town.DARUNIA_ROOF, Town.NEW_KASUTO, Town.OLD_KASUTO};
    */

    //Unused
    //private Dictionary<int, int> spellEnters;
    //Unused
    //private Dictionary<int, int> spellExits;
    //Unused
    public HashSet<String> reachableAreas;
    //Which vanilla spell corresponds with which shuffled spell
    //private Dictionary<Town, Collectable> WizardCollectables { get; set; }
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

    //private Character character;

    public Dictionary<Collectable, bool> ItemGet { get; set; }
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

    //DEBUG/STATS
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static int DEBUG_THRESHOLD = 200;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    public DateTime startTime = DateTime.Now;
    // public DateTime startRandomizeStartingValuesTimestamp;
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

    /*
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
    */

    public ROM ROMData { get; set; }
    public Random r { get; set; }
    public string Flags { get; private set; }
    public int SeedHash { get; private set; }
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

    private static readonly Js65Options assemblerOptions = new()
    {
        // Must exist for the FileResolve callback to be called
        includePaths = [""],
        // debugLevel = 0,
        // debugLevel = 1,
#if DEBUG
        generateDebugInfo = true,
        debugLevel = 2,
#else
        generateDebugInfo = false,
        debugLevel = 0,
#endif
    };

    private readonly NewAssemblerFn NewAssembler;
    private readonly PalaceRooms palaceRooms;

    public string Hash { get; private set; }

    //This entire class structure is hot fucking garbage, but refactoring it such that it makes sense is
    //going to be a massive project, so for now we're just ignoring the fact that basically every property
    //is not guaranteed to initialize to the analyzer
#pragma warning disable CS8618
    public Hyrule(NewAssemblerFn createAsm, PalaceRooms rooms)
#pragma warning restore CS8618
    {
        NewAssembler = createAsm;
        palaceRooms = rooms;
    }
    public async Task<RandomizerResult> Randomize(byte[] vanillaRomData, RandomizerConfiguration config, Func<string, Task> progress, CancellationToken ct)
    {
        try
        {
            Hash = "";
            World.ResetStats();

            SeedHash = BitConverter.ToInt32(MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(config.Seed!)).AsSpan()[..4]);
            r = new Random(SeedHash);

            config.CheckForFlagConflicts();
            props = config.Export(r);
            //To make sure there isn't any similarity between the spoiler and non-spoiler versions of the seed, spin the RNG a bit.
            if(config.GenerateSpoiler)
            {
                r.NextBytes(new byte[64]);
            }
#if UNSAFE_DEBUG
            string export = JsonSerializer.Serialize(props, SourceGenerationContext.Default.RandomizerProperties);
            Debug.WriteLine(export);
#endif
            Flags = config.SerializeFlags();

            using Assembler assembler = CreateAssemblyEngine();
            logger.Info($"Started generation for flags: {Flags} seed: {config.Seed} seedhash: {SeedHash}");
            //character = new Character(props);
            shuffler = new Shuffler(props);

            palaces = [];
            ItemGet = [];
            foreach (Collectable item in Enum.GetValues(typeof(Collectable)))
            {
                if (item.IsItemGetItem())
                {
                    ItemGet.Add(item, false);
                }
            }
            ItemGet.Remove(props.ReplaceFireWithDash ? Collectable.FIRE_SPELL : Collectable.DASH_SPELL);
            accessibleMagicContainers = 4;
            reachableAreas = new HashSet<string>();
            //areasByLocation = new SortedDictionary<string, List<Location>>();

            byte[] correctVanillaHash = [0x76, 0x4D, 0x36, 0xFA, 0x8A, 0x24, 0x50, 0x83, 0x4D, 0xA5, 0xE8, 0x19, 0x42, 0x81, 0x03, 0x5A];
            var vanillaRomHash = MD5Hash.ComputeHash(vanillaRomData);
            if (!correctVanillaHash.SequenceEqual(vanillaRomHash))
            {
                throw new UserFacingException("Vanilla ROM checksum failure", "Please provide an unmodified Zelda 2 ROM (US release).");
            }

            // Make a copy of the vanilla data to prevent seed bleed
            ROMData = new ROM(vanillaRomData.ToArray(), true);

            if (props.RandomizeNewKasutoBasementRequirement)
            {
                kasutoJars = r.Next(5, 8);
            }
            else
            {
                kasutoJars = 7;
            }

            bool raftIsRequired = IsRaftAlwaysRequired(props);
            bool passedValidation = false;
            HashSet<int> freeBanks = [];
            if (ct.IsCancellationRequested) { return new RandomizerResult(false); }
            UpdateProgress(progress, 1);

            while (palaces.Count != 7 || passedValidation == false)
            {
                freeBanks = new(ROM.FreeRomBanks);
                var palaceGenerator = new Palaces();
                palaces = await palaceGenerator.CreatePalaces(r, props, palaceRooms, raftIsRequired, ct);

                if (palaces.Count == 0)
                {
                    continue;
                }

                /*
                if(!palaces.SelectMany(i => i.AllRooms).Any(i => i.Name == "gtmOldgpRooms M10"))
                {
                    continue;
                }
                */

                //Randomize Enemies
                if (props.ShufflePalaceEnemies)
                {
                    palaces.ForEach(i => i.RandomizeEnemies(props, r));
                }

                if (props.RandomizeSmallItems || props.ExtraKeys)
                {
                    palaces[0].RandomizeSmallItems(r, props.ExtraKeys);
                    palaces[1].RandomizeSmallItems(r, props.ExtraKeys);
                    palaces[2].RandomizeSmallItems(r, props.ExtraKeys);
                    palaces[3].RandomizeSmallItems(r, props.ExtraKeys);
                    palaces[4].RandomizeSmallItems(r, props.ExtraKeys);
                    palaces[5].RandomizeSmallItems(r, props.ExtraKeys);
                    //Small items in GP shouldn't be randomized. This is intentional to keep
                    //the randomization in line with the original "shuffle" behavior
                    //palaces[6].RandomizeSmallItems(RNG, props.ExtraKeys);
                }

                AsmModule sideviewModule = new();
                passedValidation = await FillPalaceRooms(sideviewModule);


                assembler.Add(sideviewModule);
            }

            ROMData.WriteKasutoJarAmount(kasutoJars);
            ROMData.DoHackyFixes();
            ROMData.AdjustGpProjectileDamage();

            shuffler.ShuffleDrops(ROMData, r);
            shuffler.ShufflePbagAmounts(ROMData, r);

            ROMData.DisableTurningPalacesToStone();
            ROMData.UpdateMapPointers();

            if (props.DashAlwaysOn)
            {
                ROMData.Put(0x13C3, [0x30, 0xD0]);
            }

            if (props.PermanentBeam)
            {
                ROMData.Put(0x186c, 0xEA);
                ROMData.Put(0x186d, 0xEA);
            }

            ShortenWizards();

            firstProcessOverworldTimestamp = DateTime.Now;
            await ProcessOverworld(progress, ct);
            if (ct.IsCancellationRequested) { return new RandomizerResult(false); }
            UpdateProgress(progress, 8);

            if (props.ShuffleOverworldEnemies)
            {
                // move a PRG1 background map to make space for more enemy data
                ROMData.RelocateData(assembler, 1, 0x8000);
                OverworldEnemyShuffler.Shuffle(worlds, assembler, ROMData, props.MixLargeAndSmallEnemies, props.GeneratorsAlwaysMatch, r);
            }

            if (props.CombineFire)
            {
                List<Collectable>? customSpellOrder = props.IncludeSpellsInShuffle
                    ? null
                    : AllLocationsForReal()
                        .Where(l => l.ActualTown != null && Towns.STRICT_SPELL_LOCATIONS.Contains((Town)l.ActualTown))
                        //This makes the assumption that is currently true that each "town" has exactly one item.
                        //If we later restructure towns to be omni-towns to get rid of fake towns, this will be untrue
                        .Select(l => l.Collectables[0]).ToList();
                ROMData.CombineFireSpell(assembler, customSpellOrder, r);
            }

            Dictionary<Town, Collectable> spellMap = new()
            {
                { (Town)westHyrule.locationAtRauru.ActualTown!, westHyrule.locationAtRauru.Collectables[0] },
                { (Town)westHyrule.locationAtMido.ActualTown!, westHyrule.locationAtMido.Collectables[0] },
                { (Town)westHyrule.locationAtSariaNorth.ActualTown!, westHyrule.locationAtSariaNorth.Collectables[0] },
                { (Town)westHyrule.locationAtRuto.ActualTown!, westHyrule.locationAtRuto.Collectables[0] },
                { (Town)eastHyrule.townAtNabooru.ActualTown!, eastHyrule.townAtNabooru.Collectables[0] },
                { (Town)eastHyrule.townAtDarunia.ActualTown!, eastHyrule.townAtDarunia.Collectables[0] },
                { (Town)eastHyrule.townAtNewKasuto.ActualTown!, eastHyrule.townAtNewKasuto.Collectables[0] },
                { (Town)eastHyrule.townAtOldKasuto.ActualTown!, eastHyrule.townAtOldKasuto.Collectables[0] },
            };

            if (props.StartsWithCollectable(spellMap[Town.RUTO]))
            {
                ROMData.Put(0x17b14, 0x10); //Trophy
            }
            if (props.StartsWithCollectable(spellMap[Town.SARIA_NORTH]))
            {
                ROMData.Put(0x17b15, 0x01); //Mirror
            }
            if (props.StartsWithCollectable(spellMap[Town.MIDO_WEST]))
            {
                ROMData.Put(0x17b16, 0x40); //Medicine
            }
            if (props.StartsWithCollectable(spellMap[Town.NABOORU]))
            {
                ROMData.Put(0x17b17, 0x01); //Water
            }
            if (props.StartsWithCollectable(spellMap[Town.DARUNIA_WEST]))
            {
                ROMData.Put(0x17b18, 0x20); //Child
            }

            if (ct.IsCancellationRequested) { return new RandomizerResult(false); }
            UpdateProgress(progress, 9);

            List<Text> texts = CustomTexts.GenerateTexts(AllLocationsForReal(), itemLocs, ROMData.GetGameText(), props, r);
            StatRandomizer randomizedStats = new(ROMData, props);
            randomizedStats.Randomize(r);
            randomizedStats.Write(ROMData);

            // ideally this should be calculated later, but custom music changes asm patches
            byte[] randoRomHash = MD5Hash.ComputeHash(ROMData.rawdata);

            // ROM changes after this will vary with customize tab options
            //Allows casting magic without requeueing a spell
            if (props.FastCast)
            {
                ROMData.WriteFastCastMagic();
            }

            bool randomizeMusic = false;
            if (props.DisableMusic)
            {
                ROMData.DisableMusic();
            }
            else
            {
                randomizeMusic = props.RandomizeMusic;
            }

            ApplyAsmPatches(props, assembler, r, texts, ROMData, randomizedStats);

            var rom = await ROMData.ApplyAsm(assembler);
            if (!rom.success)
            {
                return new RandomizerResult(false, null, null, string.Join(Environment.NewLine, rom.messages));
            }
            ROMData = new ROM(rom.romdata);

            if (randomizeMusic)
            {
                string musicDir = "Music";
                List<string> jsonLibPaths = new(),
                    yamlLibPaths = new();
                if (Directory.Exists(musicDir))
                {
                    var jsonExts = Z2Importer.JsonExtensions();
                    var yamlExts = Z2Importer.YamlExtensions();

                    foreach (string path in Directory.EnumerateFiles(musicDir))
                    {
                        string ext = Path.GetExtension(path);
                        if (jsonExts.Contains(ext))
                            jsonLibPaths.Add(path);
                        else if (yamlExts.Contains(ext))
                            yamlLibPaths.Add(path);
                    }
                }

                Random musicRng = new(SeedHash);
                bool success = false;
                int triesLeft = 8;
                while (!success)
                {
                    try
                    {
                        MusicRandomizer musicRnd = new(
                            this,
                            musicRng.Next(),
                            jsonLibPaths,
                            yamlLibPaths,
                            freeBanks,
                            props.MixCustomAndOriginalMusic,
                            props.IncludeDiverseMusic,
                            props.DisableUnsafeMusic);
                        musicRnd.ImportSongs();

                        musicRandomizer = musicRnd;
                        freeBanks = new(musicRnd.FreeBanks);

                        success = true;
                    }
                    catch (RomFullException e)
                    {
                        if (--triesLeft == 0)
                            throw;

                        logger.Debug(e, $"Song packing failed. {triesLeft} tries left.");
                    }
                }
            }

            byte[] finalRNGState = new byte[32];

            r.NextBytes(finalRNGState);
            byte[] hash = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(
                Flags +
                SeedHash +
                randoRomHash + // ideally this should be all that's required
                // Util.ReadAllTextFromFile(config.GetRoomsFile()) +
                Util.ByteArrayToHexString(finalRNGState)
            ));

            UpdateRom();

            //0 -> ! to avoid 0/O confusion
            byte[] z2Hash = ConvertHash(hash);
            for(int i = 0; i < z2Hash.Length; i++)
            {
                if(z2Hash[i] == 0xD0)
                {
                    z2Hash[i] = 0x36;
                }
            }
            //(0xD0), 0x36);
            ROMData.Put(0x17C2C, z2Hash);
            Hash = Util.FromGameText(z2Hash);

            /*
            if (UNSAFE_DEBUG)
            {
                PrintDebugSpoiler(LogLevel.Error);
                //DEBUG
                StringBuilder sb = new();
                foreach (Palace palace in palaces)
                {
                    sb.AppendLine("Palace: " + palace.Number);
                    foreach (Room room in palace.AllRooms.OrderBy(i => i.Map))
                    {
                        sb.AppendLine(room.DebugString());
                    }
                    File.WriteAllText("rooms.log", sb.ToString());
                }
            }*/
            return new RandomizerResult(true, ROMData.rawdata, rom.debugfile, string.Join(Environment.NewLine, rom.messages));
        }
        catch(Exception e)
        {
            logger.Error(e);
            Debug.WriteLine(e.StackTrace);
            throw;
        }
    }

    private Assembler CreateAssemblyEngine()
    {
        var asm = NewAssembler(assemblerOptions);
        asm.Callbacks = new Js65Callbacks
        {
            OnFileReadText = AsmFileReadTextCallback,
        };

        asm.Module().Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.Init.s"), "__init.s");

        return asm;
    }

    private string AsmFileReadTextCallback(string basePath, string path)
    {
        return Util.ReadResource($"Z2Randomizer.RandomizerCore.Asm.{path.Replace('/', '.').Replace('\\', '.')}");
    }

    private static byte[] ConvertHash(byte[] hash)
    {
        var inthash = BitConverter.ToInt64(hash, 0);
        return [
            (byte)(((inthash >> 0)  & 0x1F) + 0xD0),
            0xf4,
            (byte)(((inthash >> 5)  & 0x1F) + 0xD0),
            0xf4,
            (byte)(((inthash >> 10)  & 0x1F) + 0xD0),
            0xf4,
            (byte)(((inthash >> 15)  & 0x1F) + 0xD0),
            0xf4,
            (byte)(((inthash >> 20)  & 0x1F) + 0xD0),
            0xf4,
            (byte)(((inthash >> 25)  & 0x1F) + 0xD0)
        ];
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

    private void ShuffleItems()
    {
        List<Collectable> shufflableItems = [
            Collectable.CANDLE, Collectable.GLOVE, Collectable.RAFT, Collectable.BOOTS,
            Collectable.FLUTE, Collectable.CROSS, Collectable.HEART_CONTAINER, Collectable.HEART_CONTAINER,
            Collectable.MAGIC_CONTAINER, Collectable.MEDICINE, Collectable.TROPHY, Collectable.HEART_CONTAINER,
            Collectable.HEART_CONTAINER, Collectable.MAGIC_CONTAINER, Collectable.MAGIC_KEY, Collectable.MAGIC_CONTAINER,
            Collectable.HAMMER, Collectable.CHILD, Collectable.MAGIC_CONTAINER];
        List<Collectable> minorItems = [Collectable.BLUE_JAR, Collectable.RED_JAR, Collectable.SMALL_BAG,
            Collectable.MEDIUM_BAG, Collectable.LARGE_BAG, Collectable.XL_BAG, Collectable.ONEUP, Collectable.KEY];

        if (props.PbagItemShuffle)
        {
            westHyrule.pbagCave.Collectables = [(Collectable)ROMData.GetByte(RomMap.WEST_PBAG_CAVE_COLLECTABLE)];
            eastHyrule.pbagCave1.Collectables = [(Collectable)ROMData.GetByte(RomMap.EAST_PBAG_CAVE1_COLLECTABLE)];
            eastHyrule.pbagCave2.Collectables = [(Collectable)ROMData.GetByte(RomMap.EAST_PBAG_CAVE2_COLLECTABLE)];
            shufflableItems.Add(westHyrule.pbagCave.Collectables[0]);
            shufflableItems.Add(eastHyrule.pbagCave1.Collectables[0]);
            shufflableItems.Add(eastHyrule.pbagCave2.Collectables[0]);
        }
        else
        {
            westHyrule.pbagCave.Collectables = [Collectable.LARGE_BAG];
            eastHyrule.pbagCave1.Collectables = [Collectable.LARGE_BAG];
            eastHyrule.pbagCave2.Collectables = [Collectable.XL_BAG];
        }

        if(props.IncludeSpellsInShuffle)
        {
            shufflableItems.Add(Collectable.SHIELD_SPELL);
            shufflableItems.Add(Collectable.JUMP_SPELL);
            shufflableItems.Add(Collectable.LIFE_SPELL);
            shufflableItems.Add(Collectable.FAIRY_SPELL);
            shufflableItems.Add(props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL);
            shufflableItems.Add(Collectable.REFLECT_SPELL);
            shufflableItems.Add(Collectable.SPELL_SPELL);
            shufflableItems.Add(Collectable.THUNDER_SPELL);
        }
        else if(props.ShuffleSpellLocations)
        {
            ShuffleSpells();
        }

        if (props.IncludeQuestItemsInShuffle)
        {
            shufflableItems.Add(Collectable.BAGUS_NOTE);
            if (props.StartWithSpellItems)
            {
                shufflableItems.Add(minorItems[r.Next(minorItems.Count)]);
                shufflableItems.Add(minorItems[r.Next(minorItems.Count)]);
            }
            else
            {
                shufflableItems.Add(Collectable.MIRROR);
                shufflableItems.Add(Collectable.WATER);
            }
        }

        if (props.IncludeSwordTechsInShuffle)
        {
            shufflableItems.Add(Collectable.UPSTAB);
            shufflableItems.Add(Collectable.DOWNSTAB);
        }

        else if(props.SwapUpAndDownStab)
        {
            SwapUpAndDownstab();
        }

        heartContainersInItemPool = maxHearts - startHearts;

        foreach (Collectable item in ItemGet.Keys.ToList())
        {
            ItemGet[item] = false;
        }

        //TODO: Refactor these puts out of this class
        ROMData.Put(RomMap.START_CANDLE, props.StartCandle ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_GLOVE, props.StartGlove ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_RAFT, props.StartRaft ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_BOOTS, props.StartBoots ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_FLUTE, props.StartFlute ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_CROSS, props.StartCross ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_HAMMER, props.StartHammer ? (byte)1 : (byte)0);
        ROMData.Put(RomMap.START_MAGICAL_KEY, props.StartKey ? (byte)1 : (byte)0);

        foreach(Collectable collectable in ItemGet.Keys)
        {
            ItemGet[collectable] = props.StartsWithCollectable(collectable);
        }

        pbagHearts = [];
        //Replace any unused heart containers with small items
        if (heartContainersInItemPool < 4)
        {
            int heartContainersToRemove = 4 - heartContainersInItemPool;
            while (heartContainersToRemove > 0)
            {
                int remove = r.Next(shufflableItems.Count);
                if (shufflableItems[remove] == Collectable.HEART_CONTAINER)
                {
                    shufflableItems[remove] = minorItems[r.Next(minorItems.Count)];
                    heartContainersToRemove--;
                }
            }
        }

        if (props.StartWithSpellItems)
        {
            Debug.Assert(shufflableItems[9] == Collectable.MEDICINE);
            Debug.Assert(shufflableItems[10] == Collectable.TROPHY);
            Debug.Assert(shufflableItems[17] == Collectable.CHILD);
            shufflableItems[9] = minorItems[r.Next(minorItems.Count)];
            shufflableItems[10] = minorItems[r.Next(minorItems.Count)];
            shufflableItems[17] = minorItems[r.Next(minorItems.Count)];
            ItemGet[Collectable.TROPHY] = true;
            ItemGet[Collectable.MEDICINE] = true;
            ItemGet[Collectable.CHILD] = true;
            ItemGet[Collectable.MIRROR] = true;
            ItemGet[Collectable.WATER] = true;
        }
        else
        {
            Collectable townCollectable;
            townCollectable = westHyrule.AllLocations.First(i => i.ActualTown == Town.RUTO).Collectables[0];
            if (!townCollectable.IsMinorItem() && ItemGet[townCollectable] && !props.IncludeSpellsInShuffle)
            {
                Debug.Assert(shufflableItems[10] == Collectable.TROPHY);
                shufflableItems[10] = minorItems[r.Next(minorItems.Count)];
                ItemGet[Collectable.TROPHY] = true;
            }

            townCollectable = westHyrule.AllLocations.First(i => i.ActualTown == Town.MIDO_WEST).Collectables[0];
            if (!townCollectable.IsMinorItem() && ItemGet[townCollectable] && !props.IncludeSpellsInShuffle)
            {
                Debug.Assert(shufflableItems[9] == Collectable.MEDICINE);
                shufflableItems[9] = minorItems[r.Next(minorItems.Count)];
                ItemGet[Collectable.MEDICINE] = true;
            }

            townCollectable = eastHyrule.AllLocations.First(i => i.ActualTown == Town.DARUNIA_WEST).Collectables[0];
            if (!townCollectable.IsMinorItem() && ItemGet[townCollectable] && !props.IncludeSpellsInShuffle)
            {
                Debug.Assert(shufflableItems[17] == Collectable.CHILD);
                shufflableItems[17] = minorItems[r.Next(minorItems.Count)];
                ItemGet[Collectable.CHILD] = true;
            }
        }

        List<Collectable> possibleStartItems = [
            Collectable.CANDLE,
            Collectable.GLOVE,
            Collectable.RAFT,
            Collectable.BOOTS,
            Collectable.FLUTE,
            Collectable.CROSS,
            Collectable.HAMMER,
            Collectable.MAGIC_KEY];
        List<Collectable> possibleStartSpells = [
            Collectable.SHIELD_SPELL,
            Collectable.JUMP_SPELL,
            Collectable.LIFE_SPELL,
            Collectable.FAIRY_SPELL,
            Collectable.FIRE_SPELL,
            Collectable.DASH_SPELL,
            Collectable.REFLECT_SPELL,
            Collectable.SPELL_SPELL,
            Collectable.THUNDER_SPELL];

        foreach(Collectable item in possibleStartItems)
        {
            if(props.StartsWithCollectable(item))
            {
                shufflableItems[shufflableItems.IndexOf(item)] = minorItems.Sample(r);
            }
        }

        if(props.IncludeSpellsInShuffle)
        {
            foreach (Collectable item in possibleStartSpells)
            {
                if (props.StartsWithCollectable(item) && shufflableItems.Contains(item))
                {
                    shufflableItems[shufflableItems.IndexOf(item)] = minorItems.Sample(r);
                }
            }
        }

        if(props.IncludeSwordTechsInShuffle)
        {
            if (props.StartWithDownstab)
            {
                shufflableItems[shufflableItems.IndexOf(Collectable.DOWNSTAB)] = minorItems.Sample(r);
            }
            if (props.StartWithUpstab)
            {
                shufflableItems[shufflableItems.IndexOf(Collectable.UPSTAB)] = minorItems.Sample(r);
            }
        }

        //Handle excess items

        List<Collectable> excessItems = [];

        //Heart containers over 4 are excess
        for (int i = 4; i < heartContainersInItemPool; i++)
        {
            excessItems.Add(Collectable.HEART_CONTAINER);
        }

        int extraPalaceItemCount = props.PalaceItemRoomCounts.Select(c => Math.Max(c - 1, 0)).Sum();
        for (int i = 0; i < extraPalaceItemCount; i++)
        {
            shufflableItems.Add(minorItems.Sample(r));
        }


        List<Collectable> vanillaPalaceItems = [Collectable.CANDLE, Collectable.GLOVE, Collectable.RAFT, Collectable.BOOTS, Collectable.FLUTE, Collectable.CROSS];

        //No palace items handling
        for (int i = 0; i < 6; i++)
        {
            if (props.PalaceItemRoomCounts[i] == 0)
            {
                var vanillaPalaceItem = vanillaPalaceItems[i];
                if (props.StartsWithCollectable(vanillaPalaceItem))
                {
                    // since the palace item is replaced by a minor item, remove a minor item from the shuffle
                    var firstMinorItemIndex = shufflableItems.FindIndex(c => c.IsMinorItem());
                    Debug.Assert(firstMinorItemIndex != -1);
                    shufflableItems.RemoveAt(firstMinorItemIndex);
                }
                else
                {
                    excessItems.Add(vanillaPalaceItem);
                    shufflableItems.Remove(vanillaPalaceItem);
                }
            }
        }

        List<int> minorItemIndexes = [];
        for(int i = 0; i < shufflableItems.Count; i++)
        {
            if (shufflableItems[i].IsMinorItem())
            {
                minorItemIndexes.Add(i);
            }
        }

        //Add the auto pbag cave promotion
        List<Location> overflowLocations = [];
        if (!props.PbagItemShuffle)
        {
            overflowLocations.AddRange([westHyrule.pbagCave, eastHyrule.pbagCave1, eastHyrule.pbagCave2]);
        }
        int overflowLocationsRequired = excessItems.Count - minorItemIndexes.Count;


        if (overflowLocationsRequired > overflowLocations.Count)
        {
            throw new Exception("Insufficient locations to place excess items. The validation should have caught this.");
        }

        for(int i = 0; i < overflowLocationsRequired; i++)
        {
            Location overflowLocation = overflowLocations.Sample(r)!;
            itemLocs.Add(overflowLocation);
            shufflableItems.Add(Collectable.SMALL_BAG);
            minorItemIndexes.Add(shufflableItems.Count - 1);
            overflowLocations.Remove(overflowLocation);
        }

        while(excessItems.Count > 0)
        {
            int minorItemIndexIndex = r.Next(0, minorItemIndexes.Count);
            int minorItemIndex = minorItemIndexes[minorItemIndexIndex];
            int excessItemIndex = r.Next(0, excessItems.Count);

            shufflableItems[minorItemIndex] = excessItems[excessItemIndex];
            minorItemIndexes.RemoveAt(minorItemIndexIndex);
            excessItems.RemoveAt(excessItemIndex);
        }

        //Do the actual shuffling
        List<Location> duplicateItemPlacementCandidates = [];
        palaces.ForEach(i => i.ItemRooms.ForEach(j => j.Collectable = null));
        if (props.MixOverworldPalaceItems)
        {
            duplicateItemPlacementCandidates.AddRange(itemLocs);
            DoShuffle(shufflableItems, itemLocs);
        }
        else
        {
            List<Collectable> itemsToActuallyShuffle;
            List<Location> shufflableItemLocations;

            if (props.ShufflePalaceItems)
            {
                itemsToActuallyShuffle = [];
                shufflableItemLocations = [];
                foreach (Location palaceLocation in itemLocs.Where(i => i.PalaceNumber != null))
                {
                    shufflableItemLocations.Add(palaceLocation);
                    Collectable vanillaCollectable = Palace.GetVanillaCollectable(palaceLocation.PalaceNumber);
                    itemsToActuallyShuffle.Add(shufflableItems.Contains(vanillaCollectable) ? vanillaCollectable : minorItems.Sample(r));

                    for (int i = 1; i < props.PalaceItemRoomCounts[(int)palaceLocation.PalaceNumber! - 1]; i++)
                    {
                        itemsToActuallyShuffle.Add(minorItems.Sample(r));
                    }
                }
                duplicateItemPlacementCandidates.AddRange(shufflableItemLocations);
                DoShuffle(itemsToActuallyShuffle, shufflableItemLocations);
            }
            else
            {
                foreach (Location palaceLocation in itemLocs.Where(i => i.PalaceNumber != null && i.PalaceNumber < 7))
                {
                    Collectable vanillaCollectable = Palace.GetVanillaCollectable(palaceLocation.PalaceNumber);
                    palaceLocation.Collectables = [shufflableItems.Contains(vanillaCollectable) ? vanillaCollectable : minorItems.Sample(r)];
                    palaces[(int)palaceLocation.PalaceNumber! - 1].ItemRooms.Sample(r)!.Collectable = vanillaCollectable;
                    for (int i = 0; i < props.PalaceItemRoomCounts[(int)palaceLocation.PalaceNumber! - 1]; i++)
                    {
                        if (palaces[(int)palaceLocation.PalaceNumber! - 1].ItemRooms[i].Collectable == null)
                        {
                            Collectable smallItem = minorItems.Sample(r);
                            palaceLocation.Collectables.Add(smallItem);
                            palaces[(int)palaceLocation.PalaceNumber! - 1].ItemRooms[i].Collectable = smallItem;
                        }
                    }
                }
            }

            if (props.ShuffleOverworldItems)
            {
                itemsToActuallyShuffle = [];
                shufflableItemLocations = [];
                foreach (Location nonPalaceLocation in itemLocs.Where(i => i.PalaceNumber == null))
                {
                    shufflableItemLocations.Add(nonPalaceLocation);
                    itemsToActuallyShuffle.Add(shufflableItems.Contains(nonPalaceLocation.VanillaCollectable) 
                        ? nonPalaceLocation.VanillaCollectable : minorItems.Sample(r));
                }
                duplicateItemPlacementCandidates.AddRange(shufflableItemLocations);
                DoShuffle(itemsToActuallyShuffle, shufflableItemLocations);
            }
            else
            {
                foreach (Location nonPalaceLocation in itemLocs.Where(i => i.PalaceNumber == null))
                {
                    Collectable vanillaCollectable = nonPalaceLocation.VanillaCollectable;
                    nonPalaceLocation.Collectables = [shufflableItems.Contains(vanillaCollectable) ? vanillaCollectable : minorItems.Sample(r)];
                }
            }
        }

        //Assigning unshuffled locations can make the number of heart containers wrong, so re-adjust them
        List<Location> heartContainerLocations = itemLocs.Where(i => i.Collectables.Contains(Collectable.HEART_CONTAINER)).ToList();
        int heartContainerCount = itemLocs.SelectMany(i => i.Collectables).Count(i => i == Collectable.HEART_CONTAINER);
        while(heartContainerCount > heartContainersInItemPool)
        {
            Location location = heartContainerLocations.Sample(r)!;
            int index = r.Next(location.Collectables.Count);
            if (location.Collectables[index] == Collectable.HEART_CONTAINER)
            {
                location.Collectables[index] = minorItems.Sample(r);
                heartContainerCount--;
                if (!location.Collectables.Any(i => i == Collectable.HEART_CONTAINER))
                {
                    heartContainerLocations.Remove(location);
                }
            }
        }

        while(heartContainerCount < heartContainersInItemPool)
        {
            List<Location> minorItemLocations = itemLocs.Where(i => i.Collectables.Any(j => j.IsMinorItem())).ToList();

            Location location = minorItemLocations.Sample(r)!;
            int index = r.Next(location.Collectables.Count);
            if (location.Collectables[index].IsMinorItem())
            {
                location.Collectables[index] = Collectable.HEART_CONTAINER;
                heartContainerCount++;
                if (!location.Collectables.Any(i => i.IsMinorItem()))
                {
                    minorItemLocations.Remove(location);
                }
            }
        }

        if (props.AllowImportantItemDuplicates)
        {
            List<Collectable> importantItemsToDuplicate = [ /* in (non-random) priority order */
                Collectable.GLOVE,
                Collectable.DOWNSTAB,
                Collectable.FAIRY_SPELL,
                Collectable.THUNDER_SPELL,
                Collectable.REFLECT_SPELL,
                Collectable.MAGIC_KEY,
                Collectable.RAFT,
                Collectable.BOOTS,
                Collectable.HAMMER,
                Collectable.FLUTE,
                Collectable.UPSTAB,
                Collectable.JUMP_SPELL,
            ];

            importantItemsToDuplicate = importantItemsToDuplicate.Where(shufflableItems.Contains).ToList();

            List<Location> minorItemLocations = duplicateItemPlacementCandidates.Where(i => i.Collectables.Any(c => c.IsMinorItem())).ToList();
            int replaceableMinorItemCount = duplicateItemPlacementCandidates.Sum(l => l.Collectables.Count(c => c.IsMinorItem()));
            importantItemsToDuplicate = importantItemsToDuplicate.GetRange(0, int.Min(replaceableMinorItemCount, importantItemsToDuplicate.Count));

            importantItemsToDuplicate.FisherYatesShuffle(r);
            minorItemLocations.FisherYatesShuffle(r);

            for (int itemIndex = 0; itemIndex < importantItemsToDuplicate.Count; itemIndex++)
            {
                Location minorItemLocation = minorItemLocations.Sample(r)!;
                minorItemLocation.Collectables.FisherYatesShuffle(r);
                for(int collectableIndex = 0; collectableIndex < minorItemLocation.Collectables.Count; collectableIndex++) 
                {
                    if(minorItemLocation.Collectables[collectableIndex].IsMinorItem())
                    {
                        minorItemLocation.Collectables[collectableIndex] = importantItemsToDuplicate[itemIndex];
                        if(!minorItemLocation.Collectables.Any(c => c.IsMinorItem()))
                        {
                            minorItemLocations.Remove(minorItemLocation);
                        }
                        break;
                    }
                }
            }
        }

    }

    private void DoShuffle(List<Collectable> itemsToShuffle, List<Location> itemShuffleLocations)
    {
        int itemShuffleLocationsCount = itemShuffleLocations.Count;
        if(itemShuffleLocations.Any(i => i.PalaceNumber != null))
        {
            itemShuffleLocationsCount = itemShuffleLocationsCount
                - itemShuffleLocations.Count(i => i.PalaceNumber != null)
                + props.PalaceItemRoomCounts.Sum(i => i);
                //- props.PalaceItemRoomCounts.Where(i => i == 0).Count();
        }
        if (itemsToShuffle.Count != itemShuffleLocationsCount)
        {
            throw new Exception("Item locations must match number of items");
        }

        itemsToShuffle.FisherYatesShuffle(r);

        //Clear all locations, then set them to the newly shuffled items
        itemShuffleLocations.ForEach(i => i.Collectables.Clear());

        using var itemLocsIterator = itemShuffleLocations.GetEnumerator();
        Location? location = null;
        int subIndex = 0;
        foreach (Collectable item in itemsToShuffle)
        {
            if (location?.PalaceNumber == null)
            {
                if (!itemLocsIterator.MoveNext()) { throw new InvalidOperationException("Ran out of item locations."); }
                location = itemLocsIterator.Current;
            }
            while (location.PalaceNumber is int palaceNum && ++subIndex > props.PalaceItemRoomCounts[palaceNum - 1])
            {
                subIndex = 0;
                if (!itemLocsIterator.MoveNext()) { throw new InvalidOperationException("Ran out of item locations."); }
                location = itemLocsIterator.Current;
            }

            location.Collectables.Add(item);
            if (location.PalaceNumber is int palaceNumInIf)
            {
                palaces[palaceNumInIf - 1].ItemRooms[subIndex - 1].Collectable = item;
            }
        }
        Debug.Assert(!itemLocsIterator.MoveNext(), "All item locations were not used. This should not happen.");
    }

    private async Task<bool> FillPalaceRooms(AsmModule sideviewModule)
    {
        //AssemblerCommon.AssemblerCommon validation_sideview_module = new();
        //AssemblerCommon.AssemblerCommon validation_gp_sideview_module = new();

        //This is an awful hack. We need to make a determination about whether the sideviews can fit in the available space,
        //but there is (at present) no way to test whether that is possible without rendering the entire engine into an irrecoverably
        //broken state, so we'll just run it twice. As long as this is the first modification that gets made on the engine, this is
        //guaranteed to succeed iff running on the original engine would succeed.
        //Jrowe feel free to engineer a less insane fix here.
        using Assembler validationEngine = CreateAssemblyEngine();

        int i = 0;
        //If multiple palaces use the same item room, they'll get consolidated under a single sideview
        //with multiple pointers, but then when the item update happens later, it will affect all the rooms,
        //so do a first pass with distinct items so all item rooms are always separate sideviews
        int itemRoomIndex = 0;
        foreach(Palace palace in palaces)
        {
            foreach(Room itemRoom in palace.ItemRooms)
            {
                while(((Collectable)itemRoomIndex).IsMinorItem())
                {
                    itemRoomIndex++;
                }
                itemRoom.UpdateSideviewItem((Collectable)itemRoomIndex++);
            }
        }
        //In Reconstructed, enemy pointers aren't separated between 125 and 346, they're just all in 1 big pile,
        //so we just start at the 125 pointer address
        // int enemyAddr = Enemies.NormalPalaceEnemyAddr;
        Dictionary<byte[], List<Room>> sideviews = new(new Util.StandardByteArrayEqualityComparer());
        Dictionary<byte[], List<Room>> sideviewsgp = new(new Util.StandardByteArrayEqualityComparer());
        foreach (Room room in palaces.Where(palace => palace.Number < 7).SelectMany(palace => palace.AllRooms).Where(room => room.Enabled))
        {
            if (sideviews.TryGetValue(room.SideView, out var value))
            {
                value.Add(room);
            }
            else
            {
                sideviews.Add(room.SideView, [room]);
            }
        }
        foreach (Room room in palaces.Where(palace => palace.Number == 7).SelectMany(palace => palace.AllRooms).Where(room => room.Enabled))
        {
            if (sideviewsgp.TryGetValue(room.SideView, out var value))
            {
                value.Add(room);
            }
            else
            {
                sideviewsgp.Add(room.SideView, [room]);
            }
        }

        var palaceItemBits = new Dictionary<PalaceGrouping, byte[]>
        {
            [PalaceGrouping.Palace125] = ROMData.GetBytes(Room.Group1ItemGetStartAddress, 0x20),
            [PalaceGrouping.Palace346] = ROMData.GetBytes(Room.Group2ItemGetStartAddress, 0x20),
            [PalaceGrouping.PalaceGp] = ROMData.GetBytes(Room.Group3ItemGetStartAddress, 0x20),
        };

        int enemyAddr = Enemies.NormalPalaceEnemyAddr;
        foreach (var sv in sideviews.Keys)
        {
            var name = "Sideview_" + i++;
            sideviewModule.Segment(["PRG1C", "PRG1D"]);
            sideviewModule.Reloc();
            sideviewModule.Label(name);
            sideviewModule.Byt(sv);
            //var j = 0;
            foreach (var room in sideviews[sv])
            {
                room.WriteSideViewPtr(sideviewModule, name);
                room.UpdateItemGetBits(palaceItemBits);
                enemyAddr += room.UpdateEnemies(sideviewModule, enemyAddr);
                room.UpdateConnectionStartAddress();
            }
        }

        i = 0;
        enemyAddr = Enemies.GPEnemyAddr;
        //GP Reconstructed
        foreach (var sv in sideviewsgp.Keys)
        {
            var name = "SideviewGP_" + i++;
            sideviewModule.Segment(["PRG1C", "PRG1D"]);
            sideviewModule.Reloc();
            sideviewModule.Label(name);
            sideviewModule.Byt(sv);
            //var j = 0;
            foreach (var room in sideviewsgp[sv])
            {
                room.WriteSideViewPtr(sideviewModule, name);
                room.UpdateItemGetBits(palaceItemBits);
                enemyAddr += room.UpdateEnemies(sideviewModule, enemyAddr);
                room.UpdateConnectionStartAddress();
            }
        }

        // Add the palace item bits to the assembler
        sideviewModule.Segment("PRG5");
        foreach (var (palaceGroup, itemBits) in palaceItemBits)
        {
            sideviewModule.Org((ushort)(0xBB95 + ((byte)palaceGroup) * 0x20));
            sideviewModule.Byt(itemBits);
        }

        try
        {
            ROM testRom = new(ROMData);
            Random testRng = new Random();
            //This continues to get worse, the text is based on the palaces and asm patched, so it needs to
            //be tested here, but we don't actually know what they will be until later, for now i'm just
            //testing with the vanilla text, but this could be an issue down the line.
            ApplyAsmPatches(props, validationEngine, testRng, ROMData.GetGameText(), testRom, new StatRandomizer(testRom, props));
            validationEngine.Add(sideviewModule);
            await testRom.ApplyAsm(validationEngine); //.Wait(ct);
        }
        catch (Exception e)
        {
            // Microsoft.ClearScript.ScriptEngine needs to be abstracted for browser
            if (e.Message.Contains("Could not find space for"))
            {
                logger.Debug(e, "Room packing failed. Retrying.");
                return false;
            }
            logger.Error(e, "Failed to build assembly patches");
            throw;
        }
        return true;
    }

    private bool IsEverythingReachable(Dictionary<Collectable, bool> itemGet)
    {
        totalReachableCheck++;
        int dm = 0;
        int mi = 0;
        int wh = 0;
        int eh = 0;
        int reachableLocationsCount = 1;
        int previousReachableLocationsCount = 0;
        int gettableItemsCount = 1;
        int previousGettableItemsCount = 0;
        debug++;

        int totalLocationsCount = westHyrule.AllLocations.Count + eastHyrule.AllLocations.Count 
            + deathMountain.AllLocations.Count + mazeIsland.AllLocations.Count;
        //logger.Debug("Locations count: West-" + westHyrule.AllLocations.Count + " East-" + eastHyrule.AllLocations.Count +
        //   " DM-" + deathMountain.AllLocations.Count + " MI-" + mazeIsland.AllLocations.Count + " Total-" + totalLocationsCount);
        while (reachableLocationsCount != previousReachableLocationsCount || gettableItemsCount != previousGettableItemsCount)
        {
            previousReachableLocationsCount = reachableLocationsCount;
            previousGettableItemsCount = gettableItemsCount;
            westHyrule.UpdateVisit(itemGet);
            deathMountain.UpdateVisit(itemGet);
            eastHyrule.UpdateVisit(itemGet);
            mazeIsland.UpdateVisit(itemGet);

            foreach (World world in worlds)
            {
                if (world.raft != null && CanGet(world.raft) && itemGet[Collectable.RAFT])
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
            gettableItemsCount = UpdateItemGets();

            //This 2nd pass is weird and may not need to exist, eventually I should run some stats on whether it helps or not
            westHyrule.UpdateVisit(itemGet);
            deathMountain.UpdateVisit(itemGet);
            eastHyrule.UpdateVisit(itemGet);
            mazeIsland.UpdateVisit(itemGet);

            gettableItemsCount = UpdateItemGets();

            reachableLocationsCount = 0;
            dm = 0;
            mi = 0;
            wh = 0;
            eh = 0;

            wh = westHyrule.AllLocations.Count(i => i.Reachable);
            eh = eastHyrule.AllLocations.Count(i => i.Reachable);
            dm = deathMountain.AllLocations.Count(i => i.Reachable);
            mi = mazeIsland.AllLocations.Count(i => i.Reachable);
            reachableLocationsCount = wh + eh + dm + mi;

            //logger.Debug("Starting reachable main loop(" + loopCount++ + ". prevCount:" + prevCount + " count:" + count
            //+ " updateItemsResult:" + updateItemsResult + " updateSpellsResult:" + updateSpellsResult);
        }

        foreach(Collectable item in ItemGet.Keys)
        {
            if (ItemGet[item] == false && item.IsItemGetItem())
            {
                itemGetReachableFailures++;
#if UNSAFE_DEBUG
                if (reachableLocationsCount >= DEBUG_THRESHOLD)
                {
                    Debug.WriteLine("Failed on collectables");
                    PrintRoutingDebug(reachableLocationsCount, wh, eh, dm, mi);
                    //Debug.WriteLine(westHyrule.GetMapDebug());
                    return false;
                }
#endif
                return false;
            }
        }
        if (accessibleMagicContainers != 8)
        {
            magicContainerReachableFailures++;
            //PrintRoutingDebug(reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }
        if (heartContainers < maxHearts)
        {
            heartContainerReachableFailures++;
            //PrintRoutingDebug(reachableLocationsCount, wh, eh, dm, mi);
            return false;
        }
        if(heartContainers > maxHearts)
        {
            throw new Exception("More hearts found than should exist in the seed.");
        }

        bool retval =
            CanGet(westHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(eastHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(deathMountain.RequiredLocations(props.HiddenPalace, props.HiddenKasuto))
            && CanGet(mazeIsland.RequiredLocations(props.HiddenPalace, props.HiddenKasuto));
        if (retval == false)
        {
            logicallyRequiredLocationsReachableFailures++;
#if UNSAFE_DEBUG
            List<Location> missingLocations =
            [
                .. westHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable),
                .. eastHyrule.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable),
                .. mazeIsland.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable),
            ];

            List<Location> nonDmLocations = new(missingLocations);
            missingLocations.AddRange(deathMountain.RequiredLocations(props.HiddenPalace, props.HiddenKasuto).Where(i => !i.Reachable));

            Debug.WriteLine("Unreachable locations:");
            Debug.WriteLine(string.Join("\n", missingLocations.Select(i => i.GetDebuggerDisplay())));

            return false;
#endif
        }

#if UNSAFE_DEBUG
        if (retval)
        {
            //PrintRoutingDebug(reachableLocationsCount, wh, eh, dm, mi);
        }
#endif
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether any items were marked accessable</returns>
    private int UpdateItemGets()
    {
        //TODO: Remove all the needX flags and replace them with a set of requirements.

        Location[] delayedEvaluationLocations = [eastHyrule.townAtNabooru, eastHyrule.townAtDarunia,
            eastHyrule.townAtNewKasuto, eastHyrule.townAtOldKasuto, eastHyrule.newKasutoBasement];
        List<RequirementType> requireables = GetRequireables();
        accessibleMagicContainers = 4;
        heartContainers = startHearts;
        Location newKasuto = eastHyrule.AllLocations.First(i => i.ActualTown == Town.NEW_KASUTO);
        List<Collectable> gottenItems = [];
        List<Location> locations = AllLocationsForReal().ToList();
        foreach (Location location in AllLocationsForReal())
        {
            foreach (Collectable collectable in location.Collectables)
            {
                if (collectable.IsInternalUse() || delayedEvaluationLocations.Contains(location))
                {
                    continue;
                }
                bool canGet;
                //Location is a palace
                if (location.PalaceNumber != null && location.PalaceNumber < 7)
                {
                    Palace palace = palaces[(int)location.PalaceNumber - 1];
                    canGet = CanGet(location)
                        //All palaces inherently required fairy spell or magic key
                        && (ItemGet[Collectable.FAIRY_SPELL] || ItemGet[Collectable.MAGIC_KEY])
                        && palace.GetGettableItems(requireables).Contains(collectable);
                }
                else if (location == eastHyrule.spellTower)
                {
                    canGet = CanGet(location) && ItemGet[Collectable.SPELL_SPELL];
                }
                //Location is a town
                else if (location.ActualTown != null
                    && Towns.townSpellAndItemRequirements.ContainsKey(location.ActualTown ?? Town.INVALID))
                {
                    canGet = CanGet(location) && Towns.townSpellAndItemRequirements[(Town)location.ActualTown!].AreSatisfiedBy(requireables);
                }
                else if (!delayedEvaluationLocations.Contains(location))
                {
                    //TODO: Remove all the needX flags and replace them with a set of requirements.
                    canGet = CanGet(location) && (!location.NeedHammer || ItemGet[Collectable.HAMMER]) && (!location.NeedRecorder || ItemGet[Collectable.FLUTE]);
                }
                else
                {
                    break;
                }

                ItemGet[collectable] = canGet;
                if (canGet)
                {
                    gottenItems.Add(collectable);
                }

                if (canGet && collectable == Collectable.HEART_CONTAINER)
                {
                    heartContainers++;
                }
                if (canGet && collectable == Collectable.MAGIC_CONTAINER)
                {
                    accessibleMagicContainers++;
                }
            }
        }

        //Special case for locations that could be dependent on the number of magic containers
        foreach (Location location in delayedEvaluationLocations)
        {
            bool canGet;
            if (location.ActualTown != null && Towns.townSpellAndItemRequirements.ContainsKey((Town)location.ActualTown))
            {
                canGet = CanGet(location) && Towns.townSpellAndItemRequirements[(Town)location.ActualTown!].AreSatisfiedBy(requireables);
            }
            else if (location == eastHyrule.newKasutoBasement)
            {
                canGet = CanGet(newKasuto) && accessibleMagicContainers >= kasutoJars;
            }
            else throw new Exception("Unrecognized delayed evaluation ItemGet location");

            if (canGet)
            {
                foreach (Collectable item in location.Collectables)
                {
                    ItemGet[item] = true;
                    gottenItems.Add(item);
                    if (canGet && item == Collectable.HEART_CONTAINER)
                    {
                        heartContainers++;
                    }
                    if (canGet && item == Collectable.MAGIC_CONTAINER)
                    {
                        accessibleMagicContainers++;
                    }
                }
            }
        }

        return gottenItems.Count;
    }

    public List<RequirementType> GetRequireables()
    {
        List<RequirementType> requireables = new();

        foreach(Collectable item in ItemGet.Keys)
        {
            if (ItemGet[item] && item.AsRequirement() != null)
            {
                RequirementType? reqirement = item.AsRequirement();
                if(reqirement != null)
                {
                    requireables.Add((RequirementType)reqirement);
                }
            }
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
        return requireables;
    }

    private async Task ProcessOverworld(Func<string, Task> progress, CancellationToken ct)
    {
        if (props.RandomizeSmallItems)
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
                westHyrule = new WestHyrule(props, r, ROMData);
                deathMountain = new DeathMountain(props, r, ROMData);
                eastHyrule = new EastHyrule(props, r, ROMData);
                mazeIsland = new MazeIsland(props, r, ROMData);
                worlds.Add(westHyrule);
                worlds.Add(deathMountain);
                worlds.Add(eastHyrule);
                worlds.Add(mazeIsland);
                ResetTowns();

                if (props.ContinentConnections == ContinentConnectionType.NORMAL)
                {
                    connections = [
                        (westHyrule.LoadCave1(ROMData, Continent.WEST, Continent.DM), deathMountain.LoadCave1(ROMData, Continent.DM, Continent.WEST)),
                        (westHyrule.LoadCave2(ROMData, Continent.WEST, Continent.DM), deathMountain.LoadCave2(ROMData, Continent.DM, Continent.WEST)),
                        (westHyrule.LoadRaft(ROMData, Continent.WEST, Continent.EAST), eastHyrule.LoadRaft(ROMData, Continent.EAST, Continent.WEST)),
                        (eastHyrule.LoadBridge(ROMData, Continent.EAST, Continent.MAZE), mazeIsland.LoadBridge(ROMData, Continent.MAZE, Continent.EAST))
                    ];
                }
                else if (props.ContinentConnections == ContinentConnectionType.TRANSPORTATION_SHUFFLE)
                {
                    List<int> chosen = new List<int>();
                    int type = r.Next(4);
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
                            type = r.Next(4);
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
                            type = r.Next(4);
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
                            type = r.Next(4);
                        } while (chosen.Contains(type));
                    }
                    SetTransportation(2, 3, type);
                }
                else if(props.ContinentConnections == ContinentConnectionType.ANYTHING_GOES)
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

                    int raftw1 = r.Next(worlds.Count);

                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        raftw1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(raftw1))
                        {
                            raftw1 = r.Next(worlds.Count);
                        }
                    }

                    int raftw2 = r.Next(worlds.Count);
                    if (props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        raftw2 = 2;
                    }
                    else
                    {
                        do
                        {
                            raftw2 = r.Next(worlds.Count);
                        } while (raftw1 == raftw2 || doNotPick.Contains(raftw2));
                    }

                    l1 = worlds[raftw1].LoadRaft(ROMData, (Continent)raftw1, (Continent)raftw2);
                    l2 = worlds[raftw2].LoadRaft(ROMData, (Continent)raftw2, (Continent)raftw1);
                    connections[0] = (l1, l2);

                    int bridgew1 = r.Next(worlds.Count);
                    if (props.EastBiome == Biome.VANILLA || props.EastBiome == Biome.VANILLA_SHUFFLE)
                    {
                        bridgew1 = 2;
                    }
                    else
                    {
                        while (doNotPick.Contains(bridgew1))
                        {
                            bridgew1 = r.Next(worlds.Count);
                        }
                    }
                    int bridgew2 = r.Next(worlds.Count);
                    if (props.MazeBiome == Biome.VANILLA || props.MazeBiome == Biome.VANILLA_SHUFFLE)
                    {
                        bridgew2 = 3;
                    }
                    else
                    {
                        do
                        {
                            bridgew2 = r.Next(worlds.Count);
                        } while (bridgew1 == bridgew2 || doNotPick.Contains(bridgew2));
                    }

                    l1 = worlds[bridgew1].LoadBridge(ROMData, (Continent)bridgew1, (Continent)bridgew2);
                    l2 = worlds[bridgew2].LoadBridge(ROMData, (Continent)bridgew2, (Continent)bridgew1);
                    connections[1] = (l1, l2);

                    int c1w1 = r.Next(worlds.Count);
                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c1w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c1w1))
                        {
                            c1w1 = r.Next(worlds.Count);
                        }
                    }
                    int c1w2 = r.Next(worlds.Count);
                    if (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c1w2 = 1;
                    }
                    else
                    {
                        do
                        {
                            c1w2 = r.Next(worlds.Count);
                        } while (c1w1 == c1w2 || doNotPick.Contains(c1w2));
                    }

                    l1 = worlds[c1w1].LoadCave1(ROMData, (Continent)c1w1, (Continent)c1w2);
                    l2 = worlds[c1w2].LoadCave1(ROMData, (Continent)c1w2, (Continent)c1w1);
                    connections[2] = (l1, l2);

                    int c2w1 = r.Next(worlds.Count);
                    if (props.WestBiome == Biome.VANILLA || props.WestBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c2w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c2w1))
                        {
                            c2w1 = r.Next(worlds.Count);
                        }
                    }
                    int c2w2 = r.Next(worlds.Count);
                    if (props.DmBiome == Biome.VANILLA || props.DmBiome == Biome.VANILLA_SHUFFLE)
                    {
                        c2w2 = 1;
                    }
                    else
                    {
                        do
                        {
                            c2w2 = r.Next(worlds.Count);
                        } while (c2w1 == c2w2 || doNotPick.Contains(c2w2));
                    }

                    l1 = worlds[c2w1].LoadCave2(ROMData, (Continent)c2w1, (Continent)c2w2);
                    l2 = worlds[c2w2].LoadCave2(ROMData, (Continent)c2w2, (Continent)c2w1);
                    connections[3] = (l1, l2);
                }
                else
                {
                    throw new ImpossibleException("Unrecognized continent connector type");
                }
            } while (!AllContinentsAreConnected(connections));

            int nonContinentGenerationAttempts = 0;
            int nonTerrainShuffleAttempt = 0;
            DateTime timestamp;
            //Shuffle everything else
            do //while (wtries < 10 && !EverythingReachable());
            {
                //GENERATE WEST
                if (ct.IsCancellationRequested) { return; }
                UpdateProgress(progress, 2);
                nonContinentGenerationAttempts++;
                timestamp = DateTime.Now;
                if (!westHyrule.AllReached)
                {
                    while (!westHyrule.Terraform(props, ROMData))
                    { 
                        if (++totalWestGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate West Continent");
                        }
                    }
                    totalWestGenerationAttempts++;
                }
                westHyrule.ResetVisitabilityState();
                timeSpentBuildingWest += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE DM
                if (ct.IsCancellationRequested) { return; }
                UpdateProgress(progress, 3);
                timestamp = DateTime.Now;
                if (!deathMountain.AllReached)
                {
                    while (!deathMountain.Terraform(props, ROMData))
                    { 
                        if (++totalDeathMountainGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate Death Mountain");
                        }
                    }
                    totalDeathMountainGenerationAttempts++;
                }
                deathMountain.ResetVisitabilityState();
                timeSpentBuildingDM += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE EAST
                if (ct.IsCancellationRequested) { return; }
                UpdateProgress(progress, 4);
                timestamp = DateTime.Now;
                if (!eastHyrule.AllReached)
                {
                    while (!eastHyrule.Terraform(props, ROMData))
                    { 
                        if (++totalEastGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate East Continent");
                        }
                    }
                    totalEastGenerationAttempts++;
                }
                eastHyrule.ResetVisitabilityState();
                timeSpentBuildingEast += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE MAZE ISLAND
                if (ct.IsCancellationRequested) { return; }
                UpdateProgress(progress, 5);
                timestamp = DateTime.Now;
                if (!mazeIsland.AllReached)
                {
                    while (!mazeIsland.Terraform(props, ROMData))
                    {
                        if (++totalMazeIslandGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate Maze Island");
                        }
                    }
                    totalMazeIslandGenerationAttempts++;
                }
                mazeIsland.ResetVisitabilityState();
                timeSpentBuildingMI += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                worlds.ForEach(i => i.SynchronizeLinkedLocations());

                if (ct.IsCancellationRequested) { return; }
                UpdateProgress(progress, 6);

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
                    westHyrule.ResetVisitabilityState();
                    eastHyrule.ResetVisitabilityState();
                    mazeIsland.ResetVisitabilityState();
                    deathMountain.ResetVisitabilityState();

                    //There was a spooky extra call to LoadItemLocs that used to be here that shouldn't be needed, but be aware.
                    westHyrule.SetStart();

                    ShufflePalaces();
                    LoadItemLocs(props.PalaceItemRoomCounts);
                    ShuffleItems();


                    westHyrule.UpdateAllReached();
                    eastHyrule.UpdateAllReached();
                    mazeIsland.UpdateAllReached();
                    deathMountain.UpdateAllReached();

                    nonTerrainShuffleAttempt++;
                } while (nonTerrainShuffleAttempt < NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT && !IsEverythingReachable(ItemGet));

                if (nonTerrainShuffleAttempt != NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT)
                {
                    break;
                }
            } while (nonContinentGenerationAttempts < NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT);
        } while (!IsEverythingReachable(ItemGet));
    }

    private async void UpdateProgress(Func<string, Task> progress, int v)
    {
        switch (v)
        {
            case 1:
                await progress.Invoke("Generating Palaces");
                break;
            case 2:
                await progress.Invoke("Generating Western Hyrule");
                break;
            case 3:
                await progress.Invoke("Generating Death Mountain");
                break;
            case 4:
                await progress.Invoke("Generating East Hyrule");
                break;
            case 5:
                await progress.Invoke("Generating Maze Island");
                break;
            case 6:
                await progress.Invoke("Shuffling Items and Spells");
                break;
            case 7:
                await progress.Invoke("Running Seed Completability Checks");
                break;
            case 8:
                await progress.Invoke("Generating Hints");
                break;
            case 9:
                await progress.Invoke("Finishing up");
                break;
        }
    }

    /// <param name="type">1 = raft, 2 = bridge, 3 = cave1, 4/other = cave2</param>
    private void SetTransportation(int w1, int w2, int type)
    {
        Location l1, l2;
        if (type == 1)
        {
            l1 = worlds[w1].LoadRaft(ROMData, (Continent)w1, (Continent)w2);
            l2 = worlds[w2].LoadRaft(ROMData, (Continent)w2, (Continent)w1);
            connections[0] = (l1, l2);
        }
        else if (type == 2)
        {
            l1 = worlds[w1].LoadBridge(ROMData, (Continent)w1, (Continent)w2);
            l2 = worlds[w2].LoadBridge(ROMData, (Continent)w2, (Continent)w1);
            connections[1] = (l1, l2);
        }
        else if (type == 3)
        {
            l1 = worlds[w1].LoadCave1(ROMData, (Continent)w1, (Continent)w2);
            l2 = worlds[w2].LoadCave1(ROMData, (Continent)w2, (Continent)w1);
            connections[2] = (l1, l2);
        }
        else
        {
            l1 = worlds[w1].LoadCave2(ROMData, (Continent)w1, (Continent)w2);
            l2 = worlds[w2].LoadCave2(ROMData, (Continent)w2, (Continent)w1);
            connections[3] = (l1, l2);
        }
    }

    private bool AllContinentsAreConnected((Location, Location)[] connections)
    {
        if(connections.Length != 4)
        {
            throw new ImpossibleException("Too many connections!");
        }
        foreach (var connection in connections)
        {
            if (connection.Item1 == null) { return false; }
            if (connection.Item2 == null) { return false; }
        }
        HashSet<Continent> continentsFound = [Continent.WEST];
        bool progress = true;
        while(progress == true)
        {
            progress = false;
            foreach ((Location, Location) connection in connections)
            {
                if(continentsFound.Contains(connection.Item1.Continent) && !continentsFound.Contains(connection.Item2.Continent))
                {
                    continentsFound.Add(connection.Item2.Continent);
                    progress = true;
                }
                if (continentsFound.Contains(connection.Item2.Continent) && !continentsFound.Contains(connection.Item1.Continent))
                {
                    continentsFound.Add(connection.Item1.Continent);
                    progress = true;
                }
            }
        }
        return continentsFound.Count == 4;
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
        eastHyrule.townAtOldKasuto.ActualTown = Town.OLD_KASUTO;
    }

    private void ShufflePalaces()
    {
        if (!props.PalacesCanSwapContinent) return;
        List<Location> pals = [westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3, mazeIsland.locationAtPalace4, eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6];

        if (props.P7shuffle)
        {
            pals.Add(eastHyrule.locationAtGP);
        }

        for (int i = pals.Count - 1; i > 0; i--)
        {
            int swap = r.Next(i + 1);
            Util.Swap(pals[i], pals[swap]);
        }

    }

    //ItemLocs is specifically only those locations that contain shufflable items
    //If you're looking for a global reference for which items are where... too bad it doesn't exist :(
    private List<Location> LoadItemLocs(int[] itemsPerPalaces)
    {
        List<Location> GetPalacesWithItems(IEnumerable<Location> palaces) =>
            palaces.Where(p => p.PalaceNumber is int n && n != 7 && props.PalaceItemRoomCounts[n - 1] != 0).ToList();

        // Listed somewhat in order of expected progression. This way we can
        // use the indexes of two locations to determine approx distance.

        itemLocs = new List<Location>(30);
        itemLocs = GetPalacesWithItems([
            westHyrule.locationAtPalace1,
            westHyrule.locationAtPalace2,
            westHyrule.locationAtPalace3
        ]);
        itemLocs.AddRange([
            westHyrule.grassTile,
            westHyrule.heartContainerCave,
            westHyrule.magicContainerCave,
            westHyrule.medicineCave,
            westHyrule.trophyCave,
        ]);
        if (props.PbagItemShuffle)
        {
            itemLocs.Add(westHyrule.pbagCave);
        }
        if (props.IncludeQuestItemsInShuffle)
        {
            itemLocs.Add(westHyrule.bagu);
        }
        if (props.IncludeSpellsInShuffle)
        {
            itemLocs.Add(westHyrule.locationAtRauru);
            itemLocs.Add(westHyrule.locationAtRuto);
            itemLocs.Add(westHyrule.locationAtSariaNorth);
        }
        if (props.IncludeQuestItemsInShuffle)
        {
            itemLocs.Add(westHyrule.mirrorTable);
        }
        if (props.IncludeSpellsInShuffle)
        {
            itemLocs.Add(westHyrule.locationAtMido);
        }
        if (props.IncludeSwordTechsInShuffle)
        {
            itemLocs.Add(westHyrule.midoChurch);
        }

        itemLocs.AddRange([
            deathMountain.specRock,
            deathMountain.hammerCave,
        ]);

        itemLocs.AddRange(GetPalacesWithItems([
            eastHyrule.locationAtPalace5,
            eastHyrule.locationAtPalace6,
            eastHyrule.locationAtGP
        ]));
        itemLocs.AddRange([
            eastHyrule.waterTile,
            eastHyrule.desertTile,
        ]);
        if (props.PbagItemShuffle)
        {
            itemLocs.Add(eastHyrule.pbagCave1);
            itemLocs.Add(eastHyrule.pbagCave2);
        }
        if (props.IncludeQuestItemsInShuffle)
        {
            itemLocs.Add(eastHyrule.fountain);
        }
        if (props.IncludeSpellsInShuffle)
        {
            itemLocs.Add(eastHyrule.townAtNabooru);
            itemLocs.Add(eastHyrule.townAtDarunia);
        }
        if (props.IncludeSwordTechsInShuffle)
        {
            itemLocs.Add(eastHyrule.daruniaRoof);
        }
        if (props.IncludeSpellsInShuffle)
        {
            itemLocs.Add(eastHyrule.townAtOldKasuto);
            itemLocs.Add(eastHyrule.townAtNewKasuto);
        }
        itemLocs.Add(eastHyrule.newKasutoBasement);
        itemLocs.Add(eastHyrule.spellTower);

        itemLocs.AddRange(GetPalacesWithItems([mazeIsland.locationAtPalace4]));
        itemLocs.AddRange([
            mazeIsland.childDrop,
            mazeIsland.magicContainerDrop
        ]);

        return itemLocs;
    }

    /// <summary>
    /// Shuffles which spells are in which towns. 
    /// Specifically only used when shuffle spells is on but spells are not in the shuffle pool
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ShuffleSpells()
    {
        foreach(Collectable collectable in ItemGet.Keys)
        {
            if(collectable.IsSpell())// || collectable == Collectable.UPSTAB || collectable == Collectable.DOWNSTAB)
            {
                ItemGet[collectable] = false;
            }
        }

        List<Town> unallocatedTowns = new List<Town> { Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST,
            Town.NABOORU, Town.DARUNIA_WEST, Town.NEW_KASUTO, Town.OLD_KASUTO };

        var filteredToJustSpells = Enum.GetValues(typeof(Collectable)).Cast<Collectable>().Where(i => i.IsSpell());
        foreach (Collectable spell in filteredToJustSpells)
        {
            if (props.ReplaceFireWithDash && spell == Collectable.FIRE_SPELL
                || !props.ReplaceFireWithDash && spell == Collectable.DASH_SPELL
                || spell == Collectable.UPSTAB
                || spell == Collectable.DOWNSTAB)
            {
                continue;
            }
            Town town = unallocatedTowns[r.Next(unallocatedTowns.Count)];
            Location townLocation = GetTownLocation(town);
            townLocation.Collectables = [spell];
            unallocatedTowns.Remove(town);
        }
    }

    private void SwapUpAndDownstab()
    {
        eastHyrule.daruniaRoof.Collectables = [Collectable.DOWNSTAB];
        westHyrule.midoChurch.Collectables = [Collectable.UPSTAB];
    }

    //Updated to use fisher-yates. Eventually i'll catch all of these. N is small enough here it REALLY makes a difference
    private static void ShuffleEncounters(ROM rom, Random r, List<int> addr)
    {
        for (int i = addr.Count - 1; i > 0; --i)
        {
            int swap = r.Next(i + 1);

            byte temp = rom.GetByte(addr[i]);
            rom.Put(addr[i], rom.GetByte(addr[swap]));
            rom.Put(addr[swap], temp);
        }
    }

    private void RandomizeStartingValues(RandomizerProperties props, Assembler a, Random r, ROM rom)
    {

        rom.Put(0x17AF3, (byte)props.StartAtk);
        rom.Put(0x17AF4, (byte)props.StartMag);
        rom.Put(0x17AF5, (byte)props.StartLifeLvl);

        if (props.SpellEnemy)
        {
            //3, 4, 6, 7, 14, 16, 17, 18, 24, 25, 26
            List<int> enemies = new List<int> { 3, 4, 6, 7, 0x0E, 0x10, 0x11, 0x12, 0x18, 0x19, 0x1A };
            rom.Put(0x11ef, (byte)enemies[r.Next(enemies.Count)]);
        }
        if (props.BossItem)
        {
            shuffler.ShuffleBossDrop(rom, r, a);
        }

        if (props.StartWithSpellItems)
        {
            //ROMData.Put(0xF584, 0xA9);
            //ROMData.Put(0xF585, 0x01);
            //ROMData.Put(0xF586, 0xEA);
            //Instead of patching out the checks for the spell items, actually update the default save data so you start with them.
            rom.Put(0x17b14, 0x10); //Trophy
            rom.Put(0x17b15, 0x01); //Mirror
            rom.Put(0x17b16, 0x40); //Medicine
            rom.Put(0x17b17, 0x01); //Water
            rom.Put(0x17b18, 0x20); //Child
        }

        rom.Put(ROM.ChrRomOffset + 0x1a000, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.item_sprites.chr"));
        rom.UpdateSprite(props.CharSprite, true, props.ChangeItemSprites);
        rom.UpdateSpritePalette(props.TunicColor, props.SkinTone, props.OutlineColor, props.ShieldColor, props.BeamSprite);
        rom.Put(ROM.ChrRomOffset + 0x01000, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.randomizer_text.chr"));

        if (props.EncounterRates == EncounterRate.NONE)
        {
            rom.Put(0x294, 0x60); //skips the whole routine
        }

        if (props.EncounterRates == EncounterRate.HALF)
        {
            //terrain timers
            rom.Put(0x250, 0x40);
            rom.Put(0x251, 0x30);
            rom.Put(0x252, 0x30);
            rom.Put(0x253, 0x40);
            rom.Put(0x254, 0x12);
            rom.Put(0x255, 0x06);

            //initial overworld timer
            rom.Put(0x88A, 0x10);

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
            rom.Put(0x29f, new byte[] { 0x4C, 0xAA, 0xA8 });

            rom.Put(0x28ba, new byte[] { 0xA5, 0x26, 0xD0, 0x0D, 0xEE, 0xE0, 0x06, 0xA9, 0x01, 0x2D, 0xE0, 0x06, 0xD0, 0x03, 0x4C, 0x98, 0x82, 0x4C, 0x93, 0x82 });
        }


        if (props.ShuffleLifeRefill)
        {
            int lifeRefill = r.Next(1, 6);
            rom.Put(0xE7A, (byte)(lifeRefill * 16));
        }

        if (props.ShuffleStealExpAmt)
        {
            int small = rom.GetByte(0x1E30E);
            int big = rom.GetByte(0x1E314);
            small = r.Next((int)(small - small * .5), (int)(small + small * .5) + 1);
            big = r.Next((int)(big - big * .5), (int)(big + big * .5) + 1);
            rom.Put(0x1E30E, (byte)small);
            rom.Put(0x1E314, (byte)big);
        }

        List<int> addr = new List<int>();
        if (props.ShuffleEncounters)
        {
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

            ShuffleEncounters(rom, r, addr);

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

            ShuffleEncounters(rom, r, addr);
        }

        if (props.JumpAlwaysOn)
        {
            rom.Put(0x1482, rom.GetByte(0x1480));
            rom.Put(0x1483, rom.GetByte(0x1481));
            rom.Put(0x1486, rom.GetByte(0x1484));
            rom.Put(0x1487, rom.GetByte(0x1485));

        }

        if (props.DisableMagicRecs)
        {
            rom.Put(0xF539, (byte)0xC9);
            rom.Put(0xF53A, (byte)0);
        }

        rom.SetLevelCap(a, props.AttackCap, props.MagicCap, props.LifeCap);

        rom.ChangeLevelUpCancelling(a);

        rom.Put(0x17B10, (byte)props.StartGems);

        startHearts = props.StartHearts;
        rom.Put(0x17B00, (byte)startHearts);

        maxHearts = props.MaxHearts;
        heartContainersInItemPool = maxHearts - startHearts;

        rom.Put(0x1C369, (byte)props.StartLives);

        rom.Put(0x17B12, (byte)((props.StartWithUpstab ? 0x04 : 0) + (props.StartWithDownstab ? 0x10 : 0)));

        //Swap up and Downstab
        if (props.SwapUpAndDownStab)
        {
            //Swap the ORAs that determine which stab to give you
            rom.Put(0xF4DF, 0x04);
            rom.Put(0xF4F7, 0x10);
            //Swap the ANDs that check whether or not you have the stab
            rom.Put(0xF4D3, 0x04);
            rom.Put(0xF4EB, 0x10);
        }

        if (props.ShufflePalacePalettes)
        {
            shuffler.ShufflePalacePalettes(rom, r);
        }

        if (props.ShuffleItemDropFrequency)
        {
            int drop = r.Next(5) + 4;
            rom.Put(0x1E8B0, (byte)drop);
        }
    }

    private static void ApplyBeepSettings(RandomizerProperties props, ROM rom)
    {
        //CMP      #$20                      ; 0x1d4e4 $D4D4 C9 20
        rom.Put(0x1d4e5, props.BeepThreshold);
        if (props.BeepFrequency == 0)
        {
            //C9 20 - EA 38
            //CMP 20 -> NOP SEC
            rom.Put(0x1D4E4, (byte)0xEA);
            rom.Put(0x1D4E5, (byte)0x38);
        }
        else
        {
            //LDA      #$30                      ; 0x193c1 $93B1 A9 30
            rom.Put(0x193c2, props.BeepFrequency);
        }
    }

    private void UpdateRom()
    {
        foreach (World world in worlds)
        {
            List<Location> locs = world.AllLocations;
            foreach (Location location in locs.Where(i => i.AppearsOnMap))
            {
                byte[] locationBytes = location.GetLocationBytes();
                ROMData.Put(location.MemAddress, locationBytes[0]);
                ROMData.Put(location.MemAddress + overworldXOff, locationBytes[1]);
                ROMData.Put(location.MemAddress + overworldMapOff, locationBytes[2]);
                ROMData.Put(location.MemAddress + overworldWorldOff, locationBytes[3]);
            }
            foreach (Location location in world.RemovedLocations)
            {
                ROMData.Put(location.MemAddress, 0x00);
            }
            ROMData.RemoveUnusedConnectors(world);
        }

        // Copy heart and magic container sprite tiles to the new location.
        for (int i = 0; i < 64; i++)
        {
            byte heartByte = ROMData.GetByte(ROM.ChrRomOffset + 0x7800 + i);
            ROMData.Put(ROM.ChrRomOffset + 0x09800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x0B800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x0D800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x13800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x15800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x17800 + i, heartByte);
            ROMData.Put(ROM.ChrRomOffset + 0x19800 + i, heartByte);
        }

        foreach (Palace palace in palaces)
        {
            palace.AssertItemRoomsAreUnique(ROMData);
            palace.ValidateRoomConnections();
            palace.WriteConnections(ROMData);
        }

        ROMData.Put(0x1CD3A, (byte)palGraphics[(int)westHyrule.locationAtPalace1.PalaceNumber!]);
        ROMData.Put(0x1CD3B, (byte)palGraphics[(int)westHyrule.locationAtPalace2.PalaceNumber!]);
        ROMData.Put(0x1CD3C, (byte)palGraphics[(int)westHyrule.locationAtPalace3.PalaceNumber!]);
        ROMData.Put(0x1CD46, (byte)palGraphics[(int)mazeIsland.locationAtPalace4.PalaceNumber!]);
        ROMData.Put(0x1CD42, (byte)palGraphics[(int)eastHyrule.locationAtPalace5.PalaceNumber!]);
        ROMData.Put(0x1CD43, (byte)palGraphics[(int)eastHyrule.locationAtPalace6.PalaceNumber!]);
        ROMData.Put(0x1CD44, (byte)palGraphics[(int)eastHyrule.locationAtGP.PalaceNumber!]);

        if (props.ShuffleEnemyPalettes)
        {
            List<int> doubleLocs = [0x40b4, 0x80b4, 0x100b4, 0x100b8, 0x100bc, 0x140b4, 0x140b8, 0x140bc];
            List<int> singleLocs = [0x40b8, 0x40bc, 0x80b8, 0x80bc];

            foreach (int i in doubleLocs)
            {
                int low = r.Next(12) + 1;
                int high = (r.Next(2) + 1) * 16;
                int color = high + low;
                ROMData.Put(i, (byte)color);
                ROMData.Put(i + 16, (byte)color);
                ROMData.Put(i - 1, (byte)(color - 15));
                ROMData.Put(i + 16 - 1, (byte)(color - 15));
            }
            foreach (int i in singleLocs)
            {
                int low = r.Next(13);
                int high = (r.Next(3)) * 16;
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
                    int n = r.Next(4);
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
                    int n = r.Next(4);
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
                    int n = r.Next(4);
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
                    int n = r.Next(4);
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
                    int n = r.Next(4);
                    n = n << 6;
                    ROMData.Put(i, (byte)(n + p));
                }
            }
        }

        //WRITE UPDATES TO WIZARD/QUEST COLLECTABLES HERE

        ROMData.Put(RomMap.WEST_TROPHY_CAVE_COLLECTABLE, (byte)westHyrule.trophyCave.Collectables[0]);
        ROMData.Put(RomMap.WEST_MAGIC_CONTAINER_CAVE_COLLECTABLE, (byte)westHyrule.magicContainerCave.Collectables[0]);
        ROMData.Put(RomMap.WEST_HEART_CONTAINER_CAVE_COLLECTABLE, (byte)westHyrule.heartContainerCave.Collectables[0]);
        ROMData.Put(RomMap.WEST_MEDICINE_CAVE_COLLECTABLE, (byte)westHyrule.medicineCave.Collectables[0]);
        ROMData.Put(RomMap.WEST_GRASS_TILE_COLLECTABLE, (byte)westHyrule.grassTile.Collectables[0]);
        ROMData.Put(RomMap.DM_SPECTACLE_ROCK_COLLECTABLE, (byte)deathMountain.specRock.Collectables[0]);
        ROMData.Put(RomMap.DM_HAMMER_CAVE_COLLECTABLE, (byte)deathMountain.hammerCave.Collectables[0]);
        ROMData.Put(RomMap.EAST_WATER_TILE_COLLECTABLE, (byte)eastHyrule.waterTile.Collectables[0]);
        ROMData.Put(RomMap.EAST_DESERT_TILE_COLLECTABLE, (byte)eastHyrule.desertTile.Collectables[0]);

        ROMData.ElevatorBossFix(props.BossItem);

        // Update item rooms and entrances for all palaces
        Location[] locations = [
            westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3,
            eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6, eastHyrule.locationAtGP,
            mazeIsland.locationAtPalace4,
        ];
        foreach (var loc in locations)
        {
            var idx = (int)loc.PalaceNumber! - 1;
            if (loc.PalaceNumber != 7)
            {
                for(int i = 0; i < palaces[idx].ItemRooms.Count; i++)
                {
                    ROMData.UpdateItem(loc.Collectables[i], palaces[idx].ItemRooms[i]);
                }
            }

            if (palaces[idx].Entrance == null)
            {
                throw new Exception("Invalid palace without a palace number");
            }
            var root = palaces[idx].Entrance!;
            ROMData.Put(loc.MemAddress + 0x7e, root.Map);
        }

        ROMData.Put(RomMap.EAST_SPELL_TOWER_COLLECTABLE, (byte)eastHyrule.spellTower.Collectables[0]); //map 47
        ROMData.Put(RomMap.EAST_NEW_KASUTO_BASEMENT_COLLECTABLE, (byte)eastHyrule.newKasutoBasement.Collectables[0]); //map 46

        ROMData.Put(RomMap.MI_MAGIC_CONTAINER_DROP_COLLECTABLE, (byte)mazeIsland.magicContainerDrop.Collectables[0]);
        ROMData.Put(RomMap.MI_CHILD_DROP_COLLECTABLE, (byte)mazeIsland.childDrop.Collectables[0]);

        if (props.PbagItemShuffle)
        {
            ROMData.Put(RomMap.WEST_PBAG_CAVE_COLLECTABLE, (byte)westHyrule.pbagCave.Collectables[0]);
            ROMData.Put(RomMap.EAST_PBAG_CAVE1_COLLECTABLE, (byte)eastHyrule.pbagCave1.Collectables[0]);
            ROMData.Put(RomMap.EAST_PBAG_CAVE2_COLLECTABLE, (byte)eastHyrule.pbagCave2.Collectables[0]);
        }

        foreach (Location location in pbagHearts)
        {
            if (location == westHyrule.pbagCave)
            {
                ROMData.Put(RomMap.WEST_PBAG_CAVE_COLLECTABLE, (byte)westHyrule.pbagCave.Collectables[0]);
            }

            if (location == eastHyrule.pbagCave1)
            {
                ROMData.Put(RomMap.EAST_PBAG_CAVE1_COLLECTABLE, (byte)eastHyrule.pbagCave1.Collectables[0]);
            }
            if (location == eastHyrule.pbagCave2)
            {
                ROMData.Put(RomMap.EAST_PBAG_CAVE2_COLLECTABLE, (byte)eastHyrule.pbagCave2.Collectables[0]);
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
                    // ROMData.Put(0x538, (byte)w.raft.Xpos); // Not used anymore
                    // ROMData.Put(0x53A, (byte)w.raft.Ypos); // Not used anymore
                    firstRaft = true;
                }
                else
                {
                    // ROMData.Put(0x539, (byte)w.raft.Xpos); // Not used anymore
                    // ROMData.Put(0x53B, (byte)w.raft.Ypos); // Not used anymore
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
                    ROMData.Put(0x567, (byte)w.bridge.YRaw);
                    firstRaft = true;
                }
                else
                {
                    ROMData.Put(0x564, (byte)w.bridge.Xpos);
                    ROMData.Put(0x566, (byte)w.bridge.YRaw);
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
        //ROMData.UpdateWizardText(WizardCollectables);

        // Use the vanilla spell order for the spells if the wizards aren't guaranteed to have a spell
        // This was overwhelmingly the favorite in the community vote;
        if (props.IncludeSpellsInShuffle)
        {
            //but we still need to give out starting spells
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS, props.StartShield ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 1, props.StartJump ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 2, props.StartLife ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 3, props.StartFairy ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 4, props.StartFire ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 5, props.StartReflect ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 6, props.StartSpell ? (byte)1 : (byte)0);
            ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + 7, props.StartThunder ? (byte)1 : (byte)0);
        }
        else
        {
            // Use the old spell menu behavior where the spells are placed in the menu in the location it came from
            var wizardCollectables = AllLocationsForReal()
                .Where(l => l.ActualTown != null && Towns.STRICT_SPELL_LOCATIONS.Contains((Town)l.ActualTown))
                .Select(l => l.Collectables[0]).ToList();
            
            for (int i = 0; i < magFunction.Length; i++)
            {
                magFunction[i] = ROMData.GetByte(functionBase + wizardCollectables[i].VanillaSpellOrder());
            }

            for (int i = 0; i < magEffects.Length; i = i + 2)
            {
                magEffects[i] = ROMData.GetByte(effectBase + wizardCollectables[i / 2].VanillaSpellOrder() * 2);
                magEffects[i + 1] = ROMData.GetByte(effectBase + wizardCollectables[i / 2].VanillaSpellOrder() * 2 + 1);
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    magLevels[i, j] = ROMData.GetByte(spellCostBase + (wizardCollectables[i].VanillaSpellOrder() * 8 + j));
                }

                for (int j = 0; j < 7; j++)
                {
                    magNames[i, j] = ROMData.GetByte(spellNameBase + (wizardCollectables[i].VanillaSpellOrder() * 0xe + j));
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

            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + i, props.StartsWithCollectable(wizardCollectables[i]) ? (byte)1 : (byte)0);
            }
        }
        //fix for rope graphical glitch
        for (int i = 0; i < 16; i++)
        {
            ROMData.Put(ROM.ChrRomOffset + 0x12CC0 + i, ROMData.GetByte(ROM.ChrRomOffset + 0x14CC0 + i));
        }
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
#if UNSAFE_DEBUG
                        logger.Debug("Map: " + map);
                        logger.Debug("Item: " + item);
                        logger.Debug($"Address: {addr:X}");
#endif
                        addresses.Add(addr);
                        items.Add(item);
                    }
                    j++;
                }
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            int swap = r.Next(i, items.Count);
            (items[swap], items[i]) = (items[i], items[swap]);
        }
        for (int i = 0; i < addresses.Count; i++)
        {
            ROMData.Put(addresses[i], (byte)items[i]);
        }
    }

    public void PrintDebugSpoiler(LogLevel logLevel)
    {
        logger.Log(logLevel, "ITEMS:");
        List<string> magicContainerLocations = [];
        List<string> heartContainerLocations = [];

        StringBuilder sb = new();

        foreach (Collectable item in Enum.GetValues(typeof(Collectable)))
        {
            if (item.IsItemGetItem() && ItemGet.ContainsKey(item))
            {
                Location? location = AllLocationsForReal().Where(i => i.Collectables.Contains(item)).FirstOrDefault();
                sb.AppendLine(item.ToString() + "(" + ItemGet[item] + ") : " + location?.Name);
            }
        }
        foreach(Location location in AllLocationsForReal())
        {
            for(int i = 0; i < location.Collectables.Count; i++)
            {
                if (location.Collectables[i] == Collectable.HEART_CONTAINER)
                {
                    string locationName = location.Name;
                    if(location.Collectables.Count > 1)
                    {
                        locationName += $"[{i}]";
                    }
                    heartContainerLocations.Add(locationName);
                }
                if (location.Collectables[i] == Collectable.MAGIC_CONTAINER)
                {
                    string locationName = location.Name;
                    if (location.Collectables.Count > 1)
                    {
                        locationName += $"[{i}]";
                    }
                    magicContainerLocations.Add(locationName);
                }
            }

        }
        foreach (string locationName in heartContainerLocations)
        {
            sb.AppendLine(locationName);
        }
        foreach (string locationName in magicContainerLocations)
        {
            sb.AppendLine(locationName);
        }
        logger.Log(logLevel, sb.ToString());
    }

    public string GenerateSpoiler()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Flags: " + props.Flags);
        sb.AppendLine("Seed: " + props.Seed);
        sb.AppendLine("Hash: " + Hash.Replace(" ", ""));

        sb.AppendLine(westHyrule.GenerateSpoiler());
        sb.AppendLine(deathMountain.GenerateSpoiler());
        sb.AppendLine(eastHyrule.GenerateSpoiler());
        sb.AppendLine(mazeIsland.GenerateSpoiler());

        if (musicRandomizer is not null)
        {
            //sb.AppendLine();
            sb.AppendLine(musicRandomizer.SpoilerLog);
            //sb.AppendLine();
        }

        //sb.AppendLine("West:\n");
        //sb.AppendLine(westHyrule.GetReadableMap());

        //sb.AppendLine("\nEast:\n");
        //sb.AppendLine(eastHyrule.GetReadableMap());

        //sb.AppendLine("\nDeath Mountain:\n");
        //sb.AppendLine(deathMountain.GetReadableMap());

        //sb.AppendLine("\nMaze Island:\n");
        //sb.AppendLine(mazeIsland.GetReadableMap());

        for(int i = 0; i < 6; i++)
        {
            sb.AppendLine($"\nPalace {i+1}:\n");
            sb.AppendLine(palaces[i].GetLayoutDebug(props.PalaceStyles[i], false));
        }

        sb.AppendLine("\nGP:\n");
        sb.AppendLine(palaces[6].GetLayoutDebug(props.PalaceStyles[6], false));

        sb.AppendLine("DETAILS: ");
        sb.Append(JsonSerializer.Serialize(props, SourceGenerationContext.Default.RandomizerProperties));

        return sb.ToString().Replace('$', ' ');
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

    private static void AddCropGuideBoxesToFileSelect(Assembler a)
    {
        a.Module().Code("""
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
    }

    public IEnumerable<Location> AllLocationsForReal()
    {
        List<Location>? locations = westHyrule?.AllLocations
            .Union(eastHyrule?.AllLocations ?? [])
            .Union(mazeIsland?.AllLocations ?? [])
            .Union(deathMountain?.AllLocations ?? []).ToList();
        return locations ?? [];
    }

    public IEnumerable<Location> GetNonSideviewItemLocations()
    {
        return AllLocationsForReal().Where(
            i => i.VanillaCollectable.IsSpell()
        || i.VanillaCollectable.IsQuestItem()
        || i.VanillaCollectable.IsSwordTech());
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

        //Where(i => !i.ItemGet && !i.Collectable.IsInternalUse()
        foreach (Location location in AllLocationsForReal())
        {
            foreach (Collectable collectable in location.Collectables)
            {
                if (!collectable.IsMinorItem() && !ItemGet[collectable])
                {
                    sb.AppendLine(location.Name + " / " + Enum.GetName(typeof(Collectable), collectable));
                }
                //TODO: figure out how to display individual heart/magic containers
            }
        }

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

    private void FullItemShuffle(Assembler asm, IEnumerable<Location> nonSideviewLocations)
    {
        var a = asm.Module();
        foreach (var collect in Enum.GetValues<Collectable>())
        {
            a.Set($"{collect.ToString().ToUpper()}_ITEMLOC", (int)collect);
        }
        foreach (var loc in nonSideviewLocations)
        {
            a.Set($"{loc.VanillaCollectable.ToString().ToUpper()}_ITEMLOC", (int)loc.Collectables[0]);
        }
        a.Set("_REPLACE_FIRE_WITH_DASH", props.ReplaceFireWithDash ? 1 : 0);
        a.Set("_CHECK_WIZARD_MAGIC_CONTAINER", props.DisableMagicRecs ? 0 : 1);
        a.Set("_DO_SPELL_SHUFFLE_WIZARD_UPDATE", props.IncludeSpellsInShuffle ? 1 : 0);
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.FullItemShuffle.s"), "full_item_shuffle.s");
    }
    
    private void FixHelmetheadBossRoom(Assembler asm)
    {
        byte helmetRoom = (byte)palaces[1].BossRoom!.Map;

        var a = asm.Module();
        a.Assign("HelmetRoom", helmetRoom);
        a.Code("""
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
    }

    private static void RestartWithPalaceUpA(Assembler a) {
        a.Module().Code("""
.include "z2r.inc"

update_next_level_exp = $a057

;(0=caves, enemy encounters...; 1=west hyrule towns; 2=east hyrule towns; 3=palace 1,2,5 ; 4=palace 3,4,6 ; 5=great palace)
world_number = $707
room_code = $561

.segment "PRG7"

.org $cbaa
    jsr PalacePatch

.reloc
PalacePatch:
    sta world_number
    ; if < 3 do the original patch
    cmp #$03
    bcc @Exit
    ; or if our temp flag is already set, do the original code
    lda temp_room_flag
    bne @Exit
        lda room_code
        sta temp_room_code ; store the area code into a temp ram location
        inc temp_room_flag ; set a flag in another empty ram location
@Exit:
    lda world_number
    rts

.reloc
ReloadExpForReset:
    lda temp_room_code
    sta room_code
    jsr update_next_level_exp
    lda world_number
    rts

.org $cad0
    jsr ReloadExpForReset ; Refresh the room marker and load the world number
    cmp #3                ; World 3 or greater (palaces)
    bcs $cade ; *+9       ; jump to restore/continue routine

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
    sty world_number
    sty temp_room_flag
    rts

""", "restart_palace_upa.s");
    }

    /// <summary>
    /// I assume this fixes the XP on screen transition softlock, but who knows with all these magic bytes.
    /// </summary>
    private static void FixSoftLock(Assembler a)
    {
        a.Module().Code("""
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
    }
    
    public static void ExpandedPauseMenu(Assembler a)
    {
        a.Module().Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.ExpandedPauseMenu.s"), "expand_pause.s");
    }
    
    public static void StandardizeDrops(Assembler a)
    {
        a.Module().Code("""
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
    }

    public static void PreventSideviewOutOfBounds(Assembler a)
    {
        a.Module().Code("""
; In vanilla, sideview loading routinely reads a few extra bytes past the end of sideview data,
; and if you are unlucky, it'll read a $dx byte which is valid and will make the room get blocked off
; This changes it so during background rendering, it'll properly check for bounds

.segment "PRG7"
.org $C78A ; Where the bg rendering reloads the data offset to get the next command byte
  jsr CheckIfEndOfData

.reloc
CheckIfEndOfData:
  ldy $072f ; load the current data offset
  cpy $072e ; if it is at the end of the sideview
  beq @EndOfData
    rts     ; otherwise do nothing
@EndOfData:
  pla       ; pop the return address so we can choose where to go back to
  pla
  lda #$4f  ; Set the cursor to the end of the sideview since we are outta data
  sta $010a
  jmp $c795 ; jump back to the background rendering code after where it loads the next byte
""", "prevent_sideview_oob.s");
    }

    public void SetPalacePalettes(Assembler asm)
    {
        var a = asm.Module();
        a.Assign("P1Palette", (byte)palPalettes[westHyrule?.locationAtPalace1.PalaceNumber ?? 0]);
        a.Assign("P2Palette", (byte)palPalettes[westHyrule?.locationAtPalace2.PalaceNumber ?? 1]);
        a.Assign("P3Palette", (byte)palPalettes[westHyrule?.locationAtPalace3.PalaceNumber ?? 2]);
        a.Assign("P4Palette", (byte)palPalettes[mazeIsland?.locationAtPalace4.PalaceNumber ?? 3]);
        a.Assign("P5Palette", (byte)palPalettes[eastHyrule?.locationAtPalace5.PalaceNumber ?? 4]);
        a.Assign("P6Palette", (byte)palPalettes[eastHyrule?.locationAtPalace6.PalaceNumber ?? 5]);
        a.Assign("PGreatPalette", (byte)palPalettes[eastHyrule?.locationAtGP.PalaceNumber ?? 6]);
        a.Code("""
.include "z2r.inc"

.segment "PRG7"

.org $ce32
    lda PalacePaletteOffset,y

.reloc
PalacePaletteOffset:
    .byte P1Palette, P2Palette, P3Palette, $20, $30, $30, $30, $30, P5Palette, P6Palette, PGreatPalette, $60, P4Palette
""", "set_palace_palettes.s");
    }


    public static void FixContinentTransitions(Assembler asm)
    {
        var a = asm.Module();
        a.Code("""
.include "z2r.inc"

.import SwapPRG

PRG_bank = $0769

.segment "PRG7"

; Patch switching the bank when loading the overworld
.org $cd48
    bne +
    ldy RegionNumber
    lda ExpandedRegionBankTable,y
+   sta PRG_bank
    jsr SwapPRG
    beq $cd5f ; unconditional jmp to skip the freespace
FREE_UNTIL $cd5f

.org $c506
    tay
    lda RaftWorldMappingTable,y
    asl a
    tay

.org $cd84
    tay
    lda RaftWorldMappingTable,y
    asl a
    tay

.reloc
ExpandedRegionBankTable:
    .byte $01, $01, $02, $02
.reloc
RaftWorldMappingTable:
    .byte $00, $02, $00, $02
.export RaftWorldMappingTable

.org $C265
FREE_UNTIL $C285
; Change the pointer table for Item presence to include only the low byte
; Since the high byte is always $06.
;
; Also breaking it up into two tables, one for World == 0,
; where RegionNumber is used to determine the overworld we're in, and
; one for World != 0, where the RegionNumber does not actually matter.
.reloc
bank7_Pointer_table_for_Item_Presence_World0_ByRegion:
    .byte .lobyte($0600)  ; West Caves                           (Region 0)
    .byte .lobyte($0620)  ; Death Mountain / Maze Island Caves   (Region 1)
    .byte .lobyte($0640)  ; East Caves                           (Region 2)
    .byte .lobyte($0620)  ; Death Mountain / Maze Island Caves   (Region 3)
.reloc
bank7_Pointer_table_for_Item_Presence_ByWorld: ; this is referenced as -1, as index 0 would use the table above
    .byte .lobyte($0660)  ; Towns         (World 1)
    .byte .lobyte($0660)  ; Towns         (World 2)
    .byte .lobyte($0680)  ; Palace 125    (World 3)
    .byte .lobyte($06A0)  ; Palace 346    (World 4)
    .byte .lobyte($06C0)  ; Great Palace  (World 5)

.org $c2b3
; The vanilla index calculation was 5 * RegionNumber + WorldNumber, and was
; done at $cf30 in bank 7.
;
; Maze Island has been split from Death Mountain, and has RegionNumber 3. This
; conflicts with the Great Palace, with WorldNumber 5, in the item presence
; table: 5 * 3 + 0 == 5 * 2 + 5
; That calculation no longer works, so it's split into two tables now.
    ldy WorldNumber
    jsr GetItemPresenceLowByte
    sta $00
    lda #06
    sta $01
    lda $0561
    lsr a  ; Sets the carry flag that determines which 4 bits of the item presence byte is used
    tay
    lda ($00),y
    rts
FREE_UNTIL $c2ca

.reloc
GetItemPresenceLowByte:
    beq World0 ; y = WorldNumber & zero flag is already set by caller
NonZeroWorld:
    lda bank7_Pointer_table_for_Item_Presence_ByWorld - 1,y
    rts
World0:
    ldy RegionNumber
    lda bank7_Pointer_table_for_Item_Presence_World0_ByRegion,y
    rts


.segment "PRG0"
; ** Raft tile must have index 0x29 (41) in every world **
; No other type of tile trigger should use this index
; This is currently true for vanilla and Z2R
RAFT_TILE_INDEX = $29

PLAYER_X = $74
AREA_LOCATION_INDEX = $0748
PLAYER_HAS_RAFT = $0787

.org $8528 ; we don't use this data anymore
FREE_UNTIL $8553 ; remove unused data here
; bridge connector coordinates are at $8553

.org $8599
    stx AREA_LOCATION_INDEX  ; X stashed away like in the original code
    cpx #RAFT_TILE_INDEX
    bne NotRaftTileProceed
    lda PLAYER_HAS_RAFT
    beq EndTileComparisons  ; It's a raft tile and we don't have the raft
    ; Determine raft exit direction from our overworld position instead of the vanilla table
    lda PLAYER_X
    ldx #1       ; raft going right
    cmp #20
    bcs SetVariablesForRaftTravel
    ldx #2       ; raft going left
    bpl SetVariablesForRaftTravel
FREE_UNTIL $85BD
SetVariablesForRaftTravel = $85BD
NotRaftTileProceed = $85D5
EndTileComparisons = $8601

""", "fix_continent_transitions.s");
    }

    private static void UpdateTexts(Assembler asm, List<Text> hints)
    {
        var a = asm.Module();
        // Clear out the ROM for the existing tables
        a.Free("PRG3", 0xA380, 0xB082);

        // Update the pointers to the text tables
        a.Segment("PRG3");
        a.Org(0xB423);
        a.Word(a.Symbol("Towns_in_West_Hyrule"));
        a.Word(a.Symbol("Towns_in_East_Hyrule"));

        for (var i = 0; i < hints.Count; i++) {
            var hint = hints[i];
            a.Reloc();
            a.Label($"HintText{i}");
            a.Byt(hint.EncodedText);
        }

        a.Reloc();
        a.Label("Towns_in_West_Hyrule");
        // There are 52 texts in this first table
        for (var i = 0; i < 52; i++) {
            var hint = hints[i];
            a.Word(a.Symbol($"HintText{i}"));
        }
        // and the rest are in this table
        a.Reloc();
        a.Label("Towns_in_East_Hyrule");
        for (var i = 52; i < hints.Count; i++) {
            var hint = hints[i];
            a.Word(a.Symbol($"HintText{i}"));
        }
    }

    private void AssignRealPalaceLocations(AsmModule a)
    {
        a.Assign("RealPalaceAtLocation1", (westHyrule?.locationAtPalace1.PalaceNumber ?? 1) - 1);
        a.Assign("RealPalaceAtLocation2", (westHyrule?.locationAtPalace2.PalaceNumber ?? 2) - 1);
        a.Assign("RealPalaceAtLocation3", (westHyrule?.locationAtPalace3.PalaceNumber ?? 3) - 1);
        a.Assign("RealPalaceAtLocation4", (mazeIsland?.locationAtPalace4.PalaceNumber ?? 4) - 1);
        a.Assign("RealPalaceAtLocation5", (eastHyrule?.locationAtPalace5.PalaceNumber ?? 5) - 1);
        a.Assign("RealPalaceAtLocation6", (eastHyrule?.locationAtPalace6.PalaceNumber ?? 6) - 1);
        a.Assign("RealPalaceAtLocationGP", (eastHyrule?.locationAtGP.PalaceNumber ?? 7) - 1);
    }

    public void StatTracking(RandomizerProperties props, Assembler asm)
    {
        var a = asm.Module();
        a.Segment("PRG1");
        a.Reloc();
        a.Label("TsNameList");
        // Convert and write all the names of the types of checks you can timestamp
        var allTimeStampNames = new List<string>
        {
            "Glove",
            "Raft",
            "Boots",
            "Hammer",
            "Jump",
            "Fairy",
            "Reflect",
            "Thunder",
            "Palace 1",
            "Palace 2",
            "Palace 3",
            "Palace 4",
            "Palace 5",
            "Palace 6",
            "G.Palace",
            "Ambushes",
        }.Select(s => s.ToUpper().PadRight(8, ' '));
        foreach (var name in allTimeStampNames)
        {
            a.Byt(Util.ToGameText(name).Select(x => (byte)x).ToArray());
        }

        var message = " Press Start to view stats ".ToUpper();
        a.Segment("PRG5");
        a.Reloc();
        a.Label("PressStartString");
        a.Byt(Util.ToGameText(message).Select(x => (byte)x).ToArray());
        a.Assign("PressStartStringLen", message.Length);
        AssignRealPalaceLocations(a);
        a.Set("_ALLOW_ITEM_DUPLICATES", props.AllowImportantItemDuplicates ? 1 : 0);
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.StatTracking.s"), "stat_tracking.s");
    }

    public void AddCredits(Assembler asm)
    {
        byte[] CmdText(int x, int y, string text)
        {
            byte pos = (byte)((y << 5) + (x & 0b00011111));
            var stringByte = ROM.StringToZ2Bytes(text);
            byte stringLen = (byte)stringByte.Length;
            return [0x22, pos, stringLen, .. stringByte];
        }

        byte[] header = [
            .. CmdText(7,  2,   " THANKS A MILLION"),
            0xff];
        byte[] body = [
            .. CmdText(5,  4, "ZELDA II RANDOMIZER BY"),
            .. CmdText(5,  6, "DIGSHAKE"), .. CmdText(19, 6, "ELLENDAR"),
            .. CmdText(5,  7, "JROWEBOY"), .. CmdText(21, 7, "INITSU"),
            0xff];

        var a = asm.Module();
        a.Segment("PRG5");
        a.Reloc();
        a.Label("NewCreditsHeader");
        a.Byt(header);
        a.Reloc();
        a.Label("NewCreditsBody");
        a.Byt(body);

        a.Code("""
.include "z2r.inc"

.segment "PRG5"

bank5_Pointer_table_for_End_Credits:
.org $9259 ; first entry
.org $9279
    .word NewCreditsHeader
.org $927b
    .word NewCreditsBody

.org $934F
    FREE_UNTIL $9364
.org $9364
    FREE_UNTIL $937E
""", "add_credits.s");
    }

    private void ChangeMapperToMMC5(Assembler asm, bool preventFlash, bool enableZ2Ft)
    {
        var a = asm.Module();
        a.Assign("PREVENT_HUD_FLASH_ON_LAG", preventFlash ? 1 : 0);
        a.Assign("ENABLE_Z2FT", enableZ2Ft ? 1 : 0);
        AssignRealPalaceLocations(a);
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.MMC5.s"), "mmc5_conversion.s");
    }

    private void ApplyAsmPatches(RandomizerProperties props, Assembler engine, Random r, List<Text> texts, ROM rom, StatRandomizer randomizedStats)
    {
        bool randomizeMusic = !props.DisableMusic && props.RandomizeMusic;

        ChangeMapperToMMC5(engine, props.DisableHUDLag, randomizeMusic); // will make output vary with customize tab options
        rom.AddRandomizerToTitle(engine);
        AddCropGuideBoxesToFileSelect(engine);
        FixHelmetheadBossRoom(engine);
        FullItemShuffle(engine, GetNonSideviewItemLocations());
        rom.DontCountExpDuringTalking(engine);
        rom.FixElevatorPositionInFallRooms(engine);
        rom.AllowForChangingDoorYPosition(engine);
        rom.AllowForChangingElevatorYPosition(engine);
        rom.InstantText(engine);
        rom.ChangeLavaKillPosition(engine);
        rom.FixItemPickup(engine);
        rom.FixMinibossGlitchyAppearance(engine);
        rom.FixBossKillPaletteGlitch(engine);
        StatTracking(props, engine);
        AddCredits(engine);

        if (props.ShuffleBossHP != EnemyLifeOption.VANILLA)
        {
            rom.SetBossHpBarDivisors(engine, randomizedStats);
        }

        if (props.DripperEnemyOption != DripperEnemyOption.ONLY_BOTS)
        {
            EnemiesPalace125[] dripperEnemies;
            switch (props.DripperEnemyOption)
            {
                case DripperEnemyOption.ANY_GROUND_ENEMY:
                    dripperEnemies = Enemies.Palace125GroundEnemies;
                    break;
                case DripperEnemyOption.EASIER_GROUND_ENEMIES:
                case DripperEnemyOption.EASIER_GROUND_ENEMIES_FULL_HP:
                    dripperEnemies = [
                        ..Enemies.Palace125SmallEnemies,
                        EnemiesPalace125.TINSUIT,
                        EnemiesPalace125.ORANGE_IRON_KNUCKLE,
                        EnemiesPalace125.RED_STALFOS,
                        EnemiesPalace125.BLUE_STALFOS, // will use non-armored variant HP
                    ];
                    break;
                default:
                    throw new ImpossibleException();
            }
            byte dripperId = (byte)dripperEnemies[r.Next(dripperEnemies.Length)];
            rom.Put(RomMap.DRIPPER_ID, dripperId);

            if (props.DripperEnemyOption == DripperEnemyOption.EASIER_GROUND_ENEMIES_FULL_HP)
            {
                var dripperHp = randomizedStats.Palace125EnemyHpTable[dripperId];
                rom.SetDripperHp(engine, dripperHp);
            }
        }

        if (props.AttackEffectiveness == AttackEffectiveness.OHKO)
        {
            rom.UseOHKOMode(engine);
        }

        if (props.Global5050JarDrop)
        {
            rom.Global5050Jar(engine);
        }

        if (props.ReduceDripperVariance)
        {
            rom.ReduceDripperVariance(engine);
        }

        if (props.RandomizeKnockback)
        {
            rom.RandomizeKnockback(engine, r);
        }

        if (props.HardBosses)
        {
            rom.BuffCarrock(engine);
        }

        if (props.ReplaceFireWithDash)
        {
            rom.DashSpell(engine);
        }

        if (props.UpARestartsAtPalaces)
        {
            RestartWithPalaceUpA(engine);
        }

        if (props.RevealWalkthroughWalls)
        {
            rom.RevealWalkthroughWalls();
        }

        if (props.StandardizeDrops)
        {
            StandardizeDrops(engine);
        }
        FixSoftLock(engine);
        
        RandomizeStartingValues(props, engine, r, rom);
        rom.FixStaleSaveSlotData(engine);
        rom.UseExtendedBanksForPalaceRooms(engine);
        rom.ExtendMapSize(engine);
        ExpandedPauseMenu(engine);
        SetPalacePalettes(engine);
        FixContinentTransitions(engine);
        PreventSideviewOutOfBounds(engine);

        // things that depend on customize tab options below
        ApplyBeepSettings(props, rom);
        if (props.RemoveFlashing)
        {
            rom.DisableFlashing();
        }
        if (props.UpAC1)
        {
            rom.UpAController1(engine);
        }

        if (!props.DisableMusic && randomizeMusic)
        {
            rom.ApplyIps(
                Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2rndft.ips"));
            // really hacky workaround but i don't want to recompile z2ft
            // z2ft is compiled using an old address for NmiBankShadow8 and NmiBankShadowA so rather than
            // recompile that, I'm just gonna patch it here...
            const byte LDA_ABS = 0xAD;
            const byte STA_ABS = 0x8D;

            void PatchAddress(byte opcode, int before, int after)
            {
                var idx = 0;
                var needle = new ReadOnlySpan<byte>([opcode, (byte)before, (byte)(before >> 8)]);
                while (rom.rawdata.AsSpan(idx).IndexOf(needle) is var di and >= 0)
                {
                    rom.Put(idx + di, opcode, (byte)after, (byte)(after >> 8));
                    idx += di + 1;
                }
            }

            PatchAddress(LDA_ABS, 0x07b2, 0x6380);
            PatchAddress(LDA_ABS, 0x07b3, 0x6381);
            PatchAddress(STA_ABS, 0x07b2, 0x6380);
            PatchAddress(STA_ABS, 0x07b3, 0x6381);

            var asm = engine.Module();
            asm.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.z2ft.s"), "z2ft.s");
        }

        UpdateTexts(engine, texts);
    }

    //This entire town location shuffle structure is awful if this method needs to exist.
    private Location GetTownLocation(Town town)
    {
        if(westHyrule.locationAtMido.ActualTown == town)
        {
            return westHyrule.locationAtMido;
        }
        if (westHyrule.locationAtRauru.ActualTown == town)
        {
            return westHyrule.locationAtRauru;
        }
        if (westHyrule.locationAtRuto.ActualTown == town)
        {
            return westHyrule.locationAtRuto;
        }
        if (westHyrule.locationAtSariaNorth.ActualTown == town)
        {
            return westHyrule.locationAtSariaNorth;
        }
        if (eastHyrule.townAtDarunia.ActualTown == town)
        {
            return eastHyrule.townAtDarunia;
        }
        if (eastHyrule.townAtNabooru.ActualTown == town)
        {
            return eastHyrule.townAtNabooru;
        }
        if (eastHyrule.townAtNewKasuto.ActualTown == town)
        {
            return eastHyrule.townAtNewKasuto;
        }
        if (eastHyrule.townAtOldKasuto.ActualTown == town)
        {
            return eastHyrule.townAtOldKasuto;
        }
        throw new Exception("Unrecognized town in GetTownLocation, was this extended to non-standard wizards?");
    }
}
