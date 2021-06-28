using Solnet.Serum.Layouts;
using Solnet.Serum.Models.Flags;
using Solnet.Wallet;
using System;
using System.Buffers.Binary;
using System.Linq;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Event in Serum.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The flags that define the event type.
        /// </summary>
        public EventFlags Flags;

        /// <summary>
        /// The open order's slot.
        /// </summary>
        public byte OpenOrderSlot;

        /// <summary>
        /// The fee tier.
        /// </summary>
        public byte FeeTier;

        /// <summary>
        /// The order id.
        /// </summary>
        public byte[] OrderId;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientOrderId;

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
        /// 
        /// </summary>
        public PublicKey PublicKey;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="EventQueue"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Event Queue structure.</returns>
        public static Event Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != EventDataLayout.EventSpanLength)
                return null;

            EventFlags flags = EventFlags.Deserialize(data[..1]);
            
            return new Event
            {
                Flags = flags,
                OpenOrderSlot = data[EventDataLayout.OpenOrderSlotOffset],
                FeeTier = data[EventDataLayout.FeeTierOffset],
                NativeQuantityReleased = BinaryPrimitives.ReadUInt64LittleEndian(
                    data.Slice(EventDataLayout.NativeQuantityReleasedOffset, 8)),
                NativeQuantityPaid = BinaryPrimitives.ReadUInt64LittleEndian(
                    data.Slice(EventDataLayout.NativeQuantityPaidOffset, 8)),
                NativeFeeOrRebate = BinaryPrimitives.ReadUInt64LittleEndian(
                    data.Slice(EventDataLayout.NativeFeeOrRebateOffset, 8)),
                OrderId = data.Slice(EventDataLayout.OrderIdOffset, 16).ToArray(),
                PublicKey = 
                    new PublicKey(data.Slice(EventDataLayout.PublicKeyOffset, 32).ToArray()),
                ClientOrderId = BinaryPrimitives.ReadUInt64LittleEndian(
                    data.Slice(EventDataLayout.ClientOrderIdOffset, 8))
            };
        }
    }
}