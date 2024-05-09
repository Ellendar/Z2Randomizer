using System;
using System.Threading;

namespace Z2Randomizer.Core.Sidescroll;

public class CartesianPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        throw new NotImplementedException();
    }
}
