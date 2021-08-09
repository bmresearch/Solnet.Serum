// ReSharper disable RedundantAssignment
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Rpc;

namespace Solnet.Serum
{
    /// <summary>
    /// The client factory for the Serum Client.
    /// </summary>
    public static class ClientFactory
    {
        /// <summary>
        /// Instantiate the serum client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rpcClient">The RPC Client instance.</param>
        /// <param name="streamingRpcClient">The Streaming RPC Client instance.</param>
        /// <returns>The Serum Client.</returns>
        public static ISerumClient GetClient(IRpcClient rpcClient = null, IStreamingRpcClient streamingRpcClient = null,
            ILogger logger = null)
        {
#if DEBUG
            logger ??= GetDebugLogger();
#endif
            return new SerumClient(null, logger, rpcClient, streamingRpcClient);
        }
        
        /// <summary>
        /// Instantiate the serum client.
        /// </summary>
        /// <param name="cluster">The network cluster.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The http client.</returns>
        public static ISerumClient GetClient(Cluster cluster, ILogger logger = null)
        {
#if DEBUG
            logger ??= GetDebugLogger();
#endif
            return new SerumClient(cluster, logger);
        }
        
        /// <summary>
        /// Instantiate the serum client.
        /// </summary>
        /// <param name="url">The url of the node to connect to.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The http client.</returns>
        public static ISerumClient GetClient(string url, ILogger logger = null)
        {
#if DEBUG
            logger ??= GetDebugLogger();
#endif
            return new SerumClient(url, logger);
        }
        
#if DEBUG
        /// <summary>
        /// Get a logger instance for use in debug mode.
        /// </summary>
        /// <returns>The logger.</returns>
        private static ILogger GetDebugLogger()
        {
            return LoggerFactory.Create(x =>
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
        }
#endif
    }
}