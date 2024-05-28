using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;

namespace RandomizerCore.Sidescroll;

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

    public static List<Palace> CreatePalaces(CancellationToken ct, Random r, RandomizerProperties props, PalaceRooms palaceRooms, bool raftIsRequired)
    {
        if (props.UseCustomRooms && !File.Exists("CustomRooms.json"))
        {
            throw new Exception("Couldn't find CustomRooms.json. Please create the file or disable custom rooms on the misc tab.");
        }
        List<Palace> palaces = [];

        int[] sizes = new int[7];

        if (props.ShortenNormalPalaces)
        {
            sizes[0] = r.Next(6, 11);
            sizes[1] = r.Next(10, 16);
            sizes[2] = r.Next(7, 12);
            sizes[3] = r.Next(10, 16);
            sizes[4] = r.Next(14, Math.Min(63 - sizes[0] - sizes[1], 22));
            sizes[5] = r.Next(14, Math.Min(63 - sizes[2] - sizes[3], 22));
        }
        else
        {
            sizes[0] = r.Next(10, 17); //13
            sizes[1] = r.Next(16, 25); //20
            sizes[2] = r.Next(11, 18); //14
            sizes[3] = r.Next(16, 25); //20
            sizes[4] = r.Next(23, 63 - sizes[0] - sizes[1]); //23 to 20-36
            sizes[5] = r.Next(22, 63 - sizes[2] - sizes[3]); //22 to 21-37
        }

        if (props.ShortenGP)
        {
            sizes[6] = r.Next(27, 41); //34
        }
        else
        {
            sizes[6] = r.Next(54, 60); //57
        }

        byte group1MapIndex = 0, group2MapIndex = 0, group3MapIndex = 0;
        for (int currentPalace = 1; currentPalace < 8; currentPalace++)
        {
            PalaceGenerator palaceGenerator = props.PalaceStyles[currentPalace - 1] switch
            {
                PalaceStyle.VANILLA => new VanillaPalaceGenerator(ct),
                PalaceStyle.SHUFFLED => new VanillaShufflePalaceGenerator(ct),
                PalaceStyle.SEQUENTIAL => new SequentialPlacementCoordinatePalaceGenerator(ct),
                PalaceStyle.RECONSTRUCTED => new ReconstructedPalaceGenerator(ct),
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
                palace = palaceGenerator.GeneratePalace(props, roomPool, r, sizes[currentPalace - 1], currentPalace);
            } while (!palace.IsValid);

            if(palace.GetPalaceGroup() == 1)
            {
                group1MapIndex = palace.AssignMapNumbers(group1MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
                palace.AllRooms.ForEach(i => i.PalaceGroup = 1);
            }

            if (palace.GetPalaceGroup() == 2)
            {
                group2MapIndex = palace.AssignMapNumbers(group2MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
                palace.AllRooms.ForEach(i => i.PalaceGroup = 2);
            }

            if (palace.GetPalaceGroup() == 3)
            {
                group3MapIndex = palace.AssignMapNumbers(group3MapIndex, currentPalace == 7, props.PalaceStyles[currentPalace - 1].UsesVanillaRoomPool());
                palace.AllRooms.ForEach(i => i.PalaceGroup = 3);
            }
            palace.ValidateRoomConnections();
            palaces.Add(palace);
        }

        if (!ValidatePalaces(props, raftIsRequired, palaces))
        {
            return [];
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
