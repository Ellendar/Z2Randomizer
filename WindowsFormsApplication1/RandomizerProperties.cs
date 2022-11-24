using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    public class RandomizerProperties
    {
        public RandomizerProperties()
        {

        }

        //ROM Info
        public String filename;
        public int seed;
        public String flags;
        public bool saveRom = true;

        //Items
        public Boolean shuffleItems;
        public Boolean startCandle;
        public Boolean startGlove;
        public Boolean startRaft;
        public Boolean startBoots;
        public Boolean startFlute;
        public Boolean startCross;
        public Boolean startHammer;
        public Boolean startKey;

        //Spells
        public Boolean shuffleSpells;
        public Boolean startShield;
        public Boolean startJump;
        public Boolean startLife;
        public Boolean startFairy;
        public Boolean startFire;
        public Boolean startReflect;
        public Boolean startSpell;
        public Boolean startThunder;
        public Boolean combineFire;
        public Boolean dashSpell;

        //Other starting attributes
        public String startHearts;
        public String maxHearts;
        public String startTech;
        public Boolean shuffleLives;
        public Boolean permanentBeam;
        public Boolean useCommunityHints;
        public int startAtk;
        public int startMag;
        public int startLifeLvl;

        //Overworld
        public Boolean shuffleEncounters;
        public Boolean allowPathEnemies;
        public Boolean swapPalaceCont;
        public Boolean p7shuffle;
        public String hiddenPalace;
        public String hiddenKasuto;
        public Boolean townSwap;
        public String encounterRate;
        public String continentConnections;
        public Boolean boulderBlockConnections;
        public String westBiome;
        public String eastBiome;
        public String mazeBiome;
        public String dmBiome;
        public Boolean vanillaOriginal;
        public Boolean shuffleHidden;
        public Boolean canWalkOnWaterWithBoots;
        public Boolean bagusWoods;

        //Palaces
        public Boolean shufflePalaceRooms;
        public String startGems;
        public Boolean requireTbird;
        public Boolean palacePalette;
        public Boolean upaBox;
        public Boolean shortenGP;
        public Boolean removeTbird;
        public Boolean bossItem;
        public Boolean createPalaces;
        public Boolean customRooms;
        public Boolean blockersAnywhere;
        public Boolean bossRoomConnect;

        //Enemies
        public Boolean shuffleEnemyHP;
        public Boolean shuffleEnemyStealExp;
        public Boolean shuffleStealExpAmt;
        public Boolean shuffleSwordImmunity;
        public Boolean shuffleOverworldEnemies;
        public Boolean shufflePalaceEnemies;
        public Boolean mixEnemies;
        public Boolean shuffleDripper;
        public Boolean shuffleEnemyPalettes;
        public String expLevel;

        //Levels
        public Boolean shuffleAllExp;
        public Boolean shuffleAtkExp;
        public Boolean shuffleMagicExp;
        public Boolean shuffleLifeExp;
        public Boolean shuffleAtkEff;
        public Boolean shuffleMagEff;
        public Boolean shuffleLifeEff;
        public Boolean shuffleLifeRefill;
        public Boolean shuffleSpellLocations;
        public Boolean disableMagicRecs;
        public Boolean ohkoEnemies;
        public Boolean tankMode;
        public Boolean ohkoLink;
        public Boolean wizardMode;
        public Boolean highAtk;
        public Boolean lowAtk;
        public Boolean highDef;
        public Boolean highMag;
        public Boolean lowMag;
        public int attackCap;
        public int magicCap;
        public int lifeCap;
        public Boolean scaleLevels;
        public Boolean hideLocs;
        public Boolean saneCaves;
        public Boolean spellEnemy;

        //Items
        public Boolean shuffleOverworldItems;
        public Boolean shufflePalaceItems;
        public Boolean mixOverworldPalaceItems;
        public Boolean shuffleSmallItems;
        public Boolean extraKeys;
        public Boolean kasutoJars;
        public Boolean pbagItemShuffle;
        public Boolean removeSpellItems;
        public Boolean shufflePbagXp;

        //Drops
        public Boolean pbagDrop;
        public Boolean ShuffleEnemyDrops;
        public Boolean smallbluejar;
        public Boolean smallredjar;
        public Boolean small50;
        public Boolean small100;
        public Boolean small200;
        public Boolean small500;
        public Boolean small1up;
        public Boolean smallkey;
        public Boolean largebluejar;
        public Boolean largeredjar;
        public Boolean large50;
        public Boolean large100;
        public Boolean large200;
        public Boolean large500;
        public Boolean large1up;
        public Boolean largekey;
        public Boolean standardizeDrops;
        public Boolean randoDrops;

        //Hints
        public Boolean spellItemHints;
        public Boolean helpfulHints;
        public Boolean townNameHints;

        //Misc.
        public Boolean disableBeep;
        public Boolean jumpAlwaysOn;
        public Boolean fastCast;
        public String beamSprite;
        public Boolean disableMusic;
        public String charSprite;
        public String tunicColor;
        public String shieldColor;
        public Boolean upAC1;
        public Boolean removeFlashing;
    }
}
