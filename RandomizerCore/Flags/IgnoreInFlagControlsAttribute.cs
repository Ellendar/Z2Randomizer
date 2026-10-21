using System;

namespace Z2Randomizer.RandomizerCore.Flags;

/// <summary>
/// Excludes a config property from the generated flag controls. Use this for
/// properties the UI renders with a dedicated control (e.g. the <c>NesColor</c>
/// palette dropdowns) rather than the shared Z2CheckBox/Z2ComboBox/Z2NumericUpDown.
/// </summary>
/// <remarks>
/// Consumed by <c>CrossPlatformUI.SourceGenerator</c>. Distinct from
/// <see cref="IgnoreInFlagsAttribute"/>, which only keeps a property out of the
/// serialized flag string while still rendering a control for it.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class IgnoreInFlagControlsAttribute : Attribute
{
}
