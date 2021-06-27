using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
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
        /// The address of the cluster node.
        /// </summary>
        public Uri NodeAddress => _rpcClient.NodeAddress;

        /// <summary>
        /// Initialize the Serum Client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">An http client.</param>
        /// <param name="rpcClient">A solana rpc client.</param>
        /// <returns>The Serum Client.</returns>
        internal SerumClient(Cluster cluster, ILogger logger = null, HttpClient httpClient = default,
            IRpcClient rpcClient = default)
            => Init(cluster, null, logger, httpClient, rpcClient);

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
        private void Init(
            Cluster cluster = default, string url = null, ILogger logger = null, HttpClient httpClient = default,
            IRpcClient rpcClient = default)
        {
            _logger = logger;
            _httpClient = httpClient ?? new HttpClient {BaseAddress = new Uri(InfosBaseUrl)};
            _jsonSerializerOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            _rpcClient = rpcClient ?? (url != null
                ? Solnet.Rpc.ClientFactory.GetClient(url, logger)
                : Solnet.Rpc.ClientFactory.GetClient(cluster, logger));
            _streamingRpcClient = url != null
                ? Solnet.Rpc.ClientFactory.GetStreamingClient(url, logger)
                : Solnet.Rpc.ClientFactory.GetStreamingClient(cluster, logger);
        }
        
        
        /// <inheritdoc cref="ISerumClient.GetEventQueueAsync(string,Commitment)"/>
        public async Task<EventQueue> GetEventQueueAsync(string marketAddress, Commitment commitment = Commitment.Finalized)
        {
            RequestResult<ResponseValue<AccountInfo>> res =
                await _rpcClient.GetAccountInfoAsync(marketAddress, commitment);
            return res.WasSuccessful ? EventQueue.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }
        
        /// <inheritdoc cref="ISerumClient.GetEventQueue(string,Commitment)"/>
        public EventQueue GetEventQueue(string marketAddress, Commitment commitment = Commitment.Finalized)
            => GetEventQueueAsync(marketAddress, commitment).Result;

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
            Task<HttpResponseMessage> res = _httpClient.GetAsync(MarketInfosEndpoint);

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