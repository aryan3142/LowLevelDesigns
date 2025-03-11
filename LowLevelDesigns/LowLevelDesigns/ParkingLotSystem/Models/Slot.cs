using System;

public class Slot
{
    public int SlotNumber { get; set; }
    public int SlotType { get; set; }
    public bool IsSlotForElectric { get; set; }
    public bool IsSlotTaken { get; set; }
    public Floor FloorDetail { get; set; }
}
