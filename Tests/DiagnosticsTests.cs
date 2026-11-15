using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Diagnostics;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
[DoNotParallelize] // These tests capture the process stderr sink.
public class DiagnosticsTests
{
#if Z2R_DIAGNOSTICS
    private static readonly bool Enabled = true;
#else
    private static readonly bool Enabled = false;
#endif

    [TestMethod]
    public void ConditionalCall_RemovesArgumentEvaluationWhenDisabled()
    {
        int evaluations = 0;
        string Argument() { evaluations++; return "event=probe"; }
        using var output = new StringWriter();
        TextWriter original = Console.Error;
        try
        {
            Console.SetError(output);
            Diag.Trace("test", Argument());
        }
        finally { Console.SetError(original); }
        evaluations.Should().Be(Enabled ? 1 : 0);
        output.ToString().Should().Be(Enabled ? $"[Z2R:test] event=probe{Environment.NewLine}" : "");
    }

    [TestMethod]
    public void Block_PrefixesEveryLine_AndTraceEscapesEmbeddedNewlines()
    {
        using var output = new StringWriter();
        TextWriter original = Console.Error;
        try
        {
            Console.SetError(output);
            Diag.Block("test", "first\r\nsecond");
            Diag.Trace("test", "third\nfourth");
        }
        finally { Console.SetError(original); }
        output.ToString().Should().Be(Enabled
            ? $"[Z2R:test] first{Environment.NewLine}[Z2R:test] second{Environment.NewLine}[Z2R:test] third\\nfourth{Environment.NewLine}"
            : "");
    }

    [TestMethod]
    public void Sampling_ReportsEarlyFailuresAndContinuesDuringLongRuns()
    {
        Enumerable.Range(1, 2001).Where(n => Diag.IsSample(n)).Should().Equal(
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1000, 1024, 2000);
        Diag.IsSample(0).Should().BeFalse();
        Diag.IsSample(-1).Should().BeFalse();
    }

    [TestMethod]
    public void BlockedPalace_ReportsFailureContext_AndPreservesTraversalResults()
    {
        Palace palace = new(3, true);
        Room entrance = new() { IsEntrance = true, Map = 1, LinkRequirements = Requirements.NONE };
        Room item = new() { Map = 2, Collectable = Collectable.RAFT, LinkRequirements = new Requirements([RequirementType.GLOVE]) };
        entrance.Right = item;
        item.Left = entrance;
        palace.Entrance = entrance;
        palace.AllRooms.AddRange([entrance, item]);
        palace.ItemRooms.Add(item);
        var props = new RandomizerProperties { KeyLogic = KeyLogicOption.FAIRY_OR_MAGICKEY };
        using var output = new StringWriter();
        TextWriter original = Console.Error;
        try
        {
            Console.SetError(output);
            palace.CanReachAnItemRoom(new HashSet<RequirementType>()).Should().BeFalse();
            palace.GetGettableItems(props, new HashSet<RequirementType>(), false).Should().BeEmpty();
            palace.GetGettableItems(props, new HashSet<RequirementType> { RequirementType.GLOVE }, false)
                .Should().Equal(Collectable.RAFT);
        }
        finally { Console.SetError(original); }
        if (Enabled)
        {
            output.ToString().Should().Contain("palace=3 failed check=item-room covered=2/2 states=2 itemRooms=[2]");
            output.ToString().Should().Contain("event=end palace=3 call=1 dequeues=2");
            output.ToString().Should().Contain("itemRooms=[2:RAFT:False]");
            output.ToString().Should().Contain("items=[RAFT]");
        }
        else output.ToString().Should().BeEmpty();
    }

    [TestMethod]
    public void OversizedTerrain_ReportsSampledRejectionsAndLeavesRomUntouched()
    {
        var world = new WorldCharacterizationTests.TestWorld(1, 64, Terrain.GRASS, mapRomCapacity: 3);
        var encoded = world.EncodeMap();
        var rom = new ROM(new byte[16]);
        using var candidate = world.BeginTerrainCandidate(rom);
        using var output = new StringWriter();
        TextWriter original = Console.Error;
        try
        {
            Console.SetError(output);
            for (int i = 0; i < 5; i++)
            {
                world.StageEncodedMap(encoded).Should().BeFalse();
            }
            candidate.Commit();
        }
        finally { Console.SetError(original); }

        rom.rawdata.Should().OnlyContain(value => value == 0);
        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (Enabled)
        {
            lines.Should().HaveCount(3);
            foreach (var (line, count) in lines.Zip(new[] { 1, 2, 4 }))
            {
                line.Should().StartWith("[Z2R:terrain.reject] world=");
                line.Should().EndWith($"reason=map-too-large count={count} bytes=4 capacity=3");
            }
        }
        else lines.Should().BeEmpty();
    }

    [TestMethod]
    public void InstrumentationStorage_OnlyExistsInTraceBuilds()
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        typeof(Random).GetProperty("DrawCount", instance).Should().Match<PropertyInfo?>(p => (p != null) == Enabled);
        typeof(Palace).GetField("itemBfsCalls", instance).Should().Match<FieldInfo?>(f => (f != null) == Enabled);
        typeof(ReachabilitySimulation).GetField("consecutiveReachabilityFailures", instance).Should().Match<FieldInfo?>(f => (f != null) == Enabled);
        typeof(Diag).GetNestedType("Generation", BindingFlags.NonPublic).Should().Match<Type?>(t => (t != null) == Enabled);
    }

    [TestMethod]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Tests inspect the complete, untrimmed generator assembly.")]
    public void GeneratorIL_ContainsNoDiagnosticCallsWhenDisabled()
    {
        // Inspect actual compiled call operands, including compiler-generated async methods
        // and lambdas; searching source or raw IL bytes would miss or misidentify calls.
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
            .ToDictionary(op => unchecked((ushort)op.Value));
        var calls = new List<string>();
        foreach (Type type in typeof(Hyrule).Assembly.GetTypes())
        {
            if (type.Namespace == typeof(Diag).Namespace) continue;
            const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            foreach (MethodBase method in type.GetMethods(declared).Cast<MethodBase>().Concat(type.GetConstructors(declared)))
            {
                byte[]? il = method.GetMethodBody()?.GetILAsByteArray();
                if (il == null) continue;
                int offset = 0;
                while (offset < il.Length)
                {
                    ushort code = il[offset++];
                    if (code == 0xfe) code = (ushort)(0xfe00 | il[offset++]);
                    OpCode op = opcodes[code];
                    if (op.OperandType == OperandType.InlineMethod)
                    {
                        MethodBase? called = method.Module.ResolveMethod(BitConverter.ToInt32(il, offset),
                            type.IsGenericType ? type.GetGenericArguments() : null,
                            method.IsGenericMethod ? method.GetGenericArguments() : null);
                        if (called?.DeclaringType?.Namespace == typeof(Diag).Namespace
                            || called?.GetCustomAttributes<ConditionalAttribute>().Any(a => a.ConditionString == "Z2R_DIAGNOSTICS") == true)
                            calls.Add($"{type.FullName}.{method.Name} -> {called!.Name}");
                    }
                    offset += op.OperandType switch
                    {
                        OperandType.InlineNone => 0,
                        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                        OperandType.InlineVar => 2,
                        OperandType.InlineI8 or OperandType.InlineR => 8,
                        OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, offset),
                        _ => 4
                    };
                }
            }
        }
        if (Enabled) calls.Should().NotBeEmpty();
        else calls.Should().BeEmpty("ordinary builds must contain no generator calls into the diagnostics facility");
    }

#if Z2R_DIAGNOSTICS
    [TestMethod]
    public void RngReceipts_CountCoreDraws_WithoutChangingRngState()
    {
        var main = new Random(4242);
        var difficulty = new Random(5019);
        string initial = main.GetState();
        using var output = new StringWriter();
        TextWriter original = Console.Error;
        try
        {
            Console.SetError(output);
            using var receipt = new Diag.Generation(main, difficulty);
            receipt.Stage("config");
            main.GetState().Should().Be(initial);
            main.NextBytes(new byte[17]); // Three 64-bit draws, including the partial chunk.
            main.DrawCount.Should().Be(3);
            difficulty.NextDouble();
            string afterDraws = main.GetState();
            receipt.Stage("link");
            main.GetState().Should().Be(afterDraws);
            main.SetState(initial);
            main.DrawCount.Should().Be(3, "restoring RNG state must not rewind the cumulative receipt");
        }
        finally { Console.SetError(original); }
        output.ToString().Should().Contain("stage=config event=end mainDraws=3 difficultyDraws=1");
        output.ToString().Should().Contain("stage=link event=end mainDraws=0 difficultyDraws=0");
    }
#endif
}
