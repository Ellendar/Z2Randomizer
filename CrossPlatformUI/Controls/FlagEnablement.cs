using System;
using ReactiveUI.Primitives;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Controls;

/// <summary>
/// Builds the <c>IsEnabled</c> observable for a flag whose enablement is a pure
/// function of the config: the predicate is re-evaluated every time the host's flags
/// change. Returns null when the DataContext is not an <see cref="ViewModels.IFlagHost"/>,
/// which the binding treats as "no gating".
/// </summary>
internal static class FlagEnablement
{
    public static IObservable<bool>? Build(
        object? dataContext,
        Func<RandomizerConfiguration, bool> predicate)
        => dataContext is ViewModels.IFlagHost host
            ? host.FlagsChanged
                .Select(_ => predicate(host.Config))
                .DistinctUntilChanged()
            : null;
}
