using StockExchangeOrderMatching.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockExchangeOrderMatching
{
    public class ProcessTrades
    {
        public static void Main(string[] args)
        {
            var filePath = "";
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File path is empty or incorrect");
                return;
            }

            List<Order> buyOrders = new List<Order>();
            List<Order> sellOrders = new List<Order>();
            List<Trade> tradesExecuted = new List<Trade>();
            foreach (var line in File.ReadLines(filePath))
            {
                var data = line.Split(" ");
                if (data.Length <= 5) continue;

                var order = new Order()
                {
                    OrderId = int.Parse(data[0].Trim('#')),
                    Time = DateTime.Parse(data[1]),
                    Stock = data[2],
                    Type = data[3] == "buy" ? OrderType.Buy : OrderType.Sell,
                    Price = double.Parse(data[4]),
                    Quantity = int.Parse(data[5])
                };

                if (order.Type == OrderType.Buy)
                    buyOrders.Add(order);
                else
                    sellOrders.Add(order);
            }

            MatchOrder(buyOrders, sellOrders, tradesExecuted);
        }

        private static void MatchOrder(List<Order> buyOrders, List<Order> sellOrders, List<Trade> tradesExecuted)
        {
            buyOrders = buyOrders.OrderByDescending(x => x.Price).ThenBy(x => x.Time).ToList();

            sellOrders = sellOrders.OrderBy(x => x.Price).ThenBy(x => x.Time).ToList();

            foreach (var buyOrder in buyOrders.ToList())
            {
                foreach (var sellOrder in sellOrders.ToList())
                {
                    if (buyOrder.Stock == sellOrder.Stock && buyOrder.Price >= sellOrder.Price)
                    {
                        double tradePrice = sellOrder.Price;
                        int tradeQuantity = Math.Min(sellOrder.Quantity, buyOrder.Quantity);
                        var trade = new Trade()
                        {
                            BuyOrderId = buyOrder.OrderId,
                            SellOrderId = sellOrder.OrderId,
                            TradeQuantity = tradeQuantity,
                            TradePrice = tradePrice
                        };

                        sellOrder.Quantity -= tradeQuantity;
                        if (sellOrder.Quantity <= 0) sellOrders.Remove(sellOrder);

                        buyOrder.Quantity -= tradeQuantity;
                        if (buyOrder.Quantity <= 0) buyOrders.Remove(buyOrder);
                    }
                }
            }
        }
    }
}
