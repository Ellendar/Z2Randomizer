using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace CrossPlatformUI.SourceGenerator;

/// <summary>
/// Emits one bound Avalonia control per user-settable flag on a
/// <c>[FlagSerialize]</c> config, in namespace
/// <c>CrossPlatformUI.Controls.Generated</c>. The control kind follows the
/// property type:
///
/// - <c>bool</c> / <c>bool?</c>  →  <c>CheckBox</c>
/// - Enum  →  <c>ComboBox</c>
/// - <c>int</c> / <c>int?</c> with <c>[Minimum]</c> + <c>[Maximum]</c>  →  <c>NumericUpDown</c>
/// - a <c>[UiNumericPair]</c> pair of fields  →  one <c>StackPanel</c> holding both boxes
///
/// A view can then use:
///
/// <code>
/// &lt;gen:RestartAtPalacesOnGameOver /&gt;
/// &lt;gen:Global5050JarDrop /&gt;
/// </code>
///
/// Resource names are derived from the generated property name:
/// <c>Resources.Foo</c> for content and <c>Resources.FooToolTip</c> for the
/// tooltip. Numeric captions and tooltips are lazy <c>ResourceManager</c>
/// lookups instead, so their keys are optional.
///
/// <c>bool?</c> additionally gets <c>IsThreeState = true</c>.
///
/// The generated control uses a direct <c>INotifyPropertyChanged</c> bridge
/// instead of reflection or string-path Avalonia bindings. This also avoids
/// depending on ReactiveUI's generated binding members across generator
/// boundaries.
///
///
/// The generated constructor runs before the view's XAML attributes are
/// applied, so anything set on the <c>&lt;gen:X /&gt;</c> element wins over the
/// constructor defaults.
///
/// <code>
/// &lt;gen:StartItemsLimit Grid.Column="2" Height="78" Margin="-16 -4 0 24" /&gt;
/// </code>
///
/// <b>Theming and structure live in the generator.</b>
///
/// Each control kind is emitted by one method as a single string template.
/// Every flag of that kind is therefore identical at runtime:
///
/// - <see cref="GenerateControl"/>         — checkbox
/// - <see cref="GenerateComboBox"/>        — combo
/// - <see cref="GenerateNumeric"/>         — single box
/// - <see cref="GenerateNumericPair"/>     — pair of boxes
///
/// The combos' theme is a constant inside <c>GenerateComboBox</c>:
/// <c>const string ThemeKey = "MaterialOutlineComboBox"</c>.
///
/// The same method chooses the base type with
/// <c>StyleKeyOverride => typeof(ComboBox)</c> — the type Avalonia themes and
/// style selectors match on — and carries the rest of the class as verbatim
/// <c>sb.AppendLine</c> strings.
///
/// <b>Theming one control only</b>: a type selector naming a generated type never
/// matches (matching is on <c>StyleKey</c>, never on the concrete class). Give
/// the element a class in the view and style that class in <c>App.axaml</c>:
///
/// <code>
/// &lt;gen:StartWithCandle Classes="WideAndAiry" /&gt;
/// ...
/// &lt;Style Selector="CheckBox.WideAndAiry"&gt;
///     &lt;Setter Property="MinWidth" Value="140"/&gt;
/// &lt;/Style&gt;
/// </code>
///
/// This is the same shape as the existing <c>Selector="CheckBox.DifficultyOnly"</c>
/// rules, which the generator already relies on for <c>[DifficultyOnly]</c>
/// flags.
/// </summary>
[Generator]
public sealed class FlagsControlsGenerator : IIncrementalGenerator
{
    private const string Namespace = "CrossPlatformUI.Controls.Generated";

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

    internal static readonly SymbolDisplayFormat FullyQualified =
        SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The .resx files are surfaced as additional files (see Aigamo.ResXGenerator's
        // build props), so the names of the strongly-typed Resources members are
        // available here even though the Resources class itself is source-generated.
        var resourceTexts = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(
                "Resources.resx",
                StringComparison.OrdinalIgnoreCase))
            .Select(static (file, cancellationToken) =>
                file.GetText(cancellationToken)?.ToString() ?? string.Empty)
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(resourceTexts),
            static (spc, source) =>
            {
                var (compilation, texts) = source;

                var resourceNames = new HashSet<string>(StringComparer.Ordinal);
                foreach (var text in texts)
                    CollectResourceNames(text, resourceNames);

                foreach (var configType in FindConfigTypes(compilation))
                {
                    var classInfo = GetClassGenerationInfo(
                        compilation,
                        configType,
                        resourceNames);
                    if (classInfo is null)
                        continue;

                    spc.AddSource(
                        $"{classInfo.ClassName}.Resolver.g.cs",
                        GenerateResolver(classInfo));

                    foreach (var field in classInfo.ControlFields)
                    {
                        spc.AddSource(
                            $"{field.PropertyName}.g.cs",
                            GenerateControl(classInfo, field));
                    }
                }
            });
    }

    private static void CollectResourceNames(
        string resx,
        HashSet<string> names)
    {
        foreach (Match match in Regex.Matches(resx, "<data\\s+name=\"([^\"]+)\""))
            names.Add(match.Groups[1].Value);
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
        INamedTypeSymbol classSymbol,
        HashSet<string> resourceNames)
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

            // ComboBoxes are opt-in by convention: a caption resource and a matching
            // Enums.<PropertyName>List must both exist. Until a caption is written the
            // view keeps its hand-written ComboBox.
            if (isEnum &&
                (!resourceNames.Contains(property.Name) ||
                 enumsType is null ||
                 !HasEnumList(enumsType, property.Name)))
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

        foreach (var numericField in ResolveNumericCandidates(
                     numericCandidates,
                     resourceNames))
        {
            controlFields.Add(numericField);
        }

        if (controlFields.Count == 0)
            return null;

        var classInfo = new ClassGenerationInfo
        {
            ClassName = classSymbol.Name,
            ConfigType = classSymbol.ToDisplayString(FullyQualified),
            ControlFields = controlFields,
            Resolvers = BuildConfigResolvers(compilation, classSymbol)
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

        foreach (var included in BuildGeneratedEnablers(
                     classSymbol,
                     controlFields,
                     classInfo.Resolvers,
                     handWired))
        {
            included.ResolverMethod = "Resolve" + included.ControlPropertyName +
                "Included";

            var field = controlFields.First(f =>
                f.PropertyName == included.ControlPropertyName);
            field.IncludedResolverMethod = included.ResolverMethod;

            classInfo.IncludedResolvers.Add(included);
        }

        return classInfo;
    }

    /// <summary>
    /// Builds enablement rules for flags that need none of their own. If the config
    /// declares a predicate <c>fooIncluded()</c> then the control is live exactly when
    /// that predicate holds, so the observable can be derived instead of written:
    /// <c>FlagsChanged.Select(_ =&gt; config.fooIncluded()).DistinctUntilChanged()</c>.
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
    private static List<IncludedResolverInfo> BuildGeneratedEnablers(
        INamedTypeSymbol configSymbol,
        List<ControlFieldInfo> controlFields,
        Dictionary<string, ConfigResolverInfo> configResolvers,
        HashSet<string> handWired)
    {
        var result = new List<IncludedResolverInfo>();

        foreach (var field in controlFields)
        {
            if (handWired.Contains(field.PropertyName))
                continue;

            if (field.Kind is ControlKind.Numeric or
                ControlKind.NumericPair)
            {
                continue;
            }

if (IncludedPredicateName(configSymbol, field.PropertyName)
                is not { } predicate)
            {
                continue;
            }

            var owners = new List<ConfigResolverInfo>();

            foreach (var resolver in configResolvers.Values)
            {
                if (FlagsChangedExpression(resolver.OwnerType) is not { } flagsChanged)
                    continue;

                owners.Add(new ConfigResolverInfo(
                    resolver.OwnerType,
                    $"{flagsChanged}.Select(_ => {resolver.ConfigExpression}." +
                    $"{predicate}()).DistinctUntilChanged()"));
            }

            if (owners.Count == 0)
                continue;

            result.Add(new IncludedResolverInfo
            {
                ControlPropertyName = field.PropertyName,
                Owners = owners
            });
        }

        return result;
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

    /// <summary>
    /// The expression reaching <c>FlagsChanged</c> from a DataContext of the given
    /// type: directly on the root view model, or via its <c>Main</c>. Null when the
    /// type exposes neither.
    /// </summary>
    private static string? FlagsChangedExpression(INamedTypeSymbol candidate)
    {
        if (PublicInstanceProperty(candidate, "FlagsChanged") is not null)
            return "value.FlagsChanged";

        var main = PublicInstanceProperty(candidate, "Main");

        if (main?.Type is INamedTypeSymbol mainType &&
            PublicInstanceProperty(mainType, "FlagsChanged") is not null)
        {
            return "value.Main.FlagsChanged";
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
        // Select/DistinctUntilChanged for the generated enablement observables.
        // Not System.Reactive.Linq: this repo takes them from ReactiveUI.Primitives.
        sb.AppendLine("using ReactiveUI.Primitives;");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Resolves the <c>{configType}</c> behind a view's DataContext.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class RandomizerConfigResolver");
        sb.AppendLine("{");

        if (classInfo.Resolvers.Count == 0)
        {
            sb.AppendLine(
                $"    public static {configType}? Resolve(object? dataContext) => null;");
        }
        else
        {
            sb.AppendLine(
                $"    public static {configType}? Resolve(object? dataContext)");
            sb.AppendLine("    {");
            sb.AppendLine("        switch (dataContext)");
            sb.AppendLine("        {");

            foreach (var resolver in classInfo.Resolvers.Values.OrderBy(r => r.DataContextType, StringComparer.Ordinal))
            {
                sb.AppendLine(
                    $"            case {resolver.DataContextType} value:");
                sb.AppendLine(
                    $"                return {resolver.ConfigExpression};");
            }

            sb.AppendLine("            default:");
            sb.AppendLine("                return null;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        if (classInfo.IncludedResolvers.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine(
                "/// Resolves the observable that drives a control's IsEnabled.");
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
        sb.AppendLine("/// Creates the generated control for a config property from its name alone,");
        sb.AppendLine("/// for callers that only have the name at runtime (the preset diff). Null");
        sb.AppendLine("/// when the field has no generated control: a gated ComboBox/numeric, or a");
        sb.AppendLine("/// flag whose control was deliberately left hand-written.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class FlagControlFactory");
        sb.AppendLine("{");
        sb.AppendLine(
            "    public static global::Avalonia.Controls.Control? Create(string fieldName)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (fieldName)");
        sb.AppendLine("        {");

        foreach (var field in classInfo.ControlFields
                     .OrderBy(f => f.PropertyName, StringComparer.Ordinal))
        {
            if (field.Kind == ControlKind.NumericPair && field.PairMembers is not null)
            {
                // Diff reports the Min/Max config properties individually; both ends
                // share the one merged control, named after the Min field minus Min.
                foreach (var member in field.PairMembers)
                {
                    sb.AppendLine($"            case \"{member.PropertyName}\":");
                }
            }
            else
            {
                sb.AppendLine($"            case \"{field.PropertyName}\":");
            }

            sb.AppendLine($"                return new {field.PropertyName}();");
        }

        sb.AppendLine("            default:");
        sb.AppendLine("                return null;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateControl(
        ClassGenerationInfo classInfo,
        ControlFieldInfo field)
    {
        if (field.Kind == ControlKind.ComboBox)
            return GenerateComboBox(classInfo, field);

        if (field.Kind == ControlKind.Numeric)
            return GenerateNumeric(classInfo, field);

        if (field.Kind == ControlKind.NumericPair)
            return GenerateNumericPair(classInfo, field);

        var sb = new StringBuilder();
        var configType = classInfo.ConfigType;

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Avalonia;");
        sb.AppendLine("using Avalonia.Controls;");
        sb.AppendLine("using Avalonia.VisualTree;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine(
            $"/// Binds <c>{configType}.{field.PropertyName}</c> to a CheckBox.");
        sb.AppendLine(
            $"/// Tri-state: {(field.IsNullable ? "yes (bool?)" : "no (bool)")}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine(
            $"public sealed class {field.PropertyName} : CheckBox");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine(
            "    // Avalonia's type selectors (Selector=\"CheckBox\", including the one");
        sb.AppendLine(
            "    // Material.Avalonia uses to supply the template) match on StyleKey, which");
        sb.AppendLine(
            "    // defaults to the *concrete* type. Without this override every CheckBox style");
        sb.AppendLine(
            "    // and the theme are skipped: the control still handles clicks and still shows");
        sb.AppendLine(
            "    // its Content, but has no template, so the box itself never draws.");
        sb.AppendLine("    protected override Type StyleKeyOverride => typeof(CheckBox);");
        sb.AppendLine();

        sb.AppendLine(
            $"    private const string ConfigPropertyName = " +
            $"nameof({configType}.{field.PropertyName});");
        sb.AppendLine();

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine(
                "    private const string ShareSeedPropertyName = " +
                $"nameof({configType}.ShareSeedAcrossDifficulty);");
            sb.AppendLine();
            sb.AppendLine(
                "    private const string DifficultyOnlyClassName = \"DifficultyOnly\";");
            sb.AppendLine();
        }

        sb.AppendLine($"    private {configType}? _config;");
        sb.AppendLine(
            "    private PropertyChangedEventHandler? _configPropertyChanged;");
        sb.AppendLine("    private bool _syncing;");

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine(
                "    private global::System.IDisposable? _includedSubscription;");
        }

        sb.AppendLine();

        sb.AppendLine($"    public {field.PropertyName}()");
        sb.AppendLine("    {");
        sb.AppendLine(
            "        // A wrapping TextBlock, not a bare string: Avalonia renders string content");
        sb.AppendLine("        // with TextWrapping=NoWrap, so long labels get clipped at the panel edge");
        sb.AppendLine(
            "        // instead of wrapping. This is what the hand-written <LineBreak/>s in the");
        sb.AppendLine("        // views were working around.");
        sb.AppendLine("        Content = new Avalonia.Controls.TextBlock");
        sb.AppendLine("        {");
        sb.AppendLine(
            $"            Text = global::{ResourcesNamespace}.Resources.{field.PropertyName},");
        sb.AppendLine("            TextWrapping = Avalonia.Media.TextWrapping.Wrap,");
        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine(
            "        var toolTip = " +
            $"global::{ResourcesNamespace}.Resources.{field.TooltipResourceName};");
        sb.AppendLine();
        sb.AppendLine(
            "        // An empty resource means \"no tooltip\": Avalonia would otherwise show");
        sb.AppendLine(
            "        // an empty popup for every property that has no description yet.");
        sb.AppendLine(
            "        if (!string.IsNullOrEmpty(toolTip))");
        sb.AppendLine("            ToolTip.SetTip(this, toolTip);");
        sb.AppendLine();
        sb.AppendLine(
            $"        IsThreeState = {(field.IsNullable ? "true" : "false")};");
        sb.AppendLine("    }");
        sb.AppendLine();

        // DataContext can change before or after attachment. Rebind when it changes,
        // but let attachment perform the initial bind when the control enters a tree.
        sb.AppendLine(
            "    protected override void OnDataContextChanged(EventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnDataContextChanged(e);");
        sb.AppendLine();
        sb.AppendLine("        if (VisualRoot is not null)");
        sb.AppendLine("            Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine(
            "    protected override void OnAttachedToVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnAttachedToVisualTree(e);");
        sb.AppendLine();
        sb.AppendLine("        Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine(
            "    protected override void OnDetachedFromVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine();
        sb.AppendLine("        base.OnDetachedFromVisualTree(e);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    protected override void OnPropertyChanged(");
        sb.AppendLine("        AvaloniaPropertyChangedEventArgs change)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnPropertyChanged(change);");
        sb.AppendLine();
        sb.AppendLine(
            "        if (_syncing || _config is null || " +
            "change.Property != IsCheckedProperty)");
        sb.AppendLine("            return;");
        sb.AppendLine();

        if (field.IsNullable)
        {
            sb.AppendLine(
                $"        _config.{field.PropertyName} = IsChecked;");
        }
        else
        {
            sb.AppendLine(
                $"        _config.{field.PropertyName} = IsChecked is true;");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    private void Rebind(object? dataContext)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine();

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine(
                "        // This flag only participates while its <c>Included</c> guard is");
            sb.AppendLine(
                "        // on. The observable is StartWith-ed, so the initial value lands");
            sb.AppendLine(
                "        // synchronously and the box is never briefly enabled or stale.");
            sb.AppendLine(
                $"        _includedSubscription = FlagIncludedResolver.{field.IncludedResolverMethod}(" +
                "dataContext)");
            sb.AppendLine("            ?.Subscribe(onNext: value => IsEnabled = value);");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        var config = RandomizerConfigResolver.Resolve(dataContext);");
        sb.AppendLine("        if (config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        _config = config;");
        sb.AppendLine(
            "        _configPropertyChanged = OnConfigPropertyChanged;");
        sb.AppendLine(
            "        _config.PropertyChanged += _configPropertyChanged;");
        sb.AppendLine();
        sb.AppendLine("        PushConfigToControl();");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    private void Unbind()");
        sb.AppendLine("    {");

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine("        _includedSubscription?.Dispose();");
            sb.AppendLine("        _includedSubscription = null;");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        if (_config is not null && " +
            "_configPropertyChanged is not null)");
        sb.AppendLine("        {");
        sb.AppendLine(
            "            _config.PropertyChanged -= _configPropertyChanged;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        _config = null;");
        sb.AppendLine("_configPropertyChanged = null;");

        if (field.IsDifficultyOnly)
        {
            // No config means no answer for ShareSeedAcrossDifficulty, so the class
            // comes off rather than staying stale from a previous DataContext.
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine(
            "    private void OnConfigPropertyChanged(");
        sb.AppendLine(
            "        object? sender, PropertyChangedEventArgs e)");
        sb.AppendLine("    {");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine(
                "        if (e.PropertyName == ShareSeedPropertyName)");
            sb.AppendLine("        {");
            sb.AppendLine("            UpdateDifficultyOnlyClass();");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        if (e.PropertyName != ConfigPropertyName)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        PushConfigToControl();");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    private void PushConfigToControl()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        _syncing = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(
            $"            IsChecked = _config.{field.PropertyName};");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _syncing = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        AppendDifficultyOnlyEmitter(sb, field);

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the members that keep the "DifficultyOnly" style class in sync with
    /// <c>ShareSeedAcrossDifficulty</c>, for a control whose config field carries
    /// <c>[DifficultyOnly]</c>. No-op for every other field, so nothing is emitted
    /// unless the attribute is actually there.
    /// </summary>
    private static void AppendDifficultyOnlyEmitter(
        StringBuilder sb,
        ControlFieldInfo field)
    {
        if (!field.IsDifficultyOnly)
            return;

        sb.AppendLine("    /// <summary>");
        sb.AppendLine(
            "    /// Difficulty-only flags are shown read-only while the seed is shared");
        sb.AppendLine(
            "    /// across difficulties: App.axaml's <c>.DifficultyOnly</c> selectors do");
        sb.AppendLine(
            "    /// the graying, and this control only has to keep the class in sync, so");
        sb.AppendLine(
            "    /// no view has to repeat the binding.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    private void UpdateDifficultyOnlyClass()");
        sb.AppendLine("    {");
        sb.AppendLine(
            "        var difficultyOnly = " +
            "_config?.ShareSeedAcrossDifficulty ?? false;");
        sb.AppendLine(
            "        if (difficultyOnly == " +
            "Classes.Contains(DifficultyOnlyClassName))");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        if (difficultyOnly)");
        sb.AppendLine("            Classes.Add(DifficultyOnlyClassName);");
        sb.AppendLine("        else");
        sb.AppendLine("            Classes.Remove(DifficultyOnlyClassName);");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string GenerateComboBox(
        ClassGenerationInfo classInfo,
        ControlFieldInfo field)
    {
        var sb = new StringBuilder();
        var configType = classInfo.ConfigType;
        var enumType = field.EnumType!;
        const string enumDescription =
            "global::Z2Randomizer.RandomizerCore.EnumDescription";
        const string ThemeKey = "MaterialOutlineComboBox";
        var itemsSource =
            "global::Z2Randomizer.RandomizerCore.Enums." +
            field.PropertyName + "List";

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Avalonia;");
        sb.AppendLine("using Avalonia.Controls;");
        sb.AppendLine("using Avalonia.Controls.Templates;");
        sb.AppendLine("using Avalonia.Layout;");
        sb.AppendLine("using Avalonia.VisualTree;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine(
            $"/// Binds <c>{configType}.{field.PropertyName}</c> to a ComboBox.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {field.PropertyName} : ComboBox");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine(
            "    // Avalonia style selectors match on StyleKey; without this override");
        sb.AppendLine(
            "    // the theme's ComboBox template would not be applied.");
        sb.AppendLine(
            "    protected override Type StyleKeyOverride => typeof(ComboBox);");
        sb.AppendLine();
        sb.AppendLine(
            $"    private const string ConfigPropertyName = " +
            $"nameof({configType}.{field.PropertyName});");
        sb.AppendLine();

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine(
                "    private const string ShareSeedPropertyName = " +
                $"nameof({configType}.ShareSeedAcrossDifficulty);");
            sb.AppendLine();
            sb.AppendLine(
                "    private const string DifficultyOnlyClassName = \"DifficultyOnly\";");
            sb.AppendLine();
        }

        sb.AppendLine(
            "    private readonly global::System.Collections.Generic.List<" +
            $"{enumDescription}> _items;");
        sb.AppendLine();
        sb.AppendLine($"    private {configType}? _config;");
        sb.AppendLine(
            "    private PropertyChangedEventHandler? _configPropertyChanged;");
        sb.AppendLine("    private bool _syncing;");

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine(
                "    private global::System.IDisposable? _includedSubscription;");
        }

        sb.AppendLine();
        sb.AppendLine($"    public {field.PropertyName}()");
        sb.AppendLine("    {");
        sb.AppendLine($"        _items = {itemsSource}.ToList();");
        sb.AppendLine("        ItemsSource = _items;");
        sb.AppendLine();
        sb.AppendLine(
            "        // Material's outline variant. Resolved from the Application resource");
        sb.AppendLine(
            "        // host, which is the same lookup the XAML compiler performs for");
        sb.AppendLine(
            "        // Theme=\"{StaticResource MaterialOutlineComboBox}\".");
        sb.AppendLine(
            "        if (global::Avalonia.Application.Current is { } app &&");
        sb.AppendLine(
            $"            app.TryFindResource(\"{ThemeKey}\", out var outline) &&");
        sb.AppendLine(
            "            outline is global::Avalonia.Styling.ControlTheme outlineTheme)");
        sb.AppendLine("        {");
        sb.AppendLine("            Theme = outlineTheme;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine(
            "        var toolTip = " +
            $"global::{ResourcesNamespace}.Resources.{field.TooltipResourceName};");
        sb.AppendLine("        if (!string.IsNullOrEmpty(toolTip))");
        sb.AppendLine("            ToolTip.SetTip(this, toolTip);");
        sb.AppendLine();
        sb.AppendLine(
            "        var label = " +
            $"global::{ResourcesNamespace}.Resources.{field.PropertyName};");
        sb.AppendLine("        if (!string.IsNullOrEmpty(label))");
        sb.AppendLine(
            "            global::Material.Styles.Assists.ComboBoxAssist.SetLabel(this, label);");

        if (field.HasInfo)
        {
            sb.AppendLine();
            sb.AppendLine(
                "        // Options may carry a right-aligned detail column: the");
            sb.AppendLine(
                "        // text after the ';' in the enum member's [Description].");
            sb.AppendLine(
                $"        ItemTemplate = new FuncDataTemplate<{enumDescription}>(");
            sb.AppendLine("            (data, _) =>");
            sb.AppendLine("            {");
            sb.AppendLine("                if (data is null)");
            sb.AppendLine("                {");
            sb.AppendLine(
                "                    // The closed ComboBox runs the template for a null");
            sb.AppendLine(
                "                    // selection while PushConfigToControl clears it.");
            sb.AppendLine("                    return null;");
            sb.AppendLine("                }");
            sb.AppendLine();
            sb.AppendLine(
                "                var grid = new Grid { ColumnDefinitions = " +
                "new ColumnDefinitions(\"Auto, *\") };");
            sb.AppendLine(
                "                var description = new TextBlock { Text = data.Description };");
            sb.AppendLine("                Grid.SetColumn(description, 0);");
            sb.AppendLine("                grid.Children.Add(description);");
            sb.AppendLine(
                "                if (!string.IsNullOrEmpty(data.Info))");
            sb.AppendLine("                {");
            sb.AppendLine(
                "                    var info = new TextBlock");
            sb.AppendLine("                    {");
            sb.AppendLine("                        Text = data.Info,");
            sb.AppendLine(
                "                        HorizontalAlignment = HorizontalAlignment.Right,");
            sb.AppendLine("                    };");
            sb.AppendLine("                    Grid.SetColumn(info, 1);");
            sb.AppendLine("                    grid.Children.Add(info);");
            sb.AppendLine("                }");
            sb.AppendLine("                return grid;");
            sb.AppendLine("            });");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDataContextChanged(EventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnDataContextChanged(e);");
        sb.AppendLine();
        sb.AppendLine("        if (VisualRoot is not null)");
        sb.AppendLine("            Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnAttachedToVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnAttachedToVisualTree(e);");
        sb.AppendLine();
        sb.AppendLine("        Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDetachedFromVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine();
        sb.AppendLine("        base.OnDetachedFromVisualTree(e);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected override void OnPropertyChanged(");
        sb.AppendLine("        AvaloniaPropertyChangedEventArgs change)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnPropertyChanged(change);");
        sb.AppendLine();
        sb.AppendLine(
            "        if (_syncing || _config is null || " +
            "change.Property != SelectedItemProperty)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine(
            $"        if (SelectedItem is {enumDescription} item && " +
            $"item.Value is {enumType} value)");
        sb.AppendLine($"            _config.{field.PropertyName} = value;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Rebind(object? dataContext)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine();

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine(
                "        // This flag only participates while its <c>Included</c> guard is");
            sb.AppendLine(
                "        // on. The observable is StartWith-ed, so the initial value lands");
            sb.AppendLine(
                "        // synchronously and the list is never briefly enabled or stale.");
            sb.AppendLine(
                $"        _includedSubscription = FlagIncludedResolver.{field.IncludedResolverMethod}(" +
                "dataContext)");
            sb.AppendLine(
                "            ?.Subscribe(onNext: value => IsEnabled = value);");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        var config = RandomizerConfigResolver.Resolve(dataContext);");
        sb.AppendLine("        if (config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        _config = config;");
        sb.AppendLine(
            "        _configPropertyChanged = OnConfigPropertyChanged;");
        sb.AppendLine(
            "        _config.PropertyChanged += _configPropertyChanged;");
        sb.AppendLine();
        sb.AppendLine("        PushConfigToControl();");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Unbind()");
        sb.AppendLine("    {");

        if (field.IncludedResolverMethod is not null)
        {
            sb.AppendLine("        _includedSubscription?.Dispose();");
            sb.AppendLine("        _includedSubscription = null;");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        if (_config is not null && " +
            "_configPropertyChanged is not null)");
        sb.AppendLine("        {");
        sb.AppendLine(
            "            _config.PropertyChanged -= _configPropertyChanged;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        _config = null;");
        sb.AppendLine("        _configPropertyChanged = null;");

        if (field.IsDifficultyOnly)
        {
            // No config means no answer for ShareSeedAcrossDifficulty, so the class
            // comes off rather than staying stale from a previous DataContext.
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    private void OnConfigPropertyChanged(");
        sb.AppendLine(
            "        object? sender, PropertyChangedEventArgs e)");
        sb.AppendLine("    {");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine(
                "        if (e.PropertyName == ShareSeedPropertyName)");
            sb.AppendLine("        {");
            sb.AppendLine("            UpdateDifficultyOnlyClass();");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine(
            "        if (e.PropertyName != ConfigPropertyName)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        PushConfigToControl();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void PushConfigToControl()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine();
        sb.AppendLine("        _syncing = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine(
            $"            var value = _config.{field.PropertyName};");
        sb.AppendLine("            SelectedItem = null;");
        sb.AppendLine("            foreach (var item in _items)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (Equals(item.Value, value))");
        sb.AppendLine("                {");
        sb.AppendLine("                    SelectedItem = item;");
        sb.AppendLine("                    break;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _syncing = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        AppendDifficultyOnlyEmitter(sb, field);

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Emits an integer NumericUpDown bound to a single <c>int</c>/<c>int?</c> config
    /// field. Bounds are read from the field's <c>[Minimum]</c>/<c>[Maximum]</c>, the
    /// Material hint (floating label) from <c>Resources.&lt;PropertyName&gt;</c>.
    /// </summary>
    private static string GenerateNumeric(
        ClassGenerationInfo classInfo,
        ControlFieldInfo field)
    {
        var sb = new StringBuilder();
        var configType = classInfo.ConfigType;
        var typeName = field.PropertyName;

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Avalonia;");
        sb.AppendLine("using Avalonia.Controls;");
        sb.AppendLine("using Avalonia.VisualTree;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Binds <c>{configType}.{typeName}</c> to an integer NumericUpDown.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {typeName} : NumericUpDown");
        sb.AppendLine("{");
        sb.AppendLine(
            "    // The Material theme selects NumericUpDown templates on StyleKey;");
        sb.AppendLine(
            "    // without this override the concrete type has no template.");
        sb.AppendLine(
            "    protected override Type StyleKeyOverride => typeof(NumericUpDown);");
        sb.AppendLine();
        sb.AppendLine(
            $"    private const string ConfigPropertyName = nameof({configType}.{typeName});");
        sb.AppendLine();

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine(
                "    private const string ShareSeedPropertyName = " +
                $"nameof({configType}.ShareSeedAcrossDifficulty);");
            sb.AppendLine();
            sb.AppendLine(
                "    private const string DifficultyOnlyClassName = \"DifficultyOnly\";");
            sb.AppendLine();
        }

        sb.AppendLine($"    private {configType}? _config;");
        sb.AppendLine(
            "    private PropertyChangedEventHandler? _configPropertyChanged;");
        sb.AppendLine("    private bool _syncing;");
        sb.AppendLine();
        sb.AppendLine($"    public {typeName}()");
        sb.AppendLine("    {");
        sb.AppendLine("        FormatString = \"N0\";");
        sb.AppendLine(
            "        ParsingNumberStyle = global::System.Globalization.NumberStyles.Integer;");
        sb.AppendLine("        ClipValueToMinMax = true;");
        sb.AppendLine($"        Minimum = {field.NumericMinimum};");
        sb.AppendLine($"        Maximum = {field.NumericMaximum};");
        sb.AppendLine("        Height = 72;");
        sb.AppendLine();
        sb.AppendLine(
            $"        global::Material.Styles.Assists.TextFieldAssist.SetHints(" +
            $"this, global::{ResourcesNamespace}.Resources.{typeName});");
        sb.AppendLine();
        sb.AppendLine(
            $"        var toolTip = global::{ResourcesNamespace}.Resources.ResourceManager" +
            $".GetString(\"{field.TooltipResourceName}\");");
        sb.AppendLine("        if (!string.IsNullOrEmpty(toolTip))");
        sb.AppendLine("            ToolTip.SetTip(this, toolTip);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDataContextChanged(EventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnDataContextChanged(e);");
        sb.AppendLine("        if (VisualRoot is not null)");
        sb.AppendLine("            Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnAttachedToVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnAttachedToVisualTree(e);");
        sb.AppendLine("        Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDetachedFromVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine("        base.OnDetachedFromVisualTree(e);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected override void OnPropertyChanged(");
        sb.AppendLine("        AvaloniaPropertyChangedEventArgs change)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnPropertyChanged(change);");
        sb.AppendLine(
            "        if (_syncing || _config is null || change.Property != ValueProperty)");
        sb.AppendLine("            return;");
        sb.AppendLine();

        if (field.IsNumericNullable)
        {
            sb.AppendLine($"        _config.{typeName} = (int?)Value;");
        }
        else
        {
            sb.AppendLine($"        if (Value is {{ }} value)");
            sb.AppendLine($"            _config.{typeName} = (int)value;");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Rebind(object? dataContext)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine("        var config = RandomizerConfigResolver.Resolve(dataContext);");
        sb.AppendLine("        if (config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine("        _config = config;");
        sb.AppendLine(
            "        _configPropertyChanged = OnConfigPropertyChanged;");
        sb.AppendLine("        _config.PropertyChanged += _configPropertyChanged;");
        sb.AppendLine();
        sb.AppendLine("        PushConfigToControl();");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Unbind()");
        sb.AppendLine("    {");
        sb.AppendLine(
            "        if (_config is not null && _configPropertyChanged is not null)");
        sb.AppendLine("        {");
        sb.AppendLine("            _config.PropertyChanged -= _configPropertyChanged;");
        sb.AppendLine("        }");
        sb.AppendLine("        _config = null;");
        sb.AppendLine("        _configPropertyChanged = null;");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine("        UpdateDifficultyOnlyClass();");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void OnConfigPropertyChanged(");
        sb.AppendLine("        object? sender, PropertyChangedEventArgs e)");
        sb.AppendLine("    {");

        if (field.IsDifficultyOnly)
        {
            sb.AppendLine("        if (e.PropertyName == ShareSeedPropertyName)");
            sb.AppendLine("        {");
            sb.AppendLine("            UpdateDifficultyOnlyClass();");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        if (e.PropertyName != ConfigPropertyName)");
        sb.AppendLine("            return;");
        sb.AppendLine("        PushConfigToControl();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void PushConfigToControl()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine("        _syncing = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            Value = _config.{typeName};");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _syncing = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        AppendDifficultyOnlyEmitter(sb, field);

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Emits a two-box Min/Max pair bound to two sibling config fields
    /// (<c>[UiNumericPair]</c>). The Min box's upper bound tracks the Max field and
    /// vice versa, mirroring the hand-written
    /// <c>Minimum="&#123;Binding ...Max&#125;"</c> cross-bindings. An optional caption
    /// is shown when <c>Resources.&lt;ControlName&gt;</c> is non-empty.
    /// </summary>
    private static string GenerateNumericPair(
        ClassGenerationInfo classInfo,
        ControlFieldInfo field)
    {
        var sb = new StringBuilder();
        var configType = classInfo.ConfigType;
        var typeName = field.PropertyName;
        var minMember = field.PairMembers![0];
        var maxMember = field.PairMembers[1];

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Avalonia;");
        sb.AppendLine("using Avalonia.Controls;");
        sb.AppendLine("using Avalonia.Layout;");
        sb.AppendLine("using Avalonia.VisualTree;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine();
        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine(
            $"/// Binds <c>{configType}.{minMember.PropertyName}</c> and");
        sb.AppendLine(
            $"/// <c>{configType}.{maxMember.PropertyName}</c> to two cross-bounded");
        sb.AppendLine("/// NumericUpDowns.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {typeName} : StackPanel");
        sb.AppendLine("{");
        sb.AppendLine(
            $"    private const string ConfigMinPropertyName = " +
            $"nameof({configType}.{minMember.PropertyName});");
        sb.AppendLine(
            $"    private const string ConfigMaxPropertyName = " +
            $"nameof({configType}.{maxMember.PropertyName});");
        sb.AppendLine();
        sb.AppendLine("    private readonly NumericUpDown _min;");
        sb.AppendLine("    private readonly NumericUpDown _max;");
        sb.AppendLine($"    private {configType}? _config;");
        sb.AppendLine(
            "    private PropertyChangedEventHandler? _configPropertyChanged;");
        sb.AppendLine("    private bool _syncing;");
        sb.AppendLine();
        sb.AppendLine($"    public {typeName}()");
        sb.AppendLine("    {");
        sb.AppendLine("        Orientation = global::Avalonia.Layout.Orientation.Vertical;");
        sb.AppendLine();
        sb.AppendLine(
            $"        var caption = global::{ResourcesNamespace}.Resources.ResourceManager" +
            $".GetString(\"{typeName}\");");
        sb.AppendLine("        if (!string.IsNullOrEmpty(caption))");
        sb.AppendLine("        {");
        sb.AppendLine("            Children.Add(new TextBlock");
        sb.AppendLine("            {");
        sb.AppendLine("                Text = caption,");
        sb.AppendLine("                Margin = new global::Avalonia.Thickness(2, 0, 0, 4),");
        sb.AppendLine(
            "                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,");
        sb.AppendLine("            });");
        sb.AppendLine();
        sb.AppendLine(
            $"            var toolTip = global::{ResourcesNamespace}.Resources" +
            $".ResourceManager.GetString(\"{typeName}ToolTip\");");
        sb.AppendLine("            if (!string.IsNullOrEmpty(toolTip))");
        sb.AppendLine("                ToolTip.SetTip(this, toolTip);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var values = new Grid");
        sb.AppendLine("        {");
        sb.AppendLine("            ColumnDefinitions = new ColumnDefinitions(\"72,72\"),");
        sb.AppendLine("            ColumnSpacing = 8,");
        sb.AppendLine("            Margin = new global::Avalonia.Thickness(2, 0, 0, 0),");
        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine(
            $"        _min = CreateNumericBox(\"{minMember.Label}\", " +
            $"{minMember.Minimum}, {minMember.Maximum});");
        sb.AppendLine(
            $"        _max = CreateNumericBox(\"{maxMember.Label}\", " +
            $"{maxMember.Minimum}, {maxMember.Maximum});");
        sb.AppendLine("        _min.PropertyChanged += OnMinValueChanged;");
        sb.AppendLine("        _max.PropertyChanged += OnMaxValueChanged;");
        sb.AppendLine("        Grid.SetColumn(_min, 0);");
        sb.AppendLine("        Grid.SetColumn(_max, 1);");
        sb.AppendLine("        values.Children.Add(_min);");
        sb.AppendLine("        values.Children.Add(_max);");
        sb.AppendLine("        Children.Add(values);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    private static NumericUpDown CreateNumericBox(" +
            "string hint, decimal minimum, decimal maximum)");
        sb.AppendLine("    {");
        sb.AppendLine("        var box = new NumericUpDown");
        sb.AppendLine("        {");
        sb.AppendLine("            FormatString = \"N0\",");
        sb.AppendLine(
            "            ParsingNumberStyle = global::System.Globalization.NumberStyles.Integer,");
        sb.AppendLine("            ClipValueToMinMax = true,");
        sb.AppendLine("            Minimum = minimum,");
        sb.AppendLine("            Maximum = maximum,");
        sb.AppendLine("            Height = 72,");
        sb.AppendLine("        };");
        sb.AppendLine(
            "        global::Material.Styles.Assists.TextFieldAssist.SetHints(box, hint);");
        sb.AppendLine("        return box;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void OnMinValueChanged(");
        sb.AppendLine("        object? sender, AvaloniaPropertyChangedEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (e.Property != NumericUpDown.ValueProperty)");
        sb.AppendLine("            return;");
        sb.AppendLine("        if (_syncing || _config is null)");
        sb.AppendLine("            return;");
        AppendPairWriteBack(sb, "_min", "_config", minMember);
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void OnMaxValueChanged(");
        sb.AppendLine("        object? sender, AvaloniaPropertyChangedEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (e.Property != NumericUpDown.ValueProperty)");
        sb.AppendLine("            return;");
        sb.AppendLine("        if (_syncing || _config is null)");
        sb.AppendLine("            return;");
        AppendPairWriteBack(sb, "_max", "_config", maxMember);
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDataContextChanged(EventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnDataContextChanged(e);");
        sb.AppendLine("        if (VisualRoot is not null)");
        sb.AppendLine("            Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnAttachedToVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        base.OnAttachedToVisualTree(e);");
        sb.AppendLine("        Rebind(DataContext);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            "    protected override void OnDetachedFromVisualTree(");
        sb.AppendLine(
            "        VisualTreeAttachmentEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine("        base.OnDetachedFromVisualTree(e);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Rebind(object? dataContext)");
        sb.AppendLine("    {");
        sb.AppendLine("        Unbind();");
        sb.AppendLine("        var config = RandomizerConfigResolver.Resolve(dataContext);");
        sb.AppendLine("        if (config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine("        _config = config;");
        sb.AppendLine(
            "        _configPropertyChanged = OnConfigPropertyChanged;");
        sb.AppendLine("        _config.PropertyChanged += _configPropertyChanged;");
        sb.AppendLine("        PushConfigToControl();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void Unbind()");
        sb.AppendLine("    {");
        sb.AppendLine(
            "        if (_config is not null && _configPropertyChanged is not null)");
        sb.AppendLine("        {");
        sb.AppendLine("            _config.PropertyChanged -= _configPropertyChanged;");
        sb.AppendLine("        }");
        sb.AppendLine("        _config = null;");
        sb.AppendLine("        _configPropertyChanged = null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void OnConfigPropertyChanged(");
        sb.AppendLine("        object? sender, PropertyChangedEventArgs e)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (e.PropertyName is not (ConfigMinPropertyName or " +
            "ConfigMaxPropertyName))");
        sb.AppendLine("            return;");
        sb.AppendLine("        PushConfigToControl();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private void PushConfigToControl()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_config is null)");
        sb.AppendLine("            return;");
        sb.AppendLine("        _syncing = true;");
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            _min.Value = _config.{minMember.PropertyName};");
        sb.AppendLine($"            _max.Value = _config.{maxMember.PropertyName};");
        sb.AppendLine(
            "            // Cross-bounds: each box's far edge follows the sibling field,");
        sb.AppendLine(
            "            // replacing the hand-written Maximum/Minimum bindings.");
        sb.AppendLine(
            $"            _min.Maximum = {NullableBoundExpression(maxMember, "_config", maxMember.Maximum)};");
        sb.AppendLine(
            $"            _max.Minimum = {NullableBoundExpression(minMember, "_config", minMember.Minimum)};");
        sb.AppendLine("        }");
        sb.AppendLine("        finally");
        sb.AppendLine("        {");
        sb.AppendLine("            _syncing = false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the config write-back for one Min/Max box: nullable members store a
    /// cleared box as null (the "roll randomly" state), non-nullable members only
    /// write back when a value is present.
    /// </summary>
    private static void AppendPairWriteBack(
        StringBuilder sb,
        string box,
        string config,
        NumericPairMemberInfo member)
    {
        if (member.IsNullable)
        {
            sb.AppendLine($"        {config}.{member.PropertyName} = (int?){box}.Value;");
        }
        else
        {
            sb.AppendLine($"        if ({box}.Value is {{ }} value)");
            sb.AppendLine($"            {config}.{member.PropertyName} = (int)value;");
        }
    }

    private static string NullableBoundExpression(
        NumericPairMemberInfo member,
        string config,
        int fallback)
    {
        return member.IsNullable
            ? $"{config}.{member.PropertyName} ?? {fallback}"
            : $"{config}.{member.PropertyName}";
    }

    /// <summary>
    /// Finds the known RandomizerCore config plus UI-side types that expose it through
    /// either Config or Main.Config.
    /// </summary>
    private static Dictionary<string, ConfigResolverInfo> BuildConfigResolvers(
        Compilation compilation,
        INamedTypeSymbol configSymbol)
    {
        var resolvers = new Dictionary<string, ConfigResolverInfo>(
            StringComparer.Ordinal);

        // The Config type itself comes from RandomizerCore, while the view models
        // that expose it live in the consuming UI compilation.
        foreach (var candidate in AllTypes(compilation, configSymbol.ContainingAssembly))
        {
            if (candidate.TypeKind != TypeKind.Class ||
                candidate.IsAbstract ||
                candidate.IsGenericType)
            {
                continue;
            }

            var display = candidate.ToDisplayString(FullyQualified);

            if (SymbolEqualityComparer.Default.Equals(candidate, configSymbol))
            {
                if (!resolvers.ContainsKey(display))
                    resolvers.Add(display, new ConfigResolverInfo(candidate, "value"));
                continue;
            }

            var configProperty = PublicInstanceProperty(candidate, "Config");

            if (configProperty is not null &&
                SymbolEqualityComparer.Default.Equals(
                    configProperty.Type,
                    configSymbol))
            {
                if (!resolvers.ContainsKey(display))
                    resolvers.Add(
                        display,
                        new ConfigResolverInfo(candidate, "value.Config"));

                continue;
            }

            var mainProperty = PublicInstanceProperty(candidate, "Main");

            if (mainProperty?.Type is not INamedTypeSymbol mainType)
                continue;

            var mainConfig = PublicInstanceProperty(mainType, "Config");

            if (mainConfig is not null &&
                SymbolEqualityComparer.Default.Equals(
                    mainConfig.Type,
                    configSymbol))
            {
                if (!resolvers.ContainsKey(display))
                    resolvers.Add(
                        display,
                        new ConfigResolverInfo(candidate, "value.Main.Config"));
            }
        }

        return resolvers;
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

    private static IPropertySymbol? PublicInstanceProperty(
        INamedTypeSymbol type,
        string name)
    {
        return type.GetMembers(name)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(
                p =>
                    p.DeclaredAccessibility == Accessibility.Public &&
                    !p.IsStatic &&
                    p.GetMethod is not null);
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
    /// <c>[UiNumericPair(label, partner)]</c> matching info when the field is one end
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

            if (attribute.ConstructorArguments.Length == 2 &&
                attribute.ConstructorArguments[0].Value is string label &&
                attribute.ConstructorArguments[1].Value is string partner)
            {
                candidate.Pair = (label, partner);
            }

            break;
        }

        return true;
    }

    /// <summary>
    /// Turns the collected numeric fields into generated controls. Pair endpoints
    /// merge into a single control named after the "Min" field minus its trailing
    /// "Min" (e.g. two fields <c>PalacesToCompleteMin</c>/<c>Max</c> become
    /// <c>PalacesToComplete</c>); other pairs fall back to the "Max" field. Singles
    /// are opt-in by caption resource, like ComboBoxes.
    /// </summary>
    private static IEnumerable<ControlFieldInfo> ResolveNumericCandidates(
        List<NumericCandidate> candidates,
        HashSet<string> resourceNames)
    {
        var byName = new Dictionary<string, NumericCandidate>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
            byName.Add(candidate.PropertyName, candidate);

        var used = new HashSet<string>(StringComparer.Ordinal);

        foreach (var candidate in candidates
                     .OrderBy(c => c.PropertyName, StringComparer.Ordinal))
        {
            if (!used.Add(candidate.PropertyName))
                continue;

            if (candidate.Pair is { } pair &&
                byName.TryGetValue(
                    Pascalize(pair.Partner),
                    out var partner) &&
                partner.Pair is { } partnerPair &&
                Pascalize(partnerPair.Partner) == candidate.PropertyName)
            {
                used.Add(partner.PropertyName);

                // The "Min"-suffixed member is the low end regardless of its label
                // (labels are free-form: "Min"/"Max" in Palaces, "Start Min"/"Start
                // Max" in Start).
                var minEnd = candidate.PropertyName.EndsWith(
                        "Min",
                        StringComparison.Ordinal)
                    ? candidate
                    : partner;
                var maxEnd = ReferenceEquals(minEnd, candidate) ? partner : candidate;

                yield return new ControlFieldInfo
                {
                    PropertyName = PairControlName(minEnd, maxEnd),
                    Kind = ControlKind.NumericPair,
                    PairMembers = new List<NumericPairMemberInfo>
                    {
                        new()
                        {
                            PropertyName = minEnd.PropertyName,
                            Label = minEnd.Pair!.Value.Label,
                            Minimum = minEnd.Minimum,
                            Maximum = minEnd.Maximum,
                            IsNullable = minEnd.IsNullable,
                        },
                        new()
                        {
                            PropertyName = maxEnd.PropertyName,
                            Label = maxEnd.Pair!.Value.Label,
                            Minimum = maxEnd.Minimum,
                            Maximum = maxEnd.Maximum,
                            IsNullable = maxEnd.IsNullable,
                        },
                    },
                };

                continue;
            }

            // Singles are opt-in by caption, mirroring the ComboBox rule: until a
            // Resources.<PropertyName> caption exists the view keeps its hand-written
            // NumericUpDown.
            if (!resourceNames.Contains(candidate.PropertyName))
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
            };
        }
    }

    private static string PairControlName(
        NumericCandidate minEnd,
        NumericCandidate maxEnd)
    {
        if (minEnd.PropertyName.EndsWith("Min", StringComparison.Ordinal))
            return minEnd.PropertyName[..^"Min".Length];

        if (maxEnd.PropertyName.EndsWith("Max", StringComparison.Ordinal))
            return maxEnd.PropertyName[..^"Max".Length];

        return maxEnd.PropertyName;
    }

    private static string Pascalize(string name)
        => char.ToUpperInvariant(name[0]) + name[1..];

    /// <summary>
    /// True when <c>Enums</c> exposes a public static <c>&lt;property&gt;List</c>,
    /// which a generated ComboBox binds as its ItemsSource.
    /// </summary>
    private static bool HasEnumList(
        INamedTypeSymbol enumsType,
        string propertyName)
    {
        return enumsType.GetMembers(propertyName + "List")
            .OfType<IPropertySymbol>()
            .Any(p =>
                p.IsStatic &&
                p.DeclaredAccessibility == Accessibility.Public);
    }

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
    public Dictionary<string, ConfigResolverInfo> Resolvers { get; set; } = new(StringComparer.Ordinal);
    public List<IncludedResolverInfo> IncludedResolvers { get; set; } = new();
}

public enum ControlKind
{
    CheckBox,
    ComboBox,
    Numeric,
    NumericPair
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

    // Non-null only when this control actually auto-binds IsEnabled.
    public string? IncludedResolverMethod { get; set; }

    // The config field carries [DifficultyOnly]: apply the "DifficultyOnly" style
    // class while ShareSeedAcrossDifficulty is on, instead of every view repeating
    // Classes.DifficultyOnly="{Binding Config.ShareSeedAcrossDifficulty}".
    public bool IsDifficultyOnly { get; set; }

    // Single numeric (ControlKind.Numeric): bounds from the config's [Minimum]/
    // [Maximum], pushed into the generated NumericUpDown.
    public int? NumericMinimum { get; set; }
    public int? NumericMaximum { get; set; }
    public bool IsNumericNullable { get; set; }

    // Min/Max pair (ControlKind.NumericPair): [0] is the Min box, [1] the Max box.
    // PropertyName is the generated control's name (Min field minus its "Min"
    // suffix) and the optional Resources.<PropertyName> caption/tooltip keys.
    public List<NumericPairMemberInfo>? PairMembers { get; set; }
}

/// <summary>
/// One NumericUpDown within a generated Min/Max pair control.
/// </summary>
public sealed class NumericPairMemberInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public bool IsNullable { get; set; }
}

/// <summary>
/// A collected int/int? config field awaiting pairing/emission.
/// </summary>
public sealed class NumericCandidate
{
    public string PropertyName { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public (string Label, string Partner)? Pair { get; set; }
    public bool IsDifficultyOnly { get; set; }
    public string? TooltipOverride { get; set; }
}

public sealed class ConfigResolverInfo
{
    public ConfigResolverInfo(
        INamedTypeSymbol ownerType,
        string configExpression)
    {
        OwnerType = ownerType;
        DataContextType = ownerType.ToDisplayString(FlagsControlsGenerator.FullyQualified);
        ConfigExpression = configExpression;
    }

    /// <summary>The DataContext view model type, kept as a symbol for later inspection.</summary>
    public INamedTypeSymbol OwnerType { get; }

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
