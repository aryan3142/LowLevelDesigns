using System;

public class ParkingLot
{
    private int initialFloorCount = 4;
    public int FloorCount { get; set; }
    public bool IsParkingLotFull { get; set; }

    public ParkingLot()
    {
        FloorCount = initialFloorCount;
        IsParkingLotFull = GetParkingLotStatus();
    }

    private bool GetParkingLotStatus()
    {
        
    }
}
