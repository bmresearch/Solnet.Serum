// unset

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the possible behaviors of an order when trading against one's own orders.
    /// </summary>
    public enum SelfTradeBehavior
    {
        /// <summary>
        /// Decrements the order size.
        /// </summary>
        DecrementTake = 0,
        
        /// <summary>
        /// Cancels the liquidity provision.
        /// </summary>
        CancelProvide = 1,
        
        /// <summary>
        /// Aborts the transaction.
        /// </summary>
        AbortTransaction = 2,
    }
}