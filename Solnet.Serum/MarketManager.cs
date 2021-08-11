using Microsoft.Extensions.Logging;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary> 
    /// A manager class for Serum <see cref="Market"/>s that returns user friendly data.
    /// </summary>
    public class MarketManager : IMarketManager
    {
        /// <summary>
        /// The market's public key.
        /// </summary>
        private readonly PublicKey _marketAddress;

        /// <summary>
        /// The logger instance.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The serum client instance to use to fetch and decode Serum data.
        /// </summary>
        private readonly ISerumClient _serumClient;

        /// <summary>
        /// The decimals of the base token for the current <see cref="Market"/>.
        /// </summary>
        private byte _baseDecimals;

        /// <summary>
        /// The decimals of the quote token for the current <see cref="Market"/>.
        /// </summary>
        private byte _quoteDecimals;
        
        /// <summary>
        /// The task used to get the <see cref="Market"/> data.
        /// </summary>
        private Task _marketTask;
        
        /// <summary>
        /// The task used to get the decimals for the <see cref="Market"/>'s Base Token Mint.
        /// </summary>
        private Task _baseDecimalsTask;
        
        /// <summary>
        /// The task used to get the decimals for the <see cref="Market"/>'s Quote Token Mint.
        /// </summary>
        private Task _quoteDecimalsTask;

        /// <summary>
        /// The subscription object corresponding to the <see cref="Market"/>'s <see cref="EventQueue"/>.
        /// </summary>
        private Subscription _eventQueueSubscription;

        /// <summary>
        /// The subscription object corresponding to the <see cref="Market"/>'s bid <see cref="OrderBookSide"/>.
        /// </summary>
        private Subscription _bidSideSubscription;

        /// <summary>
        /// The subscription object corresponding to the <see cref="Market"/>'s ask <see cref="OrderBookSide"/>.
        /// </summary>
        private Subscription _askSideSubscription;

        /// <summary>
        /// The bid side of the <see cref="OrderBook"/>.
        /// </summary>
        private OrderBookSide _bidSide;
        
        /// <summary>
        /// The ask side of the <see cref="OrderBook"/>.
        /// </summary>
        private OrderBookSide _askSide;

        /// <summary>
        /// Initialize the <see cref="Market"/> manager with the given market <see cref="PublicKey"/>.
        /// </summary>
        /// <param name="marketAddress">The public key of the market.</param>
        /// <param name="url">The cluster to use when not passing in a serum client instance.</param>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="serumClient">The serum client instance to use.</param>
        public MarketManager(PublicKey marketAddress, string url = null, ILogger logger = null,
            ISerumClient serumClient = default)
        {
            _marketAddress = marketAddress;
            _logger = logger;

            _serumClient = serumClient ?? ClientFactory.GetClient(url, logger);

            // Instantly request the market data in order to build the market later
            _marketTask = Task.Run(
                () =>
                {
                    Market = _serumClient.GetMarketAsync(marketAddress).Result;
                    _logger?.Log(LogLevel.Information, $"Fetched Market data for: {Market.OwnAddress.Key} ::" +
                                                      $" Base Token: {Market.BaseMint.Key} / Quote Token: {Market.QuoteMint.Key}");
                });
            _marketTask.Wait();
            Task.Run(() => {
                _baseDecimalsTask = Task.Run(
                    () =>
                    {
                        _baseDecimals = GetTokenDecimals(Market.BaseMint);
                        _logger?.Log(LogLevel.Information, $"Decimals for Base Token: {_baseDecimals}");
                    });
                _quoteDecimalsTask = Task.Run(
                    () =>
                    {
                        _quoteDecimals = GetTokenDecimals(Market.QuoteMint);
                        _logger?.Log(LogLevel.Information, $"Decimals for Quote Token: {_quoteDecimals}");
                    });
            });
        }

        #region Manager Setup

        /// <summary>
        /// Gets the decimals for the given token mint.
        /// </summary>
        /// <param name="tokenMint">The public key of the token mint.</param>
        private byte GetTokenDecimals(PublicKey tokenMint)
        {
            RequestResult<ResponseValue<AccountInfo>> accountInfo =
                _serumClient.RpcClient.GetAccountInfoAsync(tokenMint).Result;
            return MarketUtils.DecimalsFromTokenMintData(Convert.FromBase64String(accountInfo.Result.Value.Data[0]));
        }

        #endregion


        #region Data Retriaval

        /// <inheritdoc cref="IMarketManager.SubscribeTrades"/>
        public void SubscribeTrades(Action<List<TradeEvent>, ulong> action)
        {
            Task.WhenAll(_baseDecimalsTask, _quoteDecimalsTask).Wait();
            _eventQueueSubscription = _serumClient.SubscribeEventQueue((_, queue, slot) =>
            {
                List<TradeEvent> tradeEvents =
                    (from evt in queue.Events
                        where evt.Flags.IsFill && evt.NativeQuantityPaid > 0
                        select MarketUtils.ProcessTradeEvent(evt, _baseDecimals, _quoteDecimals)).ToList();
                action(tradeEvents, slot);
            }, Market.EventQueue, Commitment.Confirmed);
        }


        /// <inheritdoc cref="IMarketManager.SubscribeOrderBook"/>
        public void SubscribeOrderBook(Action<OrderBook, ulong> action)
        {
            Task.WhenAll(_baseDecimalsTask, _quoteDecimalsTask).Wait();

            _bidSideSubscription = _serumClient.SubscribeOrderBookSide((_, orderBookSide, slot) =>
            {
                _bidSide = orderBookSide;
                OrderBook ob = new () 
                {
                    Bids = orderBookSide,
                    Asks = _askSide,
                    BaseDecimals = _baseDecimals,
                    QuoteDecimals = _quoteDecimals,
                    BaseLotSize = Market.BaseLotSize,
                    QuoteLotSize = Market.QuoteLotSize,
                };
                action(ob, slot);
            }, Market.Bids, Commitment.Confirmed);

            _askSideSubscription = _serumClient.SubscribeOrderBookSide((_, orderBookSide, slot) =>
            {
                _askSide = orderBookSide;
                OrderBook ob = new () 
                {
                    Bids = _bidSide,
                    Asks = orderBookSide,
                    BaseDecimals = _baseDecimals,
                    QuoteDecimals = _quoteDecimals,
                    BaseLotSize = Market.BaseLotSize,
                    QuoteLotSize = Market.QuoteLotSize,
                };
                action(ob, slot);
            }, Market.Asks, Commitment.Confirmed);
        }

        /// <inheritdoc cref="IMarketManager.UnsubscribeTrades"/>
        public void UnsubscribeTrades()
            => _serumClient.UnsubscribeEventQueue(_eventQueueSubscription.Address);

        /// <inheritdoc cref="IMarketManager.UnsubscribeOrderBook"/>
        public void UnsubscribeOrderBook()
        {
            _serumClient.UnsubscribeOrderBookSide(_bidSideSubscription.Address);
            _serumClient.UnsubscribeOrderBookSide(_askSideSubscription.Address);
        }

        #endregion

        /// <summary>
        /// The decoded data of the underlying <see cref="Market"/>.
        /// </summary>
        public Market Market { get; private set; }
    }
}