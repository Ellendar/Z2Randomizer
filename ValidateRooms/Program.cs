// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RandomizerCore;
using RandomizerCore.Sidescroll;

StringBuilder sb = new StringBuilder("");

void ValidateRoomsForFile(string filename)
{
    sb.AppendLine("Validating \"" + filename + "\"...");
    var json = RandomizerCore.Util.ReadAllTextFromFile(filename);
    var palaceRooms = JsonSerializer.Deserialize(json, RoomSerializationContext.Default.ListRoom);

    foreach (var room in palaceRooms!.Where(r => r.Enabled && r.PalaceNumber != 7 && !r.SuppressValidation))
    {
        var name = room.Name.Length > 0 ? room.Name : Convert.ToHexString(room.SideView);
        var sv = new SideviewEditable<PalaceObject>(room.SideView);
        SortedSet<int> dropTiles = [];
        if (sv.HasItem() != room.HasItem)
        {
            sb.AppendLine(name + ": Room HasItem mismatch.");
        }

        var horizontalPits = sv.FindAll(o => o.Id == PalaceObject.HorizontalPit && o.Y < 12);
        foreach (var pit in horizontalPits)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Add(pit.AbsX + i);
            }
        }

        var walkthruDrop = sv.FindAll(o => o.Id == PalaceObject.WalkThruBricks && o.Y == 11);
        foreach (var pit in walkthruDrop)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Add(pit.AbsX + i);
            }
        }

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

        var verticalPits = sv.FindAll(o => (o.Id == PalaceObject.VerticalPit1 || o.Id == PalaceObject.VerticalPit2) && o.Y < 13 && o.Y + o.Param >= 12);
        foreach (var pit in verticalPits)
        {
            dropTiles.Add(pit.AbsX);
        }

        var lavaPits = sv.FindAll(o => o.IsLava());
        foreach (var pit in lavaPits)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Remove(pit.AbsX + i);
            }
        }

        var elevators = sv.FindAll(o => o.IsElevator());
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);

            var x = elevator.AbsX;
            if (x / 16 != room.ElevatorScreen)
            {
                sb.AppendLine($"{name}: ElevatorScreen={room.ElevatorScreen} but elevator.xpos={x}");
            }
            var pageX = x % 16;
            if (pageX < 7)
            {
                sb.AppendLine($"{name}: Elevator.xpos={x} is too far left on the page");
            }
            if (pageX > 7)
            {
                sb.AppendLine($"{name}: Elevator.xpos={x} is too far right on the page");
            }
        }
        if (room.ElevatorScreen != -1 && elevators.Count == 0)
        {
            sb.AppendLine($"{name}: ElevatorScreen={room.ElevatorScreen} but room has no elevator");
        }
        if (room.ElevatorScreen == -1 && elevators.Count == 0 && room.HasUpExit)
        {
            sb.AppendLine($"{name}: Room has no elevator but is marked as having an up exit");
        }
        if (room.ElevatorScreen == -1 && elevators.Count == 0 && dropTiles.Count == 0 && room.HasDownExit)
        {
            sb.AppendLine($"{name}: Room has no elevator or drop but is marked as having a down exit");
        }

        if (dropTiles.Count > 0)
        {
            if (!room.HasDrop)
            {
                // too spammy - areas are probably not reachable
                // sb.AppendLine(Truncate($"{name}: not HasDrop, but has drop tiles at x={String.Join(",x=", dropTiles)}", 135));
            }
        } else if (room.HasDrop) {
            sb.AppendLine(name + ": HasDrop but no drop tiles found.");
        }
    }

    foreach (var room in palaceRooms!.Where(r => r.Enabled && r.PalaceNumber == 7 && !r.SuppressValidation))
    {
        var name = room.Name.Length > 0 ? room.Name : Convert.ToHexString(room.SideView);
        var sv = new SideviewEditable<GreatPalaceObject>(room.SideView);
        SortedSet<int> dropTiles = [];

        var verticalPits = sv.FindAll(o => o.Id == GreatPalaceObject.ElevatorShaft && o.Y < 13);
        foreach (var pit in verticalPits)
        {
            dropTiles.Add(pit.AbsX);
        }

        var walkthruDrop = sv.FindAll(o => o.Id == GreatPalaceObject.WalkThruBricks && o.Y == 11);
        foreach (var pit in walkthruDrop)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Add(pit.AbsX + i);
            }
        }

        var lavaPits = sv.FindAll(o => o.IsLava());
        foreach (var pit in lavaPits)
        {
            for (var i = 0; i < pit.Param + 1; i++)
            {
                dropTiles.Remove(pit.AbsX + i);
            }
        }

        var elevators = sv.FindAll(o => o.IsElevator());
        foreach (var elevator in elevators)
        {
            dropTiles.Remove(elevator.AbsX);
            dropTiles.Remove(elevator.AbsX + 1);

            var x = elevator.AbsX;
            if (x / 16 != room.ElevatorScreen)
            {
                sb.AppendLine($"{name}: ElevatorScreen={room.ElevatorScreen} but elevator.xpos={x}");
            }
            var pageX = x % 16;
            if (pageX < 1)
            {
                sb.AppendLine($"{name}: Elevator.xpos={x} is close to page start");
            }
            if (pageX > 13)
            {
                sb.AppendLine($"{name}: Elevator.xpos={x} is close to page end");
            }
        }

        if (room.ElevatorScreen != -1 && elevators.Count == 0)
        {
            sb.AppendLine($"{name}: ElevatorScreen={room.ElevatorScreen} but room has no elevator");
        }
        if(room.ElevatorScreen == -1 && elevators.Count == 0 && room.HasUpExit)
        {
            sb.AppendLine($"{name}: Room has no elevator but is marked as having an up exit");
        }
        if (room.ElevatorScreen == -1 && elevators.Count == 0 && dropTiles.Count == 0 && room.HasDownExit)
        {
            sb.AppendLine($"{name}: Room has no elevator or drop but is marked as having a down exit");
        }

        if (dropTiles.Count > 0)
        {
            if (!room.HasDrop)
            {
                // too spammy - areas are probably not reachable
                // sb.AppendLine(name + ": is not drop, but has drop tiles at x=" + String.Join(", x=", dropTiles));
            }
        }
        else if (room.HasDrop)
        {
            sb.AppendLine(name + ": HasDrop but no drop tiles found.");
        }
    }
}

#pragma warning disable CS8321 // Local function is declared but never used
string Truncate(string value, int maxChars)
{
    return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
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

//DebugPalaceRoomHex<PalaceCommandType>("2960030804FE802F96C3240705FD802D95C3730F0C06FE802E130794C3A40704FB802B330792C3D703");

var result = sb.ToString();
File.WriteAllText("ValidationOutput.txt", result);
Console.Write(result);
