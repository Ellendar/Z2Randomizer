using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class PalacesViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public IObservable<bool> BossRoomsExitTypeIncludedObservable { get; }
    public IObservable<bool> NoDuplicateRoomsByLayoutIncludedObservable { get; }
    public IObservable<bool> NoDuplicateRoomsByEnemiesIncludedObservable { get; }
    public IObservable<bool> RandomStylesAllowVanillaIncludedObservable { get; }
    public IObservable<bool> RemoveLongDeadEndsIncludedObservable { get; }
    public IObservable<bool> IncludeVanillaRoomsIncludedObservable { get; }
    public IObservable<bool> Includev4_0RoomsIncludedObservable { get; }
    public IObservable<bool> Includev5_0RoomsIncludedObservable { get; }
    public IObservable<bool> IncludeExpertRoomsIncludedObservable { get; }
    public IObservable<bool> BlockingRoomsInAnyPalaceIncludedObservable { get; }
    public IObservable<bool> RemoveTBirdIncludedObservable { get; }
    public IObservable<bool> TBirdRequiredIncludedObservable { get; }

    public PalacesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        BossRoomsExitTypeIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.bossRoomsExitTypeIncluded())
            .DistinctUntilChanged();

        NoDuplicateRoomsByLayoutIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.noDuplicateRoomsByLayoutIncluded())
            .DistinctUntilChanged();

        NoDuplicateRoomsByEnemiesIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.noDuplicateRoomsByEnemiesIncluded())
            .DistinctUntilChanged();

        RandomStylesAllowVanillaIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.randomStylesAllowVanillaIncluded())
            .DistinctUntilChanged();

        RemoveLongDeadEndsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.removeLongDeadEndsIncluded())
            .DistinctUntilChanged();

        IncludeVanillaRoomsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.includeVanillaRoomsIncluded())
            .DistinctUntilChanged();

        Includev4_0RoomsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.includev4_0RoomsIncluded())
            .DistinctUntilChanged();

        Includev5_0RoomsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.includev5_0RoomsIncluded())
            .DistinctUntilChanged();

        IncludeExpertRoomsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.includeExpertRoomsIncluded())
            .DistinctUntilChanged();

        BlockingRoomsInAnyPalaceIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.blockingRoomsInAnyPalaceIncluded())
            .DistinctUntilChanged();

        RemoveTBirdIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.removeTBirdIncluded())
            .DistinctUntilChanged();

        TBirdRequiredIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.tBirdRequiredIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
