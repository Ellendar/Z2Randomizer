using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

public class Hints
{
    private const int baguText = 50;
    private static readonly int[] rauruHints = new int[] { 32, 12, 30 }; //Three houses, first screen
    private static readonly int[] rutoHints = new int[] { 18, 33, 25, 26 }; //error is 25 and 26, two houses, outside left
    private static readonly int[] sariaHints = new int[] { 50, 28 };//moving middle screen, sleeping thing, stationary right
    private static readonly int[] sariaHintsWithBaguWoods = new int[] { 28 };
    private static readonly int[] kingsTomb = new int[] { 51 };
    private static readonly int[] midoHints = new int[] { 45 };//moving old lady left, moving kid middle, inside house right
    private static readonly int[] nabooruHints = new int[] { 67, 64, 97 };//inside house right, moving bagu middle, stationary left, moving left, persistent left
    private static readonly int[] daruniaHints = new int[] { 77, 73 }; //wall first screen, outside last screen
    private static readonly int[] newkasutoHints = new int[] { 83, 68, 92 }; //outside first screen, wall first screen
    private static readonly int[] oldkasutoHint = new int[] { 74 };

    private static readonly int[] rauruMoving = { 9, 10, };
    private static readonly int[] rutoMoving = { 19, 17 };
    private static readonly int[] sariaMoving = { 27 };
    private static readonly int[] movingMido = { 40, 39 };
    private static readonly int[] movingNabooru = { 61, 60 };
    private static readonly int[] daruniaMoving = { 72, 75, };
    private static readonly int[] newkasutoMoving = { 88, 89 };

    //Indexes in the hints list
    private const int rauruSign = 11;
    private const int rutoSign = 20;
    private const int sariaSign = 29;
    private const int midoSign = 41;
    private const int nabooruSign = 62;
    private const int daruniaSign = 76;
    private const int newKasutoSign = 86;
    private const int oldKasutoSign = 94;

    private const int maxTextLength = 3134;
    private const int numberOfTextEntries = 98;
    private const int baguTextIndex = 48;
    private const int bridgeTextIndex = 37;
    private const int downstabTextIndex = 47;
    private const int upstabTextIndex = 82;
    private const int trophyIndex = 13;
    private const int medIndex = 43;
    private const int kidIndex = 79;
    private const int numberOfHints = 4;

    private const int errorTextIndex1 = 25;
    private const int errorTextIndex2 = 26;

    private static List<int> usedWizardHints = new List<int>();
    private static readonly int[] wizardindex = { 15, 24, 35, 46, 70, 81, 93, 96 };

    private static readonly int[][] hintIndexes = { rauruHints, rutoHints, sariaHints, kingsTomb, midoHints, nabooruHints, daruniaHints, newkasutoHints, oldkasutoHint };

    private static readonly String[] wizardTexts =
    {
        "do you know$why we$stopped$the car?",
        "link...$i am your$father",
        "I like big$bots and i$cannot lie",
        "why am i$locked in$a basement",
        "thats just$like your$opinion$man",
        "the dude$abides",
        "i hope$this isnt$fire spell",
        "boy this$is really$expensive",
        "10th enemy$has the$bomb",
        "stay$awhile and$listen",
        "Dude eff$this game",
        "you teach$me a spell",
        "you know$nothing",
        "thats what$she said",
        "lets throw$a rave",
        "jump in$lava for$200 rupees",
        "you wont$be able$to cast$this",
        "big bucks$no whammys",
        "bagu owes$me 20$rupees",
        "you are$the$weakest$link",
        "link i$am your$father",
        "theres no$wifi here",
        "a wild$link$appears",
        "welcome$to walmart",
        "whats the$wifi$password",
        "dont send$me back to$the home",
        "pull my$finger",
        "id like$to buy a$vowel",
        "i only$know one$spell",
        "i went$to$college$for this",
        "larry is$still in$northern$palace",
        "this game$needs more$categories",
        "who$picked$these$flags",
        "i found$this in$the$garbage",
        "have you$heard my$mixtape"
    };

    private static readonly String[] bridgetext = {
        "bagu said$what? that$jerk!",
        "try not$to drown",
        "who is$bagu? i$dont know$any bagu",
        "3 5 10 7$12 4 11 6$1 13 14 2$15 8 9",
        "why cant$you swim?",
        "what is$your$quest?",
        "what is$your$favorite$color?",
        "what is$the speed$of a laden$swallow?",
        "tickets$please",
        "you know$magoo? i$can help$you cross",
        "boom boom$boom",
        "WRAAAAAAFT"
        };

    private static readonly String[] bagutext =
    {
        "have you$seen error$around?",
        "tell the$riverman$i said hes$an idiot",
        "i am bagu.$husband$of$baguette",
        "wanna see$a corpse?",
        "aliens$are real",
        "rupees are$mind$control$devices",
        "would you$like a$cookie?",
        "anybody$want a$peanut?",
        "please dont$tell my$wife i am$here",
        "bam bam$bam",
        "ASL?",
    };

    private static readonly String[] downstabtext =
    {
        "stick them$with the$pointy end",
        "youll stab$your eye$out",
        "press down$you idiot",
        "have a$pogo stick",
        "yakhammer$acquired",
        "press down$to crouch",
        "press$dongward$to stab",
        "kick punch$chop block$duck jump",
        "jump crouch$its all in$the mind!",
        "you walked$past me$didnt you"
    };

    private static readonly String[] upstabtext =
    {
        "bet you$wish this$was$downstab",
        "you$probably$wont need$this",
        "press up$you idiot",
        "press up$to go in$doors",
        "are you$santa$claus?",
        "SHORYUKEN!",
        "you wasted$your time"
    };
    public static void Reset()
    {
        usedWizardHints = new List<int>();
    }

    public static List<Hint> GenerateHints(
        List<Location> itemLocs, 
        bool startsWithTrophy, 
        bool startsWithMedicine, 
        bool startsWithKid, 
        Dictionary<Spell, Spell> spellMap, 
        Location bagu,
        List<Hint> hints,
        RandomizerProperties props,
        Random r)
    {
        //
        if (props.DashSpell)
        {
            hints[70] = new Hint(Util.ToGameText("USE THIS$TO GO$FAST", true));
        }
        if (props.UseCommunityHints)
        {
            GenerateCommunityHints(hints, r);
        }

        if (props.SpellItemHints)
        {
            GenerateSpellHints(itemLocs, hints, startsWithTrophy, startsWithMedicine, startsWithKid);
        }

        List<int> placedIndex = new List<int>();
        if (props.BagusWoods)
        {
            hints[baguText] = GenerateBaguHint(bagu);
            //sariaHints.Remove(baguText);
        }
        if (props.HelpfulHints)
        {
            placedIndex = GenerateHelpfulHints(hints, itemLocs, r, props.SpellItemHints);
        }

        if (props.HelpfulHints)
        {
            GenerateKnowNothings(hints, placedIndex, props.BagusWoods);
        }

        if (props.TownNameHints)
        {
            GenerateTownNameHints(hints, spellMap, props.DashSpell);
        }

        if (props.SwapUpAndDownStab)
        {
            (hints[upstabTextIndex], hints[downstabTextIndex]) = (hints[downstabTextIndex], hints[upstabTextIndex]);
        }

        return hints;
    }
    private static Hint GenerateBaguHint(Location bagu)
    {
        int baguy = bagu.Ypos - 30;
        int bagux = bagu.Xpos;
        String hint = "BAGU IN$";
        if (baguy < 25)
        {
            if (bagux < 21)
            {
                hint += "NORTHWEST$";
            }
            else if (bagux < 42)
            {
                hint += "NORTH$";
            }
            else
            {
                hint += "NORTHEAST$";
            }
        }
        else if (baguy < 50)
        {
            if (bagux < 21)
            {
                hint += "WEST$";
            }
            else if (bagux < 42)
            {
                hint += "CENTER";
            }
            else
            {
                hint += "EAST$";
            }
        }
        else
        {
            if (bagux < 21)
            {
                hint += "SOUTHWEST$";
            }
            else if (bagux < 42)
            {
                hint += "SOUTH$";
            }
            else
            {
                hint += "SOUTHEAST$";
            }
        }
        hint += "WOODS";
        Hint baguH = new Hint(Util.ToGameText(hint, true));
        return baguH;
    }

    private static void GenerateTownNameHints(List<Hint> hints, Dictionary<Spell, Spell> spellMap, bool useDash)
    {
        Hint h = new Hint();
        h.GenerateTownHint(spellMap[Spell.SHIELD], useDash);
        hints[rauruSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.JUMP], useDash);
        hints[rutoSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.LIFE], useDash);
        hints[sariaSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.FAIRY], useDash);
        hints[midoSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.FIRE], useDash);
        hints[nabooruSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.REFLECT], useDash);
        hints[daruniaSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.SPELL], useDash);
        hints[newKasutoSign] = h;

        h = new Hint();
        h.GenerateTownHint(spellMap[Spell.THUNDER], useDash);
        hints[oldKasutoSign] = h;
    }

    public static Hint GenerateCommunityHint(HintType type, Random r)
    {
        switch (type)
        {
            case HintType.WIZARD:
                int selectedHintIndex = r.Next(wizardTexts.Count());
                while (usedWizardHints.Contains(selectedHintIndex))
                {
                    selectedHintIndex = r.Next(wizardTexts.Count());
                }
                usedWizardHints.Add(selectedHintIndex);
                return new Hint(Util.ToGameText(wizardTexts[selectedHintIndex], true));
            case HintType.BAGU:
                return new Hint(Util.ToGameText(bagutext[r.Next(bagutext.Count())], true));
            case HintType.BRIDGE:
                return new Hint(Util.ToGameText(bridgetext[r.Next(bridgetext.Length)], true));
            case HintType.DOWNSTAB:
                return new Hint(Util.ToGameText(downstabtext[r.Next(downstabtext.Length)], true));
            case HintType.UPSTAB:
                return new Hint(Util.ToGameText(upstabtext[r.Next(upstabtext.Length)], true));
            default:
                throw new Exception("Invalid Hint Type");
        }
    }

    private static void GenerateKnowNothings(List<Hint> hints, List<int> placedIndex, bool useBaguWoods)
    {
        List<int> stationary = new List<int>();
        stationary.AddRange(rauruHints);
        stationary.AddRange(rutoHints);
        stationary.AddRange(useBaguWoods ? sariaHintsWithBaguWoods : sariaHints);
        stationary.AddRange(midoHints);
        stationary.AddRange(nabooruHints);
        stationary.AddRange(daruniaHints);
        stationary.AddRange(newkasutoHints);
        stationary.AddRange(kingsTomb);
        stationary.AddRange(oldkasutoHint);

        List<int> moving = new List<int>();
        moving.AddRange(rauruMoving);
        moving.AddRange(rutoMoving);
        moving.AddRange(sariaMoving);
        moving.AddRange(movingMido);
        moving.AddRange(movingNabooru);
        moving.AddRange(daruniaMoving);
        moving.AddRange(newkasutoMoving);

        Hint knowNothing = new Hint();
        for (int i = 0; i < stationary.Count(); i++)
        {
            if (!placedIndex.Contains(stationary[i]))
            {
                hints[stationary[i]] = knowNothing;
            }
        }

        for (int i = 0; i < moving.Count(); i++)
        {
            hints[moving[i]] = knowNothing;
        }
    }

    private static List<int> GenerateHelpfulHints(List<Hint> hints, List<Location> itemLocs, Random r, bool useSpellItemHints)
    {
        List<int> placedIndex = new List<int>();

        List<Item> placedItems = new List<Item>();
        bool placedSmall = false;
        List<Item> smallItems = new List<Item> { Item.BLUE_JAR, Item.XL_BAG, Item.KEY, Item.MEDIUM_BAG, Item.MAGIC_CONTAINER, Item.HEART_CONTAINER, Item.ONEUP, Item.RED_JAR, Item.SMALL_BAG, Item.LARGE_BAG };
        List<int> placedTowns = new List<int>();

        List<Item> it = new List<Item>();
        for (int i = 0; i < itemLocs.Count(); i++)
        {
            it.Add(itemLocs[i].item);
        }

        if (useSpellItemHints)
        {
            it.Remove(Item.TROPHY);
            it.Remove(Item.CHILD);
            it.Remove(Item.MEDICINE);
        }

        for (int i = 0; i < numberOfHints; i++)
        {
            Item doThis = it[r.Next(it.Count())];
            int tries = 0;
            while (((placedSmall && smallItems.Contains(doThis)) || placedItems.Contains(doThis)) && tries < 1000)
            {
                doThis = it[r.Next(it.Count())];
                tries++;
            }
            int j = 0;
            while (itemLocs[j].item != doThis)
            {
                j++;
            }
            Hint hint = new Hint();
            hint.GenerateHelpfulHint(itemLocs[j]);
            int town = r.Next(9);
            while (placedTowns.Contains(town))
            {
                town = r.Next(9);
            }
            int index = hintIndexes[town][r.Next(hintIndexes[town].Count())];
            if (index == errorTextIndex1 || index == errorTextIndex2)
            {
                hints[errorTextIndex1] = hint;
                hints[errorTextIndex2] = hint;
                placedIndex.Add(errorTextIndex1);
                placedIndex.Add(errorTextIndex2);
            }
            else
            {
                hints[index] = hint;
                placedIndex.Add(index);
            }

            placedTowns.Add(town);
            placedItems.Add(doThis);
            if (smallItems.Contains(doThis))
            {
                placedSmall = true;
            }

        }
        return placedIndex;
    }

    private static void GenerateSpellHints(List<Location> itemLocs, List<Hint> hints, bool startsWithTrophy, bool startsWithMedicine, bool startsWithKid)
    {

        foreach (Location itemLocation in itemLocs)
        {
            if (itemLocation.item == Item.TROPHY && !startsWithTrophy)
            {
                Hint trophyHint = new Hint();
                trophyHint.GenerateHelpfulHint(itemLocation);
                hints[trophyIndex] = trophyHint;
            }
            else if (itemLocation.item == Item.MEDICINE && !startsWithMedicine)
            {
                Hint medHint = new Hint();
                medHint.GenerateHelpfulHint(itemLocation);
                hints[medIndex] = medHint;
            }
            else if (itemLocation.item == Item.CHILD && !startsWithKid)
            {
                Hint kidHint = new Hint();
                kidHint.GenerateHelpfulHint(itemLocation);
                hints[kidIndex] = kidHint;
            }
        }
    }

    private static void GenerateCommunityHints(List<Hint> hints, Random r)
    {
        Hints.Reset();
        do
        {
            for (int i = 0; i < 8; i++)
            {
                Hint wizardHint = GenerateCommunityHint(HintType.WIZARD, r);
                hints.RemoveAt(wizardindex[i]);
                hints.Insert(wizardindex[i], wizardHint);

            }

            Hint baguHint = GenerateCommunityHint(HintType.BAGU, r);
            hints[baguTextIndex] = baguHint;

            Hint bridgeHint = GenerateCommunityHint(HintType.BRIDGE, r);
            hints[bridgeTextIndex] = bridgeHint;

            Hint downstabHint = GenerateCommunityHint(HintType.DOWNSTAB, r);
            hints[downstabTextIndex] = downstabHint;

            Hint upstabHint = GenerateCommunityHint(HintType.UPSTAB, r);
            hints[upstabTextIndex] = upstabHint;

        } while (TextLength(hints) > maxTextLength);
    }

    private static int TextLength(List<Hint> texts)
    {
        int sum = 0;
        for (int i = 0; i < texts.Count(); i++)
        {
            sum += texts[i].Text.Count;
        }
        return sum;
    }
}
