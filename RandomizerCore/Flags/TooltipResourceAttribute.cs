using System;

namespace Z2Randomizer.RandomizerCore.Flags;

/// <summary>
/// Overrides the resx key a generated UI control reads its tooltip from.
/// </summary>
/// <remarks>
/// By default a control for flag <c>Foo</c> reads <c>Resources.FooToolTip</c>. Use
/// this when several flags legitimately share one piece of explanatory text -- for
/// example <c>WestSize</c> and <c>EastSize</c>, which both just describe continent
/// sizing -- where copying the text per flag would create two strings to keep in
/// sync for no benefit.
///
/// Apply it to the backing field, since <c>[Reactive]</c> generates the property:
///
/// <code>
/// [Reactive]
/// [TooltipResource("OverworldSizeToolTip")]
/// private OverworldSizeOption westSize = OverworldSizeOption.LARGE;
/// </code>
///
/// The property generator forwards attributes it does not recognise
/// (<c>FlagsSerializeGenerator.GetPassThroughAttributes</c>), so the attribute
/// survives onto the property that <c>FlagsControlsGenerator</c> reads. Reading it
/// off the private field instead would not work: the field is not reachable from the
/// referencing compilation.
///
/// The name is emitted verbatim as <c>Resources.&lt;name&gt;</c>, so it is a
/// compile-time reference: a typo fails the build rather than silently dropping the
/// tooltip.
///
/// This affects presentation only. It has no effect on flag serialization, which is
/// governed by <see cref="ConditionallyIncludeInFlagsAttribute"/> and the config's
/// <c>xIncluded()</c> predicates.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TooltipResourceAttribute : Attribute
{
    public TooltipResourceAttribute(string resourceName)
    {
        ResourceName = resourceName;
    }

    /// <summary>The resx key, without the <c>Resources.</c> prefix or a suffix.</summary>
    public string ResourceName { get; }
}
