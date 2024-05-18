using System.Text.RegularExpressions;
using RandomizerCore;

namespace Tests;

[TestClass]
public class HintsTests
{
    //10 is the max that formats well. 11 "works" but runs into the padding. 12+ Overflows off the text box.
    //TODO: This test is probably broken with the new non-spell wizards. Fix it eventually
    const int MAX_HINT_LENGTH = 11;
    [TestMethod]
    public void EnsureAllHintsAreValid()
    {
        List<string> hints = CustomTexts.GENERIC_WIZARD_TEXTS
            .Union(CustomTexts.RIVER_MAN_TEXTS).ToList()
            .Union(CustomTexts.BAGU_TEXTS).ToList();

        CustomTexts.WIZARD_SPELL_TEXTS_BY_COLLECTABLE.Values.SelectMany(i => i).ToList().ForEach(hints.Add);
        CustomTexts.WIZARD_SPELL_TEXTS_BY_TOWN.Values.SelectMany(i => i).ToList().ForEach(hints.Add);

        foreach(string hint in hints)
        {
            Assert.IsTrue(Regex.IsMatch(hint.ToUpper(), @"^[-A-Z0-9 /,.!?*\n$]+$"), "Hint contains an invalid character: " + hint);
            string[] parts = hint.Split('$');
            Assert.IsFalse(parts.Any(i => i.Length > MAX_HINT_LENGTH), "Hint has a part that is too long: " + hint);
        }
    }
}
