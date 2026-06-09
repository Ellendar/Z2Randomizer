using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Town;

public class TownMap
{
    public string Name { get; set; } = "";
    public int Map { get; set; }
    public bool IsInternalLocation { get; }
    public SideviewEditable<TownObject> Sideview { get; }

    public Collectable? Collectable { get; set; }
    public bool CollectableIsShufflable { get; set; }
    public TownMap? Left { get; set; }
    public TownMap? Right { get; set; }
    public TownMap? Door0 { get; set; }
    public TownMap? Door1 { get; set; }
    public TownMap? Door2 { get; set; }
    public TownMap? Door3 { get; set; }

    //WTF should the script look like

    //For now, all screens that have collectables don't have separate requirements between access and collection.
    //This may change in the future, which will require a bit of refactoring
    public Requirements AccessRequirements { get; set; } = Requirements.NONE;
    public EnemiesEditable<EnemiesTown> Enemies { get; }

    //Some way to store what the Left/Right exit connection should be if it connects to the overworld.
    //I'm fairly certain FC vs FD matters for saria, but I need to test it, if it doesn't matter it can just always be FC
    //For now i'm assuming it doesn't matter and any null left/right connection is an external connection FC
    //We can just test it when we have writing working and modify if needed
    public bool LeftExitIsOutside { get; set; }
    public bool RightExitIsOutside { get; set; }

    public TownMap(int mapNumber, byte[] sideviewsRaw, byte[] enemiesRaw, bool isInternalLocation)
    {
        Sideview = new(sideviewsRaw);
        Enemies = new(enemiesRaw);
        Map = mapNumber;
        IsInternalLocation = isInternalLocation;
    }
}