using Solnet.Serum.Models;

namespace Solnet.Serum
{
    /// <summary>
    /// Represents the instruction types for the <see cref="SerumProgram"/> along with a friendly name so as not to use reflection.
    /// <remarks>
    /// For more information see:
    /// https://github.com/project-serum/serum-dex/
    /// https://github.com/project-serum/serum-dex/blob/master/dex/src/instruction.rs
    /// </remarks>
    /// </summary>
    internal static class SerumProgramInstructions 
    {
        /// <summary>
        /// Represents the user-friendly names for the instruction types for the <see cref="SerumProgram"/>.
        /// </summary>
        internal static string[] Names = new[]
        {
            "Initialize Market",
            "New Order",
            "Match Orders",
            "Consume Events",
            "Cancel Order",
            "Settle Funds",
            "Cancel Order By Client Id",
            "Disable Market",
            "Sweep Fees",
            "New Order V2",
            "New Order V3",
            "Cancel Order V2",
            "Cancel Order By Client Id V2",
            "Send Take",
            "Close Open Orders",
            "Init Open Orders",
            "Prune"
        };
        
        /// <summary>
        /// Represents the instruction types for the <see cref="SerumProgram"/>.
        /// </summary>
        public enum Values : byte
        {
            /// <summary>
            /// An instruction which is used to initialize a new market.
            /// </summary>
            InitializeMarket = 0,
            
            /// <summary>
            /// An instruction which sets a new order (v1).
            /// </summary>
            NewOrder = 1,
            
            /// <summary>
            /// An instruction which is used to match orders.
            /// </summary>
            MatchOrders = 2,
        
            /// <summary>
            /// An instruction which is used to consume events from the event queue.
            /// </summary>
            ConsumeEvents = 3,
            
            /// <summary>
            /// An instruction which is used to cancel an order.
            /// </summary>
            CancelOrder = 4,
        
            /// <summary>
            /// An instruction which is used to settle funds.
            /// </summary>
            SettleFunds = 5,
            
            /// <summary>
            /// An instruction which cancels an order by it's client id (v1).
            /// </summary>
            CancelOrderByClientId = 6,
            
            /// <summary>
            /// An instruction which is used to disable a market.
            /// </summary>
            DisableMarket = 7,
            
            /// <summary>
            /// An instruction which is used to sweep fees.
            /// </summary>
            SweepFees = 8,

            /// <summary>
            /// An instruction which sets a new order (v2).
            /// </summary>
            NewOrderV2 = 9,
        
            /// <summary>
            /// An instruction which sets a new order (v3).
            /// </summary>
            NewOrderV3 = 10,
        
            /// <summary>
            /// An instruction which cancels an order (v2).
            /// </summary>
            CancelOrderV2 = 11,
            
            /// <summary>
            /// An instruction which cancels an order by it's client id (v2).
            /// </summary>
            CancelOrderByClientIdV2 = 12,
            
            /// <summary>
            /// An instruction which takes liquidity at a certain limit price, limited by maximum coin and price coin quantities.
            /// </summary>
            SendTake = 13,

            /// <summary>
            /// An instruction which closes an open orders account.
            /// </summary>
            CloseOpenOrders = 14,

            /// <summary>
            /// An instruction which initializes an open orders account.
            /// </summary>
            InitOpenOrders = 15,
            
            /// <summary>
            /// 
            /// </summary>
            Prune = 16,
        }
    }
}