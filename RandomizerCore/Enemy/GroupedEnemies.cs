using System;

namespace Z2Randomizer.RandomizerCore.Enemy;

public abstract class GroupedEnemiesBase
{
    /// the purpose of this is to have a handle to GroupedEnemies
    /// without having to pass along the generic type T
}

public class GroupedEnemies<T> : GroupedEnemiesBase where T : Enum
{
    public GroupedEnemies(T[] smallEnemies, T[] largeEnemies, T[] flyingEnemies, T[] generators)
    {
        SmallEnemies = smallEnemies;
        LargeEnemies = largeEnemies;
        FlyingEnemies = flyingEnemies;
        Generators = generators;
        GroundEnemies = [.. SmallEnemies, .. LargeEnemies];
    }

    public T[] SmallEnemies { get; init; }
    public T[] LargeEnemies { get; init; }
    public T[] GroundEnemies { get; init; }
    public T[] FlyingEnemies { get; init; }
    public T[] Generators { get; init; }
}
