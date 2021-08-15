using Solnet.Programs.Utilities;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements the system program data encodings.
    /// </summary>
    internal static class SerumProgramData
    {
        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.ConsumeEvents"/> method.
        /// </summary>
        /// <param name="limit">The maximum number of events to consume.</param>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeConsumeEventsData(ushort limit)
        {
            byte[] data = new byte[7];
            data.WriteU32((uint)SerumProgramInstructions.Values.ConsumeEvents, SerumProgramLayouts.MethodOffset);
            data.WriteU16(limit, SerumProgramLayouts.ConsumeEventsLimitOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.SettleFunds"/> method.
        /// </summary>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeSettleFundsData()
        {
            byte[] data = new byte[5];
            data.WriteU32((uint)SerumProgramInstructions.Values.SettleFunds, SerumProgramLayouts.MethodOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.NewOrderV3"/> method.
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The encoded data.</returns>
        public static byte[] EncodeNewOrderV3Data(Order order)
            => EncodeNewOrderV3Data(order.Side, order.RawPrice, order.RawQuantity, order.Type, order.ClientOrderId,
                order.SelfTradeBehavior, order.MaxQuoteQuantity, ushort.MaxValue);

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.NewOrderV3"/> method.
        /// </summary>
        /// <param name="side"></param>
        /// <param name="limitPrice"></param>
        /// <param name="maxCoinQty"></param>
        /// <param name="orderType"></param>
        /// <param name="clientOrderId"></param>
        /// <param name="selfTradeBehaviorType"></param>
        /// <param name="maxNativePcQtyIncludingFees"></param>
        /// <param name="limit">The maximum number of iterations of the Serum order matching loop.</param>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeNewOrderV3Data(Side side, ulong limitPrice, ulong maxCoinQty,
            OrderType orderType, ulong clientOrderId, SelfTradeBehavior selfTradeBehaviorType,
            ulong maxNativePcQtyIncludingFees, ushort limit)
        {
            byte[] data = new byte[51];
            data.WriteU32((uint)SerumProgramInstructions.Values.NewOrderV3, SerumProgramLayouts.MethodOffset);
            data.WriteU8((byte)side, SerumProgramLayouts.NewOrderV3.SideOffset);
            data.WriteU64(limitPrice, SerumProgramLayouts.NewOrderV3.PriceOffset);
            data.WriteU64(maxCoinQty, SerumProgramLayouts.NewOrderV3.MaxBaseQuantityOffset);
            data.WriteU64(maxNativePcQtyIncludingFees, SerumProgramLayouts.NewOrderV3.MaxQuoteQuantity);
            data.WriteU8((byte)selfTradeBehaviorType, SerumProgramLayouts.NewOrderV3.SelfTradeBehaviorOffset);
            data.WriteU8((byte)orderType, SerumProgramLayouts.NewOrderV3.OrderTypeOffset);
            data.WriteU64(clientOrderId, SerumProgramLayouts.NewOrderV3.ClientIdOffset);
            data.WriteU16(limit, SerumProgramLayouts.NewOrderV3.LimitOffset);
            return data;
        }


        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.CancelOrderV2"/> method.
        /// </summary>
        /// <param name="side">The order's side.</param>
        /// <param name="clientOrderId">The client's order id.</param>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeCancelOrderV2Data(Side side, BigInteger clientOrderId)
        {
            byte[] data = new byte[25];
            data.WriteU32((uint)SerumProgramInstructions.Values.CancelOrderV2, SerumProgramLayouts.MethodOffset);
            data.WriteU32((uint)side, SerumProgramLayouts.CancelOrderV2.SideOffset);
            data.WriteBigInt(clientOrderId, SerumProgramLayouts.CancelOrderV2.OrderIdOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.CancelOrderByClientIdV2"/> method.
        /// </summary>
        /// <param name="clientOrderId">The client's <c>orderId</c>.</param>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeCancelOrderByClientIdV2Data(ulong clientOrderId)
        {
            byte[] data = new byte[13];
            data.WriteU32((uint)SerumProgramInstructions.Values.CancelOrderByClientIdV2, SerumProgramLayouts.MethodOffset);
            data.WriteU64(clientOrderId, SerumProgramLayouts.CancelOrderByClientIdV2ClientIdOffset);
            return data;
        }
        
        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.CloseOpenOrders"/> method.
        /// </summary>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeCloseOpenOrdersData()
        {
            byte[] data = new byte[5];
            data.WriteU32((uint)SerumProgramInstructions.Values.CloseOpenOrders, SerumProgramLayouts.MethodOffset);
            return data;
        }
        
        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.InitOpenOrders"/> method.
        /// </summary>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodeInitOpenOrdersData()
        {
            byte[] data = new byte[5];
            data.WriteU32((uint)SerumProgramInstructions.Values.InitOpenOrders, SerumProgramLayouts.MethodOffset);
            return data;
        }
        
        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.Values.Prune"/> method.
        /// </summary>
        /// <param name="limit">The maximum number of orders to cancel.</param>
        /// <returns>The encoded data.</returns>
        internal static byte[] EncodePruneData(ushort limit)
        {
            byte[] data = new byte[7];
            data.WriteU32((uint)SerumProgramInstructions.Values.Prune, SerumProgramLayouts.MethodOffset);
            data.WriteU16(limit, SerumProgramLayouts.PruneLimitOffset);
            return data;
        }

        /// <summary>
        /// Derive the vault signer address for the given market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns>The vault signer address.</returns>
        /// <exception cref="Exception">Throws exception when unable to derive the vault signer address.</exception>
        internal static byte[] DeriveVaultSignerAddress(Market market)
        {
            bool success = AddressExtensions.TryCreateProgramAddress(
                new List<byte[]> { market.OwnAddress.KeyBytes, BitConverter.GetBytes(market.VaultSignerNonce) },
                SerumProgram.ProgramIdKey.KeyBytes, out byte[] vaultSignerAddress);
            return !success ? null : vaultSignerAddress;
        }
    }
}