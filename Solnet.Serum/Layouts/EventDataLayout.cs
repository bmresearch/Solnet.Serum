using Solnet.Serum.Models;

namespace Solnet.Serum.Layouts
{
    /// <summary>
    /// Represents the layout of the <see cref="Event"/> data structure.
    /// </summary>
    internal static class EventDataLayout
    {
        /// <summary>
        /// The size of the data for an event queue account.
        /// </summary>
        internal const int EventSpanLength = 88;
        
        /// <summary>
        /// The offset at which the value of the Open Order Slot begins.
        /// </summary>
        internal const int OpenOrderSlotOffset = 1;

        /// <summary>
        /// The offset at which the value of the Fee Tier begins.
        /// </summary>
        internal const int FeeTierOffset = 2;

        /// <summary>
        /// The offset at which the value of the Native Quantity Released begins.
        /// </summary>
        internal const int NativeQuantityReleasedOffset = 8;

        /// <summary>
        /// The offset at which the value of the Native Quantity Paid begins.
        /// </summary>
        internal const int NativeQuantityPaidOffset = 16;

        /// <summary>
        /// The offset at which the value of the Native Fee or Rebate begins.
        /// </summary>
        internal const int NativeFeeOrRebateOffset = 24;

        /// <summary>
        /// The offset at which the value of the Order Id begins.
        /// </summary>
        internal const int OrderIdOffset = 32;

        /// <summary>
        /// The offset at which the value of the Public Key begins.
        /// </summary>
        internal const int PublicKeyOffset = 48;

        /// <summary>
        /// The offset at which the value of the Client Order Id begins.
        /// </summary>
        internal const int ClientOrderIdOffset = 80;
    }
}