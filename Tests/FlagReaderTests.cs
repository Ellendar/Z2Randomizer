using RandomizerCore;
using RandomizerCore.Flags;

namespace Z2Randomizer.Tests;

[TestClass]
public class FlagReaderTests
{
    const string FALSE_FLAG = "A"; //000000
    const string TRUE_FLAG = "w"; //101110
    const string ENUM_FLAG = "q"; //101000
    [TestMethod]
    public void TestReadBool()
    {
        FlagReader flagReader = new FlagReader(FALSE_FLAG);
        Assert.IsFalse(flagReader.ReadBool());
        Assert.IsFalse(flagReader.ReadNullableBool());

        flagReader = new FlagReader(TRUE_FLAG);
        Assert.IsTrue(flagReader.ReadBool());
        Assert.IsTrue(flagReader.ReadNullableBool());
        flagReader.ReadBool();
        Assert.IsNull(flagReader.ReadNullableBool());
    }

    [TestMethod]
    public void TestReadByte()
    {
        /*
        FlagReader flagReader = new FlagReader(FALSE_FLAG);
        Assert.IsTrue(flagReader.ReadByte(2) == 0);
        Assert.IsTrue(flagReader.ReadNullableByte(3) == 0);
        */

        FlagReader flagReader = new FlagReader(TRUE_FLAG);
        Assert.IsTrue(flagReader.ReadByte(2) == 1);
        Assert.IsTrue(flagReader.ReadNullableByte(2) == 1);
        Assert.IsNull(flagReader.ReadNullableByte(3));

        flagReader = new FlagReader(TRUE_FLAG);
        Assert.IsTrue(flagReader.ReadByte(64) == 46);
    }

    [TestMethod]
    public void TestReadEnum()
    {
        FlagReader flagReader = new FlagReader(ENUM_FLAG);
        Assert.AreEqual(flagReader.ReadEnum<EncounterRate>(), EncounterRate.NORMAL);
        Assert.IsNull(flagReader.ReadNullableEnum<EncounterRate>());
    }


}