namespace Z2Randomizer.RandomizerCore.Overworld;

public class OverworldMap
{
    private Terrain[,] inner;

    public OverworldMap(int mapRows, int mapColumns)
    {
        inner = new Terrain[mapRows, mapColumns];
    }

    public OverworldMap(Terrain[,] map)
    {
        inner = map;
    }

    public Terrain this[int y, int x]
    {
        get => inner[y, x];

        set
        {
            // if (y == 27 && x == 20) Debugger.Break();
            inner[y, x] = value;
        }
    }

    public Terrain this[IntVector2 pos]
    {
        get => inner[pos.Y, pos.X];

        set
        {
            inner[pos.Y, pos.X] = value;
        }
    }

    public int GetLength(int dimension)
    {
        return inner.GetLength(dimension);
    }
}
