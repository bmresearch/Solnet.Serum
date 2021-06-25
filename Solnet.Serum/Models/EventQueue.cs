using Solnet.Serum.Models.Flags;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Event Queue in Serum.
    /// </summary>
    public class EventQueue : Queue
    {
        /// <summary>
        /// The flags that define the event type.
        /// </summary>
        public EventQueueFlags Flags;

        /// <summary>
        /// 
        /// </summary>
        public ulong NativeQuantityReleased;

        /// <summary>
        /// 
        /// </summary>
        public ulong NativeQuantityPaid;

        /// <summary>
        /// 
        /// </summary>
        public ulong NativeFeeOrRebate;

        /// <summary>
        /// Initialize the Event Queue with the given data.
        /// </summary>
        public EventQueue(ReadOnlySpan<byte> data) : base(data) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static EventQueue Deserialize(ReadOnlySpan<byte> data)
        {
            return new (data)
            {
                
            };
        }
    }
}