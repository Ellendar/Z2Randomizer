using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    /// <summary>
    /// This is a hacky version of how this should work. EVENTUALLY the entirety of the options from the config should
    /// be stored into a single object that this and Hyrule use as an input, and then Hyrule should ouput a single state object.
    /// Then this can persist the state, and the main application carries a writer that just writes the output state to the ROM.
    /// This will allow us to use some simple ORM to persist the entirety of the input and output.
    /// 
    /// For now, we're just picking out a few choice fields that i'm interested in and recording them.
    /// </summary>
    class Statistics
    {
        private static readonly String BASE64 = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz1234567890!@#$";
        //This is a lazy backwards implementation Digshake's base64 encoding system.
        //There should be a seperate class that does the full encode/decode cycle for both projects.
        private static Dictionary<char, int> BASE64_DECODE = new Dictionary<char, int>(64)
        {
            {'A', 0},
            {'B', 1},
            {'C', 2},
            {'D', 3},
            {'E', 4},
            {'F', 5},
            {'G', 6},
            {'H', 7},
            {'J', 8},
            {'K', 9},
            {'L', 10},
            {'M', 11},
            {'N', 12},
            {'O', 13},
            {'P', 14},
            {'Q', 15},
            {'R', 16},
            {'S', 17},
            {'T', 18},
            {'U', 19},
            {'V', 20},
            {'W', 21},
            {'X', 22},
            {'Y', 23},
            {'Z', 24},

            {'a', 25},
            {'b', 26},
            {'c', 27},
            {'d', 28},
            {'e', 29},
            {'f', 30},
            {'g', 31},
            {'h', 32},
            {'i', 33},
            {'j', 34},
            {'k', 35},
            {'m', 36},
            {'n', 37},
            {'o', 38},
            {'p', 39},
            {'q', 40},
            {'r', 41},
            {'s', 42},
            {'t', 43},
            {'u', 44},
            {'v', 45},
            {'w', 46},
            {'x', 47},
            {'y', 48},
            {'z', 49},

            {'1', 50},
            {'2', 51},
            {'3', 52},
            {'4', 53},
            {'5', 54},
            {'6', 55},
            {'7', 56},
            {'8', 57},
            {'9', 58},
            {'0', 59},
            {'!', 60},
            {'@', 61},
            {'#', 62},
            {'$', 63},


        };


        private static readonly string FLAGS = "hAhhD0j9$78$Jp5$$gAhOAdEScuA";
        private static readonly string VANILLA_ROM_PATH = "C:\\emu\\NES\\roms\\Zelda II - The Adventure of Link (USA).nes";
        private static readonly string DB_PATH = "C:\\Workspace\\Z2Randomizer\\Statistics\\db\\stats.sqlite";
        private static readonly int LIMIT = 1000;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main()
        {
            StatisticsDbContext dbContext = new StatisticsDbContext(DB_PATH);

            Random random = new Random();
            logger.Info("Started statistics generation with limit: " + LIMIT);
            for (int i = 0; i < LIMIT; i++)
            {
                RandomizerProperties properties = GetPropertiesFromFlags(FLAGS);
                properties.flags = FLAGS;
                properties.seed = random.Next(1000000000);
                properties.filename = VANILLA_ROM_PATH;
                BackgroundWorker backgroundWorker = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                DateTime startTime = DateTime.Now;
                Hyrule hyrule = new Hyrule(properties, backgroundWorker);
                DateTime endTime = DateTime.Now;
                Result result = new Result(hyrule, properties);
                result.GenerationTime = (int)(endTime - startTime).TotalMilliseconds;
                dbContext.Add(result);
                logger.Info("Finished seed# " + i + " in: " + result.GenerationTime + "ms");
                dbContext.SaveChanges();
            }
        }

        //TODO: Why is RandomizerProperties a struct? Shouldn't this just be a constructor? If C#6 ended up adding value constructors
        //for structs i'd just use that. For now we have this hack, which is just the reverse of MainUI.UpdateFlags
        private static RandomizerProperties GetPropertiesFromFlags(String flags)
        {
            RandomizerProperties properties = new RandomizerProperties();

            BitArray bits;
            int i = 0;

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));

            properties.shuffleItems = bits[0];
            properties.startCandle = bits[1];
            properties.startGlove = bits[2];
            properties.startRaft = bits[3];
            properties.startBoots = bits[4];
            properties.shuffleOverworldEnemies = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startFlute = bits[0];
            properties.startCross = bits[1];
            properties.startHammer = bits[2];
            properties.startKey = bits[3];
            properties.shuffleSpells = bits[4];
            properties.hideLocs = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startShield = bits[0];
            properties.startJump = bits[1];
            properties.startLife = bits[2];
            properties.startFairy = bits[3];
            properties.startFire = bits[4];
            properties.combineFire = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startReflect = bits[0];
            properties.startSpell = bits[1];
            properties.startThunder = bits[2];
            properties.shuffleLives = bits[3];
            properties.removeTbird = bits[4];
            properties.saneCaves = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            //For some reason the low 3 bits of the heart container start setting are stored on one byte and the 4th bit is disjointed on the next bite...
            BitArray nextBits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startHearts = ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0) + (nextBits[2] ? 8 : 0) + 1).ToString();
            if(properties.startHearts == "9")
            {
                properties.startHearts = "Random";
            }
            switch((bits[3] ? 1 : 0) + (bits[4] ? 2 : 0) + (bits[5] ? 4 : 0))
            {
                case 0:
                    properties.startTech = "None";
                    break;
                case 1:
                    properties.startTech = "Downstab";
                    break;
                case 2:
                    properties.startTech = "Upstab";
                    break;
                case 3:
                    properties.startTech = "Both";
                    break;
                case 4:
                    properties.startTech = "Random";
                    break;
            }

            properties.pbagDrop = nextBits[0];
            properties.pbagItemShuffle = nextBits[1];
            properties.p7shuffle = nextBits[3];
            properties.palacePalette = nextBits[4];
            properties.shuffleEncounters = nextBits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.extraKeys = bits[0];
            properties.swapPalaceCont = bits[1];
            switch ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0))
            {
                case 0:
                    properties.startTech = "None";
                    break;
                case 1:
                    properties.startTech = "Downstab";
                    break;
                case 2:
                    properties.startTech = "Upstab";
                    break;
                case 3:
                    properties.startTech = "Both";
                    break;
                case 4:
                    properties.startTech = "Random";
                    break;
            }
            properties.allowPathEnemies = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.permanentBeam = bits[0];
            properties.shuffleDripper = bits[1];
            properties.dashSpell = bits[2];
            properties.shuffleEnemyHP = bits[3];
            properties.shuffleAllExp = bits[4];
            properties.shufflePalaceEnemies = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.shuffleAtkExp = bits[0];
            properties.shuffleLifeExp = bits[1];
            properties.shuffleMagicExp = bits[2];
            properties.upaBox = bits[3];
            properties.shortenGP = bits[4];
            properties.requireTbird = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0))
            {
                case 0:
                    properties.shuffleMagEff = true;
                    break;
                case 1:
                    properties.highMag = true;
                    break;
                case 2:
                    //Vanilla
                    break;
                case 3:
                    properties.lowMag = true;
                    break;
                case 4:
                    properties.wizardMode = true;
                    break;
            }
            properties.shuffleEnemyStealExp = bits[3];
            properties.shuffleStealExpAmt = bits[4];
            properties.shuffleLifeRefill = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.shuffleSwordImmunity = bits[0];
            properties.jumpAlwaysOn = bits[1];
            properties.startGems = ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0)).ToString();
            if(properties.startGems == "7")
            {
                properties.startGems = "Random";
            }
            properties.mixEnemies = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.shufflePalaceItems = bits[0];
            properties.shuffleOverworldItems = bits[1];
            properties.mixOverworldPalaceItems = bits[2];
            properties.shuffleSmallItems = bits[3];
            properties.shuffleSpellLocations = bits[4];
            properties.disableMagicRecs = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0))
            {
                case 0:
                    properties.shuffleLifeEff = true;
                    break;
                case 1:
                    properties.ohkoLink = true;
                    break;
                case 2:
                    //Vanilla
                    break;
                case 3:
                    properties.highDef = true;
                    break;
                case 4:
                    properties.tankMode = true;
                    break;
            }
            properties.kasutoJars = bits[3];
            properties.useCommunityHints = bits[4];
            properties.shuffleEnemyPalettes = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.maxHearts = ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0) + (bits[3] ? 8 : 0) + 1).ToString();
            if (properties.maxHearts == "9")
            {
                properties.maxHearts = "Random";
            }
            switch ((bits[4] ? 1 : 0) + (bits[5] ? 2 : 0))
            {
                case 0:
                    properties.hiddenPalace = "Off";
                    break;
                case 1:
                    properties.hiddenPalace = "On";
                    break;
                case 2:
                    properties.hiddenPalace = "Random";
                    break;
            }

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0))
            {
                case 0:
                    properties.hiddenKasuto = "Off";
                    break;
                case 1:
                    properties.hiddenKasuto = "On";
                    break;
                case 2:
                    properties.hiddenKasuto = "Random";
                    break;
            }
            properties.ShuffleEnemyDrops = bits[2];
            properties.removeSpellItems = bits[3];
            properties.smallbluejar = bits[4];
            properties.smallredjar = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.small50 = bits[0];
            properties.small100 = bits[1];
            properties.small200 = bits[2];
            properties.small500 = bits[3];
            properties.small1up = bits[4];
            properties.smallkey = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.largebluejar = bits[0];
            properties.largeredjar = bits[1];
            properties.large50 = bits[2];
            properties.large100 = bits[3];
            properties.large200 = bits[4];
            properties.large500 = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.large1up = bits[0];
            properties.largekey = bits[1];
            properties.helpfulHints = bits[2];
            properties.spellItemHints = bits[3];
            properties.standardizeDrops = bits[4];
            properties.randoDrops = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            nextBits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.shufflePbagXp = bits[0];
            properties.attackCap = (8 - (bits[1] ? 1 : 0) + (bits[2] ? 2 : 0) + (bits[3] ? 4 : 0));
            properties.magicCap = (8 - (bits[4] ? 1 : 0) + (bits[5] ? 2 : 0) + (nextBits[0] ? 4 : 0));
            properties.lifeCap = (8 - (nextBits[1] ? 1 : 0) + (nextBits[2] ? 2 : 0) + (nextBits[3] ? 4 : 0));
            properties.scaleLevels = bits[4];
            properties.townNameHints = bits[5];


            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0))
            {
                case 0:
                    properties.encounterRate = "Normal";
                    break;
                case 1:
                    properties.encounterRate = "50%";
                    break;
                case 2:
                    properties.encounterRate = "None";
                    break;
            }
            switch ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0))
            {
                case 0:
                    properties.expLevel = "Vanilla";
                    break;
                case 1:
                    properties.expLevel = "None";
                    break;
                case 2:
                    properties.expLevel = "Low";
                    break;
                case 3:
                    properties.expLevel = "Average";
                    break;
                case 4:
                    properties.expLevel = "High";
                    break;
            }
            bool startAttackLowBit = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startAtk = (startAttackLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + 1;
            properties.startMag = (bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0) + 1;
            bool startLifeLowBit = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.startLifeLvl = (startLifeLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + 1;
            switch((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0))
            {
                case 0:
                    properties.continentConnections = "Normal";
                    break;
                case 1:
                    properties.continentConnections = "R+B Border Shuffle";
                    break;
                case 2:
                    properties.continentConnections = "Transportation Shuffle";
                    break;
                case 3:
                    properties.continentConnections = "Anything Goes";
                    break;
            }
            properties.boulderBlockConnections = bits[4];
            bool westBiomeLowBit = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((westBiomeLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + (bits[2] ? 8 : 0))
            {
                case 0:
                    properties.westBiome = "Vanilla";
                    break;
                case 1:
                    properties.westBiome = "Vanilla (shuffled)";
                    break;
                case 2:
                    properties.westBiome = "Vanilla-Like";
                    break;
                case 3:
                    properties.westBiome = "Islands";
                    break;
                case 4:
                    properties.westBiome = "Canyon";
                    break;
                case 5:
                    properties.westBiome = "Caldera";
                    break;
                case 6:
                    properties.westBiome = "Mountainous";
                    break;
                case 7:
                    properties.westBiome = "Random (no Vanilla)";
                    break;
                case 8:
                    properties.westBiome = "Random (with Vanilla)";
                    break;
            }
            int dmBiome = (bits[3] ? 1 : 0) + (bits[4] ? 2 : 0) + (bits[5] ? 4 : 0);

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch (dmBiome + (bits[0] ? 8 : 0))
            {
                case 0:
                    properties.dmBiome = "Vanilla";
                    break;
                case 1:
                    properties.dmBiome = "Vanilla (shuffled)";
                    break;
                case 2:
                    properties.dmBiome = "Vanilla-Like";
                    break;
                case 3:
                    properties.dmBiome = "Islands";
                    break;
                case 4:
                    properties.dmBiome = "Canyon";
                    break;
                case 5:
                    properties.dmBiome = "Caldera";
                    break;
                case 6:
                    properties.dmBiome = "Mountainous";
                    break;
                case 7:
                    properties.dmBiome = "Random (no Vanilla)";
                    break;
                case 8:
                    properties.dmBiome = "Random (with Vanilla)";
                    break;
            }

            switch ((bits[1] ? 1 : 0) + (bits[2] ? 2 : 0) + (bits[3] ? 4 : 0) + (bits[4] ? 8 : 0))
            {
                case 0:
                    properties.eastBiome = "Vanilla";
                    break;
                case 1:
                    properties.eastBiome = "Vanilla (shuffled)";
                    break;
                case 2:
                    properties.eastBiome = "Vanilla-Like";
                    break;
                case 3:
                    properties.eastBiome = "Islands";
                    break;
                case 4:
                    properties.eastBiome = "Canyon";
                    break;
                case 5:
                    properties.eastBiome = "Volcano";
                    break;
                case 6:
                    properties.eastBiome = "Mountainous";
                    break;
                case 7:
                    properties.eastBiome = "Random (no Vanilla)";
                    break;
                case 8:
                    properties.eastBiome = "Random (with Vanilla)";
                    break;
            }
            bool mazeBiomeLowBit = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            switch ((mazeBiomeLowBit ? 1 : 0) + (bits[0] ? 2 : 0))
            {
                case 0:
                    properties.mazeBiome = "Vanilla";
                    break;
                case 1:
                    properties.mazeBiome = "Vanilla (shuffled)";
                    break;
                case 2:
                    properties.mazeBiome = "Vanilla-Like";
                    break;
                case 3:
                    properties.mazeBiome = "Random (with Vanilla)";
                    break;
            }
            properties.vanillaOriginal = bits[1];
            properties.shuffleHidden = bits[2];
            properties.bossItem = bits[3];
            properties.bootsWater = bits[4];
            properties.spellEnemy = bits[5];

            bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
            properties.bagusWoods = bits[0];
            switch ((bits[1] ? 1 : 0) + (bits[5] ? 2 : 0))
            {
                case 0:
                    //vanilla
                    break;
                case 1:
                    properties.shufflePalaceRooms = true;
                    break;
                case 2:
                    properties.createPalaces = true;
                    break;
            }
            properties.customRooms = bits[2];
            properties.blockersAnywhere = bits[3];
            properties.bossRoomConnect = bits[4];

            //These properties aren't stored in the flags, but aren't defaulted out in properties and will break if they are null.
            //Probably properties at some point should stop being a struct and default these in the right place
            properties.charSprite = "Link";
            properties.tunicColor = "Default";
            properties.shieldColor = "Orange";
            properties.beamSprite = "Default";

            properties.disableBeep = true;
            properties.disableMusic = false;
            properties.fastCast = true;
            properties.shuffleEnemyPalettes = false;
            properties.upaBox = false;

            return properties;
        }
    }
}
