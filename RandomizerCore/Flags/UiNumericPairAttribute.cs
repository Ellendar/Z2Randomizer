using System;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Marks a single integer config field as one end of a Min/Max pair that the UI
/// renders as two side-by-side NumericUpDowns (cross-bounded: the Min box's upper
/// bound tracks the Max value and vice versa). Both fields of the pair must carry
/// the attribute and reference each other as <see cref="Partner"/>.
/// </summary>
/// <remarks>
/// The attribute is consumed by <c>CrossPlatformUI.SourceGenerator</c>; each end is
/// its own control (two boxes, not one). The Min/Max role is decided by the
/// "Min"/"Max" name suffix, never by anything on this attribute. Display text - the
/// box hint, the pair caption and the shared tooltip - lives in
/// <c>Resources.resx</c> under each field's name, exactly as for a lone numeric.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class UiNumericPairAttribute : Attribute
{
    /// <param name="partner">The other field of the pair (<c>nameof(otherField)</c>).</param>
    public UiNumericPairAttribute(string partner)
    {
        Partner = partner;
    }

    public string Partner { get; }
}
