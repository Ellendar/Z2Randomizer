using Newtonsoft.Json;
using Z2Randomizer.CommandLine.Models;
using Z2Randomizer.RandomizerCore;

namespace Tests
{
    [TestClass]
    public class PlayerOptionsSerializationTests
    {
        [TestClass]
        public class BeepThresholdTests
        {
            [TestMethod]
            public void QuarterBeepThresholdIsSupported()
            {
                var input = "{BeepThreshold: 'QuarterBar'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepThreshold.QuarterBar, output?.BeepThreshold);
            }

            [TestMethod]
            public void HalfBeepThresholdIsSupported()
            {
                var input = "{BeepThreshold: 'HalfBar'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepThreshold.HalfBar, output?.BeepThreshold);
            }

            [TestMethod]
            public void NormalThresholdIsSupported()
            {
                var input = "{BeepThreshold: 'Normal'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepThreshold.Normal, output?.BeepThreshold);
            }

            [TestMethod]
            public void TwoBarsThresholdIsSupported()
            {
                var input = "{BeepThreshold: 'TwoBars'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepThreshold.TwoBars, output?.BeepThreshold);
            }
        }

        [TestClass]
        public class BeepFrequencyTests
        {
            [TestMethod]
            public void NormalFrequencyIsSupported()
            {
                var input = "{\"BeepFrequency\": \"Normal\"}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepFrequency.Normal, output?.BeepFrequency);
            }

            [TestMethod]
            public void HalfFrequencyIsSupported()
            {
                var input = "{BeepFrequency: 'HalfSpeed'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepFrequency.HalfSpeed, output?.BeepFrequency);
            }

            [TestMethod]
            public void QuarterFrequencyIsSupported()
            {
                var input = "{BeepFrequency: 'QuarterSpeed'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepFrequency.QuarterSpeed, output?.BeepFrequency);
            }

            [TestMethod]
            public void BeepsCanBeTurnedOff()
            {
                var input = "{BeepFrequency: 'Off'}";
                var output = JsonConvert.DeserializeObject<PlayerOptions>(input);

                Assert.AreEqual(BeepFrequency.Off, output?.BeepFrequency);

                output!.Sprite = "Link";
                Console.WriteLine(JsonConvert.SerializeObject(output));
            }
        }
    }
}
