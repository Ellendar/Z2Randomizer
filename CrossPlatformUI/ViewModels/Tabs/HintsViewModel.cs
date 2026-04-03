using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Media;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class HintsViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }
    public IObservable<IBrush> HeaderBackgroundObservable { get; }

    public HintsViewModel(MainViewModel main, RandomizerViewModel randomizerViewModel)
    {
        Main = main;
        Activator = new();

        var alertFlags = Main.FlagsChanged
            .Select(_ => Main.Config.GenerateSpoiler)
            .DistinctUntilChanged();

        HeaderBackgroundObservable = randomizerViewModel.ThemeVariantSubject.CombineLatest(alertFlags)
            .Select(pair => ThemeHelper.GetFlagAlertBackgroundBrush(pair.First, pair.Second));

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
