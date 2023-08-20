using System.Text.RegularExpressions;

namespace Tests;

[TestClass]
public class HintsTests
{
    //10 is the max that formats well. 11 "works" but runs into the padding. 12+ Overflows off the text box.
    const int MAX_HINT_LENGTH = 11;
    [TestMethod]
    public void EnsureAllHintsAreValid()
    {
        List<string> hints = Hints.GENERIC_WIZARD_TEXTS
            .Union(Hints.RIVER_MAN_TEXTS).ToList()
            .Union(Hints.BAGU_TEXTS).ToList();

        Hints.WIZARD_TEXTS_BY_SPELL.Values.SelectMany(i => i).ToList().ForEach(hints.Add);
        Hints.WIZARD_TEXTS_BY_TOWN.Values.SelectMany(i => i).ToList().ForEach(hints.Add);

        foreach(string hint in hints)
        {
            Assert.IsTrue(Regex.IsMatch(hint.ToUpper(), @"^[-A-Z0-9 /,.!?*\n$]+$"), "Hint contains an invalid character: " + hint);
            string[] parts = hint.Split('$');
            Assert.IsFalse(parts.Any(i => i.Length > MAX_HINT_LENGTH), "Hint has a part that is too long: " + hint);
        }
    }
}
