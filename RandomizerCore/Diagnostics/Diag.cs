using System;
using System.Diagnostics;
using System.Globalization;

namespace Z2Randomizer.RandomizerCore.Diagnostics;

/// <summary>
/// Compile-time generator tracing. Build the CLI with <c>-p:Z2RTrace=true</c>.
/// Events go to stderr, for example <c>[Z2R:timing] stage=overworld event=begin</c>.
/// See Diagnostics/README.md for the stuck-seed workflow and tag reference.
/// </summary>
/// <remarks>
/// Conditional calls and their arguments compile out unless <c>Z2R_DIAGNOSTICS</c>
/// is defined. Guard diagnostic-only counters and scopes with the same symbol.
/// </remarks>
internal static class Diag
{
    /// <summary>Emits one tagged event line.</summary>
    [Conditional("Z2R_DIAGNOSTICS")]
    public static void Trace(string tag, string message)
    {
        Console.Error.WriteLine($"[Z2R:{tag}] {message.Replace("\r", "\\r").Replace("\n", "\\n")}");
    }

    /// <summary>
    /// Emits a block, prefixing every line so multi-line snapshots stay greppable
    /// and attributable to their tag.
    /// </summary>
    [Conditional("Z2R_DIAGNOSTICS")]
    public static void Block(string tag, string message)
    {
        foreach (string line in message.Split('\n'))
        {
            Trace(tag, line.TrimEnd('\r'));
        }
    }

    /// <summary>First two, powers of two, then every thousand attempts.</summary>
    internal static bool IsSample(long attempt) => attempt > 0
        && (attempt <= 2 || (attempt & (attempt - 1)) == 0 || attempt % 1000 == 0);

    [Conditional("Z2R_DIAGNOSTICS")]
    public static void Retry(string tag, long attempt, string context)
    {
        if (IsSample(attempt))
        {
            Trace(tag, $"attempt={attempt} {context}");
        }
    }

#if Z2R_DIAGNOSTICS
    /// <summary>Per-generation receipts; never instantiated in ordinary builds.</summary>
    internal sealed class Generation(Random main, Random? difficulty) : IDisposable
    {
        private string stage = "";
        private long started;
        private ulong mainDraws, difficultyDraws;

        public void Stage(string next)
        {
            Finish();
            stage = next;
            started = Stopwatch.GetTimestamp();
            mainDraws = main.DrawCount;
            difficultyDraws = difficulty?.DrawCount ?? 0;
            Trace("timing", $"stage={stage} event=begin");
            Trace("rng", $"stage={stage} event=begin main={main.GetState()} difficulty={difficulty?.GetState()}");
        }

        private void Finish()
        {
            if (stage.Length == 0) return;
            Trace("timing", $"stage={stage} event=end elapsedMs={Stopwatch.GetElapsedTime(started).TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)}");
            Trace("rng", $"stage={stage} event=end mainDraws={main.DrawCount - mainDraws} "
                + $"difficultyDraws={(difficulty?.DrawCount ?? 0) - difficultyDraws} "
                + $"main={main.GetState()} difficulty={difficulty?.GetState()}");
        }

        // An end receipt means the stage exited; generation's result event says whether it succeeded.
        public void Dispose() => Finish();
    }
#endif
}
