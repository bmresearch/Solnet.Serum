// unset

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Order in Serum, this class is built using the <see cref="OrderBuilder"/>.
    /// </summary>
    public class Order : OrderBase
    {
        /// <summary>
        /// The order's side.
        /// </summary>
        public Side Side;
        
        /// <summary>
        /// The order's self trade behavior.
        /// </summary>
        public SelfTradeBehavior SelfTradeBehavior;

        /// <summary>
        /// The order's type.
        /// </summary>
        public OrderType Type;

        /// <summary>
        /// The order's price.
        /// </summary>
        public float Price;

        /// <summary>
        /// The order's size.
        /// </summary>
        public float Quantity;

        /// <summary>
        /// The order's maximum quote quantity.
        /// </summary>
        public ulong MaxQuoteQuantity;
    }
}