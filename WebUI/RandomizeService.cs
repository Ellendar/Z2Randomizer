using System;
using BlazorBootstrap;
using Z2Randomizer.Core;
using Z2Randomizer.Core.Sidescroll;

namespace WebUI;

public interface IRandomizerService
{
	Task<byte[]> Randomize(RandomizerConfiguration config, byte[] rom, string palaceRooms, Action<string> progress);
}

public class RandomizeService : IRandomizerService
{
	public RandomizeService()
	{
	}

    public Task<byte[]> Randomize(RandomizerConfiguration config, byte[] rom, string palaceRooms, Action<string> progress)
    {
        //Convert.FromBase64String(rom);
        var rooms = new PalaceRooms(palaceRooms, null);
        var randomizer = new Hyrule(config, rom, rooms);
        return randomizer.Randomize(progress);
    }
}

