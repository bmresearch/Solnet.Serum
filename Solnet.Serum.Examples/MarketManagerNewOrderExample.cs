using Solnet.KeyStore;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solnet.Serum.Examples
{
    public class MarketManagerNewOrderExample : IRunnableExample
    {
        private readonly PublicKey _marketAddress = new("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ");
        private readonly ISerumClient _serumClient;
        private readonly IMarketManager _marketManager;
        private readonly Wallet.Wallet _wallet;
        private bool _newOrderConfirmed;
        private bool _cancelOrderConfirmed;

        public MarketManagerNewOrderExample()
        {
            Console.WriteLine($"Initializing {ToString()}");
            // init stuff
            SolanaKeyStoreService keyStore = new ();
            
            // get the wallet
            _wallet = keyStore.RestoreKeystoreFromFile("/path/to/wallet.json");

            // serum client
            _serumClient = ClientFactory.GetClient(Cluster.MainNet);
            _serumClient.ConnectAsync().Wait();
            
            // initialize market manager
            _marketManager = MarketFactory.GetMarket(_marketAddress, _wallet.Account, SignRequest, serumClient: _serumClient);
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
        
        public void Run()
        {
            Order order = new OrderBuilder()
                .SetPrice(0.017f)
                .SetQuantity(1)
                .SetSide(Side.Buy)
                .SetOrderType(OrderType.Limit)
                .SetSelfTradeBehavior(SelfTradeBehavior.DecrementTake)
                .SetClientOrderId(1_000_000UL)
                .Build();

            SignatureConfirmation sigConf = _marketManager.NewOrder(order);
            sigConf.ConfirmationChanged += NewOrderSignatureConfirmationOnConfirmationChanged;

            while (!_newOrderConfirmed)
            {
                Task.Delay(250);
            }
            
            /**/
            SignatureConfirmation cancelSigConf = _marketManager.CancelOrder(1_000_000UL);
            cancelSigConf.ConfirmationChanged += CancelOrderSignatureConfirmationOnConfirmationChanged;
            
            while (!_cancelOrderConfirmed)
            {
                Task.Delay(250);
            }
            
            
            Console.ReadKey();
        }
        
        private void CancelOrderSignatureConfirmationOnConfirmationChanged(object sender, SignatureConfirmationStatus e)
        {
            Console.WriteLine($"TxErr: {e.TransactionError?.Type}\n\tIxErr: {e.InstructionError?.CustomError}\n\t\tSerumErr: {e.Error}");
            _cancelOrderConfirmed = true;
        }

        private void NewOrderSignatureConfirmationOnConfirmationChanged(object sender, SignatureConfirmationStatus e)
        {
            Console.WriteLine($"TxErr: {e.TransactionError?.Type}\n\tIxErr: {e.InstructionError?.CustomError}\n\t\tSerumErr: {e.Error}");
            _newOrderConfirmed = true;
        }
    }
}