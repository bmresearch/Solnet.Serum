using Solnet.Serum.Models.Flags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an OpenOrder Book in Serum.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Represents the layout of the <see cref="OrderBook"/> data structure.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The number of bytes of the padding at the beginning of the OpenOrder Book structure.
            /// </summary>
            internal const int StartPadding = 5;

            /// <summary>
            /// The number of bytes of the padding at the end of the OpenOrder Book structure.
            /// </summary>
            internal const int EndPadding = 7;

            /// <summary>
            /// The offset at which the slab layout begins.
            /// </summary>
            internal const int SlabLayoutOffset = 8;
        }

        /// <summary>
        /// The flags that define the account type.
        /// </summary>
        public AccountFlags Flags;

        /// <summary>
        /// The slab structure of the order book.
        /// </summary>
        public Slab Slab;

        /// <summary>
        /// Gets the list of orders in the order book.
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOrders()
        {
            return (from slabNode in Slab.Nodes
                where slabNode is SlabLeafNode
                select (SlabLeafNode)slabNode
                into slabLeafNode
                select new Order
                {
                    Price = slabLeafNode.Price, 
                    Quantity = slabLeafNode.Quantity, 
                    ClientOrderId = slabLeafNode.ClientOrderId, 
                    Owner = slabLeafNode.Owner,
                }).ToList();
        }
        
        
        /// <summary>
        /// Deserialize a span of bytes into a <see cref="OrderBook"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The OrderBook structure.</returns>
        public static OrderBook Deserialize(ReadOnlySpan<byte> data)
        {
            ReadOnlySpan<byte> padLessData = data.Slice(
                Layout.StartPadding,
                data.Length - (Layout.StartPadding + Layout.EndPadding));

            return new OrderBook
            {
                Flags = AccountFlags.Deserialize(padLessData[..8]),
                Slab = Slab.Deserialize(padLessData[8..])
            };
        }        
    }
}