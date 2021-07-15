// unset

using System.Numerics;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// A base class for the Orders in Serum.
    /// </summary>
    public abstract class OrderBase
    {
        /// <summary>
        /// The order id.
        /// </summary>
        public BigInteger OrderId;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientId;

        /// <summary>
        /// The price for the order.
        /// </summary>
        public ulong RawPrice;

        /// <summary>
        /// The order's maximum base quantity.
        /// </summary>
        public ulong RawQuantity;
    }
}