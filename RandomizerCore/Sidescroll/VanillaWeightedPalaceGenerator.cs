using System;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class VanillaWeightedPalaceGenerator : RandomWalkCoordinatePalaceGenerator
{
    private static readonly TableWeightedRandom<int>[] WeightedRandomDirections = [
        new([ // Palace 1  (7x3 in vanilla)
            (0, 7),  // left
            (1, 3),  // down
            (2, 3),  // up
            (3, 7),  // right
        ]),
        new([ // Palace 2  (8x4 in vanilla)
            (0, 2),  // left
            (1, 1),  // down
            (2, 1),  // up
            (3, 2),  // right
        ]),
        new([ // Palace 3  (7x3 in vanilla)
            (0, 7),  // left
            (1, 3),  // down
            (2, 3),  // up
            (3, 7),  // right
        ]),
        new([ // Palace 4  (6x4 in vanilla)
            (0, 3),  // left
            (1, 2),  // down
            (2, 2),  // up
            (3, 3),  // right
        ]),
        new([ // Palace 5  (7x5 in vanilla)
            (0, 7),  // left
            (1, 5),  // down
            (2, 5),  // up
            (3, 7),  // right
        ]),
        new([ // Palace 6  (10x6 in vanilla)
            (0, 5),  // left
            (1, 3),  // down
            (2, 3),  // up
            (3, 5),  // right
        ]),
        new([ // Great Palace  (10x13 in vanilla)
            (0, 10),  // left
            (1, 13),  // down
            (2, 13),  // up
            (3, 10),  // right
        ]),
    ];

    // how many rooms you have to traverse to reach the boss in the vanilla palaces.
    // does not include the entrance, but does include the boss room itself
    private static readonly int[] VanillaDistanceToBosses = [
        7,  // Palace 1
        13, // Palace 2
        10, // Palace 3
        7,  // Palace 4
        12, // Palace 5
        13, // Palace 6
        24, // GP
    ];

    protected override IWeightedSampler<int> GetDirectionWeights(int palaceNumber)
    {
        return WeightedRandomDirections[palaceNumber - 1];
    }

    protected override int GetBossMinDistance(RandomizerProperties props, int palaceNumber)
    {
        if (palaceNumber == 7)
        {
            // still just use the selected value for GP
            return props.DarkLinkMinDistance;
        }
        // lets not alter the chance of items being behind the boss
        bool continuesAfter = props.BossRoomsExitToPalace[palaceNumber - 1];
        if (continuesAfter)
        {
            return 0;
        }

        double vanillaLength = Palaces.VANILLA_LENGTHS[palaceNumber - 1];
        double length = props.PalaceLengths[palaceNumber - 1];
        double lengthScale = length / vanillaLength;

        // compress the min distance a bit for longer palaces
        double a = 0.4; // compression aggressiveness
        lengthScale = lengthScale / (1.0 + a * (lengthScale - 0.5));

        double vanillaDistance = VanillaDistanceToBosses[palaceNumber - 1];
        return (int)Math.Round((double)(vanillaDistance * lengthScale));
    }
}
