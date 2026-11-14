using System.Runtime.CompilerServices;
using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.Tests;

[TestClass]
public class DialogRewardTests
{
    [TestMethod]
    [DataRow(Collectable.TANOOKI, Collectable.JUMP_SPELL)]
    [DataRow(Collectable.DASH_SPELL, Collectable.FIRE_SPELL)]
    [DataRow(Collectable.FIRE_SPELL, Collectable.FIRE_SPELL)]
    [DataRow(Collectable.SHIELD_SPELL, Collectable.SHIELD_SPELL)]
    [DataRow(Collectable.SMALL_BAG, Collectable.SMALL_BAG)]
    public void DialogRewards_UseRomItemIds(Collectable reward, Collectable romItem)
    {
        // Only the town containers are needed; do not construct ROM-dependent
        // continents or generate terrain for this reward-table regression.
        var west = (WestHyrule)RuntimeHelpers.GetUninitializedObject(typeof(WestHyrule));
        var east = (EastHyrule)RuntimeHelpers.GetUninitializedObject(typeof(EastHyrule));
        west.rauru = RewardTown(TownType.RAURU, VanillaTownMap.RAURU_WIZARD);
        west.ruto = RewardTown(TownType.RUTO, VanillaTownMap.RUTO_WIZARD);
        west.sariaNorth = RewardTown(TownType.SARIA, VanillaTownMap.SARIA_WIZARD, VanillaTownMap.SARIA_TABLE);
        west.mido = RewardTown(TownType.MIDO, VanillaTownMap.MIDO_WIZARD, VanillaTownMap.MIDO_TRAINER);
        west.bagu = RewardTown(TownType.BAGU, VanillaTownMap.BAGU_INDOORS);
        east.nabooru = RewardTown(TownType.NABOORU, VanillaTownMap.NABOORU_WIZARD, VanillaTownMap.NABOORU_MID);
        east.darunia = RewardTown(TownType.DARUNIA, VanillaTownMap.DARUNIA_WIZARD, VanillaTownMap.DARUNIA_TRAINER);
        east.newKasuto = RewardTown(TownType.NEW_KASUTO, VanillaTownMap.NEW_KASUTO_WIZARD);
        east.oldKasuto = RewardTown(TownType.OLD_KASUTO, VanillaTownMap.OLD_KASUTO_WIZARD);
        var hyrule = new Hyrule((_, _) => throw new NotSupportedException(), null!, null)
        {
            westHyrule = west,
            eastHyrule = east
        };

        var rewards = hyrule.GetFullShuffleLocationCollectables().ToArray();

        rewards.Should().HaveCount(13);
        rewards.Select(entry => entry.Item1).Should().OnlyHaveUniqueItems();
        rewards.Select(entry => entry.Item2).Should().OnlyContain(id => id == (int)romItem);

        Location RewardTown(TownType type, params VanillaTownMap[] maps)
        {
            var townMaps = maps.Select(map => new TownMap((int)map, [4, 0, 0, 0], [1], true)
            {
                Collectable = reward
            }).ToList();
            return new Location(LocationID.WEST_TOWN_RAURU, 0, 0, 0, Continent.WEST)
            {
                Town = new Town(townMaps, [], []) { Type = type }
            };
        }
    }
}
