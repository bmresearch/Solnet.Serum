// ReSharper disable InconsistentNaming
using Solnet.Programs.Abstract;
using System;


namespace Solnet.Serum.Models.Flags
{
    /// <summary>
    /// Represents the event queue's flags.
    /// </summary>
    public class EventFlags
    {
        /// <summary>
        /// The flag which specifies the event type.
        /// </summary>
        private readonly ByteFlag Flag;

        /// <summary>
        /// Whether the event is a fill or not.
        /// </summary>
        public bool IsFill => Flag.Bit0;

        /// <summary>
        /// Whether the event is an output or not.
        /// </summary>
        public bool IsOut => Flag.Bit1;

        /// <summary>
        /// Whether the event is a bid or not.
        /// </summary>
        public bool IsBid => Flag.Bit2;

        /// <summary>
        /// Whether the event is a maker or not.
        /// </summary>
        public bool IsMaker => Flag.Bit3;

        /// <summary>
        /// Whether the event is a release of funds or not.
        /// </summary>
        public bool IsReleaseFunds => Flag.Bit4;

        /// <summary>
        /// Initialize the event queue flags with the given bit mask.
        /// </summary>
        /// <param name="bitmask">The bit mask.</param>
        private EventFlags(byte bitmask)
        {
            Flag = new ByteFlag(bitmask);
        }
        
        /// <summary>
        /// Deserialize a span of bytes into a <see cref="EventFlags"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The EventFlags structure.</returns>
        internal static EventFlags Deserialize(ReadOnlySpan<byte> data) 
            =>  new (data[0]);
    }
}