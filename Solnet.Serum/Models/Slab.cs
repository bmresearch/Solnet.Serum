using Solnet.Programs.Utilities;
using System;
using System.Collections.Generic;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the Slab structure of the Serum <see cref="OrderBookSide"/>. 
    /// </summary>
    public class Slab
    {
        /// <summary>
        /// Represents the layout of the <see cref="Slab"/> data structure.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The offset at which the slab nodes begin.
            /// <remarks>This offset takes into account the removal of the paddings.</remarks>
            /// </summary>
            internal const int SlabNodesOffset = 32;
        }
        
        /// <summary>
        /// The header of the slab.
        /// </summary>
        public SlabHeader Header;

        /// <summary>
        /// The nodes that compose the slab.
        /// </summary>
        public IList<SlabNode> Nodes;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="SlabHeader"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The SlabHeader structure.</returns>
        public static Slab Deserialize(ReadOnlySpan<byte> data)
        {
            SlabHeader header = SlabHeader.Deserialize(data[..Layout.SlabNodesOffset]);
            List<SlabNode> slabNodes = new ((int) header.BumpIndex);

            ReadOnlySpan<byte> slabNodeBytes = data.Slice(Layout.SlabNodesOffset, data.Length - 38);

            for (int i = 0; i < header.BumpIndex; i++)
            {
                SlabNode slabNode = SlabNode.Deserialize(slabNodeBytes.Slice(72 * i, 72));
                if (slabNode == null) continue;
                slabNodes.Add(slabNode);
            }

            return new Slab()
            {
                Header = header,
                Nodes = slabNodes,
            };
        }
    }
}