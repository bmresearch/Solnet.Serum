using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class SerumProgramTest
    {
        private const string MnemonicWords =
            "route clerk disease box emerge airport loud waste attitude film army tray " +
            "forward deal onion eight catalog surface unit card window walnut wealth medal";

        private static byte[] ExpectedNewOrderV3Data =
        {
            0, 10, 0, 0, 0, 1, 0, 0, 0, 16, 39, 0, 0, 0,
            0, 0, 0, 16, 39, 0, 0, 0, 0, 0, 0, 16, 39, 0,
            0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 197, 190,
            91, 7, 0, 0, 0, 0, 255, 255
        };
        
        private static byte[] ExpectedNewOrderV3SerumFeeData =
        {
            0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 16, 39, 0, 0, 0, 0, 0, 0, 16, 39, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 197, 190,
            91, 7, 0, 0, 0, 0, 255, 255
        };

        private static byte[] ExpectedSettleFundsData =
        {
            0, 5, 0, 0, 0
        };

        private static byte[] ExpectedConsumeEventsData =
        {
            0, 3, 0, 0, 0, 10, 0
        };

        private static byte[] ExpectedCancelOrderV2Data =
        {
            0, 11, 0, 0, 0, 1, 0, 0, 0, 212, 212, 174, 255,
            255, 255, 255, 255, 112, 98, 0, 0, 0, 0, 0, 0
        };
        
        private static byte[] ExpectedCancelOrderByClientIdV2Data =
        {
            0, 12, 0, 0, 0, 197, 190, 91, 7, 0, 0, 0, 0
        };

        private static OpenOrdersAccount GetOpenOrdersAccount()
        {
            string bytes = File.ReadAllText("Resources/OpenOrdersAccountData");
            
            return OpenOrdersAccount.Deserialize(Convert.FromBase64String(bytes));
        }
        
        private static Market GetMarketAccount()
        {
            string bytes = File.ReadAllText("Resources/MarketAccountData");
            
            return Market.Deserialize(Convert.FromBase64String(bytes));
        }

        [TestMethod]
        public void NewOrderV3Test()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var payer = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);

            var order = new OrderBuilder()
                .SetPrice(33.3f)
                .SetQuantity(1f)
                .SetSide(Side.Sell)
                .SetOrderType(OrderType.PostOnly)
                .SetClientOrderId(123453125UL)
                .SetSelfTradeBehavior(SelfTradeBehavior.AbortTransaction)
                .Build();
            
            //enforce these as they'll be calculated by the MarketManager class
            order.RawPrice = 10000UL;
            order.RawQuantity = 10000UL;
            order.MaxQuoteQuantity = 10000UL;
            
            TransactionInstruction txInstruction = SerumProgram.NewOrderV3(
                ownerAccount, 
                payer.PublicKey, 
                openOrders.PublicKey, 
                market, order
            );
            
            Assert.AreEqual(12, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedNewOrderV3Data, txInstruction.Data);
            CollectionAssert.AreEqual(SerumProgram.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void NewOrderV3SerumFeeDiscountTest()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var payer = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var serumAccount = wallet.GetAccount(4);

            var order = new OrderBuilder()
                .SetPrice(33.3f)
                .SetQuantity(1f)
                .SetSide(Side.Buy)
                .SetOrderType(OrderType.Limit)
                .SetClientOrderId(123453125UL)
                .SetSelfTradeBehavior(SelfTradeBehavior.DecrementTake)
                .Build();

            //enforce these as they'll be calculated by the MarketManager class
            order.RawQuantity = 10000UL;
            order.MaxQuoteQuantity = 10000UL;
            
            TransactionInstruction txInstruction = SerumProgram.NewOrderV3(
                ownerAccount,
                payer.PublicKey,
                openOrders.PublicKey,
                market,
                order,
                serumAccount.PublicKey
            );

            Assert.AreEqual(13, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedNewOrderV3SerumFeeData, txInstruction.Data);
            CollectionAssert.AreEqual(SerumProgram.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        
        [TestMethod]
        public void SettleFundsInvalidTest()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var quoteW = wallet.GetAccount(4);

            TransactionInstruction txInstruction = SerumProgram.SettleFunds(
                market,
                openOrders,
                ownerAccount, 
                baseW, 
                quoteW);
            
            Assert.IsNull(txInstruction);
        }
        
        
        [TestMethod]
        public void SettleFundsTest()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            
            // TODO: go get actual data so createPDA doesn't blow up in your face and you don't leak secret serum alpha on stream
        }
        
        [TestMethod]
        public void ConsumeEventsTest()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var quoteW = wallet.GetAccount(4);

            TransactionInstruction txInstruction = SerumProgram.ConsumeEvents(
                ownerAccount,
                new List<PublicKey>() {openOrders}, 
                market, 
                baseW, 
                quoteW,
                10);
            
            Assert.AreEqual(6, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedConsumeEventsData, txInstruction.Data);
            CollectionAssert.AreEqual(SerumProgram.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }

        [TestMethod]
        public void CancelOrderV2Test()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);

            /* TODO: re think shared structure between OpenOrdersAccount and Orders to submit*/
            var orderId = new BigInteger(464876397401554404955348M);
            TransactionInstruction txInstruction = SerumProgram.CancelOrderV2(
                market,
                openOrders,
                ownerAccount,
                Side.Sell, 
                orderId);
            
            Assert.AreEqual(6, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedCancelOrderV2Data, txInstruction.Data);
            CollectionAssert.AreEqual(SerumProgram.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void CancelOrderByClientIdV2Test()
        {
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);

            TransactionInstruction txInstruction = SerumProgram.CancelOrderByClientIdV2(
                market,
                openOrders,
                ownerAccount,
                123453125UL);
            
            Assert.AreEqual(6, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedCancelOrderByClientIdV2Data, txInstruction.Data);
            CollectionAssert.AreEqual(SerumProgram.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
    }
}