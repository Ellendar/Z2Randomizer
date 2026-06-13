using FluentAssertions;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Tests;

[TestClass]
public class RandomTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_Deterministic_ForSameSeed()
    {
        var r1 = new Random(42L);
        var r2 = new Random(42L);

        for (int i = 0; i < 100; i++)
        {
            r1.Next().Should().Be(r2.Next());
        }
    }

    [TestMethod]
    public void Constructor_DifferentSequences_ForDifferentSeeds()
    {
        var r1 = new Random(42L);
        var r2 = new Random(43L);

        bool differs = false;
        for (int i = 0; i < 100; i++)
        {
            if (r1.Next() != r2.Next())
            {
                differs = true;
                break;
            }
        }
        differs.Should().BeTrue();
    }

    #endregion

    #region GetState / SetState

    [TestMethod]
    public void GetState_ReturnsBase64String()
    {
        var r = new Random(42L);
        string state = r.GetState();

        state.Should().NotBeNullOrEmpty();
        state.Length.Should().Be(44); // Base64 of 32 bytes = 44 chars (with padding)

        byte[] decoded = Convert.FromBase64String(state);
        decoded.Length.Should().Be(32);
    }

    [TestMethod]
    public void SetState_RestoresExactSequence()
    {
        var r1 = new Random(42L);
        int[] before = Enumerable.Range(0, 10).Select(_ => r1.Next()).ToArray();

        string state = r1.GetState();
        int[] afterState = Enumerable.Range(0, 10).Select(_ => r1.Next()).ToArray();

        var r2 = new Random(999L);
        r2.SetState(state);
        int[] restored = Enumerable.Range(0, 10).Select(_ => r2.Next()).ToArray();

        restored.Should().Equal(afterState);
        restored.Should().NotEqual(before);
    }

    [TestMethod]
    public void SetState_InvalidLength_Throws()
    {
        var r = new Random(42L);
        string shortState = Convert.ToBase64String(new byte[16]);

        Action act = () => r.SetState(shortState);
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void SetState_AllZero_Throws()
    {
        var r = new Random(42L);
        string zeroState = Convert.ToBase64String(new byte[32]);

        Action act = () => r.SetState(zeroState);
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void GetState_SetState_PreservesAllMethods()
    {
        var r1 = new Random(123L);
        _ = r1.Next();
        _ = r1.NextInt64();
        _ = r1.NextDouble();

        string state = r1.GetState();

        var r2 = new Random(999L);
        r2.SetState(state);

        for (int i = 0; i < 50; i++)
        {
            r1.Next().Should().Be(r2.Next());
            r1.NextInt64().Should().Be(r2.NextInt64());
            r1.NextDouble().Should().Be(r2.NextDouble());
        }
    }

    #endregion

    #region Next()

    [TestMethod]
    public void Next_ReturnsNonNegative()
    {
        var r = new Random(42L);
        for (int i = 0; i < 1000; i++)
        {
            int val = r.Next();
            val.Should().BeGreaterThanOrEqualTo(0);
            val.Should().BeLessThan(int.MaxValue);
        }
    }

    [TestMethod]
    public void Next_Distribution_IsNotConstant()
    {
        var r = new Random(42L);
        var values = Enumerable.Range(0, 10000).Select(_ => r.Next()).ToList();

        values.Distinct().Count().Should().BeGreaterThan(100);
    }

    #endregion

    #region Next(maxValue)

    [TestMethod]
    public void NextMaxValue_One_ReturnsZero()
    {
        var r = new Random(42L);
        for (int i = 0; i < 100; i++)
        {
            r.Next(1).Should().Be(0);
        }
    }

    [TestMethod]
    public void NextMaxValue_Two_ReturnsZeroOrOne()
    {
        var r = new Random(42L);
        for (int i = 0; i < 1000; i++)
        {
            int val = r.Next(2);
            val.Should().BeInRange(0, 1);
        }
    }

    [TestMethod]
    public void NextMaxValue_InRange()
    {
        var r = new Random(42L);
        int max = 100;
        for (int i = 0; i < 10000; i++)
        {
            int val = r.Next(max);
            val.Should().BeGreaterThanOrEqualTo(0);
            val.Should().BeLessThan(max);
        }
    }

    [TestMethod]
    public void NextMaxValue_Distribution_ContainsAllValues()
    {
        var r = new Random(42L);
        int max = 10;
        var values = Enumerable.Range(0, 10000).Select(_ => r.Next(max)).ToList();

        for (int i = 0; i < max; i++)
        {
            values.Should().Contain(i);
        }
    }

    #endregion

    #region Next(minValue, maxValue)

    [TestMethod]
    public void NextRange_InRange()
    {
        var r = new Random(42L);
        int min = -50, max = 50;
        for (int i = 0; i < 10000; i++)
        {
            int val = r.Next(min, max);
            val.Should().BeGreaterThanOrEqualTo(min);
            val.Should().BeLessThan(max);
        }
    }

    [TestMethod]
    public void NextRange_NegativeRange()
    {
        var r = new Random(42L);
        for (int i = 0; i < 1000; i++)
        {
            int val = r.Next(-100, -10);
            val.Should().BeGreaterThanOrEqualTo(-100);
            val.Should().BeLessThan(-10);
        }
    }

    [TestMethod]
    public void NextRange_SameMinMax_ReturnsMin()
    {
        var r = new Random(42L);
        for (int i = 0; i < 100; i++)
        {
            r.Next(5, 6).Should().Be(5);
        }
    }

    [TestMethod]
    public void NextRange_Distribution_ContainsAllValues()
    {
        var r = new Random(42L);
        var values = Enumerable.Range(0, 10000).Select(_ => r.Next(0, 10)).ToList();

        for (int i = 0; i < 10; i++)
        {
            values.Should().Contain(i);
        }
    }

    #endregion

    #region NextInt64()

    [TestMethod]
    public void NextInt64_ReturnsNonNegative()
    {
        var r = new Random(42L);
        for (int i = 0; i < 1000; i++)
        {
            long val = r.NextInt64();
            val.Should().BeGreaterThanOrEqualTo(0);
            val.Should().BeLessThan(long.MaxValue);
        }
    }

    [TestMethod]
    public void NextInt64_Distribution_IsNotConstant()
    {
        var r = new Random(42L);
        var values = Enumerable.Range(0, 10000).Select(_ => r.NextInt64()).ToList();

        values.Distinct().Count().Should().BeGreaterThan(100);
    }

    #endregion

    #region NextInt64(maxValue)

    [TestMethod]
    public void NextInt64MaxValue_One_ReturnsZero()
    {
        var r = new Random(42L);
        for (int i = 0; i < 100; i++)
        {
            r.NextInt64(1L).Should().Be(0L);
        }
    }

    [TestMethod]
    public void NextInt64MaxValue_InRange()
    {
        var r = new Random(42L);
        long max = 1000;
        for (int i = 0; i < 10000; i++)
        {
            long val = r.NextInt64(max);
            val.Should().BeGreaterThanOrEqualTo(0);
            val.Should().BeLessThan(max);
        }
    }

    #endregion

    #region NextInt64(minValue, maxValue)

    [TestMethod]
    public void NextInt64Range_InRange()
    {
        var r = new Random(42L);
        long min = -500, max = 500;
        for (int i = 0; i < 10000; i++)
        {
            long val = r.NextInt64(min, max);
            val.Should().BeGreaterThanOrEqualTo(min);
            val.Should().BeLessThan(max);
        }
    }

    [TestMethod]
    public void NextInt64Range_Distribution_ContainsAllValues()
    {
        var r = new Random(42L);
        var values = Enumerable.Range(0, 10000).Select(_ => r.NextInt64(0L, 10L)).ToList();

        for (long i = 0; i < 10; i++)
        {
            values.Should().Contain(i);
        }
    }

    #endregion

    #region NextBytes

    [TestMethod]
    public void NextBytes_FillsBuffer()
    {
        var r = new Random(42L);
        byte[] buffer = new byte[64];
        r.NextBytes(buffer);

        buffer.Any(b => b != 0).Should().BeTrue();
    }

    [TestMethod]
    public void NextBytes_CorrectLength()
    {
        var r = new Random(42L);
        byte[] buffer = new byte[37];
        r.NextBytes(buffer);

        buffer.Length.Should().Be(37);
    }

    [TestMethod]
    public void NextBytes_EmptyBuffer_DoesNotThrow()
    {
        var r = new Random(42L);
        byte[] buffer = Array.Empty<byte>();
        Action act = () => r.NextBytes(buffer);
        act.Should().NotThrow();
    }

    [TestMethod]
    public void NextBytes_Deterministic()
    {
        var r1 = new Random(42L);
        var r2 = new Random(42L);

        byte[] b1 = new byte[64];
        byte[] b2 = new byte[64];
        r1.NextBytes(b1);
        r2.NextBytes(b2);

        b1.Should().Equal(b2);
    }

    [TestMethod]
    public void NextBytes_SingleByte()
    {
        var r = new Random(42L);
        byte[] buffer = new byte[1];
        r.NextBytes(buffer);
        buffer.Length.Should().Be(1);
    }

    #endregion

    #region NextDouble

    [TestMethod]
    public void NextDouble_InRange()
    {
        var r = new Random(42L);
        for (int i = 0; i < 10000; i++)
        {
            double val = r.NextDouble();
            val.Should().BeGreaterThanOrEqualTo(0.0);
            val.Should().BeLessThan(1.0);
        }
    }

    [TestMethod]
    public void NextDouble_Distribution_IsNotConstant()
    {
        var r = new Random(42L);
        var values = Enumerable.Range(0, 10000).Select(_ => r.NextDouble()).ToList();

        values.Distinct().Count().Should().BeGreaterThan(100);
    }

    [TestMethod]
    public void NextDouble_Deterministic()
    {
        var r1 = new Random(42L);
        var r2 = new Random(42L);

        for (int i = 0; i < 100; i++)
        {
            r1.NextDouble().Should().Be(r2.NextDouble());
        }
    }

    #endregion

    #region GetItems

    [TestMethod]
    public void GetItemsArray_ChoicesFromSource()
    {
        var r = new Random(42L);
        int[] choices = { 10, 20, 30 };

        int[] result = r.GetItems(choices, 100);
        result.Length.Should().Be(100);
        foreach (int val in result)
        {
            val.Should().BeOneOf(10, 20, 30);
        }
    }

    [TestMethod]
    public void GetItemsArray_LengthZero_ReturnsEmpty()
    {
        var r = new Random(42L);
        int[] choices = { 1, 2, 3 };

        int[] result = r.GetItems(choices, 0);
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetItemsSpan_FillsDestination()
    {
        var r = new Random(42L);
        string[] choices = { "a", "b", "c" };
        string[] dest = new string[5];

        r.GetItems(choices, dest);
        dest.Length.Should().Be(5);
        foreach (string val in dest)
        {
            val.Should().BeOneOf("a", "b", "c");
        }
    }

    [TestMethod]
    public void GetItems_Distribution_ContainsAllChoices()
    {
        var r = new Random(42L);
        int[] choices = { 1, 2, 3, 4, 5 };

        int[] result = r.GetItems(choices, 1000);
        foreach (int choice in choices)
        {
            result.Should().Contain(choice);
        }
    }

    #endregion

    #region Shuffle

    [TestMethod]
    public void Shuffle_PreservesElements()
    {
        var r = new Random(42L);
        int[] original = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int[] shuffled = (int[])original.Clone();
        r.Shuffle(shuffled);

        shuffled.Should().BeEquivalentTo(original); // ignores order
    }

    [TestMethod]
    public void Shuffle_Deterministic()
    {
        var r1 = new Random(42L);
        var r2 = new Random(42L);

        int[] a = { 1, 2, 3, 4, 5 };
        int[] b = (int[])a.Clone();
        r1.Shuffle(a);
        r2.Shuffle(b);

        a.Should().Equal(b);
    }

    [TestMethod]
    public void Shuffle_Empty()
    {
        var r = new Random(42L);
        int[] arr = Array.Empty<int>();
        Action act = () => r.Shuffle(arr);
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Shuffle_SingleElement()
    {
        var r = new Random(42L);
        int[] arr = { 42 };
        r.Shuffle(arr);
        arr.Should().Equal([42]);
    }

    [TestMethod]
    public void Shuffle_ChangesOrder()
    {
        var r = new Random(42L);
        int[] arr = Enumerable.Range(0, 100).ToArray();
        r.Shuffle(arr);

        bool changed = false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != i)
            {
                changed = true;
                break;
            }
        }
        changed.Should().BeTrue();
    }

    #endregion
}
