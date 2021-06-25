using Solnet.Serum.Models;

namespace Solnet.Serum.Layouts
{
    /// <summary>
    /// Represents the layout of the <see cref="Market"/> data structure.
    /// </summary>
    internal static class MarketDataLayout
    {
        /// <summary>
        /// The size of the data for a market account.
        /// </summary>
        internal const int MarketAccountDataSize = 388;
        
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
}