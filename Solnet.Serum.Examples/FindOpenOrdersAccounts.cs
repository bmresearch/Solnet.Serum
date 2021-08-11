using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;

namespace Solnet.Serum.Examples
{
    public class FindOpenOrdersAccounts : IRunnableExample
    {
        private static readonly IRpcClient RpcClient = Solnet.Rpc.ClientFactory.GetClient(Cluster.MainNet);

        private const string SerumV3Address = "9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin";
        private const string MarketAddress = "9wFFyRfZBsuAha4YcuxcXLKwMxJR43S7fPfQLusDBzvT";
        private const string OwnerAddress = "hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh";
        private const int OpenOrdersAccountSize = 3228;
        
        public FindOpenOrdersAccounts()
        {
        }
        
        public void Run()
        {
            List<MemCmp> filters = new ()
            {
                new MemCmp{ Offset = 13, Bytes = MarketAddress }, new MemCmp{ Offset = 45, Bytes = OwnerAddress }
            };
            RequestResult<List<AccountKeyPair>> accounts = RpcClient.GetProgramAccounts(SerumV3Address, dataSize: OpenOrdersAccountSize, memCmpList: filters);

            foreach (AccountKeyPair account in accounts.Result)
            {
                Console.WriteLine($"---------------------");
                Console.WriteLine($"OpenOrdersAccount: {account.PublicKey} - Owner: {account.Account.Owner}");
                OpenOrdersAccount ooa = OpenOrdersAccount.Deserialize(Convert.FromBase64String(account.Account.Data[0]));
                Console.WriteLine($"OpenOrdersAccount:: Owner: {ooa.Owner.Key} Market: {ooa.Market.Key}\n" +
                                  $"BaseTotal: {ooa.BaseTokenTotal} BaseFree: {ooa.BaseTokenFree}\n" +
                                  $"QuoteTotal: {ooa.QuoteTokenTotal} QuoteFree: {ooa.QuoteTokenFree}");

                foreach (OpenOrder openOrder in ooa.Orders)
                {
                    Console.WriteLine($"OpenOrder:: IsBid: {openOrder.IsBid} Price: {openOrder.RawPrice} OrderId: {openOrder.OrderId} ClientOrderId: {openOrder.ClientOrderId}");
                }
            }
        }
    }
}