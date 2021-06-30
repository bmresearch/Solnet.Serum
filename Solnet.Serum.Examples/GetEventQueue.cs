using Solnet.Rpc;
using Solnet.Serum.Models;
using System;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches the account data of a serum event queue.
    /// </summary>
    public class GetEventQueue : IRunnableExample
    {
        private readonly ISerumClient _serumClient;
        
        /// <summary>
        /// Public key for SXP/USDC Serum Market.
        /// </summary>
        private const string MarketAddress = "13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR";

        public GetEventQueue()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
            
            Market res = _serumClient.GetMarket(MarketAddress);
            
            Console.WriteLine($"Market:: Own Address: {res.OwnAddress.Key}" +
                              $" Base Mint: {res.BaseMint.Key}" +
                              $" Quote Mint: {res.QuoteMint.Key}");

            EventQueue eventQueue = _serumClient.GetEventQueue(res.EventQueue);

            Console.WriteLine($"EventQueue:: Events: {eventQueue.Events.Count} Head: {eventQueue.Header.Head} Count: {eventQueue.Header.Count} Sequence: {eventQueue.Header.NextSeqNum}");
        }
    }
}