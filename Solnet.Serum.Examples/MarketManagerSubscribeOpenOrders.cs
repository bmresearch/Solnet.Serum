using Solnet.Rpc;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Solnet.Serum.Examples
{
    public class MarketManagerSubscribeOpenOrders : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly List<TradeEvent> _trades;


        public MarketManagerSubscribeOpenOrders()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);            
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, new ("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh"), serumClient: _serumClient);
            _marketManager.InitAsync().Wait();
        }

        public void Run()
        {
            
            _marketManager.SubscribeOpenOrders((openOrders, slot) =>
            {
                foreach (OpenOrder order in openOrders)
                {
                    Console.WriteLine($"OpenOrder:: IsBid: {order.IsBid} Price: {order.RawPrice}");
                }
            });


            Console.ReadKey();
        }
    }
}