using Solnet.Rpc;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches the order book of a market.
    /// </summary>
    public class GetOrderBook : IRunnableExample
    {
        private ISerumClient _serumClient;

        private const string MarketAddress = "9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT";
        
        public GetOrderBook()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
            
            Market market = _serumClient.GetMarket(MarketAddress);
            
            Console.WriteLine($"Market:: Own Address: {market.OwnAddress.Key}\n" +
                              $"Base Mint: {market.BaseMint.Key} Quote Mint: {market.QuoteMint.Key}\n" +
                              $"Bids: {market.Bids.Key} Asks: {market.Asks.Key}");
            

            OrderBook bidOrderBook = _serumClient.GetOrderBook(market.Bids.Key);
            Console.WriteLine($"BidOrderBook:: SlabNodes: {bidOrderBook.Slab.Nodes.Count}"); 
            
            OrderBook askOrderBook = _serumClient.GetOrderBook(market.Asks.Key);
            Console.WriteLine($"AskOrderBook:: SlabNodes: {askOrderBook.Slab.Nodes.Count}");

            /*
             FULL ORDER BOOK STRUCTURE FROM HIGHEST ASK TO LOWEST BID
            List<Order> askOrders = askOrderBook.GetOrders();
            askOrders.Sort(Comparer<Order>.Create(
                (order, order1) =>
                {
                    if (order.Price > order1.Price) return -1;

                    return 1;
                }));
            askOrders.ForEach((order => Console.WriteLine($"SOL/USDC Ask:\t{order.Price}\tSize:\t{order.Quantity}")));
            
            List<Order> bidOrders = bidOrderBook.GetOrders();
            bidOrders.Sort(Comparer<Order>.Create(
                (order, order1) =>
                {
                    if (order.Price > order1.Price) return -1;

                    return 1;
                }));
            bidOrders.ForEach((order => Console.WriteLine($"SOL/USDC Bid:\t{order.Price}\tSize:\t{order.Quantity}")));
            */
            
            List<Order> askOrders = askOrderBook.GetOrders();
            askOrders.Sort(Comparer<Order>.Create(
                (order, order1) =>
                {
                    if (order.Price == order1.Price) return 0;
                    if (order.Price > order1.Price) return 1;
                    return -1;
                }));       
            
            List<Order> bidOrders = bidOrderBook.GetOrders();
            bidOrders.Sort(Comparer<Order>.Create(
                (order, order1) =>
                {
                    if (order.Price == order1.Price) return 0;
                    if (order.Price > order1.Price) return -1;
                    return 1;
                }));
            for (int i = 4; i >= 0; i--)
            {
                Console.WriteLine($"SOL/USDC Ask:\t{askOrders[i].Price}\tSize:\t{askOrders[i].Quantity}");
            }
            Console.WriteLine($"---------------------");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"SOL/USDC Bid:\t{bidOrders[i].Price}\tSize:\t{bidOrders[i].Quantity}");
            }

        }
    }
}