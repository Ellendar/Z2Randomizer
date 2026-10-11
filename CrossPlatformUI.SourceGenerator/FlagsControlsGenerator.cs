using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CrossPlatformUI.SourceGenerator;

/// <summary>
/// Emits, for every user-settable flag on a <c>[FlagSerialize]</c> config, the
/// metadata a shared control needs, in namespace
/// <c>CrossPlatformUI.Controls.Generated</c>: the <c>FlagRegistry</c> table, the
/// per-config <c>FlagIncludedResolver</c>, and the <c>FlagControlFactory</c>. The
/// <c>ConfigFlag</c> enum those name is emitted into RandomizerCore by
/// CoreSourceGenerator, which owns the config the members are named after.
/// The control kind follows the property type:
///
/// - <c>bool</c> / <c>bool?</c>  →  <c>Z2CheckBox</c>
/// - Enum  →  <c>Z2ComboBox</c>
/// - <c>int</c> / <c>int?</c> with <c>[Minimum]</c> + <c>[Maximum]</c>  →  <c>Z2NumericUpDown</c>
///
/// A <c>[UiNumericPair]</c> field is just an ordinary numeric: the two ends are two
/// separate boxes, cross-limited through <c>PartnerFlag</c> so each box's far bound
/// follows the other.
///
/// A view can then use:
///
/// <code>
/// &lt;z:Z2CheckBox Flag="RestartAtPalacesOnGameOver" /&gt;
/// &lt;z:Z2ComboBox Flag="GpLength" /&gt;
/// </code>
///
/// No per-flag control class is generated. A flag is addressed by its enum member
/// and its behaviour comes from the <c>FlagRegistry</c> entry, whose members are
/// literal references (<c>Resources.Foo</c>, <c>c =&gt; c.Foo</c>) so a missing
/// resource or a renamed property is a build error.
///
/// Resource names are derived from the flag name:
/// <c>Resources.Foo</c> for content and <c>Resources.FooToolTip</c> for the
/// tooltip. A <c>[UiNumericPair]</c> end's caption/tooltip are lazy
/// <c>ResourceManager</c> lookups instead (<c>Resources.&lt;Field&gt;Caption</c>,
/// one shared <c>Resources.&lt;MinField&gt;ToolTip</c> for both ends), so those keys
/// are optional.
///
/// <c>bool?</c> additionally gets <c>IsThreeState = true</c>.
///
/// The shared controls keep config and control in sync through a direct
/// <c>INotifyPropertyChanged</c> bridge instead of reflection or string-path
/// Avalonia bindings. This also avoids depending on ReactiveUI's generated
/// binding members across generator boundaries.
///
/// The XAML attributes are applied after the control's constructor, so anything
/// set on the element wins over the control's defaults.
///
/// <b>Theming one control only</b>: a type selector naming the shared control never
/// matches a concrete <c>z:</c> type (matching is on <c>StyleKey</c>, never on the
/// concrete class). Give the element a class in the view and style that class in
/// <c>App.axaml</c>:
///
/// <code>
/// &lt;z:Z2CheckBox Flag="StartWithCandle" Classes="WideAndAiry" /&gt;
/// ...
/// &lt;Style Selector="CheckBox.WideAndAiry"&gt;
///     &lt;Setter Property="MinWidth" Value="140"/&gt;
/// &lt;/Style&gt;
/// </code>
///
/// This is the same shape as the existing <c>Selector="CheckBox.DifficultyOnly"</c>
/// rules, which the shared controls already rely on for <c>[DifficultyOnly]</c>
/// flags.
/// </summary>
[Generator]
public sealed class FlagsControlsGenerator : IIncrementalGenerator
{
    private const string Namespace = "CrossPlatformUI.Controls.Generated";

    // ConfigFlag itself is emitted into RandomizerCore by CoreSourceGenerator.
    private const string FlagsNamespace = "Z2Randomizer.RandomizerCore.Flags";

    private const string FlagSerialize = "FlagSerializeAttribute";
    private const string EnabledObservableSuffix = "EnabledObservable";
    // Simple assembly name, which is what IAssemblySymbol.Name reports -- not the
    // "Z2Randomizer.RandomizerCore" metadata name, which never matches here.
    private const string CoreAssemblyName = "RandomizerCore";
    private const string ResourcesNamespace = "Z2Randomizer.CrossPlatformUI.Lang";
    private const string TooltipResource = "TooltipResourceAttribute";
    private const string MinimumAttribute = "MinimumAttribute";
    private const string MaximumAttribute = "MaximumAttribute";
    private const string NumericPairAttribute = "UiNumericPairAttribute";
    private const string IgnoreInFlagControls = "IgnoreInFlagControlsAttribute";

    internal static readonly SymbolDisplayFormat FullyQualified =
        SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider,
            static (spc, compilation) =>
            {
                // Flags whose control is addressed by name from XAML, collected across
                // config types for the shared FlagRegistry.
                var flagFields = new List<FlagFieldInfo>();

                foreach (var configType in FindConfigTypes(compilation))
                {
                    var classInfo = GetClassGenerationInfo(
                        compilation,
                        configType);
                    if (classInfo is null)
                        continue;

                    spc.AddSource(
                        $"{classInfo.ClassName}.Resolver.g.cs",
                        GenerateResolver(classInfo));

                    foreach (var field in classInfo.ControlFields)
                    {
                        // Every kind is served by a shared control
                        // (Z2CheckBox/Z2ComboBox/Z2NumericUpDown) that reads the
                        // flag's metadata from FlagRegistry: no per-flag class is
                        // generated for any of them.
                        flagFields.Add(new FlagFieldInfo
                        {
                            ConfigType = classInfo.ConfigType,
                            Field = field,
                        });
                    }
                }

                if (flagFields.Count > 0)
                {
                    spc.AddSource(
                        "FlagRegistry.g.cs",
                        GenerateFlagRegistry(flagFields));
                }
            });
    }

    private static IEnumerable<INamedTypeSymbol> FindConfigTypes(Compilation compilation)
    {
        // The config class lives in RandomizerCore, which is a referenced assembly from
        // the UI project. Its syntax tree is therefore not part of this compilation.
        var configAssembly = compilation.SourceModule.ReferencedAssemblySymbols
            .FirstOrDefault(a => a.Name == CoreAssemblyName);

        if (configAssembly is null)
        { 
            yield break;
        }

        foreach (var type in AllTypes(compilation, configAssembly))
        {
            if (type.TypeKind == TypeKind.Class &&
                HasAttribute(type.GetAttributes(), FlagSerialize))
            {
                yield return type;
            }
        }
    }

    private static ClassGenerationInfo? GetClassGenerationInfo(
        Compilation compilation,
        INamedTypeSymbol classSymbol)
    {
        var controlFields = new List<ControlFieldInfo>();
        var numericCandidates = new List<NumericCandidate>();

        var enumsType = classSymbol.ContainingAssembly
            .GetTypeByMetadataName("Z2Randomizer.RandomizerCore.Enums");

        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (property.IsStatic || property.IsIndexer)
                continue;

            if (property.GetMethod is null || property.SetMethod is null)
                continue;

            if (HasAttribute(property.GetAttributes(), IgnoreInFlagControls))
                continue;

            if (TryGetNumeric(property, out var numeric))
            {
                numericCandidates.Add(numeric);
                continue;
            }

            var isNullable = IsNullableBool(property.Type);
            var isEnum = property.Type.TypeKind == TypeKind.Enum;

            if (!isNullable &&
                !isEnum &&
                property.Type.SpecialType != SpecialType.System_Boolean)
            {
                continue;
            }

            // Every enum property becomes a ComboBox, like every bool becomes a
            // checkbox. Its caption (Resources.<PropertyName>) and items
            // (Enums.<PropertyName>List) are referenced unconditionally.
            if (isEnum && enumsType is null)
            {
                continue;
            }

            controlFields.Add(new ControlFieldInfo
            {
                PropertyName = property.Name,
                IsNullable = isNullable,
                Kind = isEnum ? ControlKind.ComboBox : ControlKind.CheckBox,
                EnumType = isEnum
                    ? property.Type.ToDisplayString(FullyQualified)
                    : null,
                HasInfo = isEnum &&
                    property.Type is INamedTypeSymbol enumSymbol &&
                    EnumHasInfo(enumSymbol),
                TooltipResourceName =
                    TooltipResourceOverride(property) ??
                    property.Name + "ToolTip",
                IsDifficultyOnly = property.GetAttributes()
                    .Any(a =>
                        a.AttributeClass?.Name.StartsWith("DifficultyOnly")
                            == true),
            });
        }

        foreach (var numericField in ResolveNumericCandidates(numericCandidates))
        {
            controlFields.Add(numericField);
        }

        if (controlFields.Count == 0)
            return null;

        var classInfo = new ClassGenerationInfo
        {
            ClassName = classSymbol.Name,
            ConfigType = classSymbol.ToDisplayString(FullyQualified),
            ControlFields = controlFields
        };

        // Wire enablement. A hand-written <c>FooEnabledObservable</c> on a view model
        // always wins, so a control whose rule is not mechanical (or that needs to
        // consult view-model state) can override the generated default.
        foreach (var included in BuildIncludedResolvers(compilation, controlFields))
        {
            included.ResolverMethod = "Resolve" + included.ControlPropertyName +
                "Included";

            var field = controlFields.First(f =>
                f.PropertyName == included.ControlPropertyName);
            field.IncludedResolverMethod = included.ResolverMethod;

            classInfo.IncludedResolvers.Add(included);
        }

        var handWired = new HashSet<string>(
            classInfo.IncludedResolvers.Select(r => r.ControlPropertyName),
            StringComparer.Ordinal);

        ApplyGeneratedEnablers(classSymbol, controlFields, handWired);

        return classInfo;
    }

    /// <summary>
    /// Records enablement for flags that need none of their own. If the config
    /// declares a predicate <c>fooIncluded()</c> then the control is live exactly when
    /// that predicate holds, so the registry derives the observable at bind time:
    /// <c>FlagEnablement.Build(ctx, static c =&gt; c.fooIncluded())</c>.
    /// Applies to every control kind except numerics, which are never auto-disabled.
    /// </summary>
    /// <remarks>
    /// The predicate's presence is the opt-in, rather than
    /// <c>[ConditionallyIncludeInFlags]</c> read directly, because the two are the
    /// same thing in practice: <c>FlagsSerializeGenerator</c> emits
    /// <c>if (fooIncluded())</c> around serialization of every conditional field, so a
    /// conditional flag without a predicate would not compile. Deriving from the same
    /// predicate is what keeps "is this control live?" and "does this flag get
    /// written?" from drifting apart.
    ///
    /// Reusing the predicate is also what keeps the tri-state semantics right. Flag
    /// properties are mostly <c>bool?</c> where <c>null</c> means *indeterminate* — the
    /// generator still rolls the option — so the control must stay enabled. That is
    /// why every hand-written predicate is an <c>!= false</c> test. A hand-written
    /// <c>== true</c> (or a bare <c>{Binding Config.Foo}</c>, which Avalonia coerces
    /// from <c>null</c> to <c>false</c>) disables the control precisely when it should
    /// remain reachable.
    /// </remarks>
    private static void ApplyGeneratedEnablers(
        INamedTypeSymbol configSymbol,
        List<ControlFieldInfo> controlFields,
        HashSet<string> handWired)
    {
        foreach (var field in controlFields)
        {
            if (handWired.Contains(field.PropertyName))
                continue;

            if (field.Kind is ControlKind.Numeric)
                continue;

            if (IncludedPredicateName(configSymbol, field.PropertyName)
                is not { } predicate)
            {
                continue;
            }

            field.GeneratedIncludedPredicate = predicate;
        }
    }

    /// <summary>
    /// Reads an explicit <c>[TooltipResource("OverworldSizeToolTip")]</c> override, or
    /// returns null to fall back to the <c>&lt;PropertyName&gt;ToolTip</c> convention.
    /// </summary>
    /// <remarks>
    /// Read it off the <em>property</em>, never the backing field.
    /// <c>FlagsSerializeGenerator.GetPassThroughAttributes</c> forwards the field's
    /// attributes onto the property it generates from <c>[Reactive] private T field;</c>,
    /// so the property carries it, whereas the private field is not reachable from the
    /// referencing compilation this generator walks.
    /// </remarks>
    private static string? TooltipResourceOverride(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != TooltipResource ||
                attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attribute.ConstructorArguments[0].Value is string name &&
                !string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        return null;
    }

    /// <summary>
    /// The name of the config's <c>...Included()</c> predicate for a flag, or null if
    /// it has none. Note the casing: the reactive properties are PascalCase
    /// (<c>RemoveLongDeadEnds</c>) but the hand-written predicates are camelCase
    /// (<c>removeLongDeadEndsIncluded()</c>), so both spellings are tried.
    /// </summary>
    private static string? IncludedPredicateName(
        INamedTypeSymbol configSymbol,
        string propertyName)
    {
        var camel = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];

        foreach (var candidate in new[] { propertyName + "Included", camel + "Included" })
        {
            foreach (var member in configSymbol.GetMembers(candidate))
            {
                if (member is IMethodSymbol method &&
                    method.DeclaredAccessibility == Accessibility.Public &&
                    !method.IsStatic &&
                    method.Parameters.Length == 0 &&
                    method.ReturnType.SpecialType == SpecialType.System_Boolean)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string GenerateResolver(ClassGenerationInfo classInfo)
    {
        var sb = new StringBuilder();
        var configType = classInfo.ConfigType;

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"using {FlagsNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Resolves the <c>{configType}</c> behind a view's DataContext. Every");
        sb.AppendLine("/// DataContext that can host a flag control implements IFlagHost, so this is");
        sb.AppendLine("/// a single cast rather than a per-type switch.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class RandomizerConfigResolver");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {configType}? Resolve(object? dataContext)");
        sb.AppendLine(
            "        => (dataContext as " +
            "global::CrossPlatformUI.ViewModels.IFlagHost)?.Config;");
        sb.AppendLine("}");

        if (classInfo.IncludedResolvers.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine(
                "/// Resolves the observable that drives a control's IsEnabled for the");
            sb.AppendLine(
                "/// few flags whose rule is hand-written on view-model state (a");
            sb.AppendLine(
                "/// &lt;c&gt;FooEnabledObservable&lt;/c&gt; property), not a config predicate.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine("internal static class FlagIncludedResolver");
            sb.AppendLine("{");

            foreach (var included in classInfo.IncludedResolvers)
            {
                sb.AppendLine(
                    $"    public static global::System.IObservable<bool>? " +
                    $"{included.ResolverMethod}(object? dataContext)");
                sb.AppendLine("    {");
                sb.AppendLine("        switch (dataContext)");
                sb.AppendLine("        {");

                foreach (var owner in included.Owners)
                {
                    sb.AppendLine(
                        $"            case {owner.DataContextType} value:");
                    sb.AppendLine(
                        $"                return {owner.ConfigExpression};");
                }

                sb.AppendLine("            default:");
                sb.AppendLine("                return null;");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");
        }

        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Creates the shared control for a config flag from its <c>ConfigFlag</c>,");
        sb.AppendLine("/// for the preset diff (which names changed fields by that enum). Each");
        sb.AppendLine("/// Min/Max pair end maps to its own box, with the end's optional caption");
        sb.AppendLine("/// shown above it. Null when the field has no control (an opt-out numeric,");
        sb.AppendLine("/// or a flag whose control was deliberately left hand-written).");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class FlagControlFactory");
        sb.AppendLine("{");
        sb.AppendLine(
            "    public static global::Avalonia.Controls.Control? Create(ConfigFlag flag)");
        sb.AppendLine("        {");
        sb.AppendLine(
            "        // ConfigFlag is a dense enum (implicit 0..N), so Roslyn lowers this");
        sb.AppendLine(
            "        // switch to an IL switch jump table - O(1), not a linear scan.");
        sb.AppendLine("        switch (flag)");
        sb.AppendLine("        {");

        foreach (var field in classInfo.ControlFields
                     .OrderBy(f => f.PropertyName, StringComparer.Ordinal))
        {
            sb.AppendLine($"            case ConfigFlag.{field.PropertyName}:");

            if (field.Kind == ControlKind.Numeric)
            {
                // A numeric gets the diff composite (optional caption + one box) so
                // a Min/Max end shown on its own still carries its caption.
                sb.AppendLine(
                    "                return global::CrossPlatformUI.Controls" +
                    $".NumericDiffControl.Build(ConfigFlag.{field.PropertyName});");
                continue;
            }

            var controlType = field.Kind switch
            {
                ControlKind.CheckBox => "Z2CheckBox",
                ControlKind.ComboBox => "Z2ComboBox",
                _ => "Z2CheckBox",
            };

            sb.AppendLine(
                $"                return new global::CrossPlatformUI.Controls.{controlType} " +
                $"{{ Flag = ConfigFlag.{field.PropertyName} }};");
        }

        sb.AppendLine("            default:");
        sb.AppendLine("                return null;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the <c>ConfigFlag</c> -&gt; <see cref="FlagFieldInfo"/>-derived metadata
    /// table the shared controls read at attach time. Config accessors are literal
    /// references (<c>c =&gt; c.X</c>), so a renamed property is a build error, which
    /// is the whole point of generating this rather than looking the flag up by
    /// reflection. Captions/tooltips are compile-time <c>Resources.X</c> references,
    /// except for a Min/Max pair end, whose caption and shared tooltip are optional
    /// and therefore read late through <c>ResourceManager</c>.
    /// </summary>
    private static string GenerateFlagRegistry(List<FlagFieldInfo> fields)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        // ConfigFlag is emitted into RandomizerCore next to the config it names.
        sb.AppendLine($"using {FlagsNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Maps each <c>ConfigFlag</c> to the metadata its control needs.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class FlagRegistry");
        sb.AppendLine("{");
        sb.AppendLine(
            "    private static readonly global::System.Collections.Generic.Dictionary<");
        sb.AppendLine(
            "        ConfigFlag, global::CrossPlatformUI.Controls.FlagEntry> Entries = new()");
        sb.AppendLine("    {");

        foreach (var info in fields.OrderBy(
                     f => f.Field.PropertyName,
                     StringComparer.Ordinal))
        {
            var field = info.Field;
            var name = field.PropertyName;

            sb.AppendLine(
                $"        [ConfigFlag.{name}] = " +
                "new global::CrossPlatformUI.Controls.FlagEntry");
            sb.AppendLine("        {");

            if (field.Kind == ControlKind.Numeric)
            {
                sb.AppendLine(
                    $"            PropertyName = nameof({info.ConfigType}.{name}),");

                if (field.PartnerPropertyName is not null)
                {
                    // A [UiNumericPair] end: like a single, the box hint is a
                    // compile-time Resources.<PropertyName> reference (so a missing
                    // key is a build error); the caption and shared tooltip are
                    // optional lazy lookups keyed off the field names, and the
                    // partner lets the box cross-limit the other end.
                    sb.AppendLine($"            Label = () => global::{ResourcesNamespace}.Resources.{name},");
                    sb.AppendLine(
                        "            Caption = () => " +
                        $"global::{ResourcesNamespace}.Resources.ResourceManager" +
                        $".GetString(\"{field.CaptionResourceKey}\"),");
                    sb.AppendLine(
                        "            ToolTip = () => " +
                        $"global::{ResourcesNamespace}.Resources.ResourceManager" +
                        $".GetString(\"{field.SharedTooltipResourceKey}\"),");
                    sb.AppendLine(
                        $"            PartnerPropertyName = nameof({info.ConfigType}.{field.PartnerPropertyName}),");
                    sb.AppendLine(
                        $"            PartnerFlag = ConfigFlag.{field.PartnerPropertyName},");
                    sb.AppendLine(
                        $"            IsMinEnd = {(field.IsMinEnd ? "true" : "false")},");
                }
                else
                {
                    sb.AppendLine(
                        $"            Label = () => " +
                        $"global::{ResourcesNamespace}.Resources.{name},");
                    sb.AppendLine(
                        "            ToolTip = () => " +
                        $"global::{ResourcesNamespace}.Resources" +
                        $".ResourceManager.GetString(\"{field.TooltipResourceName}\"),");
                }

                sb.AppendLine(
                    "            Numeric = new global::CrossPlatformUI.Controls.NumericMember");
                sb.AppendLine("            {");
                sb.AppendLine($"                Read = c => c.{name},");
                sb.AppendLine(
                    field.IsNumericNullable
                        ? $"                Write = (c, v) => c.{name} = v,"
                        : "                Write = (c, v) => { if (v is { } value) c." +
                          $"{name} = value; }},");
                sb.AppendLine($"                Minimum = {field.NumericMinimum ?? 0},");
                sb.AppendLine($"                Maximum = {field.NumericMaximum ?? 0},");
                sb.AppendLine("            },");
            }
            else
            {
                sb.AppendLine(
                    $"            PropertyName = nameof({info.ConfigType}.{name}),");
                sb.AppendLine(
                    $"            Label = () => " +
                    $"global::{ResourcesNamespace}.Resources.{name},");
                sb.AppendLine(
                    $"            ToolTip = () => " +
                    $"global::{ResourcesNamespace}.Resources.{field.TooltipResourceName},");

                if (field.Kind == ControlKind.ComboBox)
                {
                    sb.AppendLine(
                        $"            Items = () => " +
                        $"global::Z2Randomizer.RandomizerCore.Enums.{name}List,");

                    if (field.HasInfo)
                        sb.AppendLine("            HasInfoColumn = true,");

                    sb.AppendLine($"            ReadValue = c => c.{name},");
                    sb.AppendLine(
                        $"            WriteValue = (c, v) => c.{name} = " +
                        $"({field.EnumType})v!,");
                }
                else
                {
                    sb.AppendLine(
                        $"            IsThreeState = {(field.IsNullable ? "true" : "false")},");
                    sb.AppendLine($"            Read = c => c.{name},");
                    sb.AppendLine(
                        field.IsNullable
                            ? $"            Write = (c, v) => c.{name} = v,"
                            : $"            Write = (c, v) => c.{name} = v is true,");
                }
            }

            if (field.IsDifficultyOnly)
                sb.AppendLine("            IsDifficultyOnly = true,");

            if (field.IncludedResolverMethod is not null)
                sb.AppendLine(
                    $"            Included = ctx => " +
                    $"FlagIncludedResolver.{field.IncludedResolverMethod}(ctx),");
            else if (field.GeneratedIncludedPredicate is not null)
                sb.AppendLine(
                    "            Included = ctx => " +
                    "global::CrossPlatformUI.Controls.FlagEnablement.Build(" +
                    $"ctx, static c => c.{field.GeneratedIncludedPredicate}()),");

            sb.AppendLine("        },");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine(
            "    public static global::CrossPlatformUI.Controls.FlagEntry? " +
            "Get(ConfigFlag flag)");
        sb.AppendLine(
            "        => Entries.TryGetValue(flag, out var entry) ? entry : null;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Finds the observables that drive a control's <c>IsEnabled</c>: a view
    /// model property <c>FooEnabledObservable</c> of type
    /// <see cref="System.IObservable{T}"/>{<see cref="bool"/>}.
    /// No flag attribute is required -- any control with a matching observable
    /// is wired, whatever the reason the tab disabled it.
    /// </summary>
    private static List<IncludedResolverInfo> BuildIncludedResolvers(
        Compilation compilation,
        List<ControlFieldInfo> controlFields)
    {
        var fields = controlFields.ToDictionary(
            f => f.PropertyName,
            StringComparer.Ordinal);

        // View models live in the consuming UI compilation, not in RandomizerCore.
        var matches = new Dictionary<string, IncludedResolverInfo>(
            StringComparer.Ordinal);

        foreach (var candidate in
                 EnumerateTypes(compilation.Assembly.GlobalNamespace))
        {
            if (candidate.TypeKind != TypeKind.Class ||
                candidate.IsAbstract ||
                candidate.IsGenericType)
            {
                continue;
            }

            var display = candidate.ToDisplayString(FullyQualified);

            foreach (var property in
                     candidate.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.DeclaredAccessibility != Accessibility.Public ||
                    property.IsStatic ||
                    property.GetMethod is null ||
                    !IsBoolObservable(property.Type) ||
                    !property.Name.EndsWith(
                        EnabledObservableSuffix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                var propertyName = property.Name;

                var controlName = propertyName.Substring(
                    0,
                    propertyName.Length - EnabledObservableSuffix.Length);

                if (!fields.ContainsKey(controlName))
                    continue;

                if (!matches.TryGetValue(
                        controlName,
                        out var resolver))
                {
                    resolver = new IncludedResolverInfo
                    {
                        ControlPropertyName = controlName
                    };

                    matches.Add(controlName, resolver);
                }

                resolver.Owners.Add(
                    new ConfigResolverInfo(candidate, "value." + propertyName));
            }
        }

        return matches.Values
            .Where(r => r.Owners.Count > 0)
            .OrderBy(r => r.ControlPropertyName, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsBoolObservable(ITypeSymbol type)
    {
        return type is INamedTypeSymbol
        {
            Name: "IObservable",
            TypeArguments.Length: 1
        } observable &&
               observable.TypeArguments[0].SpecialType ==
               SpecialType.System_Boolean;
    }

    private static IEnumerable<INamedTypeSymbol> AllTypes(
        Compilation compilation,
        IAssemblySymbol configAssembly)
    {
        foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace))
            yield return type;

        // Also inspect the assembly containing the configuration types.
        // This is normally RandomizerCore.
        foreach (var type in EnumerateTypes(configAssembly.GlobalNamespace))
            yield return type;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(
        INamespaceOrTypeSymbol root)
    {
        var queue = new Queue<INamespaceOrTypeSymbol>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var member in current.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol ns:
                        queue.Enqueue(ns);
                        break;

                    case INamedTypeSymbol type:
                        yield return type;

                        foreach (var nested in type.GetTypeMembers())
                            queue.Enqueue(nested);

                        break;
                }
            }
        }
    }

    private static bool IsNullableBool(ITypeSymbol type)
    {
        return type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
        } nullable &&
        nullable.TypeArguments[0].SpecialType ==
        SpecialType.System_Boolean;
    }

    /// <summary>
    /// True for <c>int?</c>. <c>int</c> alone is checked via
    /// <see cref="SpecialType.System_Int32"/> at the call site.
    /// </summary>
    private static bool IsNullableInt(ITypeSymbol type)
    {
        return type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
        } nullable &&
        nullable.TypeArguments[0].SpecialType ==
        SpecialType.System_Int32;
    }

    /// <summary>
    /// Reads the integer constructor argument of an attribute like
    /// <c>[Minimum(n)]</c>/<c>[Maximum(n)]</c>, or null when absent/malformed.
    /// </summary>
    private static int? IntAttributeValue(
        IPropertySymbol property,
        string attributeName)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (!Matches(attribute.AttributeClass?.Name, attributeName))
                continue;

            if (attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is int value)
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Recognizes a numeric config field: <c>int</c>/<c>int?</c> with both a
    /// <c>[Minimum]</c> and a <c>[Maximum]</c>. Carries the optional
    /// <c>[UiNumericPair(partner)]</c> matching info when the field is one end
    /// of a Min/Max pair.
    /// </summary>
    private static bool TryGetNumeric(
        IPropertySymbol property,
        out NumericCandidate candidate)
    {
        candidate = null!;

        var isNullable = IsNullableInt(property.Type);
        var isInt = isNullable ||
            property.Type.SpecialType == SpecialType.System_Int32;

        if (!isInt)
            return false;

        var min = IntAttributeValue(property, MinimumAttribute);
        var max = IntAttributeValue(property, MaximumAttribute);

        if (min is null || max is null)
            return false;

        candidate = new NumericCandidate
        {
            PropertyName = property.Name,
            IsNullable = isNullable,
            Minimum = min.Value,
            Maximum = max.Value,
            IsDifficultyOnly = property.GetAttributes()
                .Any(a =>
                    a.AttributeClass?.Name.StartsWith("DifficultyOnly") == true),
            TooltipOverride = TooltipResourceOverride(property),
        };

        foreach (var attribute in property.GetAttributes())
        {
            if (!Matches(attribute.AttributeClass?.Name, NumericPairAttribute))
                continue;

            if (attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string partner)
            {
                candidate.Pair = partner;
            }

            break;
        }

        return true;
    }

    /// <summary>
    /// Turns the collected numeric fields into generated controls. Each field is its
    /// own control (the two ends of a <c>[UiNumericPair]</c> are two boxes); a mutual
    /// pair carries the partner and which end is the low one, so each box can
    /// cross-limit the other.
    /// </summary>
    private static IEnumerable<ControlFieldInfo> ResolveNumericCandidates(
        List<NumericCandidate> candidates)
    {
        var byName = new Dictionary<string, NumericCandidate>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            if (!byName.ContainsKey(candidate.PropertyName))
                byName.Add(candidate.PropertyName, candidate);
        }

        foreach (var candidate in candidates
                     .OrderBy(c => c.PropertyName, StringComparer.Ordinal))
        {
            string? partnerProperty = null;
            var isMinEnd = false;
            var isPair = false;

            if (candidate.Pair is { } partnerKey &&
                byName.TryGetValue(Pascalize(partnerKey), out var partner) &&
                partner.Pair is { } partnerBack &&
                Pascalize(partnerBack) == candidate.PropertyName)
            {
                isPair = true;
                partnerProperty = partner.PropertyName;

                // The "Min"-suffixed member is the low end. The role is decided by
                // the name suffix alone - never by a display string - so the
                // StartView pairs can hint their boxes "Start Min"/"Start Max"
                // without affecting which end is which.
                isMinEnd = candidate.PropertyName.EndsWith(
                    "Min",
                    StringComparison.Ordinal);
            }

            // A [UiNumericPair] field whose partner is missing or not mutual cannot
            // cross-limit; skip it rather than emit a box that reads a resource key
            // that does not exist. A plain numeric is emitted as a single box.
            if (candidate.Pair is not null && !isPair)
                continue;

            yield return new ControlFieldInfo
            {
                PropertyName = candidate.PropertyName,
                IsNullable = candidate.IsNullable,
                Kind = ControlKind.Numeric,
                TooltipResourceName =
                    candidate.TooltipOverride ?? candidate.PropertyName + "ToolTip",
                IsDifficultyOnly = candidate.IsDifficultyOnly,
                NumericMinimum = candidate.Minimum,
                NumericMaximum = candidate.Maximum,
                IsNumericNullable = candidate.IsNullable,
                PartnerPropertyName = partnerProperty,
                IsMinEnd = isMinEnd,
                CaptionResourceKey =
                    isPair ? candidate.PropertyName + "Caption" : null,
                SharedTooltipResourceKey = isPair
                    ? (isMinEnd ? candidate.PropertyName : partnerProperty) + "ToolTip"
                    : null,
            };
        }
    }

    private static string Pascalize(string name)
        => char.ToUpperInvariant(name[0]) + name[1..];

    /// <summary>
    /// True when any member's <c>[Description]</c> contains a ';', which
    /// <c>EnumDescription</c> splits into a second (Info) column at runtime.
    /// </summary>
    private static bool EnumHasInfo(INamedTypeSymbol enumType)
    {
        foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.HasConstantValue)
                continue;

            foreach (var attribute in member.GetAttributes())
            {
                if (!Matches(
                        attribute.AttributeClass?.Name,
                        "DescriptionAttribute"))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 1 &&
                    attribute.ConstructorArguments[0].Value is string text &&
                    text.IndexOf(';') >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasAttribute(
        IEnumerable<AttributeData> attributes,
        string name)
    {
        return attributes.Any(
            a => Matches(a.AttributeClass?.Name, name));
    }

    private static bool Matches(string? actual, string name)
    {
        return actual == name ||
               actual == name.Substring(
                   0,
                   name.Length - "Attribute".Length);
    }
}

public sealed class ClassGenerationInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string ConfigType { get; set; } = string.Empty;
    public List<ControlFieldInfo> ControlFields { get; set; } = new();
    public List<IncludedResolverInfo> IncludedResolvers { get; set; } = new();
}

public enum ControlKind
{
    CheckBox,
    ComboBox,
    Numeric
}

/// <summary>
/// A generated control field together with the config type it reads, for the
/// shared <c>FlagRegistry</c>.
/// </summary>
public sealed class FlagFieldInfo
{
    public string ConfigType { get; set; } = string.Empty;
    public ControlFieldInfo Field { get; set; } = new();
}

public sealed class ControlFieldInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public bool IsNullable { get; set; }

    public ControlKind Kind { get; set; } = ControlKind.CheckBox;

    // Fully-qualified enum type; non-null only for ComboBox controls.
    public string? EnumType { get; set; }

    // Whether the enum's members produce a non-null Info column.
    public bool HasInfo { get; set; }

        /// <summary>
        /// The resx key the control reads its tooltip from, either the
        /// <c>&lt;PropertyName&gt;ToolTip</c> convention or the value of an explicit
        /// <c>[TooltipResource(...)]</c> on the config's backing field.
        /// </summary>
        public string TooltipResourceName { get; set; } = string.Empty;

    // Non-null when the control's IsEnabled is routed through a generated
    // FlagIncludedResolver method (a hand-written FooEnabledObservable).
    public string? IncludedResolverMethod { get; set; }

    // Non-null when the control's IsEnabled derives from a config predicate
    // (fooIncluded()); the registry builds the observable with FlagEnablement.Build.
    public string? GeneratedIncludedPredicate { get; set; }

    // The config field carries [DifficultyOnly]: apply the "DifficultyOnly" style
    // class while ShareSeedAcrossDifficulty is on, instead of every view repeating
    // Classes.DifficultyOnly="{Binding Config.ShareSeedAcrossDifficulty}".
    public bool IsDifficultyOnly { get; set; }

    // Numeric (ControlKind.Numeric): bounds from the config's [Minimum]/
    // [Maximum], served by the shared Z2NumericUpDown.
    public int? NumericMinimum { get; set; }
    public int? NumericMaximum { get; set; }
    public bool IsNumericNullable { get; set; }

    // A [UiNumericPair] end: the partner field this box cross-limits against,
    // which end this is, and the optional lazy caption/shared-tooltip resource
    // keys. All null/false for a lone numeric.
    public string? PartnerPropertyName { get; set; }
    public bool IsMinEnd { get; set; }
    public string? CaptionResourceKey { get; set; }
    public string? SharedTooltipResourceKey { get; set; }
}

/// <summary>
/// A collected int/int? config field awaiting emission.
/// </summary>
public sealed class NumericCandidate
{
    public string PropertyName { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    /// <summary>The other end of the pair, as <c>nameof(otherField)</c>, or null for a lone numeric.</summary>
    public string? Pair { get; set; }
    public bool IsDifficultyOnly { get; set; }
    public string? TooltipOverride { get; set; }
}

public sealed class ConfigResolverInfo
{
    public ConfigResolverInfo(
        INamedTypeSymbol ownerType,
        string configExpression)
    {
        DataContextType = ownerType.ToDisplayString(FlagsControlsGenerator.FullyQualified);
        ConfigExpression = configExpression;
    }

    /// <summary>The DataContext view model type's fully-qualified name.</summary>
    public string DataContextType { get; }
    public string ConfigExpression { get; }
}

/// <summary>
/// One generated <c>FlagIncludedResolver</c> method: maps a DataContext to the
/// observable that drives a control's IsEnabled.
/// </summary>
public sealed class IncludedResolverInfo
{
    public string ControlPropertyName { get; set; } = string.Empty;
    public string ResolverMethod { get; set; } = string.Empty;
    public List<ConfigResolverInfo> Owners { get; set; } = new();
}
