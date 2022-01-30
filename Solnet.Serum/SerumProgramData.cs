using Solnet.Programs;
using Solnet.Programs.Utilities;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements the system program data encodings.
    /// </summary>
    public static class SerumProgramData
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
        public static byte[] EncodeNewOrderV3Data(Side side, ulong limitPrice, ulong maxCoinQty,
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
        /// <param name="orderId">The client's order id.</param>
        /// <returns>The encoded data.</returns>
        public static byte[] EncodeCancelOrderV2Data(Side side, BigInteger orderId)
        {
            byte[] data = new byte[25];
            data.WriteU32((uint)SerumProgramInstructions.Values.CancelOrderV2, SerumProgramLayouts.MethodOffset);
            data.WriteU32((uint)side, SerumProgramLayouts.CancelOrderV2.SideOffset);
            data.WriteBigInt(orderId, SerumProgramLayouts.CancelOrderV2.OrderIdOffset);
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
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.Prune"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodePrune(DecodedInstruction decodedInstruction, IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Bids", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Asks", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Prune Authority", keys[keyIndices[3]]);
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[4]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[5]]);
            decodedInstruction.Values.Add("Event Queue", keys[keyIndices[6]]);
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.InitOpenOrders"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeInitOpenOrders(DecodedInstruction decodedInstruction, IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Market", keys[keyIndices[2]]);

            if (keyIndices.Length == 5)
                decodedInstruction.Values.Add("Market Authority", keys[keyIndices[4]]);
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.CloseOpenOrders"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeCloseOpenOrders(DecodedInstruction decodedInstruction, IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Destination", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Market", keys[keyIndices[3]]);
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.SettleFunds"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeSettleFunds(DecodedInstruction decodedInstruction, IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Base Vault", keys[keyIndices[3]]);
            decodedInstruction.Values.Add("Quote Vault", keys[keyIndices[4]]);
            decodedInstruction.Values.Add("Base Account", keys[keyIndices[5]]);
            decodedInstruction.Values.Add("Quote Account", keys[keyIndices[6]]);
            decodedInstruction.Values.Add("Vault Signer", keys[keyIndices[7]]);
            decodedInstruction.Values.Add("Token Program Id", keys[keyIndices[8]]);
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.ConsumeEvents"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeConsumeEvents(DecodedInstruction decodedInstruction, IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[^4]]);
            decodedInstruction.Values.Add("Event Queue", keys[keyIndices[^3]]);
            decodedInstruction.Values.Add("Base Account", keys[keyIndices[^2]]);
            decodedInstruction.Values.Add("Quote Account", keys[keyIndices[^1]]);
            for (int i = 0; i < keyIndices.Length - 4; i++)
            {
                decodedInstruction.Values.Add($"Open Orders Account {i + 1}", keys[keyIndices[i]]);
            }
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.CancelOrderByClientIdV2"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="data">The instruction data to decode.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeCancelOrderByClientIdV2(DecodedInstruction decodedInstruction, ReadOnlySpan<byte> data,
            IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Bids", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Asks", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[3]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[4]]);
            decodedInstruction.Values.Add("Event Queue", keys[keyIndices[5]]);

            decodedInstruction.Values.Add("Client Id", data.GetU64(SerumProgramLayouts.CancelOrderByClientIdV2ClientIdOffset));
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.CancelOrderV2"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="data">The instruction data to decode.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeCancelOrderV2(DecodedInstruction decodedInstruction, ReadOnlySpan<byte> data,
            IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Bids", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Asks", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[3]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[4]]);
            decodedInstruction.Values.Add("Event Queue", keys[keyIndices[5]]);

            Side side = (Side)Enum.Parse(typeof(Side),
                data.GetU8(SerumProgramLayouts.CancelOrderV2.SideOffset).ToString());
            decodedInstruction.Values.Add("Side", side);
            decodedInstruction.Values.Add("Order Id", data.GetBigInt(SerumProgramLayouts.CancelOrderV2.OrderIdOffset, 16));
        }

        /// <summary>
        /// Decodes the instruction instruction data  for the <see cref="SerumProgramInstructions.Values.NewOrderV3"/> method
        /// </summary>
        /// <param name="decodedInstruction">The decoded instruction to add data to.</param>
        /// <param name="data">The instruction data to decode.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        internal static void DecodeNewOrderV3(DecodedInstruction decodedInstruction, ReadOnlySpan<byte> data,
            IList<PublicKey> keys, byte[] keyIndices)
        {
            decodedInstruction.Values.Add("Market", keys[keyIndices[0]]);
            decodedInstruction.Values.Add("Open Orders Account", keys[keyIndices[1]]);
            decodedInstruction.Values.Add("Request Queue", keys[keyIndices[2]]);
            decodedInstruction.Values.Add("Event Queue", keys[keyIndices[3]]);
            decodedInstruction.Values.Add("Bids", keys[keyIndices[4]]);
            decodedInstruction.Values.Add("Asks", keys[keyIndices[5]]);
            decodedInstruction.Values.Add("Payer", keys[keyIndices[6]]);
            decodedInstruction.Values.Add("Owner", keys[keyIndices[7]]);
            decodedInstruction.Values.Add("Base Vault", keys[keyIndices[8]]);
            decodedInstruction.Values.Add("Quote Vault", keys[keyIndices[9]]);
            decodedInstruction.Values.Add("Token Program Id", keys[keyIndices[10]]);

            Side side = (Side)Enum.Parse(typeof(Side),
                data.GetU8(SerumProgramLayouts.NewOrderV3.SideOffset).ToString());
            decodedInstruction.Values.Add("Side", side);
            decodedInstruction.Values.Add("Limit Price", data.GetU64(SerumProgramLayouts.NewOrderV3.PriceOffset));
            decodedInstruction.Values.Add("Max Base Coin Quantity", data.GetU64(SerumProgramLayouts.NewOrderV3.MaxBaseQuantityOffset));
            decodedInstruction.Values.Add("Max Quote Coin Quantity", data.GetU64(SerumProgramLayouts.NewOrderV3.MaxQuoteQuantity));

            SelfTradeBehavior selfTradeBehavior = (SelfTradeBehavior)Enum.Parse(typeof(SelfTradeBehavior),
                data.GetU8(SerumProgramLayouts.NewOrderV3.SelfTradeBehaviorOffset).ToString());
            decodedInstruction.Values.Add("Self Trade Behavior", selfTradeBehavior);

            OrderType orderType = (OrderType)Enum.Parse(typeof(OrderType),
                data.GetU8(SerumProgramLayouts.NewOrderV3.OrderTypeOffset).ToString());
            decodedInstruction.Values.Add("Order Type", orderType);
        }

        /// <summary>
        /// Derive the vault signer address for the given market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="vaultSignerNonce">The vault's signer nonce.</param>
        /// <returns>The vault signer address.</returns>
        public static byte[] DeriveVaultSignerAddress(PublicKey market, ulong vaultSignerNonce)
        {
            byte[] buffer = new byte[8];
            buffer.WriteU64(vaultSignerNonce, 0);

            List<byte[]> seeds = new() { market.KeyBytes, BitConverter.GetBytes(vaultSignerNonce) };

            bool success = AddressExtensions.TryCreateProgramAddress(seeds,
                SerumProgram.ProgramIdKey.KeyBytes, out byte[] vaultSignerAddress);

            return !success ? null : vaultSignerAddress;
        }

        /// <summary>
        /// Derive the vault signer address for the given market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns>The vault signer address.</returns>
        /// <exception cref="Exception">Throws exception when unable to derive the vault signer address.</exception>
        public static byte[] DeriveVaultSignerAddress(Market market) => DeriveVaultSignerAddress(market.OwnAddress, market.VaultSignerNonce);
    }
}