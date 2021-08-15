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
        /// <param name="account">The <see cref="PublicKey"/> of the owner account.</param>
        /// <param name="srmAccount">The <see cref="PublicKey"/> of the serum account to use for fee discount, not used when not provided.</param>
        /// <param name="signatureMethod">A delegate method used to request a signature for transactions crafted by the <see cref="MarketManager"/> which will submit, cancel orders, or settle funds.</param>
        /// <param name="url">The cluster to use when not passing in a serum client instance.</param>
        /// <param name="serumClient">The Serum Client instance.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Serum Client.</returns>
        public static IMarketManager GetMarket(PublicKey marketAddress, PublicKey account, PublicKey srmAccount = null,
            MarketManager.RequestSignature signatureMethod = null, string url = null, ISerumClient serumClient = null,
            ILogger logger = null)
        {
#if DEBUG
            logger ??= GetDebugLogger();
#endif
            return new MarketManager(marketAddress, account, srmAccount, signatureMethod, url, logger, serumClient);
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