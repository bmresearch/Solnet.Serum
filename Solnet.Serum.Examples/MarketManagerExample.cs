using Solnet.KeyStore;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// Example of market manager usage
    /// </summary>
    public class MarketManagerExample : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("65HCcVzCVLDLEUHVfQrvE5TmHAUKgnCnbii9dojxE7wV");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly List<TradeEvent> _trades;
        private readonly Wallet.Wallet _wallet;
        private readonly SolanaKeyStoreService _keyStore;
        private OrderBook _orderBook;
        private float _cumulativeVolume;
        private int _numTrades;


        public MarketManagerExample()
        {
            Console.WriteLine($"Initializing {ToString()}");
            InstructionDecoder.Register(SerumProgram.ProgramIdKey, SerumProgram.Decode);
            // init stuff
            _keyStore = new SolanaKeyStoreService();
            _trades = new List<TradeEvent>();
            
            // get the wallet
            _wallet = _keyStore.RestoreKeystoreFromFile("/path/to/solana-keygen-wallet.json");

            // serum client
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, _wallet.Account, _marketAddress, SignRequest, serumClient: _serumClient);
        }

        private byte[] SignRequest(ReadOnlySpan<byte> messageData)
        {
            List<DecodedInstruction> instruction =
                InstructionDecoder.DecodeInstructions(Message.Deserialize(messageData));
            
            return _wallet.Account.Sign(messageData.ToArray());
        }

        public void Run()
        {
            _marketManager.SubscribeTrades(TradesHandler);
            _marketManager.SubscribeOrderBook(OrderBookHandler);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            CancellationTokenSource cts = new ();
            RT(() =>
            {
                Console.Clear();
                if (_orderBook != null)
                {
                    Console.WriteLine($"-------------------------------------------- ORDER BOOK -------------------------------------------");
                    List<Order> asks = _orderBook.GetAsks();
                    List<Order> bids = _orderBook.GetBids();
                    float cumulativeAsk = 0;
                    float cumulativeBid = 0;
                    float cumulativeAskUsd = 0;
                    float cumulativeBidUsd = 0;
                    for (int i = 24; i >= 0; i--)
                    {
                        cumulativeAsk += asks[i].Quantity;
                        cumulativeAskUsd += asks[i].Quantity*asks[i].Price;
                    }
                    for (int i = 24; i >= 0; i--)
                    {
                        Console.WriteLine(
                            $"Ask: Owner: {bids[i].Owner.Key} Cum:\t{cumulativeAsk:N2}\t(~{cumulativeAskUsd:C2})\tPrice:\t{asks[i].Price:C5}\tSize:\t{asks[i].Quantity:N2}\t");
                        cumulativeAsk -= asks[i].Quantity;
                        cumulativeAskUsd -= asks[i].Quantity*asks[i].Price;
                    }
                    Console.WriteLine($"---------------------------------------------------------------------------------------------------");
                    for (int i = 0; i < 25; i++)
                    {
                        cumulativeBid += bids[i].Quantity;
                        cumulativeBidUsd += bids[i].Quantity*bids[i].Price;
                        Console.WriteLine(
                            $"Bid: Owner: {bids[i].Owner.Key} Cum:\t{cumulativeBid:N2}\t(~{cumulativeBidUsd:C2})\tPrice:\t{bids[i].Price:C5}\tSize:\t{bids[i].Quantity:N2}");
                    }
                    Console.WriteLine($"---------------------------------------------------------------------------------------------------");
                        
                }
                Console.WriteLine(
                    $"------------------------------------------- STATS        -----------------------------------------");
                Console.WriteLine(
                    $"Connection Statistics: {_serumClient.ConnectionStatistics.AverageThroughput60Seconds / 1024} KB/s last 60 seconds " +
                    $" - {_serumClient.ConnectionStatistics.AverageThroughput10Seconds / 1024} KB/s last 10 seconds " +
                    $" - {_serumClient.ConnectionStatistics.TotalReceivedBytes / 1024} KB");

                if (_trades.Count == 0) return;

                Console.WriteLine(
                    $"------------------------------------------- RECENT TRADES -----------------------------------------");
                Console.WriteLine(
                    $"Trades: Total Trades:\t{_numTrades}\tVolume:\t{_cumulativeVolume:C2}");
                foreach (TradeEvent trade in _trades)
                {
                    Console.WriteLine(
                        $"TradeEvent:: OpenOrdersAccount:\t{trade.Event.PublicKey.Key}\tSide:\t{trade.Side.ToString()}\tPrice:\t{trade.Price:C5}\tSize:\t{trade.Size:N2}\tCost:\t{trade.Price*trade.Size:C2}");
                }
                if (_trades.Count > 25) _trades.Clear();
                
            }, 1, cts.Token);

            Console.ReadKey();
            cts.Cancel();
        }

        private void OrderBookHandler(OrderBook orderBook, ulong slot)
        {
            _orderBook = orderBook;
        }

        private void TradesHandler(IList<TradeEvent> trades, ulong slot)
        {
            foreach (TradeEvent trade in trades.Where(trade => !trade.Event.Flags.IsMaker))
            {
                _trades.Add(trade);
                _numTrades += 1;
                _cumulativeVolume += trade.Price * trade.Size;
            }
        }

        static void RT(Action action, int seconds, CancellationToken token)
        {
            if (action == null)
                return;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }
    }
}