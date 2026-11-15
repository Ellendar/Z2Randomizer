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

    [TestMethod]
    [DataRow(10, 1, 8)]
    [DataRow(10, 2, 7)]
    [DataRow(5, 1, 8)]
    public void FindFloorUsesTheBridgeAboveLavaInDmInPalacesM15(int startY, int height, int expectedY)
    {
        var bytes = Convert.FromHexString("32600308D400F01F806FD60AD40CD40AF21F806FD20CD40AD200D809F01F806FD20CD809D200D409F0178067D20CD409D203");
        var sideview = new SideviewEditable<PalaceObject>(bytes);
        var grid = sideview.CreateSolidGrid();

        Assert.AreEqual(expectedY, SolidGridHelper.FindFloor(grid, 5, startY, 1, height));
    }

    [TestMethod]
    public void FindFloorReturnsZeroInDmInPalacesM15WithoutBridges()
    {
        var bytes = Convert.FromHexString("2A600308D400F01FD60AD40CD40AF21FD20CD40AD200D809F01FD20CD809D200D409F017D20CD409D203");
        var sideview = new SideviewEditable<PalaceObject>(bytes);
        var grid = sideview.CreateSolidGrid();

        Assert.AreEqual(0, SolidGridHelper.FindFloor(grid, 5, 10, 1, 1));
    }

    [TestMethod]
    public void FindFloorReturnsZeroOverUnbridgedLava()
    {
        // The lava covers columns 4 through 19, replacing the floor in those columns.
        var sideview = new SideviewEditable<PalaceObject>(Convert.FromHexString("08600308D400F01F"));
        var grid = sideview.CreateSolidGrid();

        Assert.AreEqual(0, SolidGridHelper.FindFloor(grid, 5, 10, 1, 1));
        Assert.AreEqual(10, SolidGridHelper.FindFloor(grid, 20, 10, 1, 1));
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(58)]
    public void FindFloorUsesBrickPlatformsOverPitsInWinterSolsticeM10(int x)
    {
        var bytes = Convert.FromHexString("2960030804FF802F96C3240706FF802F94C3730F0C09FE1007802E94C3A40707F83007802892C3D703");
        var sideview = new SideviewEditable<PalaceObject>(bytes);
        var grid = sideview.CreateSolidGrid();

        Assert.IsFalse(grid[x, 12]);
        Assert.AreEqual(7, SolidGridHelper.FindFloor(grid, x, 10, 1, 1));
    }

    [TestMethod]
    public void FindFloorRequiresSupportInsideTheGrid()
    {
        var grid = new bool[1, 13];

        Assert.AreEqual(0, SolidGridHelper.FindFloor(grid, 0, 10, 1, 1));
    }
}
