using Microsoft.Extensions.Logging;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary> 
    /// A manager class for Serum <see cref="Market"/>s that returns user friendly data.
    /// </summary>
    public class MarketManager : IMarketManager
    {
        /// <summary>
        /// The owner's <see cref="PublicKey"/>.
        /// </summary>
        private readonly PublicKey _ownerAccount;

        /// <summary>
        /// The <see cref="Market"/>'s <see cref="PublicKey"/>.
        /// </summary>
        private readonly PublicKey _marketAccount;

        /// <summary>
        /// The <see cref="OpenOrdersAccount"/>'s <see cref="PublicKey"/>.
        /// </summary>
        private readonly PublicKey _openOrdersAccount;

        /// <summary>
        /// The <see cref="PublicKey"/> of the SRM token account to use for fee discount..
        /// </summary>
        private readonly PublicKey _srmAccount;

        /// <summary>
        /// A delegate method type used to request the user to sign transactions crafted by the <see cref="MarketManager"/>.
        /// </summary>
        private readonly RequestSignature _requestSignature;

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
        /// The subscription object corresponding to the <see cref="Market"/>'s <see cref="OpenOrdersAccount"/>.
        /// </summary>
        private Subscription _openOrdersSubscription;

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
        /// A recent blockhash, updated periodically.
        /// </summary>
        private string _blockHash;

        /// <summary>
        /// The cancellation token source for the blockhash periodic request.
        /// </summary>
        private CancellationTokenSource _blockHashCancellationToken;

        /// <summary>
        /// Initialize the <see cref="Market"/> manager with the given market <see cref="PublicKey"/>.
        /// </summary>
        /// <param name="marketAccount">The <see cref="PublicKey"/> of the <see cref="Market"/>.</param>
        /// <param name="ownerAccount">The <see cref="PublicKey"/> of the owner account.</param>
        /// <param name="srmAccount">The <see cref="PublicKey"/> of the serum account to use for fee discount, not used when not provided.</param>
        /// <param name="signatureMethod">A delegate method used to request a signature for transactions crafted by the <see cref="MarketManager"/> which will submit, cancel orders, or settle funds.</param>
        /// <param name="url">The cluster to use when not passing in a serum client instance.</param>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="serumClient">The serum client instance to use.</param>
        public MarketManager(PublicKey marketAccount, PublicKey ownerAccount, PublicKey srmAccount = null,
            RequestSignature signatureMethod = null, string url = null, ILogger logger = null,
            ISerumClient serumClient = default)
        {
            _marketAccount = marketAccount;
            _ownerAccount = ownerAccount;
            _srmAccount = srmAccount;
            _requestSignature = signatureMethod;
            _blockHashCancellationToken = new CancellationTokenSource();
            _logger = logger;

            if (url != null)
            {
                _serumClient = serumClient ?? ClientFactory.GetClient(url, logger);
            }
            else
            {
                _serumClient = serumClient ?? ClientFactory.GetClient(Cluster.MainNet, logger);
            }

            // Instantly request the market data in order to build the market later
            Task.Run(GetMarket).ContinueWith(async _ =>
            {
                // Get decimals for the market's tokens
                await GetBaseDecimals();
                await GetQuoteDecimals();
                
                // Get the ATAs for both
                BaseAccount = await GetAssociatedTokenAccount(Market.BaseMint);
                QuoteAccount = await GetAssociatedTokenAccount(Market.QuoteMint);

                // Get the open orders account, if it exists
                await GetOpenOrdersAccount();
            });

            // If the user passed in a RequestSignature delegate method then we'll auto-update the most recent blockhash
            if (_requestSignature != null)
                Task.Run(async () => { await UpdateBlockHash(_blockHashCancellationToken.Token); });
        }

        #region Manager Setup

        /// <summary>
        /// Get the decimals for the quote token.
        /// </summary>
        private async Task GetQuoteDecimals()
        {
            _quoteDecimals = await GetTokenDecimals(Market.QuoteMint);
            _logger?.Log(LogLevel.Information, $"Decimals for Quote Token: {_quoteDecimals}");
        }

        /// <summary>
        /// Get the decimals for the base token.
        /// </summary>
        private async Task GetBaseDecimals()
        {
            _baseDecimals = await GetTokenDecimals(Market.BaseMint);
            _logger?.Log(LogLevel.Information, $"Decimals for Base Token: {_baseDecimals}");
        }

        /// <summary>
        /// Get the market data.
        /// </summary>
        private async Task GetMarket()
        {
            Market = await _serumClient.GetMarketAsync(_marketAccount);
            _logger?.Log(LogLevel.Information,
                $"Fetched Market data for: {Market.OwnAddress.Key} ::" +
                $" Base Token: {Market.BaseMint.Key} / Quote Token: {Market.QuoteMint.Key}");
        }

        /// <summary>
        /// Gets the decimals for the given token mint.
        /// </summary>
        /// <param name="tokenMint">The public key of the token mint.</param>
        private async Task<byte> GetTokenDecimals(PublicKey tokenMint)
        {
            while (true)
            {
                RequestResult<ResponseValue<AccountInfo>> accountInfo = await
                    _serumClient.RpcClient.GetAccountInfoAsync(tokenMint);
                if (accountInfo.WasRequestSuccessfullyHandled)
                    return MarketUtils.DecimalsFromTokenMintData(
                        Convert.FromBase64String(accountInfo.Result.Value.Data[0]));

                await Task.Delay(250);
            }
        }

        /// <summary>
        /// Updates the stored blockhash every 15s.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token which stops this task from happening periodically.</param>
        private async Task UpdateBlockHash(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                RequestResult<ResponseValue<BlockHash>> blockHash =
                    await _serumClient.RpcClient.GetRecentBlockHashAsync();

                if (blockHash.WasRequestSuccessfullyHandled)
                {
                    _blockHash = blockHash.Result.Value.Blockhash;
                    await Task.Delay(10000, cancellationToken);
                }

                await Task.Delay(250, cancellationToken);
            }
        }

        /// <summary>
        /// Gets the <see cref="OpenOrdersAccount"/> for the given <see cref="Market"/> and owner address.
        /// </summary>
        private async Task GetOpenOrdersAccount()
        {
            List<MemCmp> filters = new()
            {
                new MemCmp { Offset = 13, Bytes = _marketAccount },
                new MemCmp { Offset = 45, Bytes = _ownerAccount }
            };
            while (true)
            {
                RequestResult<List<AccountKeyPair>> accounts = await
                    _serumClient.RpcClient.GetProgramAccountsAsync(SerumProgram.ProgramIdKey,
                        dataSize: OpenOrdersAccount.Layout.SpanLength, memCmpList: filters);

                if (!accounts.WasRequestSuccessfullyHandled)
                {
                    await Task.Delay(250);
                    continue;
                }

                if (accounts.Result.Count != 0)
                {
                    OpenOrdersAccount = 
                        OpenOrdersAccount.Deserialize(Convert.FromBase64String(accounts.Result[0].Account.Data[0]));
                    break;
                }

                _logger?.Log(LogLevel.Information,
                    $"Could not find open orders account for market {_marketAccount} and owner {_ownerAccount}");
                break;
            }
        }

        /// <summary>
        /// Gets the associated token account for the given mint and the owner address,
        /// </summary>
        /// <param name="mint">The <see cref="PublicKey"/> of the token mint</param>
        private async Task<TokenAccount> GetAssociatedTokenAccount(PublicKey mint)
        {
            while (true)
            {
                RequestResult<ResponseValue<List<TokenAccount>>> accounts = await
                    _serumClient.RpcClient.GetTokenAccountsByOwnerAsync(_ownerAccount, mint, TokenProgram.ProgramIdKey);

                if (!accounts.WasRequestSuccessfullyHandled)
                {
                    await Task.Delay(500);
                    continue;
                }

                if (accounts.Result.Value.Count != 0)
                    return accounts.Result.Value[0];

                _logger?.Log(LogLevel.Information,
                    $"Could not find open orders account for market {_marketAccount} and owner {_ownerAccount}");
                break;
            }

            return null;
        }

        #endregion

        #region Data Retriaval

        /// <inheritdoc cref="IMarketManager.SubscribeTrades"/>
        public async void SubscribeTrades(Action<IList<TradeEvent>, ulong> action)
        {
            await Task.WhenAll(_baseDecimalsTask, _quoteDecimalsTask);
            _eventQueueSubscription = _serumClient.SubscribeEventQueueAsync((_, queue, slot) =>
            {
                List<TradeEvent> tradeEvents =
                    (from evt in queue.Events
                        where evt.Flags.IsFill && evt.NativeQuantityPaid > 0
                        select MarketUtils.ProcessTradeEvent(evt, _baseDecimals, _quoteDecimals)).ToList();
                action(tradeEvents, slot);
            }, Market.EventQueue, Commitment.Confirmed).Result;
        }

        /// <inheritdoc cref="IMarketManager.SubscribeOrderBook"/>
        public async void SubscribeOrderBook(Action<OrderBook, ulong> action)
        {
            await Task.WhenAll(_baseDecimalsTask, _quoteDecimalsTask);

            _bidSideSubscription = _serumClient.SubscribeOrderBookSideAsync((_, orderBookSide, slot) =>
            {
                _bidSide = orderBookSide;
                OrderBook ob = new()
                {
                    Bids = orderBookSide,
                    Asks = _askSide,
                    BaseDecimals = _baseDecimals,
                    QuoteDecimals = _quoteDecimals,
                    BaseLotSize = Market.BaseLotSize,
                    QuoteLotSize = Market.QuoteLotSize,
                };
                action(ob, slot);
            }, Market.Bids, Commitment.Confirmed).Result;

            _askSideSubscription = _serumClient.SubscribeOrderBookSideAsync((_, orderBookSide, slot) =>
            {
                _askSide = orderBookSide;
                OrderBook ob = new()
                {
                    Bids = _bidSide,
                    Asks = orderBookSide,
                    BaseDecimals = _baseDecimals,
                    QuoteDecimals = _quoteDecimals,
                    BaseLotSize = Market.BaseLotSize,
                    QuoteLotSize = Market.QuoteLotSize,
                };
                action(ob, slot);
            }, Market.Asks, Commitment.Confirmed).Result;
        }

        /// <inheritdoc cref="IMarketManager.SubscribeOpenOrders"/>
        public void SubscribeOpenOrders(Action<IList<OpenOrder>, ulong> action)
        {
            _openOrdersSubscription = _serumClient.SubscribeOpenOrdersAccountAsync((_, account, slot) =>
            {
                OpenOrdersAccount = account;
                action(account.Orders, slot);
            }, _openOrdersAccount).Result;
        }

        /// <inheritdoc cref="IMarketManager.UnsubscribeTrades"/>
        public void UnsubscribeTrades()
            => _serumClient.UnsubscribeEventQueueAsync(_eventQueueSubscription.Address);

        /// <inheritdoc cref="IMarketManager.UnsubscribeOrderBook"/>
        public void UnsubscribeOrderBook()
        {
            _serumClient.UnsubscribeOrderBookSideAsync(_bidSideSubscription.Address);
            _serumClient.UnsubscribeOrderBookSideAsync(_askSideSubscription.Address);
        }

        /// <inheritdoc cref="IMarketManager.UnsubscribeOpenOrders"/>
        public void UnsubscribeOpenOrders()
            => _serumClient.UnsubscribeOpenOrdersAccountAsync(_openOrdersSubscription.Address);

        #endregion

        #region Order Management

        /// <inheritdoc cref="IMarketManager.NewOrderAsync(Order)"/>
        public Task<SignatureConfirmation> NewOrderAsync(Order order)
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            order.ConvertOrderValues(_baseDecimals, _quoteDecimals, Market);


            TransactionInstruction txInstruction = SerumProgram.NewOrderV3(
                Market,
                _openOrdersAccount,
                order.Side == Side.Buy ? new PublicKey(QuoteAccount.PublicKey) : new PublicKey(BaseAccount.PublicKey),
                _ownerAccount,
                order,
                _srmAccount);

            byte[] txBytes = new TransactionBuilder()
                .SetRecentBlockHash(_blockHash)
                .SetFeePayer(_ownerAccount)
                .AddInstruction(txInstruction)
                .CompileMessage();

            byte[] signatureBytes = _requestSignature(txBytes);

            Transaction tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

            return SendTransactionAndSubscribeSignature(tx.Serialize());
        }

        /// <inheritdoc cref="IMarketManager.NewOrder(Order)"/>
        public SignatureConfirmation NewOrder(Order order) => NewOrderAsync(order).Result;

        /// <inheritdoc cref="IMarketManager.NewOrderAsync(Side, OrderType, SelfTradeBehavior, float, float, ulong)"/>
        public Task<SignatureConfirmation> NewOrderAsync(
            Side side, OrderType type, SelfTradeBehavior selfTradeBehavior, float size, float price,
            ulong clientId = ulong.MaxValue)
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            Order order = new OrderBuilder()
                .SetPrice(price)
                .SetQuantity(size)
                .SetOrderType(type)
                .SetSide(side)
                .SetClientOrderId(clientId)
                .SetSelfTradeBehavior(selfTradeBehavior)
                .Build();

            order.ConvertOrderValues(_baseDecimals, _quoteDecimals, Market);

            TransactionInstruction txInstruction =
                SerumProgram.NewOrderV3(
                    Market,
                    _openOrdersAccount,
                    order.Side == Side.Buy
                        ? new PublicKey(QuoteAccount.PublicKey)
                        : new PublicKey(BaseAccount.PublicKey),
                    _ownerAccount,
                    order,
                    _srmAccount);

            byte[] txBytes = new TransactionBuilder()
                .SetRecentBlockHash(_blockHash)
                .SetFeePayer(_ownerAccount)
                .AddInstruction(txInstruction)
                .CompileMessage();

            byte[] signatureBytes = _requestSignature(txBytes);

            Transaction tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

            return SendTransactionAndSubscribeSignature(tx.Serialize());
        }

        /// <inheritdoc cref="IMarketManager.NewOrder(Side, OrderType, SelfTradeBehavior, float, float, ulong)"/>
        public SignatureConfirmation NewOrder(
            Side side, OrderType type, SelfTradeBehavior selfTradeBehavior, float size, float price,
            ulong clientId = ulong.MaxValue) =>
            NewOrderAsync(side, type, selfTradeBehavior, size, price, clientId).Result;

        /// <inheritdoc cref="IMarketManager.NewOrdersAsync(IList{Order})"/>
        public Task<IList<SignatureConfirmation>> NewOrdersAsync(IList<Order> orders)
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            return Task.Run(async () =>
            {
                IList<SignatureConfirmation> signatureConfirmations = new List<SignatureConfirmation>();

                while (orders.Count != 0)
                {
                    TransactionBuilder txBuilder = new TransactionBuilder()
                        .SetRecentBlockHash(_blockHash)
                        .SetFeePayer(_ownerAccount);
                    byte[] txBytes = null;
                    byte[] signatureBytes = null;
                    SignatureConfirmation sigConf = null;

                    foreach (Order order in orders)
                    {
                        order.ConvertOrderValues(_baseDecimals, _quoteDecimals, Market);

                        TransactionInstruction txInstruction = SerumProgram.NewOrderV3(
                            Market,
                            _openOrdersAccount,
                            order.Side == Side.Buy
                                ? new PublicKey(QuoteAccount.PublicKey)
                                : new PublicKey(BaseAccount.PublicKey),
                            _ownerAccount,
                            order,
                            _srmAccount);
                        txBuilder.AddInstruction(txInstruction);
                        orders.Remove(order);

                        txBytes = txBuilder.CompileMessage();
                        if (txBytes.Length >= 1000)
                            break;
                    }

                    if (txBytes != null) signatureBytes = _requestSignature(txBytes);
                    
                    Transaction tx = Transaction.Populate(
                        Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

                    if (signatureBytes != null) sigConf = await SendTransactionAndSubscribeSignature(tx.Serialize());
                    if (sigConf != null) signatureConfirmations.Add(sigConf);
                }

                return signatureConfirmations;
            });
        }

        /// <inheritdoc cref="IMarketManager.NewOrders(IList{Order})"/>
        public IList<SignatureConfirmation> NewOrders(IList<Order> orders) => NewOrdersAsync(orders).Result;

        /// <inheritdoc cref="IMarketManager.CancelOrderAsync(BigInteger)"/>
        public Task<SignatureConfirmation> CancelOrderAsync(BigInteger orderId)
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            OpenOrder openOrder = OpenOrders.FirstOrDefault(order => order.OrderId.Equals(orderId));
            
            if (openOrder == null)
                throw new Exception("could not find open order for given order id");
            
            TransactionInstruction txInstruction =
                SerumProgram.CancelOrderV2(
                    Market,
                    _openOrdersAccount,
                    _ownerAccount,
                    openOrder.IsBid ? Side.Buy : Side.Sell,
                    openOrder.OrderId);

            byte[] txBytes = new TransactionBuilder()
                .SetRecentBlockHash(_blockHash)
                .SetFeePayer(_ownerAccount)
                .AddInstruction(txInstruction)
                .CompileMessage();

            byte[] signatureBytes = _requestSignature(txBytes);

            Transaction tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

            return SendTransactionAndSubscribeSignature(tx.Serialize());
        }

        /// <inheritdoc cref="IMarketManager.CancelOrder(BigInteger)"/>
        public SignatureConfirmation CancelOrder(BigInteger orderId) => CancelOrderAsync(orderId).Result;

        /// <inheritdoc cref="IMarketManager.CancelOrderAsync(ulong)"/>
        public Task<SignatureConfirmation> CancelOrderAsync(ulong clientId)
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            TransactionInstruction txInstruction =
                SerumProgram.CancelOrderByClientIdV2(
                    Market,
                    _openOrdersAccount,
                    _ownerAccount, clientId);

            byte[] txBytes = new TransactionBuilder()
                .SetRecentBlockHash(_blockHash)
                .SetFeePayer(_ownerAccount)
                .AddInstruction(txInstruction)
                .CompileMessage();

            byte[] signatureBytes = _requestSignature(txBytes);

            Transaction tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

            return SendTransactionAndSubscribeSignature(tx.Serialize());
        }

        /// <inheritdoc cref="IMarketManager.CancelOrder(ulong)"/>
        public SignatureConfirmation CancelOrder(ulong clientId) => CancelOrderAsync(clientId).Result;

        /// <inheritdoc cref="IMarketManager.CancelAllOrdersAsync"/>
        public Task<IList<SignatureConfirmation>> CancelAllOrdersAsync()
        {
            if (_requestSignature == null)
                throw new Exception("signature request method hasn't been set");

            return Task.Run(async () =>
            {
                IList<SignatureConfirmation> signatureConfirmations = new List<SignatureConfirmation>();
                TransactionBuilder txBuilder = new TransactionBuilder()
                    .SetRecentBlockHash(_blockHash)
                    .SetFeePayer(_ownerAccount);

                foreach (OpenOrder openOrder in OpenOrders)
                {
                    SignatureConfirmation sigConf = null;
                    
                    TransactionInstruction txInstruction =
                        SerumProgram.CancelOrderV2(
                            Market,
                            _openOrdersAccount,
                            _ownerAccount,
                            openOrder.IsBid ? Side.Buy : Side.Sell,
                            openOrder.OrderId);
                    
                    txBuilder.AddInstruction(txInstruction);
                    byte[] txBytes = txBuilder.CompileMessage();
                    
                    if (txBytes.Length < 1000) continue;

                    byte[] signatureBytes = _requestSignature(txBytes);
                        
                    Transaction tx = Transaction.Populate(
                        Message.Deserialize(txBytes), new List<byte[]> { signatureBytes });

                    if (signatureBytes != null) sigConf = await SendTransactionAndSubscribeSignature(tx.Serialize());
                    if (sigConf != null) signatureConfirmations.Add(sigConf);
                    
                    txBuilder = new TransactionBuilder()
                        .SetRecentBlockHash(_blockHash)
                        .SetFeePayer(_ownerAccount);
                }

                return signatureConfirmations;
            });
        }

        /// <inheritdoc cref="IMarketManager.CancelAllOrders"/>
        public IList<SignatureConfirmation> CancelAllOrders() => CancelAllOrdersAsync().Result;

        /// <summary>
        /// Submits a transaction to the cluster and subscribes to its confirmation.
        /// </summary>
        /// <param name="transaction">The signed transaction bytes.</param>
        /// <returns>A task which may return a <see cref="SubscriptionState"/>.</returns>
        private async Task<SignatureConfirmation> SendTransactionAndSubscribeSignature(byte[] transaction)
        {
            RequestResult<string> req = await SubmitTransaction(transaction);
            SignatureConfirmation sigConf = new() { Signature = req.Result, Result = req };

            SubscriptionState sub = await _serumClient.StreamingRpcClient.SubscribeSignatureAsync(req.Result,
                (state, value) =>
                {
                    sigConf.ChangeState(state, value);
                }, Commitment.Confirmed);

            sigConf.Subscription = sub;

            return sigConf;
        }

        /// <summary>
        /// Attempts to submit a transaction to the cluster.
        /// </summary>
        /// <param name="transaction">The signed transaction bytes.</param>
        /// <returns>A task which may return a <see cref="RequestResult{string}"/>.</returns>
        private async Task<RequestResult<string>> SubmitTransaction(byte[] transaction)
        {
            while (true)
            {
                RequestResult<string> req =
                    await _serumClient.RpcClient.SendTransactionAsync(transaction, false, Commitment.Confirmed);

                if (req.WasRequestSuccessfullyHandled)
                    return req;

                await Task.Delay(250);
            }
        }

        #endregion

        /// <inheritdoc cref="IMarketManager.OpenOrders"/>
        public IList<OpenOrder> OpenOrders => OpenOrdersAccount.Orders;

        /// <inheritdoc cref="IMarketManager.OpenOrdersAccount"/>
        public OpenOrdersAccount OpenOrdersAccount { get; private set; }

        /// <inheritdoc cref="IMarketManager.BaseAccount"/>
        public TokenAccount BaseAccount { get; private set; }

        /// <inheritdoc cref="IMarketManager.QuoteAccount"/>
        public TokenAccount QuoteAccount { get; private set; }

        /// <inheritdoc cref="IMarketManager.Market"/>
        public Market Market { get; private set; }

        /// <summary>
        /// The method type which the callee must provide in order to sign a given message crafted by the <see cref="MarketManager"/>.
        /// </summary>
        public delegate byte[] RequestSignature(ReadOnlySpan<byte> messageData);
    }
}