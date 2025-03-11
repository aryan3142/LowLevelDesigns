using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.ParkingLotSystem.Models
{
    public class Payment
    {
        public string PaymentId { get; set; }
        public int TicketId { get; set; }
        public bool IsPaid { get; set; }

        public Payment(int ticketId)
        {
            TicketId = ticketId;
            PaymentId = Guid.NewGuid().ToString();
        }

        public void MakePayment()
        {
            IsPaid = true;
        }
    }
}
