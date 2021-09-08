using Solnet.Rpc;
using Solnet.Serum.Models;
using System;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches the account data of a serum market.
    /// </summary>
    public class GetMarket : IRunnableExample
    {
        private readonly ISerumClient _serumClient;
        
        /// <summary>
        /// Public key for SXP/USDC Serum Market.
        /// </summary>
        private const string MarketAddress = "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ";

        public GetMarket()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
            
            Market res = _serumClient.GetMarket(MarketAddress);
            
            Console.WriteLine($"Market:: Own Address: {res.OwnAddress.Key} Base Mint: {res.BaseMint.Key} Quote Mint: {res.QuoteMint.Key}");
        }
    }
}