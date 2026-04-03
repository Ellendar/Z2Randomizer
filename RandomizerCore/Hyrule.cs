using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NLog;
using js65;
using static js65.AsmModule;
using FtRandoLib.Importer;
using Z2Randomizer.RandomizerCore.Diagnostics;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore;

public readonly record struct RandomizerResult(
    bool success,
    byte[]? romdata = null,
    string? debuginfo = null,
    string? messages = null);

public enum ProgressEnum
{
    GENERATING_PALACES = 1,
    PROCESSING_OVERWORLD,
    GENERATING_WEST_HYRULE,
    GENERATING_DEATH_MOUNTAIN,
    GENERATING_EAST_HYRULE,
    GENERATING_MAZE_ISLAND,
    SHUFFLING_ITEMS_AND_SPELLS,
    RUNNING_COMPLETABILITY_CHECKS,
    SHUFFLING_ENEMIES,
    GENERATING_HINTS,
    APPLYING_PATCHES,
    LINKING_ASSEMBLY,
    FINISHING_UP,
}
public class Hyrule
{
    public delegate Assembler NewAssemblerFn(Js65Options? options = null, bool debugJavaScript = false);

    //IMPORTANT: Tuning these factors can have a big impact on generation times.
    //In general, the non-terrain shuffle features are much cheaper than the cost of generating terrain or verifying the seed.

    //This controls how many attempts will be made to shuffle the non-terrain features like towns, spells, and items.
    //The higher you set it, the more likely a given terrain is to find a set of items that works, resulting in fewer terrain generations.
    //It will also increase the number of seeds that have more arcane solutions, where only a specific item route works.
    //This was originally set to 10, but increasing it to 100 massively reduces the number of extremely degenerate caldera and mountain generation times
    private const int NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT = 100;

    private const int NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT = 10;

    private const int TERRAIN_GENERATION_ATTEMPT_LIMIT = 50000;

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private MusicRandomizer? musicRandomizer = null;

    private readonly int[] palPalettes = [0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60];
    private readonly int[] palGraphics = [0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06];

    private const int overworldXOff = 0x3F;
    private const int overworldMapOff = 0x7E;
    private const int overworldWorldOff = 0xBD;

    public HashSet<String> reachableAreas;
    //Continent connectors
    protected (Location, Location)[] connections = new (Location, Location)[4];

    private readonly ReachabilitySimulation reachability;

    public GettableItemSet GettableItems { get; private set; }
    private List<Collectable> spellListOrder { get; set; }

    public WestHyrule westHyrule;
    public EastHyrule eastHyrule;
    private MazeIsland mazeIsland;
    private DeathMountain deathMountain;
    private RandomizerProperties props;
    public List<World> worlds;
    public List<Palace> palaces;
    public List<Room> rooms;
    private Location? startLocation;
    public StatRandomizer randomizedStats;
    public DropRandomizer randomizedDrops;

    private static readonly Collectable[] minorItems = [Collectable.BLUE_JAR, Collectable.RED_JAR, Collectable.SMALL_BAG,
            Collectable.MEDIUM_BAG, Collectable.LARGE_BAG, Collectable.XL_BAG, Collectable.ONEUP, Collectable.KEY];
    private static readonly Collectable[] minorItemsMinusKey = [Collectable.BLUE_JAR, Collectable.RED_JAR, Collectable.SMALL_BAG,
            Collectable.MEDIUM_BAG, Collectable.LARGE_BAG, Collectable.XL_BAG, Collectable.ONEUP];

    private static readonly Collectable[] spellItems = [Collectable.TROPHY, Collectable.MEDICINE, Collectable.CHILD, Collectable.MIRROR, Collectable.WATER];


    // Statistics consumed by Statistics/Result.cs. Reachability failures are also
    // reported by the compile-time-gated diagnostics inside IsEverythingReachable.
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

    public int timeSpentBuildingWest = 0;
    public int timeSpentBuildingEast = 0;
    public int timeSpentBuildingDM = 0;
    public int timeSpentBuildingMI = 0;

    public ROM ROMData { get; set; }
    public RandomizerCore.Random r { get; set; }
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
        lineContinuations = true,
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
    private readonly RoomPoolSpec? roomPool;

    // HACK: A few places in the app need to check if we are using mario mode, and i didn't feel like refactoring them
    // to have another bool parameter passed in.
    public static bool Z2MarioModeEnabled = false;
    // In general, we will distribute the randomizer app in release mode, so this should always be true for players.
    // But for z2mario as a romhack, I build in debug mode, so this just removes the "RANDOMIZER" text in the title.
#if DEBUG
    const bool IS_RANDOMIZED = true;
#else
    const bool IS_RANDOMIZED = true;
#endif

    public string Hash { get; private set; }

    //This entire class structure is hot fucking garbage, but refactoring it such that it makes sense is
    //going to be a massive project, so for now we're just ignoring the fact that basically every property
    //is not guaranteed to initialize to the analyzer
#pragma warning disable CS8618
    public Hyrule(NewAssemblerFn createAsm, PalaceRooms rooms, RoomPoolSpec? roomPoolSpec)
#pragma warning restore CS8618
    {
        NewAssembler = createAsm;
        palaceRooms = rooms;
        roomPool = roomPoolSpec;
        reachability = new(ItemLocations);
    }
    public async Task<RandomizerResult> Randomize(byte[] vanillaRomData, RandomizerConfiguration config, Func<string, Task> progress, CancellationToken ct)
    {
        try
        {
            Hash = "";
            World.ResetStats();
            Z2MarioModeEnabled = config.MarioMode;

            SeedHash = MD5Hash.ComputeHashInt(Encoding.UTF8.GetBytes(config.Seed!));
            r = new Random(SeedHash);
            bool shareSeedAcrossDifficulty = config.ShareSeedAcrossDifficulty;
            Random? difficultyRng = shareSeedAcrossDifficulty ? CreateDifficultyRng(config.Seed) : null;

#if Z2R_DIAGNOSTICS
            using var diagnostics = new Diag.Generation(r, difficultyRng);
            reachability.ResetDiagnostics();
            Diag.Trace("seed", $"event=begin seed={config.Seed} flags={config.SerializeFlags()}");
            diagnostics.Stage("config");
#endif
            config.CheckForFlagConflicts();
            props = config.Export(r, includeDifficulty: !shareSeedAcrossDifficulty);
            //To make sure there isn't any similarity between the spoiler and non-spoiler versions of the seed, spin the RNG a bit.
            if (config.GenerateSpoiler)
            {
                r.NextBytes(new byte[64]);
                difficultyRng?.NextBytes(new byte[64]);
            }
#if UNSAFE_DEBUG
            string export = JsonSerializer.Serialize(props, SourceGenerationContext.Default.RandomizerProperties);
            Debug.WriteLine(export);
#endif
            Flags = config.SerializeFlags();

            // If shared difficulty is on, we need a stripped version of the
            // flags for the shared part of the seed hash.
            string sharedSeedFlags = shareSeedAcrossDifficulty ? config.SerializeSharedSeedFlags() : Flags;

            using Assembler assembler = CreateAssemblyEngine();
            logger.Info($"Started generation for flags: {Flags} sharedseedflags: {sharedSeedFlags} seed: {config.Seed} seedhash: {SeedHash}");
            //character = new Character(props);

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("palaces");
#endif
            palaces = [];
            GettableItems = new GettableItemSet.Builder()
                .Excluding(
                    props.ReplaceFireWithDash ? Collectable.FIRE_SPELL : Collectable.DASH_SPELL,
                    props.MarioMode ? Collectable.JUMP_SPELL : Collectable.TANOOKI)
                .Excluding(props.RemoveItems)
                .StartingWith(Enum.GetValues<Collectable>().Where(collectable => props.StartsWithCollectable(collectable)))
                .Build();

            reachableAreas = new HashSet<string>();

            ROM.ValidateVanillaRom(ref vanillaRomData);
            // Make a copy of the vanilla data to prevent seed bleed
            ROMData = new ROM(vanillaRomData.ToArray(), true);

            bool raftIsRequired = IsRaftAlwaysRequired(props);
            bool passedValidation = false;
            HashSet<int> freeBanks = [];
            if (ct.IsCancellationRequested)
            {
                Diag.Trace("seed", "event=cancelled");
                return new RandomizerResult(false);
            }
            UpdateProgress(progress, ProgressEnum.GENERATING_PALACES);

#if Z2R_DIAGNOSTICS
            long palaceAttempts = 0;
#endif
            while (palaces.Count != 7 || passedValidation == false)
            {
#if Z2R_DIAGNOSTICS
                Diag.Retry("palace.retry", ++palaceAttempts, $"event=begin count={palaces.Count} packed={passedValidation}");
#endif
                freeBanks = new(ROM.FreeRomBanks);
                var palaceGenerator = new Palaces();
                palaces = await palaceGenerator.CreatePalaces(r, props, palaceRooms, roomPool, raftIsRequired, ct);

                if (palaces.Count == 0)
                {
                    continue;
                }

                //Randomize Enemies
                if (props.ShufflePalaceEnemies)
                {
                    palaces.ForEach(i => i.RandomizeEnemies(props, r));
                }

                if (props.RandomizeSmallItems || props.ExtraKeys)
                {
                    RandomizeSmallPalaceItems(1);
                    RandomizeSmallPalaceItems(2);
                    RandomizeSmallPalaceItems(3);
                    RandomizeSmallPalaceItems(4);
                    RandomizeSmallPalaceItems(5);
                    RandomizeSmallPalaceItems(6);
                    //Small items in GP shouldn't be randomized. This is intentional to keep
                    //the randomization in line with the original "shuffle" behavior
                    //palaces[6].RandomizeSmallItems(RNG, props.ExtraKeys);
                }

                AsmModule sideviewModule = new();
                passedValidation = await FillPalaceRooms(sideviewModule);

                Diag.Trace("palace", $"event=packed count={palaces.Count} valid={passedValidation}");
                assembler.Add(sideviewModule);
            }

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("drops");
#endif
            ROMData.WriteKasutoJarAmount(props.NewKasutoBasementRequirement);
            ROMData.DoHackyFixes();
            ROMData.AdjustGpProjectileDamage();

            randomizedDrops = new(ROMData, props);
            randomizedDrops.Randomize(r);
            randomizedDrops.Write(ROMData);

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

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("overworld");
#endif
            firstProcessOverworldTimestamp = DateTime.Now;
            await ProcessOverworld(progress, ct);

            if (ct.IsCancellationRequested)
            {
                Diag.Trace("seed", "event=cancelled");
                return new RandomizerResult(false);
            }
            UpdateProgress(progress, ProgressEnum.SHUFFLING_ENEMIES);

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("overworld-enemies");
#endif
            if (props.ShuffleOverworldEnemies)
            {
                // move a PRG1 background map to make space for more enemy data
                ROMData.RelocateData(assembler, 1, 0x8000);
                OverworldEnemyShuffler.Shuffle(worlds, assembler, ROMData, props.MixLargeAndSmallEnemies, props.GeneratorsAlwaysMatch, r);
            }

            if (props.LinkedFireSpell != null)
            {
                ROMData.CombineFireSpell(assembler, props.LinkedFireSpell.Value, spellListOrder);
            }

            if (props.StartsWithCollectable(westHyrule.ruto.Town?.GetWizard()?.Collectable))
            {
                ROMData.Put(0x17b14, 0x10); //Trophy
            }
            if (props.StartsWithCollectable(westHyrule.sariaNorth.Town?.GetWizard()?.Collectable))
            {
                ROMData.Put(0x17b15, 0x01); //Mirror
            }
            if (props.StartsWithCollectable(westHyrule.mido.Town?.GetWizard()?.Collectable))
            {
                ROMData.Put(0x17b16, 0x40); //Medicine
            }
            if (props.StartsWithCollectable(eastHyrule.nabooru.Town?.GetWizard()?.Collectable))
            {
                ROMData.Put(0x17b17, 0x01); //Water
            }
            if (props.StartsWithCollectable(eastHyrule.darunia.Town?.GetWizard()?.Collectable))
            {
                ROMData.Put(0x17b18, 0x20); //Child
            }
            if (props.StartMagicContainers >= props.NewKasutoBasementRequirement)
            {
                ROMData.Put(0x17b19, 0x08); // give New Kasuto Basement access at start
            }

            if (ct.IsCancellationRequested)
            {
                Diag.Trace("seed", "event=cancelled");
                return new RandomizerResult(false);
            }
            UpdateProgress(progress, ProgressEnum.GENERATING_HINTS);

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("hints-stats");
#endif
            List<Text> texts = CustomTexts.GenerateTexts(ItemLocations(), ROMData.GetGameText(), props, r);
            randomizedStats = new(ROMData, props);
            randomizedStats.Randomize(r, skipDifficultyOnly: shareSeedAcrossDifficulty);

            // Apply difficulty after shared randomization, then let ApplyAsmPatches see full
            // props. A second ApplyAsm pass would re-claim PRG4/PRG5 free space (js65 sees it
            // as fully free) and clobber first-pass code like the palace elevator routines.
#if Z2R_DIAGNOSTICS
            diagnostics.Stage("difficulty");
#endif
            byte[]? sharedRngState = null;
            if (shareSeedAcrossDifficulty)
            {
                sharedRngState = new byte[32];
                r.NextBytes(sharedRngState);

                config.ApplyDifficultyOnlySettings(props, difficultyRng!);
                ReplaceDifficultyStartingItemsInWorld(difficultyRng!);
                randomizedStats.RandomizeDifficultyOnly(difficultyRng!);
            }

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("customization");
#endif
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

            UpdateProgress(progress, ProgressEnum.APPLYING_PATCHES);
#if Z2R_DIAGNOSTICS
            diagnostics.Stage("patches");
#endif
            ApplyAsmPatches(props, assembler, r, texts, ROMData, randomizedStats, startLocation!);

            UpdateProgress(progress, ProgressEnum.LINKING_ASSEMBLY);
#if Z2R_DIAGNOSTICS
            diagnostics.Stage("link");
#endif
            var rom = await ROMData.ApplyAsm(assembler);
            if (!rom.success)
            {
                Diag.Block("seed", $"event=failed reason=link messages={string.Join(";", rom.messages)}");
                return new RandomizerResult(false, null, null, string.Join(Environment.NewLine, rom.messages.Select(x => x.ToString())));
            }
            UpdateProgress(progress, ProgressEnum.FINISHING_UP);
            ROMData = new ROM(rom.romdata);

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("music");
#endif
            if (randomizeMusic)
            {
                string musicDir = "Music";
                List<string> jsonLibPaths = new(),
                    yamlLibPaths = new();
                if (Directory.Exists(musicDir))
                {
                    var jsonExts = Importer.JsonExtensions();
                    var yamlExts = Importer.YamlExtensions();

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
                    Diag.Trace("music.retry", $"attempt={9 - triesLeft}/8");
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
#if Z2R_DIAGNOSTICS
                Diag.Trace("rng", $"stage=music stream=music draws={musicRng.DrawCount} state={musicRng.GetState()}");
#endif
            }

#if Z2R_DIAGNOSTICS
            diagnostics.Stage("finalize");
#endif
            UpdateRom();

            byte[] z2Hash;
            if (shareSeedAcrossDifficulty)
            {
                byte[] sharedHash = CalculateHash(sharedSeedFlags, SeedHash, randoRomHash, sharedRngState!);

                byte[] difficultyRngState = new byte[32];
                difficultyRng!.NextBytes(difficultyRngState);
                byte[] finalHash = CalculateHash(Flags, SeedHash, randoRomHash, difficultyRngState);
                z2Hash = CombineHashes(sharedHash, finalHash);
            }
            else
            {
                byte[] finalRngState = new byte[32];
                r.NextBytes(finalRngState);
                byte[] finalHash = CalculateHash(Flags, SeedHash, randoRomHash, finalRngState);
                z2Hash = ConvertHash(finalHash);
                SanitizeHashCharacters(z2Hash);
            }

            if (Z2MarioModeEnabled && !IS_RANDOMIZED)
            {
                z2Hash = Util.ToGameText("VER. 2.0.0 ");
            }
            ROMData.Put(0x17C2C, z2Hash);
            Hash = Util.FromGameText(z2Hash);
            Diag.Trace("seed", $"event=success seed={config.Seed} hash={Hash}");

            return new RandomizerResult(true, ROMData.rawdata, rom.mlbfile, string.Join(Environment.NewLine, rom.messages.Select(x => x.ToString())));
        }
        catch (Exception e)
        {
            logger.Error(e);
            Diag.Block("seed", $"event=exception {e}");
            throw;
        }

        void RandomizeSmallPalaceItems(int palaceNumber)
        {
            palaces[palaceNumber - 1].RandomizeSmallItems(r, props.ExtraKeys, props.KeyLogic == KeyLogicOption.KEYS_PER_PALACE_SOLVABLE);
        }
    }

    private Assembler CreateAssemblyEngine()
    {
        var asm = NewAssembler(assemblerOptions, false);
        asm.Callbacks = new Js65Callbacks
        {
            OnFileResolveText = AsmFileResolveTextCallback,
        };

        asm.Module().Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.Init.s"), "__init.s");

        return asm;
    }

    private Js65ResolvedText? AsmFileResolveTextCallback(IReadOnlyList<string> basePaths, string path)
    {
        return new Js65ResolvedText(0, Util.ReadResource($"Z2Randomizer.RandomizerCore.Asm.{path.Replace('/', '.').Replace('\\', '.')}"));
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

    private static byte[] CalculateHash(string flags, int seedHash, byte[] romHash, byte[] rngState)
    {
        return MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(
            flags +
            seedHash +
            romHash +
            Util.ByteArrayToHexString(rngState)
        ));
    }

    private static Random CreateDifficultyRng(string? seed)
    {
        int difficultySeedHash = MD5Hash.ComputeHashInt(Encoding.UTF8.GetBytes($"{seed}:difficulty"));
        return new Random(difficultySeedHash);
    }

    private static byte[] CombineHashes(byte[] sharedHash, byte[] finalHash)
    {
        byte[] sharedZ2Hash = ConvertHash(sharedHash);
        byte[] finalZ2Hash = ConvertHash(finalHash);
        SanitizeHashCharacters(sharedZ2Hash);
        SanitizeHashCharacters(finalZ2Hash);

        return [
            sharedZ2Hash[0], 0xF4,
            sharedZ2Hash[2], 0xF4,
            sharedZ2Hash[4], 0xF4,
            sharedZ2Hash[6], 0xF4,
            finalZ2Hash[8], 0xF4,
            finalZ2Hash[10]
        ];
    }

    private static void SanitizeHashCharacters(byte[] z2Hash)
    {
        for (int i = 0; i < z2Hash.Length; i++)
        {
            if (z2Hash[i] == 0xD0)
            {
                z2Hash[i] = 0xF0;
            }
            if (z2Hash[i] == 0xD6)
            {
                z2Hash[i] = 0xF1;
            }
        }
    }

    // In shared-seed mode, candle/cross are placed in the world during the
    // shared shuffle (StartCandle/StartCross were false at Export time). If
    // the final difficulty flags have the player starting with either item,
    // swap that item's world pickup for a minor item so the seed doesn't
    // hand out a duplicate.
    private void ReplaceDifficultyStartingItemsInWorld(Random r)
    {
        Collectable[] difficultyStartingItems = [Collectable.CANDLE, Collectable.CROSS];
        Collectable[] minorItems = Enum.GetValues<Collectable>().Where(i => i.IsMinorItem()).ToArray();
        foreach (Collectable item in difficultyStartingItems)
        {
            if (!props.StartsWithCollectable(item)) { continue; }
            Collectable replacementItem = minorItems.Sample(r);
            foreach (Location location in ItemLocations())
            {
                if (location.ReplaceCollectable(item, replacementItem))
                {
                    return;
                }
            }
        }
    }

    private void ShufflePalaces()
    {
        if (!props.PalacesCanSwapContinent)
            return;

        List<Location> palaces = [westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3, mazeIsland.locationAtPalace4, eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6];

        if (props.P7shuffle)
        {
            palaces.Add(eastHyrule.locationAtGP);
        }

        if (props.StartingLocation is StartingLocation.GREAT_PALACE)
        {
            var gpLocation = palaces.FirstOrDefault(loc => loc.Palace?.Number == 7);
            if (gpLocation != null)
            {
                var westPalaces = palaces.Where(loc => loc.Continent is Continent.WEST).ToList();
                if (westPalaces.Count > 0)
                {
                    var gpSwapLocation = westPalaces[r.Next(westPalaces.Count)];
                    palaces.Remove(gpSwapLocation);
                    (gpLocation.Palace, gpSwapLocation.Palace) = (gpSwapLocation.Palace, gpLocation.Palace);
                }
            }
        }

        for (int i = palaces.Count - 1; i > 0; i--)
        {
            int swap = r.Next(i + 1);
            (palaces[i].Palace, palaces[swap].Palace) = (palaces[swap].Palace, palaces[i].Palace);
        }
    }

    private void ShuffleItems()
    {
        ImmutableList<Location> possibleItemLocations = ItemLocations().ToImmutableList();
        List<Location> palaceLocations = AllLocations().Where(i => i.Palace != null).ToList();

        spellListOrder = [Collectable.SHIELD_SPELL,
            props.MarioMode ? Collectable.TANOOKI : Collectable.JUMP_SPELL,
            Collectable.LIFE_SPELL,
            Collectable.FAIRY_SPELL,
            props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL,
            Collectable.REFLECT_SPELL,
            Collectable.SPELL_SPELL,
            Collectable.THUNDER_SPELL];

        //Non-pbag, non-wizard, non-palace items are inherently in the global shuffle pool
        List<Collectable> shufflableItems = [Collectable.TROPHY, Collectable.MEDICINE, Collectable.HEART_CONTAINER, Collectable.MAGIC_CONTAINER,
            Collectable.HEART_CONTAINER, Collectable.HAMMER, Collectable.MAGIC_CONTAINER, Collectable.HEART_CONTAINER, Collectable.HEART_CONTAINER,
            Collectable.MAGIC_CONTAINER, Collectable.MAGIC_KEY, Collectable.MAGIC_CONTAINER, Collectable.CHILD];
        List<Location> globalShuffleLocations = [westHyrule.trophyCave, westHyrule.medicineCave, westHyrule.heartContainerCave, westHyrule.magicContainerCave,
            westHyrule.grassTile, deathMountain.hammerCave, deathMountain.specRock, eastHyrule.desertTile, eastHyrule.waterTile, eastHyrule.newKasuto,
            mazeIsland.childDrop, mazeIsland.magicContainerDrop];

        //Items that have no location of their own and must be put in a location before the shuffle
        List<Collectable> overworldExcessItems = [];

        if (props.PbagItemShuffle)
        {
            Collectable collectable = (Collectable)ROMData.GetByte(RomMap.WEST_PBAG_CAVE_COLLECTABLE);
            westHyrule.pbagCave.SetCollectables([collectable]);
            shufflableItems.Add(collectable);

            collectable = (Collectable)ROMData.GetByte(RomMap.WEST_PBAG_CAVE_COLLECTABLE);
            eastHyrule.pbagCave1.SetCollectables([collectable]);
            shufflableItems.Add(collectable);

            collectable = (Collectable)ROMData.GetByte(RomMap.WEST_PBAG_CAVE_COLLECTABLE);
            eastHyrule.pbagCave2.SetCollectables([collectable]);
            shufflableItems.Add(collectable);
            globalShuffleLocations.Add([westHyrule.pbagCave, eastHyrule.pbagCave1, eastHyrule.pbagCave2]);
        }
        else
        {
            westHyrule.pbagCave.SetCollectables([Collectable.LARGE_BAG]);
            eastHyrule.pbagCave1.SetCollectables([Collectable.XL_BAG]);
            eastHyrule.pbagCave2.SetCollectables([Collectable.XL_BAG]);
        }

        bool canReplaceStartingSpellsWithMinorItems = !props.ShuffleSpellLocations;
        if (props.IncludeSpellsInShuffle)
        {
            shufflableItems.Add(Collectable.SHIELD_SPELL);
            shufflableItems.Add(props.MarioMode ? Collectable.TANOOKI : Collectable.JUMP_SPELL);
            shufflableItems.Add(Collectable.LIFE_SPELL);
            shufflableItems.Add(Collectable.FAIRY_SPELL);
            shufflableItems.Add(props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL);
            shufflableItems.Add(Collectable.REFLECT_SPELL);
            shufflableItems.Add(Collectable.SPELL_SPELL);
            shufflableItems.Add(Collectable.THUNDER_SPELL);
            globalShuffleLocations.Add([westHyrule.rauru, westHyrule.ruto, westHyrule.sariaNorth, westHyrule.mido,
                eastHyrule.nabooru, eastHyrule.darunia, eastHyrule.newKasuto, eastHyrule.oldKasuto]);
        }
        else if (props.ShuffleSpellLocations)
        {
            ShuffleSpells();
            //Mario's spell list is inherently always vanilla (until we either update mario to not break with a shuffled spell list)
            //or we add the option to choose vanilla spell order, in which case properties should always set that to true for mario
            //and this conditional should be that one
            if (!props.MarioMode)
            {
                spellListOrder = possibleItemLocations.Where(i => i.Town != null && i?.Town?.Type != TownType.BAGU)
                    .OrderBy(i => (int)i.Town!.Type!)
                    .Select(i => (Collectable)i.Town!.GetWizard()!.Collectable!)
                    .ToList();
            }
        }


        if (props.IncludeBagusNoteInShuffle)
        {
            shufflableItems.Add(Collectable.BAGUS_NOTE);
            globalShuffleLocations.Add(westHyrule.bagu);
        }

        if (props.IncludeQuestItemsInShuffle)
        {
            if (props.TownQuestLocationsAreMinorItems)
            {
                ReplaceWithUnshuffledMinorItem(westHyrule.sariaNorth.Town!.GetTownMap(VanillaTownMap.SARIA_TABLE)!);
                ReplaceWithUnshuffledMinorItem(eastHyrule.nabooru.Town!.GetTownMap(VanillaTownMap.NABOORU_MID)!);
                if (!props.StartsWithCollectable(Collectable.MIRROR)) { overworldExcessItems.Add(Collectable.MIRROR); }
                if (!props.StartsWithCollectable(Collectable.WATER)) { overworldExcessItems.Add(Collectable.WATER); }
            }
            else
            {
                shufflableItems.Add(props.StartsWithCollectable(Collectable.MIRROR) ? minorItems.Sample(r) : Collectable.MIRROR);
                shufflableItems.Add(props.StartsWithCollectable(Collectable.WATER) ? minorItems.Sample(r) : Collectable.WATER);
                globalShuffleLocations.AddRange([westHyrule.sariaNorth, eastHyrule.nabooru]);
            }
        }

        if (props.IncludeSwordTechsInShuffle)
        {
            if (props.TownQuestLocationsAreMinorItems)
            {
                ReplaceWithUnshuffledMinorItem(westHyrule.mido.Town!.GetTownMap(VanillaTownMap.MIDO_TRAINER)!);
                ReplaceWithUnshuffledMinorItem(eastHyrule.darunia.Town!.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!);
                if (!props.StartWithDownstab) { overworldExcessItems.Add(Collectable.DOWNSTAB); }
                if (!props.StartWithUpstab) { overworldExcessItems.Add(Collectable.UPSTAB); }
            }
            else
            {
                shufflableItems.Add(props.StartWithDownstab ? minorItems.Sample(r) : Collectable.DOWNSTAB);
                shufflableItems.Add(props.StartWithUpstab ? minorItems.Sample(r) : Collectable.UPSTAB);
                globalShuffleLocations.AddRange([westHyrule.mido, eastHyrule.darunia]);
            }
        }
        else if (props.SwapUpAndDownStab)
        {
            SwapUpAndDownstab();
        }

        GettableItems.Reset();


        //Spell Item handling

        //Remove 
        if (props.StartWithSpellItems)
        {
            foreach (Collectable spellItem in spellItems)
            {
                int index = shufflableItems.IndexOf(spellItem);
                if (index > -1)
                {
                    shufflableItems[index] = minorItems.Sample(r);
                }
            }
        }
        HandleStartingSpellItems(possibleItemLocations);

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

        foreach (Collectable item in possibleStartItems)
        {
            if (props.StartsWithCollectable(item) && (canReplaceStartingSpellsWithMinorItems || !item.IsSpell()))
            {
                int index = shufflableItems.IndexOf(item);
                if (index > -1)
                {
                    shufflableItems[index] = minorItems.Sample(r);
                }
            }
        }

        if (props.IncludeSpellsInShuffle)
        {
            foreach (Collectable item in possibleStartSpells)
            {
                if (props.StartsWithCollectable(item) && shufflableItems.Contains(item))
                {
                    shufflableItems[shufflableItems.IndexOf(item)] = minorItems.Sample(r);
                }
            }
        }

        //Do the actual shuffling
        List<Location> duplicateItemPlacementCandidates = [];

        if (props.MixOverworldPalaceItems)
        {

            if (props.ShufflePalaceItems)
            {
                List<Collectable> palaceCollectables = [.. palaceLocations.SelectMany(i => i.GetAllCollectables())];
                shufflableItems.Add(palaceCollectables.Select(c => props.StartsWithCollectable(c) ? minorItems.Sample(r) : c));
                globalShuffleLocations.Add(palaceLocations);
            }
            duplicateItemPlacementCandidates.AddRange(globalShuffleLocations);
            DoShuffle(shufflableItems, globalShuffleLocations);
        }
        else
        {
            List<Location> palaceItemLocs = possibleItemLocations.Where(i => i.Palace != null).ToList();
            List<Location> nonPalaceItemLocs = possibleItemLocations.Where(i => i.Palace == null).ToList();

            List<Collectable> itemsToActuallyShuffle;
            List<Location> shufflableItemLocations;

            if (props.ShufflePalaceItems)
            {
                itemsToActuallyShuffle = [];
                shufflableItemLocations = [];
                foreach (Location palaceLocation in palaceItemLocs)
                {
                    shufflableItemLocations.Add(palaceLocation);
                    itemsToActuallyShuffle.Add(palaceLocation.GetAllCollectables());
                }
                duplicateItemPlacementCandidates.AddRange(shufflableItemLocations);
                DoShuffle(itemsToActuallyShuffle, shufflableItemLocations);
            }
            else //vanilla palace items
            {
                foreach (Location palaceLocation in palaceItemLocs.Where(i => i.Palace!.Number < 7))
                {
                    Palace palace = palaceLocation.Palace!;
                    List<Collectable> collectables = palace.GetVanillaCollectables(palace.ItemRooms.Count);

                    for (int i = 0; i < collectables.Count; i++)
                    {
                        collectables[i] = props.StartsWithCollectable(collectables[i]) ? minorItems.Sample(r) : collectables[i];
                    }
                    palaceLocation.SetCollectables(collectables, false);
                }
            }

            if (props.ShuffleOverworldItems)
            {
                itemsToActuallyShuffle = [];
                shufflableItemLocations = [];
                foreach (Location nonPalaceLocation in nonPalaceItemLocs)
                {
                    shufflableItemLocations.Add(nonPalaceLocation);
                    foreach (Collectable collectable in nonPalaceLocation.GetShufflableCollectables())
                    {
                        itemsToActuallyShuffle.Add(
                            props.StartsWithCollectable(collectable) && (!collectable.IsSpell() || canReplaceStartingSpellsWithMinorItems)
                                ? minorItems.Sample(r) : collectable);
                    }
                }
                duplicateItemPlacementCandidates.AddRange(shufflableItemLocations);
                DoShuffle(itemsToActuallyShuffle, shufflableItemLocations);
            }
            else
            {
                foreach (Location nonPalaceLocation in nonPalaceItemLocs)
                {
                    List<Collectable> collectables = nonPalaceLocation.GetAllCollectables();
                    for (int i = 0; i < collectables.Count; i++)
                    {
                        bool isMinorItem = props.StartsWithCollectable(collectables[i])
                            && (!collectables[i].IsSpell() || canReplaceStartingSpellsWithMinorItems);
                        collectables[i] = isMinorItem ? minorItems.Sample(r) : collectables[i];
                    }
                    nonPalaceLocation.SetCollectables(collectables, false);
                }
            }
        }

        //Placing unmixed vanilla locations could have replaced unwanted spell items, so recheck them
        HandleStartingSpellItems(ItemLocations().ToList());

        if (props.RemoveItems.Count > 0)
        {
            foreach (Location location in possibleItemLocations)
            {
                foreach (Collectable collectable in location.GetAllCollectables().Where(props.RemoveItems.Contains).ToList())
                {
                    location.ReplaceCollectable(collectable, minorItems.Sample(r));
                }
            }
        }

        int heartContainersInItemPool = props.MaxHearts - props.StartHearts;
        int magicContainersInItemPool = props.MaxMagicContainers - props.StartMagicContainers;

        List<Location> heartContainerLocations = possibleItemLocations.Where(i => i.GetAllCollectables().Contains(Collectable.HEART_CONTAINER)).ToList();

        //If there are more starting containers than vanilla, replace some containers with small items
        if (heartContainersInItemPool < 4)
        {
            int heartContainersToRemove = 4 - heartContainersInItemPool;
            while (heartContainersToRemove > 0)
            {
                Location hcLocation = heartContainerLocations.Sample(r)!;
                List<Collectable> collectables = hcLocation.GetAllCollectables();
                List<int> order = Enumerable.Range(0, collectables.Count).ToList();
                order.FisherYatesShuffle(r);
                foreach (int index in order)
                {
                    if (collectables[index] == Collectable.HEART_CONTAINER)
                    {
                        collectables[index] = minorItems.Sample(r);
                        hcLocation.SetCollectables(collectables);
                        heartContainersToRemove--;
                        shufflableItems.Remove(Collectable.HEART_CONTAINER);
                        shufflableItems.Add(collectables[index]);
                        if (!collectables.Any(i => i == Collectable.HEART_CONTAINER))
                        {
                            heartContainerLocations.Remove(hcLocation);
                        }
                        break;
                    }
                }
            }
        }
        List<Location> magicContainerLocations = possibleItemLocations.Where(i => i.GetAllCollectables().Contains(Collectable.MAGIC_CONTAINER)).ToList();
        if (magicContainersInItemPool < 4)
        {
            int magicContainersToRemove = 4 - magicContainersInItemPool;
            while (magicContainersToRemove > 0)
            {
                Location mcLocation = magicContainerLocations.Sample(r)!;
                List<Collectable> collectables = mcLocation.GetAllCollectables();
                List<int> order = Enumerable.Range(0, collectables.Count).ToList();
                order.FisherYatesShuffle(r);
                foreach (int index in order)
                {
                    if (collectables[index] == Collectable.MAGIC_CONTAINER)
                    {
                        collectables[index] = minorItems.Sample(r);
                        mcLocation.SetCollectables(collectables);
                        magicContainersToRemove--;
                        shufflableItems.Remove(Collectable.MAGIC_CONTAINER);
                        shufflableItems.Add(collectables[index]);
                        if (!collectables.Any(i => i == Collectable.MAGIC_CONTAINER))
                        {
                            magicContainerLocations.Remove(mcLocation);
                        }
                        break;
                    }
                }
            }
        }

        //Excess items are items that need to be placed beyond the normal vanilla items
        //these could be containers if more than 4 need to be placed, or palace items that were removed from palaces
        int overworldMinorItemCount = possibleItemLocations
            .Where(loc => loc.Palace == null)
            .SelectMany(loc => loc.GetShufflableCollectables())
            .Count(item => item.IsMinorItem());
        int palaceMinorItemCount = possibleItemLocations
            .Where(loc => loc.Palace != null)
            .SelectMany(loc => loc.GetShufflableCollectables())
            .Count(item => item.IsMinorItem());
        //Heart containers over 4 are excess (overworld bucket)
        for (int i = 4; i < heartContainersInItemPool; i++)
        {
            overworldExcessItems.Add(Collectable.HEART_CONTAINER);
        }
        for (int i = 4; i < magicContainersInItemPool; i++)
        {
            overworldExcessItems.Add(Collectable.MAGIC_CONTAINER);
        }
        //Palace items that didn't get placed in palaces are excess (palace bucket)
        List<Collectable> palaceExcessItems = [];
        foreach (Collectable palaceItem in Enum.GetValues<Collectable>().Where(c => c.IsVanillaPalaceItem()))
        {
            if (!possibleItemLocations
                .SelectMany(l => l.GetAllCollectables())
                .Any(c => c == palaceItem) && !props.StartsWithCollectable(palaceItem))
            {
                palaceExcessItems.Add(palaceItem);
            }
        }

        List<Location> overworldMinorItemLocations = [.. possibleItemLocations
            .Where(loc => loc.Palace == null)
            .Where(l => l.GetShufflableCollectables()
            .Any(c => c.IsMinorItem()))];
        List<Location> palaceMinorItemLocations = [.. possibleItemLocations
            .Where(loc => loc.Palace != null)
            .Where(l => l.GetShufflableCollectables()
            .Any(c => c.IsMinorItem()))];
        List<Location> minorItemLocationsJoined = [.. overworldMinorItemLocations, .. palaceMinorItemLocations];

        void PlaceExcessItems(int minorItemCount, List<Collectable> excessItems, List<Location> minorItemLocations)
        {
            if (excessItems.Count > minorItemCount)
            {
                throw new Exception("Insufficient locations to place excess items. The validation should have caught this.");
            }
            foreach (Collectable excessItem in excessItems)
            {
                Location minorItemLocation = minorItemLocations.Sample(r) ?? throw new ImpossibleException("No minor item to replace after promotion???");
                Collectable minorItem = minorItemLocation.GetShufflableCollectables().Where(i => i.IsMinorItem()).ToList().Sample(r);
                minorItemLocation.ReplaceCollectable(minorItem, excessItem, r);
                if (!minorItemLocation.GetShufflableCollectables().Any(i => i.IsMinorItem()))
                {
                    minorItemLocations.Remove(minorItemLocation);
                }
            }
        }

        if (props.MixOverworldPalaceItems)
        {
            int minorItemCountJoined = overworldMinorItemCount + palaceMinorItemCount;
            List<Collectable> excessItemsJoined = [.. overworldExcessItems, .. palaceExcessItems];
            PlaceExcessItems(minorItemCountJoined, excessItemsJoined, minorItemLocationsJoined);
        }
        else
        {
            PlaceExcessItems(overworldMinorItemCount, overworldExcessItems, overworldMinorItemLocations);
            PlaceExcessItems(palaceMinorItemCount, palaceExcessItems, palaceMinorItemLocations);
        }

        //If allow item duplicates is on, as many duplicates of these items will be added as possible, up to the number
        //of minor items that can be replaced, in this order
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

            //Mario mode has different priority because the relative value of upgrades is very different
            if (props.MarioMode)
            {
                importantItemsToDuplicate = [
                Collectable.GLOVE,
                Collectable.HAMMER,
                Collectable.TANOOKI,
                Collectable.FAIRY_SPELL,
                Collectable.THUNDER_SPELL,
                Collectable.REFLECT_SPELL,
                Collectable.DOWNSTAB,
                Collectable.MAGIC_KEY,
                Collectable.RAFT,
                Collectable.BOOTS,
                Collectable.FLUTE,
                Collectable.UPSTAB,
                ];
            }

            importantItemsToDuplicate = importantItemsToDuplicate.Where(c => shufflableItems.Contains(c) && !props.RemoveItems.Contains(c)).ToList();

            var minorItemLocations = duplicateItemPlacementCandidates.Where(i => i.GetShufflableCollectables().Any(c => c.IsMinorItem())).ToList();
            int replaceableMinorItemCount = duplicateItemPlacementCandidates.Sum(l => l.GetShufflableCollectables().Count(c => c.IsMinorItem()));
            importantItemsToDuplicate = importantItemsToDuplicate.GetRange(0, int.Min(replaceableMinorItemCount, importantItemsToDuplicate.Count));
            importantItemsToDuplicate.FisherYatesShuffle(r);
            minorItemLocations.FisherYatesShuffle(r);

            for (int itemIndex = 0; itemIndex < importantItemsToDuplicate.Count; itemIndex++)
            {
                Location minorItemLocation = minorItemLocations.Sample(r)!;
                List<Collectable> locationCollectables = minorItemLocation.GetAllCollectables();
                List<int> collectableIndexes = Enumerable.Range(0, minorItemLocation.GetCollectableCount()).ToList();
                collectableIndexes.FisherYatesShuffle(r);
                foreach (int collectableIndex in collectableIndexes)
                {
                    if (locationCollectables[collectableIndex].IsMinorItem())
                    {
                        locationCollectables[collectableIndex] = importantItemsToDuplicate[itemIndex];
                        minorItemLocation.SetCollectables(locationCollectables);
                        if (!locationCollectables.Any(c => c.IsMinorItem()))
                        {
                            minorItemLocations.Remove(minorItemLocation);
                        }
                        break;
                    }
                }
            }
        }

        if (props.PreventSpellItemChains)
        {
            PreventSpellItemChains();
        }

        if (props.KeyLogic == KeyLogicOption.KEYS_PER_PALACE_SOLVABLE)
        {
            var nonPalaceLocs = possibleItemLocations.Where(loc => loc.Palace == null);
            // don't want to roll this for every ReplaceCollectable call
            Collectable replacementItem = minorItemsMinusKey.Sample(r);
            foreach (var loc in nonPalaceLocs)
            {
                while (true)
                {
                    if (loc.ReplaceCollectable(Collectable.KEY, replacementItem))
                    {
                        // we did a key replacement, roll next replacement
                        replacementItem = minorItemsMinusKey.Sample(r);
                    }
                    else
                    {
                        break; // nothing to replace - next!
                    }
                }
            }
        }
    }

    private void ReplaceWithUnshuffledMinorItem(TownMap townMap)
    {
        townMap.Collectable = minorItems.Sample(r);
        townMap.CollectableIsShufflable = false;
    }

    private void HandleStartingSpellItems(IList<Location> possibleItemLocations)
    {
        if (props.StartWithSpellItems)
        {
            foreach (Collectable spellItem in spellItems)
            {
                Location? defaultLocation = possibleItemLocations.FirstOrDefault(i => i.GetAllCollectables().Contains(spellItem));
                defaultLocation?.ReplaceCollectable(spellItem, minorItems.Sample(r));
                GettableItems[spellItem] = true;
            }
        }
        else
        {
            List<Location> spellItemTowns = [westHyrule.ruto, westHyrule.mido, eastHyrule.darunia];
            if (props.IncludeQuestItemsInShuffle)
            {
                spellItemTowns.Add([westHyrule.sariaNorth, eastHyrule.nabooru]);
            }

            //var allTowns = possibleItemLocations.Where(i => i.Town != null && i?.Town?.Type != TownType.BAGU);
            for (int i = 0; i < spellItemTowns.Count; i++)
            {
                Collectable wizardCollectable = (Collectable)spellItemTowns[i].Town!.GetWizard()!.Collectable!;
                if (props.StartsWithCollectable(wizardCollectable))
                {
                    GettableItems[spellItems[i]] = true;
                    Location? spellItemLocation =
                        possibleItemLocations.FirstOrDefault(location => location.GetAllCollectables().Contains(spellItems[i]));
                    spellItemLocation?.ReplaceCollectable(spellItems[i], minorItems.Sample(r));
                }
            }
        }
    }

    // Disallow old men from holding other spell items, preventing long
    // item chains
    private void PreventSpellItemChains()
    {
        //child, trophy, medicine, mirror, water
        List<Location> locationsThatContainWizards = [eastHyrule.darunia, westHyrule.ruto, westHyrule.mido];
        if (props.IncludeQuestItemsInShuffle)
        {
            locationsThatContainWizards.AddRange([westHyrule.sariaNorth, eastHyrule.nabooru]);
        }

        List<Location> possibleReplacementLocations = ItemLocations().ToList();
        foreach (Location wizardLocation in locationsThatContainWizards)
        {
            bool replaced = false;
            Collectable wizardCollectable = (Collectable)wizardLocation.Town?.GetWizard()?.Collectable!;
            if (!wizardCollectable.IsSpellItem())
            {
                continue;
            }
            possibleReplacementLocations.FisherYatesShuffle(r);
            foreach (Location replacementLocation in possibleReplacementLocations)
            {
                if (!replacementLocation.GetShufflableCollectables().Any(i => !i.IsSpellItem())) { continue; }
                List<Collectable> replacementCollectables = replacementLocation.GetShufflableCollectables();
                replacementCollectables.FisherYatesShuffle(r);
                foreach (Collectable replacementCollectable in replacementCollectables)
                {
                    if (!replacementCollectable.IsSpellItem())
                    {
                        replacementLocation.ReplaceCollectable(replacementCollectable, wizardCollectable, r);
                        wizardLocation.ReplaceCollectable(wizardCollectable, replacementCollectable, r);
                        //WTB C# 15 labeled continue
                        replaced = true;
                        break;
                    }
                }
                if (replaced)
                {
                    break;
                }
            }
        }
    }

    private void DoShuffle(List<Collectable> itemsToShuffle, List<Location> itemShuffleLocations)
    {
        itemShuffleLocations = itemShuffleLocations.Distinct().ToList();
        int possibleItemPlacements = itemShuffleLocations.Sum(i => i.GetCollectableCount(true));

        if (itemsToShuffle.Count != possibleItemPlacements)
        {
            Diag.Block("items", string.Join("\n", itemShuffleLocations.Select(i => $"location={i.Name} slots={i.GetShufflableCollectables().Count}")));
            Diag.Trace("items", $"placements={possibleItemPlacements} items=[{string.Join(",", itemsToShuffle)}]");
            throw new Exception("Item locations must match number of items");
        }

        itemsToShuffle.FisherYatesShuffle(r);

        int itemIndex = 0;
        foreach (Location location in itemShuffleLocations)
        {
            int collectableCount = location.GetCollectableCount(true);
            List<Collectable> newCollectables = itemsToShuffle.GetRange(itemIndex, collectableCount);
            itemIndex += collectableCount;
            location.SetCollectables(newCollectables, true);
        }
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
        Collectable[] temporaryCollectables = Enum.GetValues<Collectable>().Where(i => !i.IsInternalUse() && !i.IsMinorItem()).ToArray();
        foreach (Palace palace in palaces)
        {
            foreach (Room itemRoom in palace.ItemRooms)
            {
                while (((Collectable)itemRoomIndex).IsMinorItem())
                {
                    itemRoomIndex++;
                }
                itemRoom.UpdateSideviewItem(temporaryCollectables[itemRoomIndex++]);
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
            [PalaceGrouping.Palace125] = ROMData.GetBytes(Room.Group1ItemPresenceBitsStartAddress, 0x20),
            [PalaceGrouping.Palace346] = ROMData.GetBytes(Room.Group2ItemPresenceBitsStartAddress, 0x20),
            [PalaceGrouping.PalaceGp] = ROMData.GetBytes(Room.Group3ItemPresenceBitsStartAddress, 0x20),
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
                room.PutItemPresenceBitsIntoFinalTable(palaceItemBits);
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
                room.PutItemPresenceBitsIntoFinalTable(palaceItemBits);
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

        /* this shouldn't be needed anymore
        try
        {
            ROM testRom = new(ROMData);
            Random testRng = new(SeedHash);
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
        */
        return true;
    }

    private bool IsEverythingReachable()
        => reachability.IsEverythingReachable(worlds, props, GettableItems, ref isEverythingReachableFailures);

    private void ShortenWizards()
    {
        //These are all changing the vanilla addresses so they can be used properly when towns aren't modified
        //and to make it easier to parse the vanilla town data even when it will be later modified.

        //Change the exit leading to the intermedaite wizard room to go directly to the basement
        //uses the map door table
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 1 * 4 + 2, 36 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 4 * 4 + 3, 37 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 8 * 4 + 1, 38 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 9 * 4 + 3, 39 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 13 * 4 + 3, 40 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 15 * 4 + 3, 41 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 19 * 4 + 1, 42 << 2);
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 23 * 4 + 2, 43 << 2);

        //Change the exit from the wizard room to point back out to the town
        //these are general connections table
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 36 * 4, (1 << 2) + 2);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 37 * 4, (4 << 2) + 3);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 38 * 4, (8 << 2) + 1);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 39 * 4, (9 << 2) + 3);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 40 * 4, (13 << 2) + 3);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 41 * 4, (15 << 2) + 3);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 42 * 4, (19 << 2) + 1);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 43 * 4, (23 << 2) + 2);

        //downstab guy
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 10 * 4 + 1, 44 << 2);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 44 * 4, (10 << 2) + 1);

        //granny's basement
        ROMData.Put(RomMap.TOWN_DOOR_CONNECTIONS_TABLE + 18 * 4 + 1, 46 << 2);
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 46 * 4, (18 << 2) + 1);

        //upstab guy entrance is unchanged because you drop into it, but still change the exit to lead directly out
        ROMData.Put(RomMap.TOWN_NORMAL_CONNECTIONS_TABLE + 45 * 4, (16 << 2) + 3);
    }

    public IReadOnlySet<RequirementType> GetRequireables(RandomizerProperties props)
        => reachability.GetRequireables(props, GettableItems);

    private async Task ProcessOverworld(Func<string, Task> progress, CancellationToken ct)
    {
        UpdateProgress(progress, ProgressEnum.PROCESSING_OVERWORLD);
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
            Diag.Retry("overworld.retry", totalReachabilityOverworldAttempts, "event=begin loop=outer");

            //Generate a pathable set of continents
            do // } while (!AllContinentsHaveConnection(worlds));
            {
                totalContinentConnectionOverworldAttempts++;
                Diag.Retry("overworld.connections", totalContinentConnectionOverworldAttempts, $"event=begin outer={totalReachabilityOverworldAttempts}");
                worlds = new List<World>();
                westHyrule = new WestHyrule(props, r, ROMData);
                deathMountain = new DeathMountain(props, r, ROMData);
                eastHyrule = new EastHyrule(props, r, ROMData);
                mazeIsland = new MazeIsland(props, r, ROMData);
                // Reachability visits and indexes these worlds in Continent order.
                worlds.Add(westHyrule);
                worlds.Add(deathMountain);
                worlds.Add(eastHyrule);
                worlds.Add(mazeIsland);

                AssignPalaceLocations();

                if (props.ContinentConnections == ContinentConnectionType.NORMAL)
                {
                    connections = [
                        (westHyrule.LoadCave1(ROMData, Continent.DM), deathMountain.LoadCave1(ROMData, Continent.WEST)),
                        (westHyrule.LoadCave2(ROMData, Continent.DM), deathMountain.LoadCave2(ROMData, Continent.WEST)),
                        (westHyrule.LoadRaft(ROMData, Continent.EAST), eastHyrule.LoadRaft(ROMData, Continent.WEST)),
                        (eastHyrule.LoadBridge(ROMData, Continent.MAZE), mazeIsland.LoadBridge(ROMData, Continent.EAST))
                    ];
                }
                else if (props.ContinentConnections == ContinentConnectionType.TRANSPORTATION_SHUFFLE)
                {
                    // 1 = raft, 2 = bridge, 3 = cave1, 4 = cave2
                    List<int> available = [1, 2, 3, 4];

                    // Remove all vanilla-forced types before any random picks
                    if (props.WestBiome.UsesVanillaMap() || props.DmBiome.UsesVanillaMap())
                    {
                        available.Remove(3); // west<->DM: cave1
                        available.Remove(4); // west<->DM: cave2
                    }
                    if (props.WestBiome.UsesVanillaMap() || props.EastBiome.UsesVanillaMap())
                    {
                        available.Remove(1); // west<->east: raft
                    }
                    if (props.EastBiome.UsesVanillaMap() || props.MazeBiome.UsesVanillaMap())
                    {
                        available.Remove(2); // east<->maze: bridge
                    }

                    int PickRandomType()
                    {
                        int idx = r.Next(available.Count);
                        int t = available[idx];
                        available.RemoveAt(idx);
                        return t;
                    }

                    // west<->DM 1
                    int type = props.WestBiome.UsesVanillaMap() || props.DmBiome.UsesVanillaMap() ? 3 : PickRandomType();
                    SetTransportation(0, 1, type);

                    // west<->DM 2
                    type = props.WestBiome.UsesVanillaMap() || props.DmBiome.UsesVanillaMap() ? 4 : PickRandomType();
                    SetTransportation(0, 1, type);

                    // west<->east
                    type = props.WestBiome.UsesVanillaMap() || props.EastBiome.UsesVanillaMap() ? 1 : PickRandomType();
                    SetTransportation(0, 2, type);

                    // east<->maze
                    type = props.EastBiome.UsesVanillaMap() || props.MazeBiome.UsesVanillaMap() ? 2 : PickRandomType();
                    SetTransportation(2, 3, type);
                }
                else if (props.ContinentConnections == ContinentConnectionType.ANYTHING_GOES)
                {
                    Location l1, l2;
                    List<int> doNotPick = new List<int>();
                    if (props.WestBiome.UsesVanillaMap())
                    {
                        doNotPick.Add(0);
                    }
                    if (props.EastBiome.UsesVanillaMap())
                    {
                        doNotPick.Add(2);
                    }
                    if (props.DmBiome.UsesVanillaMap())
                    {
                        doNotPick.Add(1);
                    }
                    if (props.MazeBiome.UsesVanillaMap())
                    {
                        doNotPick.Add(3);
                    }

                    int raftw1 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long raftw1Attempts = 0;
#endif

                    if (props.WestBiome.UsesVanillaMap())
                    {
                        raftw1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(raftw1))
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++raftw1Attempts))
                                Diag.Trace("overworld.connector", $"attempt={raftw1Attempts} connector=raftw1 current={raftw1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            raftw1 = r.Next(worlds.Count);
                        }
                    }

                    int raftw2 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long raftw2Attempts = 0;
#endif
                    if (props.EastBiome.UsesVanillaMap())
                    {
                        raftw2 = 2;
                    }
                    else
                    {
                        do
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++raftw2Attempts))
                                Diag.Trace("overworld.connector", $"attempt={raftw2Attempts} connector=raftw2 current={raftw2} other={raftw1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            raftw2 = r.Next(worlds.Count);
                        } while (raftw1 == raftw2 || doNotPick.Contains(raftw2));
                    }

                    l1 = worlds[raftw1].LoadRaft(ROMData, (Continent)raftw2);
                    l2 = worlds[raftw2].LoadRaft(ROMData, (Continent)raftw1);
                    connections[0] = (l1, l2);

                    int bridgew1 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long bridgew1Attempts = 0;
#endif
                    if (props.EastBiome.UsesVanillaMap())
                    {
                        bridgew1 = 2;
                    }
                    else
                    {
                        while (doNotPick.Contains(bridgew1))
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++bridgew1Attempts))
                                Diag.Trace("overworld.connector", $"attempt={bridgew1Attempts} connector=bridgew1 current={bridgew1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            bridgew1 = r.Next(worlds.Count);
                        }
                    }
                    int bridgew2 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long bridgew2Attempts = 0;
#endif
                    if (props.MazeBiome.UsesVanillaMap())
                    {
                        bridgew2 = 3;
                    }
                    else
                    {
                        do
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++bridgew2Attempts))
                                Diag.Trace("overworld.connector", $"attempt={bridgew2Attempts} connector=bridgew2 current={bridgew2} other={bridgew1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            bridgew2 = r.Next(worlds.Count);
                        } while (bridgew1 == bridgew2 || doNotPick.Contains(bridgew2));
                    }

                    l1 = worlds[bridgew1].LoadBridge(ROMData, (Continent)bridgew2);
                    l2 = worlds[bridgew2].LoadBridge(ROMData, (Continent)bridgew1);
                    connections[1] = (l1, l2);

                    int c1w1 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long c1w1Attempts = 0;
#endif
                    if (props.WestBiome.UsesVanillaMap())
                    {
                        c1w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c1w1))
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++c1w1Attempts))
                                Diag.Trace("overworld.connector", $"attempt={c1w1Attempts} connector=c1w1 current={c1w1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            c1w1 = r.Next(worlds.Count);
                        }
                    }
                    int c1w2 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long c1w2Attempts = 0;
#endif
                    if (props.DmBiome.UsesVanillaMap())
                    {
                        c1w2 = 1;
                    }
                    else
                    {
                        do
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++c1w2Attempts))
                                Diag.Trace("overworld.connector", $"attempt={c1w2Attempts} connector=c1w2 current={c1w2} other={c1w1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            c1w2 = r.Next(worlds.Count);
                        } while (c1w1 == c1w2 || doNotPick.Contains(c1w2));
                    }

                    l1 = worlds[c1w1].LoadCave1(ROMData, (Continent)c1w2);
                    l2 = worlds[c1w2].LoadCave1(ROMData, (Continent)c1w1);
                    connections[2] = (l1, l2);

                    int c2w1 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long c2w1Attempts = 0;
#endif
                    if (props.WestBiome.UsesVanillaMap())
                    {
                        c2w1 = 0;
                    }
                    else
                    {
                        while (doNotPick.Contains(c2w1))
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++c2w1Attempts))
                                Diag.Trace("overworld.connector", $"attempt={c2w1Attempts} connector=c2w1 current={c2w1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            c2w1 = r.Next(worlds.Count);
                        }
                    }
                    int c2w2 = r.Next(worlds.Count);
#if Z2R_DIAGNOSTICS
                    long c2w2Attempts = 0;
#endif
                    if (props.DmBiome.UsesVanillaMap())
                    {
                        c2w2 = 1;
                    }
                    else
                    {
                        do
                        {
#if Z2R_DIAGNOSTICS
                            if (Diag.IsSample(++c2w2Attempts))
                                Diag.Trace("overworld.connector", $"attempt={c2w2Attempts} connector=c2w2 current={c2w2} other={c2w1} excluded=[{string.Join(",", doNotPick)}]");
#endif
                            c2w2 = r.Next(worlds.Count);
                        } while (c2w1 == c2w2 || doNotPick.Contains(c2w2));
                    }

                    l1 = worlds[c2w1].LoadCave2(ROMData, (Continent)c2w2);
                    l2 = worlds[c2w2].LoadCave2(ROMData, (Continent)c2w1);
                    connections[3] = (l1, l2);
                }
                else
                {
                    throw new ImpossibleException("Unrecognized continent connector type");
                }
            } while (!AllContinentsAreConnected(connections));

            ShufflePalaces();
            startLocation = DetermineStartingLocation();
            GetWorld(startLocation!).SetStart(startLocation!);

            int nonContinentGenerationAttempts = 0;
            int nonTerrainShuffleAttempt = 0;
            DateTime timestamp;
            //Shuffle everything else
            do //while (wtries < 10 && !EverythingReachable());
            {
                //GENERATE WEST
                if (ct.IsCancellationRequested) 
                {
                    Diag.Trace("seed", "event=cancelled");
                    return;
                }
                UpdateProgress(progress, ProgressEnum.GENERATING_WEST_HYRULE);
                nonContinentGenerationAttempts++;
                Diag.Retry("overworld.terrain", nonContinentGenerationAttempts,
                    $"outer={totalReachabilityOverworldAttempts} limit={NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT} "
                    + $"west={westHyrule.AllReached} east={eastHyrule.AllReached} dm={deathMountain.AllReached} maze={mazeIsland.AllReached}");
                timestamp = DateTime.Now;
                if (!westHyrule.AllReached)
                {
                    Diag.Trace("terrain", $"event=begin world=westHyrule attempts={totalWestGenerationAttempts}");
                    while (!westHyrule.Terraform(props, ROMData))
                    {
                        Diag.Retry("terrain.retry", totalWestGenerationAttempts + 1, $"world=westHyrule limit={TERRAIN_GENERATION_ATTEMPT_LIMIT}");
                        if (++totalWestGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate West Continent");
                        }
                    }
                    totalWestGenerationAttempts++;
                    Diag.Trace("terrain", $"event=end world=westHyrule attempts={totalWestGenerationAttempts}");
                }
                westHyrule.ResetVisitabilityState();
                timeSpentBuildingWest += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE DM
                if (ct.IsCancellationRequested)
                {
                    Diag.Trace("seed", "event=cancelled");
                    return;
                }
                UpdateProgress(progress, ProgressEnum.GENERATING_DEATH_MOUNTAIN);
                timestamp = DateTime.Now;
                if (!deathMountain.AllReached)
                {
                    Diag.Trace("terrain", $"event=begin world=deathMountain attempts={totalDeathMountainGenerationAttempts}");
                    while (!deathMountain.Terraform(props, ROMData))
                    {
                        Diag.Retry("terrain.retry", totalDeathMountainGenerationAttempts + 1, $"world=deathMountain limit={TERRAIN_GENERATION_ATTEMPT_LIMIT}");
                        if (++totalDeathMountainGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate Death Mountain");
                        }
                    }
                    totalDeathMountainGenerationAttempts++;
                    Diag.Trace("terrain", $"event=end world=deathMountain attempts={totalDeathMountainGenerationAttempts}");
                }
                deathMountain.ResetVisitabilityState();
                timeSpentBuildingDM += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE EAST
                if (ct.IsCancellationRequested)
                {
                    Diag.Trace("seed", "event=cancelled");
                    return;
                }
                UpdateProgress(progress, ProgressEnum.GENERATING_EAST_HYRULE);
                timestamp = DateTime.Now;
                if (!eastHyrule.AllReached)
                {
                    Diag.Trace("terrain", $"event=begin world=eastHyrule attempts={totalEastGenerationAttempts}");
                    while (!eastHyrule.Terraform(props, ROMData))
                    {
                        Diag.Retry("terrain.retry", totalEastGenerationAttempts + 1, $"world=eastHyrule limit={TERRAIN_GENERATION_ATTEMPT_LIMIT}");
                        if (++totalEastGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate East Continent");
                        }
                    }
                    totalEastGenerationAttempts++;
                    Diag.Trace("terrain", $"event=end world=eastHyrule attempts={totalEastGenerationAttempts}");
                }
                eastHyrule.ResetVisitabilityState();
                timeSpentBuildingEast += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                //GENERATE MAZE ISLAND
                if (ct.IsCancellationRequested)
                {
                    Diag.Trace("seed", "event=cancelled");
                    return;
                }
                UpdateProgress(progress, ProgressEnum.GENERATING_MAZE_ISLAND);
                timestamp = DateTime.Now;
                if (!mazeIsland.AllReached)
                {
                    Diag.Trace("terrain", $"event=begin world=mazeIsland attempts={totalMazeIslandGenerationAttempts}");
                    while (!mazeIsland.Terraform(props, ROMData))
                    {
                        Diag.Retry("terrain.retry", totalMazeIslandGenerationAttempts + 1, $"world=mazeIsland limit={TERRAIN_GENERATION_ATTEMPT_LIMIT}");
                        if (++totalMazeIslandGenerationAttempts == TERRAIN_GENERATION_ATTEMPT_LIMIT)
                        {
                            throw new Exception("Failed to generate Maze Island");
                        }
                    }
                    totalMazeIslandGenerationAttempts++;
                    Diag.Trace("terrain", $"event=end world=mazeIsland attempts={totalMazeIslandGenerationAttempts}");
                }
                mazeIsland.ResetVisitabilityState();
                timeSpentBuildingMI += (int)DateTime.Now.Subtract(timestamp).TotalMilliseconds;

                worlds.Where(i => i.biome == Biome.VANILLA_SHUFFLE).ToList().ForEach(i => i.DisableDisallowedPassthroughs());

                if (ct.IsCancellationRequested)
                {
                    Diag.Trace("seed", "event=cancelled");
                    return;
                }
                UpdateProgress(progress, ProgressEnum.SHUFFLING_ITEMS_AND_SPELLS);

                //Then perform non-terrain shuffles looking for one that works.
                nonTerrainShuffleAttempt = 0;
                do
                {
                    worlds.ForEach(i => i.ResetVisitabilityState()); // Sets all locations as unreachable
                    worlds.ForEach(i => i.ResetCollectables(props));

                    ShuffleItems();

                    westHyrule.UpdateAllReached();
                    eastHyrule.UpdateAllReached();

                    mazeIsland.UpdateAllReached();
                    deathMountain.UpdateAllReached();

                    nonTerrainShuffleAttempt++;
                    Diag.Retry("overworld.items", nonTerrainShuffleAttempt,
                        $"outer={totalReachabilityOverworldAttempts} terrain={nonContinentGenerationAttempts} limit={NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT}");
                } while (nonTerrainShuffleAttempt < NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT && !IsEverythingReachable());

                if (nonTerrainShuffleAttempt != NON_TERRAIN_SHUFFLE_ATTEMPT_LIMIT)
                {
                    break;
                }
            } while (nonContinentGenerationAttempts < NON_CONTINENT_SHUFFLE_ATTEMPT_LIMIT);
        } while (!IsEverythingReachable());
    }

    private async void UpdateProgress(Func<string, Task> progress, ProgressEnum v)
    {
        switch (v)
        {
            case ProgressEnum.GENERATING_PALACES:
                await progress.Invoke("Generating Palaces");
                break;
            case ProgressEnum.PROCESSING_OVERWORLD:
                await progress.Invoke("Processing Overworld");
                break;
            case ProgressEnum.GENERATING_WEST_HYRULE:
                await progress.Invoke("Generating Western Hyrule");
                break;
            case ProgressEnum.GENERATING_DEATH_MOUNTAIN:
                await progress.Invoke("Generating Death Mountain");
                break;
            case ProgressEnum.GENERATING_EAST_HYRULE:
                await progress.Invoke("Generating East Hyrule");
                break;
            case ProgressEnum.GENERATING_MAZE_ISLAND:
                await progress.Invoke("Generating Maze Island");
                break;
            case ProgressEnum.SHUFFLING_ITEMS_AND_SPELLS:
                await progress.Invoke("Shuffling Items and Spells");
                break;
            case ProgressEnum.RUNNING_COMPLETABILITY_CHECKS:
                await progress.Invoke("Running Seed Completability Checks");
                break;
            case ProgressEnum.SHUFFLING_ENEMIES:
                await progress.Invoke("Shuffling Enemies");
                break;
            case ProgressEnum.GENERATING_HINTS:
                await progress.Invoke("Generating Hints");
                break;
            case ProgressEnum.APPLYING_PATCHES:
                await progress.Invoke("Applying Patches");
                break;
            case ProgressEnum.LINKING_ASSEMBLY:
                await progress.Invoke("Linking Assembly");
                break;
            case ProgressEnum.FINISHING_UP:
                await progress.Invoke("Finishing up");
                break;
        }
    }

    /// <param name="type">1 = raft, 2 = bridge, 3 = cave1, 4 = cave2</param>
    private void SetTransportation(int w1, int w2, int type)
    {
        Location l1, l2;
        if (type == 1)
        {
            l1 = worlds[w1].LoadRaft(ROMData, (Continent)w2);
            l2 = worlds[w2].LoadRaft(ROMData, (Continent)w1);
            connections[0] = (l1, l2);
        }
        else if (type == 2)
        {
            l1 = worlds[w1].LoadBridge(ROMData, (Continent)w2);
            l2 = worlds[w2].LoadBridge(ROMData, (Continent)w1);
            connections[1] = (l1, l2);
        }
        else if (type == 3)
        {
            l1 = worlds[w1].LoadCave1(ROMData, (Continent)w2);
            l2 = worlds[w2].LoadCave1(ROMData, (Continent)w1);
            connections[2] = (l1, l2);
        }
        else if (type == 4)
        {
            l1 = worlds[w1].LoadCave2(ROMData, (Continent)w2);
            l2 = worlds[w2].LoadCave2(ROMData, (Continent)w1);
            connections[3] = (l1, l2);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    private bool AllContinentsAreConnected((Location, Location)[] connections)
    {
        if (connections.Length != 4)
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
        while (progress == true)
        {
            progress = false;
            foreach ((Location, Location) connection in connections)
            {
                if (continentsFound.Contains(connection.Item1.Continent) && !continentsFound.Contains(connection.Item2.Continent))
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

    private Location DetermineStartingLocation()
    {
        Location[] westPalaces = [westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3];
        Location[] westTowns = [westHyrule.rauru, westHyrule.ruto, westHyrule.sariaNorth, westHyrule.mido];
        Location[] westAllEligible = [
            .. westTowns,
            .. westPalaces,
            westHyrule.kingsTomb,
        ];

        return props.StartingLocation switch
        {
            StartingLocation.NORTH_PALACE => westHyrule.northPalace,
            // Palace 4's Moa makes it unsuitable as a start.
            StartingLocation.REGULAR_PALACE => westPalaces.Where(loc => loc.Palace!.Number is not (4 or 7)).ToArray().Sample(r)!,
            // this relies on code in ShufflePalace making sure GP is in the west (if possible)
            StartingLocation.GREAT_PALACE => westPalaces.FirstOrDefault(loc => loc.Palace!.Number == 7)
                ?? throw new UserFacingException("Impossible Start Location", "Great Palace can not be in the West with the current settings."),
            StartingLocation.RANDOM => westAllEligible.Sample(r)!,
            _ => throw new NotImplementedException()
        };
    }

    //ItemLocs is no longer a global reference (and probably shouldn't have been to begin with).
    //It's possible this makes a return at some point just for efficency but for now it's dead Jim
    /*
    private List<Location> LoadItemLocs(int[] itemsPerPalaces)
    {
        List<Location> GetPalacesWithItems(IEnumerable<Location> palaces) =>
            palaces.Where(p => p.Palace?.Number is int n && n != 7 && props.PalaceItemRoomCounts[n - 1] != 0).ToList();

        // Listed somewhat in order of expected progression. This way we can
        // use the indexes of two locations to determine approx distance.

        itemLocs = new List<Location>(30);
        //palaces
        itemLocs = GetPalacesWithItems([
            westHyrule.locationAtPalace1,
            westHyrule.locationAtPalace2,
            westHyrule.locationAtPalace3,
            mazeIsland.locationAtPalace4,
            eastHyrule.locationAtPalace5,
            eastHyrule.locationAtPalace6,
            eastHyrule.locationAtGP
        ]);

        //always live locations
        itemLocs.AddRange([
            westHyrule.grassTile,
            westHyrule.heartContainerCave,
            westHyrule.magicContainerCave,
            westHyrule.medicineCave,
            westHyrule.trophyCave,
            deathMountain.specRock,
            deathMountain.hammerCave,
            eastHyrule.waterTile,
            eastHyrule.desertTile,
            mazeIsland.childDrop,
            mazeIsland.magicContainerDrop
        ]);

        //pbag shuffle
        if (props.PbagItemShuffle)
        {
            itemLocs.Add(westHyrule.pbagCave);
            itemLocs.Add(eastHyrule.pbagCave1);
            itemLocs.Add(eastHyrule.pbagCave2);
        }


        //Quest item shuffle
        if (props.IncludeBagusNoteInShuffle)
        {
            itemLocs.Add(westHyrule.bagu);
        }
        westHyrule.bagu.Town!.GetTownMap(VanillaTownMap.BAGU_INDOORS)!.CollectableIsShufflable = props.IncludeQuestItemsInShuffle;
        westHyrule.sariaNorth.Town!.GetTownMap(VanillaTownMap.SARIA_TABLE)!.CollectableIsShufflable = props.IncludeQuestItemsInShuffle;
        eastHyrule.nabooru.Town!.GetTownMap(VanillaTownMap.NABOORU_MID)!.CollectableIsShufflable = props.IncludeQuestItemsInShuffle;


        itemLocs.Add(westHyrule.rauru);
        itemLocs.Add(westHyrule.ruto);
        itemLocs.Add(westHyrule.sariaNorth);
        itemLocs.Add(westHyrule.mido);
        itemLocs.Add(eastHyrule.nabooru);
        itemLocs.Add(eastHyrule.darunia);
        itemLocs.Add(eastHyrule.oldKasuto);
        itemLocs.Add(eastHyrule.newKasuto);

        westHyrule.rauru.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        westHyrule.ruto.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        westHyrule.sariaNorth.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        westHyrule.mido.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        eastHyrule.nabooru.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        eastHyrule.darunia.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        eastHyrule.newKasuto.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;
        eastHyrule.oldKasuto.Town!.GetWizard()!.CollectableIsShufflable = props.IncludeSpellsInShuffle;

        //stab shuffle
        westHyrule.mido.Town!.GetTownMap(VanillaTownMap.MIDO_TRAINER)!.CollectableIsShufflable = props.IncludeSwordTechsInShuffle;
        eastHyrule.darunia.Town!.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!.CollectableIsShufflable = props.IncludeSwordTechsInShuffle;

        //Remove locations that don't have any shufflable items
        itemLocs.RemoveAll(i => i.GetShufflableCollectables().Count == 0);

        return itemLocs;
    }
    */

    /// <summary>
    /// Shuffles which spells are in which towns. 
    /// Specifically only used when shuffle spells is on but spells are not in the shuffle pool
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ShuffleSpells()
    {
        List<Location> unallocatedTowns = new List<Location> { westHyrule.rauru, westHyrule.ruto, westHyrule.sariaNorth, westHyrule.mido,
            eastHyrule.nabooru, eastHyrule.darunia, eastHyrule.newKasuto, eastHyrule.oldKasuto };

        var filteredToJustSpells = Enum.GetValues<Collectable>().Cast<Collectable>().Where(i => i.IsSpell());
        foreach (Collectable spell in filteredToJustSpells)
        {
            if (props.ReplaceFireWithDash && spell == Collectable.FIRE_SPELL
                || !props.ReplaceFireWithDash && spell == Collectable.DASH_SPELL
                || props.MarioMode && spell == Collectable.JUMP_SPELL
                || !props.MarioMode && spell == Collectable.TANOOKI
                || spell == Collectable.UPSTAB
                || spell == Collectable.DOWNSTAB)
            {
                continue;
            }
            Location townLocation = unallocatedTowns.Sample(r)!;
            townLocation.Town!.GetWizard()!.Collectable = spell;
            unallocatedTowns.Remove(townLocation);
        }
    }

    private void SwapUpAndDownstab()
    {
        eastHyrule.darunia.Town!.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!.Collectable = Collectable.DOWNSTAB;
        westHyrule.mido.Town!.GetTownMap(VanillaTownMap.MIDO_TRAINER)!.Collectable = Collectable.UPSTAB;
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
        rom.Put(0x17AF4, (byte)props.StartingMagicLevel);
        rom.Put(0x17AF5, (byte)props.StartLifeLvl);

        if (props.SpellEnemy)
        {
            //3, 4, 6, 7, 14, 16, 17, 18, 24, 25, 26
            List<int> enemies = [3, 4, 6, 7, 0x0E, 0x10, 0x11, 0x12, 0x18, 0x19, 0x1A];
            rom.Put(0x11ef, (byte)enemies[r.Next(enemies.Count)]);
        }

        //neither of these starting item handlings really belong here, but this is good enough.

        //Instead of patching out the checks for the spell items, actually update the default save data so you start with them.
        if (props.StartsWithCollectable(Collectable.TROPHY) || props.StartsWithCollectable(westHyrule.ruto.GetAllCollectables()[0]))
        {
            rom.Put(0x17b14, 0x10); //Trophy
        }

        if (props.StartsWithCollectable(Collectable.MIRROR)
            || props.StartsWithCollectable(westHyrule.sariaNorth.Town!.GetTownMap(VanillaTownMap.SARIA_WIZARD)!.Collectable))
        {
            rom.Put(0x17b15, 0x01); //Mirror
        }
        if (props.StartsWithCollectable(Collectable.MEDICINE)
            || props.StartsWithCollectable(westHyrule.mido.Town!.GetTownMap(VanillaTownMap.MIDO_WIZARD)!.Collectable))
        {
            rom.Put(0x17b16, 0x40); //Medicine
        }
        if (props.StartsWithCollectable(Collectable.WATER)
            || props.StartsWithCollectable(eastHyrule.nabooru.Town!.GetTownMap(VanillaTownMap.NABOORU_WIZARD)!.Collectable))
        {
            rom.Put(0x17b17, 0x01); //Water
        }
        if (props.StartsWithCollectable(Collectable.CHILD)
            || props.StartsWithCollectable(eastHyrule.darunia.Town!.GetTownMap(VanillaTownMap.DARUNIA_WIZARD)!.Collectable))
        {
            rom.Put(0x17b18, 0x20); //Child
        }

        rom.Put(RomMap.START_CANDLE, props.StartCandle ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_GLOVE, props.StartGlove ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_RAFT, props.StartRaft ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_BOOTS, props.StartBoots ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_FLUTE, props.StartFlute ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_CROSS, props.StartCross ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_HAMMER, props.StartHammer ? (byte)1 : (byte)0);
        rom.Put(RomMap.START_MAGICAL_KEY, props.StartKey ? (byte)1 : (byte)0);

        rom.Put(ROM.ChrRomOffset + 0x1a000, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.item_sprites.chr"));
        //replace menu/non-wizard pickup icons
        if (props.MarioMode)
        {
            //These two are double wide normally but could just be single wide and mirrored. If we really need
            //to save 2 slots in mario mode we can improve this
            rom.Put(ROM.ChrRomOffset + 0x1a0E0, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.FireFlower.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1a100, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.FireFlower.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1a120, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.Tanooki.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1a140, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.Tanooki.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1a200, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.Mario_Upstab_Varcal.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1a220, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.Mario_Downstab_Dude.chr"));
        }
        else
        {
            //Linked fire/dash custom sprites replace fire's sprite. This lets us free up c7 for future use.
            if (props.LinkedFireSpell != null)
            {
                rom.Put(ROM.ChrRomOffset + 0x1a0E0, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.linkedFire.chr"));
            }
            else if (props.ReplaceFireWithDash)
            {
                rom.Put(ROM.ChrRomOffset + 0x1a0E0, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.dash.chr"));
            }

            // Mario mode sprites and colors are too weird to allow changing it atm 
            // maybe after we fix the bugs with it...
            rom.UpdateSprite(props.CharSprite, true, props.ChangeItemSprites);
            rom.UpdateSpritePalette(props.TunicColor, props.SkinTone, props.OutlineColor, props.ShieldColor, props.BeamSprite);
        }

#pragma warning disable CS0162 // Unreachable code detected
        if (IS_RANDOMIZED)
        {
            rom.Put(ROM.ChrRomOffset + 0x01000, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.randomizer_text.chr"));
        }
        else
        {
            // We need to move the "OF" graphics over the unused zelda graphics
            // to fit the new MARIO text (whether its used or not)
            rom.Put(ROM.ChrRomOffset + 0x1000, rom.GetBytes(ROM.ChrRomOffset + 0x1380, 0x13e0 - 0x1380));
        }
#pragma warning restore CS0162 // Unreachable code detected
        if (props.MarioMode)
        {
            // Overwrite link's downstab animation with fireball explosion sprites
            var fireballExplosion = Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.fireball_explosion.chr");
            var extra_sprites = Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.tanooki_tail.chr");
            var tanooki_tail = extra_sprites[..0xa0]; // theres an extra unused tail tile in here
            var oneup = extra_sprites[0xc0..0xe0];
            var lifecount = extra_sprites[0xe0..0xf0];
            for (var i = 0; i < 13; i++)
            {
                rom.Put(ROM.ChrRomOffset + 0x0400 + (i * 0x2000), fireballExplosion);
                rom.Put(ROM.ChrRomOffset + 0x0560 + (i * 0x2000), tanooki_tail);
                rom.Put(ROM.ChrRomOffset + 0x0a80 + (i * 0x2000), oneup);
                rom.Put(ROM.ChrRomOffset + 0x1960 + (i * 0x2000), lifecount);
            }
            rom.Put(ROM.ChrRomOffset + 0x1a800, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.sprites_mario_hammer.chr"));
            rom.Put(ROM.ChrRomOffset + 0x1b800, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.sprites_mario_tanooki.chr"));
            var span = Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.map_mario.chr");
            rom.Put(ROM.ChrRomOffset + 0x11a00, span[0..0x220]);
            var title_graphics = Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.z2mario.mario_title.chr");
            rom.Put(ROM.ChrRomOffset + 0x1380, title_graphics[0..0x300]);
            rom.Put(ROM.ChrRomOffset + 0x1840, title_graphics[0x300..0x400]);
            // The title graphics also include a pipe now, place that somewhere we can use it.
            rom.Put(ROM.ChrRomOffset + 0x0ae0, title_graphics[0x400..0x500]);
            // I rewrote the star sparkle animation on the title screen to use different star sprites instead
            // I build them from copying the existing star sprite
            var starSpriteTile = rom.GetBytes(ROM.ChrRomOffset + 0x0e80, 0x20);
            // Make a palette 1 and 2 version
            var starSpriteTilePalette1 = starSpriteTile.ToArray();
            var starSpriteTilePalette2 = starSpriteTile.ToArray();
            starSpriteTilePalette1[3] = 0;
            starSpriteTilePalette2[3 + 8] = 0;
            rom.Put(ROM.ChrRomOffset + 0x0ea0, starSpriteTilePalette1);
            rom.Put(ROM.ChrRomOffset + 0x0ec0, starSpriteTilePalette2);
        }
        else
        {
            if (props.FluteWarpMode != FluteWarpMode.NONE)
            {
                rom.Put(ROM.ChrRomOffset + 0x11b40, Util.ReadBinaryResource("Z2Randomizer.RandomizerCore.Asm.Graphics.whirlwind.chr"));
            }
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

        rom.SetLevelCap(a, props.AttackCap, props.MagicCap, props.LifeCap);

        rom.ChangeLevelUpCancelling(a);

        rom.Put(0x17B10, (byte)props.StartGems);


        rom.Put(0x17B00, (byte)props.StartHearts);
        rom.Put(0x17AFF, (byte)props.StartMagicContainers);

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
            Palettes.ShufflePalacePalettes(rom, r);
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
        /*
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
        */

        foreach (Palace palace in palaces)
        {
            palace.AssertItemRoomsAreUnique(ROMData);
            palace.ValidateRoomConnections();
            palace.WriteConnections(ROMData);
        }

        /*
        ROMData.Put(0x1CD3A, (byte)palGraphics[(int)westHyrule.locationAtPalace1.Palace!.Number!]);
        ROMData.Put(0x1CD3B, (byte)palGraphics[(int)westHyrule.locationAtPalace2.Palace!.Number!]);
        ROMData.Put(0x1CD3C, (byte)palGraphics[(int)westHyrule.locationAtPalace3.Palace!.Number!]);
        ROMData.Put(0x1CD46, (byte)palGraphics[(int)mazeIsland.locationAtPalace4.Palace!.Number!]);
        ROMData.Put(0x1CD42, (byte)palGraphics[(int)eastHyrule.locationAtPalace5.Palace!.Number!]);
        ROMData.Put(0x1CD43, (byte)palGraphics[(int)eastHyrule.locationAtPalace6.Palace!.Number!]);
        ROMData.Put(0x1CD44, (byte)palGraphics[(int)eastHyrule.locationAtGP.Palace!.Number!]);
        */
        if (props.ShuffleEnemyPalettes)
        {
            Random customizationRng = new Random(SeedHash);
            Palettes.RerollSideviewPaletteTables(ROMData, customizationRng);
        }

        //WRITE UPDATES TO WIZARD/QUEST COLLECTABLES HERE

        ROMData.Put(RomMap.WEST_TROPHY_CAVE_COLLECTABLE, westHyrule.trophyCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.WEST_MAGIC_CONTAINER_CAVE_COLLECTABLE, westHyrule.magicContainerCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.WEST_HEART_CONTAINER_CAVE_COLLECTABLE, westHyrule.heartContainerCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.WEST_MEDICINE_CAVE_COLLECTABLE, westHyrule.medicineCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.WEST_GRASS_TILE_COLLECTABLE, westHyrule.grassTile.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.DM_SPECTACLE_ROCK_COLLECTABLE, deathMountain.specRock.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.DM_HAMMER_CAVE_COLLECTABLE, deathMountain.hammerCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.EAST_WATER_TILE_COLLECTABLE, eastHyrule.waterTile.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.EAST_DESERT_TILE_COLLECTABLE, eastHyrule.desertTile.GetAllCollectables()[0].GetROMCollectableIndex());

        // Update item rooms and entrances for all palaces
        Location[] palaceLocations = [
            westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3,
            eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6, eastHyrule.locationAtGP,
            mazeIsland.locationAtPalace4,
        ];
        foreach (var loc in palaceLocations)
        {
            var idx = (int)loc.Palace!.Number! - 1;
            if (loc.Palace!.Number != 7)
            {
                foreach (Room itemRoom in loc.Palace.ItemRooms)
                {
                    Debug.Assert(itemRoom.Collectable != null, $"Item room {itemRoom.Name} map {itemRoom.Map} in palace {loc.Palace.Number} is missing its item");
                    ROMData.UpdateItem((Collectable)itemRoom.Collectable, itemRoom);
                }
            }

            if (palaces[idx].Entrance == null)
            {
                throw new Exception("Invalid palace without a palace number");
            }
            var root = palaces[idx].Entrance!;
            ROMData.Put(loc.MemAddress + 0x7e, root.Map);
        }

        Collectable? collectable = (eastHyrule.newKasuto.Town!.GetTownMap(VanillaTownMap.SPELL_TOWER_INTERIOR)?.Collectable)
            ?? throw new Exception("Unable to find spell tower while setting collectables");
        ROMData.Put(RomMap.EAST_SPELL_TOWER_COLLECTABLE, ((Collectable)collectable).GetROMCollectableIndex());
        collectable = eastHyrule.newKasuto.Town!.GetTownMap(VanillaTownMap.GRANNYS_BASEMENT)!.Collectable
            ?? throw new Exception("Unable to find granny's basement while setting collectables");
        ROMData.Put(RomMap.EAST_NEW_KASUTO_BASEMENT_COLLECTABLE, ((Collectable)collectable).GetROMCollectableIndex());

        ROMData.Put(RomMap.MI_MAGIC_CONTAINER_DROP_COLLECTABLE, mazeIsland.magicContainerDrop.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.MI_CHILD_DROP_COLLECTABLE, mazeIsland.childDrop.GetAllCollectables()[0].GetROMCollectableIndex());

        ROMData.Put(RomMap.WEST_PBAG_CAVE_COLLECTABLE, westHyrule.pbagCave.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.EAST_PBAG_CAVE1_COLLECTABLE, eastHyrule.pbagCave1.GetAllCollectables()[0].GetROMCollectableIndex());
        ROMData.Put(RomMap.EAST_PBAG_CAVE2_COLLECTABLE, eastHyrule.pbagCave2.GetAllCollectables()[0].GetROMCollectableIndex());

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
        if (westHyrule.locationAtPalace1.Palace!.Number == 7 || westHyrule.locationAtPalace2.Palace!.Number == 7 || westHyrule.locationAtPalace3.Palace!.Number == 7)
        {
            ROMData.Put(0x1dd3b, 0x05);
        }

        if (mazeIsland.locationAtPalace4.Palace!.Number == 7)
        {
            ROMData.Put(0x1dd3b, 0x14);
        }

        int spellNameBase = 0x1c3a, effectBase = 0x00e58, spellCostBase = 0xd8b, functionBase = 0xdcb;

        int[,] magLevels = new int[8, 8];
        int[,] magNames = new int[8, 8];
        int[] magEffects = new int[16];
        int[] magFunction = new int[8];
        //ROMData.UpdateWizardText(WizardCollectables);

        // Add marker for linked fire. Do this before old spell name shuffle so this gets shuffled in there too
        if (props.LinkedFireSpell != null)
        {
            ROMData.Put(0x1c72, [.. ROM.StringToZ2Bytes("FIRE"), 0xFC]);
        }

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
            /*var wizardCollectables = ItemLocations()
                .Where(l => l.Town?.GetWizard() != null && l != westHyrule.sariaSouth && l != westHyrule.bagu)
                .Select(l => (Collectable)l.Town!.GetWizard()!.Collectable!).ToList();
            */

            for (int i = 0; i < magFunction.Length; i++)
            {
                magFunction[i] = ROMData.GetByte(functionBase + spellListOrder[i].VanillaSpellOrder());
            }

            for (int i = 0; i < magEffects.Length; i = i + 2)
            {
                magEffects[i] = ROMData.GetByte(effectBase + spellListOrder[i / 2].VanillaSpellOrder() * 2);
                magEffects[i + 1] = ROMData.GetByte(effectBase + spellListOrder[i / 2].VanillaSpellOrder() * 2 + 1);
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    magLevels[i, j] = ROMData.GetByte(spellCostBase + (spellListOrder[i].VanillaSpellOrder() * 8 + j));
                }

                for (int j = 0; j < 8; j++)
                {
                    magNames[i, j] = ROMData.GetByte(spellNameBase + (spellListOrder[i].VanillaSpellOrder() * 0xe + j));
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

                for (int j = 0; j < 8; j++)
                {
                    ROMData.Put(spellNameBase + (i * 0xe) + j, (byte)magNames[i, j]);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                ROMData.Put(TownExtensions.SPELL_GET_START_ADDRESS + i, props.StartsWithCollectable(spellListOrder[i]) ? (byte)1 : (byte)0);
            }
        }
        //fix for rope graphical glitch
        for (int i = 0; i < 16; i++)
        {
            ROMData.Put(ROM.ChrRomOffset + 0x12CC0 + i, ROMData.GetByte(ROM.ChrRomOffset + 0x14CC0 + i));
        }
    }

    public void RandomizeSmallItems(int bank, bool firstTable)
    {
        logger.Debug("Bank: " + bank);
        List<int> visited = [];
        List<int> items = [];
        int startAddr;
        if (firstTable) // West/East
        {
            startAddr = 0x8523 - 0x8000 + (bank * 0x4000) + 0x10;
        }
        else // DM/Maze
        {
            startAddr = 0xa000 - 0x8000 + (bank * 0x4000) + 0x10;
        }
        int map = 0;
        for (int i = startAddr; i < startAddr + 126; i = i + 2)
        {

            map++;
            int ptr = ROMData.GetShort(i + 1, i);
            int offset = 16 - 0x8000 + (bank * 0x4000);
            int numBytes = ROMData.GetByte(ptr + offset);
            for (int j = 4; j < numBytes; j = j + 2)
            {
                int yPos = ROMData.GetByte(ptr + j + offset) & 0xF0;
                yPos = yPos >> 4;
                if (ROMData.GetByte(ptr + j + 1 + offset) == 0x0F && yPos < 13)
                {
                    int addr = ptr + j + 2 + offset;
                    if (!visited.Contains(addr))
                    {
                        int item = ROMData.GetByte(addr);
                        Collectable collectable = (Collectable)item;
                        if (collectable.IsMinorItem())
                        {
#if UNSAFE_DEBUG
                            logger.Debug("Map: " + map);
                            logger.Debug("Item: " + item);
                            logger.Debug($"Address: {addr:X}");
#endif
                            visited.Add(addr);
                            items.Add(item);
                        }
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
        for (int i = 0; i < visited.Count; i++)
        {
            ROMData.Put(visited[i], (byte)items[i]);
        }
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

        for (int i = 0; i < 6; i++)
        {
            sb.AppendLine($"\nPalace {i + 1}:\n");
            sb.AppendLine(palaces[i].GetLayoutDebug(props.PalaceStyles[i], false));
        }

        sb.AppendLine("\nGP:\n");
        sb.AppendLine(palaces[6].GetLayoutDebug(props.PalaceStyles[6], false));

        sb.AppendLine("\nDROPS:\n");
        sb.AppendLine(randomizedDrops.GenerateSpoiler());

        //Currently randomizedStats only exists if stats were randomized.
        //This should just be a characterStats object that holds the stats regardless of whether they
        //are randomized or not
        if (randomizedStats != null)
        {
            sb.AppendLine("\nSTATS:\n");
            sb.AppendLine(randomizedStats.GenerateSpoiler());
        }

        sb.AppendLine("\nDETAILS: ");
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
.include "z2r.inc"

.segment "PRG5"
.org $b28d
    jsr CustomFileSelectUpdates

.reloc
CustomFileSelectUpdates:
    cpy #$01
    beq @Skip
        sta DialogBoxDrawing  ; perform the original hooked code before exiting
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

    //No longer AllLocationsForReal since all fakeness has been removed.
    public IEnumerable<Location> AllLocations()
    {
        List<Location>? locations = westHyrule?.AllLocations
            .Union(eastHyrule?.AllLocations ?? [])
            .Union(mazeIsland?.AllLocations ?? [])
            .Union(deathMountain?.AllLocations ?? []).ToList();
        return locations ?? [];
    }

    public IEnumerable<(string, int)> GetFullShuffleLocationCollectables()
    {
        return [
            ("SHIELD_SPELL_ITEMLOC", westHyrule.rauru.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("JUMP_SPELL_ITEMLOC", westHyrule.ruto.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("LIFE_SPELL_ITEMLOC", westHyrule.sariaNorth.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("FAIRY_SPELL_ITEMLOC", westHyrule.mido.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("FIRE_SPELL_ITEMLOC", eastHyrule.nabooru.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("REFLECT_SPELL_ITEMLOC", eastHyrule.darunia.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("SPELL_SPELL_ITEMLOC", eastHyrule.newKasuto.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("THUNDER_SPELL_ITEMLOC", eastHyrule.oldKasuto.Town!.GetWizard()!.Collectable!.Value.GetROMCollectableIndex()),
            ("MIRROR_ITEMLOC", westHyrule.sariaNorth.Town!.GetTownMap(VanillaTownMap.SARIA_TABLE)!.Collectable!.Value.GetROMCollectableIndex()),
            ("BAGUS_NOTE_ITEMLOC", westHyrule.bagu.Town!.GetTownMap(VanillaTownMap.BAGU_INDOORS)!.Collectable!.Value.GetROMCollectableIndex()),
            ("WATER_ITEMLOC", eastHyrule.nabooru.Town!.GetTownMap(VanillaTownMap.NABOORU_MID)!.Collectable!.Value.GetROMCollectableIndex()),
            ("DOWNSTAB_ITEMLOC", westHyrule.mido.Town!.GetTownMap(VanillaTownMap.MIDO_TRAINER)!.Collectable!.Value.GetROMCollectableIndex()),
            ("UPSTAB_ITEMLOC", eastHyrule.darunia.Town.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!.Collectable!.Value.GetROMCollectableIndex()),
        ];
    }

    public IEnumerable<Location> ItemLocations()
    {
        List<Location> possibleItemLocations = AllLocations().Where(i => i.GetCollectableCount() > 0).ToList();
        if (!props.PbagItemShuffle)
        {
            possibleItemLocations.RemoveMany([westHyrule.pbagCave, eastHyrule.pbagCave1, eastHyrule.pbagCave2]);
        }
        possibleItemLocations.Remove(westHyrule.sariaSouth);
        return possibleItemLocations;
    }


    private bool IsRaftAlwaysRequired(RandomizerProperties props)
    {
        //If continent connections are vanilla, you for sure need the raft.
        if (props.ContinentConnections == ContinentConnectionType.NORMAL)
        {
            return true;
        }
        //If west and DM are both vanilla, the caves have to connect between west and DM, and vanilla connections are forced
        if ((props.WestBiome.UsesVanillaMap())
            && (props.DmBiome.UsesVanillaMap()))
        {
            return true;
        }
        //If east is vanilla, you can only get to it from raft or bridge, so if MI is vanilla, raft is required
        if ((props.EastBiome.UsesVanillaMap())
            && (props.MazeBiome.UsesVanillaMap()))
        {
            return true;
        }
        return false;
    }

    private void FullItemShuffle(Assembler asm, IEnumerable<(string, int)> fullShuffleLocations)
    {
        var a = asm.Module();
        foreach (var collect in Enum.GetValues<Collectable>())
        {
            a.Set($"{collect.ToString().ToUpper()}_ITEMLOC", (int)collect);
        }
        foreach (var loc in fullShuffleLocations)
        {
            a.Set(loc.Item1, loc.Item2);
        }
        foreach (var val in Enum.GetValues<DialogWest>())
        {
            a.Set($"{val.ToString()}_DIALOG_WEST_INDEX", (int)val);
        }
        foreach (var val in Enum.GetValues<DialogEast>())
        {
            a.Set($"{val.ToString()}_DIALOG_EAST_INDEX", (int)val);
        }
        if (props.ShuffleSpellLocations)
        {
            var spellBytes = new byte[8];
            for (byte i = 0; i < 8; i++)
            {
                spellBytes[spellListOrder[i].VanillaSpellOrder()] = i;
            }
            a.Segment("PRG7");
            a.Reloc();
            a.Label("ShuffledSpellList");
            a.Byt(spellBytes);
        }
        a.Set("_CHECK_WIZARD_MAGIC_CONTAINER", props.DisableMagicRecs ? 0 : 1);
        a.Set("_DO_SPELL_SHUFFLE_WIZARD_UPDATE", 1); // always on now, so starting spells can be replaced by minor items
        a.Set("SHUFFLED_SPELL_LIST", props.ShuffleSpellLocations ? 1 : 0);
        a.Assign("ENABLE_Z2_MARIO", props.MarioMode ? 1 : 0);
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.FullItemShuffle.s"), "full_item_shuffle.s");
    }

    private static void RestartWithPalaceUpA(Assembler a)
    {
        a.Module().Code("""
.include "z2r.inc"

update_next_level_exp = $a057

.segment "PRG7"

.org $cbaa
    jsr PalacePatch

.reloc
PalacePatch:
    sta WorldNumber
    ; if < 3 do the original patch
    cmp #$03
    bcc @Exit
    ; or if our temp flag is already set, do the original code
    lda temp_room_flag
    bne @Exit
        lda MapNumber
        sta temp_room_code ; store the area code into a temp ram location
        inc temp_room_flag ; set a flag in another empty ram location
@Exit:
    lda WorldNumber
    rts

.reloc
ReloadExpForReset:
    lda temp_room_code
    sta MapNumber
    jsr update_next_level_exp
    lda WorldNumber
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
    sty WorldNumber
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
.include "z2r.inc"

.segment "PRG7"
.org $e18a
    jsr FixSoftlock

.reloc
FixSoftlock:
    inc DialogBoxDrawing
    lda CurrentDialogType ; branch if dialog type is not "talking"
    cmp #$02
    beq +
        ldx #$00  ; otherwise close the talking dialog
        stx CurrentDialogType
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
DrawRngForDrop := $e8ad
.org DrawRngForDrop
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

    /*
    public void SetPalacePalettes(Assembler asm)
    {
        var a = asm.Module();
        a.Assign("P1Palette", (byte)palPalettes[westHyrule?.locationAtPalace1.Palace!.Number ?? 0]);
        a.Assign("P2Palette", (byte)palPalettes[westHyrule?.locationAtPalace2.Palace!.Number ?? 1]);
        a.Assign("P3Palette", (byte)palPalettes[westHyrule?.locationAtPalace3.Palace!.Number ?? 2]);
        a.Assign("P4Palette", (byte)palPalettes[mazeIsland?.locationAtPalace4.Palace!.Number ?? 3]);
        a.Assign("P5Palette", (byte)palPalettes[eastHyrule?.locationAtPalace5.Palace!.Number ?? 4]);
        a.Assign("P6Palette", (byte)palPalettes[eastHyrule?.locationAtPalace6.Palace!.Number ?? 5]);
        a.Assign("PGreatPalette", (byte)palPalettes[eastHyrule?.locationAtGP.Palace!.Number ?? 6]);
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
    */

    public static void FixContinentTransitions(Assembler asm)
    {
        var a = asm.Module();
        a.Code("""
.include "z2r.inc"

.import SwapPRG

.segment "PRG7"

; Patch switching the bank when loading the overworld
.org $cd48
    bne +
    ldy RegionNumber
    lda ExpandedRegionBankTable,y
+   sta CurrentPrgBank
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
    .byte .lobyte(ItemPresenceBitsWest)  ; West Caves                           (Region 0)
    .byte .lobyte(ItemPresenceBitsDmMi)  ; Death Mountain / Maze Island Caves   (Region 1)
    .byte .lobyte(ItemPresenceBitsEast)  ; East Caves                           (Region 2)
    .byte .lobyte(ItemPresenceBitsDmMi)  ; Death Mountain / Maze Island Caves   (Region 3)
.reloc
bank7_Pointer_table_for_Item_Presence_ByWorld: ; this is referenced as -1, as index 0 would use the table above
    .byte .lobyte(ItemPresenceBitsTowns)  ; Towns         (World 1)
    .byte .lobyte(ItemPresenceBitsTowns)  ; Towns         (World 2)
    .byte .lobyte(ItemPresenceBitsP125)   ; Palace 125    (World 3)
    .byte .lobyte(ItemPresenceBitsP346)   ; Palace 346    (World 4)
    .byte .lobyte(ItemPresenceBitsGP)     ; Great Palace  (World 5)

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
    lda MapNumber
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

.org $8528 ; we don't use this data anymore
FREE_UNTIL $8553 ; remove unused data here
; bridge connector coordinates are at $8553

.org $8599
    stx LocationNumber  ; X stashed away like in the original code
    stx AreaEntranceIndex
    cpx #RAFT_TILE_INDEX
    bne NotRaftTileProceed
    lda HaveRaft
    beq EndTileComparisons  ; It's a raft tile and we don't have the raft
    ; Determine raft exit direction from our overworld position instead of the vanilla table
    lda OverworldXPosition
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
        a.Word(Symbol("Towns_in_West_Hyrule"));
        a.Word(Symbol("Towns_in_East_Hyrule"));

        for (var i = 0; i < hints.Count; i++)
        {
            var hint = hints[i];
            a.Reloc();
            a.Label($"HintText{i}");
            a.Byt(hint.EncodedText);
        }

        a.Reloc();
        a.Label("Towns_in_West_Hyrule");
        // There are 52 texts in this first table
        for (var i = 0; i < CustomTexts.WEST_TEXT_COUNT; i++)
        {
            var hint = hints[i];
            a.Word(Symbol($"HintText{i}"));
        }
        // and the rest are in this table
        a.Reloc();
        a.Label("Towns_in_East_Hyrule");
        for (var i = CustomTexts.WEST_TEXT_COUNT; i < hints.Count; i++)
        {
            var hint = hints[i];
            a.Word(Symbol($"HintText{i}"));
        }
    }

    private static void UpdateMovingNpcDialog(Assembler asm, List<Text> texts)
    {
        if (texts.Count == 0)
        {
            return;
        }
        var a = asm.Module();
        a.Assign("MOVING_DIALOG_COUNT", texts.Count);
        a.Segment("PRG3");
        a.Reloc();
        a.Label("MovingDialogTable");
        for (var i = 0; i < texts.Count; i++)
        {
            a.Word(Symbol($"MovingDialogText{i}"));
        }
        for (var i = 0; i < texts.Count; i++)
        {
            a.Reloc();
            a.Label($"MovingDialogText{i}");
            a.Byt(texts[i].EncodedText);
        }

        a.Code("""
.include "z2r.inc"
LoadTextFromPointer = $b609

.segment "PRG3"

; patch the random walking NPC dialog to choose a dialog on spawn
.org $973b
    jsr ChooseDialogOnSpawn
; Conveniently the other type of walking NPC spawn
; will also jump to $976B at then end
.org $9753
    jmp ChooseDialogOnSpawn

.reloc
ChooseDialogOnSpawn:
    lda RNG,x
    ; RNG modulo COUNT of texts
    cmp #MOVING_DIALOG_COUNT
    bcc +
    ; subtract until we underflow
-   sbc #MOVING_DIALOG_COUNT
    bcs -
    ; then add it back once to complete the modulo
    adc #MOVING_DIALOG_COUNT
+
    sta EnemyHP,x
    jmp $976B ; patched code

; Repoint the walking generic NPC dialogs to our new routine
.org $b46c
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog
    .word CustomMovingNpcDialog

.reloc
CustomMovingNpcDialog:
    lda EnemyHP,x ; repurposed as villager dialog option
    asl
    tay
    lda #<MovingDialogTable
    sta $00
    lda #>MovingDialogTable
    sta $01
    jmp LoadTextFromPointer
""");
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
        ROM.AssignRealPalaceLocations(a, GetPalaceOrder());
        a.Set("_ALLOW_ITEM_DUPLICATES", props.AllowImportantItemDuplicates ? 1 : 0);
        if (props.MarioMode)
            a.Assign("ENABLE_Z2_MARIO", 1);
        if (props.FluteWarpMode == FluteWarpMode.CLEARED_PALACES)
            a.Assign("FLUTE_WARP_CLEARED_PALACES", 1);
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.StatTracking.s"), "stat_tracking.s");
    }

    public void KeysPerPalace(Assembler asm, bool enabled)
    {
        var a = asm.Module();

        if (!enabled)
        {
            a.Code(/* lang=s */"""
.include "z2r.inc"
.import FlagHudUpdate

.segment "PRG7"
.org $d9e4
    jsr DecreaseKeysUpdateHud

.reloc
DecreaseKeysUpdateHud:
    dec Keys
    jmp FlagHudUpdate

.reloc
GetKeysForCurrentPalace:
    lda Keys
    rts
.export GetKeysForCurrentPalace

IncKeysForCurrentPalace:
    inc Keys
    rts
.export IncKeysForCurrentPalace

""", "keys_per_palace_disabled.s");
            return;
        }

        a.Code(/* lang=s */"""
.include "z2r.inc"
.import FlagHudUpdate

.segment "PRG0"
PauseMenuLoadKeys := $a150
.org PauseMenuLoadKeys
    jsr PauseMenuLoadKeysHook  ; was lda Keys

.reloc
; In a palace (WorldNumber >= 3): returns A = key count.
; Not in a palace:                draw spaces over the menu key count
; Clobbers X
PauseMenuLoadKeysHook:
    lda WorldNumber
    cmp #$03
    bcc @NotInPalace
    jmp GetKeysForCurrentPalace
@NotInPalace:
    lda #$f4
    sta $787f  ; blank out key symbol
    sta $7880  ; blank out x symbol
    lda #$24   ; will add up to $f4 as well
    rts

.segment "PRG7"

LockedDoorLoadKeys := $d9df
.org LockedDoorLoadKeys
    jsr GetKeysForCurrentPalace

LockedDoorDecKeys := $d9e4
.org LockedDoorDecKeys
    jsr DecKeysForCurrentPalace

KeyStabbedIncKeys := $e7b5
.org KeyStabbedIncKeys
jsr IncKeysForCurrentPalace

DropTable := $e870  ; bank7_Table_for_Probability_for_Item_given_by_killed_enemy
LoadDropTableY := $e8b9
.org LoadDropTableY
    jsr GetEnemyDrop

.reloc
; In: Y = drop index (0-7 small pool / 8-15 large pool).
; Out: A = drop item.
; Outside of a palace a key drop ($88) becomes a small bag.
; Clobbering Y is safe.
GetEnemyDrop:
    lda DropTable,y
    cmp #$88                    ; SmallItem.KEY
    bne @Done
    ldy WorldNumber
    cpy #$03
    bcs @Done                   ; World 3, 4, 5 = palaces, where keys are ok
    lda #$8a                    ; SmallItem.SMALL_BAG
@Done:
    rts

.reloc
; Sets A and Z from the current palace's key count.
; X = PalaceNumber on return.
; Callers must only reach this while in a palace (WorldNumber >= 3),
; where PalaceNumber is a valid 1-7. (or you get garbage outputs)
GetKeysForCurrentPalace:
    ldx PalaceNumber
    lda Palace1Keys-1,x
    rts
.export GetKeysForCurrentPalace

.reloc
; Input X must be PalaceNumber
; Reloads X = EnemyIndex after
; We can only spend keys inside palaces so this should always be safe.
DecKeysForCurrentPalace:
    dec Palace1Keys-1,x
    ldx EnemyIndex
    jmp FlagHudUpdate

.reloc
; IncKeysForCurrentPalace increments the current palace's key count.
; Does nothing outside a palace, to make sure a key picked up outside does not corrupt random memory.
IncKeysForCurrentPalace:
;.if EXTRA_SAFE_KEYS  ; in the future there may be 0 chance of finding keys outside of palaces
    ldx WorldNumber
    cpx #$03
    bcc @Skip
;.endif
    ldx PalaceNumber
    inc Palace1Keys-1,x
@Skip:
    ldx EnemyIndex
    rts
.export IncKeysForCurrentPalace

""", "keys_per_palace.s");
    }


    public void CustomHud(Assembler asm, bool enabled, bool keysPerPalace)
    {
        var a = asm.Module();

        if (!enabled)
        {
            // Just put an RTS placeholder. js65 will surely find an existing byte to use.
            a.Code(/* lang=s */"""
.segment "PRG7"
.reloc
FlagHudUpdate:
    rts
.export FlagHudUpdate
""");
            return;
        }

        byte[] HudCmdText(int x, int y, string text)
        {
            byte pos = (byte)((y << 5) + (x & 0b00011111));
            var stringByte = ROM.StringToZ2Bytes(text);
            byte stringLen = (byte)stringByte.Length;
            return [0x20, pos, stringLen, .. stringByte];
        }
        byte[] HudCmdSymbol(int x, int y, byte[] symbols)
        {
            byte pos = (byte)((y << 5) + (x & 0b00011111));
            var stringByte = symbols;
            byte stringLen = (byte)stringByte.Length;
            return [0x20, pos, stringLen, .. stringByte];
        }

        // VanillaHudCmd1 = $962e
        /* 20 4A 01 D1              ; magic level (D1 is replaced in code with current level)
           20 62 0B D1              ; attack level
           F4 F9                    ; empty tiles (f4) and magic bar M tile (f9)
           F4 F4 F4 F4 F4 F4 F4 F4  ; replaced in code with full/empty containers
           FF
         */
        // VanillaHudCmd2 = $9641
        /* 20 52 01 D1              ; life level
           20 6E 10 
           F4 F4 F4 F4 F4 F4 F4 F4  ; empty tiles - replaced by life containers in code
           D0 D0 D0 D0 CE D0 D0 D0  ; XP/NEXT (minus static last 0)
           FF
         */

        // static HUD (set on sideview load)
        ushort staticHudCmdAddr = 0xd0d1;
        byte[] staticHud = [
            .. HudCmdText(4, 2, "MAGIC-"),
            .. HudCmdText(13, 2, "LIFE-"),
            // .. HudCmdText(26, 2, "NEXT"),
            .. HudCmdSymbol(1, 3, [0xc9]),  // attack level sword tile (c9)
            .. HudCmdSymbol(13, 3, [0xf7]), // life bar L tile (f7)
            .. HudCmdText(30, 3, "0"),      // last level up XP zero (3 bytes trimmed)
            0x4c, 0x02, 0x03                // link back to PPU macro buffer
        ];
        a.Segment("PRG7");
        a.Org(staticHudCmdAddr);
        a.Byt(staticHud);
        a.Free("PRG7", (ushort)(staticHudCmdAddr + staticHud.Length), 0xd0fc);

        // dynamic HUD (new)
        List<byte> newHudCmd = [];

        byte rightCmdStart = (byte)newHudCmd.Count;
        newHudCmd.AddRange([.. HudCmdSymbol(23, 2, [])]);

        newHudCmd.AddRange([0x96, 0xf4]);                                 // lives tile (96)
        byte livesIndex = (byte)(newHudCmd.Count - 1);
        newHudCmd.Add(0xf4);
        newHudCmd.AddRange([0xb9, 0xf4]);                                 // key tile (b9)
        byte keyIndex = (byte)(newHudCmd.Count - 1);
        newHudCmd.Add(0xf4);
        newHudCmd.AddRange([0xcd, 0xf4]);                                 // crystal tile (cd)
        byte crystalIndex = (byte)(newHudCmd.Count - 1);

        byte rightLength = (byte)(newHudCmd.Count - rightCmdStart - 3);
        newHudCmd[rightCmdStart + 2] = rightLength;
        newHudCmd.Add(0xff);

        a.Assign("HUD_INCLUDES_KEYS", 1);
        a.Assign("KEYS_PER_PALACE", keysPerPalace ? 1 : 0);
        a.Segment("PRG0");
        a.Reloc();
        a.Label("NewHudCmd");
        a.Byt(newHudCmd.ToArray());
        a.Assign("NewHudLength", newHudCmd.Count());
        a.Assign("KeyIndex", keyIndex);
        a.Assign("LivesIndex", livesIndex);
        a.Assign("CrystalIndex", crystalIndex);
        a.Code(/* lang=s */"""
.include "z2r.inc"
.import GetItemReturn
.import GetKeysForCurrentPalace

.segment "PRG0"

.org $96ff  ; optimized code to make room for our UpdateNewHud hook
    tax
    @Loop:
        dex
        bmi @Done
        lda #$c6
        sta $030c,y
        iny
        bne @Loop
    @Done:
        jmp UpdateNewHud
FREE_UNTIL $970e

.reloc
UpdateNewHud:
    pla
    tay
    dey
    ldx #$00
    @CopyLoop:
        lda NewHudCmd,x
        sta $0302,y
        inx
        iny
        cpx #NewHudLength
        bne @CopyLoop

    lda Lives
    adc #$d0 - 2                                 ; add offset for digit tile index. -1 since carry is guaranteed to be set from CPX and -1 since we show remaining lives
    sta $0302 - (NewHudLength - LivesIndex),y

.if HUD_INCLUDES_KEYS
    .if KEYS_PER_PALACE                              ; per-palace keys: only show inside a palace
        lda WorldNumber
        cmp #$03
        bcs @DrawKeys
        lda #$f6                                     ; minus sign
        sta $0302 - (NewHudLength - KeyIndex),y
        clc
        bcc @DrawCrystals+1
    .endif
@DrawKeys:
    clc
    jsr GetKeysForCurrentPalace                  ; clobbers X
    adc #$d0                                     ; add offset for digit tile index
    sta $0302 - (NewHudLength - KeyIndex),y
.endif

@DrawCrystals:
    clc                                          ; clear just in case keys overflow
    lda Crystals
    adc #$d0                                     ; add offset for digit tile index
    sta $0302 - (NewHudLength - CrystalIndex),y

@Done:
    tya
    pha
    jmp $970e

; Insert HUD updates
.segment "PRG7"

.reloc
FlagHudUpdate:
    lda HudSectionFlag
    ora #$80
    sta HudSectionFlag
    rts
.export FlagHudUpdate

.if HUD_INCLUDES_KEYS
    .org $e7b8  ; update hud when a key is stabbed
        jmp UpdateHudGetItemReturn

    .reloc
    UpdateHudGetItemReturn:
        jsr FlagHudUpdate
        jmp GetItemReturn
.endif
""");
    }

    public void AddCredits(Assembler asm, RandomizerProperties props)
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

        if (props.MarioMode)
        {
            // New Zelda II Mario credit screens which comes after the z2 credits but before the
            // rando credits
            byte[] marioRole1 = [.. CmdText(7, 2, "Z2M PROGRAMER"), 0xff];
            byte[] marioName1 = [.. CmdText(11, 4, "JROWEBOY"), 0xff];
            byte[] marioRole2 = [.. CmdText(7, 2, "Z2M SOUND COMPOSER"), 0xff];
            byte[] marioName2 = [.. CmdText(11, 4, "HEYDON  "), 0xff];
            byte[] marioRole3 = [.. CmdText(7, 2, "SPECIAL THANKS    "), 0xff];
            byte[] marioName3 = [.. CmdText(11, 4, "STUDSX"), .. CmdText(11, 6, "QUANTAM"), 0xff];
            byte[] marioRole4 = [.. CmdText(7, 2, "SPECIAL THANKS"), 0xff];
            byte[] marioName4 = [.. CmdText(11, 4, "YOU   "), .. CmdText(11, 6, "       "), 0xff];
            a.Assign("ENABLE_Z2_MARIO", 1);

            a.Reloc(); a.Label("MarioRole1"); a.Byt(marioRole1);
            a.Reloc(); a.Label("MarioName1"); a.Byt(marioName1);
            a.Reloc(); a.Label("MarioRole2"); a.Byt(marioRole2);
            a.Reloc(); a.Label("MarioName2"); a.Byt(marioName2);
            a.Reloc(); a.Label("MarioRole3"); a.Byt(marioRole3);
            a.Reloc(); a.Label("MarioName3"); a.Byt(marioName3);
            a.Reloc(); a.Label("MarioRole4"); a.Byt(marioRole4);
            a.Reloc(); a.Label("MarioName4"); a.Byt(marioName4);
        }

        ushort[] vanillaEntries = [
            0x927D, 0x9293, 0x92A1, 0x92B7, 0x92C5, 0x92D1, 0x92E6, 0x92F2,
            0x9307, 0x9319, 0x9325, 0x9337, 0x9325, 0x937E, 0x9325, 0x9396];
        a.Reloc();
        a.Label("NewCreditTable");
        a.Word(vanillaEntries);
        if (props.MarioMode)
        {
            a.Word(Symbol("MarioRole1")); a.Word(Symbol("MarioName1"));
            a.Word(Symbol("MarioRole2")); a.Word(Symbol("MarioName2"));
            a.Word(Symbol("MarioRole3")); a.Word(Symbol("MarioName3"));
            a.Word(Symbol("MarioRole4")); a.Word(Symbol("MarioName4"));
        }
        a.Word(Symbol("NewCreditsHeader"));
        a.Word(Symbol("NewCreditsBody"));

        a.Code($$"""
.include "z2r.inc"

.segment "PRG5"

.org $9095
    lda NewCreditTable,y
.org $909a
    lda NewCreditTable+1,y

.ifdef ENABLE_Z2_MARIO
.org $9175
    cmp #$12 + 8 ; we added 8 new entries to the credits
.org $9196
    cmp #$10 + 8
.endif

; Free the now unused table and stuff
.org $9259
    FREE_UNTIL $927D
.org $934F
    FREE_UNTIL $9364
.org $9364
    FREE_UNTIL $937E
""", "add_credits.s");
    }


    private void MarioModeActivate(Assembler asm)
    {
        // "metasprite_engine.s" is included in metasprite.s
        string[] modules = ["boss.s", "integration.s", "map.s", "mario.s", "metasprite.s", "sfx.s", "CustomTitleScreen.s"];
        foreach (var mod in modules)
        {
            var a = asm.Module();
            a.Assign("ENABLE_Z2_MARIO", 1);
            a.Code(Util.ReadResource($"Z2Randomizer.RandomizerCore.Asm.z2mario.{mod}"), mod);
        }
        // Rename JUMP to TANOOKI
        var m = asm.Module();
        m.Segment("PRG0");
        m.Org(0x9c38);
        m.Byt(Util.ToGameText("TANOOKI"));
    }

    private void ApplyAsmPatches(RandomizerProperties props, Assembler engine, Random RNG, List<Text> texts, ROM rom, StatRandomizer randomizedStats, Location startLocation)
    {
        bool randomizeMusic = !props.DisableMusic && props.RandomizeMusic;

        rom.ChangeMapperToMMC5(engine, GetPalaceOrder(), props.DisableHUDLag, randomizeMusic, props.MarioMode); // will make output vary with customize tab options
        rom.ApplyHiddenLocationPatches(engine, eastHyrule.HiddenLocations, eastHyrule.biome, !props.LegacyVanillaShuffledLocations);
        // the second flag determines if we should add the "randomizer" text, but we don't have a "vanilla" option
        // right now, so this is just always off, except when i manually make an update
        rom.AddRandomizerToTitle(engine, props.MarioMode, IS_RANDOMIZED);
        AddCropGuideBoxesToFileSelect(engine);
        rom.SetStartingLocation(engine, startLocation, palaces);
        rom.SetEncounterRate(engine, props, r);
        FullItemShuffle(engine, GetFullShuffleLocationCollectables());
        rom.DontCountExpDuringTalking(engine);
        rom.FixElevatorPositionInFallRooms(engine);
        rom.AllowForChangingDoorYPosition(engine);
        rom.AllowForChangingElevatorYPosition(engine);
        rom.InstantText(engine);
        KeysPerPalace(engine, props.KeyLogic.UsesKeysPerPalace());
        CustomHud(engine, props.UpdatedHud, props.KeyLogic.UsesKeysPerPalace());
        rom.ChangeLavaKillPosition(engine);
        rom.FixItemPickup(engine, props.FastItemPickup);
        rom.CollisionFixes(engine);
        rom.FixSideviewEntryFacing(engine);
        if (!randomizeMusic) // custom music already optimizes the overworld similarly
        {
            rom.OptimizeOverworldByteDecode(engine);
        }
        rom.FixBigBubbleSplit(engine, randomizedStats);
        rom.ApplyBossPatches(engine, props, randomizedStats, palaces);
        StatTracking(props, engine);
        AddCredits(engine, props);

        if (props.MarioMode)
        {
            MarioModeActivate(engine);
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

        if (props.FluteWarpMode != FluteWarpMode.NONE)
        {
            ROM.FluteTwisterWarp(engine, props.FluteWarpMode, GetPalaceOrder(), props.MarioMode);
        }

        if (props.UpARestartsAtPalaces)
        {
            RestartWithPalaceUpA(engine);
        }

        if (props.RevealHiddenJars)
        {
            rom.RevealHiddenJars(engine);
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
        //SetPalacePalettes(engine);
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
            const byte LDX_ABS = 0xAE;
            const byte STX_ABS = 0x8E;

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

            PatchAddress(LDA_ABS, 0x6380, 0x6340);
            PatchAddress(LDA_ABS, 0x6381, 0x6341);
            PatchAddress(STA_ABS, 0x6380, 0x6340);
            PatchAddress(STA_ABS, 0x6381, 0x6341);
            PatchAddress(LDX_ABS, 0x6380, 0x6340);
            PatchAddress(LDX_ABS, 0x6381, 0x6341);
            PatchAddress(STX_ABS, 0x6380, 0x6340);
            PatchAddress(STX_ABS, 0x6381, 0x6341);
            var asm = engine.Module();
            asm.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.z2ft.s"), "z2ft.s");

            if (props.MarioMode)
            {
                // There is a patch in z2ft to reduce offscreen enemy checking overhead
                // but for the randomizer, it counters out its own lag reduction by adding
                // additional forced lag to counterbalance the lag it removed.
                // For z2mario, there's just so much more stuff going on, and people don't
                // have the same expectations for the game, so this will "unpatch" the
                // extra lag routine, so that it uses the lag reduction code.
                rom.Put(0x1ffec, 0xb4, 0xf2);
            }
        }

        UpdateMovingNpcDialog(engine, CustomTexts.BuildMovingNpcDialogPool(props));
        UpdateTexts(engine, texts);
    }

    private int[] GetPalaceOrder()
    {
        return [
            (westHyrule?.locationAtPalace1.Palace!.Number ?? 1),
            (westHyrule?.locationAtPalace2.Palace!.Number ?? 2),
            (westHyrule?.locationAtPalace3.Palace!.Number ?? 3),
            (mazeIsland?.locationAtPalace4.Palace!.Number ?? 4),
            (eastHyrule?.locationAtPalace5.Palace!.Number ?? 5),
            (eastHyrule?.locationAtPalace6.Palace!.Number ?? 6),
            (eastHyrule?.locationAtGP.Palace!.Number ?? 7)
        ];
    }

    private void AssignPalaceLocations()
    {
        List<Location> palaceLocations = [westHyrule.locationAtPalace1, westHyrule.locationAtPalace2, westHyrule.locationAtPalace3,
                mazeIsland.locationAtPalace4, eastHyrule.locationAtPalace5, eastHyrule.locationAtPalace6];
        if (props.PalacesCanSwapContinent)
        {
            if (props.P7shuffle)
            {
                palaceLocations.Add(eastHyrule.locationAtGP);
                palaceLocations.FisherYatesShuffle(r);
            }
                else
            {
                palaceLocations.FisherYatesShuffle(r);
                palaceLocations.Add(eastHyrule.locationAtGP);
           }
        }
        else
        {
            palaceLocations.Add(eastHyrule.locationAtGP);
        }
        for (int i = 0; i < 7; i++)
        {
            palaceLocations[i].Palace = palaces[i];
        }
        switch (props.KeyLogic)
        {
            case KeyLogicOption.KEYS_PER_PALACE_SOLVABLE:
                foreach (var loc in palaceLocations)
                {
                    var palace = loc.Palace!;
                    if (props.RequiresSmallKeyValidation(palace.Number))
                    {
                        loc.AccessRequirements = Requirements.NONE;
                    }
                    else if (palace.Number == 6)
                    {
                        // with vanilla palace blockers, Magic Key is in-logic for P6
                        var p6 = palaceLocations.Single(loc => loc.Palace!.Number == 6);
                        p6.AccessRequirements = new([RequirementType.MAGIC_KEY]);
                    }
                }
                break;
            case KeyLogicOption.FAIRY_OR_MAGICKEY:
                palaceLocations.ForEach(loc => loc.AccessRequirements = Requirements.DEFAULT_PALACE_REQUIREMENTS);
                break;
        }
    }

    private World GetWorld(Location location)
    {
        switch (location.Continent)
        {
            case Continent.WEST:
                return westHyrule;
            case Continent.DM:
                return deathMountain;
            case Continent.EAST:
                return eastHyrule;
            case Continent.MAZE:
                return mazeIsland;
            default:
                throw new Exception($"Location {location} is on unknown continent");
}
    }
}
