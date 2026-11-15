namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>An immutable encoding: the budget check and eventual write use the same bytes.</summary>
internal sealed class EncodedWorldMap
{
    private readonly byte[] bytes;
    internal int Length => bytes.Length;

    internal EncodedWorldMap(OverworldMap map, MapEncodingOptions options = default)
        => bytes = WorldMapCodec.Encode(map, options);

    internal bool TryStage(TerrainRomPatch patch, int offset, int capacity)
    {
        if (Length > capacity) return false;
        patch.Put(offset, bytes);
        for (int i = Length; i < capacity; i++) patch.Put(checked(offset + i), 0x0B);
        return true;
    }
}
