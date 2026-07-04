using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

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
            int brickRow = r.Next(PalaceColors.bricks.GetLength(0));
            int curtainRow = r.Next(PalaceColors.curtains.GetLength(0));

            int[] bricks = new int[3];
            int[] curtains = new int[3];
            for (int j = 0; j < 3; j++)
            {
                bricks[j] = PalaceColors.bricks[brickRow, j];
                curtains[j] = PalaceColors.curtains[curtainRow, j];
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
        List<DropCollectable> small = [];
        List<DropCollectable> large = [];

        if (props.Smallbluejar)
        {
            small.Add(DropCollectable.BLUE_JAR);
        }
        if (props.Smallredjar)
        {
            small.Add(DropCollectable.RED_JAR);
        }
        if (props.Small50)
        {
            small.Add(DropCollectable.SMALL_BAG);
        }
        if (props.Small100)
        {
            small.Add(DropCollectable.MEDIUM_BAG);
        }
        if (props.Small200)
        {
            small.Add(DropCollectable.LARGE_BAG);
        }
        if (props.Small500)
        {
            small.Add(DropCollectable.XL_BAG);
        }
        if (props.Small1up)
        {
            small.Add(DropCollectable.ONEUP);
        }
        if (props.Smallkey)
        {
            small.Add(DropCollectable.KEY);
        }

        if (props.Largebluejar)
        {
            large.Add(DropCollectable.BLUE_JAR);
        }
        if (props.Largeredjar)
        {
            large.Add(DropCollectable.RED_JAR);
        }
        if (props.Large50)
        {
            large.Add(DropCollectable.SMALL_BAG);
        }
        if (props.Large100)
        {
            large.Add(DropCollectable.MEDIUM_BAG);
        }
        if (props.Large200)
        {
            large.Add(DropCollectable.LARGE_BAG);
        }
        if (props.Large500)
        {
            large.Add(DropCollectable.XL_BAG);
        }
        if (props.Large1up)
        {
            large.Add(DropCollectable.ONEUP);
        }
        if (props.Largekey)
        {
            large.Add(DropCollectable.KEY);
        }

        // drops are kept vanilla if nothing is selected & RandomizeDrops is off
        if (small.Count > 0)
        {
            // shuffle order
            for (int i = 0; i < small.Count; i++)
            {
                int swap = r.Next(small.Count);
                (small[i], small[swap]) = (small[swap], small[i]);
            }
            // the game uses 8 drop items, fill the rest with copies at random
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
            }
        }
        if (large.Count > 0)
        {
            // shuffle order
            for (int i = 0; i < large.Count; i++)
            {
                int swap = r.Next(large.Count);
                (large[i], large[swap]) = (large[swap], large[i]);
            }
            // the game uses 8 drop items, fill the rest with copies at random
            for (int i = 0; i < 8; i++)
            {
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

    public void ShuffleBossDrop(ROM ROMData, Random r)
    {
        var options = Enum.GetValues<DropCollectable>();
        var drop = options.Sample(r);
        ROMData.Put(0x1de29, (byte)(drop - 0x80));
    }
}
