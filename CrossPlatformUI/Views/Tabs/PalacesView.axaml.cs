using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using CrossPlatformUI.ViewModels;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Views.Tabs;

public partial class PalacesView : ReactiveUserControl<MainViewModel>
{
    public PalacesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(disposables =>
    {
            CheckBox noDuplicateRoomsByLayoutCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByEnemiesCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox noDuplicateRoomsByEnemiesCheckbox = this.FindControl<CheckBox>("NoDuplicateRoomsByLayoutCheckbox") ?? throw new System.Exception("Missing Required Validation Element");

            IObservable<bool?> byLayoutObservable = noDuplicateRoomsByLayoutCheckbox.GetObservable(CheckBox.IsCheckedProperty);
            IObservable<bool?> byEnemiesObservable = noDuplicateRoomsByEnemiesCheckbox.GetObservable(CheckBox.IsCheckedProperty);

            byLayoutObservable.Subscribe(byLayoutValue =>
            {
                if (byLayoutValue ?? false)
                {
                    noDuplicateRoomsByEnemiesCheckbox.IsChecked = false;
    }
            })
            .DisposeWith(disposables);

            byEnemiesObservable.Subscribe(byEnemiesValue =>
    {
                if (byEnemiesValue ?? false)
                {
        noDuplicateRoomsByLayoutCheckbox.IsChecked = false;
    }
            })
            .DisposeWith(disposables);

            ComboBox normalPalaceStyleSelector = this.FindControl<ComboBox>("NormalPalaceStyleSelector") ?? throw new System.Exception("Missing Required Validation Element");
            ComboBox gpStyleSelector = this.FindControl<ComboBox>("GPPalaceStyleSelector") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox thunderbirdRequiredCheckbox = this.FindControl<CheckBox>("TbirdRequiredCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox includeVanillaCheckbox = this.FindControl<CheckBox>("IncludeVanillaRoomsCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox include4_0Checkbox = this.FindControl<CheckBox>("Include4_0RoomsCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            CheckBox include5_0Checkbox = this.FindControl<CheckBox>("Include5_0RoomsCheckbox") ?? throw new System.Exception("Missing Required Validation Element");

            CheckBox blockingRoomsAnywhereCheckbox = this.FindControl<CheckBox>("BlockingRoomsInAnyPalaceCheckbox") ?? throw new System.Exception("Missing Required Validation Element");
            ComboBox bossRoomsExitTypeSelector = this.FindControl<ComboBox>("BossRoomsExitTypeSelector") ?? throw new System.Exception("Missing Required Validation Element");


            IObservable<object> normalStyleObservable = normalPalaceStyleSelector.GetObservable(ComboBox.SelectedItemProperty);
            var gpStyleObservable = gpStyleSelector.GetObservable(ComboBox.SelectedItemProperty);

            gpStyleObservable.Subscribe(selectedItem =>
    {
                EnumDescription? selectedDescription = selectedItem as EnumDescription;
                if(selectedDescription != null)
                {
                    PalaceStyle palaceStyle = (PalaceStyle)(selectedDescription.Value ?? PalaceStyle.RECONSTRUCTED);
                    if (palaceStyle == PalaceStyle.VANILLA)
                    {
                        thunderbirdRequiredCheckbox.IsChecked = true;
                        thunderbirdRequiredCheckbox.IsEnabled = false;
    }
                    else
                    {
                        thunderbirdRequiredCheckbox.IsEnabled = true;
}
                }
            });

            normalStyleObservable.CombineLatest(gpStyleObservable, (normal, gp) =>
            {
                EnumDescription? normalStyleDescription = normal as EnumDescription;
                PalaceStyle normalPalaceStyle = (PalaceStyle)(normalStyleDescription?.Value ?? PalaceStyle.RECONSTRUCTED);
                EnumDescription? gpPalaceStyleDescription = gp as EnumDescription;
                PalaceStyle gpPalaceStyle = (PalaceStyle)(gpPalaceStyleDescription?.Value ?? PalaceStyle.RECONSTRUCTED);
                return !((normalPalaceStyle == PalaceStyle.VANILLA || normalPalaceStyle == PalaceStyle.SHUFFLED)
                    && (gpPalaceStyle == PalaceStyle.VANILLA || gpPalaceStyle == PalaceStyle.SHUFFLED));
            })
            .Subscribe(enableRoomSelection =>
            {
                includeVanillaCheckbox.IsEnabled = enableRoomSelection;
                include4_0Checkbox.IsEnabled = enableRoomSelection;
                include5_0Checkbox.IsEnabled = enableRoomSelection;
                noDuplicateRoomsByLayoutCheckbox.IsEnabled = enableRoomSelection;
                noDuplicateRoomsByEnemiesCheckbox.IsEnabled = enableRoomSelection;
                blockingRoomsAnywhereCheckbox.IsEnabled = enableRoomSelection;
                bossRoomsExitTypeSelector.IsEnabled = enableRoomSelection;

                if (!enableRoomSelection)
                {
                    includeVanillaCheckbox.IsChecked = true;
                    include4_0Checkbox.IsChecked = false;
                    include5_0Checkbox.IsChecked = false;
                    noDuplicateRoomsByLayoutCheckbox.IsChecked = enableRoomSelection;
                    noDuplicateRoomsByEnemiesCheckbox.IsChecked = enableRoomSelection;
                    blockingRoomsAnywhereCheckbox.IsChecked = enableRoomSelection;
                    bossRoomsExitTypeSelector.SelectedIndex = 0;
                }
            });
        });
    }

}
