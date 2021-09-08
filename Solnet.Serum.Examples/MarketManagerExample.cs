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
        private readonly PublicKey _marketAddress = new("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly List<TradeEvent> _trades;
        private OrderBook _orderBook;
        private float _cumulativeVolume;
        private int _numTrades;


        public MarketManagerExample()
        {
            Console.WriteLine($"Initializing {ToString()}");
            _trades = new List<TradeEvent>();
            
            // serum client
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, serumClient: _serumClient);
            _marketManager.InitAsync().Wait();
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
                            $"Ask: Owner: {bids[i].Owner.Key} Cum:\t{cumulativeAsk:N2}\t(~{cumulativeAskUsd:C2}) Price:\t{asks[i].Price:C5} Size:\t{asks[i].Quantity:N2}");
                        cumulativeAsk -= asks[i].Quantity;
                        cumulativeAskUsd -= asks[i].Quantity*asks[i].Price;
                    }
                    Console.WriteLine($"---------------------------------------------------------------------------------------------------");
                    for (int i = 0; i < 25; i++)
                    {
                        cumulativeBid += bids[i].Quantity;
                        cumulativeBidUsd += bids[i].Quantity*bids[i].Price;
                        Console.WriteLine(
                            $"Bid: Owner: {bids[i].Owner.Key} Cum:\t{cumulativeBid:N2} (~{cumulativeBidUsd:C2}) Price:\t{bids[i].Price:C5} Size:\t{bids[i].Quantity:N2}");
                    }
                    Console.WriteLine($"---------------------------------------------------------------------------------------------------");
                    
                Console.WriteLine(
                    $"------------------------------------------- STATS        -----------------------------------------");
                Console.WriteLine(
                    $"Connection Statistics: {_serumClient.ConnectionStatistics.AverageThroughput60Seconds / 1024} KB/s last 60 seconds " +
                    $" - {_serumClient.ConnectionStatistics.AverageThroughput10Seconds / 1024} KB/s last 10 seconds " +
                    $" - {_serumClient.ConnectionStatistics.TotalReceivedBytes / 1024} KB");
                }
                if (_trades.Count == 0) return;

                Console.WriteLine(
                    $"------------------------------------------- RECENT TRADES -----------------------------------------");
                Console.WriteLine(
                    $"Trades: Total Trades:\t{_numTrades}\tVolume:\t{_cumulativeVolume:C2}");
                foreach (TradeEvent trade in _trades)
                {
                    Console.WriteLine(
                        $"TradeEvent:: OpenOrdersAccount: {trade.Event.PublicKey.Key} Side:\t{trade.Side.ToString()} Price:\t{trade.Price:C5} Size:\t{trade.Size:N2} Cost:\t{trade.Price*trade.Size:C2}");
                }
                if (_trades.Count > 25) _trades.Clear();
                
            }, 5, cts.Token);

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