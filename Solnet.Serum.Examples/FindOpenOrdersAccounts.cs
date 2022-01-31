using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solnet.Serum.Examples
{
    public class FindOpenOrders : IRunnableExample
    {
        private static IRpcClient RpcClient;
        private static ISerumClient SerumClient;

        private const string MarketAddress = "9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT";
        private const string OwnerAddress = "hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh";
        
        public FindOpenOrders()
        {
            RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);
            SerumClient = ClientFactory.GetClient(RpcClient);
        }
        
        public void Run()
        {
            // Get the market account data.
            Market market = SerumClient.GetMarket(MarketAddress);
            
            // Get open orders accounts for a market.
            List<MemCmp> filters = new ()
            {
                new MemCmp{ Offset = 13, Bytes = MarketAddress },
                new MemCmp{ Offset = 45, Bytes = OwnerAddress }
            };
            RequestResult<List<AccountKeyPair>> accounts = 
                RpcClient.GetProgramAccounts(SerumProgram.MainNetProgramIdKeyV3, dataSize: OpenOrdersAccount.Layout.SpanLength, memCmpList: filters);           
            
            /* Print all of the found open orders accounts */
            foreach (AccountKeyPair account in accounts.Result)
            {
                Console.WriteLine($"---------------------");
                Console.WriteLine($"OpenOrdersAccount: {account.PublicKey} - Owner: {account.Account.Owner}");
                OpenOrdersAccount ooa = OpenOrdersAccount.Deserialize(Convert.FromBase64String(account.Account.Data[0]));
                Console.WriteLine($"OpenOrdersAccount:: Owner: {ooa.Owner.Key} Market: {ooa.Market.Key}\n" +
                                  $"BaseTotal: {ooa.BaseTokenTotal} BaseFree: {ooa.BaseTokenFree}\n" +
                                  $"QuoteTotal: {ooa.QuoteTokenTotal} QuoteFree: {ooa.QuoteTokenFree}");
                Console.WriteLine($"---------------------");
            }
            string openOrdersAddress = accounts.Result[0].PublicKey;
            OpenOrdersAccount openOrdersAccount = OpenOrdersAccount.Deserialize(Convert.FromBase64String(accounts.Result[0].Account.Data[0]));

            // Get both sides of the order book
            OrderBookSide bidSide = SerumClient.GetOrderBookSide(market.Bids.Key);
            OrderBookSide askSide = SerumClient.GetOrderBookSide(market.Asks.Key);

            List<OpenOrder> asks = askSide.GetOrders();
            foreach (OpenOrder ask in asks.Where(ask => ask.Owner.Key == openOrdersAddress))
            {
                openOrdersAccount.Orders[ask.OrderIndex].RawQuantity = ask.RawQuantity;
            }
            
            List<OpenOrder> bids = bidSide.GetOrders();
            foreach (OpenOrder bid in bids.Where(ask => ask.Owner.Key == openOrdersAddress))
            {
                openOrdersAccount.Orders[bid.OrderIndex].RawQuantity = bid.RawQuantity;
            }
            
            foreach (OpenOrder openOrder in openOrdersAccount.Orders)
            {
                Console.WriteLine($"OpenOrder:: Bid: {openOrder.IsBid}\t" +
                                  $"Price: {openOrder.RawPrice}\t" +
                                  $"Quantity: {openOrder.RawQuantity}\t" +
                                  $"OrderId: {openOrder.OrderId}\t" +
                                  $"ClientOrderId: {openOrder.ClientOrderId}");
            }
        }
    }
}