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
            askOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order.RawPrice.CompareTo(order1.RawPrice)));
            askOrders.ForEach((order => Console.WriteLine($"SOL/USDC Ask:\t{order.RawPrice}\tSize:\t{order.RawQuantity}")));
            
            List<Order> bidOrders = bidOrderBook.GetOrders();
            bidOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order1.RawPrice.CompareTo(order.RawPrice)));
            bidOrders.ForEach((order => Console.WriteLine($"SOL/USDC Bid:\t{order.RawPrice}\tSize:\t{order.RawQuantity}")));
            */
            
            List<OpenOrder> askOrders = askOrderBook.GetOrders();
            askOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order.RawPrice.CompareTo(order1.RawPrice)));       
            
            List<OpenOrder> bidOrders = bidOrderBook.GetOrders();
            bidOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order1.RawPrice.CompareTo(order.RawPrice)));
            for (int i = 4; i >= 0; i--)
            {
                Console.WriteLine($"SOL/USDC Ask:\t{askOrders[i].RawPrice}\tSize:\t{askOrders[i].RawQuantity}");
            }
            Console.WriteLine($"---------------------");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"SOL/USDC Bid:\t{bidOrders[i].RawPrice}\tSize:\t{bidOrders[i].RawQuantity}");
            }

        }
    }
}