using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches all available serum markets.
    /// </summary>
    public class GetMarkets : IRunnableExample
    {
        private readonly ISerumClient _serumClient;

        public GetMarkets()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
            
            IList<MarketInfo> res = _serumClient.GetMarkets();

            foreach (MarketInfo marketInfo in res)
            {
                Console.WriteLine($"MarketInfo:: Name:\t{marketInfo.Name}\t::\t" +
                                  $"Address: {marketInfo.Address.Key}\t::\t" +
                                  $"ProgramId: {marketInfo.ProgramId.Key}\t");
            }
        }
    }
}