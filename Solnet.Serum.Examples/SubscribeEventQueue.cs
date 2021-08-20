using Solnet.Rpc;
using Solnet.Rpc.Core.Sockets;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Solnet.Serum.Examples
{
    public class Handler
    {
        public SubscriptionState State { get; set; }
        
        public PublicKey EventQueue { get; set; }
        
        public string Name { get; set; }

        public Handler(Subscription sub)
        {
            sub.SubscriptionState.SubscriptionChanged += SubscriptionChanged;
        }
        
        private void SubscriptionChanged(object sender, SubscriptionEvent e)
        {
            Console.WriteLine($"Market: {Name} EventQueue: {EventQueue} Subscription changed to: {e.Status}");
        }
    }
    
    /// <summary>
    /// An example which subscribes to the account data of a serum event queue.
    /// </summary>
    public class SubscribeEventQueue : IRunnableExample
    {
        private readonly ISerumClient _serumClient;
        
        private List<ISerumClient> _serumClients;
        
        /// <summary>
        /// Public key for SOL/USDC Serum Market.
        /// </summary>
        private const string MarketAddress = "9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT";

        private IList<Handler> _handlers;

        public SubscribeEventQueue()
        {
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClients = new List<ISerumClient>();
            _serumClient.ConnectAsync();
            Console.WriteLine($"Initializing {ToString()}");
        }

        public void Run()
        {
            SubscribeSingle();
        }

        /// <summary>
        /// If you do this, the RPC will neither hate nor love you, he'll just do as asked.
        /// This example filters events to see if they were a trade.
        /// </summary>
        public void SubscribeSingle()
        {
            Market market = _serumClient.GetMarket(MarketAddress);
                
            Console.WriteLine($"Market:: Own Address: {market.OwnAddress.Key} Base Mint: {market.BaseMint.Key} Quote Mint: {market.QuoteMint.Key}");
            Subscription sub = _serumClient.SubscribeEventQueue((subWrapper, evtQueue, _) =>
            {
                Console.WriteLine($"EventQueue:: Address: {subWrapper.Address.Key} Events: {evtQueue.Events.Count} Head: {evtQueue.Header.Head} Count: {evtQueue.Header.Count} Sequence: {evtQueue.Header.NextSequenceNumber}");

            }, market.EventQueue);
            
            Console.ReadKey();
        }

        /// <summary>
        /// If you do this the RPC will hate you and you will, most likely, not get any notifications at all.
        /// </summary>
        public void SubscribeToAllMarkets()
        {
            // get all markets
            IList<MarketInfo> markets = _serumClient.GetMarkets();
            _handlers = new List<Handler>(markets.Count);

            // attempt to subscribe to all event queues
            foreach (MarketInfo marketInfo in markets)
            {
                if (marketInfo.Deprecated) continue;
                Market market = _serumClient.GetMarket(marketInfo.Address);
                
                if(market == null) continue;
                Console.WriteLine($"Market:: Own Address: {market.OwnAddress.Key} Base Mint: {market.BaseMint.Key} Quote Mint: {market.QuoteMint.Key}");
                
                Subscription sub = _serumClient.SubscribeEventQueue((subWrapper, evtQueue, _) =>
                {
                    Console.WriteLine($"EventQueue::\tAddress: {subWrapper.Address.Key}\t\tEvents: {evtQueue.Events.Count}\t\tHead: {evtQueue.Header.Head}\t\tCount: {evtQueue.Header.Count}\t\tSequence: {evtQueue.Header.NextSequenceNumber}");
                }, market.EventQueue);
                
                _handlers.Add(new Handler(sub));
                Thread.Sleep(1000);
            }
            Console.ReadKey();
        }
    }
}