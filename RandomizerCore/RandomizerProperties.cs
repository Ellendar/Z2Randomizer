using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Originally this class corresponded to the flags and controlled the logic, with all the actual configuration randomization
/// logic scattershot all over the rando. Now this class should (as much as possible) represent what the actual net configuration
/// was, and RandomizerConfiguration should represent the flags/interface, with the configuration randomization done entirly
/// in the interface between them.
/// </summary>

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(RandomizerProperties))]
[JsonSerializable(typeof(Climate))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
public class RandomizerProperties
{

    //ROM Info
    public string? Seed { get; set; }
    public string? Flags { get; set; }

    //Items
    //public bool shuffleItems;
    public bool StartCandle { get; set; }
    public bool StartGlove { get; set; }
    public bool StartRaft { get; set; }
    public bool StartBoots { get; set; }
    public bool StartFlute { get; set; }
    public bool StartCross { get; set; }
    public bool StartHammer { get; set; }
    public bool StartKey { get; set; }
    public StartingResourceLimit StartItemsLimit { get; set; }

    //Spells
    //public bool shuffleSpells;
    public bool StartShield { get; set; }
    public bool StartJump { get; set; }
    public bool StartLife { get; set; }
    public bool StartFairy { get; set; }
    public bool StartFire { get; set; }
    public bool StartReflect { get; set; }
    public bool StartSpell { get; set; }
    public bool StartThunder { get; set; }
    public bool CombineFire { get; set; }
    public bool ReplaceFireWithDash { get; set; }
    public StartingResourceLimit StartSpellsLimit { get; set; }

    //Other starting attributes
    public int StartHearts { get; set; }
    public int MaxHearts { get; set; }
    public bool StartWithUpstab { get; set; }
    public bool StartWithDownstab { get; set; }
    public int StartLives { get; set; }
    public bool PermanentBeam { get; set; }
    public bool UseCommunityText { get; set; }
    public int StartAtk { get; set; }
    public int StartMag { get; set; }
    public int StartLifeLvl { get; set; }
    public bool SwapUpAndDownStab { get; set; }

    //Overworld
    public bool ShuffleEncounters { get; set; }
    public bool AllowPathEnemies { get; set; }
    public bool IncludeLavaInEncounterShuffle { get; set; }
    public bool PalacesCanSwapContinent { get; set; }
    public bool P7shuffle { get; set; }
    public bool HiddenPalace { get; set; }
    public bool HiddenKasuto { get; set; }
    public bool TownSwap { get; set; }
    public EncounterRate EncounterRates { get; set; }
    public ContinentConnectionType ContinentConnections { get; set; }
    public OverworldSizeOption WestSize { get; set; }
    public OverworldSizeOption EastSize { get; set; }
    public DmSizeOption DmSize { get; set; }
    public MazeSizeOption MazeSize { get; set; }
    public bool BoulderBlockConnections { get; set; }
    public Biome WestBiome { get; set; }
    public Biome EastBiome { get; set; }
    public Biome DmBiome { get; set; }
    public Biome MazeBiome { get; set; }
    public bool DmIsHorizontal { get; set; }
    public bool WestIsHorizontal { get; set; }
    public bool EastIsHorizontal { get; set; }
    public bool EastRockIsPath { get; set; }
#pragma warning disable CS8618
    public ClimateEnum WestClimate { get; set; }
    public ClimateEnum EastClimate { get; set; }
    public ClimateEnum DmClimate { get; set; }
#pragma warning restore CS8618 
    public bool VanillaShuffleUsesActualTerrain { get; set; }
    public bool ShuffleHidden { get; set; }
    public bool CanWalkOnWaterWithBoots { get; set; }
    public bool BagusWoods { get; set; }
    public LessImportantLocationsOption LessImportantLocationsOption { get; set; }
    public bool SaneCaves { get; set; }
    public RiverDevilBlockerOption RiverDevilBlockerOption { get; set; }
    public bool EastRocks { get; set; }

    //Palaces
    [NotMapped]
    public PalaceStyle[] PalaceStyles { get; set; } = new PalaceStyle[7];
    [NotMapped]
    public int[] PalaceLengths { get; set; } = new int[7];
    public int StartGems { get; set; }
    public bool RequireTbird { get; set; }
    public int DarkLinkMinDistance { get; set; }
    public bool ShufflePalacePalettes { get; set; }
    public bool UpARestartsAtPalaces { get; set; }
    public bool Global5050JarDrop { get; set; }
    public bool ReduceDripperVariance { get; set; }
    public bool RemoveTbird { get; set; }
    public bool BossItem { get; set; }
    public bool BlockersAnywhere { get; set; }
    public bool IncludeDropRooms { get; set; }
    public bool IncludeLongDeadEnds { get; set; }
    [NotMapped]
    public bool[] BossRoomsExitToPalace { get; set; } = new bool[7];
    public bool NoDuplicateRooms { get; set; }
    public bool NoDuplicateRoomsBySideview { get; set; }
    public bool GeneratorsAlwaysMatch { get; set; }
    public bool AllowVanillaRooms { get; set; }
    public bool AllowV4Rooms { get; set; }
    public bool AllowV5_0Rooms { get; set; }
    public bool HardBosses { get; set; }
    [NotMapped]
    public int[] PalaceItemRoomCounts { get; set; } = new int[6];
    public bool UsePalaceItemRoomCountIndicator { get; set; }
    public bool RevealWalkthroughWalls { get; set; }

    //Enemies
    public EnemyLifeOption ShuffleEnemyHP { get; set; }
    public EnemyLifeOption ShuffleBossHP { get; set; }
    public bool ShuffleEnemyStealExp { get; set; }
    public bool ShuffleStealExpAmt { get; set; }
    public bool ShuffleSwordImmunity { get; set; }
    public bool ShuffleOverworldEnemies { get; set; }
    public bool ShufflePalaceEnemies { get; set; }
    public bool MixLargeAndSmallEnemies { get; set; }
    public DripperEnemyOption DripperEnemyOption { get; set; }
    public bool ShuffleEnemyPalettes { get; set; }
    public XPEffectiveness EnemyXPDrops { get; set; }

    //Levels
    public bool ShuffleAtkExp { get; set; }
    public bool ShuffleMagicExp { get; set; }
    public bool ShuffleLifeExp { get; set; }
    public bool ShuffleLifeRefill { get; set; }
    public bool ShuffleSpellLocations { get; set; }
    public bool DisableMagicRecs { get; set; }
    public AttackEffectiveness AttackEffectiveness { get; set; }
    public MagicEffectiveness MagicEffectiveness { get; set; }
    public LifeEffectiveness LifeEffectiveness { get; set; }
    public int AttackCap { get; set; }
    public int MagicCap { get; set; }
    public int LifeCap { get; set; }
    public bool ScaleLevels { get; set; }
    public bool SpellEnemy { get; set; }

    //Items
    public bool ShuffleOverworldItems { get; set; }
    public bool ShufflePalaceItems { get; set; }
    public bool MixOverworldPalaceItems { get; set; }
    public bool IncludeSpellsInShuffle { get; set; }
    public bool IncludeSwordTechsInShuffle { get; set; }
    //Bagu's note / fountain water / saria mirror
    public bool IncludeQuestItemsInShuffle { get; set; }
    public bool RandomizeSmallItems { get; set; }
    public bool ExtraKeys { get; set; }
    public bool AllowImportantItemDuplicates { get; set; }
    public bool RandomizeNewKasutoBasementRequirement { get; set; }
    //Include PBag caves in item shuffle
    public bool PbagItemShuffle { get; set; }
    public bool StartWithSpellItems { get; set; }
    public bool ShufflePbagXp { get; set; }
    public HashSet<Collectable> StartingSpells { get; set; } = [];
    public HashSet<Collectable> RemoveItems { get; set; } = [];

    //Drops
    public bool ShuffleItemDropFrequency { get; set; }
    //public bool shuffleEnemyDrops { get; set; }
    public bool Smallbluejar { get; set; }
    public bool Smallredjar { get; set; }
    public bool Small50 { get; set; }
    public bool Small100 { get; set; }
    public bool Small200 { get; set; }
    public bool Small500 { get; set; }
    public bool Small1up { get; set; }
    public bool Smallkey { get; set; }

    public bool Largebluejar { get; set; }
    public bool Largeredjar { get; set; }
    public bool Large50 { get; set; }
    public bool Large100 { get; set; }
    public bool Large200 { get; set; }
    public bool Large500 { get; set; }
    public bool Large1up { get; set; }
    public bool Largekey { get; set; }
    public bool StandardizeDrops { get; set; }
    public bool RandomizeDrops { get; set; }

    //Hints
    public bool SpellItemHints { get; set; }
    public bool HelpfulHints { get; set; }
    public bool TownNameHints { get; set; }

    //Misc.
    public byte BeepFrequency { get; set; }
    public byte BeepThreshold { get; set; }
    public bool JumpAlwaysOn { get; set; }
    public bool DashAlwaysOn { get; set; }
    public bool FastCast { get; set; }
    public BeamSprites BeamSprite { get; set; }
    public bool DisableMusic { get; set; }
    public bool RandomizeMusic { get; set; }
    public bool MixCustomAndOriginalMusic { get; set; }
    public bool IncludeDiverseMusic { get; set; }
    public bool DisableUnsafeMusic { get; set; }
    [NotMapped]
#pragma warning disable CS8618 
    public CharacterSprite CharSprite { get; set; }
#pragma warning restore CS8618
    public bool ChangeItemSprites { get; set; }
    public NesColor TunicColor { get; set; }
    public NesColor SkinTone { get; set; }
    public NesColor OutlineColor { get; set; }
    public NesColor ShieldColor { get; set; }
    public bool UpAC1 { get; set; }
    public bool RemoveFlashing { get; set; }
    public bool UseCustomRooms { get; set; }
    public bool DisableHUDLag { get; set; }
    public bool RandomizeKnockback { get; set; }

    //For Statistics
    [Key]
    public int Id { get; set; }

    public bool TownWizardsOnlyHaveSpells() => !IncludeSpellsInShuffle && RemoveItems.Count == 0 && StartingSpells.Count == 0;

    public bool StartsWithCollectable(Collectable collectable)
    {
        return collectable switch
        {
            Collectable.SHIELD_SPELL => StartShield,
            Collectable.JUMP_SPELL => StartJump,
            Collectable.LIFE_SPELL => StartLife,
            Collectable.FAIRY_SPELL => StartFairy,
            Collectable.FIRE_SPELL => StartFire,
            Collectable.DASH_SPELL => StartFire,
            Collectable.REFLECT_SPELL => StartReflect,
            Collectable.SPELL_SPELL => StartSpell,
            Collectable.THUNDER_SPELL => StartThunder,
            Collectable.UPSTAB => StartWithUpstab,
            Collectable.DOWNSTAB => StartWithDownstab,
            Collectable.CANDLE => StartCandle,
            Collectable.GLOVE => StartGlove,
            Collectable.RAFT => StartRaft,
            Collectable.BOOTS => StartBoots,
            Collectable.FLUTE => StartFlute,
            Collectable.CROSS => StartCross,
            Collectable.HAMMER => StartHammer,
            Collectable.MAGIC_KEY => StartKey,
            _ => false
        };
    }

    public void SetStartingCollectable(Collectable collectable, bool starts = true)
    {
        if(collectable == Collectable.SHIELD_SPELL)
        {
            StartShield = starts;
        }
        else if (collectable == Collectable.JUMP_SPELL)
        {
            StartJump = starts;
        }
        else if (collectable == Collectable.LIFE_SPELL)
        {
            StartLife = starts;
        }
        else if (collectable == Collectable.FAIRY_SPELL)
        {
            StartFairy = starts;
        }
        else if (collectable == Collectable.FIRE_SPELL)
        {
            StartFire = starts;
        }
        else if (collectable == Collectable.REFLECT_SPELL)
        {
            StartReflect = starts;
        }
        else if (collectable == Collectable.SPELL_SPELL)
        {
            StartSpell = starts;
        }
        else if (collectable == Collectable.THUNDER_SPELL)
        {
            StartThunder = starts;
        }
        else if (collectable == Collectable.CANDLE)
        {
            StartCandle = starts;
        }
        else if (collectable == Collectable.GLOVE)
        {
            StartGlove = starts;
        }
        else if (collectable == Collectable.RAFT)
        {
            StartRaft = starts;
        }
        else if (collectable == Collectable.BOOTS)
        {
            StartBoots = starts;
        }
        else if (collectable == Collectable.FLUTE)
        {
            StartFlute = starts;
        }
        else if (collectable == Collectable.CROSS)
        {
            StartCross = starts;
        }
        else if (collectable == Collectable.HAMMER)
        {
            StartHammer = starts;
        }
        else if (collectable == Collectable.MAGIC_KEY)
        {
            StartKey = starts;
        }
    }

    public bool HasEnoughSpaceToAllocateItems()
    {
        //The 3 pbag caves are either explicitly minor items or allowable as overflow locations
        //so they are counted either way.
        int minorItemCount = 3;

        //more or less than 4 containers in the seed adds/removes minor items
        minorItemCount -= MaxHearts - StartHearts - 4;

        //palace items other than 1 adjusts the count
        minorItemCount += PalaceItemRoomCounts.Select(c => c - 1).Sum();

        //Start items add 1 to the count
        minorItemCount += StartCandle ? 1 : 0;
        minorItemCount += StartBoots ? 1 : 0;
        minorItemCount += StartCross ? 1 : 0;
        minorItemCount += StartFlute ? 1 : 0;
        minorItemCount += StartGlove ? 1 : 0;
        minorItemCount += StartHammer ? 1 : 0;
        minorItemCount += StartKey ? 1 : 0;
        minorItemCount += StartRaft ? 1 : 0;

        if(IncludeSpellsInShuffle)
        {
            minorItemCount += StartShield ? 1 : 0;
            minorItemCount += StartJump ? 1 : 0;
            minorItemCount += StartLife ? 1 : 0;
            minorItemCount += StartFairy ? 1 : 0;
            minorItemCount += StartFire ? 1 : 0;
            minorItemCount += StartReflect ? 1 : 0;
            minorItemCount += StartSpell ? 1 : 0;
            minorItemCount += StartThunder ? 1 : 0;
        }

        if(IncludeSwordTechsInShuffle)
        {
            minorItemCount += StartWithDownstab ? 1 : 0;
            minorItemCount += StartWithUpstab ? 1 : 0;
        }


        return minorItemCount >= 0;
    }
}
