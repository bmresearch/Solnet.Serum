using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Numerics;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class MarketManagerTest
    {
        private static readonly PublicKey _marketAddress = new("65HCcVzCVLDLEUHVfQrvE5TmHAUKgnCnbii9dojxE7wV");
        private static readonly PublicKey _account = new ("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh");
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void MarketManagerNewOrderExceptionTest()
        {
            var c = MarketFactory.GetMarket(_marketAddress, _account);
            
            var order = new OrderBuilder()
                .SetPrice(33.3f)
                .SetQuantity(1f)
                .SetSide(Side.Sell)
                .SetOrderType(OrderType.PostOnly)
                .SetClientOrderId(123453125UL)
                .SetSelfTradeBehavior(SelfTradeBehavior.AbortTransaction)
                .Build();

            c.NewOrder(order);
        }
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void MarketManagerCancelOrderByOrderIdExceptionTest()
        {
            var c = MarketFactory.GetMarket(_marketAddress, _account);
            
            c.CancelOrder(new BigInteger(464876397401554404955348M));
        }
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void MarketManagerCancelOrderByClientIdExceptionTest()
        {
            var c = MarketFactory.GetMarket(_marketAddress, _account);
            
            c.CancelOrder(123453125UL);
        }
    }
}