using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

/// <summary>
/// Source generator definition providing static serialization metadata for Blazor WASM / AOT compatibility.
/// </summary>
[YamlStaticContext]
[YamlSerializable(typeof(RoomPoolSpec))]
[YamlSerializable(typeof(GroupOverride))]
[YamlSerializable(typeof(OverrideCondition))]
[YamlSerializable(typeof(RoomOverride))]
public partial class RoomYamlContext : StaticContext
{
}

public class RoomPoolSpec : IDeterministicHash
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = "Custom Room Pool";

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = "";

    [YamlMember(Alias = "version")]
    public int? Version { get; set; }

    [YamlMember(Alias = "groups_include")]
    public List<string>? GroupsInclude { get; set; }

    [YamlMember(Alias = "groups")]
    public List<GroupOverride>? Groups { get; set; }

    [YamlMember(Alias = "tags_include")]
    public List<string>? TagsInclude { get; set; }

    [YamlMember(Alias = "tags_exclude")]
    public List<string>? TagsExclude { get; set; }

    [YamlMember(Alias = "tags")]
    public List<GroupOverride>? Tags { get; set; }

    [YamlMember(Alias = "rooms")]
    public List<RoomOverride>? Rooms { get; set; }

    [YamlMember(Alias = "rooms_exclude")]
    public List<string>? RoomsExclude { get; set; }

    [YamlIgnore]
    private string? hash = null;
    [YamlIgnore]
    public string Hash
    {
        get
        {
            if (hash == null) { hash = GetDeterministicHash().ToString("X8").ToLower(); }
            return hash;
        }
    }

    public int GetDeterministicHash()
    {
        int hash = DeterministicHash.Seed;
        hash = DeterministicHash.Combine(hash, GroupsInclude);
        hash = DeterministicHash.Combine(hash, Groups);
        hash = DeterministicHash.Combine(hash, TagsInclude);
        hash = DeterministicHash.Combine(hash, TagsExclude);
        hash = DeterministicHash.Combine(hash, Tags);
        hash = DeterministicHash.Combine(hash, Rooms);
        hash = DeterministicHash.Combine(hash, RoomsExclude);
        return hash;
    }
}

public static class RoomPoolSpecDeserializer
{
    public static RoomPoolSpec FromString(string yaml)
    {
        var context = new RoomYamlContext();
        var builder = new StaticDeserializerBuilder(context);
        var deserializer = builder.Build();

        try
        {
            return deserializer.Deserialize<RoomPoolSpec>(yaml);
        }
        catch (YamlException ex)
        {
            var line = ex.Start.Line;
            var column = ex.Start.Column;
            throw new UserFacingException("Custom Room Pool Issue", $"YAML Parsing Error at Line {line}, Column {column}:\n{ex.Message}");
        }
    }
}

public class GroupOverride : IDeterministicHash
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = null!;

    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;

    [YamlMember(Alias = "priority")]
    public int? Priority { get; set; }

    [YamlMember(Alias = "condition")]
    public OverrideCondition? Condition { get; set; }

    public bool ConditionMatches(int palaceNumber, PalaceStyle palaceStyle)
    {
        if (Condition == null) { return true; }
        return Condition.Matches(palaceNumber, palaceStyle);
    }

    public int GetDeterministicHash()
    {
        int hash = DeterministicHash.Seed;
        hash = DeterministicHash.Combine(hash, Name);
        hash = DeterministicHash.Combine(hash, Enabled);
        hash = DeterministicHash.Combine(hash, Priority);
        hash = DeterministicHash.Combine(hash, Condition);
        return hash;
    }
}

public class OverrideCondition : IDeterministicHash
{
    [YamlMember(Alias = "palace")]
    public List<int>? Palace { get; set; }

    [YamlMember(Alias = "not_palace")]
    public List<int>? NotPalace { get; set; }

    [YamlMember(Alias = "style")]
    public List<string>? Style { get; set; }

    [YamlMember(Alias = "not_style")]
    public List<string>? NotStyle { get; set; }

    /// <summary>
    /// Determines whether a palace matches the predefined `condition`.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the palace satisfies all configured filters;
    /// otherwise, <see langword="false"/>. If a filter is not configured, it is ignored.
    /// </returns>
    public bool Matches(int palaceNumber, PalaceStyle palaceStyle)
    {
        bool palaceNumberMatches = true;
        if (Palace is List<int> listMustIncludePalace)
        {
            palaceNumberMatches = listMustIncludePalace.Contains(palaceNumber);
        }
        else if (NotPalace is List<int> listMustNotIncludePalace)
        {
            palaceNumberMatches = !listMustNotIncludePalace.Contains(palaceNumber);
        }

        bool palaceStyleMatches = true;
        if (Style is List<string> listMustIncludeStyle)
        {
            palaceStyleMatches = listMustIncludeStyle.Any(s => String.Equals(palaceStyle.ToString(), s, StringComparison.OrdinalIgnoreCase));
        }
        else if (NotStyle is List<string> listMustNotIncludeStyle)
        {
            palaceStyleMatches = !listMustNotIncludeStyle.Any(s => String.Equals(palaceStyle.ToString(), s, StringComparison.OrdinalIgnoreCase));
        }

        return palaceNumberMatches && palaceStyleMatches;
    }

    public int GetDeterministicHash()
    {
        int hash = DeterministicHash.Seed;
        hash = DeterministicHash.Combine(hash, Palace);
        hash = DeterministicHash.Combine(hash, NotPalace);
        hash = DeterministicHash.Combine(hash, Style);
        hash = DeterministicHash.Combine(hash, NotStyle);
        return hash;
    }
}

public class RoomOverride : IDeterministicHash
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = null!;

    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;

    [YamlMember(Alias = "sideview")]
    public string? Sideview { get; set; }

    [YamlMember(Alias = "enemies")]
    public string? Enemies { get; set; }

    [YamlMember(Alias = "itembits")]
    public int? ItemPresenceBits { get; set; }

    [YamlMember(Alias = "priority")]
    public int? Priority { get; set; }

    public int GetDeterministicHash()
    {
        int hash = DeterministicHash.Seed;
        hash = DeterministicHash.Combine(hash, Name);
        hash = DeterministicHash.Combine(hash, Enabled);
        hash = DeterministicHash.Combine(hash, Sideview);
        hash = DeterministicHash.Combine(hash, Enemies);
        hash = DeterministicHash.Combine(hash, ItemPresenceBits);
        hash = DeterministicHash.Combine(hash, Priority);
        return hash;
    }
}
