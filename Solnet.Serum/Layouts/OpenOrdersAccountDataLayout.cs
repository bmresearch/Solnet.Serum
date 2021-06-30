using Solnet.Serum.Models;

namespace Solnet.Serum.Layouts
{
    /// <summary>
    /// Represents the layout of the <see cref="OpenOrdersAccount"/> data structure.
    /// </summary>
    internal static class OpenOrdersAccountDataLayout
    {
        /// <summary>
        /// The offset at which the Market value begins.
        /// </summary>
        internal const int MarketOffset = 13;

        /// <summary>
        /// The offset at which the Owner value begins.
        /// </summary>
        internal const int OwnerOffset = 45;

        /// <summary>
        /// The offset at which the BaseTokenFree value begins.
        /// </summary>
        internal const int BaseTokenFreeOffset = 78;

        /// <summary>
        /// The offset at which the BaseTokenTotal value begins.
        /// </summary>
        internal const int BaseTokenTotalOffset = 86;

        /// <summary>
        /// The offset at which the QuoteTokenFree value begins.
        /// </summary>
        internal const int QuoteTokenFreeOffset = 94;

        /// <summary>
        /// The offset at which the QuoteTokenTotal value begins.
        /// </summary>
        internal const int QuoteTokenTotalOffset = 102;

        /// <summary>
        /// The offset at which the FreeSlotBids value begins.
        /// </summary>
        internal const int FreeSlotBitsOffset = 110;

        /// <summary>
        /// The offset at which the IsBidBit value begins.
        /// </summary>
        internal const int IsBidBitsOffset = 126;

        /// <summary>
        /// The offset at which the Orders values begin.
        /// </summary>
        internal const int OrdersOffset = 142;

        /// <summary>
        /// The offset at which the ClientIds values begin.
        /// </summary>
        internal const int ClientIdsOffset = 2190;
    }
}