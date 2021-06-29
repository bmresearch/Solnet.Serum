// unset

using Solnet.Rpc.Core.Sockets;
using Solnet.Wallet;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Wraps a subscription with a generic type to hold either order book or trade events.
    /// </summary>
    /// <typeparam name="T">The type of the subscription.</typeparam>
    public class Subscription<T>
    {
        /// <summary>
        /// The address associated with this data.
        /// </summary>
        public PublicKey Address;
        
        /// <summary>
        /// The underlying subscription state.
        /// </summary>
        public SubscriptionState SubscriptionState;

        /// <summary>
        /// The underlying data.
        /// </summary>
        public T Data;
    }
}