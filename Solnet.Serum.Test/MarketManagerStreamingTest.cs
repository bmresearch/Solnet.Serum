using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solnet.Rpc;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class MarketManagerStreamingTest : MarketManagerTestBase
    {
        private static readonly string ClusterUrl = "https://mainnet-beta.solana.com";
        private static readonly PublicKey SXPUSDCAddress = new("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
        private static readonly PublicKey SBRUSDCAddress = new("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs");
        private static readonly PublicKey Account = new("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh");
        
        
        [TestInitialize]
        public void Setup()
        {
            ResponseQueue = new Queue<string>();
            AddressQueue = new Queue<string>();
            SignatureConfirmed = false;
            ConfirmedSignatures = 0;
        }


        [TestMethod]
        public void MarketManagerSubscribeTradesTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/SaberMarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/SaberBaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/SaberQuoteTokenGetAccountInfo.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, SBRUSDCAddress);
            MockGetAccountInfoAsync(rpcMock, "Saber2gLauYim4Mvftnrasomsv6NvAuncvMEZwcLpD1");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                SBRUSDCAddress, GetMarketAccountData("Resources/MarketManager/SBRUSDCMarketAccountData.txt"));
            Mock<Subscription> eqSub = MockSerumStreamingClientSubscribeEventQueueAsync(
                serumMock, "EUre4VPaLh7B95qG3JPS3atquJ5hjbwtX7XFcTtVNkc7");

            MarketManager sut = new(SBRUSDCAddress, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs", sut.Market.OwnAddress);
            Assert.AreEqual("Saber2gLauYim4Mvftnrasomsv6NvAuncvMEZwcLpD1", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.IsNull(sut.OpenOrdersAccount);

            IList<TradeEvent> tradeEvents = null;

            sut.SubscribeTrades((trades, slot) =>
            {
                tradeEvents = trades;
            });

            Assert.IsNull(tradeEvents);

            string firstNotification = File.ReadAllText("Resources/MarketManager/SubscribeEventQueueFirstNotification.txt");
            EventQueue firstEq = EventQueue.Deserialize(Convert.FromBase64String(firstNotification));
            EventQueueAction(eqSub.Object, firstEq, 14532466);

            Assert.IsNotNull(tradeEvents);
            Assert.AreEqual(534, tradeEvents.Count);

            string secNotification = File.ReadAllText("Resources/MarketManager/SubscribeEventQueueSecondNotification.txt");
            EventQueueAction(eqSub.Object, EventQueue.DeserializeSince(Convert.FromBase64String(secNotification), firstEq.Header.NextSequenceNumber), 14532466);

            Assert.IsNotNull(tradeEvents);
            Assert.AreEqual(0, tradeEvents.Count);
        }

        [TestMethod]
        public void MarketManagerSubscribeOpenOrdersTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/SaberMarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/SaberBaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/SaberQuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/SBRUSDCBaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/SBRUSDCQuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/SBRUSDCOpenOrdersGetProgramAccounts.json");


            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, SBRUSDCAddress);
            MockGetAccountInfoAsync(rpcMock, "Saber2gLauYim4Mvftnrasomsv6NvAuncvMEZwcLpD1");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "Saber2gLauYim4Mvftnrasomsv6NvAuncvMEZwcLpD1");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SBRUSDCAddress, Account);
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                SBRUSDCAddress, GetMarketAccountData("Resources/MarketManager/SBRUSDCMarketAccountData.txt"));
            Mock<Subscription> sub = MockSerumStreamingClientSubscribeOpenOrdersAsync(serumMock, "ATYS61FqqG1mR8Kr2tCPYKqgC3gikqnQmxi4U19TRFEp");

            MarketManager sut = new(SBRUSDCAddress, Account, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs", sut.Market.OwnAddress);
            Assert.AreEqual("Saber2gLauYim4Mvftnrasomsv6NvAuncvMEZwcLpD1", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);

            IList<OpenOrder> openOrders = null;

            sut.SubscribeOpenOrders((orders, slot) =>
            {
                openOrders = orders;
            });

            Assert.IsNull(openOrders);

            string firstNotification = File.ReadAllText("Resources/MarketManager/SubscribeOpenOrdersNotification.txt");
            OpenOrdersAction(sub.Object, OpenOrdersAccount.Deserialize(Convert.FromBase64String(firstNotification)), 14532466);

            Assert.IsNotNull(openOrders);
            Assert.AreEqual(1, openOrders.Count);
        }

        [TestMethod]
        public void MarketManagerSubscribeOrderBookTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));
            Mock<Subscription> bidSub = MockSerumStreamingClientSubscribeOrderBookBidSideAsync(
                serumMock, "8MyQkxux1NnpNqpBbPeiQHYeDbZvdvs7CHmGpciSMWvs");
            Mock<Subscription> askSub = MockSerumStreamingClientSubscribeOrderBookAskSideAsync(
                serumMock, "HjB8zKe9xezDrgqXCSjCb5F7dMC9WMwtZoT7yKYEhZYV");

            MarketManager sut = new(SXPUSDCAddress, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.IsNull(sut.OpenOrdersAccount);

            OrderBook ob = null;

            sut.SubscribeOrderBook((book, _) =>
            {
                ob = book;
            });
            
            string firstNotification = File.ReadAllText("Resources/MarketManager/SubscribeOrderBookSideBidsNotification.txt");
            OrderBookBidSideAction(bidSub.Object, OrderBookSide.Deserialize(Convert.FromBase64String(firstNotification)), 14532466);
            
            Assert.IsNotNull(ob);
            Assert.IsNotNull(ob.Bids);
            Assert.AreEqual(22, ob.GetBids().Count);
            Assert.IsNull(ob.Asks);
            
            string secNotification = File.ReadAllText("Resources/MarketManager/SubscribeOrderBookSideAsksNotification.txt");
            OrderBookAskSideAction(askSub.Object, OrderBookSide.Deserialize(Convert.FromBase64String(secNotification)), 14532466);
            
            Assert.IsNotNull(ob);
            Assert.IsNotNull(ob.Bids);
            Assert.IsNotNull(ob.Asks);
            Assert.AreEqual(19, ob.GetAsks().Count);
        }
    }
}