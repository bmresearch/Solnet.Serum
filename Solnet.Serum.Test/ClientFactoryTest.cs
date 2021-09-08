using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Rpc;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class ClientFactoryTest
    {
        [TestMethod]
        public void BuildSerumClient()
        {
            var c = ClientFactory.GetClient(Cluster.DevNet);

            Assert.IsInstanceOfType(c, typeof(SerumClient));
        }

        [TestMethod]
        public void BuildSerumClientFromString()
        {
            string url = "https://testnet.solana.com";
            var c = ClientFactory.GetClient(url);

            Assert.IsInstanceOfType(c, typeof(SerumClient));
        }
        
        [TestMethod]
        public void BuildSerumClientWithClients()
        {
            var rpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);
            var streamingRpcClient = Solnet.Rpc.ClientFactory.GetStreamingClient(Cluster.MainNet);
            var c = ClientFactory.GetClient(rpcClient, streamingRpcClient);

            Assert.IsInstanceOfType(c, typeof(SerumClient));
        }
    }
}