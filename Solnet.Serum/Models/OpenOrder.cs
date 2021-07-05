// unset

using System.Numerics;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Open Order in a Serum Open Orders Account.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The index of the order within the <see cref="OpenOrdersAccount"/> data.
        /// </summary>
        public int OrderIndex;

        /// <summary>
        /// The client id.
        /// </summary>
        public ulong ClientId;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientOrderId;

        /// <summary>
        /// The price for the order.
        /// </summary>
        public ulong Price;

        /// <summary>
        /// Whether this slot within the <see cref="OpenOrdersAccount"/> Orders is free or not.
        /// </summary>
        public bool IsFreeSlot;

        /// <summary>
        /// Whether this order is a bid or not.
        /// </summary>
        public bool IsBid;
    }
}