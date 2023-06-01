using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

public class Hint
{

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
