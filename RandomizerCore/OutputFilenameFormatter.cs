using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Z2Randomizer.RandomizerCore;

public static class OutputFilenameFormatter
{
    public const string DefaultTemplate = "Z2-%f-%s-%h.nes";

    // The characters Windows forbids in a filename. Linux and macOS only
    // disallow '/' (and NUL), so honoring the Windows set produces a name that
    // is valid on all three platforms.
    private static readonly char[] InvalidFileNameChars =
        ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    public static string Format(string? template, string flags, string? seed, string hash, DateTime? timestamp = null, string? version = null)
    {
        var resolvedTemplate = string.IsNullOrWhiteSpace(template) ? DefaultTemplate : template;
        var resolvedTimestamp = (timestamp ?? DateTime.Now).ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture);
        var normalizedHash = string.Concat((hash ?? "").Where(c => !char.IsWhiteSpace(c)));

        var formatted = resolvedTemplate
            .Replace("%d", resolvedTimestamp, StringComparison.Ordinal)
            .Replace("%f", flags, StringComparison.Ordinal)
            .Replace("%s", seed ?? "", StringComparison.Ordinal)
            .Replace("%h", normalizedHash, StringComparison.Ordinal)
            .Replace("%v", version ?? "", StringComparison.Ordinal);

        return SanitizeFileName(formatted);
    }

    // Replaces any character that wouldn't be a valid filename on Windows,
    // Linux, or macOS so a user-supplied template can never produce a name the
    // OS rejects.
    public static string SanitizeFileName(string fileName)
    {
        var builder = new StringBuilder(fileName.Length);
        foreach (var character in fileName)
        {
            var invalid = character < ' ' || Array.IndexOf(InvalidFileNameChars, character) >= 0;
            builder.Append(invalid ? '_' : character);
        }

        // Windows rejects names that end in a space or a dot.
        var sanitized = builder.ToString().TrimEnd(' ', '.');
        if (sanitized.Length == 0)
        {
            return "_";
        }

        var dot = sanitized.IndexOf('.');
        var baseName = dot >= 0 ? sanitized[..dot] : sanitized;

        return sanitized;
    }
}
