using Solnet.Rpc;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;

namespace Solnet.Serum.Examples
{
    public class MarketManagerSubscribeOpenOrders : IRunnableExample
    {
        
        /// <summary>
        /// Public key for Open Orders Account.
        /// </summary>
        private const string OpenOrdersAccountAddress = "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF";

        private readonly PublicKey _marketAddress = new("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly List<TradeEvent> _trades;


        public MarketManagerSubscribeOpenOrders()
        {
            Console.WriteLine($"Initializing {ToString()}");
            var rpcClient = Solnet.Rpc.ClientFactory.GetClient("https://node.openserum.ch/");
            var streamingRpcClient = Solnet.Rpc.ClientFactory.GetStreamingClient("wss://node.openserum.ch/ws/");
            _serumClient = ClientFactory.GetClient(rpcClient, streamingRpcClient);
            
            _serumClient.ConnectAsync().Wait();
            Console.WriteLine($"Initializing {ToString()}");
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, serumClient: _serumClient);
            _marketManager.InitAsync().Wait();
        }

        public void Run()
        {
            
            _marketManager.SubscribeTrades((trades, slot) =>
            {
                foreach (var t in trades)
                {
                    Console.WriteLine($"Trade: {t.Event.OrderId}\tPrice: {t.Price}\tSize: {t.Size}");
                }
            });
            
            
        }
    }
}