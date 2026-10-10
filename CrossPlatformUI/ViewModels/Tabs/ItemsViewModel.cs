using System;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class ItemsViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public IObservable<bool> TownQuestLocationsAreMinorItemsIncludedObservable { get; }

    public ItemsViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        TownQuestLocationsAreMinorItemsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.townQuestLocationsAreMinorItemsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(MultipleDisposable disposables)
    {
    }
}
