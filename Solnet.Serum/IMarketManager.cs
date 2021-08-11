using Solnet.Serum.Models;
using System;
using System.Collections.Generic;

namespace Solnet.Serum
{
    /// <summary>
    /// The <see cref="MarketManager"/> interface.
    /// </summary>
    public interface IMarketManager
    {
        /// <summary>
        /// The decoded data of the underlying <see cref="Market"/>.
        /// </summary>
        Market Market { get; }

        /// <summary>
        /// Subscribe to the trades feed of this market.
        /// </summary>
        /// <param name="action">An action used to receive the list of trades and the respective slot.</param>
        void SubscribeTrades(Action<List<TradeEvent>, ulong> action);
        
        /// <summary>
        /// Subscribe to the order book of this market.
        /// </summary>
        /// <param name="action">An action used to receive the order book and the respective slot.</param>
        void SubscribeOrderBook(Action<OrderBook, ulong> action);

        /// <summary>
        /// Unsubscribe to the order book of this market.
        /// </summary>
        void UnsubscribeTrades();

        /// <summary>
        /// Unsubscribe to the order book of this market.
        /// </summary>
        void UnsubscribeOrderBook();
    }
}