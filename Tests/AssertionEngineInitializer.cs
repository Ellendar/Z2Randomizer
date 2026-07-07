namespace Z2Randomizer.Tests;

public static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning()
    {
        FluentAssertions.License.Accepted = true;
    }
}
