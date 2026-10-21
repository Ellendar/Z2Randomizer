# Frontend / UI conventions (CrossPlatformUI)

## Contents

1. [Project layout](#1-project-layout)
2. [How a flag becomes a control](#2-how-a-flag-becomes-a-control)
3. [ReactiveUI conventions](#3-reactiveui-conventions)
4. [Flag controls in detail](#4-flag-controls-in-detail)
5. [Enabling and disabling controls](#5-enabling-and-disabling-controls)
6. [Adding a flag, end to end](#6-adding-a-flag-end-to-end)

---

## 1. Project layout

| Project | Role |
| --- | --- |
| `CrossPlatformUI/` | Shared Avalonia views and view models (contents listed below the table). |
| `CrossPlatformUI.Desktop/` | Avalonia desktop host. |
| `CrossPlatformUI.Browser/` | WebAssembly host. |
| `Desktop.Common/` | `DesktopJsEngine.cs` (ClearScript V8) and `DesktopFileService.cs`. |
| `CrossPlatformUI.SourceGenerator/` | Roslyn incremental generator. `FlagsControlsGenerator.cs` emits only the `FlagRegistry` metadata table, the `RandomizerConfigResolver` cast, a small `FlagIncludedResolver` (for hand-written enablement rules), and `FlagControlFactory`. |
| `CoreSourceGenerator/` | The main source generator used by `RandomizerCore`. `FlagsSerializeGenerator.cs` (reactive properties and flag-string serialization, including `Diff`) and `ConfigFlagGenerator.cs` (emits the `ConfigFlag` enum). |

**Inside `CrossPlatformUI/`:**

| Path | Contents |
| --- | --- |
| `Views/` | One `.axaml` per screen |
| `Views/Tabs/` | The tab pages |
| `ViewModels/` | View models |
| `ViewModels/Tabs/` | Tab-page view models |
| `Lang/Resources.resx` | Label and tooltip strings |
| `Presets/` | Preset definitions such as `StandardPreset` |
| `Behaviors/` | Avalonia behaviors |
| `Converters/` | Value converters |
| `Assets/` | Static assets |
| `Services/` | Shared services |
| `Controls/` | The core flag controls: `Z2CheckBox`, `Z2ComboBox`, `Z2NumericUpDown`, `FlagBinding`, `FlagEntry` |

---

## 2. How a flag becomes a control

A view refers to a flag **by name**:

```xml
<StackPanel xmlns:z="clr-namespace:CrossPlatformUI.Controls">
  <z:Z2CheckBox Flag="LegacyVanillaShuffledLocations" />
  <z:Z2ComboBox Flag="GpLength" />
  <z:Z2NumericUpDown Flag="AttackLevelCap" />
  <z:Z2NumericUpDown Flag="PalacesToCompleteMin" />
</StackPanel>
```

What happens behind that one line:

1. A public property is declared in `RandomizerConfiguration.cs`.
2. When you build, `FlagsSerializeGenerator` generates the reactive property
   and its flag-string serialization. `ConfigFlagGenerator` adds a matching
   `ConfigFlag` member.
3. `FlagsControlsGenerator` scans those properties, picks a control kind from
   the property type, and emits **one `FlagRegistry.g.cs` entry** per flag.
4. At runtime our custom control (`Controls/Z2*.cs`) looks up its registry
   entry. `FlagBinding` syncs it with `Config` on the DataContext, and
   `FlagEntry` supplies the per-flag metadata (label, tooltip, bounds, list,
   enablement rule).

### Which control a property uses

| Property type | Control | Extra requirements |
| --- | --- | --- |
| `bool` / `bool?` | `Z2CheckBox` | `Resources.<Prop>` and `Resources.<Prop>ToolTip` |
| any enum | `Z2ComboBox` | `Resources.<Prop>`, `Resources.<Prop>ToolTip`, and `Enums.<Prop>List` |
| `int` / `int?` with both `[Minimum]` and `[Maximum]` | `Z2NumericUpDown` | `Resources.<Prop>` box hint; a Min/Max pair also needs `Resources.<Prop>Caption` (see 4.4) |

### Attribute cheat sheet

Put these on the backing field in `RandomizerConfiguration`. The generator
forwards attributes it does not recognise onto the generated property
(`FlagsSerializeGenerator.GetPassThroughAttributes`), so an attribute used this
way must be declared with `Field` as a valid target.

| Attribute | Effect |
| --- | --- |
| `[ConditionallyIncludeInFlags]` | The flag is left out of the flag string when its `xIncluded()` predicate is false. Requires that predicate. |
| `[IgnoreInFlags]` | Keeps a property out of the serialized flag string (e.g. `DisableMusic`) but **still generates a control**. |
| `[IgnoreInFlagControls]` | Keeps a property out of the generated controls but **still serializes it**. Works for enums, `int`s and pair ends. Used for the four `NesColor` palette properties (`Tunic`, `SkinTone`, `TunicOutline`, `ShieldTunic`), which have their own `NesColorDropdown`. |
| `[Minimum(n)]` / `[Maximum(n)]` | Numeric bounds. These are the only source of truth for bounds. |
| `[UiNumericPair(nameof(partner))]` | Marks both ends of a Min/Max pair. |
| `[DifficultyOnly]` | Gives the control a read-only look while `ShareSeedAcrossDifficulty` is on. |
| `[TooltipResource("Key")]` | Points the tooltip at a shared resource key. See 4.3.2. |

---

## 3. ReactiveUI conventions

The UI uses **ReactiveUI 25** with **ReactiveUI.SourceGenerators**. View models
are `partial`.

- **Declare properties with `[RUISG.Reactive]`**:
  `[RUISG.Reactive] public partial T Prop { get; set; }`, not
  `RaiseAndSetIfChanged`.
- **Do not import `System.Reactive.Linq`.** Use `ReactiveUI.Primitives`, or the
  codebase's `SubscribeExtensions.Subscribe(obs, action).DisposeWith(disposables)`.
  That helper exists specifically to avoid the `System.Reactive.Linq` ambiguity.
- **`RandomizerCore` has its own `ReactiveAttribute`**, so the core needs no
  ReactiveUI dependency. Never import both into one file.

---

## 4. Flag controls in detail

### 4.1 The shared contract

**Compile-time checking.** Every `FlagEntry` member is a literal reference
(`Resources.X`, `c => c.X`, `Enums.XList`). A renamed property or missing resx
key fails the build. Nothing is reflection or a string-keyed lookup.

**Binding.** `FlagBinding` is a direct `INotifyPropertyChanged` bridge, not
reflection and not a string-path Avalonia binding. This avoids depending on
ReactiveUI's generated binding members across generator boundaries. It binds on
both `OnDataContextChanged` and `OnAttachedToVisualTree`, and unbinds on detach.

**DataContext resolution.** The control resolves its config with a single cast:
`RandomizerConfigResolver.Resolve` is `(dataContext as IFlagHost)?.Config`.
`IFlagHost` (`CrossPlatformUI/ViewModels/IFlagHost.cs`) exposes `Config` and
`FlagsChanged`. A DataContext that doesn't implement it resolves to `null`, and
**the control silently does nothing**.

| DataContext | How it gets there |
| --- | --- |
| `MainViewModel` | Implements `IFlagHost` directly. |
| `SpritePreviewViewModel` (inside `CustomizeView`) | Forwards to its `Main` through explicit interface members, so it adds no public surface. |
| Views with no state of their own (`Biomes`, `Hints`, `Items`, `Palaces`, `Spells`, `Customize`, `Drops`, `Enemies`, `Levels`, `Overworld`, `Start`) | Bind `DataContext` straight to `Main` and have no view model. |
| `PresetsViewModel` | Creates its controls in code and sets each control's `DataContext` explicitly. It does not implement the interface. |

Because the three control types are shared, a style selector can no longer
target one flag's class. Use `Classes` instead, which is how `DifficultyOnly`
works.

**`[DifficultyOnly]` styles itself.** When the config field carries
`[DifficultyOnly]`, the control watches `ShareSeedAcrossDifficulty` and adds or
removes the `DifficultyOnly` class, which `App.axaml`'s `.DifficultyOnly`
selectors use for the read-only look.

**`FlagControlFactory.Create(ConfigFlag)`** is for callers that only know a flag
at runtime. It returns the control for that flag, or `null` when the flag has no
flag control (a serialization-only member: an `[IgnoreInFlagControls]` property,
or a field type that isn't a control).

Its inputs are exactly the values `RandomizerConfiguration.Diff` reports in
`Field`.

**What is still hand-written.** Not every control in the views is a flag
control:

- The "shuffle all experience" grouping in `LevelsView`.
- The `Character Sprite` combo in `SpritePreviewView`: preview UI state with a
  custom item template, not a flag.

### 4.2 Checkboxes (`Z2CheckBox`)

- Applies to every `bool` / `bool?` property.
- A `bool?` is tri-state; the indeterminate state means "roll randomly".
- Label and tooltip come from the matching `Resources` entries (see section 6,
  step 2).

### 4.3 Combo boxes (`Z2ComboBox`)

Every enum-typed public property gets a `ConfigFlag` member and is served by
`Z2ComboBox`.

**4.3.1 Required symbols**

The generator references these symbols **unconditionally**, so a missing one is
a build error that names it:

- `Resources.<PropertyName>`
- `Resources.<PropertyName>ToolTip`
- `Enums.<PropertyName>List`

**4.3.2 Shared tooltips (`[TooltipResource]`)**

By default the tooltip key is `Resources.<Prop>ToolTip`. When several flags
genuinely share one tooltip, put `[TooltipResource("KeyName")]` on the config's
backing **field**.

**4.3.3 Two-column ComboBoxes**

If any member of the enum has a `;` in its `[Description]`
(`[Description("Long name;Extra info")]`), the ComboBox will use two columns.
Without a `;` anywhere, the default one-line template is used.

**4.3.4 Why not derive the list from the enum type?**

Several lists apply a per-flag filter where not all enum values are valid:

| List | Filter |
| --- | --- |
| `BossLifeOptionList` | Excludes `EnemyLifeOption.WIDE`. |
| `WestBiomeList` / `EastBiomeList` / `DmBiomeList` / `MazeBiomeList` | Filter by region. |
| The `West`/`East`/`Dm`/`Maze` climate lists | Filter the same way. |

### 4.4 Numeric controls (`Z2NumericUpDown`)

An `int` / `int?` property with both `[Minimum(n)]` and `[Maximum(n)]` gets a
registry entry served by `Z2NumericUpDown`.

**4.4.1 Min/Max pairs**

Both fields carry `[UiNumericPair(nameof(partner))]`.

- `Z2NumericUpDown.ApplyCrossBound` sets the low box's `Maximum` from the high
  box's live value, and vice versa. If the partner is cleared, it falls back to
  the partner's hard bound.
- A Min/Max pair needs three resource keys: `<Name>` is the box hint
  (compile-time, so a missing key fails the build), `<Name>Caption` labels it
  in the preset diff, and one shared tooltip `<MinName>ToolTip` serves both
  boxes. The caption and tooltip are lazy lookups, so a missing key just means
  no caption or no tooltip.

**Numerics cannot be disabled.** `ApplyGeneratedEnablers` skips numerics.

---

## 5. Enabling and disabling controls

A control gets its `IsEnabled` state purely by **naming convention**: no
attribute is needed.

### 5.1 The tri-state rule: `!= false`, never `== true`

Most flag properties are `bool?`, and for those **`null` is a third state, not
"off"**. It means *indeterminate*: the generator still rolls the option (50/50,
or whatever ratio is configured). So an option whose parent is indeterminate is
still **live**, and the user must be able to reach it.

The correct predicate is `Config.X != false`.

**Prefer the existing `xIncluded()` accessors** on `RandomizerConfiguration`
over hand-writing the comparison. They are `!= false` tests, sometimes composed
(`allowUnsafePathEncountersIncluded()` is
`shuffleEncountersIncluded() && shuffleEncounters != false`). They matter
because `FlagsSerializeGenerator` emits `if (xIncluded())` around
serialization of every `[ConditionallyIncludeInFlags]` field. The rule that
decides whether a control is live and the rule that decides whether the flag is
written are therefore *required to agree*.

### 5.2 Rules the generator derives for you

If `RandomizerConfiguration` declares a predicate `fooIncluded()`, the generator
inlines the rule into the registry entry. A `[ConditionallyIncludeInFlags]`
field always has one, because the serializer would not compile without it.

- The predicate's **presence** is the opt-in. The
  `[ConditionallyIncludeInFlags]` attribute is not used by the UI code, so you
  can use the same logic for non-flag options (like the music options).
- The lookup handles the casing difference: properties are PascalCase
  (`RemoveLongDeadEnds`), predicates are camelCase
  (`removeLongDeadEndsIncluded()`).

---

## 6. Adding a flag, end to end

1. **Add the property** to `RandomizerConfiguration.cs`.
   - Add `[ConditionallyIncludeInFlags]` if the flag is conditional. This also
     requires an `xIncluded()` predicate.
   - For an `int` / `int?`, add `[Minimum(n)]` and `[Maximum(n)]`. Bounds come
     only from these.
   - For a Min/Max pair, put `[UiNumericPair(nameof(partner))]` on **both**
     fields.
2. **Add the resources** to `CrossPlatformUI/Lang/Resources.resx`. A checkbox
   or enum always requires <Name> and <Name>ToolTip; both are compile-time
   references, so a missing key fails the build.

   For a Min/Max pair, also add <Name>Caption (for the caption above the pair).
3. **For an enum, add its list** to `RandomizerCore/EnumTypes.cs`, named
   `<Name>List`. Add any per-flag filtering there.

4. **Add the element** to the right `Views/Tabs/*.axaml`:

   ```xml
   <z:Z2CheckBox Flag="<Name>" />
   <z:Z2ComboBox Flag="<Name>" />
   <z:Z2NumericUpDown Flag="<Name>" />
   ```

   The root element needs `xmlns:z="clr-namespace:CrossPlatformUI.Controls"`.

5. **Build project.** A missing resource key or enum list fails the build and
   names the missing symbol. In this case, see steps 1-3 above.

6. **Check enablement.**
   - If a `fooIncluded()` predicate exists, enablement is derived automatically,
     for every control kind.
   - Otherwise add a `<Name>EnabledObservable` on the owning view model, or keep
     a plain `IsEnabled="{Binding ...}"` binding for a simple view-model
     property. A plain `{Binding Config.X}` on a `bool?` is a bug (see 5.1).
   - **Do not** add an `EnabledObservable` for a numeric. They are currently
     never disabled.
