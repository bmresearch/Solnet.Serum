using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a Queue in Serum.
    /// <remarks>
    /// This holds attributes that are common between the <see cref="EventQueue"/> and the <see cref="RequestQueue"/>.
    /// </remarks>
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// The size of the data for the event queue.
        /// </summary>
        private int EventQueueSpan = 88;

        /// <summary>
        /// The size of the data for the event queue.
        /// </summary>
        private int RequestQueueSpan = 80;

        /// <summary>
        /// The open order's slot.
        /// </summary>
        public sbyte OpenOrderSlot;

        /// <summary>
        /// The fee tier.
        /// </summary>
        public sbyte FeeTier;

        /// <summary>
        /// The order id.
        /// </summary>
        public byte[] OrderId;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientOrderId;

        /// <summary>
        /// Initialize the queue with the given data.
        /// </summary>
        /// <param name="data">The data.</param>
        protected Queue(ReadOnlySpan<byte> data)
        {
            if (data.Length == EventQueueSpan)
                ReadEventQueue(data);
            
            if (data.Length == RequestQueueSpan)
                ReadRequestQueue(data);
        }

        /// <summary>
        /// Read the data using the event queue layout.
        /// </summary>
        /// <param name="data">The data.</param>
        private void ReadEventQueue(ReadOnlySpan<byte> data)
        {
            
        }

        /// <summary>
        /// Read the data using the request queue layout.
        /// </summary>
        /// <param name="data">The data.</param>
        private void ReadRequestQueue(ReadOnlySpan<byte> data)
        {
            
        }
    }
}