using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Queued map bytes, without a ROM copy. Later byte writes win.
/// </summary>
internal sealed class TerrainRomPatch
{
    private readonly List<(int Offset, byte Value)> writes = [];

    internal void Put(int offset, byte value) => writes.Add((offset, value));
    internal void Put(int offset, params byte[] values)
    {
        for (int i = 0; i < values.Length; i++) Put(checked(offset + i), values[i]);
    }

    internal void Clear()
    {
        writes.Clear();
    }

    internal void Apply(ROM destination)
    {
        // Validate the whole patch before touching the destination, including writes
        // preceding an invalid address. Applying the validated byte assignments cannot fail.
        foreach (var (offset, _) in writes)
            if ((uint)offset >= (uint)destination.rawdata.Length)
                throw new ArgumentOutOfRangeException(nameof(destination), offset, "Terrain patch exceeds the ROM.");
        foreach (var (offset, value) in writes) destination.Put(offset, value);
    }
}
