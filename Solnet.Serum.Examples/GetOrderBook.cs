// unset

using Solnet.Rpc;
using System;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches the order book of a market.
    /// </summary>
    public class GetOrderBook : IRunnableExample
    {
        private ISerumClient _serumClient;

        public GetOrderBook()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
        }
    }
}