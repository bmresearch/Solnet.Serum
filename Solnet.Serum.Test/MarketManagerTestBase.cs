using Moq;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solnet.Serum.Test
{
    public class MarketManagerTestBase
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        protected Queue<string> ResponseQueue;
        protected Queue<string> AddressQueue;
        protected WebSocketState WsState;
        protected Action<SubscriptionState, ResponseValue<ErrorResult>> WsResponseAction;
        protected Action<Subscription, EventQueue, ulong> EventQueueAction;
        protected Action<Subscription, OrderBookSide, ulong> OrderBookBidSideAction;
        protected Action<Subscription, OrderBookSide, ulong> OrderBookAskSideAction;
        protected Action<Subscription, OpenOrdersAccount, ulong> OpenOrdersAction;
        protected bool SignatureConfirmed;
        protected int ConfirmedSignatures;

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="network">The network address for the <c>GetAccountInfo</c> request.</param>
        /// <returns>The mocked rpc client.</returns>
        protected static Mock<IRpcClient> MockRpcClient(string network)
        {
            Mock<IRpcClient> rpcMock = new(MockBehavior.Strict);
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            return rpcMock;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="address"></param>
        /// <param name="commitment"></param>
        protected void MockGetAccountInfoAsync(Mock<IRpcClient> rpcMock, string address,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetAccountInfoAsync(
                    It.Is<string>(s1 => s1 == address),
                    It.Is<Commitment>(c => c == commitment)))
                .Callback(() => { })
                .ReturnsAsync(new RequestResult<ResponseValue<AccountInfo>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="marketAddress"></param>
        /// <param name="ownerAddress"></param>
        /// <param name="commitment"></param>
        protected void MockGetProgramAccountsAsync(Mock<IRpcClient> rpcMock, string marketAddress,
            string ownerAddress,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetProgramAccountsAsync(
                    It.Is<string>(s1 => s1 == SerumProgram.ProgramIdKey),
                    It.Is<Commitment>(c => c == commitment),
                    It.Is<int>(i => i == OpenOrdersAccount.Layout.SpanLength),
                    It.Is<List<MemCmp>>(
                        filters => filters.Find(cmp => cmp.Offset == 13).Bytes == marketAddress &&
                                   filters.Find(cmp => cmp.Offset == 45).Bytes == ownerAddress)))
                .ReturnsAsync(new RequestResult<List<AccountKeyPair>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<List<AccountKeyPair>>(ResponseQueue.Dequeue(), JsonSerializerOptions))
                {
                    WasRequestSuccessfullyHandled = true
                })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="ownerAddress"></param>
        /// <param name="tokenMint"></param>
        /// <param name="commitment"></param>
        protected void MockGetTokenAccountsByOwnerAsync(Mock<IRpcClient> rpcMock, string ownerAddress,
            string tokenMint,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetTokenAccountsByOwnerAsync(
                    It.Is<string>(s1 => s1 == ownerAddress),
                    It.Is<string>(s1 => s1 == tokenMint),
                    It.IsAny<string>(),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<List<TokenAccount>>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<List<TokenAccount>>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="commitment"></param>
        protected void MockGetRecentBlockhashAsync(Mock<IRpcClient> rpcMock,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetRecentBlockHashAsync(
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<BlockHash>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<BlockHash>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="accountSize"></param>
        /// <param name="commitment"></param>
        protected void MockGetMinimumBalanceForRentExemptionAsync(Mock<IRpcClient> rpcMock, long accountSize,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetMinimumBalanceForRentExemptionAsync(
                    It.Is<long>(c => c == accountSize),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ulong>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ulong>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="transaction"></param>
        /// <param name="commitment"></param>
        protected void MockSendTransactionAsync(Mock<IRpcClient> rpcMock, string transaction,
            Commitment commitment = Commitment.Confirmed)
        {
            rpcMock
                .Setup(s => s.SendTransactionAsync(
                    It.Is<byte[]>(b => Convert.ToBase64String(b) == transaction),
                    It.Is<bool>(skip => skip == false),
                    It.Is<Commitment>(c => c == commitment)
                ))
                .ReturnsAsync(new RequestResult<string>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<string>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="transaction"></param>
        /// <param name="commitment"></param>
        protected void MockSendTransactionAsyncWithError(Mock<IRpcClient> rpcMock, string transaction,
            Commitment commitment = Commitment.Confirmed)
        {
            rpcMock
                .Setup(s => s.SendTransactionAsync(
                    It.Is<byte[]>(b => Convert.ToBase64String(b) == transaction),
                    It.Is<bool>(skip => skip == false),
                    It.Is<Commitment>(c => c == commitment)
                ))
                .ReturnsAsync(new RequestResult<string>(
                    new HttpResponseMessage(HttpStatusCode.OK))
                {
                    WasRequestSuccessfullyHandled = false,
                    ServerErrorCode = -32002,
                    ErrorData = JsonSerializer.Deserialize<Dictionary<string, object>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)
                })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="network"></param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        protected Mock<IStreamingRpcClient> StreamingClientSignatureSubscribeSetup(
            string signature, string network, Commitment commitment = Commitment.Confirmed)
        {
            Mock<SubscriptionState> subscriptionStateMock = new(MockBehavior.Strict);
            Mock<IStreamingRpcClient> streamingRpcMock = new(MockBehavior.Strict);
            streamingRpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            streamingRpcMock
                .Setup(s => s.State)
                .Returns(() => WsState)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.ConnectAsync())
                .Callback(() =>
                {
                    WsState = WebSocketState.Open;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.DisconnectAsync())
                .Callback(() =>
                {
                    WsState = WebSocketState.Closed;
                })
                .Returns(() => null)
                .Verifiable();

            streamingRpcMock
                .Setup(s => s.SubscribeSignatureAsync(
                    It.Is<string>(s => s == signature),
                    It.IsAny<Action<SubscriptionState, ResponseValue<ErrorResult>>>(),
                    It.Is<Commitment>(c => c == commitment)))
                .Callback<string, Action<SubscriptionState, ResponseValue<ErrorResult>>, Commitment>(
                    (_, notificationAction, _) =>
                    {
                        WsResponseAction = notificationAction;
                    })
                .ReturnsAsync(() => subscriptionStateMock.Object)
                .Verifiable();
            return streamingRpcMock;
        }

        /// <summary>
        /// Mocks a serum client used to perform requests pertaining to the given market address.
        /// </summary>
        /// <param name="rpcClient">The rpc client.</param>
        /// <param name="streamingRpcClient">The streaming rpc client.</param>
        /// <param name="marketAddress">The market address.</param>
        /// <param name="marketAccountData">The market account data.</param>
        /// <returns>The mocked serum client.</returns>
        protected Mock<ISerumClient> MockSerumClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient,
            string marketAddress, string marketAccountData)
        {
            Market market = Market.Deserialize(Convert.FromBase64String(marketAccountData));
            Mock<ISerumClient> serumClientMock = new(MockBehavior.Strict);

            serumClientMock
                .Setup(x => x.RpcClient)
                .Returns(rpcClient);

            serumClientMock
                .Setup(x => x.StreamingRpcClient)
                .Returns(streamingRpcClient);

            serumClientMock
                .Setup(x => x.GetMarketAsync(
                    It.Is<string>(s => s == marketAddress),
                    It.IsAny<Commitment>()))
                .ReturnsAsync(() => market)
                .Verifiable();

            return serumClientMock;
        }

        /// <summary>
        /// Mocks a given serum client used to stream data pertaining to the associated market address.
        /// </summary>
        /// <param name="serumClientMock"></param>
        /// <param name="eventQueueAddress"></param>
        protected void MockSerumStreamingClientSubscribeEventQueueAsync(Mock<ISerumClient> serumClientMock, string eventQueueAddress)
        {
            Mock<Subscription> subscriptionMock = new (MockBehavior.Strict);
            subscriptionMock
                .Setup(x => x.Address)
                .Returns(() => new PublicKey(eventQueueAddress))
                .Verifiable();
            
            serumClientMock
                .Setup(x => x.SubscribeEventQueueAsync(
                    It.IsAny<Action<Subscription, EventQueue, ulong>>(),
                    It.Is<string>(eq => eq == eventQueueAddress),
                    It.IsAny<Commitment>()))
                .Callback<Action<Subscription, EventQueue, ulong>, string, Commitment>(
                    (notificationAction, _, _) =>
                    {
                        EventQueueAction = notificationAction;
                    })
                .ReturnsAsync(() => subscriptionMock.Object)
                .Verifiable();
        }
        
        /// <summary>
        /// Mocks a given serum client used to stream data pertaining to the associated market address.
        /// </summary>
        /// <param name="serumClientMock"></param>
        /// <param name="orderBidAddress"></param>
        protected Mock<Subscription> MockSerumStreamingClientSubscribeOrderBookBidSideAsync(Mock<ISerumClient> serumClientMock, string orderBidAddress)
        {
            Mock<Subscription> subscriptionMock = new (MockBehavior.Strict);
            
            serumClientMock
                .Setup(x => x.SubscribeOrderBookSideAsync(
                    It.IsAny<Action<Subscription, OrderBookSide, ulong>>(),
                    It.Is<string>(eq => eq == orderBidAddress),
                    It.IsAny<Commitment>()))
                .Callback<Action<Subscription, OrderBookSide, ulong>, string, Commitment>(
                    (notificationAction, _, _) =>
                    {
                        OrderBookBidSideAction = notificationAction;
                    })
                .ReturnsAsync(() => subscriptionMock.Object)
                .Verifiable();

            return subscriptionMock;
        }
        
        /// <summary>
        /// Mocks a given serum client used to stream data pertaining to the associated market address.
        /// </summary>
        /// <param name="serumClientMock"></param>
        /// <param name="orderAskAddress"></param>
        protected Mock<Subscription> MockSerumStreamingClientSubscribeOrderBookAskSideAsync(Mock<ISerumClient> serumClientMock, string orderAskAddress)
        {
            Mock<Subscription> subscriptionMock = new (MockBehavior.Strict);
            
            serumClientMock
                .Setup(x => x.SubscribeOrderBookSideAsync(
                    It.IsAny<Action<Subscription, OrderBookSide, ulong>>(),
                    It.Is<string>(eq => eq == orderAskAddress),
                    It.IsAny<Commitment>()))
                .Callback<Action<Subscription, OrderBookSide, ulong>, string, Commitment>(
                    (notificationAction, _, _) =>
                    {
                        OrderBookAskSideAction = notificationAction;
                    })
                .ReturnsAsync(() => subscriptionMock.Object)
                .Verifiable();
            return subscriptionMock;
        }
        
        /// <summary>
        /// Mocks a given serum client used to stream data pertaining to the associated market address.
        /// </summary>
        /// <param name="serumClientMock"></param>
        /// <param name="openOrdersAddress"></param>
        protected void MockSerumStreamingClientSubscribeOpenOrdersAsync(Mock<ISerumClient> serumClientMock, string openOrdersAddress)
        {
            Mock<Subscription> subscriptionMock = new (MockBehavior.Strict);
            subscriptionMock
                .Setup(x => x.Address)
                .Returns(() => new PublicKey(openOrdersAddress))
                .Verifiable();
            
            serumClientMock
                .Setup(x => x.SubscribeOpenOrdersAccountAsync(
                    It.IsAny<Action<Subscription, OpenOrdersAccount, ulong>>(),
                    It.Is<string>(eq => eq == openOrdersAddress),
                    It.IsAny<Commitment>()))
                .Callback<Action<Subscription, OpenOrdersAccount, ulong>, string, Commitment>(
                    (notificationAction, _, _) =>
                    {
                        OpenOrdersAction = notificationAction;
                    })
                .ReturnsAsync(() => subscriptionMock.Object)
                .Verifiable();
        }

        protected void EnqueueResponseFromFile(string pathToFile)
        {
            string data = File.ReadAllText(pathToFile);
            ResponseQueue.Enqueue(data);
        }

        protected string GetMarketAccountData()
        {
            return File.ReadAllText("Resources/MarketManager/SXPUSDCMarketAccountData.txt");
        }
    }
}