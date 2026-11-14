using Z2Randomizer.Tests;

[assembly: Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelize]

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]
