using System;

namespace Solnet.Serum.Models.Flags
{
    /// <summary>
    /// Represents the request queue's flags.
    /// </summary>
    public class RequestQueueFlags : Flag
    {
        #region Flag Mask Values

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int NewOrder = 1;

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int CancelOrder = 2;
        
        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int Bid = 4;

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int PostOnly = 8;
        
        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int ImmediateOrCancel = 16;

        #endregion
        
        /// <summary>
        /// Whether the event is a new order.
        /// </summary>
        public bool IsNewOrder => (Bitmask & NewOrder) == NewOrder;

        /// <summary>
        /// Whether the event is an order cancellation.
        /// </summary>
        public bool IsCancelOrder => (Bitmask & CancelOrder) == CancelOrder;

        /// <summary>
        /// Whether the event is related to a <see cref="OrderType.Limit"/> order.
        /// </summary>
        public bool IsBid => (Bitmask & Bid) == Bid;

        /// <summary>
        /// Whether the event is related to a <see cref="OrderType.PostOnly"/> order.
        /// </summary>
        public bool IsPostOnly => (Bitmask & PostOnly) == PostOnly;
        
        /// <summary>
        /// Whether the event is related to an <see cref="OrderType.ImmediateOrCancel"/> order.
        /// </summary>
        public bool IsImmediateOrCancel => (Bitmask & ImmediateOrCancel) == ImmediateOrCancel;
        
        /// <summary>
        /// Initialize the request queue flags with the given bit mask.
        /// </summary>
        /// <param name="bitmask">The bit mask.</param>
        public RequestQueueFlags(byte bitmask) : base(bitmask) { }
        
        /// <summary>
        /// Deserialize a span of bytes into a <see cref="RequestQueueFlags"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The RequestQueueFlags structure.</returns>
        internal static RequestQueueFlags Deserialize(ReadOnlySpan<byte> data) 
            =>  new (data[0]);
    }
}