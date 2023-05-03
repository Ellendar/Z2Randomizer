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
            int group = r.Next(3);
            int brickRow = r.Next(Graphics.brickgroup[group].GetLength(0));
            int curtainRow = r.Next(Graphics.curtaingroup[group].GetLength(0));

            int[] bricks = new int[3];
            int[] curtains = new int[3];
            for (int j = 0; j < 3; j++)
            {
                bricks[j] = Graphics.brickgroup[group][brickRow, j];
                curtains[j] = Graphics.curtaingroup[group][curtainRow, j];
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
        if (props.ShufflePbagXp)
        {
            ROMData.Put(0x1e800, (byte)r.Next(5, 10));
            ROMData.Put(0x1e801, (byte)r.Next(7, 12));
            ROMData.Put(0x1e802, (byte)r.Next(9, 14));
            ROMData.Put(0x1e803, (byte)r.Next(11, 16));
        }
    }

    public int ShuffleKasutoJars(ROM ROMData, Random r)
    {
        int kasutoJars = 7;
        if (props.KasutoJars)
        {
            kasutoJars = r.Next(5, 8);
            ROMData.WriteKasutoJarAmount(kasutoJars);
        }
        return kasutoJars;
    }

    public void ShuffleBossDrop(ROM ROMData, Random r)
    {
        int drop = drops[r.Next(drops.Count())];
        ROMData.Put(0x1de29, (byte)(drop - 0x80));

        /*
         * LE79A                                                                          ;
            lda      #$08                          ; 0x1e7aa $E79A A9 08                   ; A = 08
            sta      $EF                           ; 0x1e7ac $E79C 85 EF                   ; Sound Effects Type 4
            cpy      #$08                          ; 0x1e7ae $E79E C0 08                   ;
            bne      LE7BB                         ; 0x1e7b0 $E7A0 D0 19                   ;
            lda      $0728                         ; 0x1e7b2 $E7A2 AD 28 07                ; Related to boss key state
            beq      LE7B5                         ; 0x1e7b5 $E7A5 F0 0E                   ;
            lda      #$00                          ; 0x1e7b7 $E7A7 A9 00                   ; A = 00
            sta      $0728                         ; 0x1e7b9 $E7A9 8D 28 07                ;;_728_FreezeScrolling		= $728	;1=freeze screen, prevent from exiting left/right
            lda      $07FB                         ; 0x1e7bc $E7AC AD FB 07                ;
            bne      LE7B5                         ; 0x1e7bf $E7AF D0 04                   ;
            ;                                                                              ;Restart Music after taking a key that falls after beating a boss
            lda      #$02                          ; 0x1e7c1 $E7B1 A9 02                   ; A = 02 (04 = quiet version of Palace theme)
            sta      $EB                           ; 0x1e7c3 $E7B3 85 EB                   ; Music
            LE7B5                                                                          ;
            inc      $0793                         ; 0x1e7c5 $E7B5 EE 93 07                ; Number of Keys
            jmp      LE797                         ; 0x1e7c8 $E7B8 4C 97 E7    
        */
        ROMData.Put(0x1e7aa, new byte[] { 0xAD, 0x28, 0x07, 0xF0, 0x0E, 0xA9, 0x00, 0x8D, 0x28, 0x07, 0xAD, 0xFB, 0x07, 0xD0, 0x04, 0xa9, 0x02, 0x85, 0xeb, 0xa9, 0x08, 0x85, 0xef, 0xc0, 0x08, 0xd0, 0x06, 0xee, 0x93, 0x07, 0x4c, 0x97, 0xe7 });

        //jump to 1f33a
        ROMData.Put(0x1e81c, new byte[] { 0x20, 0x2a, 0xf3, 0xea });
        ROMData.Put(0x1e85b, new byte[] { 0x20, 0x35, 0xf3, 0xea });

        //1f33a

        //if $EB == 2
        //A5 eb
        //c9 02
        //f0 04
        //else $EB = 10
        //A9 10
        //85 eb
        //60
        ROMData.Put(0x1f33a, new byte[] { 0xA5, 0xEB, 0xC9, 0x02, 0xf0, 0x04, 0xa9, 0x10, 0x85, 0xeb, 0x60 });

        //1f345
        ROMData.Put(0x1f345, new byte[] { 0xA5, 0xEB, 0xC9, 0x02, 0xf0, 0x04, 0xa9, 0x00, 0x85, 0xeb, 0x60 });
    }
}
