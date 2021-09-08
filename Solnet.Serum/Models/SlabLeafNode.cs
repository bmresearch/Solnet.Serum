using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a leaf node of the <see cref="Slab"/> structure, this node has order data.
    /// </summary>
    public class SlabLeafNode : SlabNode
    {
        /// <summary>
        /// Represents the layout of the <see cref="SlabLeafNode"/> data structure.
        /// </summary>
        internal static new class Layout
        {
            /// <summary>
            /// The length of the key parameter.
            /// </summary>
            internal const int KeyLength = 16;
            
            /// <summary>
            /// 
            /// </summary>
            internal const int OwnerSlotOffset = 0;

            /// <summary>
            /// 
            /// </summary>
            internal const int FeeTierOffset = 1;

            /// <summary>
            /// The offset at which the price value of the order in the book begins.
            /// <remarks>This value is only valid after reading the <c>Key</c> value.</remarks>
            /// </summary>
            internal const int PriceOffset = 8;

            /// <summary>
            /// The offset at which the order's owner begins.
            /// </summary>
            internal const int OwnerOffset = 20;

            /// <summary>
            /// The offset at which the order's quantity begins.
            /// </summary>
            internal const int QuantityOffset = 52;

            /// <summary>
            /// The offset at which the order's client id begins.
            /// </summary>
            internal const int ClientOrderIdOffset = 60;
        }
        
        /// <summary>
        /// The slot of the owner.
        /// </summary>
        public byte OwnerSlot;

        /// <summary>
        /// The order's fee tier.
        /// </summary>
        public byte FeeTier;

        /// <summary>
        /// The owner of the order.
        /// </summary>
        public PublicKey Owner;

        /// <summary>
        /// The quantity of the order.
        /// </summary>
        public ulong Quantity;

        /// <summary>
        /// The client's order id.
        /// </summary>
        public ulong ClientOrderId;
        
        /// <summary>
        /// The price of the order.
        /// </summary>
        public ulong Price;
        
        /// <summary>
        /// Deserialize a span of bytes into a <see cref="SlabNode"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The SlabNode structure.</returns>
        public static new SlabLeafNode Deserialize(ReadOnlySpan<byte> data)
        {
            Span<byte> key = data.GetSpan(SlabNode.Layout.KeyOffset, Layout.KeyLength);
            ulong price = ((ReadOnlySpan<byte>)key).GetU64(Layout.PriceOffset);

            return new SlabLeafNode
            {
                OwnerSlot = data.GetU8(Layout.OwnerSlotOffset),
                FeeTier = data.GetU8(Layout.FeeTierOffset),
                Key = key.ToArray(),
                Price = price,
                Owner = data.GetPubKey(Layout.OwnerOffset),
                Quantity = data.GetU64(Layout.QuantityOffset),
                ClientOrderId = data.GetU64(Layout.ClientOrderIdOffset)
            };
        }
    }
}