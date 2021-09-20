using Solnet.Programs.Utilities;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;

namespace Solnet.Serum
{
    /// <summary>
    /// A utility class used to perform certain value conversions related to a Serum <see cref="Market"/>.
    /// </summary>
    public static class MarketUtils
    {
        /// <summary>
        /// The <see cref="PublicKey"/> of the Wrapped SOL token mint.
        /// </summary>
        public static PublicKey WrappedSolMint = new ("So11111111111111111111111111111111111111112");

        /// <summary>
        /// The number of lamports in each SOL.
        /// </summary>
        public const int LamportsPerSol = 1_000_000_000;
        
        /// <summary>
        /// The offset at which the token mint decimals value begins.
        /// </summary>
        public const int TokenMintDecimalsOffset = 44;
        
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
        /// Converts price from a raw value to a friendly number according to the given decimals and lot sizes.
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
        /// Converts quantity from a raw value to a friendly number according to the given decimals and lot sizes.
        /// </summary>
        /// <param name="quantity">The quantity of the order.</param>
        /// <param name="baseLotSize">The lot size of the base token.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <returns>The price as a float.</returns>
        public static float QuantityLotsToNumber(ulong quantity, ulong baseLotSize, byte baseDecimals)
            => (float)(quantity * baseLotSize / GetSplTokenMultiplier(baseDecimals));
        
        /// <summary>
        /// Converts price from a friendly number to the corresponding raw value according to the given decimals and market.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="market">The market.</param>
        /// <returns>The raw price.</returns>
        public static ulong PriceNumberToLots(float price, byte baseDecimals, byte quoteDecimals, Market market)
            => PriceNumberToLots(price, baseDecimals, quoteDecimals, market.BaseLotSize, market.QuoteLotSize);
        
        /// <summary>
        /// Converts price from a friendly number to the corresponding raw value according to the given decimals and lot sizes.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="baseLotSize">The lot size of the base token.</param>
        /// <param name="quoteLotSize">The lot size of the quote token.</param>
        /// <returns>The raw price.</returns>
        public static ulong PriceNumberToLots(float price, byte baseDecimals, byte quoteDecimals, ulong baseLotSize, ulong quoteLotSize)
        {
            double top = price * GetSplTokenMultiplier(quoteDecimals) * baseLotSize;
            double bottom = GetSplTokenMultiplier(baseDecimals) * quoteLotSize;
            return (ulong) Math.Round(top / bottom);
        }

        /// <summary>
        /// Converts quantity from a friendly number to the corresponding raw value according to the given base decimals and lot size.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="baseLotSize">The lot size of the base token.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <returns>The raw quantity.</returns>
        public static ulong QuantityNumberToLots(float quantity, byte baseDecimals, ulong baseLotSize) 
            => (ulong)(Math.Round(quantity * GetSplTokenMultiplier(baseDecimals)) / baseLotSize);

        /// <summary>
        /// Gets the maximum quote quantity for the given quote lot size, quantity and price as lots.
        /// </summary>
        /// <param name="quoteLotSize">The lot size of the quote token.</param>
        /// <param name="quantityAsLots">The raw quantity of the order.</param>
        /// <param name="priceAsLots">The raw price of the order.</param>
        /// <returns>The maximum quote quantity.</returns>
        public static ulong GetMaxQuoteQuantity(ulong quoteLotSize, ulong quantityAsLots, ulong priceAsLots)
            => quoteLotSize * quantityAsLots * priceAsLots;
        
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
                price = HumanizeRawTradePrice(priceBeforeFees, baseDecimals, quoteDecimals,
                    evt.NativeQuantityReleased);
                quantity = HumanizeRawTradeQuantity(evt.NativeQuantityReleased, baseDecimals);
                side = evt.Flags.IsMaker ? Side.Sell : Side.Buy;
            }
            else
            {
                ulong priceBeforeFees = evt.Flags.IsMaker
                    ? evt.NativeQuantityReleased - evt.NativeFeeOrRebate
                    : evt.NativeQuantityReleased + evt.NativeFeeOrRebate;
                price = HumanizeRawTradePrice(priceBeforeFees, baseDecimals, quoteDecimals,
                    evt.NativeQuantityPaid);
                quantity = HumanizeRawTradeQuantity(evt.NativeQuantityPaid, baseDecimals);
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
        /// Converts the price before fees of a trade from a raw value to a friendly number according to the given decimals
        /// and the amount paid or released by the trade event.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="paidOrReleased">The amount paid or released by the trade event.</param>
        /// <returns>The price as a float.</returns>
        public static float HumanizeRawTradePrice(ulong price, byte baseDecimals, byte quoteDecimals, ulong paidOrReleased)
        {
            double top = price * GetSplTokenMultiplier(baseDecimals);
            double bottom = GetSplTokenMultiplier(quoteDecimals) * paidOrReleased;
            return (float)(top / bottom);
        }
        
        /// <summary>
        /// Converts the quantity released or paid by a trade from a raw value to a friendly number according to the given decimals.
        /// </summary>
        /// <param name="quantity">The quantity released or paid by the trade event.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <returns>The price as a float.</returns>
        public static float HumanizeRawTradeQuantity(ulong quantity, byte baseDecimals)
            => (float)(quantity / GetSplTokenMultiplier(baseDecimals));
        
        /// <summary>
        /// Converts all order friendly numbers to the corresponding raw value for the given decimals and market.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="baseDecimals">The decimals of the base token.</param>
        /// <param name="quoteDecimals">The decimals of the quote token.</param>
        /// <param name="market">The market.</param>
        public static void ConvertOrderValues(this Order order, byte baseDecimals, byte quoteDecimals, Market market)
        {
            order.RawPrice = PriceNumberToLots(order.Price, baseDecimals, quoteDecimals, market);
            order.RawQuantity = QuantityNumberToLots(order.Quantity, baseDecimals, market.BaseLotSize);
            order.MaxQuoteQuantity = GetMaxQuoteQuantity(market.QuoteLotSize, order.RawQuantity, order.RawPrice);
        }

        /// <summary>
        /// Gets the minimum number of lamports necessary to wrap in order to submit an order in a market where either
        /// the base or the quote token are the <see cref="MarketUtils.WrappedSolMint"/>.
        /// </summary>
        /// <param name="price">The price of the order.</param>
        /// <param name="size">The size of the order.</param>
        /// <param name="side">The side of the order.</param>
        /// <param name="openOrdersAccount">The associated open orders account.</param>
        /// <returns>The minimum number of lamports necessary to perform the trade.</returns>
        public static ulong GetMinimumLamportsForWrapping(float price, float size, Side side,
            OpenOrdersAccount openOrdersAccount)
            => side switch
            {
                Side.Buy => (ulong)Math.Round(price * size * 1.01 * LamportsPerSol) -
                            (openOrdersAccount?.QuoteTokenFree ?? 0),
                Side.Sell => (ulong)Math.Round(size * LamportsPerSol) - 
                             (openOrdersAccount?.BaseTokenFree ?? 0),
            };
    }
}