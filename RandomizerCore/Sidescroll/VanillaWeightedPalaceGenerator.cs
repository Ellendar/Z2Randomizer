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

    protected override IWeightedSampler<int> GetDirectionWeights(int palaceNumber)
    {
        return WeightedRandomDirections[palaceNumber - 1];
    }
}
