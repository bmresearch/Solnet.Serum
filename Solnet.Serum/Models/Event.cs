using System;
using Solnet.Wallet;
using Solnet.Serum.Shared;

namespace Solnet.Serum.Models
{
    //===========================================================
    // Event Model
    //===========================================================
    public class Event
    {
        public const int SerializedLength = 88;  // Total (serialized) size of this class

        public static class Layout
        {
            public const int Flags             =  0;
            public const int OpenOrderSlot     =  1;
            public const int FeeTier           =  2;
            public const int NativeQtyReleased =  8;
            public const int NativeQtyPaid     = 16;
            public const int NativeFeeOrRebate = 24;
            public const int OrderId           = 32;
            public const int PublicKey         = 48;
            public const int ClientOrderId     = 80;
        }

        public EventFlags  Flags;              // The flags that define the event type.
        public byte        OpenOrderSlot;      // The open order's slot.
        public byte        FeeTier;            // The fee tier.
        public OrderId     OrderId;            // Order Id
        public ulong       ClientOrderId;      // The client's order id.
        public ulong       NativeQtyReleased;  
        public ulong       NativeQtyPaid;
        public ulong       NativeFeeOrRebate;
        public PublicKey   PublicKey;

        // Deserialize a span of bytes into an 'Event'
        public static Event Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != SerializedLength) { return null; }
            return new Event
            {
                Flags              = data.GetU8       (Layout.Flags), 
                OpenOrderSlot      = data.GetU8       (Layout.OpenOrderSlot),
                FeeTier            = data.GetU8       (Layout.FeeTier),
                NativeQtyReleased  = data.GetU64      (Layout.NativeQtyReleased),
                NativeQtyPaid      = data.GetU64      (Layout.NativeQtyPaid),
                NativeFeeOrRebate  = data.GetU64      (Layout.NativeFeeOrRebate),
                OrderId            = data.GetOrderId  (Layout.OrderId),
                PublicKey          = data.GetPublicKey(Layout.PublicKey),
                ClientOrderId      = data.GetU64      (Layout.ClientOrderId)
            };
        }
    }
}
