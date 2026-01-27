namespace Z2Randomizer;

public static partial class GitInfo
{
    public static string Commit { get; } = "n/a";
    public static string Branch { get; } = "unknown";
    public static string Tag { get; } = "";
    public static bool IsDirty { get; }
}
