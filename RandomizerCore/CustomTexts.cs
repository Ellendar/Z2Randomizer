using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

public class CustomTexts
{
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
    //TODO: This should just be a listing of every text index by continent, but for now it's a pile.
    private const int baguTextIndex = 48;
    private const int bridgeTextIndex = 37;
    private const int riverManTextIndex = 36;
    //private const int downstabTextIndex = 47;
    //private const int upstabTextIndex = 82;
    private const int trophyIndex = 13;
    private const int medIndex = 43;
    private const int kidIndex = 79;
    private const int HELPFUL_HINTS_COUNT = 4;

    private const int errorTextIndex1 = 25;
    private const int errorTextIndex2 = 26;

    //private static readonly Dictionary<Town, int> spellTextIndexes = { 15, 24, 35, 46, 70, 81, 93, 96 };
    private static readonly Dictionary<Town, int> spellTextIndexes = new()
    {
        { Town.RAURU, 15 },
        { Town.RUTO, 24 },
        { Town.SARIA_NORTH, 35 },
        { Town.MIDO_WEST, 46 },
        { Town.MIDO_CHURCH, 47 },
        { Town.NABOORU, 70 },
        { Town.DARUNIA_ROOF, 82 },
        { Town.DARUNIA_WEST, 81 },
        { Town.NEW_KASUTO, 93 },
        { Town.OLD_KASUTO, 96 },
    };

    private static readonly int[][] hintIndexes = { rauruHints, rutoHints, sariaHints, kingsTomb, midoHints, nabooruHints, daruniaHints, newkasutoHints, oldkasutoHint };

    public static readonly string[] GENERIC_WIZARD_TEXTS =
    {
        "do you know$why we$stopped$the car?",
        "link...$i am your$father",
        "I like big$bots and i$cannot lie",
        "why am i$locked in$a basement",
        "thats just$like your$opinion$man",
        "the dude$abides",
        "boy this$is really$expensive",
        "10th enemy$has the$bomb",
        "stay$awhile and$listen",
        "you teach$me a spell",
        "you know$nothing",
        "thats what$she said",
        "jump in$lava for$200 rupees",
        "you wont$be able$to cast$this",
        "big bucks$no whammys",
        "bagu owes$me 20$rupees",
        "you are$the$weakest$link",
        "link i$am your$father",
        "theres no$wifi here",
        "a wild$link$appears",
        "whats the$wifi$password",
        "dont send$me back to$the home",
        "id like$to buy a$vowel",
        "i only$know one$spell",
        "i went$to$college$for this",
        "this game$needs more$categories",
        "who$picked$these$flags",
        "i found$this in$the$garbage",
        "have you$heard my$mixtape",
        "Does this$robe make$me look$fat?",
        "No pom pom$shaking$here",
        "Youre$a wizard,$Link",
        "Take any$Robe you$want",
        "The child$can stay$lost",
        "Dont Move$I dropped$a contact$lens",
        "Please$support$ZSR",
        "This wont$hurt a bit",
        "How many$shakes can$a digshake$shake?",
        "Free$your mind",
        "da$na na na$naaaaaaaaa",
        "Join the$Nintendo$Power$Club",
        "Silvers$are in$palace 1",
        "Needs$more$Cowbell",
        "Which$timeline$is this?",
        "Hurry!$I have to$preheat$the oven",
        "Poyo!",
        "Sploosh$Kaboom!",
        "Let me$read my$Vogon$Poetry"
    };

    public static readonly string[] RIVER_MAN_TEXTS = 
    {
        "bagu said$what? that$jerk!",
        "try not$to drown",
        "who is$bagu? i$dont know$any bagu",
        "3 5 10 7$12 4 11 6$1 13 14 2$15 8 9",
        "why cant$you swim?",
        "what is$your$quest?",
        "tickets$please",
        "WRAAAAAAFT",
        "Which way$to Denver?",
        "Do you know$the way to$San Jose?",
        "Do you$knowThe$Muffin Man",
        "Can we$fix it?",
        "What?$You cant$swim?",
        "Link.exe$has$stopped$working",
        "No running$by$the pool",
    };

    public static readonly string[] BAGU_TEXTS =
    {
        "have you$seen error$around?",
        "tell the$riverman$i said hes$an idiot",
        "wanna see$a corpse?",
        "aliens$are real",
        "rupees are$mind$control$devices",
        "please dont$tell my$wife i am$here",
        "bam bam$bam",
        "Here is$my list of$demands",
        "my email to$River Man$was in$my drafts",
        "Hey!$Listen!",
        "Pizza$dudes got$thirty$seconds",
        "I am$Batman",
        "I am$Groot",
        "BAGU$SMAAAAASH",
        "Get out$of ma$swamp!!",
        "Praise$the sun",
        "am I$being$detained?",
        "Error$is the$evil twin",
        "Tingle$Tingle$Kooloo$Limpah!",
        "Is this a$pedestal$seed?",
        "Does$Spec rock$wear$glasses?",
        "Everyone$gets a$bridge",
    };

    public static readonly string[] DOWNSTAB_TEXTS =
    {
        "stick them$with the$pointy end",
        "youll stab$your eye$out",
        "press down$you idiot",
        "have a$pogo stick",
        "yakhammer$acquired",
        "press down$to crouch",
        "kick punch$chop block$duck jump",
        "jump crouch$its all in$the mind!",
        "you walked$past me$didnt you",
        "upstab is$the best$stab",
        "Do the$Safety$Dance",
        "easy mode$activated",
        "Never$gonna give$you up",
        "Are you$Scrooge$McDuck?"
    };

    public static readonly string[] UPSTAB_TEXTS =
    {
        "bet you$wish this$was$downstab",
        "you$probably$wont need$this",
        "press up$you idiot",
        "press up$to go in$doors",
        "are you$santa$claus?",
        "SHORYUKEN!",
        "you wasted$your time?",
        "Mario can$do this$without$magic",
        "downstab$is the$best stab",
        "Tiger$Uppercut!",
        "Never$gonna let$you down",
        "Thanks$for not$skipping$me",
    };

    public static readonly string[] KNOW_NOTHING_TEXTS =
    {
        "I Know$Nothing",
        "Knowledge$Is Not$Mine",
        "I Like$Wasting$Your Time",
        "This Is$About As$Useful As$I Am",
        "Nothing$Know I",
        "Try To$Get A$Guide",
        "Git Gud",
        "What?$Yeah!$Okay!",
        "No hint$for you",
        "What$timeline$is this?",
        "your$call is$important$please hold",
        "silence$is golden",
        "Bless you",
        "Hola!",
        "I am not$a vire$in$disguise",
        "Woah!$Dude!",
        "PAY ME$AND I'LL$TALK"
    };

    public static readonly string[] NOT_ENOUGH_CONTAINERS_TEXT =
    {
        "all signs$point to$no",
        "come back$as$adult link",
        "quit$wasting$my time",
        "Youre$sixteen$pixels$short",
        "Do you$have a$diploma?",
        "The magic$class did$not help$you enough",
        "Show me$your$credits!",
        "I cannot$contain$my laughter",
        "You must$construct$addtional$pylons",
        "bet you$forgot$this flag$was on",
        "I'll tell$you when$you're$older"
    };

    public static readonly Dictionary<Town, string[]> WIZARD_TEXTS_BY_TOWN = new()
    {
        { Town.RAURU, new string[] { } },
        { Town.RUTO, new string[] { "A winner$is you"} },
        { Town.SARIA_NORTH, new string[] { "Water$you$doing?" } },
        { Town.MIDO_WEST, new string[] { } },
        { Town.MIDO_CHURCH, new string[] { } },
        { Town.NABOORU, new string[] { } },
        { Town.DARUNIA_ROOF, new string[] { } },
        { Town.DARUNIA_WEST, new string[] { "You saved$a kid$for this?", "Dont$forget to$get upstab" } },
        { Town.NEW_KASUTO, new string[] { } },
        { Town.OLD_KASUTO, new string[] { "Sorry$about the$moas" } }
    };

    public static readonly Dictionary<Spell, string[]> WIZARD_TEXTS_BY_SPELL = new()
    {
        { Spell.SHIELD, new string[] { "Have you$tried not$dying?", "I Already$Have One", "Is This$A Red$Ring?" } },
        { Spell.JUMP, new string[] { "I get up$and nothin$gets me$down", "Kris Kross$will make$you..." } },
        { Spell.LIFE, new string[] { "have you$tried the$Healmore$spell?", "Dont$blame me$if this is$1 bar", "How Many$Bars Will$I Heal" } },
        { Spell.FAIRY, new string[] { "HEY!$LISTEN", "Just$don't say$Hey$listen!", "Watch Out$For Iron" } },
        { Spell.FIRE, new string[] { "this is$fine", "use this$to burn$gems", "This spell$is$worthless", "Goodness$Gracious!", "This one$goes out$to the one$I love" } },
        { Spell.DASH, new string[] { "Rolling$around at$the speed$of sound", "Gotta$Go$Fast", "Use the$boost to$get through!"  } },
        { Spell.REFLECT, new string[] { "I am not$Mirror$Shield", "Crysta$was$here", "You're$rubber,$They're$glue", "Send$Carock my$regards", "Is This$Hera$Basement?" } },
        { Spell.SPELL, new string[] { "Titular$redundancy$included", "Wait?$which$spell?", "you should$rescue me$instead of$Zelda", "Can you$use it$in a$sentence?", "Metamorph$Thy Enemy" } },
        { Spell.THUNDER, new string[] { "With this$you can$now beat$the game", "Ultrazord$Power Up!", "Terrible$terrible$damage" } },
        { Spell.UPSTAB, UPSTAB_TEXTS },
        { Spell.DOWNSTAB, DOWNSTAB_TEXTS }
    };

    public static List<Text> GenerateTexts(
        List<Location> itemLocs, 
        bool startsWithTrophy, 
        bool startsWithMedicine, 
        bool startsWithKid, 
        Dictionary<Town, Spell> spellMap, 
        Location bagu,
        List<Text> texts,
        RandomizerProperties props,
        Random r)
    {
        //
        if (props.ReplaceFireWithDash)
        {
            texts[70] = new Text(Util.ToGameText("USE THIS$TO GO$FAST", true));
        }
        if(props.SwapUpAndDownStab)
        {
            (texts[spellTextIndexes[Town.DARUNIA_ROOF]], texts[spellTextIndexes[Town.MIDO_CHURCH]]) =
                (texts[spellTextIndexes[Town.MIDO_CHURCH]], texts[spellTextIndexes[Town.DARUNIA_ROOF]]);
        }
        if (props.UseCommunityText)
        {
            GenerateCommunityText(texts, spellMap, r);
        }

        if (props.SpellItemHints)
        {
            GenerateSpellHints(itemLocs, texts, startsWithTrophy, startsWithMedicine, startsWithKid);
        }

        List<int> placedIndex = new List<int>();
        if (props.BagusWoods)
        {
            texts[riverManTextIndex] = GenerateBaguHint(bagu);
            //sariaHints.Remove(baguText);
        }
        if (props.HelpfulHints)
        {
            placedIndex = GenerateHelpfulHints(texts, itemLocs, r, props.SpellItemHints);
            GenerateKnowNothings(texts, placedIndex, r, props.BagusWoods, props.UseCommunityText);
        }

        if (props.UseCommunityText)
        {
            //Generate replacements for "COME BACK WHEN YOU ARE READY" that is displayed when you don't have
            //enough magic containers and container requirements are on.
            texts[17] = new Text(NOT_ENOUGH_CONTAINERS_TEXT.Sample(r));
        }

        if (props.TownNameHints)
        {
            GenerateTownNameHints(texts, spellMap, props.CombineFire);
        }

        return texts;
    }
    private static Text GenerateBaguHint(Location bagu)
    {
        int baguy = bagu.Ypos - 30;
        int bagux = bagu.Xpos;
        string hint = "BAGU IN$";
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
        Text baguHint = new Text(Util.ToGameText(hint, true));
        return baguHint;
    }

    private static void GenerateTownNameHints(List<Text> texts, Dictionary<Town, Spell> spellMap, bool linkedFire)
    {
        texts[rauruSign] = GenerateTownHint(spellMap[Town.RAURU], linkedFire);
        texts[rutoSign] = GenerateTownHint(spellMap[Town.RUTO], linkedFire);
        texts[sariaSign] = GenerateTownHint(spellMap[Town.SARIA_NORTH], linkedFire);
        texts[midoSign] = GenerateTownHint(spellMap[Town.MIDO_WEST], linkedFire);
        texts[nabooruSign] = GenerateTownHint(spellMap[Town.NABOORU], linkedFire);
        texts[daruniaSign] = GenerateTownHint(spellMap[Town.DARUNIA_WEST], linkedFire);
        texts[newKasutoSign] = GenerateTownHint(spellMap[Town.NEW_KASUTO], linkedFire);
        texts[oldKasutoSign] = GenerateTownHint(spellMap[Town.OLD_KASUTO], linkedFire);
    }

    public static Text GenerateTownHint(Spell spell, bool linkedFire)
    {
        string text = "";
        switch (spell)
        {
            case Spell.SHIELD:
                text += "shield$";
                break;
            case Spell.JUMP:
                text += "jump";
                break;
            case Spell.LIFE:
                text += "life$";
                break;
            case Spell.FAIRY:
                text += "fairy$";
                break;
            case Spell.FIRE:
                text += "fire" + (linkedFire ? "!$" : "$");
                break;
            case Spell.DASH:
                text += "dash$";
                break;
            case Spell.REFLECT:
                text += "reflect$";
                break;
            case Spell.SPELL:
                text += "spell$";
                break;
            case Spell.THUNDER:
                text += "thunder$";
                break;

        }
        text += "town";
        return new Text(text);
    }

    public static Text GenerateCommunityText(HintType type, Random r, Town? town = null, Spell? spell = null)
    {
        switch (type)
        {
            case HintType.WIZARD:
                if (town == null || spell == null)
                {
                    throw new ArgumentException("Spell/Town is required to generate wizard text");
                }
                List<string> possibleWizardHints = GENERIC_WIZARD_TEXTS
                    .Union(WIZARD_TEXTS_BY_TOWN[town ?? Town.RAURU])
                    .Union(WIZARD_TEXTS_BY_SPELL[spell ?? Spell.SHIELD]).ToList();
                int selectedHintIndex = r.Next(possibleWizardHints.Count());
                return new Text(possibleWizardHints[selectedHintIndex]);
            case HintType.BAGU:
                return new Text(BAGU_TEXTS[r.Next(BAGU_TEXTS.Count())]);
            case HintType.BRIDGE:
                return new Text(RIVER_MAN_TEXTS[r.Next(RIVER_MAN_TEXTS.Length)]);
            default:
                throw new Exception("Invalid Hint Type");
        }
    }

    private static void GenerateKnowNothings(List<Text> hints, List<int> placedIndex, Random r, bool useBaguWoods, bool useCommunityText)
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

        Text defaultKnowNothing = new();
        for (int i = 0; i < stationary.Count; i++)
        {
            if (!placedIndex.Contains(stationary[i]))
            {
                hints[stationary[i]] = useCommunityText ? new Text(KNOW_NOTHING_TEXTS[r.Next(KNOW_NOTHING_TEXTS.Length)]) : defaultKnowNothing;
            }
        }

        for (int i = 0; i < moving.Count; i++)
        {
            hints[moving[i]] =  useCommunityText ? new Text(KNOW_NOTHING_TEXTS[r.Next(KNOW_NOTHING_TEXTS.Length)]) : defaultKnowNothing;
        }
    }

    private static List<int> GenerateHelpfulHints(List<Text> hints, List<Location> itemLocs, Random r, bool useSpellItemHints)
    {
        List<int> placedIndex = new List<int>();

        List<Item> placedItems = new List<Item>();
        bool placedSmall = false;
        List<Item> smallItems = new List<Item> { Item.BLUE_JAR, Item.XL_BAG, Item.KEY, Item.MEDIUM_BAG, Item.MAGIC_CONTAINER, Item.HEART_CONTAINER, Item.ONEUP, Item.RED_JAR, Item.SMALL_BAG, Item.LARGE_BAG };
        List<int> placedTowns = new List<int>();

        List<Item> items = itemLocs.Select(i => i.Item).ToList();

        if (useSpellItemHints)
        {
            items.Remove(Item.TROPHY);
            items.Remove(Item.CHILD);
            items.Remove(Item.MEDICINE);
        }

        for (int i = 0; i < HELPFUL_HINTS_COUNT; i++)
        {
            Item doThis = items[r.Next(items.Count)];
            int tries = 0;
            while (((placedSmall && smallItems.Contains(doThis)) || placedItems.Contains(doThis)) && tries < 1000)
            {
                doThis = items[r.Next(items.Count)];
                tries++;
            }
            int j = 0;
            while (itemLocs[j].Item != doThis)
            {
                j++;
            }
            Text hint = new();
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

    private static void GenerateSpellHints(List<Location> itemLocs, List<Text> hints, bool startsWithTrophy, bool startsWithMedicine, bool startsWithKid)
    {

        foreach (Location itemLocation in itemLocs)
        {
            if (itemLocation.Item == Item.TROPHY && !startsWithTrophy)
            {
                Text trophyHint = new Text();
                trophyHint.GenerateHelpfulHint(itemLocation);
                hints[trophyIndex] = trophyHint;
            }
            else if (itemLocation.Item == Item.MEDICINE && !startsWithMedicine)
            {
                Text medHint = new Text();
                medHint.GenerateHelpfulHint(itemLocation);
                hints[medIndex] = medHint;
            }
            else if (itemLocation.Item == Item.CHILD && !startsWithKid)
            {
                Text kidHint = new Text();
                kidHint.GenerateHelpfulHint(itemLocation);
                hints[kidIndex] = kidHint;
            }
        }
    }

    private static void GenerateCommunityText(List<Text> hints, Dictionary<Town, Spell> spellMap, Random r)
    {
        List<Text> usedWizardHints = new List<Text>();
        do
        {
            foreach(Town town in spellMap.Keys)
            {
                Text wizardHint;
                Spell spell = spellMap[town];
                do
                {
                    wizardHint = GenerateCommunityText(HintType.WIZARD, r, town, spell);
                } while (usedWizardHints.Contains(wizardHint));
                //TODO: eventually when up/downstab are in the generic spell pool, they can use generic hints again.
                if(spell == Spell.DOWNSTAB)
                {
                    hints[spellTextIndexes[town]] = new Text(DOWNSTAB_TEXTS[r.Next(DOWNSTAB_TEXTS.Length)]);
                    continue;
                }
                if (spell == Spell.UPSTAB)
                {
                    hints[spellTextIndexes[town]] = new Text(UPSTAB_TEXTS[r.Next(UPSTAB_TEXTS.Length)]);
                    continue;
                }
                usedWizardHints.Add(wizardHint);
                hints[spellTextIndexes[town]] = wizardHint;
            }

            Text baguHint = GenerateCommunityText(HintType.BAGU, r);
            hints[baguTextIndex] = baguHint;

            Text bridgeHint = GenerateCommunityText(HintType.BRIDGE, r);
            hints[bridgeTextIndex] = bridgeHint;
        //TODO: Add in a routine that cleans out any extraneous scripts that were either unused in the NA version
        //      Or are now unused with the randomizer.
        } while (TextLength(hints) > maxTextLength);
    }

    private static int TextLength(List<Text> texts)
    {
        int sum = 0;
        for (int i = 0; i < texts.Count(); i++)
        {
            sum += texts[i].TextChars.Count;
        }
        return sum;
    }
}
