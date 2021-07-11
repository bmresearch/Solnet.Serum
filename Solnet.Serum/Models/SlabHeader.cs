using Solnet.Programs.Utilities;
using Solnet.Serum.Models.Flags;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a Slab Header in the <see cref="OrderBook"/> data structure.
    /// </summary>
    public class SlabHeader
    {
        /// <summary>
        /// Represents the layout of the <see cref="SlabHeader"/> data structure.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The size of the data of the Slab Header.
            /// </summary>
            internal const int SpanLength = 32;

            /// <summary>
            /// The offset at which the BumpIndex value begins.
            /// <remarks>
            /// This value is only valid because the first 13 bytes have been sliced out,
            /// 5 bytes representing the Start Padding and 8 bytes representing the <see cref="AccountFlags"/>.
            /// </remarks>
            /// </summary>
            internal const int BumpIndexOffset = 0;
            
            /// <summary>
            /// The offset at which the Free List Length value begins.
            /// </summary>
            internal const int FreeListLengthOffset = 8;
            
            /// <summary>
            /// The offset at which the Free List Head value begins.
            /// </summary>
            internal const int FreeListHeadOffset = 16;

            /// <summary>
            /// The offset at which the Root value begins.
            /// </summary>
            internal const int RootOffset = 20;
            
            /// <summary>
            /// The offset at which the Leaf Count value begins.
            /// </summary>
            internal const int LeafCountOffset = 24;
        }
        
        /// <summary>
        /// The bump index of the slab.
        /// </summary>
        public uint BumpIndex;

        /// <summary>
        /// The length of the free list of the slab.
        /// </summary>
        public uint FreeListLength;

        /// <summary>
        /// The free list head of the slab.
        /// </summary>
        public uint FreeListHead;

        /// <summary>
        /// The root element of the slab.
        /// </summary>
        public uint Root;

        /// <summary>
        /// The count of leaf nodes in the slab.
        /// </summary>
        public uint LeafCount;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="SlabHeader"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The SlabHeader structure.</returns>
        public static SlabHeader Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != Layout.SpanLength)
                return null;

            return new SlabHeader
            {
                BumpIndex = data.GetU32(Layout.BumpIndexOffset),
                FreeListLength = data.GetU32(Layout.FreeListLengthOffset),
                FreeListHead = data.GetU32(Layout.FreeListHeadOffset),
                Root = data.GetU32(Layout.RootOffset),
                LeafCount = data.GetU32(Layout.LeafCountOffset)
            };
        }
    }
}