using Solnet.Programs.Utilities;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a node in the <see cref="Slab"/> structure.
    /// </summary>
    public abstract class SlabNode
    {
        #region Layout

        /// <summary>
        /// Represents the layout of the <see cref="SlabNode"/> data structure.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The offset at which the slab node's blob starts.
            /// <remarks>
            /// This value is valid before reading the Tag property of the <see cref="SlabNode"/>.
            /// </remarks>
            /// </summary>
            internal const int BlobOffset = 4;

            /// <summary>
            /// The offset at which the Key value begins.
            /// <remarks>
            /// This value is ONLY valid after reading the Tag property of the <see cref="SlabNode"/>,
            /// while reading either a <see cref="SlabInnerNode"/> or a <see cref="SlabLeafNode"/>.
            /// </remarks>
            /// </summary>
            internal const int KeyOffset = 4;

            /// <summary>
            /// The size of the data for the slab node's blob.
            /// </summary>
            internal const int BlobSpanLength = 68;
        }

        #endregion

        /// <summary>
        /// The tag that defines this node's type.
        /// </summary>
        public int Tag;

        /// <summary>
        /// The data content of this node, defined by it's tag.
        /// </summary>
        public byte[] Blob;

        /// <summary>
        /// The key of the order.
        /// </summary>
        public byte[] Key;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="SlabNode"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The SlabNode structure.</returns>
        public static SlabNode Deserialize(ReadOnlySpan<byte> data)
        {
            uint tag = data.GetU32(0);
            if (tag is (byte)NodeType.Uninitialized or (byte)NodeType.LastFreeNode or (byte)NodeType.FreeNode)
                return null;

            ReadOnlySpan<byte> blob = data.GetSpan(Layout.BlobOffset, Layout.BlobSpanLength);

            return tag switch
            {
                (byte)NodeType.InnerNode => SlabInnerNode.Deserialize(blob),
                (byte)NodeType.LeafNode => SlabLeafNode.Deserialize(blob)
            };
        }
    }
}