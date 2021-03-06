namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Open OpenOrder in a Serum Open Orders Account.
    /// </summary>
    public class OpenOrder : OrderBase
    {
        /// <summary>
        /// Whether this slot within the <see cref="OpenOrdersAccount"/> Orders is free or not.
        /// </summary>
        public bool IsFreeSlot;

        /// <summary>
        /// Whether this order is a bit or not.
        /// </summary>
        public bool IsBid;
    }
}