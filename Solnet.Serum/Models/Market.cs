using Solnet.Programs.Utilities;
using Solnet.Serum.Models.Flags;
using Solnet.Wallet;
using System;
using System.Diagnostics;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents a Market in Serum.
    /// </summary>
    [DebuggerDisplay("PubKey = {OwnAddress.Key}")]
    public class Market
    {
        #region Layout
        
        /// <summary>
        /// Represents the layout of the <see cref="Market"/> data structure.
        /// </summary>
        internal static class Layout
        {
            /// <summary>
            /// The size of the data for a market account.
            /// </summary>
            internal const int SpanLength = 388;
            
            /// <summary>
            /// The number of bytes of the padding at the beginning of the market structure.
            /// </summary>
            internal const int StartPadding = 5;

            /// <summary>
            /// The number of bytes of the padding at the end of the market structure.
            /// </summary>
            internal const int EndPadding = 7;

            /// <summary>
            /// The offset at which the market's own address begins.
            /// </summary>
            internal const int OwnAddressOffset = 8;

            /// <summary>
            /// The offset at which the vault signer's nonce begins.
            /// </summary>
            internal const int VaultSignerOffset = 10;

            /// <summary>
            /// The offset at which the public key of the market's base mint begins.
            /// </summary>
            internal const int BaseMintOffset = 48;

            /// <summary>
            /// The offset at which the public key of the market's quote mint begins.
            /// </summary>
            internal const int QuoteMintOffset = 80;
            
            /// <summary>
            /// The offset at which the public key of the market's base token vault begins.
            /// </summary>
            internal const int BaseVaultOffset = 112;
            
            /// <summary>
            /// The offset at which the value of the total base token deposits begins.
            /// </summary>
            internal const int BaseDepositsOffset = 144;
            
            /// <summary>
            /// The offset at which the value of the base token fees accrued begins.
            /// </summary>
            internal const int BaseFeesOffset = 152;
            
            /// <summary>
            /// The offset at which the public key of the market's quote token vault begins.
            /// </summary>
            internal const int QuoteVaultOffset = 160;
            
            /// <summary>
            /// The offset at which the value of the total quote token deposits begins.
            /// </summary>
            internal const int QuoteDepositsOffset = 192;
            
            /// <summary>
            /// The offset at which the value of the quote token fees accrued begins.
            /// </summary>
            internal const int QuoteFeesOffset = 200;
            
            /// <summary>
            /// The offset at which the value of the quote token dust threshold begins.
            /// </summary>
            internal const int QuoteDustThresholdOffset = 208;
            
            /// <summary>
            /// The offset at which the public key of the market's request queue account begins.
            /// </summary>
            internal const int RequestQueueOffset = 216;
            
            /// <summary>
            /// The offset at which the public key of the market's event queue account begins.
            /// </summary>
            internal const int EventQueueOffset = 248;
            
            /// <summary>
            /// The offset at which the public key of the market's bids account begins.
            /// </summary>
            internal const int BidsOffset = 280;
            
            /// <summary>
            /// The offset at which the public key of the market's asks account begins.
            /// </summary>
            internal const int AsksOffset = 312;
            
            /// <summary>
            /// The offset at which the value of the market's base token lot size begins.
            /// </summary>
            internal const int BaseLotOffset = 344;
            
            /// <summary>
            /// The offset at which the value of the market's quote token lot size begins.
            /// </summary>
            internal const int QuoteLotOffset = 352;
            
            /// <summary>
            /// The offset at which the value of the market's fee rate basis points begins.
            /// </summary>
            internal const int FeeRateBasisOffset = 360;
            
            /// <summary>
            /// The offset at which the value of the market's referrer rebate accrued begins.
            /// </summary>
            internal const int ReferrerRebateAccruedOffset = 368;
        }
        
        #endregion
        
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
            if (data.Length != Layout.SpanLength)
                return null;

            ReadOnlySpan<byte> padLessData = data.Slice(
                Layout.StartPadding,
                data.Length - (Layout.StartPadding + Layout.EndPadding));

            AccountFlags flags = AccountFlags.Deserialize(padLessData[..8]);

            Market market = new()
            {
                Flags = flags,
                OwnAddress = padLessData.GetPubKey(Layout.OwnAddressOffset),
                VaultSignerNonce = padLessData.GetU64(Layout.VaultSignerOffset),
                BaseMint = padLessData.GetPubKey(Layout.BaseMintOffset),
                QuoteMint = padLessData.GetPubKey(Layout.QuoteMintOffset),
                BaseVault = padLessData.GetPubKey(Layout.BaseVaultOffset),
                BaseDepositsTotal = padLessData.GetU64(Layout.BaseDepositsOffset),
                BaseFeesAccrued = padLessData.GetU64(Layout.BaseFeesOffset),
                QuoteVault = padLessData.GetPubKey(Layout.QuoteVaultOffset),
                QuoteDepositsTotal = padLessData.GetU64(Layout.QuoteDepositsOffset),
                QuoteFeesAccrued = padLessData.GetU64(Layout.QuoteFeesOffset),
                QuoteDustThreshold = padLessData.GetU64(Layout.QuoteDustThresholdOffset),
                RequestQueue = padLessData.GetPubKey(Layout.RequestQueueOffset),
                EventQueue = padLessData.GetPubKey(Layout.EventQueueOffset),
                Bids = padLessData.GetPubKey(Layout.BidsOffset),
                Asks = padLessData.GetPubKey(Layout.AsksOffset),
                BaseLotSize = padLessData.GetU64(Layout.BaseLotOffset),
                QuoteLotSize = padLessData.GetU64(Layout.QuoteLotOffset),
                FeeRateBasis = padLessData.GetU64(Layout.FeeRateBasisOffset),
                ReferrerRebateAccrued = padLessData.GetU64(Layout.ReferrerRebateAccruedOffset),
            };

            return market;
        }
    }
}