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
        /// <remarks>
        /// If this value is <c>null</c>, the <see cref="IMarketManager"/> may need to be refreshed by calling <see cref="IMarketManager.ReloadAsync"/>.
        /// The default implementation of <see cref="IMarketManager"/>, <see cref="MarketManager"/>, does not automatically and/or periodically update this structure, it will be up to the user to do so.
        /// </remarks>
        /// </summary>
        OpenOrdersAccount OpenOrdersAccount { get; }
        
        /// <summary>
        /// The public key of the underlying <see cref="OpenOrdersAccount"/>.
        /// </summary>
        PublicKey OpenOrdersAddress { get; }

        /// <summary>
        /// The <see cref="TokenAccount"/> of the base token.
        /// <remarks>
        /// If this value is <c>null</c>, the <see cref="IMarketManager"/> may need to be refreshed by calling <see cref="IMarketManager.ReloadAsync"/>.
        /// The default implementation of <see cref="IMarketManager"/>, <see cref="MarketManager"/>, does not automatically and/or periodically update this structure, it will be up to the user to do so.
        /// </remarks>
        /// </summary>
        TokenAccount BaseAccount { get; }
        
        /// <summary>
        /// The public key of the underlying base <see cref="TokenAccount"/>.
        /// </summary>
        PublicKey BaseTokenAccountAddress { get; }

        /// <summary>
        /// The <see cref="TokenAccount"/> of the quote token.
        /// <remarks>
        /// If this value is <c>null</c>, the <see cref="IMarketManager"/> may need to be refreshed by calling <see cref="IMarketManager.ReloadAsync"/>.
        /// The default implementation of <see cref="IMarketManager"/>, <see cref="MarketManager"/>, does not automatically and/or periodically update this structure, it will be up to the user to do so.
        /// </remarks>
        /// </summary>
        TokenAccount QuoteAccount { get; }
        
        /// <summary>
        /// The public key of the underlying quote <see cref="TokenAccount"/>.
        /// </summary>
        PublicKey QuoteTokenAccountAddress { get; }

        /// <summary>
        /// The decimals of the base token for the associated <see cref="Market"/>.
        /// </summary>
        byte BaseDecimals { get; }
        
        /// <summary>
        /// The decimals of the quote token for the associated <see cref="Market"/>.
        /// </summary>
        byte QuoteDecimals { get; }
        
        /// <summary>
        /// The currently open orders for the associated <see cref="Market"/>.
        /// </summary>
        IList<OpenOrder> OpenOrders { get; }

        /// <summary>
        /// Initializes the market manager.
        /// </summary>
        void Init();
        
        /// <summary>
        /// Initializes the market manager.
        /// This is an asynchronous operation.
        /// </summary>
        Task InitAsync();
        
        /// <summary>
        /// Reloads the associated token accounts and open orders accounts. Useful when an order creates either accounts.
        /// This is an asynchronous operation.
        /// </summary>
        Task ReloadAsync();
        
        /// <summary>
        /// Reloads the associated token accounts and open orders accounts. Useful when an order creates either accounts.
        /// </summary>
        void Reload();

        /// <summary>
        /// Subscribe to the live <see cref="TradeEvent"/> feed of this market.
        /// </summary>
        /// <param name="action">An action used to receive the list of occurred trade events and the respective slot.</param>
        void SubscribeTrades(Action<IList<TradeEvent>, ulong> action);
        
        /// <summary>
        /// Subscribe to the live <see cref="TradeEvent"/> feed of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action used to receive the list of occurred trade events and the respective slot.</param>
        /// <returns>A task which performs the operation.</returns>
        Task SubscribeTradesAsync(Action<IList<TradeEvent>, ulong> action);

        /// <summary>
        /// Unsubscribe to the live <see cref="TradeEvent"/> feed of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which performs the operation.</returns>
        Task UnsubscribeTradesAsync();

        /// <summary>
        /// Subscribe to the <see cref="OrderBook"/> of this market.
        /// </summary>
        /// <param name="action">An action used to receive the order book and the respective slot.</param>
        void SubscribeOrderBook(Action<OrderBook, ulong> action);
        
        /// <summary>
        /// Subscribe to the <see cref="OrderBook"/> of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action used to receive the order book and the respective slot.</param>
        /// <returns>A task which performs the operation.</returns>
        Task SubscribeOrderBookAsync(Action<OrderBook, ulong> action);

        /// <summary>
        /// Unsubscribe to the <see cref="OrderBook"/> of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which performs the operation.</returns>
        Task UnsubscribeOrderBookAsync();

        /// <summary>
        /// Subscribe to the <see cref="OpenOrdersAccount"/> of this market.
        /// </summary>
        /// <param name="action">An action used to receive the open orders account and the respective slot.</param>
        void SubscribeOpenOrders(Action<IList<OpenOrder>, ulong> action);
        
        /// <summary>
        /// Subscribe to the <see cref="OpenOrdersAccount"/> of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action used to receive the open orders account and the respective slot.</param>
        /// <returns>A task which performs the operation.</returns>
        Task SubscribeOpenOrdersAsync(Action<IList<OpenOrder>, ulong> action);

        /// <summary>
        /// Unsubscribe to the order book of this market.
        /// This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which performs the operation.</returns>
        Task UnsubscribeOpenOrdersAsync();

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// If a needed token account or open orders account does not exist, it is created in this same transaction.
        /// </summary>
        /// <param name="order">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        SignatureConfirmation NewOrder(Order order);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// If a needed token account or open orders account does not exist, it is created in this same transaction.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="order">The order object created by the <see cref="OrderBuilder"/>.</param>
        /// <returns>A task which may return a <see cref="SignatureConfirmation"/> object, wrapping the requests made to submit the transaction and subscription of it's confirmation.</returns>
        Task<SignatureConfirmation> NewOrderAsync(Order order);

        /// <summary>
        /// Crafts, requests a signature and submits a transaction with the given order data, awaiting it's confirmation and notifying the user via an event in the response object.
        /// If a needed token account or open orders account does not exist, it is created in this same transaction.
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
        /// If a needed token account or open orders account does not exist, it is created in this same transaction.
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