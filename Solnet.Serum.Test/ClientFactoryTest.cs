using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Rpc;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class ClientFactoryTest
    {
        [TestMethod]
        public void BuilRpcClient()
        {
            var c = ClientFactory.GetClient(Cluster.DevNet);

            Assert.IsInstanceOfType(c, typeof(SerumClient));
        }

        [TestMethod]
        public void BuilRpcClientFromString()
        {
            string url = "https://testnet.solana.com";
            var c = ClientFactory.GetClient(url);

            Assert.IsInstanceOfType(c, typeof(SerumClient));
        }
    }
}