using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace RandomizerCore.Sidescroll;

public enum PalaceGrouping
{
    Palace125 = 0,
    Palace346 = 1,
    PalaceGp = 2,
    Unintialized = 255,
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

    public static readonly RequirementType[][] ALLOWED_BLOCKERS_BY_PALACE = [ 
        VANILLA_P1_ALLOWED_BLOCKERS,
        VANILLA_P2_ALLOWED_BLOCKERS,
        VANILLA_P3_ALLOWED_BLOCKERS,
        VANILLA_P4_ALLOWED_BLOCKERS,
        VANILLA_P5_ALLOWED_BLOCKERS,
        VANILLA_P6_ALLOWED_BLOCKERS,
        VANILLA_P7_ALLOWED_BLOCKERS
    ];

    public async Task<List<Palace>> CreatePalaces(Random r, RandomizerProperties props, PalaceRooms palaceRooms, bool raftIsRequired, CancellationToken ct)
    {
        if (props.UseCustomRooms && !File.Exists("CustomRooms.json"))
        {
            throw new Exception("Couldn't find CustomRooms.json. Please create the file or disable custom rooms on the misc tab.");
        }
        List<Palace> palaces = [];

        int[] sizes = [14, 21, 15, 21, 28, 27, 55];
        //1-4/7 first, then 5-6 because they're dependant
        for(int i = 0; i < 7; i++)
        {
            if (props.PalaceStyles[i].UsesVanillaRoomPool())
            {
                sizes[i] = (i + 1) switch
                {
                    //Shortened values consistent with the old shorten vanilla logic
                    1 => props.ShortenNormalPalaces ? r.Next(8, 12) : 14,
                    2 => props.ShortenNormalPalaces ? r.Next(11, 17) : 21,
                    3 => props.ShortenNormalPalaces ? r.Next(8, 13) : 15,
                    4 => props.ShortenNormalPalaces ? r.Next(11, 17) : 21,
                    7 => props.ShortenGP ? r.Next(28, 43) : 55,
                    _ => 0
                };
            }
            else
            {
                sizes[i] = (i + 1) switch
                {
                    1 => props.ShortenNormalPalaces ? r.Next(7, 12) : r.Next(10, 17), //13
                    2 => props.ShortenNormalPalaces ? r.Next(11, 17) : r.Next(16, 25),//20
                    3 => props.ShortenNormalPalaces ? r.Next(8, 13) : r.Next(11, 18),//14
                    4 => props.ShortenNormalPalaces ? r.Next(11, 17) : r.Next(16, 25), //20
                    7 => props.ShortenGP ? r.Next(27, 41) /*34*/ : sizes[6] = r.Next(54, 60),//57
                    _ => 0
                };
            }
        }
        for (int i = 4; i < 6; i++)
        {
            if (props.PalaceStyles[i].UsesVanillaRoomPool())
            {
                sizes[i] = (i + 1) switch
                {
                    5 => props.ShortenNormalPalaces ? r.Next(15, Math.Min(63 - sizes[0] - sizes[1], 23)) : 28,
                    6 => props.ShortenNormalPalaces ? r.Next(14, Math.Min(63 - sizes[2] - sizes[3], 22)) : 27,
                    _ => sizes[i]
                };
            }
            else
            {
                sizes[i] = (i + 1) switch
                {
                    5 => props.ShortenNormalPalaces ? r.Next(14, Math.Min(63 - sizes[0] - sizes[1], 23)) : r.Next(23, 63 - sizes[0] - sizes[1]), //23 to 20-36
                    6 => props.ShortenNormalPalaces ? r.Next(14, Math.Min(63 - sizes[2] - sizes[3], 22)) : r.Next(22, 63 - sizes[2] - sizes[3]), //22 to 21-37
                    _ => sizes[i]
                };
            }
        }

        //If P5/6 is vanilla, it's possible the previous palace(s) rolled up and the vanilla palace took us beyond the limit
        //if so, subtract the difference in rooms between the number and max proportionally between the non-vanilla palaces
        //There is almost certainly a more elegant solution for this but I don't care.
        int groupPalaceRoomCount = sizes[0] + sizes[1] + sizes[4];
        if(groupPalaceRoomCount > 63)
        {
            if (props.PalaceStyles[0].UsesVanillaRoomPool())
            {
                if (props.PalaceStyles[1].UsesVanillaRoomPool())
                {
                    throw new ImpossibleException("Palace room pool count was impossibly high");
                }
                else
                {
                    sizes[1] -= groupPalaceRoomCount - 63;
                }
            }
            else
            {
                if (props.PalaceStyles[1].UsesVanillaRoomPool())
                {
                    sizes[0] -= groupPalaceRoomCount - 63;
                }
                else
                {
                    //If neither palace is vanilla, divide the excess reduction between the palaces prioritizing P2
                    sizes[0] -= (groupPalaceRoomCount - 63) / 2;
                    sizes[1] -= 63 - sizes[0] - sizes[4];
                }
            }
        }
        groupPalaceRoomCount = sizes[2] + sizes[3] + sizes[5];
        if (groupPalaceRoomCount > 63)
        {
            if (props.PalaceStyles[2].UsesVanillaRoomPool())
            {
                if (props.PalaceStyles[3].UsesVanillaRoomPool())
                {
                    throw new ImpossibleException("Palace room pool count was impossibly high");
                }
                else
                {
                    sizes[3] -= groupPalaceRoomCount - 63;
                }
            }
            else
            {
                if (props.PalaceStyles[3].UsesVanillaRoomPool())
                {
                    sizes[2] -= groupPalaceRoomCount - 63;
                }
                else
                {
                    //If neither palace is vanilla, divide the excess reduction between the palaces prioritizing P2
                    sizes[2] -= (groupPalaceRoomCount - 63) / 2;
                    sizes[3] -= 63 - sizes[2] - sizes[5];
                }
            }
        }


        byte group1MapIndex = 0, group2MapIndex = 0, group3MapIndex = 0;
        for (int currentPalace = 1; currentPalace < 8; currentPalace++)
        {
            PalaceGenerator palaceGenerator = props.PalaceStyles[currentPalace - 1] switch
            {
                PalaceStyle.VANILLA => new VanillaPalaceGenerator(),
                PalaceStyle.SHUFFLED => new VanillaShufflePalaceGenerator(),
                PalaceStyle.SEQUENTIAL => new SequentialPlacementCoordinatePalaceGenerator(),
                PalaceStyle.RANDOM_WALK => new RandomWalkCoordinatePalaceGenerator(),
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
            } while (!palace.IsValid || (props.PalaceStyles[currentPalace - 1] != PalaceStyle.VANILLA && palace.HasInescapableDrop()));

            if(palace.PalaceGroup == PalaceGrouping.Palace125)
            {
                group1MapIndex = palace.AssignMapNumbers(group1MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
            }
            
            if (palace.PalaceGroup == PalaceGrouping.Palace346)
            {
                group2MapIndex = palace.AssignMapNumbers(group2MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
            }
            
            if (palace.PalaceGroup == PalaceGrouping.PalaceGp)
            {
                group3MapIndex = palace.AssignMapNumbers(group3MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
            }
            palace.AllRooms.ForEach(i => i.PalaceNumber = currentPalace);
            palace.ValidateRoomConnections();
            palaces.Add(palace);
            sizes[currentPalace - 1] = palace.AllRooms.Count;
        }

        palaces[3].BossRoom!.Requirements = palaces[3].BossRoom!.Requirements.AddHardRequirement(RequirementType.REFLECT);

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
            if(palace.Number < 7 && palace.ItemRoom == null || palace.BossRoom == null)
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
