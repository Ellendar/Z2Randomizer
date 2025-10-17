using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using NLog;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

public class CustomTexts
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int errorTextIndex1 = 25; // Error inside house on 1st screen in Ruto, 1st message
    private const int errorTextIndex2 = 26; // Error 2nd message

    private const int talkingBotIndexSleeping = 49; // Sleeping Bot in Saria inside house on 2nd screen, Zzz... message the first 3 talks
    private const int talkingBotIndexTalking = 50; // Sleeping Bot final message

    private const int talkingAcheIndexSleeping = 85; // Ache in Nabooru inside house on 3rd screen, ***** message the first 3 talks
    private const int talkingAcheIndexTalking = 97; // Ache final message

    private static readonly int[] rauruHints = [32 /* lady outside on 1st screen */, 30 /* man inside house on 1st screen */, 12 /* kid inside house on 2nd screen */];
    private static readonly int[] rutoHints = [errorTextIndex1, 18 /* lady outside on 2nd screen */, 33 /* woman inside on 2nd screen */];
    private static readonly int[] sariaHints = [28 /* greeting man at town entrance */, talkingBotIndexTalking];
    private static readonly int[] midoHints = [45 /* man inside house on 1st screen */];
    private static readonly int[] kingsTomb = [51];

    private static readonly int[] nabooruHints = [67 /* man inside house on 1st screen */, 64 /* lady outside on 3rd screen */, talkingAcheIndexTalking];
    private static readonly int[] daruniaHints = [77 /* text on wall inside house on 1st screen */, 73 /* kid outside on 3rd screen */];
    private static readonly int[] newkasutoHints = [83 /* greeting lady at town entrance */, 68 /* text on wall inside house on 1st screen */, /*92 unreachable Lady in Magic Container house (removed) */];
    private static readonly int[] oldkasutoHint = [74 /* readable wall inside last house on 2nd screen */];

    private static readonly int[] rauruMoving = [9, 10];
    private static readonly int[] rutoMoving = [17, 19];
    private static readonly int[] sariaMoving = [27];
    private static readonly int[] movingMido = [40, 39];
    private static readonly int[] movingNabooru = [61, 60];
    private static readonly int[] daruniaMoving = [72, 75];
    private static readonly int[] newkasutoMoving = [88, 89];

    private static readonly Collectable[] smallItems = [
        Collectable.BLUE_JAR,
        Collectable.XL_BAG,
        Collectable.KEY,
        Collectable.MEDIUM_BAG,
        Collectable.MAGIC_CONTAINER,
        Collectable.HEART_CONTAINER,
        Collectable.ONEUP,
        Collectable.RED_JAR,
        Collectable.SMALL_BAG,
        Collectable.LARGE_BAG
    ];

    //Indexes in the hints list
    private static readonly Dictionary<Town, int> TOWN_SIGN_INDEXES = new()
    {
        {Town.RAURU, 11},
        {Town.RUTO, 20},
        {Town.SARIA_NORTH, 29},
        {Town.MIDO_WEST, 41},
        {Town.NABOORU, 62},
        {Town.DARUNIA_WEST, 76},
        {Town.NEW_KASUTO, 86},
        {Town.OLD_KASUTO, 94},
    };

    private const int MAX_TEXT_LENGTH = 3134;
    private const int numberOfTextEntries = 98;
    //TODO: This should just be a listing of every text index by continent, but for now it's a pile.
    private const int westAlreadyGotItemTextIndex = 16;
    private const int eastAlreadyGotItemTextIndex = 71;
    private const int bridgeTextIndex = 37;
    private const int riverManTextIndex = 36;
    private const int downstabClosedDoorTextIndex = 42;
    private const int downstabGuyGotItemTextIndex = 47;
    private const int upstabClosedDoorTextIndex = 78;
    private const int upstabGuyGotItemTextIndex = 82;
    private const int trophySpellHintIndex = 13;
    private const int medicineSpellHintIndex = 43;
    private const int childSpellHintIndex = 79;
    private const int waterSpellHintIndex = 65;
    private const int mirrorSpellHintIndex = 22;

    private const int baguTextIndex = 48;
    private const int gotWaterTextIndex = 63;
    private const int gotMirrorTextIndex = 21;

    private const int HELPFUL_HINTS_COUNT = 4;

    //private static readonly Dictionary<Town, int> spellTextIndexes = { 15, 24, 35, 46, 70, 81, 93, 96 };
    private static readonly Dictionary<Town, int> townWizardTextIndexes = new()
    {
        { Town.RAURU, 15 },
        { Town.RUTO, 24 },
        { Town.SARIA_NORTH, 35 },
        { Town.MIDO_WEST, 46 },
        { Town.MIDO_CHURCH, downstabGuyGotItemTextIndex },
        { Town.NABOORU, 70 },
        { Town.DARUNIA_ROOF, upstabGuyGotItemTextIndex },
        { Town.DARUNIA_WEST, 81 },
        { Town.NEW_KASUTO, 93 },
        { Town.OLD_KASUTO, 96 },
    };

    private static readonly Dictionary<Collectable, int> wizardTextIndexesBySpell = new()
    {
        { Collectable.SHIELD_SPELL, 15 },
        { Collectable.JUMP_SPELL, 24 },
        { Collectable.LIFE_SPELL, 35 },
        { Collectable.FAIRY_SPELL, 46 },
        { Collectable.DOWNSTAB, downstabGuyGotItemTextIndex },
        { Collectable.FIRE_SPELL, 70 },
        { Collectable.DASH_SPELL, 70 },
        { Collectable.UPSTAB, upstabGuyGotItemTextIndex },
        { Collectable.REFLECT_SPELL, 81 },
        { Collectable.SPELL_SPELL, 93 },
        { Collectable.THUNDER_SPELL, 96 },
    };

    private static readonly int[][] hintIndexes = [rauruHints, rutoHints, sariaHints, kingsTomb, midoHints, nabooruHints, daruniaHints, newkasutoHints, oldkasutoHint
    ];

    public static readonly string[] GENERIC_WIZARD_TEXTS =
    [
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
        "Let me$read my$Vogon$Poetry",
        "somebody$set up us$the bomb",
        "boat$league$confirmed"
    ];

    public static readonly string[] RIVER_MAN_TEXTS =
    [
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
        "No running$by$the pool"
    ];

    public static readonly string[] BAGU_TEXTS =
    [
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
        "Everyone$gets a$bridge"
    ];

    public static readonly string[] DOWNSTAB_TEXTS =
    [
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
    ];

    public static readonly string[] UPSTAB_TEXTS =
    [
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
        "The$Opportunity$Arises"
    ];

    public static readonly string[] KNOW_NOTHING_TEXTS =
    [
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
        "PAY ME$AND I'LL$TALK",
        "the hint$is in$another$castle",
        "did you$check the$old kasuto$hint?"
    ];

    public static readonly string[] NOT_ENOUGH_CONTAINERS_TEXT =
    [
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
    ];

    public static readonly Dictionary<Town, string[]> WIZARD_SPELL_TEXTS_BY_TOWN = new()
    {
        { Town.RAURU, [] },
        { Town.RUTO, ["A winner$is you"] },
        { Town.SARIA_NORTH, ["Water$you$doing?"] },
        { Town.MIDO_WEST, [] },
        { Town.MIDO_CHURCH, [] },
        { Town.NABOORU, [] },
        { Town.DARUNIA_ROOF, [] },
        { Town.DARUNIA_WEST, ["You saved$a kid$for this?", "Dont$forget to$get upstab"] },
        { Town.NEW_KASUTO, [] },
        { Town.OLD_KASUTO, ["Sorry$about the$moas"] }
    };

    public static readonly Dictionary<Collectable, string[]> WIZARD_SPELL_TEXTS_BY_COLLECTABLE = new()
    {
        { Collectable.SHIELD_SPELL, ["Have you$tried not$dying?", "I Already$Have One", "Is This$A Red$Ring?"] },
        { Collectable.JUMP_SPELL, ["I get up$and nothin$gets me$down", "Kris Kross$will make$you..."] },
        { Collectable.LIFE_SPELL, ["have you$tried the$Healmore$spell?", "Dont$blame me$if this is$1 bar", "How Many$Bars Will$I Heal"] },
        { Collectable.FAIRY_SPELL, ["HEY!$LISTEN", "Just$dont say$Hey$listen!", "Watch Out$For Iron"] },
        { Collectable.FIRE_SPELL, ["this is$fine", "use this$to burn$gems", "This spell$is$worthless", "Goodness$Gracious!", "This one$goes out$to the one$I love"]},
        { Collectable.DASH_SPELL, ["Rolling$around at$the speed$of sound", "Gotta$Go$Fast", "Use the$boost to$get through"]},
        { Collectable.REFLECT_SPELL, ["I am not$Mirror$Shield", "Crysta$was$here", "Youre$rubber,$Theyre$glue", "Send$Carock my$regards", "Is This$Hera$Basement?"]},
        { Collectable.SPELL_SPELL, ["Titular$redundancy$included", "Wait?$which$spell?", "you should$rescue me$instead of$Zelda", "Can you$use it$in a$sentence?", "Metamorph$Thy Enemy"]},
        { Collectable.THUNDER_SPELL, ["With this$you can$now beat$the game", "Ultrazord$Power Up!", "Terrible$terrible$damage", "hes dead$jim"]},
        { Collectable.UPSTAB, UPSTAB_TEXTS },
        { Collectable.DOWNSTAB, DOWNSTAB_TEXTS }
    };

    public static string DEFAULT_WIZARD_COLLECTABLE = "YOU GOT$THE$%%";

    public static string[] COMMUNITY_NONSPELL_GET_TEXT =
    [
        "GET$EQUIPPED$WITH THE$%",
        "Tis a good$Day for$%%",
        "I can't$believe$it's$%",
        "All Hail$%$heroes$rise again",
        "Congrats!$it's a$%%",
        "One Fish$Two Fish$Red Fish$%",
        "Master it$and you$can have$%",
        "%$it's what$plants$crave",
        "%$bagu$gives it$5 stars",
        "The$power of$%$is yours",
        "ganon is$jealous$of your$%",
        "the secret$to life is$%%",
        "Your love$is like$bad$%",
        "Screw the$rules I$have this$%",
        "excuse me$you forgot$this$%",
        "takes this!$%$now I can$retire",
        "you$gotta$have$%",
        "I love the$%%$its so bad",
        "don't feed$%$after$midnight",
        "take the$%$leave the$cannoli",
        "%$%$%$Yay!",
        "I will$give you$%$to go away",
        "%%$does not$spark joy",
        "lets talk$about$%$baby!",
        "When all$else fails$try$%",
        "%%$it's what's$for dinner",
        "%%$needs food$badly",
        "%%$detected",
        "Badger$Badger$Badger$%",
        "The$World$is a$%",
        "link$meets$%",
        "a brand$new$%$!!!",
        "Earth Fire$wind water$%",
        "happy$%$day!!",
        "%$%$never$changes",
        "%%$is$cheating",
        "This seed$sponsored$by$%",
        "we all$live in$a yellow$%",
        "its$%$time!",
        "fresh$%$50 rupees$obo",
        "no whammy$no whammy$and stop!$%",
        "all you$need is$%%",
        "the secret$word is$%",
        "have a$%$on the$house",
    ];

    public static List<Text> GenerateTexts(
        IEnumerable<Location> locations,
        IEnumerable<Location> itemLocations,
        List<Text> texts,
        RandomizerProperties props,
        Random hashRNG)
    {
        // Make a new RNG for community text so that it doesn't affect the final hash.
        var nonhashRNG = new Random(hashRNG.Next());
        do
        {
            if (props.ReplaceFireWithDash)
            {
                texts[70] = new Text("USE THIS$TO GO$FAST");
            }
            GenerateWizardTexts(texts, locations, nonhashRNG, props.UseCommunityText);

            if (props.SpellItemHints)
            {
                GenerateSpellHints(locations, texts, props);
            }

            Location baguLocation = locations.FirstOrDefault(i => i.ActualTown == Town.BAGU)!;
            if(baguLocation == null)
            {
                throw new Exception("Bagu location not found while generating text");
            }
            Text? baguText = GenerateBaguText(baguLocation.Collectables[0], nonhashRNG, props.UseCommunityText, props.IncludeQuestItemsInShuffle);
            if(baguText != null)
            {
                texts[baguTextIndex] = baguText;
            }

            Location tableLocation = locations.FirstOrDefault(i => i.ActualTown == Town.SARIA_TABLE)!;
            Text? mirrorText = GenerateMirrorTableText(tableLocation.Collectables[0], nonhashRNG, props.UseCommunityText, props.IncludeQuestItemsInShuffle);
            if (mirrorText != null)
            {
                texts[gotMirrorTextIndex] = mirrorText;
            }

            Location fountainLocation = locations.FirstOrDefault(i => i.ActualTown == Town.NABOORU_FOUNTAIN)!;
            Text? fountainText = GenerateFountainText(fountainLocation.Collectables[0], nonhashRNG, props.UseCommunityText, props.IncludeQuestItemsInShuffle);
            if (fountainText != null)
            {
                texts[gotWaterTextIndex] = fountainText;
            }

            if (props.BagusWoods)
            {
                texts[riverManTextIndex] = GenerateBaguWoodsHint(baguLocation);
            }
            else if(props.UseCommunityText)
            {
                texts[riverManTextIndex] = GenerateRiverManText(nonhashRNG);
            }

            if (props.SpellItemHints && props.IncludeSwordTechsInShuffle)
            {
                var downstabLoc = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.DOWNSTAB));
                var upstabLoc = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.UPSTAB));
                if (downstabLoc != null)
                {
                    Text hint = Text.GenerateHelpfulHint(locations.ToList(), downstabLoc, Collectable.DOWNSTAB, props.IncludeSpellsInShuffle);
                    texts[downstabClosedDoorTextIndex] = hint;
                }
                if (upstabLoc != null)
                {
                    Text hint = Text.GenerateHelpfulHint(locations.ToList(), upstabLoc, Collectable.UPSTAB, props.IncludeSpellsInShuffle);
                    texts[upstabClosedDoorTextIndex] = hint;
                }
                if (props.SwapUpAndDownStab)
                {
                    (texts[upstabClosedDoorTextIndex], texts[downstabClosedDoorTextIndex]) = (texts[downstabClosedDoorTextIndex], texts[upstabClosedDoorTextIndex]);
                }
            }

            if (props.HelpfulHints)
            {
                List<int> placedIndexes = GenerateHelpfulHints(texts, itemLocations, hashRNG, props);
                GenerateKnowNothings(texts, placedIndexes, nonhashRNG, props.BagusWoods, props.UseCommunityText);
            }

            if (props.UseCommunityText)
            {
                //Generate replacements for "COME BACK WHEN YOU ARE READY" that is displayed when you don't have
                //enough magic containers and container requirements are on.
                texts[17] = new Text(NOT_ENOUGH_CONTAINERS_TEXT.Sample(nonhashRNG)!);
                //Old kasuto guy has a different vanilla not enough containers message
                texts[95] = new Text(NOT_ENOUGH_CONTAINERS_TEXT.Sample(nonhashRNG)!);
            }

            if (props.TownNameHints)
            {
                GenerateTownNameHints(texts, locations, props.CombineFire);
            }
        } while (TextLength(texts) > MAX_TEXT_LENGTH);

        return texts;
    }
    private static Text GenerateBaguWoodsHint(Location bagu)
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
        Text baguHint = new Text(hint);
        return baguHint;
    }

    private static void GenerateTownNameHints(List<Text> texts, IEnumerable<Location> locations, bool linkedFire)
    {
        foreach (Location location in locations.Where(i => i.ActualTown != null && i.VanillaCollectable.IsSpell()))
        {
            texts[TOWN_SIGN_INDEXES[(Town)location.ActualTown!]] = GenerateTownSignHint(location.Collectables[0], linkedFire);
        }
    }
    public static Text GenerateTownSignHint(Collectable spell, bool linkedFire)
    {
        string text = spell.EnglishText();
        if(spell == Collectable.FIRE_SPELL && linkedFire)
        {
            text = "FIRE!$SPELL";
        }
        text += "$town";
        return new Text(text);
    }

    public static Text GenerateWizardText(List<Text> texts, Random r, Location location, bool useCommunityText)
    {
        if(location.ActualTown == null)
        {
            throw new Exception("Cannot generate text for Wizard outside of town");
        }
        Town town = (Town)location.ActualTown;
        Collectable collectable = location.Collectables[0];
        if (collectable.IsSpell() 
            || collectable == Collectable.DOWNSTAB 
            || collectable == Collectable.UPSTAB)
        {
            //If it's a spell, use the old behavior
            if(useCommunityText)
            {
                List<string> possibleWizardHints = GENERIC_WIZARD_TEXTS
                    .Union(WIZARD_SPELL_TEXTS_BY_TOWN[town])
                    .Union(WIZARD_SPELL_TEXTS_BY_COLLECTABLE[collectable]).ToList();
                int selectedHintIndex = r.Next(possibleWizardHints.Count());
                return new Text(possibleWizardHints[selectedHintIndex]);
            }
            //Non community-text spells use the vanilla text corresponding to the spell you get.
            return new Text(texts[wizardTextIndexesBySpell[collectable]].RawText);
        }
        return GenerateNonSpellWizardText(collectable, r, useCommunityText);
    }

    private static Text? GenerateBaguText(Collectable baguItem, Random r, bool useCommunityText, bool includeQuestItemsInShuffle)
    {
        if(includeQuestItemsInShuffle)
        {
            return GenerateNonSpellWizardText(baguItem, r, useCommunityText);
        }
        else if(useCommunityText)
        {
            return new Text(BAGU_TEXTS.Sample(r)!);
        }
        return null;
    }
    private static Text? GenerateMirrorTableText(Collectable mirrorItem, Random r, bool useCommunityText, bool includeQuestItemsInShuffle)
    {
        if (includeQuestItemsInShuffle)
        {
            if (useCommunityText)
            {
                return GenerateNonSpellWizardText(mirrorItem, r, true);
            }
            else {
                string rawText;
                rawText = "I FOUND A$%$UNDER THE$TABLE.";
                    return new Text(rawText.Replace("%", mirrorItem.SingleLineText()));
            }
        }
        return null;
    }

    private static Text? GenerateFountainText(Collectable fountainItem, Random r, bool useCommunityText, bool includeQuestItemsInShuffle)
    {
        if (includeQuestItemsInShuffle)
        {
            return GenerateNonSpellWizardText(fountainItem, r, useCommunityText);
        }
        return null;
    }
    private static Text GenerateRiverManText(Random r)
    {
        return new Text(RIVER_MAN_TEXTS[r.Next(RIVER_MAN_TEXTS.Length)]);
    }

    private static void GenerateKnowNothings(List<Text> hints, List<int> placedIndexes, Random r, bool useBaguWoods, bool useCommunityText)
    {
        List<int> stationary =
        [
            .. rauruHints,
            .. rutoHints,
            .. sariaHints,
            .. midoHints,
            .. nabooruHints,
            .. daruniaHints,
            .. newkasutoHints,
            .. kingsTomb,
            .. oldkasutoHint,
            errorTextIndex2,
            talkingBotIndexSleeping,
            talkingAcheIndexSleeping,
        ];

        List<int> moving =
        [
            .. rauruMoving,
            .. rutoMoving,
            .. sariaMoving,
            .. movingMido,
            .. movingNabooru,
            .. daruniaMoving,
            .. newkasutoMoving,
        ];

        Text defaultKnowNothing = new();
        for (int i = 0; i < stationary.Count; i++)
        {
            int textIndex = stationary[i];
            if (!placedIndexes.Contains(textIndex))
            {
                if (textIndex == 12)
                {
                    hints[textIndex] = new Text("I am just$a kid"); // default new line for kid in Rauro (for testing purposes)
                }
                else if (textIndex == 77)
                {
                    hints[textIndex] = new Text("Who were$you$expecting?"); // default line for new purple kid in Darunia (for testing purposes)
                }
                else
                {
                    hints[textIndex] = useCommunityText ? new Text(KNOW_NOTHING_TEXTS.Sample(r)!) : defaultKnowNothing;
                }
            }
        }

        for (int i = 0; i < moving.Count; i++)
        {
            hints[moving[i]] =  useCommunityText ? new Text(KNOW_NOTHING_TEXTS.Sample(r)!) : defaultKnowNothing;
        }
    }

    private static List<int> GenerateHelpfulHints(List<Text> hints, IEnumerable<Location> locations, Random r, 
        RandomizerProperties props)
    {
        List<int> placedIndex = [];

        List<Collectable> placedItems = [];

        List<int> placedTowns = [];

        List<Collectable> items = locations.SelectMany(i => i.Collectables).ToList();
        items = items.Where(i => !i.IsInternalUse()).ToList();

        if (props.SpellItemHints && props.IncludeSwordTechsInShuffle)
        {
            items.Remove(Collectable.DOWNSTAB);
            items.Remove(Collectable.UPSTAB);
        }

        if (props.StartWithSpellItems || props.SpellItemHints)
        {
            items.Remove(Collectable.TROPHY);
            items.Remove(Collectable.CHILD);
            items.Remove(Collectable.MEDICINE);
        }

        if(props.SpellItemHints && props.IncludeSpellsInShuffle)
        {
            items.Remove(Collectable.MIRROR);
            items.Remove(Collectable.WATER);
        }

        int hintsCount = HELPFUL_HINTS_COUNT;
        if(props.IncludeSwordTechsInShuffle && props.IncludeQuestItemsInShuffle)
        {
            hintsCount++;
        }
        List<Collectable> hintCollectables = items.Where(i => !smallItems.Contains(i)).ToList();
        hintCollectables.FisherYatesShuffle(r);

        if (props.IncludeSpellsInShuffle)
        {
            Collectable[] criticalSpells = [Collectable.THUNDER_SPELL, Collectable.FAIRY_SPELL, Collectable.REFLECT_SPELL];
            criticalSpells = criticalSpells.Where(i => hintCollectables.Contains(i)).ToArray();
            if(criticalSpells.Length > 0)
            {
                var collectableSpell = criticalSpells.Sample(r);
                hintCollectables.Remove(collectableSpell);
                hintCollectables.Insert(0, collectableSpell); // move important spell to the front
                hintsCount++;
            }
        }

        hintsCount = Math.Min(hintsCount, hintCollectables.Count);
        if (hintsCount > 0)
        {
            hintCollectables = hintCollectables.Take(hintsCount - 1).ToList();
        }
        hintCollectables.Add(items.Where(smallItems.Contains).ToList().Sample(r));


        foreach(Collectable hintCollectable in hintCollectables)
        {
            List<Location> possibleHintLocations = locations.Where(i => i.Collectables.Contains(hintCollectable)).ToList();
            Location hintLocation = possibleHintLocations.Sample(r) ?? throw new ImpossibleException("Error generating hint for unplaced item");
            Text hint = Text.GenerateHelpfulHint(locations.ToList(), hintLocation, hintCollectable, props.IncludeSpellsInShuffle);
            int town = r.Next(9);
            while (placedTowns.Contains(town))
            {
                town = r.Next(9);
            }
            int index = hintIndexes[town][r.Next(hintIndexes[town].Length)];
            switch (index)
            {
                case errorTextIndex1:
                    hints[errorTextIndex1] = hint;
                    hints[errorTextIndex2] = hint;
                    placedIndex.Add(errorTextIndex1);
                    placedIndex.Add(errorTextIndex2);
                    break;
                case talkingBotIndexTalking:
                    hints[talkingBotIndexSleeping] = hint;
                    hints[talkingBotIndexTalking] = hint;
                    placedIndex.Add(talkingBotIndexSleeping);
                    placedIndex.Add(talkingBotIndexTalking);
                    break;
                case talkingAcheIndexTalking:
                    hints[talkingAcheIndexSleeping] = hint;
                    hints[talkingAcheIndexTalking] = hint;
                    placedIndex.Add(talkingAcheIndexSleeping);
                    placedIndex.Add(talkingAcheIndexTalking);
                    break;
                default:
                    hints[index] = hint;
                    placedIndex.Add(index);
                    break;
            }

            placedTowns.Add(town);
            placedItems.Add(hintCollectable);
        }
        return placedIndex;
    }

    private static void GenerateSpellHints(IEnumerable<Location> locations, List<Text> hints, RandomizerProperties props)
    {
        if(props.SpellItemHints && !props.StartWithSpellItems)
        {
            Location itemLocation;

            itemLocation = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.TROPHY))!;
            if(itemLocation != null)
            {
                Text trophyHint = Text.GenerateHelpfulHint(locations.ToList(), itemLocation, Collectable.TROPHY, props.IncludeSpellsInShuffle);
                hints[trophySpellHintIndex] = trophyHint;
            }

            itemLocation = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.MEDICINE))!;
            if (itemLocation != null)
            {
                Text medHint = Text.GenerateHelpfulHint(locations.ToList(), itemLocation, Collectable.MEDICINE, props.IncludeSpellsInShuffle);
                hints[medicineSpellHintIndex] = medHint;
            }   

            itemLocation = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.CHILD))!;
            if (itemLocation != null)
            {
                Text kidHint = Text.GenerateHelpfulHint(locations.ToList(), itemLocation, Collectable.CHILD, props.IncludeSpellsInShuffle);
                hints[childSpellHintIndex] = kidHint;
            }

            if(props.IncludeQuestItemsInShuffle)
            {
                itemLocation = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.MIRROR))!;
                Text mirrorHint = Text.GenerateHelpfulHint(locations.ToList(), itemLocation, Collectable.MIRROR, props.IncludeSpellsInShuffle);
                hints[mirrorSpellHintIndex] = mirrorHint;

                itemLocation = locations.FirstOrDefault(i => i.Collectables.Contains(Collectable.WATER))!;
                Text waterHint = Text.GenerateHelpfulHint(locations.ToList(), itemLocation, Collectable.WATER, props.IncludeSpellsInShuffle);
                hints[waterSpellHintIndex] = waterHint;
            }
        }
    }

    private static void GenerateWizardTexts(List<Text> texts, IEnumerable<Location> itemLocs, Random r, bool useCommunityText)
    {
        List<Text> vanillaText = new(texts);
        List<Text> usedWizardTexts = [];
        List<Location> wizardLocations = itemLocs.Where(i => i.ActualTown != 0 && (i?.ActualTown?.IsWizardTown() ?? false)).ToList();
        foreach (Location location in wizardLocations)
        {
            if(location.ActualTown == null)
            {
                throw new Exception("Invalid town that is not a town");
            }
            Text wizardHint;
            int tries = 0;
            do
            {
                wizardHint = GenerateWizardText(vanillaText, r, location, useCommunityText);
            } while (useCommunityText && usedWizardTexts.Contains(wizardHint) && tries++ < 100);
            usedWizardTexts.Add(wizardHint);
            texts[townWizardTextIndexes[(Town)location.ActualTown]] = wizardHint;
        }
    }

    private static Text GenerateNonSpellWizardText(Collectable collectable, Random r, bool useCommunityText)
    {
        string collectableWizardText;

        if(useCommunityText)
        {
            collectableWizardText = COMMUNITY_NONSPELL_GET_TEXT[r.Next(COMMUNITY_NONSPELL_GET_TEXT.Length)];
        }
        else
        {
            collectableWizardText = DEFAULT_WIZARD_COLLECTABLE;
        }

        return new Text(collectableWizardText, collectable);
    }

    private static int TextLength(List<Text> texts)
    {
        // assume js65 will optimize texts if they are the same, so ignore duplicates here
        HashSet<Text> uniqueTexts = new();

        int sum = 0;
        for (int i = 0; i < texts.Count(); i++)
        {
            var t = texts[i];
            if (uniqueTexts.Add(t))
            {
                sum += t.EncodedText.Length;
            }
        }
        return sum;
    }
}
