// unset

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
            logger ??= LoggerFactory.Create(x =>
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
#endif
            return new SerumClient(cluster, logger);
        }
    }
}