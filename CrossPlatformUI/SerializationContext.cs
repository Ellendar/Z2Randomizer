using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CrossPlatformUI.ViewModels;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    IgnoreReadOnlyProperties = true,
    UseStringEnumConverter = true,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
)]
[JsonSerializable(typeof(MainViewModel))]
[JsonSerializable(typeof(RandomizerConfiguration))]
public partial class SerializationContext : JsonSerializerContext
{
    public static JsonTypeInfo<MainViewModel> SettingsReadTypeInfo =>
        (JsonTypeInfo<MainViewModel>)SettingsContexts.Reader.GetTypeInfo(typeof(MainViewModel));
    public static SerializationContext SettingsWriter => SettingsContexts.Writer;
    internal static JsonTypeInfo<RandomizerConfiguration> PresetReadTypeInfo =>
        (JsonTypeInfo<RandomizerConfiguration>)SettingsContexts.PresetReader.GetTypeInfo(typeof(RandomizerConfiguration));

    private static class SettingsContexts
    {
        internal static readonly JsonSerializerOptions Reader = CreateReadOptions(CreateSettingsOptions());
        internal static readonly SerializationContext Writer = new(CreateSettingsOptions());
        internal static readonly JsonSerializerOptions PresetReader = CreateReadOptions(new JsonSerializerOptions());
    }

    private static JsonSerializerOptions CreateSettingsOptions()
    {
        var options = new JsonSerializerOptions(Default.Options);
        options.Converters.Add(new SavedPresetConverter());
        return options;
    }

    private static JsonSerializerOptions CreateReadOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new SafeBoolConverter());
        AddSavedEnumConverters(options);
        options.TypeInfoResolver = Default.WithAddedModifier(UseNumericConfigurationDefaults);
        options.MakeReadOnly();
        return options;
    }

    private static void UseNumericConfigurationDefaults(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(RandomizerConfiguration))
            return;

        var defaults = new RandomizerConfiguration();
        foreach (var property in typeInfo.Properties)
        {
            if (property.Get == null || property.Set == null || property.CustomConverter != null)
                continue;

            if (property.PropertyType == typeof(int))
                property.CustomConverter = new SafeIntConverter((int)property.Get(defaults)!);
            else if (property.PropertyType == typeof(int?))
                property.CustomConverter = new SafeNullableIntConverter((int?)property.Get(defaults));
        }
    }
}

internal sealed class SafeIntConverter(int fallback) : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadInt(ref reader) ?? fallback;

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);

    internal static int? ReadInt(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var value))
            return value;

        reader.Skip();
        return null;
    }
}

internal sealed class SafeNullableIntConverter(int? fallback) : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        SafeIntConverter.ReadInt(ref reader) ?? fallback;

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value is int number)
            writer.WriteNumberValue(number);
        else
            writer.WriteNullValue();
    }
}

internal sealed class SafeBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.String:
                return bool.TryParse(reader.GetString(), out var value) && value;
            case JsonTokenType.Number:
                return reader.TryGetInt32(out var number) && number > 0;
            default:
                reader.Skip();
                return false;
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
}

internal sealed class SafeStringEnumConverter<T>(T fallback = default) : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return Parse(reader.GetString());
            case JsonTokenType.Number when reader.TryGetInt32(out var number):
                return Parse(number.ToString(CultureInfo.InvariantCulture));
            default:
                reader.Skip();
                return fallback;
        }
    }

    private T Parse(string? text) =>
        Enum.TryParse<T>(text, ignoreCase: true, out var value) && Enum.IsDefined(value) ? value : fallback;

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Parse(reader.GetString());

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WritePropertyName(value.ToString());
}

internal sealed class SavedPresetConverter : JsonConverter<CustomPreset>
{
    public override bool HandleNull => true;

    public override CustomPreset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var record = JsonElement.ParseValue(ref reader);
        try
        {
            var preset = record.Deserialize(SerializationContext.Default.CustomPreset);
            if (preset?.Preset != null)
                return preset;
        }
        catch (JsonException)
        {
            // A malformed entry can be retained without dropping the rest of the settings.
        }

        return new CustomPreset { Preset = "Unreadable preset", UnreadableRecord = record };
    }

    public override void Write(Utf8JsonWriter writer, CustomPreset value, JsonSerializerOptions options)
    {
        if (value?.UnreadableRecord is { } record)
            record.WriteTo(writer);
        else
            JsonSerializer.Serialize(writer, value, SerializationContext.Default.CustomPreset);
    }
}
