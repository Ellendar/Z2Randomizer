using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.Tests;

[TestClass]
public class EnemiesEditableTests
{
    [TestMethod]
    [DataRow(1, 13, 9)]
    [DataRow(2, 29, 25)]
    [DataRow(3, 45, 41)]
    [DataRow(4, 61, 57)]
    public void MirrorUsesRoomWidthAndReversesShooterDirections(int pages, int rightShooterX, int leftShooterX)
    {
        byte[] original =
        [
            5,
            0x32, (byte)EnemiesPalace125.ENERGY_BALL_SHOOTER_DOWN_LEFT,  // X = 2, Y = 5
            0x66, (byte)EnemiesPalace125.ENERGY_BALL_SHOOTER_DOWN_RIGHT // X = 6, Y = 8
        ];
        var enemies = new EnemiesEditable<EnemiesPalace125>(original);

        enemies.Mirror(pages);
        var mirrored = new EnemiesEditable<EnemiesPalace125>(enemies.Finalize());

        var rightShooter = mirrored.Enemies.Single(enemy => enemy.Id == EnemiesPalace125.ENERGY_BALL_SHOOTER_DOWN_RIGHT);
        var leftShooter = mirrored.Enemies.Single(enemy => enemy.Id == EnemiesPalace125.ENERGY_BALL_SHOOTER_DOWN_LEFT);
        Assert.AreEqual((rightShooterX, 5), (rightShooter.X, rightShooter.Y));
        Assert.AreEqual((leftShooterX, 8), (leftShooter.X, leftShooter.Y));

        mirrored.Mirror(pages);
        CollectionAssert.AreEqual(original, mirrored.Finalize());
    }
}
