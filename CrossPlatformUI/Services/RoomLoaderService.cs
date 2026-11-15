using System;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore.Sidescroll;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace CrossPlatformUI.Services;

public class RoomLoaderService
{
    private Lazy<Task<PalaceRooms>> _loadPalaceRoomsTask;
    private Lazy<Task<RoomPoolSpec?>> _loadRoomPoolSpecTask;

    public RoomLoaderService(IFileSystemService fileSystemService)
    {
        _loadPalaceRoomsTask = new Lazy<Task<PalaceRooms>>(
            () => LoadPalaceRoomsAsync(fileSystemService));
        _loadRoomPoolSpecTask = new Lazy<Task<RoomPoolSpec?>>(
            () => LoadRoomPoolSpecAsync(fileSystemService));
    }

    public async Task<PalaceRooms> GetPalaceRooms()
    {
        return await _loadPalaceRoomsTask.Value;
    }

    public async Task<RoomPoolSpec?> GetRoomPoolSpec()
    {
        return await _loadRoomPoolSpecTask.Value;
    }

    private static async Task<PalaceRooms> LoadPalaceRoomsAsync(IFileSystemService fileService)
    {
        var roomsJson = await fileService.OpenFile(
            IFileSystemService.RandomizerPath.Palaces,
            "PalaceRooms.json");
        return new PalaceRooms(roomsJson!, false);
    }

    private static async Task<RoomPoolSpec?> LoadRoomPoolSpecAsync(IFileSystemService fileService)
    {
        var roomPoolYaml = await fileService.OpenFile(
            IFileSystemService.RandomizerPath.RoomPool,
            "CustomRoomPool.yaml");
        if (roomPoolYaml == null) { return null; }
        return RoomPoolSpecDeserializer.FromString(roomPoolYaml);
    }
}
