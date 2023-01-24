using Z2Randomizer.Flags;
namespace Tests;

[TestClass]
public class FlagBuilderTests
{
    [TestMethod]
    public void TestAppendNullableInt()
    {
        FlagBuilder flagBuilder = new FlagBuilder();
        int? nullInt = null;
        flagBuilder.Append(nullInt, 8);
        Assert.AreEqual("h", flagBuilder.ToString());
        flagBuilder.Append(nullInt, 8);
        Assert.AreEqual("jA", flagBuilder.ToString());
    }

    [TestMethod]
    public void TestAppendBool()
    {
        FlagBuilder flagBuilder = new FlagBuilder();
        flagBuilder.Append(false);
        Assert.AreEqual("A", flagBuilder.ToString()); //0
        flagBuilder.Append(true);
        Assert.AreEqual("R", flagBuilder.ToString()); //01

        bool? nullableBool = false;
        flagBuilder.Append(nullableBool);
        Assert.AreEqual("R", flagBuilder.ToString()); //0100
        nullableBool = true;
        flagBuilder.Append(nullableBool);
        Assert.AreEqual("S", flagBuilder.ToString()); //010001
        nullableBool = null;
        flagBuilder.Append(nullableBool);
        Assert.AreEqual("Sh", flagBuilder.ToString()); //010001 10
    }
}