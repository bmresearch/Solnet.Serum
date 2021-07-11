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
        

        private List<Order> askOrders;
        private List<Order> bidOrders;

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
                
            Subscription subBids = _serumClient.SubscribeOrderBook((subWrapper, orderBook) =>
            {
                Console.WriteLine($"{name} BidOrderBook Update:: SlabNodes: {orderBook.Slab.Nodes.Count}\n"); 
                bidOrders = orderBook.GetOrders();
                bidOrders.Sort(Comparer<Order>.Create(
                    (order, order1) =>
                    {
                        if (order.Price == order1.Price) return 0;
                        if (order.Price > order1.Price) return -1;
                        return 1;
                    }));
                
                for (int i = 4; i >= 0; i--)
                {
                    Console.WriteLine($"{name} Ask:\t{askOrders[i].Price}\tSize:\t{askOrders[i].Quantity}");
                }
                Console.WriteLine($"---------------------");
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"{name} Bid:\t{bidOrders[i].Price}\tSize:\t{bidOrders[i].Quantity}");
                }
                Console.WriteLine($"---------------------\n");
                
            }, market.Bids);
            
            Subscription subAsks = _serumClient.SubscribeOrderBook((subWrapper, orderBook) =>
            {
                Console.WriteLine($"{name} AskOrderBook Update:: SlabNodes: {orderBook.Slab.Nodes.Count}\n"); 
                askOrders = orderBook.GetOrders();
                askOrders.Sort(Comparer<Order>.Create(
                    (order, order1) =>
                    {
                        if (order.Price == order1.Price) return 0;
                        if (order.Price > order1.Price) return 1;
                        return -1;
                    }));
                Console.WriteLine($"---------------------");
                for (int i = 4; i >= 0; i--)
                {
                    Console.WriteLine($"{name} Ask:\t{askOrders[i].Price}\tSize:\t{askOrders[i].Quantity}");
                }
                Console.WriteLine($"---------------------");
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"{name} Bid:\t{bidOrders[i].Price}\tSize:\t{bidOrders[i].Quantity}");
                }
                Console.WriteLine($"---------------------\n");
                
            }, market.Asks);
            });
        }
    }
}