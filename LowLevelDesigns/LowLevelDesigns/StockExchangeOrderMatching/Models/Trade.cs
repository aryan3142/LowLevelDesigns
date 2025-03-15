using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockExchangeOrderMatching.Models
{
    public class Trade
    {
        public int BuyOrderId { get; set; }
        public int SellOrderId { get; set; }
        public double TradePrice { get; set; }
        public int TradeQuantity { get; set; }

        public override string ToString()
        {
            return $"#{BuyOrderId} {TradePrice:F2} {TradeQuantity} #{SellOrderId}";
        }
    }
}
