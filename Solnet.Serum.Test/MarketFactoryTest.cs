using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Rpc;
using Solnet.Wallet;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class MarketFactoryTest
    {
        private static readonly PublicKey _marketAddress = new("65HCcVzCVLDLEUHVfQrvE5TmHAUKgnCnbii9dojxE7wV");
        private static readonly PublicKey _account = new ("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh");
        private static readonly string TestNetUrl = "https://mainnet-beta.solana.com";
        
        [TestMethod]
        public void BuildMarketManagerTest()
        {
            var c = MarketFactory.GetMarket(_marketAddress, _account);

            Assert.IsInstanceOfType(c, typeof(MarketManager));
        }
        
        [TestMethod]
        public void BuildMarketManagerWithUrlTest()
        {
            var c = MarketFactory.GetMarket(_marketAddress, _account, url: TestNetUrl);

            Assert.IsInstanceOfType(c, typeof(MarketManager));
        }

        [TestMethod]
        public void BuildMarketManagerWithClientsTest()
        {
            var rpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);
            var streamingRpcClient = Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);
            var sc = ClientFactory.GetClient(rpcClient, streamingRpcClient);

            Assert.IsInstanceOfType(sc, typeof(SerumClient));
            
            var c = MarketFactory.GetMarket(_marketAddress, _account, serumClient: sc);

            Assert.IsInstanceOfType(c, typeof(MarketManager));
        }
    }
}