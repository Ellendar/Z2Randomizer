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

    [TestMethod]
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

    [TestMethod]
    public void TestWithHardRequirementFoldsIntoComplexRequirements()
    {
        Requirements requirements = new(
            [RequirementType.KEY],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.JUMP], [[RequirementType.FAIRY]])]);

        Requirements result = requirements.WithHardRequirement(RequirementType.DASH);

        Assert.AreEqual(1, result.CompositeRequirements.Length);
        CollectionAssert.AreEqual(new[] { RequirementType.KEY, RequirementType.DASH }, result.CompositeRequirements[0]);
        Assert.AreEqual(1, result.ComplexRequirements.Length);
        Assert.AreEqual(RequirementTarget.EXIT, result.ComplexRequirements[0].Target);
        Assert.AreEqual(0, result.ComplexRequirements[0].IndividualRequirements.Length);
        Assert.AreEqual(2, result.ComplexRequirements[0].CompositeRequirements.Length);
        CollectionAssert.AreEqual(new[] { RequirementType.JUMP, RequirementType.DASH }, result.ComplexRequirements[0].CompositeRequirements[0]);
        CollectionAssert.AreEqual(new[] { RequirementType.FAIRY, RequirementType.DASH }, result.ComplexRequirements[0].CompositeRequirements[1]);
    }

    [TestMethod]
    public void TestWithHardRequirementOnEmptyComplexRequirement()
    {
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.ITEM, [], [])]);

        Requirements result = requirements.WithHardRequirement(RequirementType.KEY);

        Assert.AreEqual(1, result.ComplexRequirements.Length);
        CollectionAssert.AreEqual(new[] { RequirementType.KEY }, result.ComplexRequirements[0].CompositeRequirements[0]);
    }

    [TestMethod]
    public void TestWithoutRemovesFromComplexRequirements()
    {
        Requirements requirements = new(
            [RequirementType.KEY],
            [],
            [new ComplexRequirement(RequirementTarget.KEY, [RequirementType.JUMP], [[RequirementType.FAIRY, RequirementType.KEY]])]);

        Requirements result = requirements.Without(RequirementType.KEY);

        Assert.AreEqual(0, result.IndividualRequirements.Length);
        Assert.AreEqual(1, result.ComplexRequirements.Length);
        // FAIRY survives in the composite and collapses to an individual requirement after KEY is removed
        CollectionAssert.AreEqual(new[] { RequirementType.JUMP, RequirementType.FAIRY }, result.ComplexRequirements[0].IndividualRequirements);
        Assert.AreEqual(0, result.ComplexRequirements[0].CompositeRequirements.Length);
    }

    [TestMethod]
    public void TestWithoutDropsEmptyComplexRequirements()
    {
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.ITEM, [RequirementType.HAMMER], [])]);

        Requirements result = requirements.Without(RequirementType.HAMMER);

        Assert.AreEqual(0, result.ComplexRequirements.Length);
    }

    [TestMethod]
    public void TestHasHardRequirementConsidersComplexRequirements()
    {
        Requirements requirements = new(
            [RequirementType.KEY],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.JUMP], [])]);

        // KEY is required by the top-level but not by the complex requirement, so it is not a hard requirement
        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.KEY));
        // JUMP is required by the complex requirement but not by the top-level, so it is not a hard requirement
        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.JUMP));
    }

    [TestMethod]
    public void TestHasHardRequirementTrueWhenAllPartsRequireIt()
    {
        Requirements requirements = new(
            [RequirementType.JUMP],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.JUMP], [])]);

        Assert.IsTrue(requirements.HasHardRequirement(RequirementType.JUMP));
        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.KEY));
    }

    [TestMethod]
    public void TestHasHardRequirementRequiresAllComplexRequirements()
    {
        Requirements requirements = new(
            [],
            [],
            [
                new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.JUMP], []),
                new ComplexRequirement(RequirementTarget.ITEM, [RequirementType.KEY], []),
            ]);

        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.JUMP));
    }

    [TestMethod]
    public void TestHasHardRequirementSingleElementComposite()
    {
        Requirements requirements = new([], [[RequirementType.JUMP]]);

        Assert.IsTrue(requirements.HasHardRequirement(RequirementType.JUMP));
        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.KEY));
    }

    [TestMethod]
    public void TestHasHardRequirementRedundantComposite()
    {
        Requirements requirements = new([], [[RequirementType.JUMP, RequirementType.JUMP]]);

        Assert.IsTrue(requirements.HasHardRequirement(RequirementType.JUMP));
    }

    [TestMethod]
    public void TestHasHardRequirementCompositeMissingRequireable()
    {
        Requirements requirements = new([], [[RequirementType.FAIRY, RequirementType.DASH]]);

        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.JUMP));
    }

    [TestMethod]
    public void TestHasHardRequirementInComplexRequirements()
    {
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [], [[RequirementType.JUMP]])]);

        Assert.IsTrue(requirements.HasHardRequirement(RequirementType.JUMP));
        Assert.IsFalse(requirements.HasHardRequirement(RequirementType.KEY));
    }

    [TestMethod]
    public void TestRoutesGateRequirementsByDirection()
    {
        RequirementRoute[] routes = [new([Direction.EAST], []), new([], [Direction.EAST])];
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.GLOVE], [], routes)]);

        // Entering from the EAST or exiting to the EAST applies the GLOVE requirement.
        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE }, RequirementTarget.EXIT, enterFrom: Direction.EAST, exitTo: Direction.WEST));
        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE }, RequirementTarget.EXIT, enterFrom: Direction.WEST, exitTo: Direction.EAST));
        Assert.IsFalse(requirements.AreSatisfiedBy(new HashSet<RequirementType>(), RequirementTarget.EXIT, enterFrom: Direction.WEST, exitTo: Direction.EAST));
        // An exit covered by no route is not gated by this requirement (no requirement applies),
        // so it is passable even without the requirement.
        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE }, RequirementTarget.EXIT, enterFrom: Direction.WEST, exitTo: Direction.WEST));
        Assert.IsTrue(requirements.AreSatisfiedBy([], RequirementTarget.EXIT, enterFrom: Direction.WEST, exitTo: Direction.WEST));
        // Without a direction context the requirement applies as it did before routes existed.
        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE }, RequirementTarget.EXIT));
    }

    [TestMethod]
    public void TestUnmatchedRouteMeansNoRequirement()
    {
        // The only requirement is "GLOVE to/from EAST". Exits that don't touch EAST
        // must therefore require nothing at all.
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.GLOVE], [], [new([Direction.EAST], [])])]);

        // EAST is gated: needs GLOVE, and is blocked without it.
        Assert.IsTrue(requirements.AreSatisfiedBy([RequirementType.GLOVE], RequirementTarget.EXIT, enterFrom: Direction.EAST, exitTo: Direction.WEST));
        Assert.IsFalse(requirements.AreSatisfiedBy([], RequirementTarget.EXIT, enterFrom: Direction.EAST, exitTo: Direction.WEST));

        // Any traversal that doesn't enter from EAST is ungated.
        Assert.IsTrue(requirements.AreSatisfiedBy([], RequirementTarget.EXIT, enterFrom: Direction.WEST, exitTo: Direction.EAST));
        Assert.IsTrue(requirements.AreSatisfiedBy([], RequirementTarget.EXIT, enterFrom: Direction.NORTH, exitTo: Direction.SOUTH));
    }

    [TestMethod]
    public void TestComplexRequirementTargetedElsewhereDoesNotGate()
    {
        // A room whose only requirement is a complex requirement targeting KEY
        // must not have that requirement gate exits (EXIT target) or items (ITEM target).
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.KEY, [], [[RequirementType.GLOVE, RequirementType.DOWNSTAB]])]);

        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE, RequirementType.DOWNSTAB }, RequirementTarget.EXIT));
        Assert.IsTrue(requirements.AreSatisfiedBy([], RequirementTarget.EXIT));
        Assert.IsTrue(requirements.AreSatisfiedBy([], RequirementTarget.ITEM));
        // The requirement still gates the KEY target it declares.
        Assert.IsTrue(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE, RequirementType.DOWNSTAB }, RequirementTarget.KEY));
        Assert.IsFalse(requirements.AreSatisfiedBy(new[] { RequirementType.GLOVE }, RequirementTarget.KEY));
    }

    [TestMethod]
    public void TestUntargetedRequirementsStillGateWhenComplexTargetDiffers()
    {
        // Individual/composite requirements carry no target and always apply, even
        // when the only complex requirement targets something else.
        Requirements requirements = new(
            [RequirementType.GLOVE],
            [],
            [new ComplexRequirement(RequirementTarget.KEY, [], [[RequirementType.DASH]])]);

        Assert.IsFalse(requirements.AreSatisfiedBy([], RequirementTarget.EXIT));
        Assert.IsTrue(requirements.AreSatisfiedBy([RequirementType.GLOVE], RequirementTarget.EXIT));
    }

    [TestMethod]
    public void TestExitRequiresKey()
    {
        RequirementRoute[] routes = [new([Direction.EAST], []), new([], [Direction.EAST])];
        Requirements requirements = new(
            [],
            [],
            [new ComplexRequirement(RequirementTarget.EXIT, [RequirementType.KEY], [], routes)]);

        // Only the EAST-gated exit requires a key.
        Assert.IsTrue(requirements.ExitRequiresKey(Direction.WEST, Direction.EAST));
        Assert.IsFalse(requirements.ExitRequiresKey(Direction.WEST, Direction.WEST));
        // A room-level KEY requirement applies to every exit.
        Assert.IsTrue(new Requirements([RequirementType.KEY]).ExitRequiresKey(Direction.WEST, Direction.WEST));
        // Without KEY in any complex requirement the locked door blocks every exit.
        Assert.IsTrue(new Requirements([RequirementType.GLOVE]).ExitRequiresKey(Direction.WEST, Direction.EAST));
    }
}
