// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RandomizerCore.Sidescroll;

StringBuilder sb = new StringBuilder("");

void ValidateRoomsForFile(string filename)
{
    sb.AppendLine("Validating \"" + filename + "\"...");
    var json = RandomizerCore.Util.ReadAllTextFromFile(filename);
    var palaceRooms = JsonSerializer.Deserialize(json, RoomSerializationContext.Default.ListRoom);

    var duplicateNames = palaceRooms!
        .Where(o => o.Name.Length > 0)
        .GroupBy(o => o.Name)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);
    foreach (var name in duplicateNames) { sb.AppendLine($"{name}: room name is used more than once."); }

    // Checking that all room data is conforming to the structure
    // (It would be very bad if any of the length bytes were wrong.)
    foreach (var room in palaceRooms!.Where(r => r.Enabled))
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
            var ee = new EnemiesEditable(room.Enemies);
            if (ee.Enemies.Count > 9) { sb.AppendLine($"{GetName(room)}: Room has too many enemies ({ee.Enemies.Count})."); }
        }
        catch (Exception e) { sb.AppendLine($"{GetName(room)}: Room enemies data error. {e.Message}"); }

        if (room.Connections.Length != 4) { sb.AppendLine($"{GetName(room)}: Room Connections data error."); }
    }

    // REGULAR PALACE ROOMS
    foreach (var room in palaceRooms!.Where(r => r.Enabled && r.PalaceNumber != 7 && !r.SuppressValidation))
    {
        var sv = new SideviewEditable<PalaceObject>(room.SideView);
        if (sv.HasItem() != room.HasItem) { sb.AppendLine($"{GetName(room)}: Room HasItem mismatch."); }

        if (sv.BackgroundMap != 0) { continue; /* Not supporting built-in "background" maps */ }

        if (room.IsBossRoom)
        {
            var statue = sv.Find(o => o.Id == PalaceObject.IronknuckleStatue && o.AbsX == 62 && o.Y == 9);
            if (statue == null) { sb.AppendLine($"{GetName(room)}: Boss room is missing exit statue."); }
            var exitBlocker = sv.Find(o => o.Id == PalaceObject.HorizontalBrick && o.AbsX == 62 && o.Y > 7 && o.Y < 11);
            if (exitBlocker != null) { sb.AppendLine($"{GetName(room)}: Boss room has low ceiling at exit preventing re-entry."); }
        }

        SortedSet<int> openCeilingTiles = [];
        SortedSet<int> dropTiles = [];

        AddOpenCeilingTiles(sv, openCeilingTiles);

        var horizontalPits = sv.FindAll(o => o.Id == PalaceObject.HorizontalPit && o.Y < 12);
        AddTilesFromHorizontalPits(horizontalPits, openCeilingTiles, dropTiles);

        var walkthruDrop = sv.FindAll(o => o.Id == PalaceObject.WalkThruBricks && o.Y < 12);
        AddTilesFrom2HighHorizontalPits(walkthruDrop, openCeilingTiles, dropTiles);

        var singleRowDrop11 = sv.FindAll(o => o.Id == PalaceObject.HorizontalPitOrLava && o.Y == 11);
        var singleRowDrop12 = sv.FindAll(o => o.Id == PalaceObject.HorizontalPitOrLava && o.Y == 12);
        if (singleRowDrop11.Count > 0 && singleRowDrop12.Count > 0)
        {
            // assuming both rows line-up here
            foreach (var pit in singleRowDrop12)
            {
                for (var i = 0; i < pit.Param + 1; i++)
                {
                    dropTiles.Add(pit.AbsX + i);
                }
            }
        }

        var verticalPits = sv.FindAll(o => (o.Id == PalaceObject.VerticalPit1 || o.Id == PalaceObject.VerticalPit2) && o.Y < 13);
        foreach (var pit in verticalPits)
        {
            if (pit.Y == 0)
            {
                openCeilingTiles.Add(pit.AbsX);
            }
            else if (pit.Y + pit.Param >= 12)
            {
                dropTiles.Add(pit.AbsX);
            }
        }

        var lavaPits = sv.FindAll(o => o.IsLava());
        RemoveTilesThatAreLavaPits(lavaPits, dropTiles);

        var elevators = sv.FindAll(o => o.IsElevator());
        CheckElevators(room, elevators, openCeilingTiles, dropTiles);
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);
        }
        CheckDropZones(room, openCeilingTiles);
        CheckDrops(room, dropTiles);
    }

    // GREAT PALACE ROOMS
    foreach (var room in palaceRooms!.Where(r => r.Enabled && r.PalaceNumber == 7 && !r.SuppressValidation))
    {
        var sv = new SideviewEditable<GreatPalaceObject>(room.SideView);

        if (sv.BackgroundMap != 0) { continue; /* Not supporting built-in "background" maps */ }

        SortedSet<int> openCeilingTiles = [];
        SortedSet<int> dropTiles = [];

        AddOpenCeilingTiles(sv, openCeilingTiles);

        var verticalPits = sv.FindAll(o => o.Id == GreatPalaceObject.ElevatorShaft && o.Y < 13);
        foreach (var pit in verticalPits)
        {
            if (pit.Y == 0)
            {
                openCeilingTiles.Add(pit.AbsX);
            }
            dropTiles.Add(pit.AbsX);
        }

        var walkthruDrop = sv.FindAll(o => o.Id == GreatPalaceObject.WalkThruBricks && o.Y < 12);
        AddTilesFrom2HighHorizontalPits(walkthruDrop, openCeilingTiles, dropTiles);

        var lavaPits = sv.FindAll(o => o.IsLava());
        RemoveTilesThatAreLavaPits(lavaPits, dropTiles);

        var elevators = sv.FindAll(o => o.IsElevator());
        CheckElevators(room, elevators, openCeilingTiles, dropTiles);
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);
        }
        CheckDropZones(room, openCeilingTiles);
        CheckDrops(room, dropTiles);
    }
}

void AddOpenCeilingTiles<T>(SideviewEditable<T> sv, SortedSet<int> openCeilingTiles) where T : Enum
{
    var floor = SideviewMapCommand<T>.CreateNewFloor(0, sv.FloorHeader);
    List<SideviewMapCommand<T>> floors = sv.FindAll(o => o.IsNewFloor());
    for (var x = 0; x < 64; x++)
    {
        while (floors.Count > 0 && floors[0].AbsX == x)
        {
            floor = floors[0];
            floors.RemoveAt(0);
        }
        if (!floor.IsFloorSolidAt(0))
        {
            openCeilingTiles.Add(x);
        }
    }
}

void AddTilesFromHorizontalPits<T>(List<SideviewMapCommand<T>> pits, SortedSet<int> openCeilingTiles, SortedSet<int> dropTiles) where T : Enum
{
    foreach (var pit in pits)
    {
        if (pit.Y == 0)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                openCeilingTiles.Add(pit.AbsX + i);
            }
        }
        for (var i = 0; i < pit.Param + 1; i++)
        {
            dropTiles.Add(pit.AbsX + i);
        }
    }
}

void AddTilesFrom2HighHorizontalPits<T>(List<SideviewMapCommand<T>> pits, SortedSet<int> openCeilingTiles, SortedSet<int> dropTiles) where T : Enum
{
    foreach (var pit in pits)
    {
        if (pit.Y == 0)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                openCeilingTiles.Add(pit.AbsX + i);
            }
        }
        else if (pit.Y == 11)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Add(pit.AbsX + i);
            }
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

void CheckElevators<T>(Room room, List<SideviewMapCommand<T>> elevators, SortedSet<int> openCeilingTiles, SortedSet<int> dropTiles) where T : Enum
{
    // skip non-exit elevators
    elevators = elevators.TakeWhile(o => openCeilingTiles.Contains(o.AbsX) || openCeilingTiles.Contains(o.AbsX + 1) || dropTiles.Contains(o.AbsX) || dropTiles.Contains(o.AbsX + 1)).ToList();
    if (elevators.Count > 1) { sb.AppendLine($"{GetName(room)}: Room has more than one exit elevator"); }
    foreach (var elevator in elevators)
    {
        var x = elevator.AbsX;
        if (x / 16 != room.ElevatorScreen) { sb.AppendLine($"{GetName(room)}: ElevatorScreen={room.ElevatorScreen} but elevator.xpos={x}"); }
        var pageX = x % 16;
        if (pageX < 7) { sb.AppendLine($"{GetName(room)}: Elevator.xpos={x} is too far left on the page"); }
        if (pageX > 7) { sb.AppendLine($"{GetName(room)}: Elevator.xpos={x} is too far right on the page"); }
    }

    if (room.ElevatorScreen != -1 && elevators.Count == 0) { sb.AppendLine($"{GetName(room)}: ElevatorScreen={room.ElevatorScreen} but room has no elevator"); }
    if (room.HasUpExit)
    {
        if (room.ElevatorScreen == -1) { sb.AppendLine($"{GetName(room)}: Room is marked as having an up exit but ElevatorScreen=-1"); }
        if (elevators.Count == 0) { sb.AppendLine($"{GetName(room)}: Room has no elevator but is marked as having an up exit"); }
        else
        {
            if (elevators.Find(o => openCeilingTiles.Contains(o.AbsX) || openCeilingTiles.Contains(o.AbsX + 1)) == null)
            {
                sb.AppendLine($"{GetName(room)}: Elevator cannot go up but is marked as having an up exit");
            }
        }
    }
    if (room.HasDownExit && !room.HasDrop)
    {
        if (room.ElevatorScreen == -1) { sb.AppendLine($"{GetName(room)}: Room is marked as having a down exit but ElevatorScreen=-1"); }
        if (elevators.Count == 0) { sb.AppendLine($"{GetName(room)}: Room has no elevator but is marked as having a down exit"); }
        else
        {
            if (elevators.Find(o => dropTiles.Contains(o.AbsX) || dropTiles.Contains(o.AbsX + 1)) == null)
            {
                sb.AppendLine($"{GetName(room)}: Elevator cannot go down but is marked as having a down exit");
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
        sb.AppendLine($"{GetName(room)}: Is drop zone but is only open at x={ConvertToRangeString(shouldBeOpen)}");
    }
}

void CheckDrops(Room room, SortedSet<int> dropTiles)
{
    if (!room.HasDrop)
    {
        // probbaly this is something like a bridge over a drop you can't get to
        // if (dropTiles.Count > 0) { sb.AppendLine($"{GetName(room)}: is not drop, but has drop tiles at x={ConvertToRangeString(dropTiles)}"); }
    }
    else
    {
        if (dropTiles.Count == 0)
        {
            sb.AppendLine(GetName(room) + ": HasDrop but no drop tiles found.");
        }
        else
        {
            var dropScreen = room.IsUpDownReversed ? 2 : 1;
            var badDropTiles = dropTiles.Where(x => x < dropScreen * 16 + 1 || dropScreen * 16 + 15 < x);
            if (badDropTiles.Count() > 0) {
                sb.AppendLine($"{GetName(room)}: Drop tiles outside of valid range at x={ConvertToRangeString(badDropTiles)} (dropScreen={dropScreen})");
            }
        }
    }
}

string GetName(Room room)
{
    return room.Name.Length > 0 ? room.Name : Convert.ToHexString(room.SideView);
}

static string ConvertToRangeString(IEnumerable<int> numbers)
{
    return string.Join(",", numbers
        .Select((num, index) => new { num, index })
        .GroupBy(x => x.num - x.index) // group by how many numbers skipped
        .Select(g => g.Count() > 1 ? $"{g.First().num}-{g.Last().num}" : $"{g.First().num}"));
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
