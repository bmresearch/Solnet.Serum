using Solnet.Programs.Utilities;
using Solnet.Serum.Models.Flags;
using Solnet.Wallet;
using System;
using System.Numerics;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Event in Serum.
    /// </summary>
    public class Event
    {
        #region Layout
        
        /// <summary>
        /// Represents the layout of the <see cref="Event"/> data structure.
        /// </summary>
        internal static class Layout
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
        
        #endregion
        
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
        public BigInteger OrderId;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientOrderId;

        /// <summary>
        /// The native quantity released due to this order.
        /// </summary>
        public ulong NativeQuantityReleased;

        /// <summary>
        /// The native quantity paid by this order.
        /// </summary>
        public ulong NativeQuantityPaid;

        /// <summary>
        /// The native fee or rebate of this order.
        /// </summary>
        public ulong NativeFeeOrRebate;

        /// <summary>
        /// The public key of the open order's account.
        /// </summary>
        public PublicKey PublicKey;

        /// <summary>
        /// The event's sequence number.
        /// <remarks>This is used to allow to process only the most recent events.</remarks>
        /// </summary>
        public long? SequenceNumber;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="EventQueue"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Event Queue structure.</returns>
        public static Event Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != Layout.EventSpanLength)
                return null;

            EventFlags flags = EventFlags.Deserialize(data[..1]);
            
            return new Event
            {
                Flags = flags,
                OpenOrderSlot = data[Layout.OpenOrderSlotOffset],
                FeeTier = data[Layout.FeeTierOffset],
                NativeQuantityReleased = data.GetU64(Layout.NativeQuantityReleasedOffset),
                NativeQuantityPaid = data.GetU64(Layout.NativeQuantityPaidOffset),
                NativeFeeOrRebate = data.GetU64(Layout.NativeFeeOrRebateOffset),
                OrderId = data.GetBigInt(Layout.OrderIdOffset, 16),
                PublicKey = data.GetPubKey(Layout.PublicKeyOffset),
                ClientOrderId = data.GetU64(Layout.ClientOrderIdOffset)
            };
        }
    }
}