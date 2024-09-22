#nullable enable

using Assembler;
using FtRandoLib.Importer;
using FtRandoLib.Library;
using FtRandoLib.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Z2Randomizer.Core;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore;

using UsesSongs = Dictionary<Usage, List<ISong>>;
using SongMap = Dictionary<int, ISong?>;

internal enum Usage
{
    Overworld,
    Town,
    Encounter,
    Cave,
    Palace,
    GreatPalace,
    Boss,
    LastBoss,
    Credits,
}

internal class RomAccessAdapter : IRomAccess
{
    byte[] _rom;

    public RomAccessAdapter(byte[] rom) => _rom = rom.ToArray();

    public byte[] Rom => _rom.ToArray();

    public void Write(int offset, byte data, string comment = "")
    {
        _rom[offset] = data;
    }

    public void Write(int offset, IReadOnlyList<byte> data, string comment = "")
    {
        byte[]? byteArray = data as byte[];
        if (byteArray is not null)
            Array.Copy(byteArray, 0, _rom, offset, byteArray.Length);
        else
            for (int i = 0; i < data.Count; i++)
                _rom[offset + i] = data[i];
    }
}

/*internal  class LoggerAdapter : Logger
{
    StringBuilder sb;

    public LoggerAdapter(StringBuilder sb) => this.sb = sb;

    protected override void InternalWrite(string? message) => sb.Append(message);
    protected override void InternalWriteLine(string? message) => sb.AppendLine(message);
}*/

internal class Z2Importer : Importer
{
    protected override int BankSize => 0x2000;

    static readonly IReadOnlyDictionary<string, BankLayout> Z2BankLayouts =
        new IstringDictionary<BankLayout>()
        {
            { "ft", new(0xa000, 0x2000) },
        };

    protected override List<int> FreeBanks { get; }

    protected override int PrimarySquareChan => 1;
    protected override IstringSet Uses { get; }
        = new(Enum.GetNames<Usage>());
    protected override IstringSet DefaultUses { get; }
        = new() { "Palace", "GreatPalace" };
    protected override bool DefaultStreamingSafe => true;

    protected override int SongMapOffs => 0x19e10;
    protected override int SongModAddrTblOffs => 0x19f10;

    protected override HashSet<int> BuiltinSongIdcs { get; }
        = new(ExRange(0, NumBuiltinSongs));
    protected override List<int> FreeSongIdcs { get; }
        = new(ExRange(NumBuiltinSongs, 0x7f));
    protected override int NumSongs => 0x7f;

    protected override IReadOnlyDictionary<string, SongMapInfo> SongMapInfos { get; }
        = new SongMapInfo[]
        {
            new SongMapInfo(AreaSongMapName, AreaSongMapOffs,
                NumAreas),
            new SongMapInfo(EnemySongMapName, EnemySongMapOffs,
                NumEnemyEntries)
        }.ToDictionary(info => info.Name);

    protected override int NumFtChannels => 5;
    protected override int DefaultFtStartAddr => 0;
    protected override int DefaultFtPrimarySquareChan => 0;

    const int NumBuiltinSongs = 0x13;

    public const string AreaSongMapName = "AreaSongMap";
    const int AreaSongMapOffs = 0x18c10;
    const int NumAreas = 0x100;

    public const string EnemySongMapName = "EnemySongMap";
    const int EnemySongMapOffs = AreaSongMapOffs + NumAreas;
    const int NumEnemyEntries = 0xc0;

    record BuiltinSongInfo(int Index, string Title, Usage[] Uses);
    static BuiltinSongInfo[] _builtinSongInfos = {
        new(1, "Overworld", new[] { Usage.Overworld }),
        new(3, "Battle", new[] { Usage.Encounter, Usage.Cave }),
        new(5, "Town", new[] { Usage.Town }),
        new(9, "Palace", new[] { Usage.Palace }),
        new(0xb, "Boss", new[] { Usage.Boss }),
        new(0xd, "Great Palace", new[] { Usage.GreatPalace }),
        new(0x10, "Credits", new[] { Usage.Credits }),
        new(0x12, "Last Boss", new[] { Usage.LastBoss }),
    };

    public Z2Importer(IRomAccess romAccess, IEnumerable<int> freeBanks)
        : base(Z2BankLayouts, romAccess)
    {
        FreeBanks = new(freeBanks);
    }

    public IEnumerable<BuiltinSong> CreateBuiltinSongs()
    {
        foreach (var info in _builtinSongInfos)
        {
            BuiltinSong song = new(
                info.Index,
                $"Zelda II The Adventure of Link - {info.Title}",
                "Akito Nakatsuka",
                Enabled: true,
                Uses: new(info.Uses.Select(usage => usage.ToString())),
                PrimarySquareChan: PrimarySquareChan,
                StreamingSafe: true);

            yield return song;
        }
    }
}

internal class MusicRandomizer
{
    /// <summary>
    /// Randomly select and import songs from the specified libraries.
    /// </summary>
    /// <returns>The list of free banks remaining after importing the songs.</returns>
    public static IEnumerable<int> ImportSongs(
        Hyrule hyrule,
        int seed,
        IEnumerable<string> libPaths,
        IEnumerable<int> freeBanks,
        bool safeOnly)
    {
        MusicRandomizer randomizer = new(hyrule, seed, libPaths, freeBanks, safeOnly);

        return randomizer.ImportSongs();
    }

    Hyrule _hyrule;
    Random _rng;
    RomAccessAdapter _romAccess;
    List<string> _libPaths;
    List<int> _freeBanks;

    Z2Importer _imptr;

    MusicRandomizer(
        Hyrule hyrule,
        int seed,
        IEnumerable<string> libPaths,
        IEnumerable<int> freeBanks,
        bool safeOnly)
    {
        _hyrule = hyrule;
        _rng = new Random(seed);
        _romAccess = new(_hyrule.ROMData.GetBytes(0, ROM.RomSize));
        _libPaths = new(libPaths);
        _freeBanks = new(freeBanks);

        _imptr = new(_romAccess, _freeBanks);
        _imptr.DefaultParserOptions.SafeOnly = safeOnly;
    }

    IEnumerable<int> ImportSongs()
    {
        if (_libPaths.Count == 0)
            return _freeBanks; // Nothing to do

        var songs = LoadSongs();
        var usesSongs = SelectUsesSongs(songs);
        var songMap = CreateSongMap(usesSongs);
        var areaSongMap = CreateAreaSongMap(usesSongs);
        var enemySongMap = CreateEnemySongMap(usesSongs);

        IstringDictionary<IReadOnlyDictionary<int, ISong?>> songMaps = new()
        {
            { Z2Importer.AreaSongMapName, areaSongMap },
            { Z2Importer.EnemySongMapName, enemySongMap },
        };

        HashSet<int> freeBanks = new(_freeBanks);
        _imptr.Import(
            songMap,
            songMaps,
            out freeBanks);

        _hyrule.ROMData.Put(0, _romAccess.Rom);

        return freeBanks;
    }

    List<ISong> LoadSongs()
    {
        List<BuiltinSong> builtins = new(_imptr.CreateBuiltinSongs());

        List<FtSong> ftSongs = new();
        foreach (var libPath in _libPaths)
        {
            string libData = File.ReadAllText(libPath, Encoding.UTF8);
            ftSongs.AddRange(_imptr.LoadFtJsonLibrarySongs(libData));
        }

        //// TODO: Fix the issue with transitioning from builtin to FT in z2ft so builtins can be used
        List<ISong> songs = new(/*builtins*/);
        songs.AddRange(ftSongs);

        return songs;
    }

    UsesSongs SelectUsesSongs(IEnumerable<ISong> songs)
    {
        Dictionary<Usage, int> numUsageSongs = new()
        {
            { Usage.Overworld, 1 },
            { Usage.Town, 4 },
            { Usage.Encounter, 4 },
            { Usage.Cave, 4 },
            { Usage.Palace, 6 },
            { Usage.GreatPalace, 1 },
            { Usage.Boss, 1 },
            { Usage.LastBoss, 1 },
            { Usage.Credits, 1 },
        };

        var usesSongs = _imptr.SplitSongsByUsage(songs);
        UsesSongs selUsesSongs = new();
        foreach (var usage in Enum.GetValues<Usage>())
        {
            string usageStr = usage.ToString();
            List<ISong> selSongs = selUsesSongs[usage] = new();
            List<ISong>? usageSongs = null;

            if (!usesSongs.TryGetValue(usageStr, out usageSongs)
                || usageSongs.Count == 0)
                continue;

            int numNeeded = numUsageSongs[usage],
                repsNeeded = numNeeded / usageSongs.Count + 1;
            ISong[] shufSongs = Enumerable.Repeat(usageSongs, repsNeeded)
                .SelectMany(x => x).ToArray();

            _rng.Shuffle(shufSongs);

            selSongs.AddRange(shufSongs.Take(numNeeded));
        }

        return selUsesSongs;
    }

    SongMap CreateSongMap(UsesSongs usesSongs)
    {
        Dictionary<Usage, int> usageSongIdcs = new()
        {
            { Usage.Overworld, 1 },
            { Usage.Boss, 0xb },
            { Usage.LastBoss, 0x12 },
            { Usage.Credits, 0x10 },
        };

        SongMap songMap = new();
        foreach (var (usage, songIdx) in usageSongIdcs)
        {
            var usageSongs = usesSongs[usage];
            if (usageSongs.Count == 0)
                continue;

            Debug.Assert(usageSongs.Count == 1);

            songMap[songIdx] = usageSongs[0];
        }

        return songMap;
    }

    SongMap CreateAreaSongMap(UsesSongs usesSongs)
    {
        // Find all the relevant locations
        var usesLocs = new Usage[] 
        { 
            Usage.Town, 
            Usage.Palace, 
            Usage.GreatPalace, 
            Usage.Cave, 
            Usage.Encounter,
        }.ToDictionary(k => k, k => new List<Location>());

        foreach (var world in _hyrule.worlds)
        {
            foreach (var loc in world.AllLocations)
            {
                Usage usage;
                if (loc.TerrainType == Terrain.TOWN)
                {
                    if (loc.ActualTown == Town.OLD_KASUTO // ??
                        /*|| loc.ActualTown == Town.SARIA_SOUTH*/)
                        continue;

                    usage = Usage.Town;
                }
                else if (loc.TerrainType == Terrain.PALACE)
                {
                    if (loc.PalaceNumber == 0)
                        continue; // North palace

                    usage = (loc.PalaceNumber < 7)
                        ? Usage.Palace
                        : Usage.GreatPalace;
                }
                else if (loc.TerrainType == Terrain.CAVE
                    || loc.FallInHole != 0)
                    usage = Usage.Cave;
                else
                    usage = Usage.Encounter;

                usesLocs[usage].Add(loc);
            }
        }

        // Assign music to the locations
        SongMap areaSongMap = new();
        foreach (var usage in new Usage[] { Usage.Town, Usage.Palace, Usage.Cave, Usage.Encounter })
        {
            var usageSongs = usesSongs[usage];
            Func<Location, int> GetSongIdx = usage switch
            {
                // One song per palace
                Usage.Palace => (loc => loc.PalaceNumber - 1),
                Usage.GreatPalace => (loc => 0),
                // One song per continent
                _ => (loc => (int)loc.Continent),
            };

            foreach (var loc in usesLocs[usage])
            {
                int contIdx = (int)loc.Continent,
                    areaIdx = loc.MemAddress - _hyrule.worlds[contIdx].baseAddr,
                    areaId = contIdx * 0x40 + areaIdx;

                if (areaIdx < 0 || areaIdx >= 0x3e)
                    //// TODO: Figure out how this happens and fix it
                    continue;

                areaSongMap[areaId] = usageSongs[GetSongIdx(loc)];
            }
        }

        return areaSongMap;
    }

    SongMap CreateEnemySongMap(UsesSongs usesSongs)
    {
        // One song per continent
        return usesSongs[Usage.Encounter]
            .SelectMany(song => Enumerable.Repeat(song, 0x30))
            .Select((song, i) => (song, i))
            .ToDictionary(x => x.i, x => (ISong?)x.song);
    }
}
