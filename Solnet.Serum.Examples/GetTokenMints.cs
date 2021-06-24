// unset

using Solnet.Rpc;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// An example which fetches all token mints that are available in serum markets.
    /// </summary>
    public class GetTokenMints : IRunnableExample
    {
        private readonly ISerumClient _serumClient;

        public GetTokenMints()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
        }

        public void Run()
        {
            Console.WriteLine($"Running {ToString()}");
            
            IList<TokenInfo> res = _serumClient.GetTokens();

            foreach (TokenInfo tokenInfo in res)
            {
                Console.WriteLine($"TokenInfo :: Name:\t{tokenInfo.Name}\t:: Address: {tokenInfo.Address.Key}");
            }
        }
    }
}