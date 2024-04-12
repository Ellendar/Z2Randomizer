using Assembler;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.Core;

/// <summary>
/// --Ellendar
/// This class is a mess of functions and constants that should almost certainly live elsewhere.
/// I've cut down substantially on what lives where, but as time goes on almost everything here should be migrated to a better place.
/// </summary>
public class Shuffler
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();


    private static readonly List<int> bossRooms = new List<int> { 13, 34, 41 }; //break this up by palace group
    private static readonly List<int> bossRooms2 = new List<int> { 14, 28, 58 }; //break this up by palace group
    private static readonly List<int> bossRooms3 = new List<int> { 53, 54 };

    private readonly int[] drops = { 0x8a, 0x8b, 0x8c, 0x8d, 0x90, 0x91, 0x92, 0x88 };//items that can be dropped



    //instance variables
    private RandomizerProperties props;
    //private ROM ROMData;
    //private Character link;
    //private Random R1;
    //public Random R { get => R1; set => R1 = value; }
    public RandomizerProperties Props { get => props; set => props = value; }



    public Shuffler(RandomizerProperties props)
    {
        this.props = props;
    }

    //None of these methods should have a reference to the ROM and write their own output.
    //All of these should output their results, and then those results should be written to the output state.
    public void ShufflePalacePalettes(ROM ROMData, Random r)
    {
        List<int[]> brickList = new List<int[]>();
        List<int[]> curtainList = new List<int[]>();
        List<int> bRows = new List<int>();
        List<int> binRows = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            int brickRow = r.Next(Sidescroll.PalaceColors.bricks.GetLength(0));
            int curtainRow = r.Next(Sidescroll.PalaceColors.curtains.GetLength(0));

            int[] bricks = new int[3];
            int[] curtains = new int[3];
            for (int j = 0; j < 3; j++)
            {
                bricks[j] = Sidescroll.PalaceColors.bricks[brickRow, j];
                curtains[j] = Sidescroll.PalaceColors.curtains[curtainRow, j];
            }

            brickList.Add(bricks);
            curtainList.Add(curtains);

            bRows.Add(r.Next(7));
            binRows.Add(r.Next(7));
        }

        ROMData.WritePalacePalettes(brickList, curtainList, bRows, binRows);
    }

    public void ShuffleDrops(ROM ROMData, Random r)
    {
        List<int> small = new List<int>();
        List<int> large = new List<int>();

        if (props.Smallbluejar)
        {
            small.Add(0x90);
        }
        if (props.Smallredjar)
        {
            small.Add(0x91);
        }
        if (props.Small50)
        {
            small.Add(0x8a);
        }
        if (props.Small100)
        {
            small.Add(0x8b);
        }
        if (props.Small200)
        {
            small.Add(0x8c);
        }
        if (props.Small500)
        {
            small.Add(0x8d);
        }
        if (props.Small1up)
        {
            small.Add(0x92);
        }
        if (props.Smallkey)
        {
            small.Add(0x88);
        }
        if (props.Largebluejar)
        {
            large.Add(0x90);
        }
        if (props.Largeredjar)
        {
            large.Add(0x91);
        }
        if (props.Large50)
        {
            large.Add(0x8a);
        }
        if (props.Large100)
        {
            large.Add(0x8b);
        }
        if (props.Large200)
        {
            large.Add(0x8c);
        }
        if (props.Large500)
        {
            large.Add(0x8d);
        }
        if (props.Large1up)
        {
            large.Add(0x92);
        }
        if (props.Largekey)
        {
            large.Add(0x88);
        }

        if (small.Count + large.Count > 0)
        {
            for (int i = 0; i < small.Count(); i++)
            {
                int swap = r.Next(small.Count());
                int temp = small[i];
                small[i] = small[swap];
                small[swap] = temp;
            }

            for (int i = 0; i < large.Count(); i++)
            {
                int swap = r.Next(large.Count());
                int temp = large[i];
                large[i] = large[swap];
                large[swap] = temp;
            }
            for (int i = 0; i < 8; i++)
            {
                if (i < small.Count())
                {
                    ROMData.Put(0x1E880 + i, (byte)small[i]);
                }
                else
                {
                    ROMData.Put(0x1E880 + i, (byte)small[r.Next(small.Count())]);
                }
                if (i < large.Count())
                {
                    ROMData.Put(0x1E888 + i, (byte)large[i]);
                }
                else
                {
                    ROMData.Put(0x1E888 + i, (byte)large[r.Next(large.Count())]);
                }
            }
        }
    }

    public void ShufflePbagAmounts(ROM ROMData, Random r)
    {
        /*
         * 0 - 0
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
         * 15 - 1000
         */
        if (props.ShufflePbagXp)
        {
            ROMData.Put(0x1e800, (byte)r.Next(5, 10));
            ROMData.Put(0x1e801, (byte)r.Next(7, 12));
            ROMData.Put(0x1e802, (byte)r.Next(9, 14));
            ROMData.Put(0x1e803, (byte)r.Next(11, 16));
        }
    }

    public void ShuffleBossDrop(ROM ROMData, Random r, Engine engine)
    {
        int drop = drops[r.Next(drops.Count())];
        ROMData.Put(0x1de29, (byte)(drop - 0x80));

        Assembler.Assembler a = new();
        a.Code("""
.segment "PRG7"
.org $E79A
    ; Branch if scroll frozen
    lda $0728
    beq +
        ; freeze scroll
        lda #0
        sta $0728
        ; branch if the music is already playing
        lda $07fb
        bne +
            ; otherwise resume the previous track (palace theme)
            lda #2
            sta $eb
    + 
    ; Write the "grab item" sound effect to the sfx queue
    lda #8
    sta $ef
    ; Branch if the item we are getting is NOT a key
    cpy #8
    bne +
        ; increment number of keys and carry on
        inc $0793
        jmp $e797
    +
    ; Otherwise continue to $E7BB which is the start of the get item code
    .assert * = $E7BB

; Patch a few locations to make sure the music returns to normal after getting an item
.org $e80c
    jsr DontSwitchMusicIfInPalace1
    nop

.org $e84b
    jsr DontSwitchMusicIfInPalace2
    nop

.reloc
DontSwitchMusicIfInPalace1:
    lda $eb
    cmp #$02
    beq +
        ; Restore track 16
        lda #$10
        sta $eb
+   rts

DontSwitchMusicIfInPalace2:
    lda $eb
    cmp #$02
    beq +
        ; Restore track 0
        lda #$10
        sta $eb
+   rts
""");
        engine.Modules.Add(a.Actions);
    }
}
