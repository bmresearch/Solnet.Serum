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
    public class CloseOpenOrdersExample : IRunnableExample
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

        public CloseOpenOrdersExample()
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

            var txBytes = new TransactionBuilder()
                .SetFeePayer(_wallet.Account)
                .SetRecentBlockHash(blockhash.Result.Value.Blockhash)
                .AddInstruction(SerumProgram.CloseOpenOrders(
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