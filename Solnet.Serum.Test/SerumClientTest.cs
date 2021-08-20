using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class SerumClientTest
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
        };

        private const string GitHubUrl = "https://raw.githubusercontent.com/project-serum/serum-ts/master/packages/serum/src/";
        private static readonly Uri GitHubUri = new (GitHubUrl);

        /// <summary>
        /// Finish the test by asserting the http request went as expected.
        /// </summary>
        /// <param name="expectedUri">The request uri.</param>
        private static void FinishHttpClientTest(Mock<HttpMessageHandler> mockHandler, Uri expectedUri)
        {
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        /// <summary>
        /// Setup the test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        private static Mock<HttpMessageHandler> SetupHttpClientTest(string responseContent)
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        message => message.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                })
                .Verifiable();
            return messageHandlerMock;
        }
        
        /// <summary>
        /// Setup the test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        private static Mock<HttpMessageHandler> SetupHttpClientUnsuccessfulTest()
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(
                        message => message.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                })
                .Verifiable();
            return messageHandlerMock;
        }

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <param name="address">The address parameter for <c>GetAccountInfo</c>.</param>
        /// <param name="commitment">The commitment parameter for the <c>GetAccountInfo</c>.</param>
        /// <param name="network">The network address for the <c>GetAccountInfo</c> request.</param>
        private static Mock<IRpcClient> SetupGetAccountInfo(string responseContent, string address, string network,
            Commitment commitment = Commitment.Finalized)
        {
            var rpcMock = new Mock<IRpcClient>(MockBehavior.Strict) { };
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            rpcMock
                .Setup(s => s.GetAccountInfoAsync(
                        It.Is<string>(s1 => s1 == address),
                        It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<AccountInfo>>(
                        new HttpResponseMessage(HttpStatusCode.OK),
                        JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(responseContent, JsonSerializerOptions)) 
                    {
                        WasRequestSuccessfullyHandled = true
                    })
                .Verifiable();
            return rpcMock;
        }

        [TestMethod]
        public void PublicKeyConverterTest()
        {
            string testData = File.ReadAllText("Resources/PublicKeyConverterTestData.json");
            string testNamingPolicyData = File.ReadAllText("Resources/PublicKeyConverterNamingTestData.json");
            var tokenInfo = new TokenInfo()
            {
                Name = "BTC",
                Address = new PublicKey("9n4nbM75f5Ui33ZbPYXn59EwSgE8CGsHtAeTH5YFeJ9E")
            };

            string serialized = JsonSerializer.Serialize(tokenInfo);
            
            Assert.AreEqual(testData, serialized);
            
            string serializedNamingConvention = JsonSerializer.Serialize(
                tokenInfo, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            Assert.AreEqual(testNamingPolicyData, serializedNamingConvention);
        }

        [TestMethod]
        public void GetTokensTest()
        {
            string responseData = File.ReadAllText("Resources/GetTokenMintsResponse.json");
            Mock<HttpMessageHandler> messageHandlerMock = SetupHttpClientTest(responseData);

            HttpClient httpClient = new(messageHandlerMock.Object)
            {
                BaseAddress = new Uri(GitHubUrl + "token-mints.json"),
            };

            SerumClient sut = new(Cluster.MainNet, null, httpClient);
            Assert.IsNotNull(sut.RpcClient);

            IList<TokenInfo> result = sut.GetTokens();

            Assert.IsNotNull(result);
            Assert.AreEqual(18, result.Count);
            Assert.AreEqual(new PublicKey("9n4nbM75f5Ui33ZbPYXn59EwSgE8CGsHtAeTH5YFeJ9E"), result[0].Address.Key);
            Assert.AreEqual("BTC", result[0].Name);

            FinishHttpClientTest(messageHandlerMock, new Uri(GitHubUrl + "token-mints.json"));
        }
        
        [TestMethod]
        public void GetTokensUnsuccessfulTest()
        {
            Mock<HttpMessageHandler> messageHandlerMock = SetupHttpClientUnsuccessfulTest();

            HttpClient httpClient = new(messageHandlerMock.Object) {BaseAddress = GitHubUri,};

            SerumClient sut = new(Cluster.MainNet, null, httpClient);
            Assert.IsNotNull(sut.RpcClient);

            IList<TokenInfo> result = sut.GetTokens();

            Assert.IsNull(result);
            FinishHttpClientTest(messageHandlerMock, new Uri(GitHubUrl + "token-mints.json"));
        }

        [TestMethod]
        public void GetMarketsTest()
        {
            string responseData = File.ReadAllText("Resources/GetMarketsResponse.json");
            Mock<HttpMessageHandler> messageHandlerMock = SetupHttpClientTest(responseData);

            HttpClient httpClient = new(messageHandlerMock.Object) {BaseAddress = GitHubUri,};

            SerumClient sut = new(Cluster.MainNet, null, httpClient);
            Assert.IsNotNull(sut.RpcClient);

            IList<MarketInfo> result = sut.GetMarkets();

            Assert.IsNotNull(result);
            Assert.AreEqual(33, result.Count);
            Assert.AreEqual(new PublicKey("EmCzMQfXMgNHcnRoFwAdPe1i2SuiSzMj1mx6wu3KN2uA"), result[0].Address.Key);
            Assert.AreEqual("ALEPH/USDT", result[0].Name);
            Assert.AreEqual(true, result[0].Deprecated);
            Assert.AreEqual("4ckmDgGdxQoPDLUkDT3vHgSAkzA3QRdNq5ywwY4sUSJn", result[0].ProgramId.Key);
            Assert.AreEqual(false, result[^1].Deprecated);

            FinishHttpClientTest(messageHandlerMock, new Uri(GitHubUrl + "markets.json"));
        }
        
        [TestMethod]
        public void GetMarketsUnsuccessfulTest()
        {
            Mock<HttpMessageHandler> messageHandlerMock = SetupHttpClientUnsuccessfulTest();

            HttpClient httpClient = new(messageHandlerMock.Object) {BaseAddress = GitHubUri,};

            SerumClient sut = new(Cluster.MainNet, null, httpClient);
            Assert.IsNotNull(sut.RpcClient);

            IList<MarketInfo> result = sut.GetMarkets();

            Assert.IsNull(result);
            FinishHttpClientTest(messageHandlerMock, new Uri(GitHubUrl + "markets.json"));
        }

        [TestMethod]
        public void GetMarketTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetMarketAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);
            Assert.IsNotNull(sut.RpcClient);

            Market result = sut.GetMarket("13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR", Commitment.Confirmed);

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Flags.IsInitialized);
            Assert.AreEqual(true, result.Flags.IsMarket);
            Assert.AreEqual(false, result.Flags.IsOpenOrders);
            Assert.AreEqual(false, result.Flags.IsAsks);
            Assert.AreEqual(false, result.Flags.IsBids);
            Assert.AreEqual(false, result.Flags.IsRequestQueue);
            Assert.AreEqual(false, result.Flags.IsEventQueue);
            Assert.AreEqual("13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR", result.OwnAddress.Key);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", result.BaseMint.Key);
            Assert.AreEqual("2ThEZPEPvwnfeS4x1a7LKdDqX6j1VjPw298HqcoQqEtp", result.BaseVault.Key);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", result.QuoteMint.Key);
            Assert.AreEqual("fBQG6bx8SFgAVcS3vtr3rJDFKHnVcKw4CpbL3o7obBu", result.QuoteVault.Key);
            Assert.AreEqual("ATkXKqzi13onqU4cxkkdK18XGWWDA7a9CJbcsaYfZnCw", result.Asks.Key);
            Assert.AreEqual("GjASMbTXya1xrqt6NSDqe1C69ogSM4WNi3We7XNYD1PD", result.Bids.Key);
            Assert.AreEqual("2ZAqGPsjBRRPvXtVe3YTiqLvwaeZYSTRMv4NzvEFjfie", result.RequestQueue.Key);
            Assert.AreEqual("3bdmcKUenYeRaEXnJEC2skJhU5KKY7JqK3aPMmxtEqTd", result.EventQueue.Key);
            Assert.AreEqual(1700000UL, result.BaseDepositsTotal);
            Assert.AreEqual(0UL, result.BaseFeesAccrued);
            Assert.AreEqual(100000UL, result.BaseLotSize);
            Assert.AreEqual(0UL, result.FeeRateBasis);
            Assert.AreEqual(1007UL, result.QuoteDepositsTotal);
            Assert.AreEqual(100UL, result.QuoteDustThreshold);
            Assert.AreEqual(3328336UL, result.QuoteFeesAccrued);
            Assert.AreEqual(100UL, result.QuoteLotSize);
            Assert.AreEqual(0UL, result.ReferrerRebateAccrued);
            Assert.AreEqual(0UL, result.VaultSignerNonce);
        }
        
        [TestMethod]
        public void GetEventQueueTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetEventQueueAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "3bdmcKUenYeRaEXnJEC2skJhU5KKY7JqK3aPMmxtEqTd",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);
            Assert.IsNotNull(sut.RpcClient);

            EventQueue result = sut.GetEventQueue("3bdmcKUenYeRaEXnJEC2skJhU5KKY7JqK3aPMmxtEqTd", Commitment.Confirmed);

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Header.Flags.IsEventQueue);
            Assert.AreEqual(true, result.Header.Flags.IsInitialized);
            Assert.AreEqual(false, result.Header.Flags.IsOpenOrders);
            Assert.AreEqual(false, result.Header.Flags.IsAsks);
            Assert.AreEqual(false, result.Header.Flags.IsBids);
            Assert.AreEqual(false, result.Header.Flags.IsRequestQueue);
            Assert.AreEqual(false, result.Header.Flags.IsMarket);
            Assert.AreEqual(2021U, result.Header.Head);
            Assert.AreEqual(0U, result.Header.Count);
            Assert.AreEqual(4112696U, result.Header.NextSequenceNumber);
            Assert.AreEqual(11915, result.Events.Count);
            Assert.AreEqual(false, result.Events[0].Flags.IsBid);
            Assert.AreEqual(false, result.Events[0].Flags.IsFill);
            Assert.AreEqual(false, result.Events[0].Flags.IsMaker);
            Assert.AreEqual(true, result.Events[0].Flags.IsOut);
        }
        
        [TestMethod]
        public void GetOpenOrdersAccountTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetOpenOrdersAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);
            Assert.IsNotNull(sut.RpcClient);

            OpenOrdersAccount result = sut.GetOpenOrdersAccount("4beBRAZSVcCm7jD7yAmizqqVyi39gVrKNeEPskickzSF", Commitment.Confirmed);

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Flags.IsInitialized);
            Assert.AreEqual(true, result.Flags.IsOpenOrders);
            Assert.AreEqual(false, result.Flags.IsEventQueue);
            Assert.AreEqual(false, result.Flags.IsAsks);
            Assert.AreEqual(false, result.Flags.IsBids);
            Assert.AreEqual(false, result.Flags.IsRequestQueue);
            Assert.AreEqual(false, result.Flags.IsMarket);
            Assert.AreEqual(42945100000000UL, result.BaseTokenTotal);
            Assert.AreEqual(2435200000000UL, result.BaseTokenFree);
            Assert.AreEqual(2337285233400UL, result.QuoteTokenTotal);
            Assert.AreEqual(146709860000UL, result.QuoteTokenFree);
            Assert.AreEqual("CuieVDEDtLo7FypA9SbLM9saXFdb1dsshEkyErMqkRQq", result.Owner.Key);
            Assert.AreEqual("9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT", result.Market.Key);
            Assert.AreEqual(30, result.Orders.Count);
        }

        [TestMethod]
        public void GetBidOrderBookTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetBidOrderBookAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);
            Assert.IsNotNull(sut.RpcClient);

            OrderBookSide result = sut.GetOrderBookSide("14ivtgssEBoBjuZJtSAPKYgpUK7DmnSwuPMqJoVTSgKJ", Commitment.Confirmed);
            Assert.IsNotNull(result);
            
            List<OpenOrder> orders = result.GetOrders();
            Assert.AreEqual(true, result.Flags.IsInitialized);
            Assert.AreEqual(true, result.Flags.IsBids);
            Assert.AreEqual(false, result.Flags.IsOpenOrders);
            Assert.AreEqual(false, result.Flags.IsEventQueue);
            Assert.AreEqual(false, result.Flags.IsAsks);
            Assert.AreEqual(false, result.Flags.IsRequestQueue);
            Assert.AreEqual(false, result.Flags.IsMarket);
            Assert.AreEqual(719, result.Slab.Nodes.Count);
            Assert.AreEqual(360, orders.Count);
            Assert.AreEqual("9XsdpLvg5Sy2gxvApcK3vt5D3g1qS9tzDg8RCPzRE2FM", orders[0].Owner.Key);
            Assert.AreEqual(0UL, orders[0].ClientOrderId);
            Assert.AreEqual(16000UL, orders[0].RawPrice);
            Assert.AreEqual(1UL, orders[0].RawQuantity);
        }
        
        [TestMethod]
        public void GetAskOrderBookTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetAskOrderBookAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "CEQdAFKdycHugujQg9k2wbmxjcpdYZyVLfV9WerTnafJ",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);
            Assert.IsNotNull(sut.RpcClient);

            OrderBookSide result = sut.GetOrderBookSide("CEQdAFKdycHugujQg9k2wbmxjcpdYZyVLfV9WerTnafJ", Commitment.Confirmed);
            Assert.IsNotNull(result);
            
            List<OpenOrder> orders = result.GetOrders();
            Assert.AreEqual(true, result.Flags.IsInitialized);
            Assert.AreEqual(true, result.Flags.IsAsks);
            Assert.AreEqual(false, result.Flags.IsBids);
            Assert.AreEqual(false, result.Flags.IsOpenOrders);
            Assert.AreEqual(false, result.Flags.IsEventQueue);
            Assert.AreEqual(false, result.Flags.IsRequestQueue);
            Assert.AreEqual(false, result.Flags.IsMarket);
            Assert.AreEqual(481, result.Slab.Nodes.Count);
            Assert.AreEqual(241, orders.Count);
            Assert.AreEqual("7aSfSYun38WU5iYgKuH6o4UY6sTZAKfz5KbZPpSgFvWX", orders[0].Owner.Key);
            Assert.AreEqual(0UL, orders[0].ClientOrderId);
            Assert.AreEqual(43780UL, orders[0].RawPrice);
            Assert.AreEqual(500UL, orders[0].RawQuantity);
        }
    }
}