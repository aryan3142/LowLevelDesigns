using LowLevelDesigns.ParkingLotSystem.DataHelper;
using LowLevelDesigns.ParkingLotSystem.Models;
using System;

public class ParkingLot
{
    private int initialFloorCount = 4;
    public int FloorCount { get; set; }
    public bool IsParkingLotFull { get; set; }

    public ParkingLot()
    {
        FloorCount = initialFloorCount;
        FloorCount = DataSource.Floors.Count;
        IsParkingLotFull = GetParkingLotStatus();
    }

    public bool GetParkingLotStatus()
    {
        bool isFull = true;
        DataSource.Floors.ForEach(x =>
        {
            if (!x.IsFloorFull())
            {
                isFull = false;
            }
        });

        return isFull;
    }

    public bool AssignSlot(int vehicleType, bool isElectric, int vehicleNumber)
    {
        if (IsParkingLotFull)
        {
            return false;
        }

        foreach(var floor in DataSource.Floors)
        {
            if (isElectric)
            {
                if (floor.GetSlotAvailbilityStatusForElectricVehicle(vehicleType))
                {
                    var slot = floor.Slots.FirstOrDefault(x => x.SlotType == vehicleType && x.IsSlotForElectric);
                    slot.IsSlotTaken = true;
                    AssignTicket(slot, vehicleNumber, vehicleType, isElectric);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (floor.GetSlotAvailbilityStatusForNonElectricVehicle(vehicleType))
                {
                    var slot = floor.Slots.FirstOrDefault(x => x.SlotType == vehicleType && x.IsSlotForElectric);
                    slot.IsSlotTaken = true;
                    AssignTicket(slot, vehicleNumber, vehicleType, isElectric);
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void ReleaseSlot(int ticketId)
    {
        Ticket ticket = DataSource.Tickets.FirstOrDefault(x => x.TicketNumber == ticketId);
        ticket.PaymentDetail = new Payment(ticketId);
        ticket.PaymentDetail.MakePayment();
        ticket.SlotDetail.IsSlotTaken = false;
    }


    private void AssignTicket(Slot slot, int vehicleNumber, int vehicleType, bool isElectric = false)
    {
        Ticket ticket = new Ticket(slot)
        {
            Vehicle = new Vehicle()
            {
                vehicleType = (VehicleType)vehicleType,
                VehicleNumber = vehicleNumber,
                IsElectric = isElectric
            }
        };

        DataSource.Tickets.Add(ticket);
    }
}
