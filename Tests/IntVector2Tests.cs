using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Tests;

[TestClass]
public class IntVector2Tests
{
    [TestMethod]
    public void Constructor_SetsCoordinates()
    {
        var v = new IntVector2(3, -4);
        v.X.Should().Be(3);
        v.Y.Should().Be(-4);
    }

    [TestMethod]
    public void EqualityOperator_True_WhenSame()
    {
        (new IntVector2(1, 2) == new IntVector2(1, 2)).Should().BeTrue();
        (new IntVector2(1, 2) == new IntVector2(3, 4)).Should().BeFalse();
    }

    [TestMethod]
    public void InequalityOperator_True_WhenDifferent()
    {
        (new IntVector2(1, 2) != new IntVector2(3, 4)).Should().BeTrue();
        (new IntVector2(1, 2) != new IntVector2(1, 2)).Should().BeFalse();
    }

    [TestMethod]
    public void AddOperator_SumsComponents()
    {
        var a = new IntVector2(1, 2);
        var b = new IntVector2(3, 4);
        (a + b).Should().Be(new IntVector2(4, 6));
    }

    [TestMethod]
    public void SubtractOperator_DiffsComponents()
    {
        var a = new IntVector2(5, 7);
        var b = new IntVector2(2, 3);
        (a - b).Should().Be(new IntVector2(3, 4));
    }

    [TestMethod]
    public void NegationOperator_NegatesComponents()
    {
        var v = new IntVector2(3, -4);
        (-v).Should().Be(new IntVector2(-3, 4));
    }

    [TestMethod]
    public void MultiplyByScalar_ScalesComponents()
    {
        var v = new IntVector2(2, -3);
        (3 * v).Should().Be(new IntVector2(6, -9));
    }

    [TestMethod]
    public void DivideByScalar_DividesComponents()
    {
        var v = new IntVector2(6, -9);
        (v / 3).Should().Be(new IntVector2(2, -3));
    }

    [TestMethod]
    public void Dot_Product()
    {
        IntVector2.Dot(new IntVector2(1, 2), new IntVector2(3, 4)).Should().Be(11);
    }

    [TestMethod]
    public void Cross_Product()
    {
        IntVector2.Cross(new IntVector2(1, 0), new IntVector2(0, 1)).Should().Be(1);
        IntVector2.Cross(new IntVector2(0, 1), new IntVector2(1, 0)).Should().Be(-1);
        IntVector2.Cross(new IntVector2(1, 2), new IntVector2(3, 4)).Should().Be(-2);
    }

    [TestMethod]
    public void ComponentMultiply_MultipliesElementWise()
    {
        new IntVector2(2, 3).ComponentMultiply(new IntVector2(4, 5)).Should().Be(new IntVector2(8, 15));
    }

    [TestMethod]
    public void IsZero_ReturnsCorrectValue()
    {
        IntVector2.ZERO.IsZero.Should().BeTrue();
        new IntVector2(1, 0).IsZero.Should().BeFalse();
        new IntVector2(0, 1).IsZero.Should().BeFalse();
        new IntVector2(1, 2).IsZero.Should().BeFalse();
    }

    [TestMethod]
    public void Abs_ReturnsAbsoluteComponents()
    {
        new IntVector2(-3, 4).Abs().Should().Be(new IntVector2(3, 4));
        new IntVector2(3, -4).Abs().Should().Be(new IntVector2(3, 4));
        new IntVector2(-3, -4).Abs().Should().Be(new IntVector2(3, 4));
        new IntVector2(3, 4).Abs().Should().Be(new IntVector2(3, 4));
    }

    [TestMethod]
    public void Min_TakesComponentWiseMinimum()
    {
        IntVector2.Min(new IntVector2(1, 5), new IntVector2(3, 2)).Should().Be(new IntVector2(1, 2));
    }

    [TestMethod]
    public void Max_TakesComponentWiseMaximum()
    {
        IntVector2.Max(new IntVector2(1, 5), new IntVector2(3, 2)).Should().Be(new IntVector2(3, 5));
    }

    [TestMethod]
    public void LengthSquared_Correct()
    {
        new IntVector2(3, 4).LengthSquared.Should().Be(25);
        IntVector2.ZERO.LengthSquared.Should().Be(0);
        new IntVector2(-3, 4).LengthSquared.Should().Be(25);
    }

    [TestMethod]
    public void Length_Correct()
    {
        new IntVector2(3, 4).Length.Should().BeApproximately(5.0, 1e-10);
        IntVector2.ZERO.Length.Should().Be(0.0);
    }

    [TestMethod]
    public void ManhattanLength_Correct()
    {
        new IntVector2(3, 4).ManhattanLength.Should().Be(7);
        new IntVector2(-3, 4).ManhattanLength.Should().Be(7);
        IntVector2.ZERO.ManhattanLength.Should().Be(0);
    }

    [TestMethod]
    public void DistanceSquared_Correct()
    {
        new IntVector2(0, 0).DistanceSquared(new IntVector2(3, 4)).Should().Be(25);
        new IntVector2(1, 1).DistanceSquared(new IntVector2(4, 5)).Should().Be(25);
    }

    [TestMethod]
    public void Distance_Correct()
    {
        new IntVector2(0, 0).Distance(new IntVector2(3, 4)).Should().BeApproximately(5.0, 1e-10);
        new IntVector2(0, 0).Distance(new IntVector2(0, 0)).Should().BeApproximately(0.0, 1e-10);
    }

    [TestMethod]
    public void ManhattanDistance_Correct()
    {
        new IntVector2(0, 0).ManhattanDistance(new IntVector2(3, 4)).Should().Be(7);
        new IntVector2(1, 1).ManhattanDistance(new IntVector2(4, 5)).Should().Be(7);
        new IntVector2(0, 0).ManhattanDistance(new IntVector2(-3, 4)).Should().Be(7);
    }

    [TestMethod]
    public void Random_MaxOnly_InRange()
    {
        var r = new Random(42L);
        for (int i = 0; i < 100; i++)
        {
            var v = IntVector2.Random(r, 10, 20);
            v.X.Should().BeInRange(0, 9);
            v.Y.Should().BeInRange(0, 19);
        }
    }

    [TestMethod]
    public void Random_Range_InRange()
    {
        var r = new Random(42L);
        for (int i = 0; i < 100; i++)
        {
            var v = IntVector2.Random(r, 5, 10, -3, 7);
            v.X.Should().BeInRange(5, 9);
            v.Y.Should().BeInRange(-3, 6);
        }
    }

    [TestMethod]
    public void Random_IsDeterministic()
    {
        var r1 = new Random(999L);
        var r2 = new Random(999L);
        for (int i = 0; i < 10; i++)
        {
            IntVector2.Random(r1, 100, 100).Should().Be(IntVector2.Random(r2, 100, 100));
        }
    }

    [TestMethod]
    public void Equals_Struct_True_WhenSame()
    {
        new IntVector2(1, 2).Equals(new IntVector2(1, 2)).Should().BeTrue();
        new IntVector2(1, 2).Equals(new IntVector2(3, 4)).Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Object_True_WhenSame()
    {
        ((object)new IntVector2(1, 2)).Equals((object)new IntVector2(1, 2)).Should().BeTrue();
        ((object)new IntVector2(1, 2)).Equals("other").Should().BeFalse();
        ((object)new IntVector2(1, 2)).Equals(null).Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_Consistent()
    {
        var a = new IntVector2(3, 4);
        var b = new IntVector2(3, 4);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [TestMethod]
    public void ToString_Formatted()
    {
        new IntVector2(3, 4).ToString().Should().Be("3,4");
        new IntVector2(-1, 0).ToString().Should().Be("-1,0");
    }

    // Extension methods
    [TestMethod]
    public void Perpendicular_Clockwise()
    {
        IntVector2.NORTH.Perpendicular().Should().Be(IntVector2.EAST);
        IntVector2.EAST.Perpendicular().Should().Be(IntVector2.SOUTH);
        IntVector2.SOUTH.Perpendicular().Should().Be(IntVector2.WEST);
        IntVector2.WEST.Perpendicular().Should().Be(IntVector2.NORTH);
    }

    [TestMethod]
    public void PerpendicularCounterClockwise_Correct()
    {
        IntVector2.NORTH.PerpendicularCounterClockwise().Should().Be(IntVector2.WEST);
        IntVector2.WEST.PerpendicularCounterClockwise().Should().Be(IntVector2.SOUTH);
        IntVector2.SOUTH.PerpendicularCounterClockwise().Should().Be(IntVector2.EAST);
        IntVector2.EAST.PerpendicularCounterClockwise().Should().Be(IntVector2.NORTH);
    }

    [TestMethod]
    public void IsBehind_East_SelfBehind()
    {
        new IntVector2(3, 2).IsBehind(new IntVector2(5, 2), IntVector2.EAST).Should().BeTrue();
    }

    [TestMethod]
    public void IsBehind_South_SelfBehind()
    {
        new IntVector2(2, 3).IsBehind(new IntVector2(2, 7), IntVector2.SOUTH).Should().BeTrue();
    }
}
