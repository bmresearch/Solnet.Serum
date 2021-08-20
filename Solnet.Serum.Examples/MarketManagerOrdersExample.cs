using Solnet.KeyStore;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    public class MarketManagerOrdersExample : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly Wallet.Wallet _wallet;
        private OrderBook _orderBook;
        private List<Order> _bids;
        private List<Order> _asks;
        private Order _bestBid;
        private Order _bestAsk;

        public MarketManagerOrdersExample()
        {
            Console.WriteLine($"Initializing {ToString()}");
            InstructionDecoder.Register(SerumProgram.ProgramIdKey, SerumProgram.Decode);
            // init stuff
            SolanaKeyStoreService keyStore = new ();
            
            // get the wallet
            _wallet = keyStore.RestoreKeystoreFromFile("/home/murlux/hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh.json");

            // serum client
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, _wallet.Account, signatureMethod: SignRequest, serumClient: _serumClient);
        }

        private byte[] SignRequest(ReadOnlySpan<byte> messageData)
        {
            Console.WriteLine(Convert.ToBase64String(messageData));
            
            List<DecodedInstruction> ix =
                InstructionDecoder.DecodeInstructions(Message.Deserialize(messageData));
            
            string aggregate = ix.Aggregate(
                "Decoded Instructions:",
                (s, instruction) =>
                {
                    s += $"\n\tProgram: {instruction.ProgramName}\n\t\t\t Instruction: {instruction.InstructionName}\n";
                    return instruction.Values.Aggregate(
                        s, 
                        (current, entry) => 
                            current + $"\t\t\t\t{entry.Key} - {Convert.ChangeType(entry.Value, entry.Value.GetType())}\n");
                });
            Console.WriteLine(aggregate);

            return _wallet.Account.Sign(messageData.ToArray());
        }

        
        public async void Run()
        {
            await _marketManager.SubscribeOrderBookAsync(OrderBookHandler);

            while (_bestBid == null || _bestAsk == null)
            {
                await Task.Delay(100);
            }
            Console.WriteLine($"Best Bid Price: {_bestBid.Price} Size: {_bestBid.Quantity}\tBest Ask Price: {_bestAsk.Price} Size: {_bestAsk.Quantity} ");
            
            List<Order> buyOrders = BuildBuyOrders(5, _bestBid.Price, 0.01f, 1, 1.25f);
            
            IList<SignatureConfirmation> newOrdersRes = await NewOrdersAsync(buyOrders);
            foreach (SignatureConfirmation tx in newOrdersRes)
            {
                tx.ConfirmationChanged += (_, status) =>
                {
                    Console.WriteLine($"Confirmation for {tx.Signature} changed.\nTxErr: {status.TransactionError?.Type}\tIxErr: {status.InstructionError?.CustomError}\tSerumErr: {status.Error}");
                };
            }
            
            Console.ReadKey();

            IList<SignatureConfirmation> cancelRes = await _marketManager.CancelAllOrdersAsync();
            foreach (SignatureConfirmation tx in cancelRes)
            {
                tx.ConfirmationChanged += (_, status) =>
                {
                    Console.WriteLine($"Confirmation for {tx.Signature} changed.\nTxErr: {status.TransactionError?.Type}\tIxErr: {status.InstructionError?.CustomError}\tSerumErr: {status.Error}");
                };
            }
            
            Console.ReadKey();
        }
        
        private void OrderBookHandler(OrderBook orderBook, ulong slot)
        {
            _orderBook = orderBook;
            _bids = _orderBook.GetBids();
            _asks = _orderBook.GetAsks();
            if (_bids.Count == 0 || _asks.Count == 0) return;
            _bestBid = _bids[0];
            _bestAsk = _asks[0];
        }

        /// <summary>
        /// Packs as many orders as possible into a single instruction, adding a <see cref="SerumProgramInstructions.Values.SettleFunds"/> instruction at the end.
        /// </summary>
        /// <param name="orders">The orders to pack.</param>
        /// <returns>A task which may return a list of signature confirmations.</returns>
        private async Task<IList<SignatureConfirmation>> NewOrdersAsync(IList<Order> orders)
        {
            IList<SignatureConfirmation> signatureConfirmations = new List<SignatureConfirmation>();
            string blockHash = await GetBlockHash();
            TransactionBuilder txBuilder = new TransactionBuilder()
                .SetRecentBlockHash(blockHash)
                .SetFeePayer(_wallet.Account);

            SignatureConfirmation sigConf = null;

            TransactionInstruction settleIx = SerumProgram.SettleFunds(
                _marketManager.Market,
                _marketManager.OpenOrdersAddress,
                _wallet.Account,
                new PublicKey(_marketManager.BaseAccount.PublicKey),
                new PublicKey(_marketManager.QuoteAccount.PublicKey));

            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].ConvertOrderValues(_marketManager.BaseDecimals, _marketManager.QuoteDecimals, _marketManager.Market);

                TransactionInstruction txInstruction = SerumProgram.NewOrderV3(
                    _marketManager.Market,
                    _marketManager.OpenOrdersAddress,
                    orders[i].Side == Side.Buy ? 
                        new PublicKey(_marketManager.QuoteAccount.PublicKey) : 
                        new PublicKey(_marketManager.BaseAccount.PublicKey),
                    _wallet.Account,
                    orders[i]);
                txBuilder.AddInstruction(txInstruction);
                byte[] txBytes = txBuilder.CompileMessage();

                if (txBytes.Length < 850 && i != orders.Count - 1) continue;

                txBuilder.AddInstruction(settleIx);
                txBytes = txBuilder.CompileMessage();

                byte[] signatureBytes = SignRequest(txBytes);

                Transaction tx = Transaction.Populate(
                    Message.Deserialize(txBytes), new List<byte[]> {signatureBytes });

                if (signatureBytes != null)
                    sigConf = await SendTransactionAndSubscribeSignature(tx.Serialize());
                if (sigConf != null)
                {
                    signatureConfirmations.Add(sigConf);
                    if (sigConf.SimulationLogs != null) break;
                }
                
                blockHash = await GetBlockHash();
                
                txBuilder = new TransactionBuilder()
                    .SetRecentBlockHash(blockHash)
                    .SetFeePayer(_wallet.Account);
            }

            return signatureConfirmations;
        }
        
        /// <summary>
        /// Attempts to submit a transaction to the cluster.
        /// </summary>
        /// <param name="transaction">The signed transaction bytes.</param>
        /// <returns>A task which may return a <see cref="RequestResult{IEnumerable}"/>.</returns>
        private async Task<RequestResult<string>> SubmitTransaction(byte[] transaction)
        {
            while (true)
            {
                RequestResult<string> req =
                    await _serumClient.RpcClient.SendTransactionAsync(transaction, false, Commitment.Confirmed);

                if (req.WasRequestSuccessfullyHandled)
                    return req;

                if (req.ServerErrorCode != 0)
                    return req;

                await Task.Delay(250);
            }
        }
        
        /// <summary>
        /// Submits a transaction to the cluster and subscribes to its confirmation.
        /// </summary>
        /// <param name="transaction">The signed transaction bytes.</param>
        /// <returns>A task which may return a <see cref="SubscriptionState"/>.</returns>
        private async Task<SignatureConfirmation> SendTransactionAndSubscribeSignature(byte[] transaction)
        {
            RequestResult<string> req = await SubmitTransaction(transaction);
            SignatureConfirmation sigConf = new() { Signature = req.Result, Result = req };

            if (req.ServerErrorCode != 0)
            {
                bool exists = req.ErrorData.TryGetValue("data", out object value);
                if (!exists) return sigConf;
                string elem = ((JsonElement)value).ToString();
                if (elem == null) return sigConf;
                SimulationLogs simulationLogs = JsonSerializer.Deserialize<SimulationLogs>(elem,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    });
                sigConf.ChangeState(simulationLogs);

                return sigConf;
            }

            SubscriptionState sub = await _serumClient.StreamingRpcClient.SubscribeSignatureAsync(req.Result,
                (state, value) =>
                {
                    sigConf.ChangeState(state, value);
                }, Commitment.Confirmed);

            sigConf.Subscription = sub;

            return sigConf;
        }

        /// <summary>
        /// Gets a recent block hash.
        /// </summary>
        /// <returns>A task which may return the block hash.</returns>
        private async Task<string> GetBlockHash()
        {
            RequestResult<ResponseValue<BlockHash>> blockHash = await _serumClient.RpcClient.GetRecentBlockHashAsync();
            return blockHash.Result.Value.Blockhash;
        }
        
        /// <summary>
        /// Builds a list of buy orders up to a limit of m given orders.
        /// </summary>
        /// <param name="m">The maximum of orders.</param>
        /// <param name="bestBid">The best bid price.</param>
        /// <param name="orderSpread">The spread between successive orders.</param>
        /// <param name="firstBidSize">The best bid size.</param>
        /// <param name="sizeIncrement">The size increment multiplier.</param>
        /// <returns>The list of orders.</returns>
        private static List<Order> BuildBuyOrders(int m, float bestBid, float orderSpread, float firstBidSize, float sizeIncrement)
        {
            List<Order> orders = new ();
            
            for (int i = 0; i <= m; i++)
            {
                orders.Add(new OrderBuilder()
                        .SetPrice(bestBid * (1 - (i * orderSpread)))
                        .SetSide(Side.Buy)
                        .SetQuantity(firstBidSize + (i * sizeIncrement))
                        .SetOrderType(OrderType.Limit)
                        .SetSelfTradeBehavior(SelfTradeBehavior.CancelProvide)
                        .SetClientOrderId(1UL)
                        .Build());
            }

            return orders;
        }

        /// <summary>
        /// Builds a list of sell orders up to a limit of m given orders.
        /// </summary>
        /// <param name="m">The maximum of orders.</param>
        /// <param name="bestAsk">The best ask price.</param>
        /// <param name="orderSpread">The spread between successive orders.</param>
        /// <param name="firstAskSize">The best ask size.</param>
        /// <param name="sizeIncrement">The size increment multiplier.</param>
        /// <returns>The list of orders.</returns>
        private static List<Order> BuildSellOrders(int m, float bestAsk, float orderSpread, float firstAskSize, float sizeIncrement)
        {
            List<Order> orders = new ();
            
            for (int i = 1; i <= m; i++)
            {
                orders.Add(new OrderBuilder()
                        .SetPrice(bestAsk * (1 + (i * orderSpread)))
                        .SetSide(Side.Sell)
                        .SetQuantity(firstAskSize + (i * sizeIncrement))
                        .SetOrderType(OrderType.Limit)
                        .SetSelfTradeBehavior(SelfTradeBehavior.CancelProvide)
                        .SetClientOrderId(1UL)
                        .Build());
            }

            return orders;
        }
    }
}