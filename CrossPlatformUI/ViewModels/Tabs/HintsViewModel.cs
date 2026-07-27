using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

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

        HeaderBackgroundObservable = randomizerViewModel.ThemeVariantSubject
            .CombineLatest(alertFlags, (theme, alert) => ThemeHelper.GetFlagAlertBackgroundBrush(theme, alert));

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(MultipleDisposable disposables)
    {
    }
}
