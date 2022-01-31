using Solnet.KeyStore;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    public class CloseOpenOrdersExample : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("HXBi8YBwbh4TXF6PjVw81m8Z3Cc4WBofvauj5SBFdgUs");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly Wallet.Wallet _wallet;
        private readonly SolanaKeyStoreService _keyStore;
        private readonly SerumProgram _serum;

        public CloseOpenOrdersExample()
        {
            Console.WriteLine($"Initializing {ToString()}");
            // init stuff
            _keyStore = new SolanaKeyStoreService();
            _serum = SerumProgram.CreateMainNet();

            // get the wallet
            _wallet = _keyStore.RestoreKeystoreFromFile("/path/to/wallet.json");

            // serum client
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, _wallet.Account, signatureMethod: SignRequest, serumClient: _serumClient);
            _marketManager.InitAsync().Wait();
        }

        private byte[] SignRequest(ReadOnlySpan<byte> messageData)
        {
            Console.WriteLine("Message Data: " + Convert.ToBase64String(messageData));
            
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

            byte[] signature = _wallet.Account.Sign(messageData.ToArray());
            Console.WriteLine("Message Signature: " + Convert.ToBase64String(signature));

            return signature;
        }

        public async Task CloseAllOpenOrders()
        {
            List<MemCmp> filters = new()
            {
                new MemCmp { Offset = 13, Bytes = _marketAddress },
                new MemCmp { Offset = 45, Bytes = _wallet.Account.PublicKey }
            };
            RequestResult<List<AccountKeyPair>> accounts = await
                _serumClient.RpcClient.GetProgramAccountsAsync(SerumProgram.MainNetProgramIdKeyV3,
                    dataSize: OpenOrdersAccount.Layout.SpanLength, memCmpList: filters);

            Console.WriteLine($"Found {accounts.Result.Count} open orders account for market {_marketAddress}");

            foreach (var openOrdersAccount in accounts.Result)
            {
                var blockhash = await _serumClient.RpcClient.GetRecentBlockHashAsync();

                Console.WriteLine($"Closing open orders account with address {openOrdersAccount.PublicKey}");

                var txBytes = new TransactionBuilder()
                    .SetFeePayer(_wallet.Account)
                    .SetRecentBlockHash(blockhash.Result.Value.Blockhash)
                    .AddInstruction(_serum.SettleFunds(
                        _marketManager.Market,
                        new (openOrdersAccount.PublicKey),
                        _wallet.Account,
                        _marketManager.BaseTokenAccountAddress,
                        _marketManager.QuoteTokenAccountAddress))
                    .AddInstruction(_serum.CloseOpenOrders(
                        new (openOrdersAccount.PublicKey),
                        _wallet.Account,
                        _wallet.Account,
                        _marketAddress))
                    .CompileMessage();

                var signature = SignRequest(txBytes);

                var tx = Transaction.Populate(
                    Message.Deserialize(txBytes), new List<byte[]> { signature });

                var res = await _serumClient.RpcClient.SendTransactionAsync(tx.Serialize());
            }
        }

        public async Task CreateOpenOrders()
        {
            var blockhash = await _serumClient.RpcClient.GetRecentBlockHashAsync();
            var openOrdersAccount = new Account();

            var rentExemption = await _serumClient.RpcClient.GetMinimumBalanceForRentExemptionAsync(OpenOrdersAccount.Layout.SpanLength);

            var txBytes = new TransactionBuilder()
                .SetFeePayer(_wallet.Account)
                .SetRecentBlockHash(blockhash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.CreateAccount(
                    _wallet.Account,
                    openOrdersAccount,
                    rentExemption.Result,
                    OpenOrdersAccount.Layout.SpanLength,
                    _serum.ProgramIdKey))
                .AddInstruction(_serum.InitOpenOrders(
                    openOrdersAccount,
                    _wallet.Account,
                    _marketAddress))
                .CompileMessage();

            var signature = SignRequest(txBytes);

            var tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signature, openOrdersAccount.Sign(txBytes) });

            var res = await _serumClient.RpcClient.SendTransactionAsync(tx.Serialize());
        }


        public async void Run()
        {
            var blockhash = await _serumClient.RpcClient.GetRecentBlockHashAsync();

            while (_marketManager.OpenOrdersAddress == null)
            {
                await Task.Delay(100);
            }
            
            var cancelRes = await _marketManager.CancelAllOrdersAsync();
            foreach (var sig in cancelRes)
            {
                sig.ConfirmationChanged += (sender, status) =>
                {
                    Console.WriteLine($"Confirmation for {sig.Signature} changed.\nTxErr: {status.TransactionError?.Type}\tIxErr: {status.InstructionError?.CustomError}\tSerumErr: {status.Error}");
                };
            }

            while (cancelRes.Any(confirmation => confirmation.ConfirmationResult == null))
            {
                await Task.Delay(100);
            }

            var txBytes = new TransactionBuilder()
                .SetFeePayer(_wallet.Account)
                .SetRecentBlockHash(blockhash.Result.Value.Blockhash)
                .AddInstruction(_serum.CloseOpenOrders(
                    _marketManager.OpenOrdersAddress, _wallet.Account, _wallet.Account, _marketAddress))
                .CompileMessage();

            var signature = SignRequest(txBytes);

            var tx = Transaction.Populate(
                Message.Deserialize(txBytes), new List<byte[]> { signature });

            var res = await _serumClient.RpcClient.SendTransactionAsync(tx.Serialize());
            
            Console.ReadKey();
        }
    }
}