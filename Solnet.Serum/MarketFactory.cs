// unset

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Solnet.Wallet;

namespace Solnet.Serum
{
    /// <summary>
    /// The factory for the Market Manager.
    /// </summary>
    public static class MarketFactory
    {
        /// <summary>
        /// Instantiate a new Market Manager.
        /// </summary>
        /// <param name="marketAddress">The market address.</param>
        /// <param name="url">The cluster to use when not passing in a serum client instance.</param>
        /// <param name="serumClient">The Serum Client instance.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Serum Client.</returns>
        public static IMarketManager GetMarket(string marketAddress, string url = null, ISerumClient serumClient = null, ILogger logger = null)
        {
#if DEBUG
            logger ??= GetDebugLogger();
#endif
            return new MarketManager(new PublicKey(marketAddress), url, logger, serumClient);
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
            }).CreateLogger<IMarketManager>();
        }
#endif
    }
}