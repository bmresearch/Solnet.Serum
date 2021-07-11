using Solnet.Programs.Utilities;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a leaf node of the <see cref="Slab"/> structure, this node has child information.
    /// </summary>
    public class SlabInnerNode : SlabNode
    {
        /// <summary>
        /// Represents the layout of the <see cref="SlabInnerNode"/> data structure.
        /// </summary>
        internal static new class Layout
        {
            /// <summary>
            /// The offset at which the PrefixLength value begins.
            /// </summary>
            internal const int PrefixLengthOffset = 0;
            
            /// <summary>
            /// The offset at which the Children values begin.
            /// </summary>
            internal const int Children1Offset = 20;
            
            /// <summary>
            /// The offset at which the Children values begin.
            /// </summary>
            internal const int Children2Offset = 24;
        }
        
        /// <summary>
        /// The prefix length of the node.
        /// </summary>
        public uint PrefixLength;

        /// <summary>
        /// The first child of this node.
        /// </summary>
        public uint Child1;

        /// <summary>
        /// The second child of this node.
        /// </summary>
        public uint Child2;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="SlabNode"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The SlabNode structure.</returns>
        public static new SlabInnerNode Deserialize(ReadOnlySpan<byte> data)
        {
            // Only the first prefixLength high-order bits of the key are meaningful
            uint prefixLength = data.GetU32(Layout.PrefixLengthOffset);
            int bytesToRead = (int) Math.Ceiling(prefixLength / 4.00);

            return new SlabInnerNode
            {
                PrefixLength = prefixLength,
                Key = data.GetSpan(SlabNode.Layout.KeyOffset, bytesToRead).ToArray(),
                Child1 = data.GetU32(Layout.Children1Offset),
                Child2 = data.GetU32(Layout.Children2Offset)
            };
        }
    }
}