using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solnet.Rpc;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class SerumClientStreamingTest
    {
        private static readonly string MainNetUrl = "https://api.mainnet-beta.solana.com/";
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private WebSocketState _wsState;
        private bool _firstNotification;
        private ManualResetEvent _event;
        private Mock<SubscriptionState> _subscriptionStateMock;
        private Action<SubscriptionState, ResponseValue<AccountInfo>> _action;

        private Mock<IStreamingRpcClient> SerumClientStreamingTestSetup<T>(
            out Action<Subscription, T, ulong> action,
            Action<T> resultCaptureCallback,
            string responseContent, string address, string network, Commitment commitment = Commitment.Finalized)
        {
            Mock<Action<Subscription, T, ulong>> actionMock = new();
            actionMock
                .Setup(_ => _(It.IsAny<Subscription>(), It.IsAny<T>(), It.IsAny<ulong>()))
                .Callback<Subscription, T, ulong>((sub, notification, slot) =>
                {
                    resultCaptureCallback(notification);
                });
            action = actionMock.Object;

            Mock<SubscriptionState> subscriptionStateMock = new(MockBehavior.Strict);
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();
            
            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();
            
            streamingRpcMock
                .Setup(s => s.SubscribeAccountInfoAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SubscriptionState, ResponseValue<AccountInfo>>>(),
                    It.Is<Commitment>(c => c == commitment)))
                .Callback<string, Action<SubscriptionState, ResponseValue<AccountInfo>>, Commitment>(
                    (pubKey, notificationAction, _) =>
                    {
                        if (pubKey != address)
                            return;

                        ResponseValue<AccountInfo> notificationContent =
                            JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(responseContent,
                                JsonSerializerOptions);
                        notificationAction(subscriptionStateMock.Object, notificationContent);
                    })
                .ReturnsAsync(() => subscriptionStateMock.Object)
                .Verifiable();
            return streamingRpcMock;
        }
        
        private Mock<IStreamingRpcClient> SerumClientMultipleNotificationsStreamingTestSetup<T>(
            out Action<Subscription, T, ulong> action, Action<T> resultCaptureCallback, string firstResponseContent,
            string secondResponseContent, string address, string network, Commitment commitment = Commitment.Finalized)
        {
            Mock<Action<Subscription, T, ulong>> actionMock = new();
            actionMock
                .Setup(_ => _(It.IsAny<Subscription>(), It.IsAny<T>(), It.IsAny<ulong>()))
                .Callback<Subscription, T, ulong>((sub, notification, slot) =>
                {
                    resultCaptureCallback(notification);
                });
            action = actionMock.Object;

            _subscriptionStateMock = new Mock<SubscriptionState>(MockBehavior.Strict);
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => _wsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();
            
            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    _wsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();
            
            streamingRpcMock
                .Setup(s => s.SubscribeAccountInfoAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SubscriptionState, ResponseValue<AccountInfo>>>(),
                    It.Is<Commitment>(c => c == commitment)))
                .Callback<string, Action<SubscriptionState, ResponseValue<AccountInfo>>, Commitment>(
                    (_, notificationAction, _) =>
                    {
                        _action = notificationAction;
                    })
                .ReturnsAsync(() => _subscriptionStateMock.Object)
                .Verifiable();
            return streamingRpcMock;
        }

        [TestInitialize]
        public void Setup()
        {
            _wsState = WebSocketState.None;
            _firstNotification = false;
        }

        [TestMethod]
        public void SubscribeOpenOrdersAccountTest()
        {
            string accountInfoNotification =
                File.ReadAllText("Resources/SubscribeOpenOrdersAccountInfoNotification.json");
            OpenOrdersAccount resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientStreamingTestSetup(
                out Action<Subscription, OpenOrdersAccount, ulong> action,
                (x) => resultNotification = x,
                accountInfoNotification,
                "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF", MainNetUrl);

            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeOpenOrdersAccount(action, "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickasD");
            Subscription sub2 = sut.SubscribeOpenOrdersAccount(action, "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF");

            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(47, resultNotification.Orders.Count);
            Assert.IsTrue(resultNotification.Flags.IsOpenOrders);
            Assert.IsTrue(resultNotification.Flags.IsInitialized);
            Assert.AreEqual(1087000000000UL, resultNotification.BaseTokenFree);
            Assert.AreEqual(25192800000000UL, resultNotification.BaseTokenTotal);
            Assert.AreEqual(557002631900UL, resultNotification.QuoteTokenFree);
            Assert.AreEqual(3687212738500UL, resultNotification.QuoteTokenTotal);
            Assert.AreEqual("CuieVDEDtLo7FypA9SbLM9saXFdb1dsshEkyErMqkRQq", resultNotification.Owner);
            Assert.AreEqual("9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT", resultNotification.Market);
            CollectionAssert.AreEqual(new byte[] { 225, 197, 72, 211, 28, 12, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                resultNotification.BidBits);
            CollectionAssert.AreEqual(
                new byte[] { 0, 0, 128, 32, 128, 2, 199, 255, 255, 255, 255, 255, 255, 255, 255, 255 },
                resultNotification.FreeSlotBits);
        }

        [TestMethod]
        public void SubscribeEventQueueTest()
        {
            string firstAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueFirstAccountInfoNotification.json");
            string secondAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueSecondAccountInfoNotification.json");
            EventQueue resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientMultipleNotificationsStreamingTestSetup(
                out Action<Subscription, EventQueue, ulong> action,
                (x) =>
                {
                    resultNotification = x;
                },
                firstAccountInfoNotification,
                secondAccountInfoNotification,
                "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht",
                "https://api.mainnet-beta.solana.com");
            
            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeEventQueue(action, "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ");
            Subscription sub2 = sut.SubscribeEventQueue(action, "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht");

            ResponseValue<AccountInfo> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(2978, resultNotification.Events.Count);
            Assert.AreEqual(1u, resultNotification.Header.Count);
            Assert.AreEqual(1489u, resultNotification.Header.Head);
            Assert.AreEqual(12419750u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
            
            notificationContent = JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(secondAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(0, resultNotification.Events.Count);
            Assert.AreEqual(1u, resultNotification.Header.Count);
            Assert.AreEqual(1489u, resultNotification.Header.Head);
            Assert.AreEqual(12419750u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
        }
        
        
        [TestMethod]
        public void SubscribeEventQueueNewEventsTest()
        {
            string firstAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueFirstAccountInfoNotificationNewEvents.json");
            string secondAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueSecondAccountInfoNotificationNewEvents.json");
            EventQueue resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientMultipleNotificationsStreamingTestSetup(
                out Action<Subscription, EventQueue, ulong> action,
                (x) =>
                {
                    resultNotification = x;
                },
                firstAccountInfoNotification,
                secondAccountInfoNotification,
                "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht",
                "https://api.mainnet-beta.solana.com");
            
            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeEventQueue(action, "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ");
            Subscription sub2 = sut.SubscribeEventQueue(action, "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht");

            ResponseValue<AccountInfo> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(2978, resultNotification.Events.Count);
            Assert.AreEqual(20u, resultNotification.Header.Count);
            Assert.AreEqual(1436u, resultNotification.Header.Head);
            Assert.AreEqual(12434606u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
            
            notificationContent = JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(secondAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(6, resultNotification.Events.Count);
            Assert.AreEqual(26u, resultNotification.Header.Count);
            Assert.AreEqual(1436u, resultNotification.Header.Head);
            Assert.AreEqual(12434612u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
        }

        [TestMethod]
        public void SubscribeOrderBookSideTest()
        {
            string accountInfoNotification =
                File.ReadAllText("Resources/SubscribeOrderBookAccountInfoNotification.json");
            OrderBookSide resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientStreamingTestSetup(
                out Action<Subscription, OrderBookSide, ulong> action,
                (x) => resultNotification = x,
                accountInfoNotification,
                "nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ",
                "https://api.mainnet-beta.solana.com");

            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeOrderBookSide(action, "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ");
            Subscription sub2 = sut.SubscribeOrderBookSide(action, "nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ");

            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.IsTrue(resultNotification.Flags.IsAsks);
            Assert.IsTrue(resultNotification.Flags.IsInitialized);
            Assert.AreEqual(33, resultNotification.GetOrders().Count);
        }

        [TestMethod]
        public void UnsubscribeOrderBookSideTest()
        {
            string accountInfoNotification =
                File.ReadAllText("Resources/SubscribeOrderBookAccountInfoNotification.json");
            OrderBookSide resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientStreamingTestSetup(
                out Action<Subscription, OrderBookSide, ulong> action,
                (x) => resultNotification = x,
                accountInfoNotification,
                "nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ",
                "https://api.mainnet-beta.solana.com");

            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeOrderBookSide(action, "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ");
            Subscription sub2 = sut.SubscribeOrderBookSide(action, "nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ");

            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.IsTrue(resultNotification.Flags.IsAsks);
            Assert.IsTrue(resultNotification.Flags.IsInitialized);
            Assert.AreEqual(33, resultNotification.GetOrders().Count);

            streamingRpcMock
                .Setup(s => s.UnsubscribeAsync(It.IsAny<SubscriptionState>()))
                .Callback<SubscriptionState>(state =>
                {
                    Assert.AreEqual(sub2.SubscriptionState, state);
                })
                .Returns(() => null)
                .Verifiable();
            
            sut.UnsubscribeOrderBookSide("nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ");

            streamingRpcMock.Verify(
                s => s.UnsubscribeAsync(
                    It.Is<SubscriptionState>(ss => ss.Channel == sub2.SubscriptionState.Channel)), Times.Once);
        }
        
        [TestMethod]
        public void UnsubscribeOpenOrdersAccountTest()
        {
            string accountInfoNotification =
                File.ReadAllText("Resources/SubscribeOpenOrdersAccountInfoNotification.json");
            OpenOrdersAccount resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientStreamingTestSetup(
                out Action<Subscription, OpenOrdersAccount, ulong> action,
                (x) => resultNotification = x,
                accountInfoNotification,
                "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF",
                "https://api.mainnet-beta.solana.com");

            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();

            Assert.AreEqual(WebSocketState.Open, sut.State);
            
            Subscription _ = sut.SubscribeOpenOrdersAccount(action, "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickasD");
            Subscription sub2 = sut.SubscribeOpenOrdersAccount(action, "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF");

            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(47, resultNotification.Orders.Count);
            Assert.IsTrue(resultNotification.Flags.IsOpenOrders);
            Assert.IsTrue(resultNotification.Flags.IsInitialized);
            Assert.AreEqual(1087000000000UL, resultNotification.BaseTokenFree);
            Assert.AreEqual(25192800000000UL, resultNotification.BaseTokenTotal);
            Assert.AreEqual(557002631900UL, resultNotification.QuoteTokenFree);
            Assert.AreEqual(3687212738500UL, resultNotification.QuoteTokenTotal);
            Assert.AreEqual("CuieVDEDtLo7FypA9SbLM9saXFdb1dsshEkyErMqkRQq", resultNotification.Owner);
            Assert.AreEqual("9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT", resultNotification.Market);
            CollectionAssert.AreEqual(new byte[] { 225, 197, 72, 211, 28, 12, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                resultNotification.BidBits);
            CollectionAssert.AreEqual(
                new byte[] { 0, 0, 128, 32, 128, 2, 199, 255, 255, 255, 255, 255, 255, 255, 255, 255 },
                resultNotification.FreeSlotBits);

            streamingRpcMock
                .Setup(s => s.UnsubscribeAsync(It.IsAny<SubscriptionState>()))
                .Callback<SubscriptionState>(state =>
                {
                    Assert.AreEqual(sub2.SubscriptionState, state);
                })
                .Returns(() => null)
                .Verifiable();
            
            sut.UnsubscribeOpenOrdersAccount("4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF");

            streamingRpcMock.Verify(
                s => s.UnsubscribeAsync(
                    It.Is<SubscriptionState>(ss => ss.Channel == sub2.SubscriptionState.Channel)), Times.Once);
        }
        
        [TestMethod]
        public void UnsubscribeEventQueueTest()
        {
            string firstAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueFirstAccountInfoNotification.json");
            string secondAccountInfoNotification =
                File.ReadAllText("Resources/SubscribeEventQueueSecondAccountInfoNotification.json");
            EventQueue resultNotification = null;
            EventQueue secondResultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientMultipleNotificationsStreamingTestSetup(
                out Action<Subscription, EventQueue, ulong> action,
                (x) =>
                {
                    resultNotification = x;
                },
                firstAccountInfoNotification,
                secondAccountInfoNotification,
                "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht",
                "https://api.mainnet-beta.solana.com");
            
            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            Subscription _ = sut.SubscribeEventQueue(action, "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ");
            Subscription sub2 = sut.SubscribeEventQueue(action, "5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht");

            ResponseValue<AccountInfo> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(sub2);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(2978, resultNotification.Events.Count);
            Assert.AreEqual(1u, resultNotification.Header.Count);
            Assert.AreEqual(1489u, resultNotification.Header.Head);
            Assert.AreEqual(12419750u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
            notificationContent =
                JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(firstAccountInfoNotification,
                    JsonSerializerOptions);
            _action(_subscriptionStateMock.Object, notificationContent);
            Assert.IsNotNull(resultNotification);
            Assert.AreEqual(0, resultNotification.Events.Count);
            Assert.AreEqual(1u, resultNotification.Header.Count);
            Assert.AreEqual(1489u, resultNotification.Header.Head);
            Assert.AreEqual(12419750u, resultNotification.Header.NextSequenceNumber);
            Assert.IsTrue(resultNotification.Header.Flags.IsInitialized);
            Assert.IsTrue(resultNotification.Header.Flags.IsEventQueue);
            
            streamingRpcMock
                .Setup(s => s.UnsubscribeAsync(It.IsAny<SubscriptionState>()))
                .Callback<SubscriptionState>(state =>
                {
                    Assert.AreEqual(sub2.SubscriptionState, state);
                })
                .Returns(() => null)
                .Verifiable();
            
            sut.UnsubscribeEventQueue("5KKsLVU6TcbVDK4BS6K1DGDxnh4Q9xjYJ8XaDCG5t8ht");

            streamingRpcMock.Verify(
                s => s.UnsubscribeAsync(
                    It.Is<SubscriptionState>(ss => ss.Channel == sub2.SubscriptionState.Channel)), Times.Once);
        }

        [TestMethod]
        public void DisconnectTest()
        {
            string accountInfoNotification =
                File.ReadAllText("Resources/SubscribeOrderBookAccountInfoNotification.json");
            OrderBookSide resultNotification = null;
            Mock<IStreamingRpcClient> streamingRpcMock = SerumClientStreamingTestSetup(
                out Action<Subscription, OrderBookSide, ulong> action,
                (x) => resultNotification = x,
                accountInfoNotification,
                "nkNzrV3ZtkWCft6ykeNGXXCbNSemqcauYKiZdf5JcKQ",
                "https://api.mainnet-beta.solana.com");

            SerumClient sut = new(Cluster.MainNet, streamingRpcClient: streamingRpcMock.Object);
            Assert.IsNotNull(sut.StreamingRpcClient);
            Assert.AreEqual(MainNetUrl, sut.NodeAddress.ToString());
            sut.ConnectAsync();
            
            Assert.AreEqual(WebSocketState.Open, sut.State);

            sut.DisconnectAsync();

            Assert.AreEqual(WebSocketState.Closed, sut.State);
        }
    }
}