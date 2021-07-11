using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements the Serum Program and client functionality for easy access to data.
    /// </summary>
    [DebuggerDisplay("Cluster = {" + nameof(NodeAddress) + "}")]
    public class SerumClient : ISerumClient
    {
        /// <summary>
        /// The base url to fetch the market's data from github.
        /// </summary>
        private const string InfosBaseUrl = "https://raw.githubusercontent.com/project-serum/serum-js/master/src/";

        /// <summary>
        /// The name of the file containing token mints.
        /// </summary>
        private const string TokenMintsEndpoint = "token-mints.json";

        /// <summary>
        /// The name of the file containing market infos.
        /// </summary>
        private const string MarketInfosEndpoint = "markets.json";

        /// <summary>
        /// The http client to fetch the market data.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// The json serializer options.
        /// </summary>
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger instance.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The rpc client instance to perform requests.
        /// </summary>
        private IRpcClient _rpcClient;

        /// <summary>
        /// The streaming rpc client instance to subscribe to data.
        /// </summary>
        private IStreamingRpcClient _streamingRpcClient;

        /// <summary>
        /// The list of <see cref="EventQueue"/> subscriptions.
        /// </summary>
        private IList<SubscriptionWrapper<EventQueue>> _eventQueueSubscriptions;
        
        /// <summary>
        /// The list of <see cref="OpenOrdersAccount"/> subscriptions.
        /// </summary>
        private IList<SubscriptionWrapper<OpenOrdersAccount>> _openOrdersSubscriptions;
        
        
        /// <summary>
        /// The list of <see cref="OrderBook"/> subscriptions.
        /// </summary>
        private IList<SubscriptionWrapper<OrderBook>> _orderBookSubscriptions;

        /// <summary>
        /// The cluster the client is connected to.
        /// </summary>
        public Uri NodeAddress => _rpcClient.NodeAddress;

        /// <summary>
        /// Initialize the Serum Client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">An http client.</param>
        /// <param name="rpcClient">A solana rpc client.</param>
        /// <param name="streamingRpcClient">A solana streaming rpc client.</param>
        /// <returns>The Serum Client.</returns>
        internal SerumClient(Cluster cluster, ILogger logger = null, HttpClient httpClient = default,
            IRpcClient rpcClient = default, IStreamingRpcClient streamingRpcClient = default)
            => Init(cluster, null, logger, httpClient, rpcClient, streamingRpcClient);

        /// <summary>
        /// Initialize the Serum Client.
        /// </summary>
        /// <param name="url">The url of the node to connect to.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Serum Client.</returns>
        internal SerumClient(string url, ILogger logger = null) => Init(default, url, logger);

        /// <summary>
        /// Initialize the client with the given arguments.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="url">The url of the node to connect to.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">An http client.</param>
        /// <param name="rpcClient">A solana rpc client.</param>
        /// <param name="streamingRpcClient">A solana streaming rpc client.</param>
        private void Init(
            Cluster cluster = default, string url = null, ILogger logger = null, HttpClient httpClient = default,
            IRpcClient rpcClient = default, IStreamingRpcClient streamingRpcClient = default)
        {
            _logger = logger;
            _httpClient = httpClient ?? new HttpClient {BaseAddress = new Uri(InfosBaseUrl)};
            _jsonSerializerOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            _rpcClient = rpcClient ?? (url != null
                ? Solnet.Rpc.ClientFactory.GetClient(url, logger)
                : Solnet.Rpc.ClientFactory.GetClient(cluster, logger));
            _streamingRpcClient = streamingRpcClient ?? (url != null
                ? Solnet.Rpc.ClientFactory.GetStreamingClient(url, logger)
                : Solnet.Rpc.ClientFactory.GetStreamingClient(cluster, logger));
            _eventQueueSubscriptions = new List<SubscriptionWrapper<EventQueue>>();
            _openOrdersSubscriptions = new List<SubscriptionWrapper<OpenOrdersAccount>>();
            _orderBookSubscriptions = new List<SubscriptionWrapper<OrderBook>>();
        }
        
        /// <inheritdoc cref="ISerumClient.RpcClient"/>
        public IRpcClient RpcClient => _rpcClient;
        
        /// <inheritdoc cref="ISerumClient.ConnectionStatistics"/>
        public IConnectionStatistics ConnectionStatistics => _streamingRpcClient.Statistics;

        #region Streaming RPC

        /// <inheritdoc cref="ISerumClient.ConnectAsync"/>
        public Task ConnectAsync() => _streamingRpcClient.ConnectAsync();

        /// <inheritdoc cref="ISerumClient.Connect"/>
        public void Connect() => ConnectAsync().Wait();

        /// <inheritdoc cref="ISerumClient.DisconnectAsync"/>
        public Task DisconnectAsync()
        {
            _eventQueueSubscriptions.Clear();
            _openOrdersSubscriptions.Clear();
            _orderBookSubscriptions.Clear();
            return _streamingRpcClient.DisconnectAsync();
        }

        /// <inheritdoc cref="ISerumClient.Disconnect"/>
        public void Disconnect() => DisconnectAsync().Wait();
        
        /// <inheritdoc cref="ISerumClient.SubscribeOpenOrdersAccountAsync"/>
        public async Task<Subscription> SubscribeOpenOrdersAccountAsync(
            Action<Subscription, OpenOrdersAccount> action, string openOrdersAccountAddress, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState sub = await _streamingRpcClient.SubscribeAccountInfoAsync(openOrdersAccountAddress,
                (_, value) =>
                {
                    SubscriptionWrapper<OpenOrdersAccount> openOrdersSub = null;
                    foreach (SubscriptionWrapper<OpenOrdersAccount> subscription in _openOrdersSubscriptions)
                    {
                        if (subscription.Address.Key == openOrdersAccountAddress)
                        {
                            openOrdersSub = subscription;
                        }
                    }
                    
                    OpenOrdersAccount openOrdersAccount = OpenOrdersAccount.Deserialize(Convert.FromBase64String(value.Value.Data[0]));
                    if (openOrdersSub != null) openOrdersSub.Data = openOrdersAccount;

                    action(openOrdersSub, openOrdersAccount);
                }, commitment);
            
            SubscriptionWrapper<OpenOrdersAccount> subOpenOrders = new ()
            {
                SubscriptionState = sub, 
                Address = new PublicKey(openOrdersAccountAddress)
            };
            _openOrdersSubscriptions.Add(subOpenOrders);
            return subOpenOrders;
        }
        
        /// <inheritdoc cref="ISerumClient.SubscribeOpenOrdersAccount"/>
        public Subscription SubscribeOpenOrdersAccount(
            Action<Subscription, OpenOrdersAccount> action, string openOrdersAccountAddress, Commitment commitment = Commitment.Finalized) 
            => SubscribeOpenOrdersAccountAsync(action, openOrdersAccountAddress, commitment).Result;
        
        /// <inheritdoc cref="ISerumClient.SubscribeOrderBookAsync"/>
        public async Task<Subscription> SubscribeOrderBookAsync(
            Action<Subscription, OrderBook> action, string orderBookAccountAddress, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState sub = await _streamingRpcClient.SubscribeAccountInfoAsync(orderBookAccountAddress,
                (_, value) =>
                {
                    SubscriptionWrapper<OrderBook> orderBookSub = null;
                    foreach (SubscriptionWrapper<OrderBook> subscription in _orderBookSubscriptions)
                    {
                        if (subscription.Address.Key == orderBookAccountAddress)
                        {
                            orderBookSub = subscription;
                        }
                    }

                    OrderBook openOrdersAccount = OrderBook.Deserialize(Convert.FromBase64String(value.Value.Data[0]));
                    action(orderBookSub, openOrdersAccount);
                }, commitment);
            
            SubscriptionWrapper<OrderBook> subOrderBook = new ()
            {
                SubscriptionState = sub, 
                Address = new PublicKey(orderBookAccountAddress)
            };
            _orderBookSubscriptions.Add(subOrderBook);
            return subOrderBook;
        }
        
        /// <inheritdoc cref="ISerumClient.SubscribeOrderBook"/>
        public Subscription SubscribeOrderBook(
            Action<Subscription, OrderBook> action, string orderBookAccountAddress, Commitment commitment = Commitment.Finalized) 
            => SubscribeOrderBookAsync(action, orderBookAccountAddress, commitment).Result;
        
        /// <inheritdoc cref="ISerumClient.SubscribeEventQueueAsync"/>
        public async Task<Subscription> SubscribeEventQueueAsync(
            Action<Subscription, EventQueue> action, string eventQueueAccountAddress, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState sub = await _streamingRpcClient.SubscribeAccountInfoAsync(eventQueueAccountAddress,
                (_, value) =>
                {
                    SubscriptionWrapper<EventQueue> evtQueueSub = null;
                    EventQueue evtQueue;
                    foreach (SubscriptionWrapper<EventQueue> subscription in _eventQueueSubscriptions)
                    {
                        if (subscription.Address.Key == eventQueueAccountAddress)
                        {
                            evtQueueSub = subscription;
                        }
                    }

                    if (evtQueueSub?.Data != null)
                    {
                        evtQueue = EventQueue.DeserializeSince(
                            Convert.FromBase64String(value.Value.Data[0]), evtQueueSub.Data.Header.NextSequenceNumber);
                        evtQueueSub.Data = evtQueue;
                    }
                    else
                    {
                        evtQueue = EventQueue.Deserialize(Convert.FromBase64String(value.Value.Data[0]));
                        if (evtQueueSub != null) evtQueueSub.Data = evtQueue;
                    }

                    action(evtQueueSub, evtQueue);
                }, commitment);
            
            SubscriptionWrapper<EventQueue> subEvtQueue = new ()
            {
                SubscriptionState = sub, 
                Address = new PublicKey(eventQueueAccountAddress)
            };
            _eventQueueSubscriptions.Add(subEvtQueue);
            return subEvtQueue;
        }
        
        /// <inheritdoc cref="ISerumClient.SubscribeEventQueue"/>
        public Subscription SubscribeEventQueue(
            Action<Subscription, EventQueue> action, string eventQueueAccountAddress, Commitment commitment = Commitment.Finalized) 
            => SubscribeEventQueueAsync(action, eventQueueAccountAddress, commitment).Result;

        #endregion
        
        #region RPC Requests
        
        /// <inheritdoc cref="ISerumClient.GetOpenOrdersAccountAsync(string,Commitment)"/>
        public async Task<OpenOrdersAccount> GetOpenOrdersAccountAsync(string address, Commitment commitment = Commitment.Finalized)
        {
            RequestResult<ResponseValue<AccountInfo>> res =
                await _rpcClient.GetAccountInfoAsync(address, commitment);
            return res.WasSuccessful ? OpenOrdersAccount.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }
        
        /// <inheritdoc cref="ISerumClient.GetOpenOrdersAccount(string,Commitment)"/>
        public OpenOrdersAccount GetOpenOrdersAccount(string address, Commitment commitment = Commitment.Finalized)
            => GetOpenOrdersAccountAsync(address, commitment).Result;

        /// <inheritdoc cref="ISerumClient.GetOrderBookAsync(string,Commitment)"/>
        public async Task<OrderBook> GetOrderBookAsync(string address, Commitment commitment = Commitment.Finalized)
        {
            RequestResult<ResponseValue<AccountInfo>> res =
                await _rpcClient.GetAccountInfoAsync(address, commitment);
            return res.WasSuccessful ? OrderBook.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }

        /// <inheritdoc cref="ISerumClient.GetOrderBook(string,Commitment)"/>
        public OrderBook GetOrderBook(string address, Commitment commitment = Commitment.Finalized) 
            => GetOrderBookAsync(address, commitment).Result;

        /// <inheritdoc cref="ISerumClient.GetEventQueueAsync(string,Commitment)"/>
        public async Task<EventQueue> GetEventQueueAsync(string eventQueueAddress, Commitment commitment = Commitment.Finalized)
        {
            RequestResult<ResponseValue<AccountInfo>> res =
                await _rpcClient.GetAccountInfoAsync(eventQueueAddress, commitment);
            return res.WasSuccessful ? EventQueue.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }
        
        /// <inheritdoc cref="ISerumClient.GetEventQueue(string,Commitment)"/>
        public EventQueue GetEventQueue(string eventQueueAddress, Commitment commitment = Commitment.Finalized)
            => GetEventQueueAsync(eventQueueAddress, commitment).Result;

        /// <inheritdoc cref="ISerumClient.GetMarketAsync(string,Commitment)"/>
        public async Task<Market> GetMarketAsync(string marketAddress, Commitment commitment = Commitment.Finalized)
        {
            RequestResult<ResponseValue<AccountInfo>> res =
                await _rpcClient.GetAccountInfoAsync(marketAddress, commitment);
            return res.WasSuccessful ? Market.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }
        
        /// <inheritdoc cref="ISerumClient.GetMarket(string,Commitment)"/>
        public Market GetMarket(string marketAddress, Commitment commitment = Commitment.Finalized)
            => GetMarketAsync(marketAddress, commitment).Result;

        #endregion

        #region Token Mints and Markets
        
        /// <summary>
        /// Handle the response to the request.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>The task which returns the <see cref="RequestResult{T}"/>.</returns>
        private async Task<T> HandleResponse<T>(HttpResponseMessage message)
        {
            string data = await message.Content.ReadAsStringAsync();
            _logger?.LogInformation(new EventId(0, "REC"), $"Result: {data}");
            return JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
        }

        /// <inheritdoc cref="ISerumClient.GetMarkets"/>
        public async Task<IList<MarketInfo>> GetMarketsAsync()
        {
            HttpResponseMessage res = await _httpClient.GetAsync(MarketInfosEndpoint);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            return await HandleResponse<List<MarketInfo>>(res);
        }

        /// <inheritdoc cref="ISerumClient.GetMarkets"/>
        public IList<MarketInfo> GetMarkets() => GetMarketsAsync().Result;

        /// <inheritdoc cref="ISerumClient.GetTokensAsync"/>
        public async Task<IList<TokenInfo>> GetTokensAsync()
        {
            Task<HttpResponseMessage> res = _httpClient.GetAsync(TokenMintsEndpoint);

            if (!res.Result.IsSuccessStatusCode)
            {
                return null;
            }

            return await HandleResponse<List<TokenInfo>>(res.Result);
        }

        /// <inheritdoc cref="ISerumClient.GetTokens"/>
        public IList<TokenInfo> GetTokens() => GetTokensAsync().Result;

        #endregion
    }
}