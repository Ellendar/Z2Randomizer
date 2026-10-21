using System;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

/// <summary>
/// The DataContext contract a flag-bound control needs: where to read and write the
/// <see cref="RandomizerConfiguration"/>, and a stream that ticks whenever the flag
/// set changes. Every view model that can host a shared <c>Z2*</c> control implements
/// this, either directly (the root <see cref="MainViewModel"/>) or by forwarding to it,
/// so the binding can resolve a DataContext with a single cast instead of a per-type
/// switch.
/// </summary>
public interface IFlagHost
{
    /// <summary>The config behind the flags this DataContext binds.</summary>
    RandomizerConfiguration Config { get; }

    /// <summary>Ticks when <see cref="Config"/>'s flag set changes.</summary>
    IObservable<RxVoid> FlagsChanged { get; }
}
