namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the order type.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// A limit order.
        /// </summary>
        Limit = 0,
        
        /// <summary>
        /// An order which is immediately filled or cancelled.
        /// </summary>
        ImmediateOrCancel = 1,
        
        /// <summary>
        /// The order is a post only order.
        /// </summary>
        PostOnly = 2,
    }
}