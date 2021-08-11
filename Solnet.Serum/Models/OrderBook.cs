// unset

using System.Collections.Generic;
using System.Linq;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the order book of a Serum <see cref="Market"/>.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// The bid side of the order book.
        /// </summary>
        public OrderBookSide Bids;

        /// <summary>
        /// The ask side of the order book.
        /// </summary>
        public OrderBookSide Asks;

        /// <summary>
        /// The lot size of the quote token.
        /// </summary>
        internal ulong QuoteLotSize;

        /// <summary>
        /// The lot size of the base token.
        /// </summary>
        internal ulong BaseLotSize;

        /// <summary>
        /// The decimals of the base token.
        /// </summary>
        internal byte BaseDecimals;

        /// <summary>
        /// The decimals of the quote token.
        /// </summary>
        internal byte QuoteDecimals;

        /// <summary>
        /// Gets the orders in the bid side of the order book.
        /// </summary>
        /// <returns>A list of orders.</returns>
        public List<Order> GetBids()
        {
            if (Bids == null) return new List<Order>();
            List<Order> orders = (from slabNode in Bids.Slab.Nodes
                            where slabNode is SlabLeafNode
                            select (SlabLeafNode)slabNode
                            into slabLeafNode
                            select new Order
                            {
                                Side = Side.Buy,
                                Price =
                                    MarketUtils.PriceLotsToNumber(slabLeafNode.Price, BaseDecimals, QuoteDecimals, BaseLotSize,
                                        QuoteLotSize),
                                Quantity = MarketUtils.QuantityLotsToNumber(slabLeafNode.Quantity, BaseLotSize, BaseDecimals),
                                RawPrice = slabLeafNode.Price,
                                RawQuantity = slabLeafNode.Quantity,
                                ClientOrderId = slabLeafNode.ClientOrderId,
                                Owner = slabLeafNode.Owner,
                            }).ToList();
            orders.Sort(Comparer<Order>.Create((order, order1) => order1.RawPrice.CompareTo(order.RawPrice)));
            return orders;
        }

        /// <summary>
        /// Gets the orders in the bid side of the order book.
        /// </summary>
        /// <returns>A list of orders.</returns>
        public List<Order> GetAsks()
        {
            if (Asks == null) return new List<Order>();
            List<Order> orders = 
                (from slabNode in Asks.Slab.Nodes
                    where slabNode is SlabLeafNode
                    select (SlabLeafNode)slabNode
                    into slabLeafNode
                    select new Order
                    {
                        Side = Side.Sell,
                        Price =
                            MarketUtils.PriceLotsToNumber(slabLeafNode.Price, BaseDecimals, QuoteDecimals, BaseLotSize,
                                QuoteLotSize),
                        Quantity = MarketUtils.QuantityLotsToNumber(slabLeafNode.Quantity, BaseLotSize, BaseDecimals),
                        RawPrice = slabLeafNode.Price,
                        RawQuantity = slabLeafNode.Quantity,
                        ClientOrderId = slabLeafNode.ClientOrderId,
                        Owner = slabLeafNode.Owner,
                    }).ToList();
            orders.Sort(Comparer<Order>.Create((order, order1) => order.RawPrice.CompareTo(order1.RawPrice)));
            return orders;
        }
    }
}