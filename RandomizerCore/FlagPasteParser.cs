using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore.Flags;

namespace Z2Randomizer.RandomizerCore;

public static class FlagPasteParser
{
    private static readonly HashSet<char> ValidFlagCharacters = [.. FlagBuilder.ENCODING_TABLE, '$'];

    public static string? ExtractFlags(string input) => Parse(input).Flags;

    public static string? ExtractSeed(string input) => Parse(input).Seed;

    public static bool IsValidFlagString(string flags) => TryNormalizeFlagString(flags, out _);

    // Replace anything that isn't a valid flag string character with a space,
    // tokenize on the resulting whitespace, then pick out the flags and seed.
    public static (string? Flags, string? Seed) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (null, null);
        }

        var cleaned = string.Concat(
            input.Select(character => ValidFlagCharacters.Contains(character) ? character : ' '));
        var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string? flags = null;
        var flagIndex = -1;
        for (var i = 0; i < tokens.Length; i++)
        {
            if (TryNormalizeFlagString(tokens[i], out var normalizedFlags))
            {
                flags = normalizedFlags;
                flagIndex = i;
                break;
            }
        }

        // A seed is only meaningful alongside flags, so don't look for one until
        // the flags have been found.
        if (flags is null)
        {
            return (null, null);
        }

        for (var i = 0; i < tokens.Length; i++)
        {
            if (i != flagIndex && tokens[i].Length >= 6 && tokens[i].All(char.IsDigit))
            {
                return (flags, tokens[i]);
            }
        }

        return (flags, null);
    }

    private static bool TryNormalizeFlagString(string flags, out string normalizedFlags)
    {
        normalizedFlags = string.Empty;
        if (string.IsNullOrWhiteSpace(flags))
        {
            return false;
        }

        var trimmedFlags = flags.Trim();
        if (trimmedFlags.Any(character => !ValidFlagCharacters.Contains(character)))
        {
            return false;
        }

        try
        {
            normalizedFlags = new RandomizerConfiguration(trimmedFlags).SerializeFlags();
            return normalizedFlags == trimmedFlags;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
