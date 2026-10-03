using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReactiveUI;
using RUISG = ReactiveUI.SourceGenerators;
using ReactiveUI.Primitives;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class RemovePresetViewModel : ReactiveObject
{
    [RUISG.Reactive]
    public partial string TargetName { get; set; } = "";

    public MainViewModel Main { get; }

    public ReactiveCommand<RxVoid, RxVoid> Confirm { get; }
    public ReactiveCommand<RxVoid, RxVoid> Cancel { get; }

    public RemovePresetViewModel(MainViewModel main)
    {
        Main = main;

        Confirm = ReactiveCommand.Create(() =>
        {
            var item = Main.SaveNewPresetViewModel.SavedPresets
                .FirstOrDefault(x => x.Preset == TargetName);
            if (item is not null)
            {
                Main.SaveNewPresetViewModel.SavedPresets.Remove(item);
            }
            Main.RemovePresetDialogOpen = false;
        });

        Cancel = ReactiveCommand.Create(() =>
        {
            Main.RemovePresetDialogOpen = false;
        });
    }
}
