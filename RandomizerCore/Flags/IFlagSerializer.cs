namespace Z2Randomizer.RandomizerCore.Flags;

public interface IFlagSerializer
{
    public int GetLimit();
    public int Serialize(object? obj);
    public object? Deserialize(int option);
}
