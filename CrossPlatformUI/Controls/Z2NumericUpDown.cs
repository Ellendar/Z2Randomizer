using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CrossPlatformUI.Controls.Generated;
using Z2Randomizer.RandomizerCore.Flags;
using Material.Styles.Assists;

namespace CrossPlatformUI.Controls;

/// <summary>
/// A real, templatable <see cref="NumericUpDown"/> bound to one
/// <see cref="ConfigFlag"/>: <c>&lt;z:Z2NumericUpDown Flag="AttackLevelCap" /&gt;</c>.
/// The hint, bounds, tooltip and difficulty-only behaviour all come from the flag's
/// <see cref="FlagEntry"/>, so a view no longer needs a class per flag.
/// <para>
/// One end of a <c>[UiNumericPair]</c> also cross-limits: its far bound follows the
/// partner field's live value (the low end's Maximum is the high end's value, and
/// vice versa), replacing the old merged pair control.
/// </para>
/// </summary>
public sealed class Z2NumericUpDown : NumericUpDown
{
    public static readonly StyledProperty<ConfigFlag?> FlagProperty =
        AvaloniaProperty.Register<Z2NumericUpDown, ConfigFlag?>(nameof(Flag));

    private FlagBinding? _binding;

    /// <summary>
    /// Created on first use rather than in the constructor: base constructors can
    /// raise property changes before this type's initializers have run.
    /// </summary>
    private FlagBinding Binding =>
        _binding ??= new FlagBinding(this, PullFromConfig, PushToConfig);

    /// <summary>The config flag this control is bound to.</summary>
    public ConfigFlag? Flag
    {
        get => GetValue(FlagProperty);
        set => SetValue(FlagProperty, value);
    }

    // NumericUpDown's Material template is selected on StyleKey, which defaults to
    // the concrete type; without this the control has no template at all.
    protected override Type StyleKeyOverride => typeof(NumericUpDown);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FlagProperty)
        {
            ApplyFlag();
            return;
        }

        if (change.Property == ValueProperty)
            Binding.Push();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (VisualRoot is not null)
            Binding.Rebind(DataContext);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Binding.Rebind(DataContext);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Binding.Unbind();

        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Takes the flag's presentation and bounds from the registry. The flag is set
    /// while the XAML is parsed, i.e. before the control is attached, so the initial
    /// bind is left to <c>OnAttachedToVisualTree</c>.
    /// </summary>
    private void ApplyFlag()
    {
        Binding.SetEntry(Flag);
        if (Binding.Entry is not { Numeric: { } numeric } entry)
            return;

        FormatString = "N0";
        ParsingNumberStyle = NumberStyles.Integer;
        ClipValueToMinMax = true;
        Minimum = numeric.Minimum;
        Maximum = numeric.Maximum;
        Height = 72;

        TextFieldAssist.SetHints(this, entry.Label() ?? string.Empty);

        var toolTip = entry.ToolTip();
        if (!string.IsNullOrEmpty(toolTip))
            ToolTip.SetTip(this, toolTip);

        if (VisualRoot is not null)
            Binding.Rebind(DataContext);
    }

    private void PullFromConfig()
    {
        if (Binding is not { Config: { } config, Entry: { Numeric: { } numeric } })
            return;

        Value = numeric.Read(config);
        ApplyCrossBound();
    }

    /// <summary>
    /// Narrows the far bound to the partner field's live value, so a Min/Max pair
    /// cannot be set crossed. A null partner value (a cleared "roll randomly" box)
    /// falls back to the partner's hard bound.
    /// </summary>
    private void ApplyCrossBound()
    {
        if (Binding is not { Config: { } config, Entry: { } entry } ||
            entry.PartnerFlag is not { } partnerFlag ||
            FlagRegistry.Get(partnerFlag)?.Numeric is not { } partner)
        {
            return;
        }

        var partnerValue = partner.Read(config);

        if (entry.IsMinEnd)
            Maximum = partnerValue ?? partner.Maximum;
        else
            Minimum = partnerValue ?? partner.Minimum;
    }

    private void PushToConfig()
    {
        if (Binding is { Config: { } config, Entry: { Numeric: { } numeric } })
            numeric.Write(config, (int?)Value);
    }
}
