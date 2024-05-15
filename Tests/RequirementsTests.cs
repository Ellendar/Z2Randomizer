using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using Z2Randomizer.Core.Sidescroll;

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

    public void TestSingleRequirement()
    {
        Requirements requirements = new Requirements(new RequirementType[] { RequirementType.JUMP });
        RequirementType[] requireables = new RequirementType[] { };

        Assert.IsFalse(requirements.AreSatisfiedBy(new RequirementType[] { }));
    }
}
