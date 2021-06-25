using System;

namespace Solnet.Serum.Models.Flags
{
    /// <summary>
    /// Represents the event queue's flags.
    /// </summary>
    public class EventQueueFlags : Flag
    {
        #region Flag Mask Values

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int Fill = 1;

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int Out = 2;
        
        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int Bid = 4;

        /// <summary>
        /// The value to check against to see if the 
        /// </summary>
        private const int Maker = 8;

        #endregion

        /// <summary>
        /// Whether the event is a fill or not.
        /// </summary>
        public bool IsFill => (Bitmask & Fill) == Fill;

        /// <summary>
        /// Whether the event is an output or not.
        /// </summary>
        public bool IsOut => (Bitmask & Out) == Out;

        /// <summary>
        /// Whether the event is a bid or not.
        /// </summary>
        public bool IsBid => (Bitmask & Bid) == Bid;

        /// <summary>
        /// Whether the event is a maker or not.
        /// </summary>
        public bool IsMaker => (Bitmask & Maker) == Maker;

        /// <summary>
        /// Initialize the event queue flags with the given bit mask.
        /// </summary>
        /// <param name="bitmask">The bit mask.</param>
        public EventQueueFlags(byte bitmask) : base(bitmask) { }
        
        /// <summary>
        /// Deserialize a span of bytes into a <see cref="EventQueueFlags"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The EventQueueFlags structure.</returns>
        internal static EventQueueFlags Deserialize(ReadOnlySpan<byte> data) 
            =>  new (data[0]);
    }
}