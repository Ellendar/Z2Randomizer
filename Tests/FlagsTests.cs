using System.Reflection;
using CrossPlatformUI.Presets;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Flags;

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
        var extent = 8; // 3 bits
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(1, extent);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(1, flagReader.ReadInt(extent));

        flagBuilder.Append(7, extent);
        flagBuilder.Append(3, extent);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(1, flagReader.ReadInt(extent));
        Assert.AreEqual(7, flagReader.ReadInt(extent));
        Assert.AreEqual(3, flagReader.ReadInt(extent));
    }

    [TestMethod]
    public void TestEnumEncodeCycle()
    {
        var index = RandomizerConfiguration.GetEnumIndex(StartingResourceLimit.NO_LIMIT);
        var count = RandomizerConfiguration.GetEnumCount<StartingResourceLimit>();
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(index, count);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(StartingResourceLimit.NO_LIMIT, RandomizerConfiguration.DeserializeEnum<StartingResourceLimit>(flagReader, "Test1"));

        index = RandomizerConfiguration.GetEnumIndex(StartingResourceLimit.FOUR);
        flagBuilder.Append(index, count);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(StartingResourceLimit.NO_LIMIT, RandomizerConfiguration.DeserializeEnum<StartingResourceLimit>(flagReader, "Test2"));
        Assert.AreEqual(StartingResourceLimit.FOUR, RandomizerConfiguration.DeserializeEnum<StartingResourceLimit>(flagReader, "Test3"));
    }

    /* We no longer have any custom flag encodes
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
    }*/

    /* Nullable int flags are no longer allowed (use Enums).
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
    */

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
        RandomizerConfiguration config = MaxRando2025Preset.Preset;
        RandomizerConfiguration config2 = new RandomizerConfiguration(MaxRando2025Preset.Preset.SerializeFlags());
        Assert.AreEqual(config.SerializeFlags(), config2.SerializeFlags());
    }

    [TestMethod]
    public void DifficultyOnlyFlagsDoNotChangeSharedSeedFlags()
    {
        RandomizerConfiguration baseConfig = new()
        {
            ShareSeedAcrossDifficulty = true
        };
        RandomizerConfiguration difficultyConfig = new()
        {
            ShareSeedAcrossDifficulty = true,
            StartWithCandle = true,
            StartWithCross = true,
            StartingTechniques = StartingTechs.BOTH,
            StartingLives = StartingLives.Lives1,
            AttackLevelCap = 4,
            MagicLevelCap = 6,
            LifeLevelCap = 7,
            ScaleLevelRequirementsToCap = true,
            AttackEffectiveness = AttackEffectiveness.OHKO,
            LifeEffectiveness = LifeEffectiveness.INVINCIBLE,
            ShuffleEnemyHP = EnemyLifeOption.WIDE,
            ShuffleBossHP = EnemyLifeOption.MEDIUM_HIGH,
            EnemyXPDrops = XPEffectiveness.NONE
        };

        Assert.AreNotEqual(baseConfig.SerializeFlags(), difficultyConfig.SerializeFlags());
        Assert.AreEqual(baseConfig.SerializeSharedSeedFlags(), difficultyConfig.SerializeSharedSeedFlags());
    }

    [TestMethod]
    public void SharedSeedExportIgnoresDifficultyOnlySettings()
    {
        RandomizerConfiguration config = new()
        {
            ShareSeedAcrossDifficulty = true,
            StartWithCandle = true,
            StartWithCross = true,
            StartingTechniques = StartingTechs.BOTH,
            StartingLives = StartingLives.Lives1,
            AttackLevelCap = 4,
            MagicLevelCap = 6,
            LifeLevelCap = 7,
            ScaleLevelRequirementsToCap = true,
            AttackEffectiveness = AttackEffectiveness.OHKO,
            LifeEffectiveness = LifeEffectiveness.INVINCIBLE,
            ShuffleEnemyHP = EnemyLifeOption.WIDE,
            ShuffleBossHP = EnemyLifeOption.MEDIUM_HIGH,
            EnemyXPDrops = XPEffectiveness.NONE
        };

        RandomizerProperties properties = config.Export(new Random(1234), includeDifficulty: false);

        Assert.IsFalse(properties.StartCandle);
        Assert.IsFalse(properties.StartCross);
        Assert.IsFalse(properties.StartWithDownstab);
        Assert.IsFalse(properties.StartWithUpstab);
        Assert.AreEqual(3, properties.StartLives);
        Assert.AreEqual(8, properties.AttackCap);
        Assert.AreEqual(8, properties.MagicCap);
        Assert.AreEqual(8, properties.LifeCap);
        Assert.IsFalse(properties.ScaleLevels);
        Assert.AreEqual(AttackEffectiveness.VANILLA, properties.AttackEffectiveness);
        Assert.AreEqual(LifeEffectiveness.VANILLA, properties.LifeEffectiveness);
        Assert.AreEqual(EnemyLifeOption.VANILLA, properties.ShuffleEnemyHP);
        Assert.AreEqual(EnemyLifeOption.VANILLA, properties.ShuffleBossHP);
        Assert.AreEqual(XPEffectiveness.VANILLA, properties.EnemyXPDrops);
    }

    [TestMethod]
    public void ShareSeedAcrossDifficultyRoundTripsInFlags()
    {
        RandomizerConfiguration config = new()
        {
            ShareSeedAcrossDifficulty = true
        };

        RandomizerConfiguration config2 = new(config.SerializeFlags());

        Assert.IsTrue(config2.ShareSeedAcrossDifficulty);
    }
}
