#nullable enable

using FtRandoLib.Importer;
using FtRandoLib.Library;
using FtRandoLib.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

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
    Hyrule _hyrule;
    IShuffler _shuffler;
    byte[] _rom;
    SimpleRomAccess _romAccess;
    List<string> _jsonLibPaths;
    List<string> _yamlLibPaths;
    List<int> _freeBanks;
    bool _includeBuiltin;
    bool _includeDiverse;

    Z2Importer _imptr;

    bool _done = false;
    HashSet<int> _resFreeBanks = new();
    StringBuilder _spoilerSb = new();

    public IEnumerable<int> FreeBanks => _resFreeBanks;
    public string SpoilerLog => _spoilerSb.ToString();

    public MusicRandomizer(
        Hyrule hyrule,
        int seed,
        IEnumerable<string> jsonLibPaths,
        IEnumerable<string> yamlLibPaths,
        IEnumerable<int> freeBanks,
        bool includeBuiltin,
        bool includeDiverse,
        bool safeOnly)
    {
        _hyrule = hyrule;
        _shuffler = new RandomShuffler(new Random(seed));
        _rom = _hyrule.ROMData.GetBytes(0, ROM.RomSize);
        _romAccess = new(_rom);
        _jsonLibPaths = new(jsonLibPaths);
        _yamlLibPaths = new(yamlLibPaths);
        _freeBanks = new(freeBanks);
        _includeBuiltin = includeBuiltin;
        _includeDiverse = includeDiverse;

        _imptr = new(_romAccess, _freeBanks);
        _imptr.DefaultParserOptions.SafeOnly = safeOnly;
    }

    public void ImportSongs()
    {
        if (_done)
            throw new InvalidOperationException("Attempt to import custom music twice");

        _spoilerSb.Clear();

        if (_jsonLibPaths.Count == 0 && _yamlLibPaths.Count == 0)
            return; // Nothing to do

        // Testing songs is to help in making libraries. As libraries will be something that the average user can make, it can't be limited to debug builds like in MM2R. However, if people end up using libraries so large that this ends up taking a lot of time, it may be necessary to remove it. It's also entirely possible that the optimizer will notice that this function doesn't actually do anything that affects the rest of the program and eliminate it completely...
        TestSongs();

        var songs = LoadSongs(_includeBuiltin, _includeDiverse);
        var usesSongs = _imptr.SplitSongsByUsage<Usage>(songs);

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
        var selUsesSongs = _imptr.SelectUsesSongs<Usage>(
            usesSongs, numUsageSongs, _shuffler);

        WriteLogLine(null);
        WriteLogLine("SELECTED SONGS:");
        foreach (var (usage, usageSongs) in selUsesSongs)
        {
            string songNames = string.Join("}, {", usageSongs);
            WriteLogLine($"\t{usage}: {{{songNames}}}");
        }

        WriteLogLine(null);

        var songMap = CreateSongMap(selUsesSongs);
        var areaSongMap = CreateAreaSongMap(selUsesSongs);
        var enemySongMap = CreateEnemySongMap(selUsesSongs);

        IstringDictionary<IReadOnlyDictionary<int, ISong?>> songMaps = new()
        {
            { Z2Importer.AreaSongMapName, areaSongMap },
            { Z2Importer.EnemySongMapName, enemySongMap },
        };

        _imptr.Import(
            songMap,
            songMaps,
            out _resFreeBanks);

        _hyrule.ROMData.Put(0, _rom);

        _done = true;
    }

    void WriteLogLine(string? line)
    {
        Trace.WriteLine(line);
        _spoilerSb.AppendLine(line);
    }

    List<ISong> LoadSongs(
        bool includeBuiltin,
        bool includeDiverse,
        LibraryParserOptions? opts = null)
    {
        List<FtSong> ftSongs = new();
        LoadLibrariesSongs(
            _jsonLibPaths, 
            d => _imptr.LoadFtJsonLibrarySongs(d, opts), 
            includeDiverse,
            ftSongs);
        LoadLibrariesSongs(
            _yamlLibPaths,
            d => _imptr.LoadFtYamlLibrarySongs(d, opts),
            includeDiverse,
            ftSongs);

        List<ISong> songs = new();
        if (includeBuiltin)
            songs.AddRange(_imptr.CreateBuiltinSongs());

        songs.AddRange(ftSongs);

        return songs;
    }

    void LoadLibrariesSongs(
        IEnumerable<string> libPaths, 
        Func<string, IEnumerable<FtSong>> loader,
        bool includeDiverse,
        List<FtSong> songs)
    {
        foreach (var libPath in libPaths)
        {
            string libData = File.ReadAllText(libPath, Encoding.UTF8);

            try
            {
                songs.AddRange(loader(libData).Where(song => includeDiverse || !song.Tags.Contains("diverse")));
            }
            catch (ParsingError ex)
            {
                StringBuilder sb = new();
                sb.AppendLine($"Error loading custom music from '{libPath}'");

                string? atStr = ex.AtString;
                if (atStr is not null)
                    sb.AppendLine(atStr);

                sb.AppendLine();

                sb.AppendLine(ex.Message);
                if (ex.Submessage is not null)
                    sb.AppendLine(ex.Submessage);

                throw new Exception(sb.ToString(), ex);
            }
        }
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
                if (object.ReferenceEquals(loc, _hyrule.westHyrule!.bagu))
                    usage = Usage.Town;
                else if (loc.TerrainType == Terrain.TOWN)
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
        foreach (var (usage, usageLocs) in usesLocs)
        {
            var usageSongs = usesSongs[usage];
            if (usageSongs.Count == 0)
                continue;

            Func<Location, int> GetSongIdx = usage switch
            {
                // One song per palace
                Usage.Palace => (loc => (int)loc.PalaceNumber! - 1),
                Usage.GreatPalace => (loc => 0),
                // One song per continent
                _ => (loc => (int)(loc.VanillaContinent ?? loc.Continent)),
            };

            foreach (var loc in usageLocs)
            {
                int contIdx = (int)(loc.VanillaContinent ?? loc.Continent),
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

    void TestSongs()
    {
        LibraryParserOptions opts = new() { EnabledOnly = false, SafeOnly = false };

        var songs = LoadSongs(false, true, opts);
        _imptr.TestRebase(songs);
    }
}
