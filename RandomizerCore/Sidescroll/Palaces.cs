using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public enum PalaceGrouping
{
    Palace125 = 0,
    Palace346 = 1,
    PalaceGp = 2
}

public class Palaces
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly RequirementType[] VANILLA_P1_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY ];
    private static readonly RequirementType[] VANILLA_P2_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY, RequirementType.JUMP, RequirementType.GLOVE ];
    private static readonly RequirementType[] VANILLA_P3_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY, RequirementType.DOWNSTAB, RequirementType.UPSTAB, RequirementType.GLOVE];
    private static readonly RequirementType[] VANILLA_P4_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP];
    private static readonly RequirementType[] VANILLA_P5_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP];
    private static readonly RequirementType[] VANILLA_P6_ALLOWED_BLOCKERS = [ 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP, RequirementType.GLOVE];
    private static readonly RequirementType[] VANILLA_P7_ALLOWED_BLOCKERS = [ 
        RequirementType.FAIRY, RequirementType.UPSTAB, RequirementType.DOWNSTAB, RequirementType.JUMP, RequirementType.GLOVE];

    public static readonly RequirementType[] ALL_PALACE_ALLOWED_BLOCKERS = [
        RequirementType.JUMP, RequirementType.FAIRY, RequirementType.UPSTAB, RequirementType.DOWNSTAB, RequirementType.JUMP, RequirementType.KEY, RequirementType.DASH, RequirementType.GLOVE];

    public static readonly RequirementType[][] ALLOWED_BLOCKERS_BY_PALACE = [ 
        VANILLA_P1_ALLOWED_BLOCKERS,
        VANILLA_P2_ALLOWED_BLOCKERS,
        VANILLA_P3_ALLOWED_BLOCKERS,
        VANILLA_P4_ALLOWED_BLOCKERS,
        VANILLA_P5_ALLOWED_BLOCKERS,
        VANILLA_P6_ALLOWED_BLOCKERS,
        VANILLA_P7_ALLOWED_BLOCKERS
    ];

    public static Dictionary<RoomExitType, int> itemRoomCounts = [];

    public async Task<List<Palace>> CreatePalaces(Random r, RandomizerProperties props, PalaceRooms palaceRooms, bool raftIsRequired, CancellationToken ct)
    {
        if (props.UseCustomRooms && !File.Exists("CustomRooms.json"))
        {
            throw new Exception("Couldn't find CustomRooms.json. Please create the file or disable custom rooms on the misc tab.");
        }
        List<Palace> palaces = [];

        int[] sizes = props.PalaceLengths;

        byte group1MapIndex = 0, group2MapIndex = 0, group3MapIndex = 0;
        for (int currentPalace = 1; currentPalace < 8; currentPalace++)
        {
            PalaceGenerator palaceGenerator = props.PalaceStyles[currentPalace - 1] switch
            {
                PalaceStyle.VANILLA => new VanillaPalaceGenerator(),
                PalaceStyle.SHUFFLED => new VanillaShufflePalaceGenerator(),
                PalaceStyle.SEQUENTIAL => new SequentialPlacementCoordinatePalaceGenerator(),
                PalaceStyle.RANDOM_WALK => new RandomWalkCoordinatePalaceGenerator(),
                PalaceStyle.VANILLA_WEIGHTED => new VanillaWeightedPalaceGenerator(),
                PalaceStyle.RECONSTRUCTED => new ReconstructedPalaceGenerator(ct),
                PalaceStyle.CHAOS => new ChaosPalaceGenerator(),
                _ => throw new Exception("Unrecognized palace style while generating palaces")
            };

            RoomPool roomPool;
            if(props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool())
            {
                roomPool = new VanillaRoomPool(palaceRooms, currentPalace, props);
            }
            else
            {
                roomPool = new(palaceRooms, currentPalace, props);
            }
            Palace palace;
            do
            {
                palace = await palaceGenerator.GeneratePalace(props, roomPool, r, sizes[currentPalace - 1], currentPalace);
            } while (
            !palace.IsValid || 
            (props.PalaceStyles[currentPalace - 1] != PalaceStyle.VANILLA 
                && palace.HasInescapableDrop(props.BossRoomsExitToPalace[currentPalace - 1])));
            PalaceGenerator.DebugCheckDuplicates(props, palace);

            if(palace.PalaceGroup == PalaceGrouping.Palace125)
            {
                group1MapIndex = palace.AssignMapNumbers(group1MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool(), sizes[currentPalace - 1]);
            }
            
            if (palace.PalaceGroup == PalaceGrouping.Palace346)
            {
                group2MapIndex = palace.AssignMapNumbers(group2MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool(), sizes[currentPalace - 1]);
            }
            
            if (palace.PalaceGroup == PalaceGrouping.PalaceGp)
            {
                group3MapIndex = palace.AssignMapNumbers(group3MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool(), sizes[currentPalace - 1]);
            }
            palace.AllRooms.ForEach(i => i.PalaceNumber = currentPalace);
            palace.ValidateRoomConnections();
            palaces.Add(palace);
            sizes[currentPalace - 1] = palace.AllRooms.Count;
        }

        palaces[3].BossRoom!.Requirements = palaces[3].BossRoom!.Requirements.AddHardRequirement(RequirementType.REFLECT);
        foreach (Room room in palaces.SelectMany(i => i.ItemRooms).Where(i => i.Collectable == null))
        {
            room.Collectable = Collectable.LARGE_BAG;
        }

        if (!ValidatePalaces(props, raftIsRequired, palaces))
        {
            return [];
        }

        List<Room> validDrops = palaces[0].AllRooms.Where(i => i.HasDrop)
            .Union(palaces[1].AllRooms.Where(i => i.HasDrop))
            .Union(palaces[2].AllRooms.Where(i => i.HasDrop))
            .Union(palaces[3].AllRooms.Where(i => i.HasDrop))
            .Union(palaces[4].AllRooms.Where(i => i.HasDrop))
            .Union(palaces[5].AllRooms.Where(i => i.HasDrop))
            .Union(palaces[6].AllRooms.Where(i => i.HasDrop))
            .ToList();

        foreach(Palace palace in palaces)
        {
            foreach(Room itemRoom in palace.ItemRooms)
            {
                RoomExitType exitType = itemRoom.CategorizeExits();
                if (itemRoomCounts.TryGetValue(exitType, out int count))
                {
                    itemRoomCounts[exitType] = count + 1;
                }
                else
                {
                    itemRoomCounts.Add(exitType, 1);
                }
            }
        }
        return palaces;
    }

    private static bool ValidatePalaces(RandomizerProperties props, bool raftIsRequired, List<Palace> palaces)
    {
        //Enforce aggregate max length of enemy data
        if (palaces.Where(i => i.Number != 7).Sum(i => i.AllRooms.Sum(j => j.Enemies.Length)) > 0x400
            || palaces.Where(i => i.Number == 7).Sum(i => i.AllRooms.Sum(j => j.Enemies.Length)) > 0x2A9)
        {
            return false;
        }

        foreach(Palace palace in palaces)
        {
            //If the palace doesn't have the right number of item rooms
            //or an item room is null
            //or it doesn't have a boss room when it should
            if (palace.Number < 7 && (palace.ItemRooms.Count() != props.PalaceItemRoomCounts[palace.Number - 1]
                    || palace.ItemRooms.Any(i => i == null))
                || palace.BossRoom == null)
            {
                return false;
            }
        }
        return CanGetGlove(props, palaces[1])
            && CanGetRaft(props, raftIsRequired, palaces[1], palaces[2])
            && AtLeastOnePalaceCanHaveGlove(props, palaces);
    }
    private static bool AtLeastOnePalaceCanHaveGlove(RandomizerProperties props, List<Palace> palaces)
    {
        //If overworld and palace items are mixed, we don't need glove to be in a palace
        if(props.MixOverworldPalaceItems)
        {
            return true;
        }
        List<RequirementType> requireables =
        [
            RequirementType.KEY,
            RequirementType.UPSTAB,
            RequirementType.DOWNSTAB,
            RequirementType.JUMP,
            RequirementType.FAIRY
        ];
        for(int i = 0; i < 6; i++)
        {
            //If there is at least one palace that would be clearable with everything but the glove
            //that palace could contain the glove, so we're not deadlocked.
            if (palaces[i].CanClearAllRooms(requireables, Collectable.GLOVE))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CanGetGlove(RandomizerProperties props, Palace palace2)
    {
        if (!props.ShufflePalaceItems)
        {
            List<RequirementType> requireables = new List<RequirementType>();
            //If shuffle overworld items is on, we assume you can get all the items / spells
            //as all progression items will eventually shuffle into spots that work
            if (props.ShuffleOverworldItems)
            {
                requireables.Add(RequirementType.KEY);
            }
            //Otherwise if it's vanilla items we can't get the magic key, because we could need glove for boots for flute to get to new kasuto
            requireables.Add(props.SwapUpAndDownStab ? RequirementType.UPSTAB : RequirementType.DOWNSTAB);
            requireables.Add(RequirementType.JUMP);
            requireables.Add(RequirementType.FAIRY);

            //If we can't clear 2 without the items available, we can never get the glove, so the palace is unbeatable
            if (!palace2.CanClearAllRooms(requireables, Collectable.GLOVE))
            {
                return false;
            }
        }
        return true;
    }

    private static bool CanGetRaft(RandomizerProperties props, bool raftIsRequired, Palace palace2, Palace palace3)
    {
        //if the flagset has vanilla connections and vanilla palace items, you have to be able to get the raft
        //or it will send the logic into an uinrecoverable nosedive since the palaces can't re-generate
        if (!props.ShufflePalaceItems && raftIsRequired)
        {
            List<RequirementType> requireables = new List<RequirementType>();
            //If shuffle overworld items is on, we assume you can get all the items / spells
            //as all progression items will eventually shuffle into spots that work
            if (props.ShuffleOverworldItems)
            {
                requireables.Add(RequirementType.KEY);
                requireables.Add(RequirementType.GLOVE);
                requireables.Add(props.SwapUpAndDownStab ? RequirementType.UPSTAB : RequirementType.DOWNSTAB);
                requireables.Add(RequirementType.JUMP);
                requireables.Add(RequirementType.FAIRY);
            }
            //Otherwise we can only get the things you can get on the west normally
            else
            {
                requireables.Add(RequirementType.JUMP);
                requireables.Add(RequirementType.FAIRY);
                requireables.Add(props.SwapUpAndDownStab ? RequirementType.UPSTAB : RequirementType.DOWNSTAB);
            }
            //If we can clear P2 with this stuff, we can also get the glove
            if (palace2.CanClearAllRooms(requireables, Collectable.GLOVE))
            {
                requireables.Add(RequirementType.GLOVE);
            }
            //If we can't clear 3 with all the items available on west/DM, we can never raft out, and so we're stuck forever
            //so start over
            if (!palace3.CanClearAllRooms(requireables, Collectable.RAFT))
            {
                return false;
            }
        }
        return true;
    }
}
