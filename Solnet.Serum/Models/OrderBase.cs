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
        public ulong ClientOrderId;

        /// <summary>
        /// The raw value for the price of the order.
        /// <remarks>This value needs to be converted according to decimals and lot sizes.</remarks>
        /// </summary>
        public ulong RawPrice;

        /// <summary>
        /// The raw value for the quantity of the order.
        /// <remarks>This value needs to be converted according to decimals and lot sizes.</remarks>
        /// </summary>
        public ulong RawQuantity;
    }
}