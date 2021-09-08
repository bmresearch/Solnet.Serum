using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solnet.Rpc;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class MarketManagerTest : MarketManagerTestBase
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private static readonly string ClusterUrl = "https://mainnet-beta.solana.com";
        private static readonly PublicKey MNGOUSDCAddress = new("65HCcVzCVLDLEUHVfQrvE5TmHAUKgnCnbii9dojxE7wV");
        private static readonly PublicKey SXPUSDCAddress = new("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
        private static readonly PublicKey Account = new("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh");

        private static readonly string NewOrderTransaction =
            "AS3pXaB2hrRjgk4AJHUrfaUl5dtIcxyT2nRdz5qB5w93SpcOdrDmx0ljL2wxxx6N0iBajR15UPn4ZHSSom\u002BkPw0BAAQPCnPnH" +
            "\u002B3SE2XtCu6E/OTisgZH/TTV7L2hFfAj0mM/h3gxkJiMEg9pt0UY4VGDhoK5gi\u002BlfT9SnYfSCwUM68ArGrDRLePOtF/pNw" +
            "BZrQrHhveA1F1zpqpkyj0EsJP1LEmzmvfcgz\u002BpCbr4QWO/dYKX36cgJPn2JslaujbRVLm5RD0COaxQQvf9r8PyaRmnlkSN6BQ" +
            "s0u7SDcCOz4CLebCdaW1hNQzbVLkSzhc9qtst1Jfq26d9dQ/22HU48BIZu1jY\u002BIjY\u002Blajf2iAo6Q1jmT81\u002BvRa3" +
            "/0oQMatxSavG6FGJINMeC0E2nzi\u002BfIC\u002BuRXE1EEGmNPrRPTkH6xVbYEuZKcSgViGOph6mtTc/i5/V7k/Malr7/YxBezc" +
            "jPsCTq/LNI8RqttgK/juTyfqv8uSiqbkGWGETaSk2Kb5xsFcNlQQWSbGlLqfiZ41yfAAH/BysfePaWsZCjnts5xkiSRTT14Abd9uHX" +
            "ZaGT2cvhRs7reawctIXtX1s3kTqM9YV\u002B/wCpBqfVFxksXFEhjMlMPUrxf1ja7gibof1E49vZigAAAACFDy1uAqR6\u002BCTQ" +
            "mradxC1wyyjL\u002BiSft\u002B5XudJWwSdi76mB2IJG4xf1cMg/zt4KpmqQOfXhSIM7gXNbEcrXg9U9mPJVJghxg/F6JhwEkgHV" +
            "8I/a29xikm/bwi4yGjV4wdICDQwBAgMEBQYHAAgJCwwzAAoAAAAAAAAArA0AAAAAAAAKAAAAAAAAAOBnNQAAAAAAAAAAAAAAAABAQg" +
            "8AAAAAAP//DQkBAgAICQoHDgsFAAUAAAA=";

        private static readonly string CancelOrderByClientIdTransaction =
            "AU8zga/hBUEAiUwB1D5D\u002Bu2aQ/A7Liq5HVbKb9RIAXoUzlQc081GgAujooggPFSZAi/XtXoBAPhVtZ7IgYAyIQIBAAMNCnPnH" +
            "\u002B3SE2XtCu6E/OTisgZH/TTV7L2hFfAj0mM/h3gxkJiMEg9pt0UY4VGDhoK5gi\u002BlfT9SnYfSCwUM68ArGm1hNQzbVLkSz" +
            "hc9qtst1Jfq26d9dQ/22HU48BIZu1jY\u002BIjY\u002Blajf2iAo6Q1jmT81\u002BvRa3/0oQMatxSavG6FGJKw0S3jzrRf6TcA" +
            "Wa0Kx4b3gNRdc6aqZMo9BLCT9SxJswI5rFBC9/2vw/JpGaeWRI3oFCzS7tINwI7PgIt5sJ1pKBWIY6mHqa1Nz\u002BLn9XuT8xqWv" +
            "v9jEF7NyM\u002BwJOr8s0jxGq22Ar\u002BO5PJ\u002Bq/y5KKpuQZYYRNpKTYpvnGwVw2VBBZJsaUup\u002BJnjXJ8AAf8HKx9" +
            "49paxkKOe2znGSJJFNPXgDTHgtBNp84vnyAvrkVxNRBBpjT60T05B\u002BsVW2BLmSnGFDy1uAqR6\u002BCTQmradxC1wyyjL\u002B" +
            "iSft\u002B5XudJWwSdi76mB2IJG4xf1cMg/zt4KpmqQOfXhSIM7gXNbEcrXg9U9Bt324ddloZPZy\u002BFGzut5rBy0he1fWzeRO" +
            "oz1hX7/AKmoZSUZjZd9tFRjhrcncGWZDjnxmp/A/Efa/ewdn2PL7wIKBgECAwQABQ0ADAAAAEBCDwAAAAAACgkBBAAGBwgJCwwFAAUAAAA=";

        private static readonly string CancelAllOrdersTransaction =
            "AScIPqPObSlNq\u002BH3OSzuyDtN0nmLGL2vePsBEeSWGh25ZLZ1N9V8DpyUtM9vP4KRFTVRNFmTIAWVcOHo/4jahAkBAAMNCnPnH" +
            "\u002B3SE2XtCu6E/OTisgZH/TTV7L2hFfAj0mM/h3gxkJiMEg9pt0UY4VGDhoK5gi\u002BlfT9SnYfSCwUM68ArGm1hNQzbVLkSz" +
            "hc9qtst1Jfq26d9dQ/22HU48BIZu1jY\u002BIjY\u002Blajf2iAo6Q1jmT81\u002BvRa3/0oQMatxSavG6FGJKw0S3jzrRf6Tc" +
            "AWa0Kx4b3gNRdc6aqZMo9BLCT9SxJswI5rFBC9/2vw/JpGaeWRI3oFCzS7tINwI7PgIt5sJ1pKBWIY6mHqa1Nz\u002BLn9XuT8xqW" +
            "vv9jEF7NyM\u002BwJOr8s0jxGq22Ar\u002BO5PJ\u002Bq/y5KKpuQZYYRNpKTYpvnGwVw2VBBZJsaUup\u002BJnjXJ8AAf8HK" +
            "x949paxkKOe2znGSJJFNPXgDTHgtBNp84vnyAvrkVxNRBBpjT60T05B\u002BsVW2BLmSnGFDy1uAqR6\u002BCTQmradxC1wyyjL" +
            "\u002BiSft\u002B5XudJWwSdi76mB2IJG4xf1cMg/zt4KpmqQOfXhSIM7gXNbEcrXg9U9Bt324ddloZPZy\u002BFGzut5rBy0he" +
            "1fWzeROoz1hX7/AKn7mWRbr0yLmaJddT3EOtAzE1r72F4ZIickCadAi7HUiQYKBgECAwQABRkACwAAAAAAAABkU97//////6wNAAA" +
            "AAAAACgYBAgMEAAUZAAsAAAAAAAAAYFPe//////\u002BsDQAAAAAAAAoGAQIDBAAFGQALAAAAAAAAAFxT3v//////rA0AAAAAAAA" +
            "KBgECAwQABRkACwAAAAAAAABXU97//////6wNAAAAAAAACgYBAgMEAAUZAAsAAAAAAAAAU1Pe//////\u002BsDQAAAAAAAAoJAQQ" +
            "ABgcICQsMBQAFAAAA";

        private static readonly string NewOrderInsufficientBalanceTransaction =
            "AZki7J4SWxvFKSpjLjYXDHnYJ2noQV6HHANcgz4SNCddassFwa5ul2P1qJl/o1MjOfKI3erVwnEA\u002Bxka1W8aKAUBAAQPCnPnH" +
            "\u002B3SE2XtCu6E/OTisgZH/TTV7L2hFfAj0mM/h3gxkJiMEg9pt0UY4VGDhoK5gi\u002BlfT9SnYfSCwUM68ArGrDRLePOtF/pN" +
            "wBZrQrHhveA1F1zpqpkyj0EsJP1LEmzmvfcgz\u002BpCbr4QWO/dYKX36cgJPn2JslaujbRVLm5RD0COaxQQvf9r8PyaRmnlkSN6B" +
            "Qs0u7SDcCOz4CLebCdaW1hNQzbVLkSzhc9qtst1Jfq26d9dQ/22HU48BIZu1jY\u002BIjY\u002Blajf2iAo6Q1jmT81\u002BvRa" +
            "3/0oQMatxSavG6FGJINMeC0E2nzi\u002BfIC\u002BuRXE1EEGmNPrRPTkH6xVbYEuZKcSgViGOph6mtTc/i5/V7k/Malr7/YxBez" +
            "cjPsCTq/LNI8RqttgK/juTyfqv8uSiqbkGWGETaSk2Kb5xsFcNlQQWSbGlLqfiZ41yfAAH/BysfePaWsZCjnts5xkiSRTT14Abd9uH" +
            "XZaGT2cvhRs7reawctIXtX1s3kTqM9YV\u002B/wCpBqfVFxksXFEhjMlMPUrxf1ja7gibof1E49vZigAAAACFDy1uAqR6\u002BCT" +
            "QmradxC1wyyjL\u002BiSft\u002B5XudJWwSdi76mB2IJG4xf1cMg/zt4KpmqQOfXhSIM7gXNbEcrXg9U90XYwkzFdM9Y0eGokHG7" +
            "OfCOn7LyKnNptjseCV\u002Bbw1hkCDQwBAgMEBQYHAAgJCwwzAAoAAAAAAAAArA0AAAAAAACghgEAAAAAAACeKSYIAAAAAAAAAAAA" +
            "AABAQg8AAAAAAP//DQkBAgAICQoHDgsFAAUAAAA=";

        private static readonly string SettleFundsTransaction =
            "ARgtQuGpzVREjYAIC6lXrFvsSsHgk\u002Bt8ah1psWXoPsrzXfY9t512USKBZ5JtsM3HD2NH9YwQFqmvjyCTsuKO6w4BAAMKCnPnH" +
            "\u002B3SE2XtCu6E/OTisgZH/TTV7L2hFfAj0mM/h3gxkJiMEg9pt0UY4VGDhoK5gi\u002BlfT9SnYfSCwUM68ArGrDRLePOtF/pN" +
            "wBZrQrHhveA1F1zpqpkyj0EsJP1LEmzKBWIY6mHqa1Nz\u002BLn9XuT8xqWvv9jEF7NyM\u002BwJOr8s0jxGq22Ar\u002BO5PJ" +
            "\u002Bq/y5KKpuQZYYRNpKTYpvnGwVw2VBBZJsaUup\u002BJnjXJ8AAf8HKx949paxkKOe2znGSJJFNPXgDTHgtBNp84vnyAvrkV" +
            "xNRBBpjT60T05B\u002BsVW2BLmSnGpgdiCRuMX9XDIP87eCqZqkDn14UiDO4FzWxHK14PVPQbd9uHXZaGT2cvhRs7reawctIXtX1" +
            "s3kTqM9YV\u002B/wCphQ8tbgKkevgk0Jq2ncQtcMsoy/okn7fuV7nSVsEnYu/OFy1JqmWOOhq9DwvsmsTrK\u002Bnq7NGWYPKcd" +
            "M4rtJJ\u002B6AEJCQECAAMEBQYHCAUABQAAAA==";

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void MarketManagerNewOrderExceptionTest()
        {
            IMarketManager c = MarketFactory.GetMarket(MNGOUSDCAddress, Account);

            Order order = new OrderBuilder()
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
            IMarketManager c = MarketFactory.GetMarket(MNGOUSDCAddress, Account);

            c.CancelOrder(new BigInteger(464876397401554404955348M));
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void MarketManagerCancelOrderByClientIdExceptionTest()
        {
            IMarketManager c = MarketFactory.GetMarket(MNGOUSDCAddress, Account);

            c.CancelOrder(123453125UL);
        }

        [TestInitialize]
        public void SetupTest()
        {
            ResponseQueue = new Queue<string>();
            AddressQueue = new Queue<string>();
            SignatureConfirmed = false;
            ConfirmedSignatures = 0;
        }

        [TestMethod]
        public void MarketManagerInitTest()
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

            MarketManager sut = new(SXPUSDCAddress, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.IsNull(sut.OpenOrdersAccount);
        }
        
        [TestMethod]
        public void MarketManagerReloadTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/WithoutOpenOrdersGetProgramAccounts.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut =
                new(SXPUSDCAddress, Account, UnusedSignatureMethod, serumClient: serumMock.Object);
            sut.Init();
            
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.AreEqual("AraQPzSsE31pdzeTe6Dkvu6g8PvreFW429DAYhsfKYRd", sut.BaseTokenAccountAddress);
            Assert.AreEqual("tWQuevB8Rou1HS9a76fjYSQPrDixZMbVzXe2Q1kY5ma", sut.QuoteTokenAccountAddress);
            Assert.IsNull(sut.OpenOrdersAccount);

            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            // Mock the methods in the order they should be called
            
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);

            sut.Reload();
            
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.AreEqual("CuDkjTkdC7dFoq9Pf6jUD5GTkDdcoiSVwmDBrf1fs4K4", sut.OpenOrdersAddress);
            Assert.IsNotNull(sut.OpenOrdersAccount);
        }

        [TestMethod]
        public void MarketManagerInitWithAccountWithoutOpenOrdersAccountTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/WithoutOpenOrdersGetProgramAccounts.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut =
                new(SXPUSDCAddress, Account, UnusedSignatureMethod, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.AreEqual("AraQPzSsE31pdzeTe6Dkvu6g8PvreFW429DAYhsfKYRd", sut.BaseTokenAccountAddress);
            Assert.AreEqual("tWQuevB8Rou1HS9a76fjYSQPrDixZMbVzXe2Q1kY5ma", sut.QuoteTokenAccountAddress);
            Assert.IsNull(sut.OpenOrdersAccount);
        }
        
        [TestMethod]
        public void MarketManagerInitWithAccountWithoutTokenAccountsTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/WithoutBaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/WithoutQuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/WithoutOpenOrdersGetProgramAccounts.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut =
                new(SXPUSDCAddress, Account, UnusedSignatureMethod, serumClient: serumMock.Object);
            sut.Init();

            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", sut.Market.OwnAddress);
            Assert.AreEqual("SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX", sut.Market.BaseMint);
            Assert.AreEqual("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", sut.Market.QuoteMint);
            Assert.IsNull(sut.BaseAccount);
            Assert.IsNull(sut.BaseTokenAccountAddress);
            Assert.IsNull(sut.QuoteAccount);
            Assert.IsNull(sut.QuoteTokenAccountAddress);
            Assert.IsNull(sut.OpenOrdersAccount);
        }

        [TestMethod]
        public void MarketManagerNewOrderTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderTestValidResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsync(rpcMock, NewOrderTransaction);

            string subscribeSignatureNotification =
                File.ReadAllText("Resources/MarketManager/NewOrderTestSubscribeSignatureNotification.json");
            Mock<IStreamingRpcClient> streamingRpcMock = StreamingClientSignatureSubscribeSetup(
                "vEt8sftW4Y2X3uMGFx9BUj8AdvQmVCG3jdhruDiZ9xQ2cUmGWADMWSur9QrcYYFotk9AtpqJQ4iaUL4hUbyXQH2",
                "https://api.mainnet-beta.solana.com");

            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, streamingRpcMock.Object,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut = new(SXPUSDCAddress, Account, NewOrderMockedSignature,
                serumClient: serumMock.Object);
            sut.Init();

            Order order = new OrderBuilder()
                .SetPrice(3.5f)
                .SetQuantity(1)
                .SetSide(Side.Buy)
                .SetOrderType(OrderType.Limit)
                .SetSelfTradeBehavior(SelfTradeBehavior.DecrementTake)
                .SetClientOrderId(1_000_000UL)
                .Build();

            SignatureConfirmation sigConf = sut.NewOrder(order);

            sigConf.ConfirmationChanged += OnConfirmationChangedAssertNoErrors;

            Assert.AreEqual("vEt8sftW4Y2X3uMGFx9BUj8AdvQmVCG3jdhruDiZ9xQ2cUmGWADMWSur9QrcYYFotk9AtpqJQ4iaUL4hUbyXQH2",
                sigConf.Signature);

            ResponseValue<ErrorResult> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<ErrorResult>>(subscribeSignatureNotification,
                    JsonSerializerOptions);
            WsResponseAction(sigConf.Subscription, notificationContent);

            while (!SignatureConfirmed)
            {
                Task.Delay(50);
            }
            
            sigConf.ConfirmationChanged -= OnConfirmationChangedAssertNoErrors;
            
            Assert.IsNull(sigConf.InstructionError);
            Assert.IsNull(sigConf.TransactionError);
            Assert.IsNull(sigConf.Error);
        }
        
        [TestMethod]
        public void MarketManagerNewOrderWithoutBuilderTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderTestValidResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsync(rpcMock, NewOrderTransaction);

            string subscribeSignatureNotification =
                File.ReadAllText("Resources/MarketManager/NewOrderTestSubscribeSignatureNotification.json");
            Mock<IStreamingRpcClient> streamingRpcMock = StreamingClientSignatureSubscribeSetup(
                "vEt8sftW4Y2X3uMGFx9BUj8AdvQmVCG3jdhruDiZ9xQ2cUmGWADMWSur9QrcYYFotk9AtpqJQ4iaUL4hUbyXQH2",
                "https://api.mainnet-beta.solana.com");

            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, streamingRpcMock.Object,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut = new(SXPUSDCAddress, Account, NewOrderMockedSignature,
                serumClient: serumMock.Object);
            sut.Init();
            
            SignatureConfirmation sigConf = sut.NewOrder(Side.Buy, OrderType.Limit, SelfTradeBehavior.DecrementTake, 1, 3.5f, 1_000_000UL);

            sigConf.ConfirmationChanged += OnConfirmationChangedAssertNoErrors;

            Assert.AreEqual("vEt8sftW4Y2X3uMGFx9BUj8AdvQmVCG3jdhruDiZ9xQ2cUmGWADMWSur9QrcYYFotk9AtpqJQ4iaUL4hUbyXQH2",
                sigConf.Signature);

            ResponseValue<ErrorResult> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<ErrorResult>>(subscribeSignatureNotification,
                    JsonSerializerOptions);
            WsResponseAction(sigConf.Subscription, notificationContent);

            while (!SignatureConfirmed)
            {
                Task.Delay(50);
            }
            
            sigConf.ConfirmationChanged -= OnConfirmationChangedAssertNoErrors;
            
            Assert.IsNull(sigConf.InstructionError);
            Assert.IsNull(sigConf.TransactionError);
            Assert.IsNull(sigConf.Error);
        }
        
        [TestMethod]
        public void MarketManagerNewOrderInsufficientBalanceTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderInsufficientBalanceTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/NewOrderInsufficientBalanceTestResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsyncWithError(rpcMock, NewOrderInsufficientBalanceTransaction);

            Mock<ISerumClient> serumMock = MockSerumClient(rpcMock.Object, null,
                "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut = new(SXPUSDCAddress, Account, NewOrderInsufficientBalanceMockedSignature,
                serumClient: serumMock.Object);
            sut.Init();

            Order order = new OrderBuilder()
                .SetPrice(3.5f)
                .SetQuantity(10_000)
                .SetSide(Side.Buy)
                .SetOrderType(OrderType.Limit)
                .SetSelfTradeBehavior(SelfTradeBehavior.DecrementTake)
                .SetClientOrderId(1_000_000UL)
                .Build();

            SignatureConfirmation sigConf = sut.NewOrder(order);
            
            Assert.IsNotNull(sigConf.InstructionError);
            Assert.IsNotNull(sigConf.TransactionError);
            Assert.IsNotNull(sigConf.Error);
            Assert.AreEqual(SerumProgramError.InsufficientFunds, sigConf.Error);
        }

        [TestMethod]
        public void MarketManagerCancelOrderByClientIdTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/CancelOrderByClientIdTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/CancelOrderByClientIdTestValidResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsync(rpcMock, CancelOrderByClientIdTransaction);

            string subscribeSignatureNotification =
                File.ReadAllText(
                    "Resources/MarketManager/CancelOrderByClientIdTestSubscribeSignatureNotification.json");
            Mock<IStreamingRpcClient> streamingRpcMock = StreamingClientSignatureSubscribeSetup(
                "2aqr3fgPUZxzhmNCsKiomA13AMCTADaqiUdVheXmBPnZHtVNsbH3WSxXAmCw3gJgbgWwKbW44NGSDETZCmhbUhRT",
                "https://api.mainnet-beta.solana.com");

            Mock<ISerumClient> serumMock = MockSerumClient(
                rpcMock.Object, streamingRpcMock.Object, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ",
                GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager marketManager = new(SXPUSDCAddress, Account, CancelOrderByClientIdMockedSignature,
                serumClient: serumMock.Object);
            marketManager.Init();

            SignatureConfirmation sigConf = marketManager.CancelOrder(1_000_000UL);

            sigConf.ConfirmationChanged += OnConfirmationChangedAssertNoErrors;

            Assert.AreEqual("2aqr3fgPUZxzhmNCsKiomA13AMCTADaqiUdVheXmBPnZHtVNsbH3WSxXAmCw3gJgbgWwKbW44NGSDETZCmhbUhRT",
                sigConf.Signature);

            ResponseValue<ErrorResult> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<ErrorResult>>(subscribeSignatureNotification,
                    JsonSerializerOptions);
            WsResponseAction(sigConf.Subscription, notificationContent);

            while (!SignatureConfirmed)
            {
                Task.Delay(50);
            }

            sigConf.ConfirmationChanged -= OnConfirmationChangedAssertNoErrors;
            Assert.IsNull(sigConf.InstructionError);
            Assert.IsNull(sigConf.TransactionError);
            Assert.IsNull(sigConf.Error);
        }

        [TestMethod]
        public void MarketManagerCancelAllOrdersTest()
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/CancelAllOrdersTestOpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/CancelAllOrdersTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/CancelAllOrdersTestValidResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsync(rpcMock, CancelAllOrdersTransaction);

            string subscribeSignatureNotification =
                File.ReadAllText(
                    "Resources/MarketManager/CancelAllOrdersTestSubscribeSignatureNotification.json");
            Mock<IStreamingRpcClient> streamingRpcMock = StreamingClientSignatureSubscribeSetup(
                "nGCGiFmWM4PSgVBraVf8RnT9crdUeEbZfqXGhHGsr6AQWaJWmCsqwy4u3sf1E8QnVJTQuokQ32wxQv2ujK8YY8G",
                "https://api.mainnet-beta.solana.com");

            Mock<ISerumClient> serumMock = MockSerumClient(
                rpcMock.Object, streamingRpcMock.Object, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ",
                GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut = new(SXPUSDCAddress, Account, CancelAllOrdersMockedSignature,
                serumClient: serumMock.Object);
            sut.Init();

            IList<SignatureConfirmation> sigConfs = sut.CancelAllOrders();

            sigConfs[0].ConfirmationChanged  += OnConfirmationChangedAssertNoErrors;

            Assert.AreEqual("nGCGiFmWM4PSgVBraVf8RnT9crdUeEbZfqXGhHGsr6AQWaJWmCsqwy4u3sf1E8QnVJTQuokQ32wxQv2ujK8YY8G",
                sigConfs[0].Signature);

            ResponseValue<ErrorResult> notificationContent =
                JsonSerializer.Deserialize<ResponseValue<ErrorResult>>(subscribeSignatureNotification,
                    JsonSerializerOptions);
            WsResponseAction(sigConfs[0].Subscription, notificationContent);

            while (!SignatureConfirmed)
            {
                Task.Delay(50);
            }

            sigConfs[0].ConfirmationChanged -= OnConfirmationChangedAssertNoErrors;
            Assert.IsNull(sigConfs[0].InstructionError);
            Assert.IsNull(sigConfs[0].TransactionError);
            Assert.IsNull(sigConfs[0].Error);
        }

        [TestMethod]
        public void MarketManagerSettleFundsTest() 
        {
            // Queue mock responses
            EnqueueResponseFromFile("Resources/MarketManager/MarketGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetAccountInfo.json");
            EnqueueResponseFromFile("Resources/MarketManager/BaseTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/QuoteTokenGetTokenAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/OpenOrdersGetProgramAccounts.json");
            EnqueueResponseFromFile("Resources/MarketManager/SettleFundsTestGetBlockHash.json");
            EnqueueResponseFromFile("Resources/MarketManager/SettleFundsTestValidResponse.json");

            // Mock the methods in the order they should be called
            Mock<IRpcClient> rpcMock = MockRpcClient(ClusterUrl);
            MockGetAccountInfoAsync(rpcMock, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
            MockGetAccountInfoAsync(rpcMock, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetAccountInfoAsync(rpcMock, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "SF3oTvfWzEP3DTwGSvUXRrGTvr75pdZNnBLAH9bzMuX");
            MockGetTokenAccountsByOwnerAsync(rpcMock, Account, "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
            MockGetProgramAccountsAsync(rpcMock, SXPUSDCAddress, Account);
            MockGetRecentBlockhashAsync(rpcMock);
            MockSendTransactionAsync(rpcMock, SettleFundsTransaction);

            string subscribeSignatureNotification =
                File.ReadAllText(
                    "Resources/MarketManager/SettleFundsTestSubscribeSignatureNotification.json");
            Mock<IStreamingRpcClient> streamingRpcMock = StreamingClientSignatureSubscribeSetup(
                "V34cxwkTimbzJmDih5Z4eWFjtJSfVyPiwvUfqDh89e6fNTxaatbGFmqac2QYbQjXpABffvqvHRXdfjHfgr5Yhuo",
                "https://api.mainnet-beta.solana.com");

            Mock<ISerumClient> serumMock = MockSerumClient(
                rpcMock.Object, streamingRpcMock.Object, "4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ",
                GetMarketAccountData("Resources/MarketManager/SXPUSDCMarketAccountData.txt"));

            MarketManager sut = new(SXPUSDCAddress, Account, SettleFundsMockedSignature,
                serumClient: serumMock.Object);
            sut.Init();

            SignatureConfirmation sigConf = sut.SettleFunds();

            Assert.IsNull(sigConf.InstructionError);
            Assert.IsNull(sigConf.TransactionError);
            Assert.IsNull(sigConf.Error);
        }

        private void OnConfirmationChangedAssertNoErrors(object? sender, SignatureConfirmationStatus e)
        {
            Assert.IsNull(e.TransactionError);
            Assert.IsNull(e.Error);
            Assert.IsNull(e.InstructionError);
            SignatureConfirmed = true;
        }

        private byte[] SettleFundsMockedSignature(ReadOnlySpan<byte> messageData)
        {
            return Convert.FromBase64String(
                "GC1C4anNVESNgAgLqVesW+xKweCT63xqHWmxZeg+yvNd9j23nXZRIoFnkm2wzccPY0f1jBAWqa+PIJOy4o7rDg==");
        }

        private byte[] CancelAllOrdersMockedSignature(ReadOnlySpan<byte> messageData)
        {
            return Convert.FromBase64String(
                "Jwg+o85tKU2r4fc5LO7IO03SeYsYva94+wER5JYaHblktnU31XwOnJS0z28/gpEVNVE0WZMgBZVw4ej/iNqECQ==");
        }

        private byte[] CancelOrderByClientIdMockedSignature(ReadOnlySpan<byte> messageData)
        {
            return Convert.FromBase64String(
                "TzOBr+EFQQCJTAHUPkP67ZpD8DsuKrkdVspv1EgBehTOVBzTzUaAC6OiiCA8VJkCL9e1egEA+FW1nsiBgDIhAg==");
        }

        private byte[] NewOrderInsufficientBalanceMockedSignature(ReadOnlySpan<byte> messageData)
        {
            return Convert.FromBase64String("mSLsnhJbG8UpKmMuNhcMedgnaehBXoccA1yDPhI0J11qywXBrm6XY/WomX+jUyM58ojd6tXCcQD7GRrVbxooBQ==");
        }
        
        private byte[] NewOrderMockedSignature(ReadOnlySpan<byte> messageData)
        {
            return Convert.FromBase64String(
                "LeldoHaGtGOCTgAkdSt9pSXl20hzHJPadF3PmoHnD3dKlw52sObHSWMvbDHHHo3SIFqNHXlQ+fhkdJKib6Q/DQ==");
        }

        private byte[] UnusedSignatureMethod(ReadOnlySpan<byte> messageData)
        {
            throw new NotImplementedException();
        }
    }
}