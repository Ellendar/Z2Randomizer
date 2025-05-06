
using System;

namespace Z2Randomizer.RandomizerCore;

public enum Collectable
{
    CANDLE = 0x00,
    GLOVE = 0x01,
    RAFT = 0x02,
    BOOTS = 0x03,
    FLUTE = 0x04,
    CROSS = 0x05,
    HAMMER = 0x06,
    MAGIC_KEY = 0x07,
    KEY = 0x08,
    DO_NOT_USE = 0x09,
    SMALL_BAG = 0x0a,
    MEDIUM_BAG = 0x0b,
    LARGE_BAG = 0x0c,
    XL_BAG = 0x0d,
    MAGIC_CONTAINER = 0x0e,
    HEART_CONTAINER = 0x0f,
    BLUE_JAR = 0x10,
    RED_JAR = 0x11,
    ONEUP = 0x12,
    CHILD = 0x13,
    TROPHY = 0x14,
    MEDICINE = 0x15,
    DO_NOT_USE_ANTIFAIRY = 0x16,
    UPSTAB = 0x17,
    DOWNSTAB = 0x18,
    BAGUS_NOTE = 0x19,
    MIRROR = 0x1a,
    WATER = 0x1b,
    SHIELD_SPELL = 0x1c,
    JUMP_SPELL = 0x1d,
    LIFE_SPELL = 0x1e,
    FAIRY_SPELL = 0x1f,
    FIRE_SPELL = 0x20,
    REFLECT_SPELL = 0x21,
    SPELL_SPELL = 0x22,
    THUNDER_SPELL = 0x23,
    DASH_SPELL = 0x24,
}


public static class CollectableExtensions
{
    public static bool IsMinorItem(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.SMALL_BAG => true,
            Collectable.MEDIUM_BAG => true,
            Collectable.LARGE_BAG => true,
            Collectable.XL_BAG => true,
            Collectable.RED_JAR => true,
            Collectable.BLUE_JAR => true,
            Collectable.KEY => true,
            Collectable.ONEUP => true,
            _ => false
        };
    }


    public static bool IsItemGetItem(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.CANDLE => true,
            Collectable.GLOVE => true,
            Collectable.RAFT => true,
            Collectable.BOOTS => true,
            Collectable.FLUTE => true,
            Collectable.CROSS => true,
            Collectable.HAMMER => true,
            Collectable.MAGIC_KEY => true,
            Collectable.KEY => false,
            Collectable.DO_NOT_USE => false,
            Collectable.SMALL_BAG => false,
            Collectable.MEDIUM_BAG => false,
            Collectable.LARGE_BAG => false,
            Collectable.XL_BAG => false,
            Collectable.MAGIC_CONTAINER => false,
            Collectable.HEART_CONTAINER => false,
            Collectable.BLUE_JAR => false,
            Collectable.RED_JAR => false,
            Collectable.ONEUP => false,
            Collectable.CHILD => true,
            Collectable.TROPHY => true,
            Collectable.MEDICINE => true,
            Collectable.UPSTAB => true,
            Collectable.DOWNSTAB => true,
            Collectable.MIRROR => true,
            Collectable.BAGUS_NOTE => true,
            Collectable.WATER => true,
            Collectable.SHIELD_SPELL => true,
            Collectable.JUMP_SPELL => true,
            Collectable.LIFE_SPELL => true,
            Collectable.FAIRY_SPELL => true,
            Collectable.FIRE_SPELL => true,
            Collectable.DASH_SPELL => true,
            Collectable.REFLECT_SPELL => true,
            Collectable.SPELL_SPELL => true,
            Collectable.THUNDER_SPELL => true,
            Collectable.DO_NOT_USE_ANTIFAIRY => false,
            _ => throw new Exception("Invalid item type determining itemgets")
        };
    }
    public static string EnglishText(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.CANDLE => "CANDLE",
            Collectable.GLOVE => "GLOVE",
            Collectable.RAFT => "RAFT",
            Collectable.BOOTS => "BOOTS",
            Collectable.FLUTE => "FLUTE",
            Collectable.CROSS => "CROSS",
            Collectable.HAMMER => "HAMMER",
            Collectable.MAGIC_KEY => "MAGIC KEY",
            Collectable.KEY => "SMALL KEY",
            Collectable.DO_NOT_USE => "ERROR",
            Collectable.SMALL_BAG => "P BAG",
            Collectable.MEDIUM_BAG => "P BAG",
            Collectable.LARGE_BAG => "P BAG",
            Collectable.XL_BAG => "P BAG",
            Collectable.MAGIC_CONTAINER => "MAGIC$CONTAINER",
            Collectable.HEART_CONTAINER => "HEART$CONTAINER",
            Collectable.BLUE_JAR => "BLUE JAR",
            Collectable.RED_JAR => "RED JAR",
            Collectable.ONEUP => "LINK DOLL",
            Collectable.CHILD => "CHILD",
            Collectable.TROPHY => "TROPHY",
            Collectable.MEDICINE => "MEDICINE",
            Collectable.UPSTAB => "UP STAB",
            Collectable.DOWNSTAB => "DOWN STAB",
            Collectable.MIRROR => "MIRROR",
            Collectable.BAGUS_NOTE => "BAGUS$LETTER",
            Collectable.WATER => "WATER",
            Collectable.SHIELD_SPELL => "SHIELD$SPELL",
            Collectable.JUMP_SPELL => "JUMP$SPELL",
            Collectable.LIFE_SPELL => "LIFE$SPELL",
            Collectable.FAIRY_SPELL => "FAIRY$SPELL",
            Collectable.FIRE_SPELL => "FIRE$SPELL",
            Collectable.DASH_SPELL => "DASH$SPELL",
            Collectable.REFLECT_SPELL => "REFLECT$SPELL",
            Collectable.SPELL_SPELL => "SPELL$SPELL",
            Collectable.THUNDER_SPELL => "THUNDER$SPELL",
            _ => "MISSING"
        };
    }
    public static string SingleLineText(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.CANDLE => "CANDLE",
            Collectable.GLOVE => "GLOVE",
            Collectable.RAFT => "RAFT",
            Collectable.BOOTS => "BOOTS",
            Collectable.FLUTE => "FLUTE",
            Collectable.CROSS => "CROSS",
            Collectable.HAMMER => "HAMMER",
            Collectable.MAGIC_KEY => "MAGIC KEY",
            Collectable.KEY => "SMALL KEY",
            Collectable.DO_NOT_USE => "ERROR",
            Collectable.SMALL_BAG => "P BAG",
            Collectable.MEDIUM_BAG => "P BAG",
            Collectable.LARGE_BAG => "P BAG",
            Collectable.XL_BAG => "P BAG",
            Collectable.MAGIC_CONTAINER => "MAGIC UP",
            Collectable.HEART_CONTAINER => "HEART",
            Collectable.BLUE_JAR => "BLUE JAR",
            Collectable.RED_JAR => "RED JAR",
            Collectable.ONEUP => "LINK DOLL",
            Collectable.CHILD => "CHILD",
            Collectable.TROPHY => "TROPHY",
            Collectable.MEDICINE => "MEDICINE",
            Collectable.UPSTAB => "UPSTAB",
            Collectable.DOWNSTAB => "DOWNSTAB",
            Collectable.MIRROR => "MIRROR",
            Collectable.BAGUS_NOTE => "LETTER",
            Collectable.WATER => "WATER",
            Collectable.SHIELD_SPELL => "SHIELD",
            Collectable.JUMP_SPELL => "JUMP",
            Collectable.LIFE_SPELL => "LIFE",
            Collectable.FAIRY_SPELL => "FAIRY",
            Collectable.FIRE_SPELL => "FIRE",
            Collectable.DASH_SPELL => "DASH",
            Collectable.REFLECT_SPELL => "REFLECT",
            Collectable.SPELL_SPELL => "SPELL",
            Collectable.THUNDER_SPELL => "THUNDER",
            _ => "MISSING"
        };
    }

    public static bool IsSpell(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.SHIELD_SPELL => true,
            Collectable.JUMP_SPELL => true,
            Collectable.LIFE_SPELL => true,
            Collectable.FAIRY_SPELL => true,
            Collectable.FIRE_SPELL => true,
            Collectable.DASH_SPELL => true,
            Collectable.REFLECT_SPELL => true,
            Collectable.SPELL_SPELL => true,
            Collectable.THUNDER_SPELL => true,
            _ => false
        };
    }

    public static bool IsQuestItem(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.WATER => true,
            Collectable.BAGUS_NOTE => true,
            Collectable.MIRROR => true,
            _ => false
        };
    }

    public static bool IsSwordTech(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.DOWNSTAB => true,
            Collectable.UPSTAB => true,
            _ => false
        };
    }

    public static RequirementType? AsRequirement(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.JUMP_SPELL => RequirementType.JUMP,
            Collectable.FAIRY_SPELL => RequirementType.FAIRY,
            Collectable.UPSTAB => RequirementType.UPSTAB,
            Collectable.DOWNSTAB => RequirementType.DOWNSTAB,
            Collectable.MAGIC_KEY => RequirementType.KEY,
            Collectable.DASH_SPELL => RequirementType.DASH,
            Collectable.GLOVE => RequirementType.GLOVE,
            Collectable.REFLECT_SPELL => RequirementType.REFLECT,
            Collectable.SPELL_SPELL => RequirementType.SPELL,
            Collectable.TROPHY => RequirementType.TROPHY,
            Collectable.MEDICINE => RequirementType.MEDICINE,
            Collectable.CHILD => RequirementType.CHILD,
            Collectable.MIRROR => RequirementType.MIRROR,
            Collectable.WATER => RequirementType.WATER,
            _ => null
        };
    }

    public static int VanillaSpellOrder(this Collectable spell)
    {
        return spell switch
        {
            Collectable.SHIELD_SPELL => 0,
            Collectable.JUMP_SPELL => 1,
            Collectable.LIFE_SPELL => 2,
            Collectable.FAIRY_SPELL => 3,
            Collectable.FIRE_SPELL => 4,
            Collectable.DASH_SPELL => 4,
            Collectable.REFLECT_SPELL => 5,
            Collectable.SPELL_SPELL => 6,
            Collectable.THUNDER_SPELL => 7,
            _ => throw new ImpossibleException("Invalid vanilla spell index")
        };
    }

    public static bool IsInternalUse(this Collectable collectable)
    {
        return collectable switch
        {
            Collectable.DO_NOT_USE => true,
            Collectable.DO_NOT_USE_ANTIFAIRY => true,
            _ => false
        };
    }
}
