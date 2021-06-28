using Solnet.Serum.Layouts;
using Solnet.Serum.Models.Flags;
using Solnet.Wallet;
using System;
using System.Buffers.Binary;
using System.Diagnostics;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a Market in Serum.
    /// </summary>
    [DebuggerDisplay("PubKey = {OwnAddress.Key}")]
    public class Market
    {
        /// <summary>
        /// The flags that define the account type.
        /// </summary>
        public AccountFlags Flags;

        /// <summary>
        /// The market's own address.
        /// </summary>
        public PublicKey OwnAddress;

        /// <summary>
        /// The vault signer nonce.
        /// </summary>
        public ulong VaultSignerNonce;

        /// <summary>
        /// The public key of the base token mint of this market.
        /// </summary>
        public PublicKey BaseMint;

        /// <summary>
        /// The public key of the quote token mint of this market.
        /// </summary>
        public PublicKey QuoteMint;

        /// <summary>
        /// The public key of the base vault.
        /// </summary>
        public PublicKey BaseVault;

        /// <summary>
        /// The total deposits of the base token.
        /// </summary>
        public ulong BaseDepositsTotal;

        /// <summary>
        /// The fees accrued by the base token.
        /// </summary>
        public ulong BaseFeesAccrued;

        /// <summary>
        /// The public key of the quote vault.
        /// </summary>
        public PublicKey QuoteVault;

        /// <summary>
        /// The total deposits of the quote token.
        /// </summary>
        public ulong QuoteDepositsTotal;

        /// <summary>
        /// The fees accrued by the quote token.
        /// </summary>
        public ulong QuoteFeesAccrued;

        /// <summary>
        /// The dust threshold of the quote token.
        /// </summary>
        public ulong QuoteDustThreshold;

        /// <summary>
        /// The public key of the request queue of this market.
        /// </summary>
        public PublicKey RequestQueue;

        /// <summary>
        /// The public key of the event queue of this market.
        /// </summary>
        public PublicKey EventQueue;

        /// <summary>
        /// The public key of the market's bids account.
        /// </summary>
        public PublicKey Bids;

        /// <summary>
        /// The public key of the market's asks account.
        /// </summary>
        public PublicKey Asks;
        
        /// <summary>
        /// The market's base token lot size.
        /// </summary>
        public ulong BaseLotSize;
        
        /// <summary>
        /// The market's quote token lot size.
        /// </summary>
        public ulong QuoteLotSize;
        
        /// <summary>
        /// The market's fee rate in basis points.
        /// </summary>
        public ulong FeeRateBasis;
        
        /// <summary>
        /// The market's referrer rebate accrued.
        /// </summary>
        public ulong ReferrerRebateAccrued;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="Market"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Market structure.</returns>
        public static Market Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != MarketDataLayout.MarketAccountSpanLength)
                return null;

            ReadOnlySpan<byte> padLessData = data.Slice(
                MarketDataLayout.StartPadding,
                data.Length - (MarketDataLayout.StartPadding + MarketDataLayout.EndPadding));

            AccountFlags flags = AccountFlags.Deserialize(padLessData[..8]);

            Market market = new()
            {
                Flags = flags,
                OwnAddress = new PublicKey(padLessData.Slice(MarketDataLayout.OwnAddressOffset, 32).ToArray()),
                VaultSignerNonce = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.VaultSignerOffset, 8)),
                BaseMint = new PublicKey(padLessData.Slice(MarketDataLayout.BaseMintOffset, 32).ToArray()),
                QuoteMint = new PublicKey(padLessData.Slice(MarketDataLayout.QuoteMintOffset, 32).ToArray()),
                BaseVault = new PublicKey(padLessData.Slice(MarketDataLayout.BaseVaultOffset, 32).ToArray()),
                BaseDepositsTotal = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.BaseDepositsOffset, 8)),
                BaseFeesAccrued = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.BaseFeesOffset, 8)),
                QuoteVault = new PublicKey(padLessData.Slice(MarketDataLayout.QuoteVaultOffset, 32).ToArray()),
                QuoteDepositsTotal = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.QuoteDepositsOffset, 8)),
                QuoteFeesAccrued = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.QuoteFeesOffset, 8)),
                QuoteDustThreshold = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.QuoteDustThresholdOffset, 8)),
                RequestQueue = new PublicKey(padLessData.Slice(MarketDataLayout.RequestQueueOffset, 32).ToArray()),
                EventQueue = new PublicKey(padLessData.Slice(MarketDataLayout.EventQueueOffset, 32).ToArray()),
                Bids = new PublicKey(padLessData.Slice(MarketDataLayout.BidsOffset, 32).ToArray()),
                Asks = new PublicKey(padLessData.Slice(MarketDataLayout.AsksOffset, 32).ToArray()),
                BaseLotSize = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.BaseLotOffset, 8)),
                QuoteLotSize = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.QuoteLotOffset, 8)),
                FeeRateBasis = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.FeeRateBasisOffset, 8)),
                ReferrerRebateAccrued = BinaryPrimitives.ReadUInt64LittleEndian(
                    padLessData.Slice(MarketDataLayout.ReferrerRebateAccruedOffset, 8)),
            };

            return market;
        }
    }
}