using Solnet.Rpc;
using Solnet.Serum.Models;
using System;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches an open orders account data
    /// </summary>
    public class GetOpenOrders : IRunnableExample
    {
        private readonly ISerumClient _serumClient;

        /// <summary>
        /// Public key for Open Orders Account.
        /// </summary>
        private const string OpenOrdersAccountAddress = "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF";

        public GetOpenOrders()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }
        
        public void Run()
        {
            OpenOrdersAccount account = _serumClient.GetOpenOrdersAccount(OpenOrdersAccountAddress);

            Console.WriteLine($"OpenOrdersAccount:: Owner: {account.Owner.Key} Market: {account.Market.Key}\n" +
                              $"BaseTotal: {account.BaseTokenTotal} BaseFree: {account.BaseTokenFree}\n" +
                              $"QuoteTotal: {account.QuoteTokenTotal} QuoteFree: {account.QuoteTokenFree}");

            foreach (Order order in account.Orders)
            {
                Console.WriteLine($"Order:: IsBid: {order.IsBid} Price: {order.Price}");
            }
        }
    }
}