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

        private static readonly byte[] ExpectedNewOrderV3Data =
        {
            0, 10, 0, 0, 0, 1, 0, 0, 0, 16, 39, 0, 0, 0,
            0, 0, 0, 16, 39, 0, 0, 0, 0, 0, 0, 16, 39, 0,
            0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 197, 190,
            91, 7, 0, 0, 0, 0, 255, 255
        };
        
        private static readonly byte[] ExpectedNewOrderV3SerumFeeData =
        {
            0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 16, 39, 0, 0, 0, 0, 0, 0, 16, 39, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 197, 190,
            91, 7, 0, 0, 0, 0, 255, 255
        };

        private static readonly byte[] ExpectedSettleFundsData =
        {
            0, 5, 0, 0, 0
        };

        private static readonly byte[] ExpectedConsumeEventsData =
        {
            0, 3, 0, 0, 0, 10, 0
        };

        private static readonly byte[] ExpectedCancelOrderV2Data =
        {
            0, 11, 0, 0, 0, 1, 0, 0, 0, 212, 212, 174, 255,
            255, 255, 255, 255, 112, 98, 0, 0, 0, 0, 0, 0
        };
        
        private static readonly byte[] ExpectedCancelOrderByClientIdV2Data =
        {
            0, 12, 0, 0, 0, 197, 190, 91, 7, 0, 0, 0, 0
        };

        private static readonly byte[] ExpectedCloseOpenOrdersData = { 0, 14, 0, 0, 0 };
        
        private static readonly byte[] ExpectedInitOpenOrdersData = { 0, 15, 0, 0, 0 };

        private static readonly byte[] ExpectedPruneData = { 0, 16, 0, 0, 0, 255, 255 };

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
        private static Market GetMarketV3Account()
        {
            string bytes = File.ReadAllText("Resources/MarketV3AccountData.txt");
            
            return Market.Deserialize(Convert.FromBase64String(bytes));
        }

        [TestMethod]
        public void NewOrderV3Test()
        {
            var serum = SerumProgram.CreateMainNet();
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
            
            TransactionInstruction txInstruction = serum.NewOrderV3(
                market, 
                openOrders.PublicKey, 
                payer.PublicKey, 
                ownerAccount,
                order
            );
            
            Assert.AreEqual(12, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedNewOrderV3Data, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        
        [TestMethod]
        public void EncodeNewOrderV3Test()
        {
            var serum = SerumProgram.CreateMainNet();
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

            var bytes = SerumProgramData.EncodeNewOrderV3Data(order);
            
            CollectionAssert.AreEqual(ExpectedNewOrderV3Data, bytes);
        }
        
        [TestMethod]
        public void NewOrderV3SerumFeeDiscountTest()
        {
            var serum = SerumProgram.CreateMainNet();
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
            
            TransactionInstruction txInstruction = serum.NewOrderV3(
                market, 
                openOrders.PublicKey, 
                payer.PublicKey, 
                ownerAccount,
                order,
                serumAccount
            );

            Assert.AreEqual(13, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedNewOrderV3SerumFeeData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        
        [TestMethod]
        public void SettleFundsInvalidTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var quoteW = wallet.GetAccount(4);
            var referrerPcWallet = wallet.GetAccount(4);

            TransactionInstruction txInstruction = serum.SettleFunds(
                market,
                openOrders,
                ownerAccount, 
                baseW, 
                quoteW,
                referrerPcWallet);
            
            Assert.IsNull(txInstruction);
        }
        
        
        [TestMethod]
        public void SettleFundsTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketV3Account();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var quoteW = wallet.GetAccount(4);
            var referrerPcWallet = wallet.GetAccount(4);
            
            TransactionInstruction txInstruction = serum.SettleFunds(
                market,
                openOrders,
                ownerAccount, 
                baseW, 
                quoteW,
                referrerPcWallet);
            
            Assert.IsNotNull(txInstruction);
            Assert.AreEqual(10, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedSettleFundsData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void ConsumeEventsTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var baseW = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);
            var quoteW = wallet.GetAccount(4);

            TransactionInstruction txInstruction = serum.ConsumeEvents(
                new List<PublicKey>{openOrders}, 
                market,
                baseW, 
                quoteW, 10);
            
            Assert.AreEqual(5, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedConsumeEventsData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }

        [TestMethod]
        public void CancelOrderV2Test()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var openOrders = wallet.GetAccount(3);

            var orderId = new BigInteger(464876397401554404955348M);
            TransactionInstruction txInstruction = serum.CancelOrderV2(
                market,
                openOrders,
                ownerAccount,
                Side.Sell, 
                orderId);
            
            Assert.AreEqual(6, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedCancelOrderV2Data, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void CancelOrderByClientIdV2Test()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var openOrders = wallet.GetAccount(3);

            TransactionInstruction txInstruction = serum.CancelOrderByClientIdV2(
                market,
                openOrders,
                ownerAccount,
                123453125UL);
            
            Assert.AreEqual(6, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedCancelOrderByClientIdV2Data, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void InitOpenOrdersTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var openOrders = wallet.GetAccount(3);

            TransactionInstruction txInstruction = serum.InitOpenOrders(
                openOrders,
                ownerAccount,
                market.OwnAddress);
            
            Assert.AreEqual(4, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedInitOpenOrdersData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void InitOpenOrdersMarketAuthorityTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var openOrders = wallet.GetAccount(3);
            var marketAuthority = wallet.GetAccount(5);

            TransactionInstruction txInstruction = SerumProgram.InitOpenOrders(
                openOrders,
                ownerAccount,
                market.OwnAddress,
                marketAuthority);
            
            Assert.AreEqual(5, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedInitOpenOrdersData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void CloseOpenOrdersTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var openOrders = wallet.GetAccount(3);
            var refundAccount = wallet.GetAccount(3);

            TransactionInstruction txInstruction = serum.CloseOpenOrders(
                openOrders,
                ownerAccount,
                refundAccount,
                market.OwnAddress);
            
            Assert.AreEqual(4, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedCloseOpenOrdersData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
        
        [TestMethod]
        public void PruneTest()
        {
            var serum = SerumProgram.CreateMainNet();
            var market = GetMarketAccount();
            var wallet = new Wallet.Wallet(MnemonicWords);
            var ownerAccount = wallet.GetAccount(1);
            var pruneAuthority = wallet.GetAccount(2);
            var openOrders = wallet.GetAccount(3);

            TransactionInstruction txInstruction = serum.Prune(
                market,
                pruneAuthority,
                openOrders,
                ownerAccount);
            
            Assert.AreEqual(7, txInstruction.Keys.Count);
            CollectionAssert.AreEqual(ExpectedPruneData, txInstruction.Data);
            CollectionAssert.AreEqual(serum.ProgramIdKey.KeyBytes, txInstruction.ProgramId);
        }
    }
}