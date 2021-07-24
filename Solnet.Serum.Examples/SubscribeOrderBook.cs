// unset

using Solnet.Rpc;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    public class SubscribeOrderBook: IRunnableExample
    {
        private readonly ISerumClient _serumClient;

        private static Dictionary<string, string> Markets = new Dictionary<string, string>()
        {
            {"SOL/USDC", "9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT"},
            {"SRM/USDT", "AtNnsY1AyRERWJ8xCskfz38YdvruWVJQUVXgScC1iPb"},
        };
        

        private List<OpenOrder> askOrders;
        private List<OpenOrder> bidOrders;

        public SubscribeOrderBook()
        {
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.Connect();
            Console.WriteLine($"Initializing {ToString()}");
        }

        public void Run()
        {
            foreach ((string key, string value) in Markets)
            {
                SubscribeTo(value, key);
            }
            
            Console.ReadKey();
        }
        
        public Task SubscribeTo(string address, string name)
        {
            return Task.Run(() =>
            {
                
            Market market = _serumClient.GetMarket(address);
                
            Console.WriteLine($"{name} Market:: Own Address: {market.OwnAddress.Key} Base Mint: {market.BaseMint.Key} Quote Mint: {market.QuoteMint.Key}");
                
            Subscription subBids = _serumClient.SubscribeOrderBookSide((subWrapper, orderBook, _) =>
            {
                Console.WriteLine($"{name} BidOrderBook Update:: SlabNodes: {orderBook.Slab.Nodes.Count}\n"); 
                bidOrders = orderBook.GetOrders();
                bidOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order1.RawPrice.CompareTo(order.RawPrice)));
                
                for (int i = 4; i >= 0; i--)
                {
                    Console.WriteLine($"{name} Ask:\t{askOrders[i].RawPrice}\tSize:\t{askOrders[i].RawQuantity}");
                }
                Console.WriteLine($"---------------------");
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"{name} Bid:\t{bidOrders[i].RawPrice}\tSize:\t{bidOrders[i].RawQuantity}");
                }
                Console.WriteLine($"---------------------\n");
                
            }, market.Bids);
            
            Subscription subAsks = _serumClient.SubscribeOrderBookSide((subWrapper, orderBook, _) =>
            {
                Console.WriteLine($"{name} AskOrderBook Update:: SlabNodes: {orderBook.Slab.Nodes.Count}\n"); 
                askOrders = orderBook.GetOrders();
                askOrders.Sort(Comparer<OpenOrder>.Create((order, order1) => order.RawPrice.CompareTo(order1.RawPrice)));
                Console.WriteLine($"---------------------");
                for (int i = 4; i >= 0; i--)
                {
                    Console.WriteLine($"{name} Ask:\t{askOrders[i].RawPrice}\tSize:\t{askOrders[i].RawQuantity}");
                }
                Console.WriteLine($"---------------------");
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"{name} Bid:\t{bidOrders[i].RawPrice}\tSize:\t{bidOrders[i].RawQuantity}");
                }
                Console.WriteLine($"---------------------\n");
                
            }, market.Asks);
            });
        }
    }
}