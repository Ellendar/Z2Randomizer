using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Overworld;

namespace Z2Randomizer;

public class Hint
{
    private static List<int> used = new List<int>();

    private readonly String[] wizardTexts =
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

    private readonly String[] bridgetext = {
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

    private readonly String[] bagutext =
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

    private readonly String[] downstabtext =
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

    private readonly String[] upstabtext =
    {
        "bet you$wish this$was$downstab",
        "you$probably$wont need$this",
        "press up$you idiot",
        "press up$to go in$doors",
        "are you$santa$claus?",
        "SHORYUKEN!",
        "you wasted$your time"
    };

    private List<char> text;

    public List<char> Text { get => text; }

    public Hint()
    {
        text = Util.ToGameText("I know$nothing", true);
    }

    public Hint(List<char> text)
    {
        this.text = text;
    }

    public void GenerateCommunityHint(HintType type, Random r)
    {
        switch(type)
        {
            case HintType.WIZARD:
                int thisone = r.Next(wizardTexts.Count());
                while (used.Contains(thisone))
                {
                    thisone = r.Next(wizardTexts.Count());
                }
                this.text = Util.ToGameText(wizardTexts[thisone], true).ToList();
                used.Add(thisone);
                break;
            case HintType.BAGU:
                this.text = Util.ToGameText(bagutext[r.Next(bagutext.Count())], true);
                break;
            case HintType.BRIDGE:
                this.text = Util.ToGameText(bridgetext[r.Next(bridgetext.Length)], true);
                break;
            case HintType.DOWNSTAB:
                this.text = Util.ToGameText(downstabtext[r.Next(downstabtext.Length)], true);
                break;
            case HintType.UPSTAB:
                this.text = Util.ToGameText(upstabtext[r.Next(upstabtext.Length)], true);
                break;
            default:
                Debug.WriteLine("Invalid hint type!");
                break;
        }
    }

    public static void Reset()
    {
        used = new List<int>();
    }

    public void GenerateHelpfulHint(Location location)
    {
        Item hintItem = location.item;
        String hint = "";
        if (location.PalNum == 1)
        {
            hint += "horsehead$neighs$with the$";
        }
        else if (location.PalNum == 2)
        {
            hint += "helmethead$guards the$";
        }
        else if (location.PalNum == 3)
        {
            hint += "rebonack$rides$with the$";
        }
        else if (location.PalNum == 4)
        {
            hint += "carock$disappears$with the$";
        }
        else if (location.PalNum == 5)
        {
            hint += "gooma sits$on the$";
        }
        else if (location.PalNum == 6)
        {
            hint += "barba$slithers$with the$";
        }
        else if (location.Continent == Continent.EAST)
        {
            hint += "go east to$find the$";
        }
        else if (location.Continent == Continent.WEST)
        {
            hint += "go west to$find the$";
        }
        else if (location.Continent == Continent.DM)
        {
            hint += "death$mountain$holds the$";
        }
        else
        {
            hint += "in a maze$lies the$";
        }

        switch (hintItem)
        {
            case (Item.BLUE_JAR):
                hint += "blue jar";
                break;
            case (Item.BOOTS):
                hint += "boots";
                break;
            case (Item.CANDLE):
                hint += "candle";
                break;
            case (Item.CROSS):
                hint += "cross";
                break;
            case (Item.XL_BAG):
            case (Item.LARGE_BAG):
            case (Item.MEDIUM_BAG):
            case (Item.SMALL_BAG):
                hint += "pbag";
                break;
            case (Item.GLOVE):
                hint += "glove";
                break;
            case (Item.HAMMER):
                hint += "hammer";
                break;
            case (Item.HEART_CONTAINER):
                hint += "heart";
                break;
            case (Item.FLUTE):
                hint += "flute";
                break;
            case (Item.KEY):
                hint += "small key";
                break;
            case (Item.CHILD):
                hint += "child";
                break;
            case (Item.MAGIC_CONTAINER):
                hint += "magic jar";
                break;
            case (Item.MAGIC_KEY):
                hint += "magic key";
                break;
            case (Item.MEDICINE):
                hint += "medicine";
                break;
            case (Item.ONEUP):
                hint += "link doll";
                break;
            case (Item.RAFT):
                hint += "raft";
                break;
            case (Item.RED_JAR):
                hint += "red jar";
                break;
            case (Item.TROPHY):
                hint += "trophy";
                break;
        }

        text = Util.ToGameText(hint, true).ToList();
    }

    public void GenerateTownHint(Spell spell, bool useDash)
    {
        String text = "";
        switch(spell)
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
                if (useDash)
                { 
                    text += "dash$";
                }
                else
                {
                    text += "fire$";
                }
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
        this.text = Util.ToGameText(text, true);
    }

}
