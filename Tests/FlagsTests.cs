using System.Reflection;

namespace Tests;

[TestClass]
public class FlagsTests
{
    [TestMethod]
    public void TestNullableBoolEncodeCycle()
    {
        bool? nullBool = null;
        FlagBuilder flagBuilder= new FlagBuilder();
        flagBuilder.Append(nullBool);
        String flags = flagBuilder.ToString();
        FlagReader flagReader = new FlagReader(flags);
        Assert.AreEqual(nullBool, flagReader.ReadNullableBool());

        flagBuilder.Append((bool?)false);
        flagBuilder.Append((bool?)true);
        flags = flagBuilder.ToString();
        flagReader = new FlagReader(flags);
        Assert.AreEqual(nullBool, flagReader.ReadNullableBool());
        Assert.AreEqual((bool?)false, flagReader.ReadNullableBool());
        Assert.AreEqual((bool?)true, flagReader.ReadNullableBool());
    }
    [TestMethod]
    public void TestBlankEncodeCycle()
    {
        RandomizerConfiguration config = new();
        String flags = config.Serialize();
        RandomizerConfiguration config2 = new(flags);
        foreach(PropertyInfo property in typeof(RandomizerConfiguration).GetProperties())
        {
            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            Assert.AreEqual(property.GetValue(config), property.GetValue(config2), 
                property.Name + " did not match. Config: " + property.GetValue(config) + " Config2: " + property.GetValue(config2));
        }
        String flags2 = config2.Serialize();
        Assert.AreEqual(flags, flags2);
    }

    [TestMethod]
    public void TestStandardFlagsEncodeCycle()
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("hAhhD0j9$78$Jp5$$gAhOAdEScuA");
        String flags = config.Serialize();
        RandomizerConfiguration config2 = new(flags);
        String flags2 = config2.Serialize();
        Assert.AreEqual(flags, flags2);
    }

    [TestMethod]
    public void TestMaxRandoEncodeCycle()
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("iyAqh$j#g7z$ZqTBT!BhOA!0P@@A");
        String flags = config.Serialize();
        RandomizerConfiguration config2 = new(flags);
        String flags2 = config2.Serialize();
        Assert.AreEqual(flags, flags2);
    }
}