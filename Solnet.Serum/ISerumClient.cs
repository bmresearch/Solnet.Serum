using Solnet.Rpc;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary>
    /// Specifies functionality for the Serum Client.
    /// </summary>
    public interface ISerumClient
    {
        /// <summary>
        /// The rpc client instance.
        /// </summary>
        IRpcClient RpcClient { get; }
        
        /// <summary>
        /// The statistics of the current websocket connection.
        /// </summary>
        IConnectionStatistics ConnectionStatistics { get; }
        
        /// <summary>
        /// Gets the available markets in the Serum DEX. This is an asynchronous operation.
        /// <remarks>
        /// This list of markets is hardcoded and managed by Project Serum, for more information see:
        /// https://github.com/project-serum/serum-ts
        /// </remarks>
        /// </summary>
        /// <returns>A task which may return a list with the market's info.</returns>
        Task<IList<MarketInfo>> GetMarketsAsync();
        
        /// <summary>
        /// Gets the available markets in the Serum DEX.
        /// <remarks>
        /// This list of markets is hardcoded and managed by Project Serum, for more information see:
        /// https://github.com/project-serum/serum-ts
        /// </remarks>
        /// </summary>
        /// <returns>A list with the market's info.</returns>
        IList<MarketInfo> GetMarkets();
        
        /// <summary>
        /// Gets the available tokens in the Serum DEX. This is an asynchronous operation.
        /// <remarks>
        /// This list of token mints is hardcoded and managed by Project Serum, for more information see:
        /// https://github.com/project-serum/serum-ts
        /// </remarks>
        /// </summary>
        /// <returns>A task which may return a list with the token's info.</returns>
        Task<IList<TokenInfo>> GetTokensAsync();
        
        /// <summary>
        /// Gets the available tokens in the Serum DEX.
        /// <remarks>
        /// This list of markets is hardcoded and managed by Project Serum, for more information see:
        /// https://github.com/project-serum/serum-ts
        /// </remarks>
        /// </summary>
        /// <returns>A list with the token's info.</returns>
        IList<TokenInfo> GetTokens();
        
        /// <summary>
        /// Gets the account data associated with the given market address in the Serum DEX.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="address">The public key of the market account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>A task which may return the market's account data.</returns>
        Task<Market> GetMarketAsync(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Gets the account data associated with the given market address in the Serum DEX.
        /// </summary>
        /// <param name="address">The public key of the market account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>The market's account data.</returns>
        Market GetMarket(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Gets the account data associated with the given Event Queue address in the Serum DEX.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="address">The public key of the Event Queue account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>A task which may return the Event Queue's account data.</returns>
        Task<EventQueue> GetEventQueueAsync(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Gets the account data associated with the given Event Queue address in the Serum DEX.
        /// </summary>
        /// <param name="address">The public key of the Event Queue account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>The Event Queue's account data.</returns>
        EventQueue GetEventQueue(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Gets the account data associated with the given open orders account address in the Serum DEX.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="address">The public key of the open orders account account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>A task which may return the open orders account data.</returns>
        Task<OpenOrdersAccount> GetOpenOrdersAccountAsync(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Gets the account data associated with the given open orders account address in the Serum DEX.
        /// </summary>
        /// <param name="address">The public key of the open orders account account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>The open orders account data.</returns>
        OpenOrdersAccount GetOpenOrdersAccount(string address, Commitment commitment = Commitment.Finalized);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        Task<OrderBook> GetOrderBookAsync(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        OrderBook GetOrderBook(string address, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Connect to the Rpc client for data streaming. This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which may connect to the Rpc.</returns>
        Task ConnectAsync();
        
        /// <summary>
        /// Connect to the Rpc client for data streaming.
        /// </summary>
        void Connect();
        
        /// <summary>
        /// Disconnects from the Rpc client for data streaming. This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which may connect to the Rpc.</returns>
        Task DisconnectAsync();
        
        /// <summary>
        /// Disconnects from the Rpc client for data streaming.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Open Orders Account. This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action which receives an Open Orders Account.</param>
        /// <param name="openOrdersAccountAddress">The public key of the Open Orders Account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>A task which may return a subscription for the Open Orders Account.</returns>
        Task<Subscription> SubscribeOpenOrdersAccountAsync(Action<Subscription, OpenOrdersAccount> action, string openOrdersAccountAddress, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Open Orders Account.
        /// </summary>
        /// <param name="action">An action which receives an Open Orders Account.</param>
        /// <param name="openOrdersAccountAddress">The public key of the Open Orders Account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        /// <returns>A subscription for the Open Orders Account.</returns>
        Subscription SubscribeOpenOrdersAccount(Action<Subscription, OpenOrdersAccount> action, string openOrdersAccountAddress, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Event Queue. This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action which receives an Event Queue.</param>
        /// <param name="eventQueueAccountAddress">The public key of the Event Queue account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        Task<Subscription> SubscribeEventQueueAsync(Action<Subscription, EventQueue> action, string eventQueueAccountAddress, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Event Queue.
        /// </summary>
        /// <param name="action">An action which receives an Event Queue.</param>
        /// <param name="eventQueueAccountAddress">The public key of the Event Queue account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        Subscription SubscribeEventQueue(Action<Subscription, EventQueue> action, string eventQueueAccountAddress, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Order Book. This will either be a Bid or Ask account data feed. This is an asynchronous operation.
        /// </summary>
        /// <param name="action">An action which receives an Order Book.</param>
        /// <param name="orderBookAccountAddress">The public key of the Order Book account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        Task<Subscription> SubscribeOrderBookAsync(Action<Subscription, OrderBook> action, string orderBookAccountAddress, Commitment commitment = Commitment.Finalized);
        
        /// <summary>
        /// Subscribe to a live feed of a Serum Market's Order Book. This will either be a Bid or Ask account data feed.
        /// </summary>
        /// <param name="action">An action which receives an Order Book.</param>
        /// <param name="orderBookAccountAddress">The public key of the Order Book account.</param>
        /// <param name="commitment">The commitment parameter for the Rpc Client.</param>
        Subscription SubscribeOrderBook(Action<Subscription, OrderBook> action, string orderBookAccountAddress, Commitment commitment = Commitment.Finalized);
    }
}