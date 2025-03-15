using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockExchangeOrderMatching.Models
{
    public enum OrderType
    {
        Buy, Sell
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string Stock { get; set; }
        public OrderType Type { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public DateTime Time { get; set; }
    }
}
