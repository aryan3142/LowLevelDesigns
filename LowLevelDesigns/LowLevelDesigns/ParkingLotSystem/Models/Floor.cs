using System;

public class Floor
{
    public int FloorNumber { get; set; }
    public int NumberOfSlots { get; set; }
    public List<Slot> Slots { get; set; }

    public int GetSlotForVehicleType(int vehicleType) 
    { 
        var slot = Slots.Where(x => !x.IsSlotTaken && x.SlotType == vehicleType)?.FirstOrDefault();

        if(slot != null)
        {
            Slots.Find(x => x == slot).IsSlotTaken = true;
            return slot.SlotNumber;
        }

        return -1;
    }

    public List<Slot> GetSlotAvailbilityInformation()
    {
        return Slots.Where(x => x.IsSlotTaken == false).ToList();
    }

    public bool GetSlotAvailbilityStatusForNonElectricVehicle(int vehicleType)
    {
        return Slots.Where(x => x.SlotType == vehicleType && x.IsSlotTaken == false).Any();
    }

    public bool GetSlotAvailbilityStatusForElectricVehicle(int vehicleType)
    {
        return Slots.Where(x => !x.IsSlotTaken && x.IsSlotForElectric && x.SlotType == vehicleType).Any();
    }

    public bool IsFloorFull()
    {
        return Slots.Where(x => x.IsSlotTaken).Count() == NumberOfSlots;
    }

}
