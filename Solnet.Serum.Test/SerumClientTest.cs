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
        protected const string MainNetUrl = "https://mainnet.solana.com";
        protected static readonly Uri MainNetUri = new Uri(MainNetUrl);

        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
        };

        private const string GitHubUrl = "https://raw.githubusercontent.com/project-serum/serum-js/master/src/";
        private static readonly Uri GitHubUri = new Uri(GitHubUrl);

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
                    StatusCode = HttpStatusCode.OK, Content = new StringContent(responseContent),
                })
                .Verifiable();
            return messageHandlerMock;
        }

        /// <summary>
        /// Setup the test with the request and response data.
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <param name="address">The address parameter for <c>GetAccountInfo</c>.</param>
        /// <param name="commitment">The address parameter for <c>GetAccountInfo</c>.</param>
        private static Mock<IRpcClient> SetupGetAccountInfo(string responseContent, string address, string network,
            Commitment commitment = Commitment.Finalized)
        {
            var rpcMock = new Mock<IRpcClient>(MockBehavior.Strict) { };
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            rpcMock
                .Setup(
                    s => s.GetAccountInfoAsync(
                        It.Is<string>(s1 => s1 == address),
                        It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(
                    new RequestResult<ResponseValue<AccountInfo>>(
                        new HttpResponseMessage(HttpStatusCode.OK),
                        JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(responseContent, JsonSerializerOptions))
                    {
                        WasRequestSuccessfullyHandled = true
                    }
                )
                .Verifiable();
            return rpcMock;
        }

        [TestMethod]
        public void PublicKeyConverterTest()
        {
            string testData = File.ReadAllText("Resources/PublicKeyConverterTestData.json");
            string testNamingPolicyData = File.ReadAllText("Resources/PublicKeyConverterNamingTestData.json");
            var tokenInfo = new TokenMintInfo()
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

            IList<TokenMintInfo> result = sut.GetTokens();

            Assert.IsNotNull(result);
            Assert.AreEqual(18, result.Count);
            Assert.AreEqual(new PublicKey("9n4nbM75f5Ui33ZbPYXn59EwSgE8CGsHtAeTH5YFeJ9E"), result[0].Address.Key);
            Assert.AreEqual("BTC", result[0].Name);

            FinishHttpClientTest(messageHandlerMock, new Uri(GitHubUrl + "token-mints.json"));
        }

        [TestMethod]
        public void GetMarketsTest()
        {
            string responseData = File.ReadAllText("Resources/GetMarketsResponse.json");
            Mock<HttpMessageHandler> messageHandlerMock = SetupHttpClientTest(responseData);

            HttpClient httpClient = new(messageHandlerMock.Object) {BaseAddress = GitHubUri,};

            SerumClient sut = new(Cluster.MainNet, null, httpClient);

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
        public void GetMarketTest()
        {
            string getAccountInfoResponseData = File.ReadAllText("Resources/GetMarketAccountInfoResponse.json");
            Mock<IRpcClient> rpcMock = SetupGetAccountInfo(
                getAccountInfoResponseData,
                "13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR",
                "https://api.mainnet-beta.solana.com",
                Commitment.Confirmed);

            SerumClient sut = new(Cluster.MainNet, null, rpcClient: rpcMock.Object);

            Market result = sut.GetMarket("13vjJ8pxDMmzen26bQ5UrouX8dkXYPW1p3VLVDjxXrKR", Commitment.Confirmed);

            Assert.IsNotNull(result);
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
            Assert.AreEqual(17329972132752316649UL, result.VaultSignerNonce);
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

            EventQueue result = sut.GetEventQueue("3bdmcKUenYeRaEXnJEC2skJhU5KKY7JqK3aPMmxtEqTd", Commitment.Confirmed);

            Assert.IsNotNull(result);
            Assert.AreEqual(2021U,    result.Header.Head);
            Assert.AreEqual(0U,       result.Header.Count);
            Assert.AreEqual(4112696U, result.Header.NextSeqNum);
            Assert.AreEqual(11915,    result.Events.Count);
        }
    }
}