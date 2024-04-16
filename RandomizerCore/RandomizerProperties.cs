using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

/// <summary>
/// Originally this class corresponded to the flags and controlled the logic, with all the actual configuration randomization
/// logic scattershot all over the rando. Now this class should (as much as possible) represent what the actual net configuration
/// was, and RandomizerConfiguration should represent the flags/interface, with the configuration randomization done entirly
/// in the interface between them.
/// </summary>
public class RandomizerProperties
{
    public RandomizerProperties()
    {
    }

    //ROM Info
    public string Filename { get; set; }
    public int Seed { get; set; }
    public string Flags { get; set; }
    public bool saveRom = true;

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
    public bool SwapPalaceCont { get; set; }
    public bool P7shuffle { get; set; }
    public bool HiddenPalace { get; set; }
    public bool HiddenKasuto { get; set; }
    public bool TownSwap { get; set; }
    public EncounterRate EncounterRate { get; set; }
    public ContinentConnectionType ContinentConnections { get; set; }
    public bool BoulderBlockConnections { get; set; }
    public Biome WestBiome { get; set; }
    public Biome EastBiome { get; set; }
    public Biome MazeBiome { get; set; }
    public Biome DmBiome { get; set; }
    public bool DmIsHorizontal { get; set; }
    public bool WestIsHorizontal { get; set; }
    public bool EastIsHorizontal { get; set; }

    public Climate Climate { get; set; }
    public bool VanillaShuffleUsesActualTerrain { get; set; }
    public bool ShuffleHidden { get; set; }
    public bool CanWalkOnWaterWithBoots { get; set; }
    public bool BagusWoods { get; set; }

    //Palaces
    //public bool shufflePalaceRooms { get; set; }
    public PalaceStyle NormalPalaceStyle { get; set; }
    public PalaceStyle GPStyle { get; set; }
    public int StartGems { get; set; }
    public bool RequireTbird { get; set; }
    public bool ShufflePalacePalettes { get; set; }
    public bool UpARestartsAtPalaces { get; set; }
    public bool RemoveTbird { get; set; }
    public bool BossItem { get; set; }
    //public bool createPalaces { get; set; }
    public bool BlockersAnywhere { get; set; }
    public bool BossRoomConnect { get; set; }
    public bool NoDuplicateRooms { get; set; }
    public bool NoDuplicateRoomsBySideview { get; set; }
    public bool GeneratorsAlwaysMatch { get; set; }
    public bool AllowVanillaRooms { get; set; }
    public bool AllowV4Rooms { get; set; }
    public bool AllowV4_4Rooms { get; set; }
    public bool HardBosses { get; set; }

    //Enemies
    public bool ShuffleEnemyHP { get; set; }
    public bool ShuffleEnemyStealExp { get; set; }
    public bool ShuffleStealExpAmt { get; set; }
    public bool ShuffleSwordImmunity { get; set; }
    public bool ShuffleOverworldEnemies { get; set; }
    public bool ShufflePalaceEnemies { get; set; }
    public bool MixLargeAndSmallEnemies { get; set; }
    public bool ShuffleDripper { get; set; }
    public bool ShuffleEnemyPalettes { get; set; }
    public StatEffectiveness ExpLevel { get; set; }

    //Levels
    //public bool shuffleAllExp { get; set; }
    public bool ShuffleAtkExp { get; set; }
    public bool ShuffleMagicExp { get; set; }
    public bool ShuffleLifeExp { get; set; }
    //public bool shuffleAtkEff { get; set; }
    //public bool shuffleMagEff { get; set; }
    //public bool shuffleLifeEff { get; set; }
    public bool ShuffleLifeRefill { get; set; }
    public bool ShuffleSpellLocations { get; set; }
    public bool DisableMagicRecs { get; set; }
    /*
    public bool ohkoEnemies { get; set; }
    public bool tankMode { get; set; }
    public bool ohkoLink { get; set; }
    public bool wizardMode { get; set; }
    public bool highAtk { get; set; }
    public bool lowAtk { get; set; }
    public bool highDef { get; set; }
    public bool highMag { get; set; }
    public bool lowMag { get; set; }
    */
    public StatEffectiveness AttackEffectiveness { get; set; }
    public StatEffectiveness MagicEffectiveness { get; set; }
    public StatEffectiveness LifeEffectiveness { get; set; }
    public int AttackCap { get; set; }
    public int MagicCap { get; set; }
    public int LifeCap { get; set; }
    public bool ScaleLevels { get; set; }
    public bool HideLessImportantLocations { get; set; }
    public bool SaneCaves { get; set; }
    public bool SpellEnemy { get; set; }

    //Items
    public bool ShuffleOverworldItems { get; set; }
    public bool ShufflePalaceItems { get; set; }
    public bool MixOverworldPalaceItems { get; set; }
    public bool ShuffleSmallItems { get; set; }
    public bool ExtraKeys { get; set; }
    public bool KasutoJars { get; set; }
    //Include PBag caves in item shuffle
    public bool PbagItemShuffle { get; set; }
    public bool RemoveSpellItems { get; set; }
    public bool ShufflePbagXp { get; set; }

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
    public String BeamSprite { get; set; }
    public bool DisableMusic { get; set; }
    [NotMapped]
    public CharacterSprite CharSprite { get; set; }
    public String TunicColor { get; set; }
    public String ShieldColor { get; set; }
    public bool UpAC1 { get; set; }
    public bool RemoveFlashing { get; set; }
    public bool UseCustomRooms { get; set; }
    public bool DisableHUDLag { get; set; }
    public bool RandomizeKnockback { get; set; }

    //For Statistics
    [Key]
    public int Id { get; set; }

    public bool StartWithSpell(Spell spell)
    {
        return spell switch
        {
            Spell.SHIELD => StartShield,
            Spell.JUMP => StartJump,
            Spell.LIFE => StartLife,
            Spell.FAIRY => StartFairy,
            Spell.FIRE => StartFire,
            Spell.DASH => StartFire,
            Spell.REFLECT => StartReflect,
            Spell.SPELL => StartSpell,
            Spell.THUNDER => StartThunder,
            Spell.UPSTAB => StartWithUpstab,
            Spell.DOWNSTAB => StartWithDownstab,
            _ => throw new ImpossibleException("Unrecognized spell")
        };
    }
}
