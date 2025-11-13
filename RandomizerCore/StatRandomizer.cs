using System;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore;

// TODO: move stat effectiveness here.

/// <summary>
/// Responsible for randomizing stats of all kinds. Each array should
/// be randomized at most once. If you have to re-roll for some reason,
/// a new StatRandomizer object should be created.
/// </summary>
public class StatRandomizer
{
    public byte[] WestEnemyHpTable { get; private set; } = null!;
    public byte[] EastEnemyHpTable { get; private set; } = null!;
    public byte[] Palace125EnemyHpTable { get; private set; } = null!;
    public byte[] Palace346EnemyHpTable { get; private set; } = null!;
    public byte[] GpEnemyHpTable { get; private set; } = null!;
    public byte[] BossHpTable { get; private set; } = null!;
    protected RandomizerProperties props { get; }

#if DEBUG
    private bool HasRandomizedHp = false;
#endif

    public StatRandomizer(ROM rom, RandomizerProperties props)
    {
        this.props = props;
        ReadHp(rom);
    }

    public void Randomize(Random r)
    {
        if (props.ShuffleEnemyHP)
        {
            RandomizeHp(r);
        }
    }

    public void Write(ROM rom)
    {
        WriteHp(rom);
    }

    protected void ReadHp(ROM rom)
    {
        WestEnemyHpTable = rom.GetBytes(RomMap.WEST_ENEMY_HP_TABLE, 0x24);
        EastEnemyHpTable = rom.GetBytes(RomMap.EAST_ENEMY_HP_TABLE, 0x24);
        Palace125EnemyHpTable = rom.GetBytes(RomMap.PALACE125_ENEMY_HP_TABLE, 0x24);
        Palace346EnemyHpTable = rom.GetBytes(RomMap.PALACE346_ENEMY_HP_TABLE, 0x24);
        GpEnemyHpTable = rom.GetBytes(RomMap.GP_ENEMY_HP_TABLE, 0x24);
        BossHpTable = [.. RomMap.bossHpAddresses.Select(rom.GetByte)];
    }

    protected void WriteHp(ROM rom)
    {
        rom.Put(RomMap.WEST_ENEMY_HP_TABLE, WestEnemyHpTable);
        rom.Put(RomMap.EAST_ENEMY_HP_TABLE, EastEnemyHpTable);
        rom.Put(RomMap.PALACE125_ENEMY_HP_TABLE, Palace125EnemyHpTable);
        rom.Put(RomMap.PALACE346_ENEMY_HP_TABLE, Palace346EnemyHpTable);
        rom.Put(RomMap.WEST_ENEMY_HP_TABLE, WestEnemyHpTable);
        rom.Put(RomMap.GP_ENEMY_HP_TABLE, GpEnemyHpTable);
        for (int i = 0; i < RomMap.bossHpAddresses.Count; i++) {
            rom.Put(RomMap.bossHpAddresses[i], BossHpTable[i]);
        }
    }

    protected void RandomizeHp(Random r)
    {
        int i;

        for (i = (int)EnemiesWest.MYU; i < 0x23; i++) { RollHpValue(r, WestEnemyHpTable, i); }

        for (i = (int)EnemiesEast.MYU; i < 0x1e; i++) { RollHpValue(r, EastEnemyHpTable, i); }

        // keeping old behavior where some non-enemies are not randomized (strike for jar, unused enemies etc.)
        for (i = (int)EnemiesPalace125.MYU; i < (int)EnemiesPalace125.STRIKE_FOR_RED_JAR; i++) { RollHpValue(r, Palace125EnemyHpTable, i); }
        for (i = (int)EnemiesPalace125.SLOW_BUBBLE; i < (int)EnemiesPalace125.HORSEHEAD; i++) { RollHpValue(r, Palace125EnemyHpTable, i); }
        for (i = (int)EnemiesPalace125.BLUE_STALFOS; i < 0x24; i++) { RollHpValue(r, Palace125EnemyHpTable, i); }

        for (i = (int)EnemiesPalace346.MYU; i < (int)EnemiesPalace346.STRIKE_FOR_RED_JAR_OR_IRON_KNUCKLE; i++) { RollHpValue(r, Palace346EnemyHpTable, i); }
        for (i = (int)EnemiesPalace346.SLOW_BUBBLE; i < (int)EnemiesPalace346.REBONAK; i++) { RollHpValue(r, Palace346EnemyHpTable, i); }
        for (i = (int)EnemiesPalace346.BLUE_STALFOS; i < 0x24; i++) { RollHpValue(r, Palace346EnemyHpTable, i); }

        for (i = (int)EnemiesGreatPalace.MYU; i < (int)EnemiesGreatPalace.STRIKE_FOR_RED_JAR_OR_FOKKA; i++) { RollHpValue(r, GpEnemyHpTable, i); }
        for (i = (int)EnemiesGreatPalace.ORANGE_MOA; i < (int)EnemiesGreatPalace.BUBBLE_GENERATOR; i++) { RollHpValue(r, GpEnemyHpTable, i); }
        for (i = (int)EnemiesGreatPalace.RED_DEELER; i < (int)EnemiesGreatPalace.ELEVATOR; i++) { RollHpValue(r, GpEnemyHpTable, i); }
        for (i = (int)EnemiesGreatPalace.SLOW_BUBBLE; i < 0x1b; i++) { RollHpValue(r, GpEnemyHpTable, i); }
        for (i = (int)EnemiesGreatPalace.FOKKERU; i < (int)EnemiesGreatPalace.KING_BOT; i++) { RollHpValue(r, GpEnemyHpTable, i); }

        for (i = 0; i < BossHpTable.Length; i++) { RollHpValue(r, BossHpTable, i); }

#if DEBUG
        Debug.Assert(!HasRandomizedHp);
        HasRandomizedHp = true;
#endif
    }

    protected void RollHpValue(Random r, byte[] array, int index)
    {
        var vanillaHp = array[index];
        var newHp = (byte)Math.Min(r.Next((int)(vanillaHp * 0.5), (int)(vanillaHp * 1.5)), 255);
        array[index] = newHp;
    }
}
