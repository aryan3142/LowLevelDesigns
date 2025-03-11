using LowLevelDesigns.ParkingLotSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.ParkingLotSystem.DataHelper
{
    public static class DataSource
    {
        public static List<Floor> Floors = new List<Floor>()
        {
            new Floor()
            {
                FloorNumber = 1,
                Slots = new List<Slot>()
                 {
                    new Slot()
                    {
                        SlotNumber = 1,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 2,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 3,
                        SlotType = 1,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 4,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 5,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 6,
                        SlotType = 2,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 7,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 8,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 9,
                        SlotType = 3,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    }
                },
                NumberOfSlots = 12
            },
            new Floor()
            {
                FloorNumber = 2,
                Slots = new List<Slot>()
                 {
                    new Slot()
                    {
                        SlotNumber = 1,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 2,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 3,
                        SlotType = 1,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 4,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 5,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 6,
                        SlotType = 2,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 7,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 8,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 9,
                        SlotType = 3,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    }
                },
                NumberOfSlots = 12
            },
            new Floor()
            {
                FloorNumber = 3,
                Slots = new List<Slot>()
                 {
                    new Slot()
                    {
                        SlotNumber = 1,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 2,
                        SlotType = 1,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 3,
                        SlotType = 1,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 4,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 5,
                        SlotType = 2,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 6,
                        SlotType = 2,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 7,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 8,
                        SlotType = 3,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 9,
                        SlotType = 3,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = false,
                        IsSlotTaken = false
                    },
                    new Slot()
                    {
                        SlotNumber = 10,
                        SlotType = 4,
                        IsSlotForElectric = true,
                        IsSlotTaken = false
                    }
                },
                NumberOfSlots = 12
            }
        };

        public static List<Ticket> Tickets = new List<Ticket>();
    }
}
