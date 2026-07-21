using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class RequirementsTests
{
    [TestMethod]
    public void TestJsonConstructor()
    {
        string? json = @"[""JUMP"",""FAIRY"",""KEY"",[""DASH"",""JUMP""]]";
        Requirements requirements = new Requirements(json);
        string? serialized = requirements.Serialize();
        Assert.AreEqual(json, serialized);
    }

    public void TestEmptyJsonConstructor()
    {
        string? json = @"[]";
        Requirements requirements = new Requirements(json);
        string? serialized = requirements.Serialize();
        Assert.AreEqual(json, serialized);
    }

    [TestMethod]
    public void TestSingleRequirement()
    {
        Requirements requirements = new Requirements([RequirementType.JUMP]);
        RequirementType[] requireables = [];

        Assert.IsFalse(requirements.AreSatisfiedBy(new HashSet<RequirementType>()));
    }
}
