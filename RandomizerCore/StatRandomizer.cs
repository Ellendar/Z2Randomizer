using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Responsible for randomizing stats of all kinds. Each array should
/// be randomized at most once. If you have to re-roll for some reason,
/// a new StatRandomizer object should be created.
/// </summary>
public class StatRandomizer
{
    public const int LIFE_EFFECTIVENESS_ROWS = 7;
    public const int MAGIC_EFFECTIVENESS_ROWS = 8;
    public byte[] AttackEffectivenessTable { get; private set; } = null!;
    public byte[] LifeEffectivenessTable { get; private set; } = null!;
    public byte[] MagicEffectivenessTable { get; private set; } = null!;

    public int[] ExperienceToLevelTable { get; private set; } = null!;

    public byte[] WestEnemyHpTable { get; private set; } = null!;
    public byte[] EastEnemyHpTable { get; private set; } = null!;
    public byte[] Palace125EnemyHpTable { get; private set; } = null!;
    public byte[] Palace346EnemyHpTable { get; private set; } = null!;
    public byte[] GpEnemyHpTable { get; private set; } = null!;
    public byte[] BossHpTable { get; private set; } = null!;
    public byte[] WestEnemyStatsTable { get; private set; } = null!;
    public byte[] EastEnemyStatsTable { get; private set; } = null!;
    public byte[] Palace125EnemyStatsTable { get; private set; } = null!;
    public byte[] Palace346EnemyStatsTable { get; private set; } = null!;
    public byte[] GpEnemyStatsTable { get; private set; } = null!;
    public byte[] BossExpTable { get; private set; } = null!;

    protected RandomizerProperties props { get; }

#if DEBUG
    private bool hasRandomized = false;
    private bool hasWritten = false;
#endif

    public StatRandomizer(ROM rom, RandomizerProperties props)
    {
        this.props = props;
        ReadExperienceToLevel(rom);

        ReadAttackEffectiveness(rom);
        ReadLifeEffectiveness(rom);
        ReadMagicEffectiveness(rom);

        ReadEnemyHp(rom);
        ReadEnemyStats(rom);
    }

    public void Randomize(Random r)
    {
#if DEBUG
        Debug.Assert(!hasRandomized);
        hasRandomized = true;
#endif
        RandomizeExperienceToLevel(r, [props.ShuffleAtkExp, props.ShuffleMagicExp, props.ShuffleLifeExp],
                                      [props.AttackCap, props.MagicCap, props.LifeCap], props.ScaleLevels);

        RandomizeAttackEffectiveness(r, props.AttackEffectiveness);
        RandomizeLifeEffectiveness(r, props.LifeEffectiveness);
        RandomizeMagicEffectiveness(r, props.MagicEffectiveness);

        RandomizeRegularEnemyHp(r);
        RandomizeBossHp(r);
        FixRebonackHorseKillBug();
        RandomizeEnemyStats(r);
    }

    public void Write(ROM rom)
    {
#if DEBUG
        Debug.Assert(!hasWritten);
        hasWritten = true;
#endif

        WriteExperienceToLevel(rom);

        WriteAttackEffectiveness(rom);
        WriteLifeEffectiveness(rom);
        WriteMagicEffectiveness(rom);

        WriteEnemyHp(rom);
        WriteEnemyStats(rom);
    }

    [Conditional("DEBUG")]
    public void AssertHasRandomized(bool value = true)
    {
#if DEBUG
        Debug.Assert(hasRandomized == value);
#endif
    }

    [Conditional("DEBUG")]
    public void AssertHasWritten(bool value = true) {
#if DEBUG
        Debug.Assert(hasWritten == value);
#endif
    }

    protected void ReadExperienceToLevel(ROM rom)
    {
        const int startAddr = RomMap.EXPERIENCE_TO_LEVEL_TABLE;
        ExperienceToLevelTable = new int[24];
        for (int i = 0; i < 24; i++)
        {
            ExperienceToLevelTable[i] = rom.GetShort(startAddr + i, startAddr + 24 + i);
        }
    }

    protected void WriteExperienceToLevel(ROM rom)
    {
        const int startAddr = RomMap.EXPERIENCE_TO_LEVEL_TABLE;
        for (int i = 0; i < ExperienceToLevelTable.Length; i++)
        {
            rom.PutShort(startAddr + i, startAddr + 24 + i, ExperienceToLevelTable[i]);
        }

        for (int i = 0; i < ExperienceToLevelTable.Length; i++)
        {
            int n = ExperienceToLevelTable[i];
            var digit1 = ROM.DigitToZ2TextByte(n / 1000);
            n %= 1000;
            var digit2 = ROM.DigitToZ2TextByte(n / 100);
            n %= 100;
            var digit3 = ROM.DigitToZ2TextByte(n / 10);
            rom.Put(RomMap.EXPERIENCE_TO_LEVEL_TEXT_TABLE + 48 + i, digit1);
            rom.Put(RomMap.EXPERIENCE_TO_LEVEL_TEXT_TABLE + 24 + i, digit2);
            rom.Put(RomMap.EXPERIENCE_TO_LEVEL_TEXT_TABLE + 0 + i, digit3);
        }
    }

    protected void ReadAttackEffectiveness(ROM rom)
    {
        AttackEffectivenessTable = rom.GetBytes(RomMap.ATTACK_EFFECTIVENESS_TABLE, 8);
    }

    protected void WriteAttackEffectiveness(ROM rom)
    {
        rom.Put(RomMap.ATTACK_EFFECTIVENESS_TABLE, AttackEffectivenessTable);
    }

    protected void ReadLifeEffectiveness(ROM rom)
    {
        // There are 7 different damage codes for which damage taken scales with Life-level
        // Each of those 7 codes have 8 values corresponding to each Life-level.
        // (This is followed by an 8th row that is OHKO regardless of Life-level.)
        LifeEffectivenessTable = rom.GetBytes(RomMap.LIFE_EFFECTIVENESS_TABLE, LIFE_EFFECTIVENESS_ROWS * 8);
    }

    protected void WriteLifeEffectiveness(ROM rom)
    {
        rom.Put(RomMap.LIFE_EFFECTIVENESS_TABLE, LifeEffectivenessTable);
    }

    protected void ReadMagicEffectiveness(ROM rom)
    {
        // 8 Spell costs by 8 Magic levels
        MagicEffectivenessTable = rom.GetBytes(RomMap.MAGIC_EFFECTIVENESS_TABLE, MAGIC_EFFECTIVENESS_ROWS * 8);
    }

    protected void WriteMagicEffectiveness(ROM rom)
    {
        rom.Put(RomMap.MAGIC_EFFECTIVENESS_TABLE, MagicEffectivenessTable);
    }

    protected void ReadEnemyHp(ROM rom)
    {
        WestEnemyHpTable = rom.GetBytes(RomMap.WEST_ENEMY_HP_TABLE, 0x24);
        EastEnemyHpTable = rom.GetBytes(RomMap.EAST_ENEMY_HP_TABLE, 0x24);
        Palace125EnemyHpTable = rom.GetBytes(RomMap.PALACE125_ENEMY_HP_TABLE, 0x24);
        Palace346EnemyHpTable = rom.GetBytes(RomMap.PALACE346_ENEMY_HP_TABLE, 0x24);
        GpEnemyHpTable = rom.GetBytes(RomMap.GP_ENEMY_HP_TABLE, 0x24);
        BossHpTable = [.. RomMap.bossHpAddresses.Select(rom.GetByte)];
    }

    protected void WriteEnemyHp(ROM rom)
    {
        rom.Put(RomMap.WEST_ENEMY_HP_TABLE, WestEnemyHpTable);
        rom.Put(RomMap.EAST_ENEMY_HP_TABLE, EastEnemyHpTable);
        rom.Put(RomMap.PALACE125_ENEMY_HP_TABLE, Palace125EnemyHpTable);
        rom.Put(RomMap.PALACE346_ENEMY_HP_TABLE, Palace346EnemyHpTable);
        rom.Put(RomMap.GP_ENEMY_HP_TABLE, GpEnemyHpTable);
        for (int i = 0; i < RomMap.bossHpAddresses.Count; i++) {
            rom.Put(RomMap.bossHpAddresses[i], BossHpTable[i]);
        }
    }

    protected void ReadEnemyStats(ROM rom)
    {
        WestEnemyStatsTable = rom.GetBytes(RomMap.WEST_ENEMY_STATS_TABLE, 0x48);
        EastEnemyStatsTable = rom.GetBytes(RomMap.EAST_ENEMY_STATS_TABLE, 0x48);
        Palace125EnemyStatsTable = rom.GetBytes(RomMap.PALACE125_ENEMY_STATS_TABLE, 0x48);
        Palace346EnemyStatsTable = rom.GetBytes(RomMap.PALACE346_ENEMY_STATS_TABLE, 0x48);
        GpEnemyStatsTable = rom.GetBytes(RomMap.GP_ENEMY_STATS_TABLE, 0x48);
        BossExpTable = [.. RomMap.bossExpAddresses.Select(rom.GetByte)]; // we only have exp byte here
    }

    protected void WriteEnemyStats(ROM rom)
    {
        rom.Put(RomMap.WEST_ENEMY_STATS_TABLE, WestEnemyStatsTable);
        rom.Put(RomMap.EAST_ENEMY_STATS_TABLE, EastEnemyStatsTable);
        rom.Put(RomMap.PALACE125_ENEMY_STATS_TABLE, Palace125EnemyStatsTable);
        rom.Put(RomMap.PALACE346_ENEMY_STATS_TABLE, Palace346EnemyStatsTable);
        rom.Put(RomMap.GP_ENEMY_STATS_TABLE, GpEnemyStatsTable);
        for (int i = 0; i < RomMap.bossExpAddresses.Count; i++)
        {
            rom.Put(RomMap.bossExpAddresses[i], BossExpTable[i]);
        }
    }

    protected void RandomizeExperienceToLevel(Random r, bool[] shuffleStat, int[] levelCap, bool scaleLevels)
    {
        int[] newTable = new int[24];
        Span<int> randomizedSpan = newTable;
        ReadOnlySpan<int> vanillaSpan = ExperienceToLevelTable;

        for (int stat = 0; stat < 3; stat++)
        {
            var statStartIndex = stat * 8;
            if (!shuffleStat[stat])
            {
                vanillaSpan.Slice(statStartIndex, 8).CopyTo(randomizedSpan.Slice(statStartIndex, 8));
                continue;
            }
            for (int i = 0; i < 8; i++)
            {
                var vanilla = ExperienceToLevelTable[statStartIndex + i];
                int nextMin = (int)(vanilla - vanilla * 0.25); // hardcoded -25%
                int nextMax = (int)(vanilla + vanilla * 0.25); // hardcoded +25%
                if (i == 0)
                {
                    newTable[statStartIndex + i] = r.Next(Math.Max(10, nextMin), nextMax);
                }
                else
                {
                    newTable[statStartIndex + i] = r.Next(Math.Max(newTable[statStartIndex + i - 1], nextMin), Math.Min(nextMax, 9990));
                }
            }
        }

        for (int i = 0; i < newTable.Length; i++)
        {
            newTable[i] = newTable[i] / 10 * 10; //wtf is this line of code? -digshake, 2020
        }

        if (scaleLevels)
        {
            int[] cappedExp = new int[24];

            for (int stat = 0; stat < 3; stat++)
            {
                var statStartIndex = stat * 8;
                var cap = levelCap[stat];

                for (int i = 0; i < 8; i++)
                {
                    if (i >= cap)
                    {
                        cappedExp[statStartIndex + i] = newTable[statStartIndex + i]; //shouldn't matter, just wanna put something here
                    }
                    else if (i == cap - 1)
                    {
                        cappedExp[statStartIndex + i] = newTable[7]; //exp to get a 1up
                    }
                    else
                    {
                        cappedExp[statStartIndex + i] = newTable[(int)(6 * ((i + 1.0) / (cap - 1)))]; //cap = 3, level 4, 8, 
                    }
                }
            }

            newTable = cappedExp;
        }

        // Make sure all 1-up levels are the same value,
        // one that is higher than any regular level
        int highestRegularLevelExp = 0;
        for (int stat = 0; stat < 3; stat++)
        {
            var cap = levelCap[stat];
            if (cap < 2) { continue; }
            var statStartIndex = stat * 8;
            int statMaxLevelIndex = statStartIndex + cap - 2;
            var maxExp = newTable[statMaxLevelIndex];
            if (highestRegularLevelExp < maxExp) { highestRegularLevelExp = maxExp; }
        }

        int exp1upMin = Math.Min(highestRegularLevelExp + 10, 9990);
        int exp1up = r.Next(exp1upMin, 9999) / 10 * 10;
        for (int stat = 0; stat < 3; stat++)
        {
            var cap = levelCap[stat];
            Debug.Assert(cap >= 1 && cap < 9);
            var statStartIndex = stat * 8;
            for (int i = cap - 1; i < 8; i++)
            {
                newTable[statStartIndex + i] = exp1up;
            }
        }

        ExperienceToLevelTable = newTable;
    }

    protected void RandomizeAttackEffectiveness(Random r, AttackEffectiveness attackEffectiveness)
    {
        if (attackEffectiveness == AttackEffectiveness.VANILLA)
        {
            return;
        }
        if (attackEffectiveness == AttackEffectiveness.OHKO)
        {
            // handled in RandomizeEnemyStats()
            return;
        }

        byte[] newTable = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            int nextVal;
            byte vanilla = AttackEffectivenessTable[i];
            switch (attackEffectiveness)
            {
                case AttackEffectiveness.LOW:
                    //the naieve approach here gives a curve of 1,2,2,4,5,6 which is weird, or a different
                    //irregular curve in digshake's old approach. Just use a linear increase for the first 6 levels on low
                    if (i < 6)
                    {
                        nextVal = i + 1;
                    }
                    else
                    {
                        nextVal = (int)Math.Round(vanilla * .5, MidpointRounding.ToPositiveInfinity);
                    }
                    break;
                case AttackEffectiveness.AVERAGE_LOW:
                    nextVal = RandomInRange(r, vanilla * .5, vanilla);
                    if (i == 1)
                    {
                        nextVal = Math.Max(nextVal, 2); // set minimum 2 damage at level 2
                    }
                    break;
                case AttackEffectiveness.AVERAGE:
                    nextVal = RandomInRange(r, vanilla * .667, vanilla * 1.5);
                    if (i == 0)
                    {
                        nextVal = Math.Max(nextVal, 2); // set minimum 2 damage at start
                    }
                    break;
                case AttackEffectiveness.AVERAGE_HIGH:
                    nextVal = RandomInRange(r, vanilla, vanilla * 1.5);
                    break;
                case AttackEffectiveness.HIGH:
                    nextVal = (int)Math.Round(vanilla * 1.5);
                    break;
                default:
                    throw new NotImplementedException("Invalid Attack Effectiveness");
            }
            if (i > 0)
            {
                byte lastValue = newTable[i - 1];
                if (nextVal < lastValue)
                {
                    nextVal = lastValue; // levelling up should never be worse
                }
            }

            newTable[i] = (byte)nextVal;
        }
        AttackEffectivenessTable = newTable;
    }

    protected void RandomizeLifeEffectiveness(Random r, LifeEffectiveness statEffectiveness)
    {
        if (statEffectiveness == LifeEffectiveness.VANILLA)
        {
            return;
        }
        if (statEffectiveness == LifeEffectiveness.OHKO)
        {
            Array.Fill<byte>(LifeEffectivenessTable, 0xFF);
            return;
        }
        if (statEffectiveness == LifeEffectiveness.INVINCIBLE)
        {
            Array.Fill<byte>(LifeEffectivenessTable, 0x00);
            return;
        }

        byte[] newTable = new byte[LIFE_EFFECTIVENESS_ROWS * 8];

        // The values we are randomizing are actually *enemy damage* values
        for (int level = 0; level < 8; level++)
        {
            for (int damageCode = 0; damageCode < LIFE_EFFECTIVENESS_ROWS; damageCode++)
            {
                int index = damageCode * 8 + level;
                byte nextVal;
                byte vanilla = (byte)(LifeEffectivenessTable[index] >> 1);
                int min = (int)(vanilla * .75);
                int max = Math.Min((int)(vanilla * 1.5), 120);
                switch (statEffectiveness)
                {
                    case LifeEffectiveness.AVERAGE_LOW:
                        nextVal = (byte)r.Next(vanilla, max);
                        break;
                    case LifeEffectiveness.AVERAGE:
                        nextVal = (byte)r.Next(min, max);
                        break;
                    case LifeEffectiveness.AVERAGE_HIGH:
                        nextVal = (byte)r.Next(min, vanilla);
                        break;
                    case LifeEffectiveness.HIGH:
                        nextVal = (byte)(vanilla * .5);
                        break;
                    default:
                        throw new NotImplementedException("Invalid Life Effectiveness");
                }
                if (level > 0)
                {
                    byte lastVal = (byte)(newTable[index - 1] >> 1);
                    if (nextVal > lastVal)
                    {
                        nextVal = lastVal; // levelling up should never be worse
                    }
                }
                newTable[index] = (byte)(nextVal << 1);
            }
        }
        LifeEffectivenessTable = newTable;
    }

    protected void RandomizeMagicEffectiveness(Random r, MagicEffectiveness statEffectiveness)
    {
        if (statEffectiveness == MagicEffectiveness.VANILLA)
        {
            return;
        }

        if (statEffectiveness == MagicEffectiveness.FREE)
        {
            Array.Fill<byte>(MagicEffectivenessTable, 0);
            return;
        }

        byte[] newTable = new byte[MAGIC_EFFECTIVENESS_ROWS * 8];

        for (int level = 0; level < 8; level++)
        {
            for (int spellIndex = 0; spellIndex < MAGIC_EFFECTIVENESS_ROWS; spellIndex++)
            {
                int index = spellIndex * 8 + level;
                byte nextVal;
                byte vanilla = (byte)(MagicEffectivenessTable[index] >> 1);
                int min = (int)(vanilla * .5);
                int max = Math.Min((int)(vanilla * 1.5), 120);
                switch (statEffectiveness)
                {
                    case MagicEffectiveness.HIGH_COST:
                        nextVal = (byte)max;
                        break;
                    case MagicEffectiveness.AVERAGE_HIGH_COST:
                        nextVal = (byte)r.Next(vanilla, max);
                        break;
                    case MagicEffectiveness.AVERAGE:
                        nextVal = (byte)r.Next(min, max);
                        break;
                    case MagicEffectiveness.AVERAGE_LOW_COST:
                        nextVal = (byte)r.Next(min, vanilla);
                        break;
                    case MagicEffectiveness.LOW_COST:
                        nextVal = (byte)min;
                        break;
                    default:
                        throw new Exception("Invalid Magic Effectiveness");
                }
                if (level > 0)
                {
                    byte lastVal = (byte)(newTable[index - 1] >> 1);
                    if (nextVal > lastVal)
                    {
                        nextVal = lastVal; // levelling up should never be worse
                    }
                }
                newTable[index] = (byte)(nextVal << 1);
            }
        }
        MagicEffectivenessTable = newTable;
    }

    protected void RandomizeRegularEnemyHp(Random r)
    {
        if (!props.ShuffleEnemyHP.IsRandom()) {
            return;
        }

        double low = props.ShuffleEnemyHP.GetRandomRangeDouble()!.Low;
        double high = props.ShuffleEnemyHP.GetRandomRangeDouble()!.High;
        int i;

        for (i = (int)EnemiesWest.MYU; i < 0x23; i++) { RandomizeInsideArray(r, WestEnemyHpTable, i, low, high); }

        for (i = (int)EnemiesEast.MYU; i < 0x1e; i++) { RandomizeInsideArray(r, EastEnemyHpTable, i, low, high); }

        // keeping old behavior where some non-enemies are not randomized (strike for jar, unused enemies etc.)
        for (i = (int)EnemiesPalace125.MYU; i < (int)EnemiesPalace125.STRIKE_FOR_RED_JAR; i++) { RandomizeInsideArray(r, Palace125EnemyHpTable, i, low, high); }
        for (i = (int)EnemiesPalace125.SLOW_BUBBLE; i < (int)EnemiesPalace125.HORSEHEAD; i++) { RandomizeInsideArray(r, Palace125EnemyHpTable, i, low, high); }
        for (i = (int)EnemiesPalace125.BLUE_STALFOS; i < 0x24; i++) { RandomizeInsideArray(r, Palace125EnemyHpTable, i, low, high); }

        for (i = (int)EnemiesPalace346.MYU; i < (int)EnemiesPalace346.STRIKE_FOR_RED_JAR_OR_IRON_KNUCKLE; i++) { RandomizeInsideArray(r, Palace346EnemyHpTable, i, low, high); }
        for (i = (int)EnemiesPalace346.SLOW_BUBBLE; i < (int)EnemiesPalace346.REBONAK; i++) { RandomizeInsideArray(r, Palace346EnemyHpTable, i, low, high); }
        for (i = (int)EnemiesPalace346.BLUE_STALFOS; i < 0x24; i++) { RandomizeInsideArray(r, Palace346EnemyHpTable, i, low, high); }

        for (i = (int)EnemiesGreatPalace.MYU; i < (int)EnemiesGreatPalace.STRIKE_FOR_RED_JAR_OR_FOKKA; i++) { RandomizeInsideArray(r, GpEnemyHpTable, i, low, high); }
        for (i = (int)EnemiesGreatPalace.ORANGE_MOA; i < (int)EnemiesGreatPalace.BUBBLE_GENERATOR; i++) { RandomizeInsideArray(r, GpEnemyHpTable, i, low, high); }
        for (i = (int)EnemiesGreatPalace.RED_DEELER; i < (int)EnemiesGreatPalace.ELEVATOR; i++) { RandomizeInsideArray(r, GpEnemyHpTable, i, low, high); }
        for (i = (int)EnemiesGreatPalace.SLOW_BUBBLE; i < 0x1b; i++) { RandomizeInsideArray(r, GpEnemyHpTable, i, low, high); }
        for (i = (int)EnemiesGreatPalace.FOKKERU; i < (int)EnemiesGreatPalace.KING_BOT; i++) { RandomizeInsideArray(r, GpEnemyHpTable, i, low, high); }
    }

    protected void RandomizeBossHp(Random r)
    {
        if (props.ShuffleBossHP == EnemyLifeOption.VANILLA) { return; }

        var rr = props.ShuffleBossHP.GetRandomRangeDouble()!;
        for (int i = 0; i < BossHpTable.Length; i++) { RandomizeInsideArray(r, BossHpTable, i, rr.Low, rr.High); }
    }

    protected void RandomizeEnemyStats(Random r)
    {
        RandomizeEnemyAttributes(r, WestEnemyStatsTable, Enemies.WestGroundEnemies, Enemies.WestFlyingEnemies, Enemies.WestGenerators);
        RandomizeEnemyAttributes(r, EastEnemyStatsTable, Enemies.EastGroundEnemies, Enemies.EastFlyingEnemies, Enemies.EastGenerators);
        RandomizeEnemyAttributes(r, Palace125EnemyStatsTable, Enemies.Palace125GroundEnemies, Enemies.Palace125FlyingEnemies, Enemies.Palace125Generators);
        RandomizeEnemyAttributes(r, Palace346EnemyStatsTable, Enemies.Palace346GroundEnemies, Enemies.Palace346FlyingEnemies, Enemies.Palace346Generators);
        RandomizeEnemyAttributes(r, GpEnemyStatsTable, Enemies.GPGroundEnemies, Enemies.GPFlyingEnemies, Enemies.GPGenerators);
        if (props.EnemyXPDrops != XPEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(r, BossExpTable, props.EnemyXPDrops);
        }
    }

    protected void RandomizeEnemyAttributes<T>(Random r, byte[] bytes, T[] groundEnemies, T[] flyingEnemies, T[] generators) where T : Enum
    {
        List<T> allEnemies = [.. groundEnemies, .. flyingEnemies, .. generators];
        var vanillaEnemyBytes1 = allEnemies.Select(n => bytes[(int)(object)n]).ToArray();
        var vanillaEnemyBytes2 = allEnemies.Select(n => bytes[(int)(object)n + 0x24]).ToArray();

        byte[] enemyBytes1 = vanillaEnemyBytes1.ToArray();
        byte[] enemyBytes2 = vanillaEnemyBytes2.ToArray();

        // enemy attributes byte1
        // ..x. .... sword immune
        // ...x .... steals exp
        // .... xxxx exp
        const int SWORD_IMMUNE_BIT = 0b00100000;
        const int XP_STEAL_BIT = 0b00010000;

        if (props.ShuffleSwordImmunity)
        {
            RandomizeBits(r, enemyBytes1, SWORD_IMMUNE_BIT);
        }
        if (props.ShuffleEnemyStealExp)
        {
            RandomizeBits(r, enemyBytes1, XP_STEAL_BIT);
        }
        if (props.EnemyXPDrops != XPEffectiveness.VANILLA)
        {
            RandomizeEnemyExp(r, enemyBytes1, props.EnemyXPDrops);
        }

        // enemy attributes byte2
        // ..x. .... immune to projectiles
        const int PROJECTILE_IMMUNE_BIT = 0b00100000;

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if ((enemyBytes1[i] & SWORD_IMMUNE_BIT) != 0)
            {
                // if an enemy is becoming sword immune, make it not fire immune
                if ((vanillaEnemyBytes1[i] & SWORD_IMMUNE_BIT) == 0)
                {
                    enemyBytes2[i] &= PROJECTILE_IMMUNE_BIT ^ 0xFF;
                }
            }
        }

        // For future reference:
        // byte4 could be used to randomize thunder immunity
        // (then we must probably exclude generators so thunder doesn't destroy them)
        // x... .... immune to thunder


        for (int i = 0; i < allEnemies.Count; i++)
        {
            int index = (int)(object)allEnemies[i];
            bytes[index] = enemyBytes1[i];
            bytes[index + 0x24] = enemyBytes2[i];
        }
    }

    protected static void RandomizeEnemyExp(Random r, byte[] bytes, XPEffectiveness effectiveness)
    {
        var rr = effectiveness.GetRandomRangeInt()!;

        for (int i = 0; i < bytes.Length; i++)
        {
            int byt = bytes[i];
            int nibble = byt & 0x0f;

            nibble = r.Next(nibble + rr.Low, nibble + rr.High + 1);
            nibble = Math.Min(Math.Max(nibble, 0), 15);

            bytes[i] = (byte)((byt & 0xf0) | nibble);
        }
    }

    protected static int RandomInRange(Random r, double minVal, double maxVal)
    {
        int nextVal = (int)Math.Round(r.NextDouble() * (maxVal - minVal) + minVal);
        nextVal = (int)Math.Min(nextVal, maxVal);
        nextVal = (int)Math.Max(nextVal, minVal);
        return nextVal;
    }

    protected static void RandomizeInsideArray(Random r, byte[] array, int index, double lower, double upper)
    {
        var vanillaVal = array[index];
        int minVal = (int)(vanillaVal * lower);
        int maxVal = (int)(vanillaVal * upper);
        var newVal = (byte)Math.Min(r.Next(minVal, maxVal), 255);
        array[index] = newVal;
    }

    /// <summary>
    /// For a given set of bytes, set a masked portion of the value of each byte on or off (all 1's or all 0's) at a rate
    /// equal to the proportion of values at the addresses that have that masked portion set to a nonzero value.
    /// In effect, turn some values in a range on or off randomly in the proportion of the number of such values that are on in vanilla.
    /// </summary>
    /// <param name="bytes">Bytes to randomize.</param>
    /// <param name="mask">What part of the byte value at each address contains the configuration bit(s) we care about.</param>
    public static void RandomizeBits(Random r, byte[] bytes, int mask)
    {
        if (bytes.Length == 0) { return; }

        int notMask = mask ^ 0xFF;
        double vanillaBitSetCount = bytes.Where(b => (b & mask) != 0).Count();

        //proportion of the bytes that have nonzero values in the masked portion
        double fraction = vanillaBitSetCount / bytes.Length;

        for (int i = 0; i < bytes.Length; i++)
        {
            int v = bytes[i] & notMask;
            if (r.NextDouble() <= fraction)
            {
                v |= mask;
            }
            bytes[i] = (byte)v;
        }
    }

    /// When Rebonack's HP is set to exactly 2 * your damage, it will
    /// trigger a bug where you kill Rebo's horse while de-horsing him.
    /// This causes an additional key to drop, as well as softlocking
    /// the player if they die before killing Rebo. It seems to also
    /// trigger if you have exactly damage == Rebo HP (very high damage).
    /// 
    /// This has to be called after RandomizeEnemyStats and
    /// RandomizeAttackEffectiveness.
    ///
    /// (In Vanilla Zelda 2, your sword damage is never this high.)
    public void FixRebonackHorseKillBug()
    {
        byte[] attackValues = AttackEffectivenessTable;
        byte reboHp = BossHpTable[2];
        while (attackValues.Any(v => v * 2 == reboHp || v == reboHp))
        {
            reboHp++;
            BossHpTable[2] = reboHp;
        }
    }
}
