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
        /// Instantiate a the serum client.
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
        /// Instantiate a the serum client.
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