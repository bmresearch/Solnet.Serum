using System;
using System.Diagnostics;
using Solnet.Wallet;
using Solnet.Serum.Shared;

namespace Solnet.Serum.Models
{
    [DebuggerDisplay("PubKey = {OwnAddress.Key}")]
    public class Market
    {
        public const int SerializedLength = 388;
        public const int PadBytesAtStart = 5;   // Number of padding bytes at the start
        public const int PadBytesAtEnd   = 7;   // Number of padding bytes at the end

        public static class Layout
        {
            // The following offsets are after padding has been removed
            public const int AccountFlags          =   0;
            public const int OwnAddress            =   8;
            public const int VaultSigner           =  10;
            public const int BaseMint              =  48;
            public const int QuoteMint             =  80;
            public const int BaseVault             = 112;
            public const int BaseDeposits          = 144;
            public const int BaseFees              = 152;
            public const int QuoteVault            = 160;
            public const int QuoteDeposits         = 192;
            public const int QuoteFees             = 200;
            public const int QuoteDustThreshold    = 208;
            public const int RequestQueue          = 216;
            public const int EventQueue            = 248;
            public const int Bids                  = 280;
            public const int Asks                  = 312;
            public const int BaseLot               = 344;
            public const int QuoteLot              = 352;
            public const int FeeRateBasis          = 360;
            public const int ReferrerRebateAccrued = 368;
        }

        public AccountFlags Flags;                  // The flags that define the account type.
        public PublicKey    OwnAddress;             // The market's own address.
        public ulong        VaultSignerNonce;       // The vault signer nonce.
        public PublicKey    BaseMint;               // The public key of the base token mint of this market.
        public PublicKey    QuoteMint;              // The public key of the quote token mint of this market.
        public PublicKey    BaseVault;              // The public key of the base vault.
        public ulong        BaseDepositsTotal;      // The total deposits of the base token.
        public ulong        BaseFeesAccrued;        // The fees accrued by the base token.
        public PublicKey    QuoteVault;             // The public key of the quote vault.
        public ulong        QuoteDepositsTotal;     // The total deposits of the quote token.
        public ulong        QuoteFeesAccrued;       // The fees accrued by the quote token.
        public ulong        QuoteDustThreshold;     // The dust threshold of the quote token.
        public PublicKey    RequestQueue;           // The public key of the request queue of this market.
        public PublicKey    EventQueue;             // The public key of the event queue of this market.
        public PublicKey    Bids;                   // The public key of the market's bids account.
        public PublicKey    Asks;                   // The public key of the market's asks account.
        public ulong        BaseLotSize;            // The market's base token lot size.
        public ulong        QuoteLotSize;           // The market's quote token lot size.
        public ulong        FeeRateBasis;           // The market's fee rate in basis points.
        public ulong        ReferrerRebateAccrued;  // The market's referrer rebate accrued.

        public static Market Deserialize(ReadOnlySpan<byte> dataWithPadding)
        {
            if (dataWithPadding.Length != SerializedLength) { return null; }
            int lengthWithoutPadding = dataWithPadding.Length - (PadBytesAtStart + PadBytesAtEnd);    
            ReadOnlySpan<byte> data  = dataWithPadding.Slice(PadBytesAtStart, lengthWithoutPadding);

            Market market = new()
            {
                Flags                 = data.GetU64      (Layout.AccountFlags),
                OwnAddress            = data.GetPublicKey(Layout.OwnAddress),
                VaultSignerNonce      = data.GetU64      (Layout.VaultSigner),
                BaseMint              = data.GetPublicKey(Layout.BaseMint),
                QuoteMint             = data.GetPublicKey(Layout.QuoteMint),
                BaseVault             = data.GetPublicKey(Layout.BaseVault),
                BaseDepositsTotal     = data.GetU64      (Layout.BaseDeposits),
                BaseFeesAccrued       = data.GetU64      (Layout.BaseFees),
                QuoteVault            = data.GetPublicKey(Layout.QuoteVault),
                QuoteDepositsTotal    = data.GetU64      (Layout.QuoteDeposits),
                QuoteFeesAccrued      = data.GetU64      (Layout.QuoteFees),
                QuoteDustThreshold    = data.GetU64      (Layout.QuoteDustThreshold),
                RequestQueue          = data.GetPublicKey(Layout.RequestQueue),
                EventQueue            = data.GetPublicKey(Layout.EventQueue),
                Bids                  = data.GetPublicKey(Layout.Bids),
                Asks                  = data.GetPublicKey(Layout.Asks),
                BaseLotSize           = data.GetU64      (Layout.BaseLot),
                QuoteLotSize          = data.GetU64      (Layout.QuoteLot),
                FeeRateBasis          = data.GetU64      (Layout.FeeRateBasis),
                ReferrerRebateAccrued = data.GetU64      (Layout.ReferrerRebateAccrued)
            };

            return market;
        }
    }
}