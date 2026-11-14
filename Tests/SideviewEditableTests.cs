using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.Tests;

[TestClass]
public class SideviewEditableTests
{
    [TestMethod]
    [DataRow(1, 13, 7)]
    [DataRow(2, 29, 23)]
    [DataRow(3, 45, 39)]
    [DataRow(4, 61, 55)]
    public void MirrorUsesTheEncodedRoomWidth(int pages, int doorX, int brickRowX)
    {
        var sideview = new SideviewEditable<PalaceObject>([4, 0, 0, 0])
        {
            PageCount = (byte)pages
        };
        sideview.Add(new SideviewMapCommand<PalaceObject>(2, 3, PalaceObject.LOCKED_DOOR));
        sideview.Add(new SideviewMapCommand<PalaceObject>(6, 8, PalaceObject.BRICK_1_ROW)
        {
            Param = 2 // Three tiles wide.
        });

        byte[] original = sideview.Finalize();
        var mirrored = new SideviewEditable<PalaceObject>(original);
        mirrored.Mirror();
        mirrored = new SideviewEditable<PalaceObject>(mirrored.Finalize());

        Assert.AreEqual(pages, (int)mirrored.PageCount);
        Assert.AreEqual(doorX, mirrored.Commands.Single(command => command.IsLockedDoor()).AbsX);
        Assert.AreEqual(brickRowX, mirrored.Commands.Single(command => command.Id == PalaceObject.BRICK_1_ROW).AbsX);

        mirrored.Mirror();
        CollectionAssert.AreEqual(original, mirrored.Finalize());
    }
}
