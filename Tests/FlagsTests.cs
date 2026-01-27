using System.Reflection;
using CrossPlatformUI.Presets;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Flags;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class FlagsTests
{
    [TestMethod]
    public void TestBoolEncodeCycle()
    {
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(false);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.IsFalse(flagReader.ReadBool());

        flagBuilder.Append(false);
        flagBuilder.Append(true);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.IsFalse(flagReader.ReadBool());
        Assert.IsFalse(flagReader.ReadBool());
        Assert.IsTrue(flagReader.ReadBool());
    }
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MSTEST0037:Use proper 'Assert' methods", Justification = "Does not work with nullable bool")]
    public void TestNullableBoolEncodeCycle()
    {
        bool? nullBool = null;
        FlagBuilder flagBuilder= new FlagBuilder();
        flagBuilder.Append(nullBool);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(nullBool, flagReader.ReadNullableBool());

        flagBuilder.Append((bool?)false);
        flagBuilder.Append((bool?)true);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(nullBool, flagReader.ReadNullableBool());
        Assert.AreEqual((bool?)false, flagReader.ReadNullableBool());
        Assert.AreEqual((bool?)true, flagReader.ReadNullableBool());
    }
    [TestMethod]
    public void TestIntEncodeCycle()
    {
        var limit = 8;
        var minimum = 1;
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(1, limit, minimum);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(1, flagReader.ReadInt(limit, minimum));

        flagBuilder.Append(8, limit, minimum);
        flagBuilder.Append(3, limit, minimum);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(1, flagReader.ReadInt(limit, minimum));
        Assert.AreEqual(8, flagReader.ReadInt(limit, minimum));
        Assert.AreEqual(3, flagReader.ReadInt(limit, minimum));
    }
    [TestMethod]
    public void TestEnumEncodeCycle()
    {
        var index = RandomizerConfiguration.GetEnumIndex<StartingResourceLimit>(StartingResourceLimit.NO_LIMIT);
        var count = RandomizerConfiguration.GetEnumCount<StartingResourceLimit>();
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(index, count);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(StartingResourceLimit.NO_LIMIT, flagReader.ReadEnum<StartingResourceLimit>());

        index = RandomizerConfiguration.GetEnumIndex<StartingResourceLimit>(StartingResourceLimit.FOUR);
        flagBuilder.Append(index, count);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(StartingResourceLimit.NO_LIMIT, flagReader.ReadEnum<StartingResourceLimit>());
        Assert.AreEqual(StartingResourceLimit.FOUR, flagReader.ReadEnum<StartingResourceLimit>());
    }
    [TestMethod]
    public void TestCustomEncodeCycle()
    {
        var climate = Climates.Chaos;
        var serializer = RandomizerConfiguration.GetSerializer<ClimateFlagSerializer>();
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(serializer.Serialize(climate), serializer.GetLimit());
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(climate, serializer.Deserialize(flagReader.ReadInt(serializer.GetLimit())));
    }
    [TestMethod]
    public void TestNullableIntEncodeCycle()
    {
        int? nullInt = null;
        var limit = 8;
        var minimum = 1;
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(nullInt, limit, minimum);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(nullInt, flagReader.ReadNullableInt(limit, minimum));

        flagBuilder.Append((int?)8, limit, minimum);
        flagBuilder.Append((int?)3,  limit, minimum);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(nullInt, flagReader.ReadNullableInt(limit, minimum));
        Assert.AreEqual((int?)8, flagReader.ReadNullableInt(limit, minimum));
        Assert.AreEqual((int?)3, flagReader.ReadNullableInt(limit, minimum));
    }
    [TestMethod]
    public void TestBlankEncodeCycle()
    {
        RandomizerConfiguration config = new();
        RandomizerConfiguration config2 = new(config.SerializeFlags());
        var failures = new List<string>();
        foreach(PropertyInfo property in typeof(RandomizerConfiguration).GetProperties())
        {
            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            var v1 = property.GetValue(config) == null ? "<null>" : property.GetValue(config)!.ToString();
            var v2 = property.GetValue(config2) == null ? "<null>" : property.GetValue(config2)!.ToString();
            if (v1 != v2)
                failures.Add($"{property.Name} did not match. Config: {v1} Config2: {v2}");
        }
        Assert.IsEmpty(failures,
            $"The following assertions failed: {Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    [TestMethod]
    public void TestStandardFlagsEncodeCycle()
    {
        RandomizerConfiguration config = StandardPreset.Preset;
        RandomizerConfiguration config2 = new RandomizerConfiguration(StandardPreset.Preset.SerializeFlags());
        Assert.AreEqual(config.SerializeFlags(), config2.SerializeFlags());
    }

    [TestMethod]
    public void TestMaxRandoEncodeCycle()
    {
        RandomizerConfiguration config = MaxRandoPreset.Preset;
        RandomizerConfiguration config2 = new RandomizerConfiguration(MaxRandoPreset.Preset.SerializeFlags());
        Assert.AreEqual(config.SerializeFlags(), config2.SerializeFlags());
    }
}