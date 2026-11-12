using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CrossPlatformUI;
using CrossPlatformUI.ViewModels;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class SettingsSerializationTests
{
    [TestMethod]
    public void EverySavedEnumHasATolerantConverter()
    {
        // Inspect the generated JSON contract so a new persisted enum cannot silently use the strict converter.
        var enumTypes = typeof(SerializationContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.PropertyType)
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonTypeInfo<>))
            .Select(type => type.GetGenericArguments()[0])
            .Where(type => type.IsEnum)
            .ToList();
        Assert.IsNotEmpty(enumTypes);
        var options = SerializationContext.SettingsReadTypeInfo.Options;
        var missing = enumTypes.Where(enumType =>
            !options.Converters.Any(converter => converter.CanConvert(enumType)));
        Assert.AreEqual("", string.Join(", ", missing), "Enum types without a tolerant reader");

        foreach (var enumType in enumTypes)
        {
            var typeInfo = options.GetTypeInfo(enumType);
            var result = JsonSerializer.Deserialize("\"UnrecognizedFutureEnumMember\"", typeInfo);
            Assert.IsTrue(Enum.IsDefined(enumType, result!), enumType.Name);
        }
    }

    [TestMethod]
    public void SettingsReadLegacyValuesAndRecoverFromMalformedEnumAndBoolTokens()
    {
        const string json = """
            {
              "Config": {
                "AttackEffectiveness": "hIgH",
                "EnemyXPDrops": "UnrecognizedFutureEnumMember",
                "StartingLives": 2,
                "MaxHeartContainers": "999",
                "WestSize": {"Future": [1]},
                "ShuffleAttackExperience": "TRUE",
                "ShuffleMagicExperience": 1,
                "ShuffleLifeExperience": [false],
                "ShuffleOverworldEnemies": null
              },
              "RomFileViewModel": {"RomData": "AQID"}
            }
            """;

        var settings = JsonSerializer.Deserialize<SettingsEnvelope>(json, CreateTestOptions(SerializationContext.SettingsReadTypeInfo.Options))!;
        var config = settings.Config;

        Assert.AreEqual(AttackEffectiveness.HIGH, config.AttackEffectiveness);
        Assert.AreEqual(default(XPEffectiveness), config.EnemyXPDrops);
        Assert.AreEqual(StartingLives.Lives3, config.StartingLives);
        Assert.AreEqual(MaxHeartsOption.EIGHT, config.MaxHeartContainers);
        Assert.AreEqual(OverworldSizeOption.LARGE, config.WestSize);
        Assert.IsTrue(config.ShuffleAttackExperience);
        Assert.IsTrue(config.ShuffleMagicExperience);
        Assert.IsFalse(config.ShuffleLifeExperience);
        Assert.IsNull(config.ShuffleOverworldEnemies);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, settings.RomFileViewModel.RomData);
    }

    [TestMethod]
    public void EnumDictionaryKeysAcceptLegacyAndUnknownNames()
    {
        var options = SerializationContext.SettingsReadTypeInfo.Options;
        var converter = (JsonConverter<Biome>)options.GetTypeInfo(typeof(Biome)).Converter;
        var reader = new Utf8JsonReader("""{"islands":7,"UnrecognizedFutureBiome":8}"""u8);
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Read());
        Assert.AreEqual(Biome.ISLANDS, converter.ReadAsPropertyName(ref reader, typeof(Biome), options));
        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.Read());
        Assert.AreEqual(default(Biome), converter.ReadAsPropertyName(ref reader, typeof(Biome), options));
    }

    [TestMethod]
    public void BrokenPresetsPreserveSettingsOtherPresetsAndTheirOwnRawJson()
    {
        const string json = """
            {
              "Config": {"AttackEffectiveness": "LOW"},
              "RomFileViewModel": {"RomData": "AQID"},
              "SaveNewPresetViewModel": {
                "SavedPresets": [
                  {"Preset": "Healthy", "Config": {"AttackEffectiveness": "HIGH", "FutureFlag": ["value"]}},
                  {"Preset": "Broken config", "Config": {"AttackEffectiveness": "HIGH", "Seed": []}},
                  {"Preset": "Wrong shape", "Config": [1, 2]},
                  {"Preset": 42, "Config": {}},
                  null,
                  17,
                  {"Preset": "Legacy", "Config": {"AttackEffectiveness": 6, "MaxHeartContainers": "Future", "ShuffleAttackExperience": "true"}}
                ]
              }
            }
            """;

        var settings = JsonSerializer.Deserialize<SettingsEnvelope>(json, CreateTestOptions(SerializationContext.SettingsReadTypeInfo.Options))!;
        var presets = settings.SaveNewPresetViewModel.SavedPresets;

        Assert.HasCount(7, presets);
        Assert.AreEqual(AttackEffectiveness.LOW, settings.Config.AttackEffectiveness);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, settings.RomFileViewModel.RomData);
        Assert.AreEqual(AttackEffectiveness.HIGH, presets[0].Config.AttackEffectiveness);
        Assert.AreEqual(AttackEffectiveness.VANILLA, presets[1].Config.AttackEffectiveness);
        Assert.AreEqual(AttackEffectiveness.VANILLA, presets[2].Config.AttackEffectiveness);
        Assert.AreEqual(AttackEffectiveness.HIGH, presets[6].Config.AttackEffectiveness);
        Assert.AreEqual(MaxHeartsOption.EIGHT, presets[6].Config.MaxHeartContainers);
        Assert.IsTrue(presets[6].Config.ShuffleAttackExperience);

        var saved = JsonSerializer.SerializeToElement(settings, CreateTestOptions(SerializationContext.SettingsWriter.Options));
        using var original = JsonDocument.Parse(json);
        var originalPresets = original.RootElement.GetProperty("SaveNewPresetViewModel").GetProperty("SavedPresets");
        var savedPresets = saved.GetProperty("SaveNewPresetViewModel").GetProperty("SavedPresets");
        Assert.IsTrue(JsonElement.DeepEquals(originalPresets, savedPresets));
        Assert.AreEqual("LOW", saved.GetProperty("Config").GetProperty("AttackEffectiveness").GetString());
    }

    [TestMethod]
    [DataRow("\"notanumber\"")]
    [DataRow("2147483648")]
    [DataRow("3.5")]
    [DataRow("{\"invalid\":[1]}")]
    [DataRow("null")]
    public void IllegalValuesKeepValidSettingsAndPresetsAcrossSaveAndReload(string invalidNumber)
    {
        var json = $$$"""
            {
              "Config": {
                "NormalPalaceStyle": "DOES_NOT_EXIST",
                "PalacesToCompleteMin": {{{invalidNumber}}},
                "StartingAttackLevel": 3,
                "StartingHeartContainersMin": "notanumber",
                "StartingMagicContainersMin": null,
                "AttackEffectiveness": "HIGH"
              },
              "RomFileViewModel": {"RomData": "AQID"},
              "SaveNewPresetViewModel": {
                "SavedPresets": [
                  {"Preset": "Mixed", "Config": {
                    "NormalPalaceStyle": "DOES_NOT_EXIST",
                    "PalacesToCompleteMin": {{{invalidNumber}}},
                    "StartingAttackLevel": 3,
                    "AttackEffectiveness": "LOW"
                  }},
                  {"Preset": "Healthy", "Config": {"NormalPalaceStyle": "SHUFFLED", "PalacesToCompleteMin": 5}}
                ]
              }
            }
            """;

        var readOptions = CreateTestOptions(SerializationContext.SettingsReadTypeInfo.Options);
        var settings = JsonSerializer.Deserialize<SettingsEnvelope>(json, readOptions)!;
        Check(settings);

        var saved = JsonSerializer.SerializeToElement(settings, CreateTestOptions(SerializationContext.SettingsWriter.Options));
        using var original = JsonDocument.Parse(json);
        Assert.IsTrue(JsonElement.DeepEquals(
            original.RootElement.GetProperty("SaveNewPresetViewModel"),
            saved.GetProperty("SaveNewPresetViewModel")));
        Check(saved.Deserialize<SettingsEnvelope>(readOptions)!);

        static void Check(SettingsEnvelope settings)
        {
            var defaults = new RandomizerConfiguration();
            var config = settings.Config;
            Assert.AreEqual(PalaceStyle.VANILLA, config.NormalPalaceStyle);
            Assert.AreEqual(defaults.PalacesToCompleteMin, config.PalacesToCompleteMin);
            Assert.AreEqual(3, config.StartingAttackLevel);
            Assert.AreEqual(defaults.StartingHeartContainersMin, config.StartingHeartContainersMin);
            Assert.IsNull(config.StartingMagicContainersMin);
            Assert.AreEqual(AttackEffectiveness.HIGH, config.AttackEffectiveness);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, settings.RomFileViewModel.RomData);

            var presets = settings.SaveNewPresetViewModel.SavedPresets;
            Assert.HasCount(2, presets);
            Assert.AreEqual("Mixed", presets[0].Preset);
            Assert.AreEqual(PalaceStyle.VANILLA, presets[0].Config.NormalPalaceStyle);
            Assert.AreEqual(defaults.PalacesToCompleteMin, presets[0].Config.PalacesToCompleteMin);
            Assert.AreEqual(3, presets[0].Config.StartingAttackLevel);
            Assert.AreEqual(AttackEffectiveness.LOW, presets[0].Config.AttackEffectiveness);
            Assert.AreEqual("Healthy", presets[1].Preset);
            Assert.AreEqual(PalaceStyle.SHUFFLED, presets[1].Config.NormalPalaceStyle);
            Assert.AreEqual(5, presets[1].Config.PalacesToCompleteMin);
        }
    }

    // Reuse the production contracts for the saved sections without starting Avalonia.
    private static JsonSerializerOptions CreateTestOptions(JsonSerializerOptions options) =>
        new(options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(options.TypeInfoResolver, new DefaultJsonTypeInfoResolver())
        };

    private sealed class SettingsEnvelope
    {
        public SettingsEnvelope() { }

        public RandomizerConfiguration Config { get; set; } = new();
        public RomFileViewModel RomFileViewModel { get; set; } = new();
        public SaveNewPresetViewModel SaveNewPresetViewModel { get; set; } = new();
    }
}
