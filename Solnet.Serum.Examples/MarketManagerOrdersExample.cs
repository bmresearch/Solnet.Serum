using Solnet.KeyStore;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    public class MarketManagerOrdersExample : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly Wallet.Wallet _wallet;
        private readonly SolanaKeyStoreService _keyStore;
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
            _keyStore = new SolanaKeyStoreService();
            
            // get the wallet
            _wallet = _keyStore.RestoreKeystoreFromFile("/home/murlux/hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh.json");

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
            
            _marketManager.SubscribeOrderBook(OrderBookHandler);

            while (_bestBid == null || _bestAsk == null)
            {
                await Task.Delay(100);
            }
            Console.WriteLine($"Best Bid Price: {_bestBid.Price} Size: {_bestBid.Quantity}\tBest Ask Price: {_bestAsk.Price} Size: {_bestAsk.Quantity} ");
            
            var buyOrders = BuildBuyOrders(5, _bestBid.Price, 0.01f, 1, 1.25f);
            
            var newOrdersRes = await _marketManager.NewOrdersAsync(buyOrders);
            foreach (var tx in newOrdersRes)
            {
                tx.ConfirmationChanged += (sender, status) =>
                {
                    Console.WriteLine($"Confirmation for {tx.Signature} changed.\nTxErr: {status.TransactionError?.Type}\tIxErr: {status.InstructionError?.CustomError}\tSerumErr: {status.Error}");
                };
            }
            
            
            /*#1#
            var sellOrders = BuildSellOrders(5, _bestAsk.Price, 0.01f, 25, 1.25f);
            newOrdersRes = await _marketManager.NewOrdersAsync(sellOrders);
            foreach (var tx in newOrdersRes)
            {
                tx.ConfirmationChanged += (sender, status) =>
                {
                    Console.WriteLine($"Confirmation for {tx.Signature} changed.\nTxErr: {status.TransactionError?.Type}\tIxErr: {status.InstructionError?.CustomError}\tSerumErr: {status.Error}");
                };
            }*/
            
            Console.ReadKey();

            var cancelRes = await _marketManager.CancelAllOrdersAsync();
            foreach (var tx in cancelRes)
            {
                tx.ConfirmationChanged += (sender, status) =>
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
        /// Builds a list of buy orders up to a limit of m given orders.
        /// </summary>
        /// <param name="m">The maximum of orders.</param>
        /// <param name="bestBid">The best bid price.</param>
        /// <param name="orderSpread">The spread between successive orders.</param>
        /// <param name="firstBidSize">The best bid size.</param>
        /// <param name="sizeIncrement">The size increment multiplier.</param>
        /// <returns>The list of orders.</returns>
        private List<Order> BuildBuyOrders(int m, float bestBid, float orderSpread, float firstBidSize, float sizeIncrement)
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
        private List<Order> BuildSellOrders(int m, float bestAsk, float orderSpread, float firstAskSize, float sizeIncrement)
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