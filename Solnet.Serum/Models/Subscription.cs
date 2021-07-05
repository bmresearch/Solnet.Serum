// unset

using Solnet.Rpc.Core.Sockets;
using Solnet.Wallet;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Wraps a subscription with a generic type to hold either order book or trade events.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// The address associated with this data.
        /// </summary>
        public PublicKey Address;
        
        /// <summary>
        /// The underlying subscription state.
        /// </summary>
        public SubscriptionState SubscriptionState;
    }

    /// <summary>
    /// Wraps the base subscription to have the underlying data of the subscription, which is sometimes needed to perform
    /// some logic before returning data to the subscription caller.
    /// </summary>
    /// <typeparam name="T">The type of the subscription.</typeparam>
    internal class SubscriptionWrapper<T> : Subscription
    {
        /// <summary>
        /// The underlying data.
        /// </summary>
        public T Data;
    }
}