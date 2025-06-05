// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Sidescroll;

StringBuilder sb = new StringBuilder("");

void ValidateRoomsForFile(string filename)
{
    sb.AppendLine("Validating \"" + filename + "\"...");
    var json = Util.ReadAllTextFromFile(filename);
    var palaceRooms = JsonSerializer.Deserialize(json, RoomSerializationContext.Default.ListRoom);

    var duplicateNames = palaceRooms!
        .Where(o => o.Name.Length > 0)
        .GroupBy(o => o.Name)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);
    foreach (var name in duplicateNames) { sb.AppendLine($"{name}: room name is used more than once."); }

    // Checking that all room data is conforming to the structure
    // (It would be very bad if any of the length bytes were wrong.)
    foreach (var room in palaceRooms!.Where(r => IsActuallyEnabled(palaceRooms!, r)))
    {
        try
        {
            // From testing: the game can handle the full 255 bytes of sideview map commands
            var sv = new SideviewEditable<PalaceObject>(room.SideView);
        }
        catch (Exception e) { sb.AppendLine($"{GetName(room)}: Room enemies data error. {e.Message}"); }

        try
        {
            // From testing: a room with 12 enemies crashes the game. 11 seemed to work.
            // In the vanilla palace rooms, the max amount of enemies is 9
            var ee = new EnemiesEditable<EnemiesPalace125>(room.Enemies);
            if (ee.Enemies.Count > 9) { sb.AppendLine($"{GetName(room)}: Room has too many enemies ({ee.Enemies.Count})."); }
        }
        catch (Exception e) { sb.AppendLine($"{GetName(room)}: Room enemies data error. {e.Message}"); }

        if (room.Connections.Length != 4) { sb.AppendLine($"{GetName(room)}: Room Connections data error."); }
    }

    // REGULAR PALACE ROOMS
    foreach (var room in palaceRooms!.Where(r => r.PalaceNumber != 7 && IsActuallyEnabled(palaceRooms!, r)))
    {
        var sv = new SideviewEditable<PalaceObject>(room.SideView);
        // checking Enabled here to skip linked rooms
        if (sv.HasItem() != room.HasItem && room.Enabled) { sb.AppendLine($"{GetName(room)}: Room HasItem mismatch."); }

        if (sv.BackgroundMap != 0) { continue; /* Not supporting built-in "background" maps */ }
        var ee = new EnemiesEditable<EnemiesPalace125>(room.Enemies);

        // a lot can probably be rewritten to use this now instead of doing their own checks
        bool[,] solidGrid = sv.CreateSolidGrid();

        CheckLeftExit(room, solidGrid);
        CheckRightExit(room, sv.PageCount, solidGrid);
        CheckDoorsAndItems(room, sv, ee);

        if (room.IsBossRoom)
        {
            var statue = sv.Find(o => o.Id == PalaceObject.IRON_KNUCKLE_STATUE && o.AbsX == 62 && o.Y == 9);
            if (statue == null) { Warning(room, "BossRoomMissingStatue", "Boss room is missing exit statue."); }
            bool hasRightOpeningWall = FindTileOpeningAtX(solidGrid, (sv.PageCount * 16) - 1);
            if (!hasRightOpeningWall) { Warning(room, "BossRoomLowCeiling", "Boss room has too low ceiling at exit, preventing re-entry."); }
        }

        SortedSet<int> openCeilingTiles = [];
        SortedSet<int> dropTiles = [];

        AddOpenCeilingTiles(sv, solidGrid, openCeilingTiles);
        AddDropTiles(sv, solidGrid, dropTiles);

        var lavaPits = sv.FindAll(o => o.IsLava());
        CheckDropsOverLava(room, sv, lavaPits, openCeilingTiles);
        RemoveTilesThatAreLavaPits(lavaPits, dropTiles);

        var elevators = sv.FindAll(o => o.IsElevator());
        CheckElevators(room, sv, solidGrid, elevators, openCeilingTiles);
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);
        }
        CheckDropZones(room, openCeilingTiles);
        CheckDrops(room, dropTiles);
    }

    // GREAT PALACE ROOMS
    foreach (var room in palaceRooms!.Where(r => r.PalaceNumber == 7 && IsActuallyEnabled(palaceRooms!, r)))
    {
        var sv = new SideviewEditable<GreatPalaceObject>(room.SideView);

        if (sv.BackgroundMap != 0) { continue; /* Not supporting built-in "background" maps */ }
        var ee = new EnemiesEditable<EnemiesGreatPalace>(room.Enemies);

        // a lot can probably be rewritten to use this now instead of doing their own checks
        bool[,] solidGrid = sv.CreateSolidGrid();

        CheckLeftExit(room, solidGrid);
        CheckRightExit(room, sv.PageCount, solidGrid);
        CheckDoorsAndItems(room, sv, ee);

        SortedSet<int> openCeilingTiles = [];
        SortedSet<int> dropTiles = [];

        AddOpenCeilingTiles(sv, solidGrid, openCeilingTiles);
        AddDropTiles(sv, solidGrid, dropTiles);

        var lavaPits = sv.FindAll(o => o.IsLava());
        CheckDropsOverLava(room, sv, lavaPits, openCeilingTiles);
        RemoveTilesThatAreLavaPits(lavaPits, dropTiles);

        var elevators = sv.FindAll(o => o.IsElevator());
        CheckElevators(room, sv, solidGrid, elevators, openCeilingTiles);
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);
        }
        CheckDropZones(room, openCeilingTiles);
        CheckDrops(room, dropTiles);
    }
}

void CheckDoorsAndItems<T,U>(Room room, SideviewEditable<T> sv, EnemiesEditable<U> ee) where T : Enum where U : Enum
{
    var lockedDoorCmds = sv.FindAll(o =>  o.Y < 12 && (int)(object)o.Id == (int)PalaceObjectShared.LOCKED_DOOR);
    var itemCmds = sv.Commands.Where(o => o.HasExtra());

    var lockedDoorEnemies = ee.Enemies.Where(o => (int)(object)o.Id == (int)EnemiesShared.LOCKED_DOOR);
    var elevatorEnemies = ee.Enemies.Where(o => (int)(object)o.Id == (int)EnemiesPalaceShared.ELEVATOR);
    var strikeForJarEnemies = ee.Enemies.Where(o => o.Id switch
    {
        EnemiesPalace125.STRIKE_FOR_RED_JAR => true,
        EnemiesPalace346.STRIKE_FOR_RED_JAR_OR_IRON_KNUCKLE => true,
        EnemiesGreatPalace.STRIKE_FOR_RED_JAR_OR_FOKKA => true,
        _ => false,
    });

    if (lockedDoorEnemies.Count() > 0) { Warning(room, "LockedDoorEnemy", "Room uses LockedDoor enemy."); }
    if (elevatorEnemies.Count() > 0) { Warning(room, "ElevatorEnemy", "Room uses Elevator enemy."); }

    if (room.Group == RoomGroup.VANILLA) { return; }

    for (int page = 0; page < 4; page++)
    {
        List<string> itemsOnPage = new();

        foreach (var o in lockedDoorCmds.Where(o => o.Page == page)) { itemsOnPage.Add($"Cmd:{o.Id} ({Convert.ToHexString(o.Bytes)})"); }
        foreach (var o in itemCmds.Where(o => o.Page == page)) { itemsOnPage.Add($"Cmd:{o.Extra} ({Convert.ToHexString(o.Bytes)})"); }
        if (itemsOnPage.Count == 0)
        {
            continue; // if no locked doors or items are involved, we don't mind.
        }
        foreach (var o in strikeForJarEnemies.Where(o => o.Page == page)) { itemsOnPage.Add($"Enemy:{o.Id} ({Convert.ToHexString(o.Bytes)})"); }

        if (itemsOnPage.Count > 1)
        {
            Warning(room, "MultipleItemsOnPage", $"Has multiple items on page { page + 1}: { string.Join(", ", itemsOnPage)}");
        }
    }
}

void AddOpenCeilingTiles<T>(SideviewEditable<T> sv, bool[,] solidGrid, SortedSet<int> openCeilingTiles) where T : Enum
{
    int w = sv.PageCount * 16;
    for (var x = 0; x < w; x++)
    {
        if (!solidGrid[x, 0])
        {
            openCeilingTiles.Add(x);
        }
    }
}

void AddDropTiles<T>(SideviewEditable<T> sv, bool[,] solidGrid, SortedSet<int> dropTiles) where T : Enum
{
    int w = sv.PageCount * 16;
    for (var x = 0; x < w; x++)
    {
        if (!solidGrid[x, 12] && !solidGrid[x, 11])
        {
            dropTiles.Add(x);
        }
    }
}

void RemoveTilesThatAreLavaPits<T>(List<SideviewMapCommand<T>> pits, SortedSet<int> dropTiles) where T : Enum
{
    foreach (var pit in pits)
    {
        for (var i = 0; i < pit.Param + 1; i++)
        {
            dropTiles.Remove(pit.AbsX + i);
        }
    }
}

void CheckDropsOverLava<T>(Room room, SideviewEditable<T> sv, List<SideviewMapCommand<T>> lavaPits, SortedSet<int> openCeilingTiles) where T : Enum
{
    if (room.IsDropZone)
    {
        SortedSet<int> dropTilesOverLava = [];

        foreach (var lavaPit in lavaPits)
        {
            var endX = lavaPit.AbsX + lavaPit.Width;
            for (int x = lavaPit.AbsX; x < endX; x++)
            {
                if (x < 16 || x > 47) { continue; }
                if (openCeilingTiles.Contains(x))
                {
                    if (sv.Find(o => o.IsSolid && o.Intersects(x, x, 8, 11)) == null)
                    {
                        dropTilesOverLava.Add(x);
                    }
                }
            }
        }
        if (dropTilesOverLava.Count > 0)
        {
            Warning(room, "DropOverLava", $"Room is a drop zone over lava at x={ConvertToRangeString(dropTilesOverLava)}");
        }
    }
}

void CheckLeftExit(Room room, bool[,] solidGrid)
{
    bool hasLeftOpeningWall = FindTileOpeningAtX(solidGrid, 0);
    if (room.HasLeftExit && !hasLeftOpeningWall) { Warning(room, "LeftExitNotOpen", "Room is marked as having a left exit, but there is no left wall opening"); }
}

void CheckRightExit(Room room, int pageCount, bool[,] solidGrid)
{
    bool hasRightOpeningWall = FindTileOpeningAtX(solidGrid, (pageCount * 16) - 1);
    if (room.HasRightExit && !hasRightOpeningWall) { Warning(room, "RightExitNotOpen", "Room is marked as having a right exit, but there is no right wall opening"); }
}

bool FindTileOpeningAtX(bool[,] solidGrid, int x, int n = 3)
{
    int count = 0;
    for (int j = 0; j < solidGrid.GetLength(1); j++)
    {
        count = !solidGrid[x, j] ? count + 1 : 0;
        if (count == n) return true;
    }
    return false;
}

void CheckElevators<T>(Room room, SideviewEditable<T> sv, bool[,] solidGrid, List<SideviewMapCommand<T>> elevators, SortedSet<int> openCeilingTiles) where T : Enum
{
    bool IsDownElevator(SideviewMapCommand<T> o)
    {
        // check if there is at least a 1 tile wide open path down
        // from the default elevator y start position
        int x = o.AbsX;
        int y = 8;
        for (; y < 13; y++)
        {
            if (solidGrid[x, y]) { break; }
        }
        if (y != 13)
        {
            x = o.AbsX + 1;
            y = 8;
            for (; y < 13; y++)
            {
                if (solidGrid[x, y]) { break; }
            }
        }
        return y == 13;
    }
    bool IsUpElevator(SideviewMapCommand<T> o) => openCeilingTiles.Contains(o.AbsX) || openCeilingTiles.Contains(o.AbsX + 1);

    // skip non-exit elevators
    elevators = elevators.TakeWhile(o => IsUpElevator(o) || IsDownElevator(o)).ToList();

    foreach (var elevator in elevators)
    {
        if (IsDownElevator(elevator))
        {
            if (elevator.Param > 0) {
                Warning(room, "ElevatorParam", $"Down elevator has param > 0.\n{FixedElevatorHexString(sv, elevator, 0)}");
            }
        }
        else
        {
            int y = 0;
            for (; y < 11; y++)
            {
                if (solidGrid[elevator.AbsX, y] && solidGrid[elevator.AbsX + 1, y]) { break; }
            }
            if (y < 3) { continue; /* not up elevator */ }
            int optimalParam = Math.Min((11 - y) * 2, 0xf);
            if (Math.Abs(elevator.Param - optimalParam) > 2) // allow a little offset from the optimal position
            {
                Warning(room, "ElevatorParam", $"Up elevator optimal param is {optimalParam}.\n{FixedElevatorHexString(sv, elevator, optimalParam)}");
            }
        }
    }

    if (elevators.Count > 1) { Warning(room, "MultipleExitElevators", "Room has more than one exit elevator"); }
    foreach (var elevator in elevators)
    {
        var x = elevator.AbsX;
        if (x / 16 != room.ElevatorScreen) { Warning(room, "ElevatorWrongScreen", $"ElevatorScreen={room.ElevatorScreen} but elevator.xpos={x}"); }
        var pageX = x % 16;
        if (pageX < 7) { Warning(room, "ElevatorNotCentered", $"Elevator.xpos={x} is too far left on the page"); }
        if (pageX > 7) { Warning(room, "ElevatorNotCentered", $"Elevator.xpos={x} is too far right on the page"); }
    }

    if (room.ElevatorScreen != -1 && elevators.Count == 0) { Warning(room, "ElevatorMissing", $"ElevatorScreen={room.ElevatorScreen} but room has no elevator"); }
    if (room.HasUpExit)
    {
        if (room.ElevatorScreen == -1) { Warning(room, "ElevatorMissing", $"Room is marked as having an up exit but ElevatorScreen=-1"); }
        if (elevators.Count == 0) { Warning(room, "ElevatorMissing", "Room has no elevator but is marked as having an up exit"); }
        else
        {
            if (elevators.Find(o => IsUpElevator(o)) == null)
            {
                Warning(room, "ElevatorCannotGoUp", "Elevator cannot go up but room is marked as having an up exit");
            }
        }
    }
    if (room.HasDownExit && !room.HasDrop)
    {
        if (room.ElevatorScreen == -1) { Warning(room, "ElevatorMissing", "Room is marked as having a down exit but ElevatorScreen=-1"); }
        if (elevators.Count == 0) { Warning(room, "ElevatorMissing", "Room has no elevator but is marked as having a down exit"); }
        else
        {
            if (elevators.Find(o => IsDownElevator(o)) == null)
            {
                Warning(room, "ElevatorCannotGoDown", "Elevator cannot go down but room is marked as having a down exit");
            }
        }
    }
}

void CheckDropZones(Room room, SortedSet<int> openCeilingTiles)
{
    if (!room.IsDropZone) { return; }
    SortedSet<int> shouldBeOpen = new(Enumerable.Range(16, 32));
    shouldBeOpen.IntersectWith(openCeilingTiles);
    if (shouldBeOpen.Count < 6) // not sure what is the lowest allowed
    {
        Warning(room, "DropZoneNotOpen", $"Is drop zone but is only open at x={ConvertToRangeString(shouldBeOpen)}");
    }
}

void CheckDrops(Room room, SortedSet<int> dropTiles)
{
    if (!room.HasDrop)
    {
        // probably this is something like a bridge over a drop that you can't get to
        if (dropTiles.Count > 0) { Warning(room, "DropTilesInNonDropRoom", $"Room is not a drop room, but has drop tiles at x={ConvertToRangeString(dropTiles)}"); }
    }
    else
    {
        if (dropTiles.Count == 0)
        {
            Warning(room, "NoDropTilesInDropRoom", "Room HasDrop but no drop tiles found.");
        }
        else
        {
            var dropScreen = room.IsUpDownReversed ? 2 : 1;
            var badDropTiles = dropTiles.Where(x => x < dropScreen * 16 + 1 || dropScreen * 16 + 15 < x);
            if (badDropTiles.Count() > 0) {
                Warning(room, "DropTilesOutsideRange", $"Drop tiles outside of valid range at x={ConvertToRangeString(badDropTiles)} (dropScreen={dropScreen})");
            }
        }
    }
}

string GetName(Room room)
{
    return room.Name.Length > 0 ? room.Name : Convert.ToHexString(room.SideView);
}

bool IsActuallyEnabled(List<Room> allRooms, Room room)
{
    if (room.Enabled) { return true; }
    Room? linkedRoom = room.LinkedRoomName is { } name ? allRooms!.Find(r => r.Name == name) : null;
    return linkedRoom?.Enabled ?? false;
}

static string ConvertToRangeString(IEnumerable<int> numbers)
{
    return string.Join(",", numbers
        .Select((num, index) => new { num, index })
        .GroupBy(x => x.num - x.index) // group by how many numbers skipped
        .Select(g => g.Count() > 1 ? $"{g.First().num}-{g.Last().num}" : $"{g.First().num}"));
}

string FixedElevatorHexString<T>(SideviewEditable<T> sv, SideviewMapCommand<T> elevator, int param) where T : Enum
{
    elevator.Param = param;
    var newBytes = sv.Finalize();
    return $"Fixed bytes:\n{Convert.ToHexString(newBytes)}\n\n";
}

void Warning(Room room, string id, string msg)
{
    if (room.SuppressWarning.Contains(id)) { return; }
    sb.AppendLine($"{$"[{id}]", -26} \"{GetName(room)}\": " + msg);
}

#pragma warning disable CS8321 // Local function is declared but never used
void DebugPalaceRoomHex<T>(String hex) where T : Enum
{
    byte[] bytes = Convert.FromHexString(hex);
    var sv = new SideviewEditable<T>(bytes);
    sb.AppendLine(sv.DebugString());
}
#pragma warning restore CS8321 // Local function is declared but never used

ValidateRoomsForFile("PalaceRooms.json");
sb.AppendLine();
//ValidateRoomsForFile("CustomRooms.json");
sb.AppendLine();

//DebugPalaceRoomHex<GreatPalaceObject>("32600808102F23F8D50123F7D502102F23F694069106D10323F5D404102F23F4D50522F3F3500006B0910106B091D10F112F");

var result = sb.ToString();
File.WriteAllText("ValidationOutput.txt", result);
Console.Write(result);
