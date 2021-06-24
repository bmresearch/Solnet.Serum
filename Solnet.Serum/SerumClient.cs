using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements the Serum Program and client functionality for easy access to data.
    /// </summary>
    public class SerumClient : ISerumClient
    {
        /// <summary>
        /// The base url to fetch the market's data from github.
        /// </summary>
        private static readonly string InfosBaseUrl =
            "https://raw.githubusercontent.com/project-serum/serum-js/master/src/";

        /// <summary>
        /// The name of the file containing token mints.
        /// </summary>
        private static readonly string TokenMintsEndpoint = "token-mints.json";
        
        /// <summary>
        /// The name of the file containing market infos.
        /// </summary>
        private static readonly string MarketInfosEndpoint = "markets.json";

        /// <summary>
        /// The http client to fetch the market data.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The json serializer options.
        /// </summary>
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        
        /// <summary>
        /// The logger instance.
        /// </summary>
        private readonly ILogger _logger;
        
        /// <summary>
        /// The rpc client instance to perform requests.
        /// </summary>
        public readonly IRpcClient RpcClient;
        
        /// <summary>
        /// The streaming rpc client instance to subscribe to data.
        /// </summary>
        public readonly IStreamingRpcClient StreamingRpcClient;

        /// <summary>
        /// Initialize the Serum client.
        /// </summary>
        internal SerumClient(Cluster cluster, ILogger logger = null)
        {
            _logger ??= LoggerFactory.Create(x =>
            {
                x.AddSimpleConsole(o =>
                    {
                        o.UseUtcTimestamp = true;
                        o.IncludeScopes = true;
                        o.ColorBehavior = LoggerColorBehavior.Enabled;
                        o.TimestampFormat = "HH:mm:ss ";
                    })
                    .SetMinimumLevel(LogLevel.Debug);
            }).CreateLogger<IRpcClient>();
            _httpClient = new HttpClient();
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            RpcClient = Solnet.Rpc.ClientFactory.GetClient(cluster, logger);
            StreamingRpcClient = Solnet.Rpc.ClientFactory.GetStreamingClient(cluster, logger);
        }

        /// <inheritdoc cref="ISerumClient.GetMarketAsync(string)"/>
        public async Task<Market> GetMarketAsync(string marketAddress)
        {
            var res = await RpcClient.GetAccountInfoAsync(marketAddress);

            return res.WasSuccessful ? Market.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0])) : null;
        }

        /// <inheritdoc cref="ISerumClient.GetMarket(string)"/>
        public Market GetMarket(string marketAddress)
            => GetMarketAsync(marketAddress).Result;

        /// <inheritdoc cref="ISerumClient.GetMarkets"/>
        public async Task<IList<MarketInfo>> GetMarketsAsync()
        {
            HttpRequestMessage req = new (HttpMethod.Get, new Uri(InfosBaseUrl + MarketInfosEndpoint));
            Task<HttpResponseMessage> res = _httpClient.SendAsync(req);

            if (!res.Result.IsSuccessStatusCode)
            {
                return null;
            }
            string resultContent = await res.Result.Content.ReadAsStringAsync();
            _logger?.LogInformation(
                new EventId(req.Options.GetHashCode(), req.Method.Method),
                $"Result: {resultContent}");
            return JsonSerializer.Deserialize<List<MarketInfo>>(resultContent, _jsonSerializerOptions);
        }

        /// <inheritdoc cref="ISerumClient.GetMarkets"/>
        public IList<MarketInfo> GetMarkets() => GetMarketsAsync().Result;

        /// <inheritdoc cref="ISerumClient.GetTokensAsync"/>
        public async Task<IList<TokenInfo>> GetTokensAsync()
        {
            HttpRequestMessage req = new (HttpMethod.Get, new Uri(InfosBaseUrl + TokenMintsEndpoint));
            Task<HttpResponseMessage> res = _httpClient.SendAsync(req);

            if (!res.Result.IsSuccessStatusCode)
            {
                return null;
            }
            string resultContent = await res.Result.Content.ReadAsStringAsync();
            _logger?.LogInformation(
                new EventId(req.Options.GetHashCode(), req.Method.Method),
                $"Result: {resultContent}");
            return JsonSerializer.Deserialize<List<TokenInfo>>(resultContent, _jsonSerializerOptions);
        }

        /// <inheritdoc cref="ISerumClient.GetTokens"/>
        public IList<TokenInfo> GetTokens() => GetTokensAsync().Result;

    }
}