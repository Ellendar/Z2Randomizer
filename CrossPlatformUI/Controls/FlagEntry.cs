using System;
using System.Collections.Generic;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Flags;

namespace CrossPlatformUI.Controls;

/// <summary>
/// Everything a shared flag control needs to know about one config property: how
/// to label it, whether it is tri-state / difficulty-only / gated, and how to read
/// and write the value.
/// </summary>
/// <remarks>
/// One instance per flag is emitted into <c>FlagRegistry.g.cs</c>. Every member is
/// written as a literal reference (<c>Resources.X</c>, <c>c =&gt; c.X</c>), so a
/// missing resource or a renamed property fails the build rather than silently
/// rendering an unlabelled no-op control.
/// <para>
/// The members are grouped by control kind: a checkbox fills <see cref="Read"/>/
/// <see cref="Write"/>, a ComboBox fills <see cref="Items"/>/<see cref="ReadValue"/>/
/// <see cref="WriteValue"/>. <see cref="FlagBinding"/>, <see cref="IsDifficultyOnly"/>
/// and <see cref="Included"/> are shared by every kind.
/// </para>
/// </remarks>
internal sealed class FlagEntry
{
    /// <summary>The config property this flag addresses; also the PropertyChanged name to watch.</summary>
    public required string PropertyName { get; init; }

    public required Func<string?> Label { get; init; }

    public required Func<string?> ToolTip { get; init; }

    /// <summary>The <c>Included</c> observable driving <c>IsEnabled</c>, or null when always enabled.</summary>
    public Func<object?, IObservable<bool>?>? Included { get; init; }

    public bool IsDifficultyOnly { get; init; }

    // CheckBox

    public Func<RandomizerConfiguration, bool?>? Read { get; init; }

    public Action<RandomizerConfiguration, bool?>? Write { get; init; }

    public bool IsThreeState { get; init; }

    // ComboBox

    /// <summary>The option list for the property's enum; read once per control.</summary>
    public Func<IEnumerable<EnumDescription>>? Items { get; init; }

    /// <summary>Whether the options carry a right-aligned detail column.</summary>
    public bool HasInfoColumn { get; init; }

    public Func<RandomizerConfiguration, object?>? ReadValue { get; init; }

    public Action<RandomizerConfiguration, object?>? WriteValue { get; init; }

    // NumericUpDown

    /// <summary>A numeric box: bounds and value accessors.</summary>
    public NumericMember? Numeric { get; init; }

    /// <summary>
    /// The caption shown above a numeric box when it is rendered on its own (the
    /// preset diff), or null to show none. Only a Min/Max pair end has one, so the
    /// two ends read clearly away from their tab's heading.
    /// </summary>
    public Func<string?>? Caption { get; init; }

    /// <summary>
    /// A Min/Max pair's other config property: the field this box cross-limits
    /// against, and the second property watched for changes. Null for a lone numeric.
    /// </summary>
    public string? PartnerPropertyName { get; init; }

    /// <summary>The partner's enum member, so the box can read the partner's bounds.</summary>
    public ConfigFlag? PartnerFlag { get; init; }

    /// <summary>True when this is the low end of a Min/Max pair.</summary>
    public bool IsMinEnd { get; init; }
}

/// <summary>
/// One integer box's worth of metadata: how to read/write the backing config
/// property and its hard bounds.
/// </summary>
internal sealed record NumericMember
{
    /// <summary>Reads the backing config property, or null when the box is clear.</summary>
    public required Func<RandomizerConfiguration, int?> Read { get; init; }

    /// <summary>
    /// Writes the box back to the config. A nullable property stores a cleared box
    /// as null (the "roll randomly" state); a non-nullable one ignores null.
    /// </summary>
    public required Action<RandomizerConfiguration, int?> Write { get; init; }

    public required decimal Minimum { get; init; }

    public required decimal Maximum { get; init; }
}
