using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Town;

public class Town
{

    public string Name { get; set; } = "";
    public TownType? Type { get; set; }

    public List<TownMap> TownMaps { get; } = [];

    //When custom towns happen, make this real
    //public Town(string townJson)

    public Town(List<TownMap> townMaps, Dictionary<int, byte[]> connections, Dictionary<int, byte[]> doorConnections)
    {
        Debug.Assert(connections.Count == doorConnections.Count);
        TownMaps = townMaps;
        foreach (TownMap townMap in townMaps.OrderBy(i => i.Map).Where(i => !i.IsInternalLocation || (i.Map == 57 && Type == TownType.DARUNIA)))
        {
            if(connections.TryGetValue(townMap.Map, out var connectionsRaw))
            {
                byte connectionMap = (byte)((connectionsRaw[0] & 0xFC) >> 2);
                townMap.Left = townMaps.FirstOrDefault(i => i.Map == connectionMap);
                connectionMap = (byte)((connectionsRaw[3] & 0xFC) >> 2);
                townMap.Right = townMaps.FirstOrDefault(i => i.Map == connectionMap);
            }

            if (doorConnections.TryGetValue(townMap.Map, out var doorConnectionsRaw))
            {
                byte connectionMap = (byte)((doorConnectionsRaw[0] & 0xFC) >> 2);
                townMap.Door0 = townMaps.FirstOrDefault(i => i.Map == connectionMap);
                connectionMap = (byte)((doorConnectionsRaw[1] & 0xFC) >> 2);
                townMap.Door1 = townMaps.FirstOrDefault(i => i.Map == connectionMap);
                connectionMap = (byte)((doorConnectionsRaw[2] & 0xFC) >> 2);
                townMap.Door2 = townMaps.FirstOrDefault(i => i.Map == connectionMap);
                connectionMap = (byte)((doorConnectionsRaw[3] & 0xFC) >> 2);
                townMap.Door3 = townMaps.FirstOrDefault(i => i.Map == connectionMap);
            }
        }
    }

    //Determine what items you can get from this town with these reqireables from this entrance direction
    public List<Collectable> GetGettableItems(Direction entranceDirection, List<RequirementType> requireables, bool shufflableItemsOnly = false)
    {
        TownMap startMap = entranceDirection switch
        {
            Direction.WEST => TownMaps.First(i => i.Left == null),
            Direction.EAST => TownMaps.First(i => i.Right == null),
            _ => throw new ArgumentException("Invalid entrance direction. Towns can only enter from the west/east")
        };

        List<Collectable> collectables = [];

        List<TownMap> checkedMaps = [];
        List<TownMap> mapsToCheck = [startMap];
        TownMap currentMap;
        do
        {
            currentMap = mapsToCheck[0];
            if (currentMap.AccessRequirements.AreSatisfiedBy(requireables))
            {
                if (currentMap.Door0 != null && !checkedMaps.Contains(currentMap.Door0) && !mapsToCheck.Contains(currentMap.Door0))
                {
                    mapsToCheck.Add(currentMap.Door0);
                }
                if (currentMap.Door1 != null && !checkedMaps.Contains(currentMap.Door1) && !mapsToCheck.Contains(currentMap.Door1))
                {
                    mapsToCheck.Add(currentMap.Door1);
                }
                if (currentMap.Door2 != null && !checkedMaps.Contains(currentMap.Door2) && !mapsToCheck.Contains(currentMap.Door2))
                {
                    mapsToCheck.Add(currentMap.Door2);
                }
                if (currentMap.Door3 != null && !checkedMaps.Contains(currentMap.Door3) && !mapsToCheck.Contains(currentMap.Door3))
                {
                    mapsToCheck.Add(currentMap.Door3);
                }
                if (entranceDirection == Direction.WEST 
                    //This is a hacky fix for the darunia upstab guy intermediary room, which is the only vanilla room
                    //after shortenWizards that connects to another internal room. Normal internal rooms specifically need
                    //to not consider their connections because there's often multiple rooms in the same map,
                    //but this specific one does so it's special cased here. This will need to be fixed for custom towns.
                    && (!currentMap.IsInternalLocation || (currentMap.Map == 57 && Type == TownType.DARUNIA))
                    && currentMap.Right != null 
                    && !checkedMaps.Contains(currentMap.Right) 
                    && !mapsToCheck.Contains(currentMap.Right))
                {
                    mapsToCheck.Add(currentMap.Right);
                }
                if (entranceDirection == Direction.EAST 
                    && !currentMap.IsInternalLocation
                    && currentMap.Left != null 
                    && !checkedMaps.Contains(currentMap.Left) 
                    && !mapsToCheck.Contains(currentMap.Left))
                {
                    mapsToCheck.Add(currentMap.Left);
                }
                if (currentMap.Collectable != null && (currentMap.CollectableIsShufflable || !shufflableItemsOnly))
                {
                    collectables.Add((Collectable)currentMap.Collectable!);
                }
            }
            checkedMaps.Add(currentMap);
            mapsToCheck.Remove(currentMap);
        }
        while (mapsToCheck.Count > 0);

        return collectables;
    }

    public bool CanBeTraversed(List<RequirementType> requireables)
    {
        TownMap leftMap = TownMaps.First(i => i.Left == null && !i.IsInternalLocation);

        //This currently only checks left-right because all towns are either bi-directionally traversable or oneway deadending right (new kasuto)
        //This means it will perform weirdly if you somehow create a monodirectional town entered from the right that you also
        //for some reason set as a connection. Don't do that.
        List<TownMap> coveredMaps = [];
        TownMap currentMap = leftMap;
        while(coveredMaps.Count < 63)
        {
            if(coveredMaps.Contains(currentMap))
            {
                throw new Exception($"Loop detected in town connection routing. Check your town connections for town {Name}.");
            }
            if(!currentMap.AccessRequirements.AreSatisfiedBy(requireables))
            {
                return false;
            }
            if (currentMap.Right == null) 
            {
                return false;
            }
            if(currentMap.RightExitIsOutside)
            {
                return true;
            }
            coveredMaps.Add(currentMap);
            currentMap = currentMap.Right;
        }
        throw new ImpossibleException("Too many non-looping maps in town traversal check");
    }

    public void SetCollectables(IEnumerable<Collectable> collectables, bool collectableOnly = false)
    {
        List<TownMap> collectableMaps = TownMaps.Where(i => i.Collectable != null && (!collectableOnly || i.CollectableIsShufflable)).ToList();
        Debug.Assert(collectables.Count() == collectableMaps.Count);
        int i = 0;
        foreach(Collectable collectable in collectables)
        {
            collectableMaps[i++].Collectable = collectable;
        }
    }

    public bool ReplaceCollectable(Collectable toReplace, Collectable replacement)
    {
        TownMap? map = TownMaps.FirstOrDefault(i => i.Collectable == toReplace);
        if(map == null)
        {
            return false;
        }
        map.Collectable = replacement;
        return true;
    }

    public TownMap? GetWizard()
    {
        if(Type == null) return null;

        VanillaTownMap map = Type switch
        {
            TownType.RAURU => VanillaTownMap.RAURU_WIZARD,
            TownType.RUTO => VanillaTownMap.RUTO_WIZARD,
            TownType.SARIA => VanillaTownMap.SARIA_WIZARD,
            TownType.MIDO => VanillaTownMap.MIDO_WIZARD,
            TownType.NABOORU => VanillaTownMap.NABOORU_WIZARD,
            TownType.DARUNIA => VanillaTownMap.DARUNIA_WIZARD,
            TownType.NEW_KASUTO => VanillaTownMap.NEW_KASUTO_WIZARD,
            TownType.OLD_KASUTO => VanillaTownMap.OLD_KASUTO_WIZARD,
            TownType.BAGU => VanillaTownMap.BAGU_INDOORS,
            _ => throw new ImpossibleException("Unrecognized town type in GetWizard()")
        };
        return GetTownMap(map);
    }

    public TownMap? GetTownMap(int mapNumber)
    {
        return TownMaps.FirstOrDefault(i => i.Map == mapNumber);
    }

    public TownMap? GetTownMap(VanillaTownMap map)
    {
        return GetTownMap((int)map);
    }

    public bool HasShufflableItems()
    {
        return TownMaps.Any(i => i.CollectableIsShufflable);
    }

    public string GenerateSpoiler()
    {
        return string.Join("\n", TownMaps.Where(i => i.Collectable != null).Select(i => $"\t{i.Name} : {i.Collectable?.EnglishText() ?? ""}"));
    }
}
