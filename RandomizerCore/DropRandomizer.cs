using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Responsible for randomizing enemy drops, pbag amounts, and boss drops.
/// Each table should be randomized at most once. If you have to re-roll for some reason,
/// a new DropRandomizer object should be created.
/// </summary>
public class DropRandomizer
{
    public const int DROP_TABLE_SIZE = 8;
    public const int PBAG_NUM_TYPES = 4;

    /* 0 - 0
     * 1 - 2
     * 2 - 3
     * 3 - 5
     * 4 - 10
     * 5 - 20
     * 6 - 30
     * 7 - 50
     * 8 - 70
     * 9 - 100
     * 10 - 150
     * 11 - 200
     * 12 - 300
     * 13 - 500
     * 14 - 700
     * 15 - 1000 */
    public static ReadOnlyCollection<int> XP_VALUES = [0, 2, 3, 5, 10, 20, 30, 50, 70, 100, 150, 200, 300, 500, 700, 1000];

    public byte[] SmallDropTable { get; private set; } = null!;
    public byte[] LargeDropTable { get; private set; } = null!;
    public byte[] PbagAmountTable { get; private set; } = null!;
    public SmallItem BossDrop { get; private set; }
    public byte DropFrequency { get; private set; }

    protected RandomizerProperties props { get; }

#if DEBUG
    private bool hasRandomized = false;
    private bool hasWritten = false;
#endif

    public DropRandomizer(ROM rom, RandomizerProperties props)
    {
        this.props = props;
        ReadDrops(rom);
        ReadPbagAmounts(rom);
        ReadBossDrop(rom);
        ReadDropFrequency(rom);
    }

    public void Randomize(Random r)
    {
#if DEBUG
        Debug.Assert(!hasRandomized);
        hasRandomized = true;
#endif
        RandomizeDrops(r);
        RandomizePbagAmounts(r);
        RandomizeBossDrop(r);
        RandomizeDropFrequency(r);
    }

    public void Write(ROM rom)
    {
#if DEBUG
        Debug.Assert(!hasWritten);
        hasWritten = true;
#endif
        WriteDrops(rom);
        WritePbagAmounts(rom);
        WriteBossDrop(rom);
        WriteDropFrequency(rom);
    }

    [Conditional("DEBUG")]
    public void AssertHasRandomized(bool value = true)
    {
#if DEBUG
        Debug.Assert(hasRandomized == value);
#endif
    }

    [Conditional("DEBUG")]
    public void AssertHasWritten(bool value = true)
    {
#if DEBUG
        Debug.Assert(hasWritten == value);
#endif
    }

    protected void ReadDrops(ROM rom)
    {
        SmallDropTable = rom.GetBytes(RomMap.SMALL_DROP_TABLE, DROP_TABLE_SIZE);
        LargeDropTable = rom.GetBytes(RomMap.LARGE_DROP_TABLE, DROP_TABLE_SIZE);
    }

    protected void WriteDrops(ROM rom)
    {
        rom.Put(RomMap.SMALL_DROP_TABLE, SmallDropTable);
        rom.Put(RomMap.LARGE_DROP_TABLE, LargeDropTable);
    }

    protected void ReadPbagAmounts(ROM rom)
    {
        PbagAmountTable = rom.GetBytes(RomMap.PBAG_XP_TABLE, PBAG_NUM_TYPES);
    }

    protected void WritePbagAmounts(ROM rom)
    {
        rom.Put(RomMap.PBAG_XP_TABLE, PbagAmountTable);
    }

    protected void ReadBossDrop(ROM rom)
    {
        byte rawValue = rom.GetByte(RomMap.BOSS_DROP_COLLECTABLE);
        BossDrop = (SmallItem)(rawValue + 0x80);
    }

    protected void WriteBossDrop(ROM rom)
    {
        rom.Put(RomMap.BOSS_DROP_COLLECTABLE, (byte)(BossDrop - 0x80));
    }

    protected void ReadDropFrequency(ROM rom)
    {
        DropFrequency = rom.GetByte(RomMap.ENEMY_DROP_FREQUENCY);
    }

    protected void WriteDropFrequency(ROM rom)
    {
        rom.Put(RomMap.ENEMY_DROP_FREQUENCY, DropFrequency);
    }

    protected void RandomizeDropFrequency(Random r)
    {
        if (!props.ShuffleItemDropFrequency) { return; }

        DropFrequency = (byte)(r.Next(5) + 4);
    }

    protected void RandomizeDrops(Random r)
    {
        List<SmallItem> small = BuildDropList(props.Smallbluejar, props.Smallredjar,
            props.Small50, props.Small100, props.Small200, props.Small500,
            props.Small1up, props.Smallkey);

        List<SmallItem> large = BuildDropList(props.Largebluejar, props.Largeredjar,
            props.Large50, props.Large100, props.Large200, props.Large500,
            props.Large1up, props.Largekey);

        // drops are kept vanilla if nothing is selected & RandomizeDrops is off
        if (small.Count > 0)
        {
            SmallDropTable = ShuffleDropTable(r, small);
        }
        if (large.Count > 0)
        {
            LargeDropTable = ShuffleDropTable(r, large);
        }
    }

    protected static List<SmallItem> BuildDropList(bool blueJar, bool redJar, bool bag50, bool bag100, bool bag200, bool bag500, bool oneUp, bool key)
    {
        List<SmallItem> list = [];
        if (blueJar) { list.Add(SmallItem.BLUE_JAR); }
        if (redJar) { list.Add(SmallItem.RED_JAR); }
        if (bag50) { list.Add(SmallItem.SMALL_BAG); }
        if (bag100) { list.Add(SmallItem.MEDIUM_BAG); }
        if (bag200) { list.Add(SmallItem.LARGE_BAG); }
        if (bag500) { list.Add(SmallItem.XL_BAG); }
        if (oneUp) { list.Add(SmallItem.ONEUP); }
        if (key) { list.Add(SmallItem.KEY); }
        return list;
    }

    protected byte[] ShuffleDropTable(Random r, List<SmallItem> drops)
    {
        for (int i = 0; i < drops.Count; i++)
        {
            int swap = r.Next(drops.Count);
            (drops[i], drops[swap]) = (drops[swap], drops[i]);
        }

        byte[] table = new byte[DROP_TABLE_SIZE];
        for (int i = 0; i < DROP_TABLE_SIZE; i++)
        {
            if (i < drops.Count)
            {
                table[i] = (byte)drops[i];
            }
            else
            {
                table[i] = (byte)drops[r.Next(drops.Count)];
            }
        }
        return table;
    }

    protected void RandomizePbagAmounts(Random r)
    {
        if (!props.ShufflePbagXp) { return; }

        PbagAmountTable = [
            (byte)r.Next(5, 10),
            (byte)r.Next(7, 12),
            (byte)r.Next(9, 14),
            (byte)r.Next(11, 16),
        ];
    }

    protected void RandomizeBossDrop(Random r)
    {
        if (!props.BossItem) { return; }

        var options = Enum.GetValues<SmallItem>();
        BossDrop = options.Sample(r);
    }

    public string GenerateSpoiler()
    {
        StringBuilder sb = new();

        sb.AppendLine("P-BAG AMOUNTS");
        sb.AppendLine("================");
        for (int i = 0; i < PBAG_NUM_TYPES; i++)
        {
            Collectable pbagCollectable = Collectable.SMALL_BAG + i;
            var xpIndex = PbagAmountTable[i];
            sb.AppendLine($"{pbagCollectable.ToString() + ":",-11} {XP_VALUES[xpIndex]}");
        }
        sb.AppendLine("----------------");
        sb.AppendLine();

        sb.AppendLine("ENEMY DROPS");
        var smallDropString = "SMALL: " + string.Join(" ", SmallDropTable.Select(b => ToShortString((SmallItem)b)));
        var largeDropString = "LARGE: " + string.Join(" ", LargeDropTable.Select(b => ToShortString((SmallItem)b)));
        var width = Math.Max(smallDropString.Length, largeDropString.Length);
        sb.AppendLine(new string('=', width));
        sb.AppendLine(smallDropString);
        sb.AppendLine(largeDropString);
        sb.AppendLine("BOSS:  " + ToShortString(BossDrop));
        sb.AppendLine(new string('-', width));
        sb.AppendLine($"Drops every {DropFrequency} enemies");

        return sb.ToString();
    }

    public string ToShortString(SmallItem b)
    {
        return (b switch
        {
            SmallItem.KEY => "Key",
            SmallItem.SMALL_BAG => XP_VALUES[PbagAmountTable[0]].ToString(),
            SmallItem.MEDIUM_BAG => XP_VALUES[PbagAmountTable[1]].ToString(),
            SmallItem.LARGE_BAG => XP_VALUES[PbagAmountTable[2]].ToString(),
            SmallItem.XL_BAG => XP_VALUES[PbagAmountTable[3]].ToString(),
            SmallItem.BLUE_JAR => "Blue",
            SmallItem.RED_JAR => "Red",
            SmallItem.ONEUP => "1-Up",
            _ => "?",
        }).PadRight(4);
    }
}
