using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.ParkingLotSystem.Models
{
    public class Ticket
    {
        public int TicketNumber { get; set; }
        public DateTime EntryTime { get; set;}
        public DateTime ExitTime { get; set; }
        public Slot SlotDetail { get; set; }
        public Vehicle Vehicle { get; set; }
        public Payment PaymentDetail { get; set; }

        public Ticket(Slot slotDetail)
        {
            TicketNumber = new Random().Next(1000, 999999);
            EntryTime = DateTime.Now;
            SlotDetail = slotDetail;
        }

        public double GetTicketPrice()
        {
            double price = 0d;
            int hours = (ExitTime - EntryTime).Hours;
            if (hours > 0)
            {
                price += 4;
                hours -= 1;
            }

            if (hours > 2)
            {
                price += (3.5 * 2) + (2.5 * (hours - 2));
            }
            else if(hours > 0 && hours <= 2)
            {
                price += (3.5 * hours);
            }

            return price;
        }
    }
}
