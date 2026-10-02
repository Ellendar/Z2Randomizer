using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReactiveUI;
using RUISG = ReactiveUI.SourceGenerators;
using ReactiveUI.Primitives;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class UpdatePresetViewModel : ReactiveObject
{
    [RUISG.Reactive]
    public partial string TargetName { get; set; } = "";

    public MainViewModel Main { get; }

    public ReactiveCommand<RxVoid, RxVoid> Confirm { get; }
    public ReactiveCommand<RxVoid, RxVoid> Cancel { get; }

    public UpdatePresetViewModel(MainViewModel main)
    {
        Main = main;

        Confirm = ReactiveCommand.Create(() =>
        {
            var updatedPreset = new CustomPreset(TargetName, new RandomizerConfiguration(Main.Config.SerializeFlags()));
            var collection = Main.SaveNewPresetViewModel.SavedPresets;
            int presetIndex = -1;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Preset == TargetName) { presetIndex = i; break; }
            }
            if (presetIndex != -1)
            {
                collection[presetIndex] = updatedPreset;
            }
            Main.UpdatePresetDialogOpen = false;
        });

        Cancel = ReactiveCommand.Create(() =>
        {
            Main.UpdatePresetDialogOpen = false;
        });
    }
}
