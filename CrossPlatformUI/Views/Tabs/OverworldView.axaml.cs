using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class OverworldView : ReactiveUserControl<MainViewModel>
{
    public OverworldView()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(disposables =>
        {
            CheckBox generateThreeEyedRock = this.FindControl<CheckBox>("GenerateThreeEyedRock") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox generateHiddenKasutoTile = this.FindControl<CheckBox>("GenerateHiddenKasutoTile") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox shuffleWhichLocationsAreHidden = this.FindControl<CheckBox>("ShuffleWhichLocationsAreHidden") ?? throw new System.Exception("Missing Required Validation Element");

            var rockObservable = generateThreeEyedRock.GetObservable(CheckBox.IsCheckedProperty);
            var forestObservable = generateHiddenKasutoTile.GetObservable(CheckBox.IsCheckedProperty);

            rockObservable.CombineLatest(forestObservable, (rock, forest) => (rock ?? true) || (forest ?? true))
                .Subscribe(possibleHiddenEastTile =>
                {
                    shuffleWhichLocationsAreHidden.IsEnabled = possibleHiddenEastTile;
                    if (!possibleHiddenEastTile)
                    {
                        shuffleWhichLocationsAreHidden.IsChecked = false;
                    }
                })
                .DisposeWith(disposables);
        });
    }
}
