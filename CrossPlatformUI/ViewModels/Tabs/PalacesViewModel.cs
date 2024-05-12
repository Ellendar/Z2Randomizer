using ReactiveUI;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels.Tabs;

public class PalacesViewModel(MainViewModel main) : ReactiveObject
{
    public MainViewModel Main { get; } = main;
    public RandomizerConfiguration Config { get; } = main.Config;
}