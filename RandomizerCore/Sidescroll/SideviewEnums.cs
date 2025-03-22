using System;

namespace RandomizerCore.Sidescroll;

public enum PalaceObject
{
    Window = 0x00,
    UnicornHead = 0x01,
    RatHead = 0x02,
    CrystalReturnStatue1 = 0x03,
    CrystalReturnStatue2 = 0x04,
    LockedDoor = 0x05,
    LargeCloud = 0x07,
    SmallCloud1 = 0x08,
    IronknuckleStatue = 0x09,
    SmallCloud2 = 0x0A,
    SmallCloud3 = 0x0B,
    SmallCloud4 = 0x0C,
    SmallCloud5 = 0x0D,
    SmallCloud6 = 0x0E,
    Collectable = 0x0F,
    HorizontalPitOrLava = 0x10,
    HorizontalBrick = 0x20,
    BreakableBlock1 = 0x30,
    SteelBrick = 0x40,
    CrumbleBridgeOrElevator = 0x50,
    Bridge = 0x60,
    PalaceBricks = 0x70,
    Curtains = 0x80,
    BreakableBlock2 = 0x90,
    WalkThruBricks = 0xA0,
    BreakableBlockVertical = 0xB0,
    Pillar = 0xC0,
    VerticalPit1 = 0xD0,
    VerticalPit2 = 0xE0,
    HorizontalPit = 0xF0,
}

public enum GreatPalaceObject
{
    Window = 0x00,
    UnicornHead = 0x01,
    RatHead = 0x02,
    CrystalReturnStatue1 = 0x03,
    CrystalReturnStatue2 = 0x04,
    LockedDoor = 0x05,
    ElevatorShaft = 0x06,
    LargeCloud = 0x07,
    SmallCloud1 = 0x08,
    SleepingZelda = 0x09,
    BirdKnight = 0x0A,
    SmallCloud2 = 0x0B,
    SmallCloud3 = 0x0C,
    SmallCloud4 = 0x0D,
    SmallCloud5 = 0x0E,
    Collectable = 0x0F,
    FinalBossCanopyOrLava = 0x10,
    HorizontalBrick = 0x20,
    BreakableBlock1 = 0x30,
    SteelBrick = 0x40,
    NorthCastleBricksOrElevator = 0x50,
    NorthCastleSteps = 0x60,
    CrumbleBridge = 0x70,
    Bridge = 0x80,
    Bricks = 0x90,
    Curtains = 0xA0,
    WalkThruBricks = 0xB0,
    BreakableBlock2 = 0xC0,
    BreakableBlockVertical = 0xD0,
    ElectricBarrier = 0xE0,
    Pillar = 0xF0
}

public static class PalaceObjectExtensions
{
    public static int Width(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.HorizontalPitOrLava:
            case PalaceObject.HorizontalBrick:
            case PalaceObject.BreakableBlock1:
            case PalaceObject.SteelBrick:
            case PalaceObject.CrumbleBridgeOrElevator:
            case PalaceObject.Bridge:
            case PalaceObject.PalaceBricks:
            case PalaceObject.Curtains:
            case PalaceObject.BreakableBlock2:
            case PalaceObject.WalkThruBricks:
            case PalaceObject.HorizontalPit:
                return 1 + command.Param;
            case PalaceObject.LargeCloud:
            case PalaceObject.SmallCloud1:
            case PalaceObject.SmallCloud2:
            case PalaceObject.SmallCloud3:
            case PalaceObject.SmallCloud4:
            case PalaceObject.SmallCloud5:
            case PalaceObject.SmallCloud6:
                return 2;
            case PalaceObject.CrystalReturnStatue1:
            case PalaceObject.CrystalReturnStatue2:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.BreakableBlockVertical:
            case PalaceObject.Pillar:
            case PalaceObject.VerticalPit1:
            case PalaceObject.VerticalPit2:
                return 1 + command.Param;
            case PalaceObject.HorizontalPit:
                return 13 - command.Y;
            case PalaceObject.Window:
            case PalaceObject.IronknuckleStatue:
            case PalaceObject.Bridge:
            case PalaceObject.PalaceBricks:
            case PalaceObject.Curtains:
            case PalaceObject.BreakableBlock2:
            case PalaceObject.WalkThruBricks:
                return 2;
            case PalaceObject.LockedDoor:
                return 3;
            case PalaceObject.CrystalReturnStatue1:
            case PalaceObject.CrystalReturnStatue2:
                return 5;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.Window:
            case PalaceObject.UnicornHead:
            case PalaceObject.RatHead:
            case PalaceObject.CrystalReturnStatue1:
            case PalaceObject.CrystalReturnStatue2:
            case PalaceObject.LockedDoor:
            case PalaceObject.LargeCloud:
            case PalaceObject.SmallCloud1:
            case PalaceObject.IronknuckleStatue:
            case PalaceObject.SmallCloud2:
            case PalaceObject.SmallCloud3:
            case PalaceObject.SmallCloud4:
            case PalaceObject.SmallCloud5:
            case PalaceObject.SmallCloud6:
            case PalaceObject.Collectable:
            case PalaceObject.HorizontalPitOrLava:
            case PalaceObject.Curtains:
            case PalaceObject.WalkThruBricks:
            case PalaceObject.Pillar:
            case PalaceObject.VerticalPit1:
            case PalaceObject.VerticalPit2:
            case PalaceObject.HorizontalPit:
                return false;
            case PalaceObject.HorizontalBrick:
            case PalaceObject.BreakableBlock1:
            case PalaceObject.SteelBrick:
            case PalaceObject.CrumbleBridgeOrElevator:
            case PalaceObject.Bridge:
            case PalaceObject.PalaceBricks:
            case PalaceObject.BreakableBlock2:
            case PalaceObject.BreakableBlockVertical:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public static bool IsBreakable(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.BreakableBlock1:
            case PalaceObject.BreakableBlock2:
            case PalaceObject.BreakableBlockVertical:
                return true;
            default:
                return false;
        }
    }
}

public static class GreatPalaceObjectExtensions
{
    public static int Width(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.FinalBossCanopyOrLava:
            case GreatPalaceObject.HorizontalBrick:
            case GreatPalaceObject.BreakableBlock1:
            case GreatPalaceObject.SteelBrick:
            case GreatPalaceObject.NorthCastleBricksOrElevator:
            case GreatPalaceObject.NorthCastleSteps:
            case GreatPalaceObject.CrumbleBridge:
            case GreatPalaceObject.Bridge:
            case GreatPalaceObject.Bricks:
            case GreatPalaceObject.Curtains:
            case GreatPalaceObject.WalkThruBricks:
            case GreatPalaceObject.BreakableBlock2:
                return 1 + command.Param;
            case GreatPalaceObject.LargeCloud:
            case GreatPalaceObject.SmallCloud1:
            case GreatPalaceObject.SmallCloud2:
            case GreatPalaceObject.SmallCloud3:
            case GreatPalaceObject.SmallCloud4:
            case GreatPalaceObject.SmallCloud5:
            case GreatPalaceObject.SleepingZelda:
                return 2;
            case GreatPalaceObject.CrystalReturnStatue1:
            case GreatPalaceObject.CrystalReturnStatue2:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.BreakableBlockVertical:
            case GreatPalaceObject.ElectricBarrier:
            case GreatPalaceObject.Pillar:
                return 1 + command.Param;
            case GreatPalaceObject.ElevatorShaft:
                return 13 - command.Y;
            case GreatPalaceObject.Window:
            case GreatPalaceObject.BirdKnight:
            case GreatPalaceObject.Bridge:
            case GreatPalaceObject.Bricks:
            case GreatPalaceObject.Curtains:
            case GreatPalaceObject.WalkThruBricks:
            case GreatPalaceObject.BreakableBlock2:
                return 2;
            case GreatPalaceObject.LockedDoor:
                return 3;
            case GreatPalaceObject.CrystalReturnStatue1:
            case GreatPalaceObject.CrystalReturnStatue2:
                return 5;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.Window:
            case GreatPalaceObject.UnicornHead:
            case GreatPalaceObject.RatHead:
            case GreatPalaceObject.CrystalReturnStatue1:
            case GreatPalaceObject.CrystalReturnStatue2:
            case GreatPalaceObject.LockedDoor:
            case GreatPalaceObject.ElevatorShaft:
            case GreatPalaceObject.LargeCloud:
            case GreatPalaceObject.SmallCloud1:
            case GreatPalaceObject.SleepingZelda:
            case GreatPalaceObject.BirdKnight:
            case GreatPalaceObject.SmallCloud2:
            case GreatPalaceObject.SmallCloud3:
            case GreatPalaceObject.SmallCloud4:
            case GreatPalaceObject.SmallCloud5:
            case GreatPalaceObject.Collectable:
            case GreatPalaceObject.FinalBossCanopyOrLava:
            case GreatPalaceObject.Curtains:
            case GreatPalaceObject.WalkThruBricks:
            case GreatPalaceObject.ElectricBarrier:
            case GreatPalaceObject.Pillar:
                return false;
            case GreatPalaceObject.HorizontalBrick:
            case GreatPalaceObject.BreakableBlock1:
            case GreatPalaceObject.SteelBrick:
            case GreatPalaceObject.NorthCastleBricksOrElevator:
            case GreatPalaceObject.NorthCastleSteps:
            case GreatPalaceObject.CrumbleBridge:
            case GreatPalaceObject.Bridge:
            case GreatPalaceObject.Bricks:
            case GreatPalaceObject.BreakableBlock2:
            case GreatPalaceObject.BreakableBlockVertical:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public static bool IsBreakable(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.BreakableBlock1:
            case GreatPalaceObject.BreakableBlock2:
            case GreatPalaceObject.BreakableBlockVertical:
                return true;
            default:
                return false;
        }
    }
}
