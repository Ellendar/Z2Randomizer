using System.Collections.Generic;
using NLog;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// --Ellendar
/// This class is a mess of functions and constants that should almost certainly live elsewhere.
/// I've cut down substantially on what lives where, but as time goes on almost everything here should be migrated to a better place.
/// </summary>
public class Shuffler
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    //None of these methods should have a reference to the ROM and write their own output.
    //All of these should output their results, and then those results should be written to the output state.
    public static void ShufflePalacePalettes(ROM ROMData, Random r)
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
}
