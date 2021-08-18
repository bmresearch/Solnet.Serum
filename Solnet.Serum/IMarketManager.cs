using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

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
        /// The decoded data of the underlying <see cref="OpenOrdersAccount"/>.
        /// </summary>
        OpenOrdersAccount OpenOrdersAccount { get; }

        /// <summary>
        /// The <see cref="TokenAccount"/> of the base token.
        /// </summary>
        TokenAccount BaseAccount { get; }

        /// <summary>
        /// The <see cref="TokenAccount"/> of the quote token.
        /// </summary>
        TokenAccount QuoteAccount { get; }

        /// <summary>
        /// The currently open orders for the associated <see cref="Market"/>.
        /// </summary>
        IList<OpenOrder> OpenOrders { get; }

        /// <summary>
        /// Subscribe to the live <see cref="TradeEvent"/> feed of this market.
        /// </summary>
        /// <param name="action">An action used to receive the list of occurred trade events and the respective slot.</param>
        void SubscribeTrades(Action<IList<TradeEvent>, ulong> action);

        /// <summary>
        /// Unsubscribe to the live <see cref="TradeEvent"/> feed of this market.
        /// </summary>
        void UnsubscribeTrades();

        /// <summary>
        /// Subscribe to the <see cref="OrderBook"/> of this market.
        /// </summary>
        /// <param name="action">An action used to receive the order book and the respective slot.</param>
        void SubscribeOrderBook(Action<OrderBook, ulong> action);

        /// <summary>
        /// Unsubscribe to the <see cref="OrderBook"/> of this market.
        /// </summary>
        void UnsubscribeOrderBook();

        /// <summary>
        /// Subscribe to the <see cref="OpenOrdersAccount"/> of this market.
        /// </summary>
        /// <param name="action">An action used to receive the open orders account and the respective slot.</param>
        void SubscribeOpenOrders(Action<IList<OpenOrder>, ulong> action);

        /// <summary>
        /// Unsubscribe to the order book of this market.
        /// </summary>
        void UnsubscribeOpenOrders();

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// </summary>
        /// <param name="order">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        SignatureConfirmation NewOrder(Order order);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="order">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> NewOrderAsync(Order order);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// </summary>
        /// <param name="side">The side of the order.</param>
        /// <param name="type">The type of the order.</param>
        /// <param name="selfTradeBehavior">The self-trade behavior of the order.</param>
        /// <param name="size">The size of the order.</param>
        /// <param name="price">The price of the order.</param>
        /// <param name="clientId">The client id of the order.</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        SignatureConfirmation NewOrder(Side side, OrderType type, SelfTradeBehavior selfTradeBehavior, float size,
            float price, ulong clientId = ulong.MaxValue);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="side">The side of the order.</param>
        /// <param name="type">The type of the order.</param>
        /// <param name="selfTradeBehavior">The self-trade behavior of the order.</param>
        /// <param name="size">The size of the order.</param>
        /// <param name="price">The price of the order.</param>
        /// <param name="clientId">The client id of the order.</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> NewOrderAsync(Side side, OrderType type, SelfTradeBehavior selfTradeBehavior,
            float size, float price, ulong clientId = ulong.MaxValue);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// </summary>
        /// <param name="orders">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        IList<SignatureConfirmation> NewOrders(IList<Order> orders);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="order">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<IList<SignatureConfirmation>> NewOrdersAsync(IList<Order> order);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction to cancel an order with the given order id.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        SignatureConfirmation CancelOrder(BigInteger orderId);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction to cancel an order with the given order id.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> CancelOrderAsync(BigInteger orderId);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction to cancel an order with the given client id.
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        SignatureConfirmation CancelOrder(ulong clientId);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction to cancel an order with the given client id.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> CancelOrderAsync(ulong clientId);

        /// <summary>
        /// Submits transaction(s) to cancel all currently open orders for the <see cref="Market"/> and <see cref="OpenOrdersAccount"/> associated with this <see cref="MarketManager"/> instance.
        /// </summary>
        /// <returns>A list of <see cref="SignatureConfirmation"/> objects, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        IList<SignatureConfirmation> CancelAllOrders();

        /// <summary>
        /// Crafts, requests signatures and submits transaction(s) to cancel all currently open orders for the <see cref="Market"/> and <see cref="OpenOrdersAccount"/> associated with this <see cref="MarketManager"/> instance.
        /// This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which may return a list of <see cref="SignatureConfirmation"/> objects, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<IList<SignatureConfirmation>> CancelAllOrdersAsync();
        
        /// <summary>
        /// Submits a transaction to settle funds associated with the accounts pertaining to this <see cref="Market"/>.
        /// </summary>
        /// <param name="referrer">The <see cref="PublicKey"/> of the referrer's quote token account.</param>
        /// <returns>A task which may return a list of <see cref="SignatureConfirmation"/> objects, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>

        SignatureConfirmation SettleFunds(PublicKey referrer = null);

        /// <summary>
        /// Submits a transaction to settle funds associated with the accounts pertaining to this <see cref="Market"/>.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="referrer">The <see cref="PublicKey"/> of the referrer's quote token account.</param>
        /// <returns>A task which may return a list of <see cref="SignatureConfirmation"/> objects, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> SettleFundsAsync(PublicKey referrer = null);
    }
}