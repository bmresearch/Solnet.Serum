using System;
using System.Buffers.Binary;

namespace Solnet.Serum.Models.Flags
{
    /// <summary>
    /// Represents the account's flags.
    /// <remarks>
    /// According to this, the account can be a <see cref="Market"/>, an <see cref="OpenOrdersAccount"/>,
    /// an <see cref="EventQueue"/>, a <see cref="RequestQueue"/> or the Bids/Asks account for the order book.
    /// </remarks>
    /// </summary>
    public class AccountFlags : Flag
    {
        #region Flag Mask Values

        /// <summary>
        /// The value to check against to see if the account is initialized.
        /// </summary>
        private const int Initialized = 1;

        /// <summary>
        /// The value to check against to see if the account is a market account.
        /// </summary>
        private const int Market = 2;

        /// <summary>
        /// The value to check against to see if the account is an open orders account.
        /// </summary>
        private const int OpenOrders = 4;

        /// <summary>
        /// The value to check against to see if the account is a request queue account.
        /// </summary>
        private const int RequestQueue = 8;

        /// <summary>
        /// The value to check against to see if the account is an event queue account.
        /// </summary>
        private const int EventQueue = 16;

        /// <summary>
        /// The value to check against to see if the account is a bids account.
        /// </summary>
        private const int Bids = 32;

        /// <summary>
        /// The value to check against to see if the account is an asks account.
        /// </summary>
        private const int Asks = 64;

        #endregion

        /// <summary>
        /// Whether the account is initialized or not.
        /// </summary>
        public bool IsInitialized => (Bitmask & Initialized) == Initialized;
        
        /// <summary>
        /// Whether the account is a market account or not.
        /// </summary>
        public bool IsMarket => (Bitmask & Market) == Market;

        /// <summary>
        /// Whether the account is an open orders account or not.
        /// </summary>
        public bool IsOpenOrders => (Bitmask & OpenOrders) == OpenOrders;
        
        /// <summary>
        /// Whether the account is a request queue account or not.
        /// </summary>
        public bool IsRequestQueue => (Bitmask & RequestQueue) == RequestQueue;

        /// <summary>
        /// Whether the account is an event queue account or not.
        /// </summary>
        public bool IsEventQueue => (Bitmask & EventQueue) == EventQueue;
        
        /// <summary>
        /// Whether the account is a bids account or not.
        /// </summary>
        public bool IsBids => (Bitmask & Bids) == Bids;

        /// <summary>
        /// Whether the account is an asks account or not.
        /// </summary>
        public bool IsAsks => (Bitmask & Asks) == Asks;

        /// <summary>
        /// Initialize the account flags with the given bit mask.
        /// </summary>
        /// <param name="bitmask">The bit mask.</param>
        private AccountFlags(byte bitmask) : base(bitmask) { }

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="Market"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Market structure.</returns>
        internal static AccountFlags Deserialize(ReadOnlySpan<byte> data) 
            =>  new (data[0]);
    }
}