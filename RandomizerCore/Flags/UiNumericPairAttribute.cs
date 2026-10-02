using System;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Marks a single integer config field as one end of a Min/Max pair that the UI
/// renders as two side-by-side NumericUpDowns (cross-bounded: the Min box's upper
/// bound tracks the Max value and vice versa). Both fields of the pair must carry
/// the attribute and reference each other as <see cref="Partner"/>.
/// </summary>
/// <remarks>
/// The attribute is consumed by <c>CrossPlatformUI.SourceGenerator</c>; the generated
/// control is named after the "Min" end's property minus its trailing "Min" suffix
/// (e.g. <c>PalacesToComplete</c>). Label is the Material hint shown in each box.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class UiNumericPairAttribute : Attribute
{
    /// <param name="label">The Material hint text shown in this box ("Min"/"Max"/"Start Min").</param>
    /// <param name="partner">The other field of the pair (<c>nameof(otherField)</c>).</param>
    public UiNumericPairAttribute(string label, string partner)
    {
        Label = label;
        Partner = partner;
    }

    public string Label { get; }
    public string Partner { get; }
}