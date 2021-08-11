using Solnet.Programs.Utilities;
using Solnet.Serum.Models;
using System;

namespace Solnet.Serum
{
    /// <summary>
    /// A utility class used to perform certain value conversions related to a Serum <see cref="Market"/>.
    /// </summary>
    public static class MarketUtils
    {
        /// <summary>
        /// The offset at which the token mint decimals value begins.
        /// </summary>
        private const int TokenMintDecimalsOffset = 44;
        
        /// <summary>
        /// Gets the token's decimals from the token mint data.
        /// </summary>
        /// <param name="data">The token mint account data.</param>
        /// <returns>A byte representing the decimals.</returns>
        public static byte DecimalsFromTokenMintData(ReadOnlySpan<byte> data)
            => data.GetU8(TokenMintDecimalsOffset);

        /// <summary>
        /// Gets the multiplier for a SPL Token.
        /// </summary>
        /// <param name="decimals">The number of decimals for the token.</param>
        /// <returns>The multiplier.</returns>
        public static double GetSplTokenMultiplier(byte decimals) 
            => Math.Pow(10, decimals);

        /// <summary>
        /// Converts an order book price from a raw value to a friendly number according to the given decimals and lot sizes.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="baseLotSize">The lot size of the base token.</param>
        /// <param name="quoteLotSize">The lot size of the quote token.</param>
        /// <returns>The price as a float.</returns>
        public static float PriceLotsToNumber(ulong price, byte baseDecimals, byte quoteDecimals, ulong baseLotSize,
            ulong quoteLotSize)
        {
            double top = price * quoteLotSize * GetSplTokenMultiplier(baseDecimals);
            double bottom = baseLotSize * GetSplTokenMultiplier(quoteDecimals);

            return (float)(top / bottom);
        }
        
        /// <summary>
        /// Converts an order book quantity from a raw value to a friendly number according to the given decimals and lot sizes.
        /// </summary>
        /// <param name="quantity">The quantity of the order.</param>
        /// <param name="baseLotSize">The lot size of the base token.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <returns>The price as a float.</returns>
        public static float QuantityLotsToNumber(ulong quantity, ulong baseLotSize, byte baseDecimals)
            => (float)(quantity * baseLotSize/ GetSplTokenMultiplier(baseDecimals));
        
        /// <summary>
        /// Processes an event from a Serum <see cref="EventQueue"/> into a trade event with the raw values converted to a friendly format.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <returns>The processed trade event.</returns>
        public static TradeEvent ProcessTradeEvent(Event evt, byte baseDecimals, byte quoteDecimals)
        {
            float price;
            float quantity;
            Side side;
                    
            if (evt.Flags.IsBid)
            {
                ulong priceBeforeFees = evt.Flags.IsMaker
                    ? evt.NativeQuantityPaid + evt.NativeFeeOrRebate
                    : evt.NativeQuantityPaid - evt.NativeFeeOrRebate;
                price = FromRawPrice(priceBeforeFees, baseDecimals, quoteDecimals,
                    evt.NativeQuantityReleased);
                quantity = FromRawQuantity(evt.NativeQuantityReleased, baseDecimals);
                side = evt.Flags.IsMaker ? Side.Sell : Side.Buy;
            }
            else
            {
                ulong priceBeforeFees = evt.Flags.IsMaker
                    ? evt.NativeQuantityReleased - evt.NativeFeeOrRebate
                    : evt.NativeQuantityReleased + evt.NativeFeeOrRebate;
                price = FromRawPrice(priceBeforeFees, baseDecimals, quoteDecimals,
                    evt.NativeQuantityPaid);
                quantity = FromRawQuantity(evt.NativeQuantityPaid, baseDecimals);
                side = evt.Flags.IsMaker ? Side.Buy : Side.Sell;
            }

            return new TradeEvent
            {
                Price = price,
                Size = quantity,
                Side = side,
                Event = evt
            };
        }
        
        /// <summary>
        /// Converts the price from a raw value to a friendly number according to the given decimals
        /// and the amount paid or released by the trade event.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="paidOrReleased">The amount paid or released by the trade event.</param>
        /// <returns>The price as a float.</returns>
        public static float FromRawPrice(ulong price, byte baseDecimals, byte quoteDecimals, ulong paidOrReleased)
        {
            double top = price * GetSplTokenMultiplier(baseDecimals);
            double bottom = GetSplTokenMultiplier(quoteDecimals) * paidOrReleased;
            return (float)(top / bottom);
        }
        
        /// <summary>
        /// Converts the quantity from a raw value to a friendly number according to the given decimals
        /// and the amount paid or released by the trade event.
        /// </summary>
        /// <param name="quantity">The quantity released or paid by the trade event.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <returns>The price as a float.</returns>
        public static float FromRawQuantity(ulong quantity, byte baseDecimals)
            => (float)(quantity / GetSplTokenMultiplier(baseDecimals));
    }
}