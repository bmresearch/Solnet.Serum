namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a trade event in Serum.
    /// </summary>
    public class TradeEvent
    {
        /// <summary>
        /// The side of the trade.
        /// </summary>
        public Side Side;

        /// <summary>
        /// The amount traded.
        /// </summary>
        public float Size;

        /// <summary>
        /// The price at which the trade occurred.
        /// </summary>
        public float Price;

        /// <summary>
        /// The underlying event that triggered the trade.
        /// </summary>
        public Event Event;
    }
}