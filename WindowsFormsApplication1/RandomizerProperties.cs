using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Overworld;

namespace Z2Randomizer;

/// <summary>
/// Originally this class corresponded to the flags and controlled the logic, with all the actual configuration randomization
/// logic scattershot all over the rando. Now this class should (as much as possible) represent what the actual net configuration
/// was, and RandomizerConfiguration should represent the flags/interface, with the configuration randomization done entirly
/// in the interface between them. This migration is still a work in progress.
/// </summary>
public class RandomizerProperties
{
    public RandomizerProperties()
    {

    }

    //ROM Info
    public String filename;
    //public int seed;
    //public String flags;
    public bool saveRom = true;

    //Items
    //public bool shuffleItems;
    public bool startCandle;
    public bool startGlove;
    public bool startRaft;
    public bool startBoots;
    public bool startFlute;
    public bool startCross;
    public bool startHammer;
    public bool startKey;

    //Spells
    //public bool shuffleSpells;
    public bool startShield;
    public bool startJump;
    public bool startLife;
    public bool startFairy;
    public bool startFire;
    public bool startReflect;
    public bool startSpell;
    public bool startThunder;
    public bool combineFire;
    public bool dashSpell;

    //Other starting attributes
    public int startHearts;
    public int maxHearts;
    public bool startWithUpstab;
    public bool startWithDownstab;
    public int startLives;
    public bool permanentBeam;
    public bool useCommunityHints;
    public int startAtk;
    public int startMag;
    public int startLifeLvl;
    public bool swapUpAndDownStab;

    //Overworld
    public bool shuffleEncounters;
    public bool allowPathEnemies;
    public bool includeLavaInEncounterShuffle;
    public bool swapPalaceCont;
    public bool p7shuffle;
    public bool hiddenPalace;
    public bool hiddenKasuto;
    public bool townSwap;
    public EncounterRate encounterRate;
    public ContinentConnectionType continentConnections;
    public bool boulderBlockConnections;
    public Biome westBiome;
    public Biome eastBiome;
    public Biome mazeBiome;
    public Biome dmBiome;
    public bool vanillaOriginal;
    public bool shuffleHidden;
    public bool canWalkOnWaterWithBoots;
    public bool bagusWoods;

    //Palaces
    //public bool shufflePalaceRooms;
    public PalaceStyle palaceStyle;
    public int startGems;
    public bool requireTbird;
    public bool palacePalette;
    public bool upaBox;
    public bool shortenGP;
    public bool removeTbird;
    public bool bossItem;
    //public bool createPalaces;
    public bool useCommunityRooms;
    public bool blockersAnywhere;
    public bool bossRoomConnect;

    //Enemies
    public bool shuffleEnemyHP;
    public bool shuffleEnemyStealExp;
    public bool shuffleStealExpAmt;
    public bool shuffleSwordImmunity;
    public bool shuffleOverworldEnemies;
    public bool shufflePalaceEnemies;
    public bool mixEnemies;
    public bool shuffleDripper;
    public bool shuffleEnemyPalettes;
    public StatEffectiveness expLevel;

    //Levels
    //public bool shuffleAllExp;
    public bool shuffleAtkExp;
    public bool shuffleMagicExp;
    public bool shuffleLifeExp;
    //public bool shuffleAtkEff;
    //public bool shuffleMagEff;
    //public bool shuffleLifeEff;
    public bool shuffleLifeRefill;
    public bool shuffleSpellLocations;
    public bool disableMagicRecs;
    /*
    public bool ohkoEnemies;
    public bool tankMode;
    public bool ohkoLink;
    public bool wizardMode;
    public bool highAtk;
    public bool lowAtk;
    public bool highDef;
    public bool highMag;
    public bool lowMag;
    */
    public StatEffectiveness attackEffectiveness;
    public StatEffectiveness magicEffectiveness;
    public StatEffectiveness lifeEffectiveness;
    public int attackCap;
    public int magicCap;
    public int lifeCap;
    public bool scaleLevels;
    public bool hideLocs;
    public bool saneCaves;
    public bool spellEnemy;

    //Items
    public bool shuffleOverworldItems;
    public bool shufflePalaceItems;
    public bool mixOverworldPalaceItems;
    public bool shuffleSmallItems;
    public bool extraKeys;
    public bool kasutoJars;
    public bool pbagItemShuffle;
    public bool removeSpellItems;
    public bool shufflePbagXp;

    //Drops
    public bool shuffleItemDropFrequency;
    //public bool shuffleEnemyDrops;
    public bool smallbluejar;
    public bool smallredjar;
    public bool small50;
    public bool small100;
    public bool small200;
    public bool small500;
    public bool small1up;
    public bool smallkey;

    public bool largebluejar;
    public bool largeredjar;
    public bool large50;
    public bool large100;
    public bool large200;
    public bool large500;
    public bool large1up;
    public bool largekey;
    public bool standardizeDrops;

    //Hints
    public bool spellItemHints;
    public bool helpfulHints;
    public bool townNameHints;

    //Misc.
    public bool disableBeep;
    public bool jumpAlwaysOn;
    public bool fastCast;
    public String beamSprite;
    public bool disableMusic;
    public CharacterSprite charSprite;
    public String tunicColor;
    public String shieldColor;
    public bool upAC1;
    public bool removeFlashing;
    public bool useCustomRooms;
}
