using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // Enum for order type
    enum OrderType { Buy, Sell }

    // Order class to represent buy/sell orders
    class Order
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Stock { get; set; }
        public OrderType Type { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    // Trade class to represent executed trades
    class Trade
    {
        public int BuyOrderId { get; set; }
        public int SellOrderId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"#{BuyOrderId} {Price:F2} {Quantity} #{SellOrderId}";
        }
    }

    static void Main1(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: OrderMatchingSystem <input_file>");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' not found.");
            return;
        }

        List<Order> buyOrders = new List<Order>();
        List<Order> sellOrders = new List<Order>();
        List<Trade> trades = new List<Trade>();

        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(' ');
            if (parts.Length != 5)
                continue;

            var order = new Order
            {
                Id = int.Parse(parts[0].Trim('#')),
                Time = DateTime.Parse(parts[1]),
                Stock = parts[2],
                Type = parts[3] == "buy" ? OrderType.Buy : OrderType.Sell,
                Price = decimal.Parse(parts[4]),
                Quantity = int.Parse(parts[5])
            };

            if (order.Type == OrderType.Buy)
                buyOrders.Add(order);
            else
                sellOrders.Add(order);

            MatchOrders(buyOrders, sellOrders, trades);
        }

        // Print trades
        foreach (var trade in trades)
        {
            Console.WriteLine(trade);
        }
    }

    static void MatchOrders(List<Order> buyOrders, List<Order> sellOrders, List<Trade> trades)
    {
        // Sort buy orders: Highest price first, then time priority
        buyOrders = buyOrders.OrderByDescending(o => o.Price).ThenBy(o => o.Time).ToList();

        // Sort sell orders: Lowest price first, then time priority
        sellOrders = sellOrders.OrderBy(o => o.Price).ThenBy(o => o.Time).ToList();

        foreach (var buy in buyOrders.ToList()) // Clone list to avoid modification issue
        {
            foreach (var sell in sellOrders.ToList())
            {
                if (buy.Price >= sell.Price) // Matching condition
                {
                    int tradeQuantity = Math.Min(buy.Quantity, sell.Quantity);
                    trades.Add(new Trade
                    {
                        BuyOrderId = buy.Id,
                        SellOrderId = sell.Id,
                        Price = sell.Price,
                        Quantity = tradeQuantity
                    });

                    // Adjust quantities
                    buy.Quantity -= tradeQuantity;
                    sell.Quantity -= tradeQuantity;

                    if (sell.Quantity == 0)
                        sellOrders.Remove(sell);
                    if (buy.Quantity == 0)
                    {
                        buyOrders.Remove(buy);
                        break;
                    }
                }
            }
        }
    }
}