using Solnet.Programs;
using Solnet.Programs.Utilities;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements the Serum Program methods.
    /// <remarks>
    /// For more information see: https://github.com/project-serum/awesome-serum
    /// </remarks>
    /// </summary>
    public class SerumProgram
    {
        /// <summary>
        /// The Serum Program key.
        /// </summary>
        public static readonly PublicKey ProgramIdKey = new PublicKey("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin");

        #region Layouts

        /// <summary>
        /// The offset at which to write the method value.
        /// </summary>
        private const int MethodOffset = 1;

        /// <summary>
        /// The offset at which to write the limit value for the <see cref="SerumProgramInstructions.ConsumeEvents"/> method.
        /// </summary>
        private const int ConsumeEventsLimitOffset = 5;

        /// <summary>
        /// The offset at which to write the client id value for the <see cref="SerumProgramInstructions.CancelOrderByClientIdV2"/> method.
        /// </summary>
        private const int CancelOrderByClientIdV2ClientIdOffset = 5;

        /// <summary>
        /// The offset at which to write the order side value for the <see cref="SerumProgramInstructions.CancelOrderV2"/> method.
        /// </summary>
        private const int CancelOrderV2SideOffset = 5;

        /// <summary>
        /// The offset at which to write the order id value for the <see cref="SerumProgramInstructions.CancelOrderV2"/> method.
        /// </summary>
        private const int CancelOrderV2OrderIdOffset = 9;

        /// <summary>
        /// Represents the layout of the <see cref="SerumProgramInstructions.NewOrderV3"/> method encoded data structure.
        /// </summary>
        private static class NewOrderV3DataLayout
        {
            /// <summary>
            /// The offset at which to write the order side value.
            /// </summary>
            internal const int SideOffset = 5;

            /// <summary>
            /// The offset at which to write the limit price value.
            /// </summary>
            internal const int PriceOffset = 9;

            /// <summary>
            /// The offset at which to write the max base quantity value.
            /// </summary>
            internal const int MaxBaseQuantityOffset = 17;

            /// <summary>
            /// The offset at which to write the max quote quantity value.
            /// </summary>
            internal const int MaxQuoteQuantity = 25;

            /// <summary>
            /// The offset at which to write the self trade behavior value.
            /// </summary>
            internal const int SelfTradeBehaviorOffset = 33;

            /// <summary>
            /// The offset at which to write the order type value.
            /// </summary>
            internal const int OrderTypeOffset = 37;

            /// <summary>
            /// The offset at which to write the client id value.
            /// </summary>
            internal const int ClientIdOffset = 41;

            /// <summary>
            /// The offset at which to write the limit value.
            /// </summary>
            internal const int LimitOffset = 49;

            /// <summary>
            /// The limit value (this is static).
            /// </summary>
            internal const ushort Limit = 65535;
        }

        #endregion

        /// <summary>
        /// Initializes an instruction to create a new Order on Serum v3.
        /// </summary>
        /// <param name="account">The <see cref="Account"/> that owns the payer and the open orders account.</param>
        /// <param name="payer">The token account funding the order.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="order">The order to place.</param>
        /// <param name="serumFeeDiscount">The public key of the SRM wallet for fee discount.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction NewOrderV3(Account account, PublicKey payer, PublicKey openOrdersAccount,
            Market market, Order order, PublicKey serumFeeDiscount = null)
        {
            List<AccountMeta> keys = new()
            {
                new AccountMeta(market.OwnAddress, true),
                new AccountMeta(openOrdersAccount, true),
                new AccountMeta(market.RequestQueue, true),
                new AccountMeta(market.EventQueue, true),
                new AccountMeta(market.Bids, true),
                new AccountMeta(market.Asks, true),
                new AccountMeta(payer, true),
                new AccountMeta(account, false),
                new AccountMeta(market.BaseVault, true),
                new AccountMeta(market.QuoteVault, true),
                new AccountMeta(TokenProgram.ProgramIdKey, false),
                new AccountMeta(SystemProgram.SysVarRentKey, false)
            };
            if (serumFeeDiscount != null)
            {
                keys.Add(new AccountMeta(serumFeeDiscount, false));
            }

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = EncodeNewOrderV3InstructionData(order)
            };
        }

        /// <summary>
        /// Initializes an instruction to settle funds on Serum.
        /// </summary>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="owner">The <see cref="Account"/> that owns the payer and the open orders account.</param>
        /// <param name="baseWallet">The public key of the base wallet or token account.</param>
        /// <param name="quoteWallet">The public key of the quote wallet or token account.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction SettleFunds(Market market, PublicKey openOrdersAccount, Account owner,
            PublicKey baseWallet, PublicKey quoteWallet)
        {
            byte[] vaultSignerAddress = DeriveVaultSignerAddress(market);

            if (vaultSignerAddress == null)
                return null;

            List<AccountMeta> keys = new()
            {
                new AccountMeta(market.OwnAddress, true),
                new AccountMeta(openOrdersAccount, true),
                new AccountMeta(owner, false),
                new AccountMeta(market.BaseVault, true),
                new AccountMeta(market.QuoteVault, true),
                new AccountMeta(baseWallet, true),
                new AccountMeta(quoteWallet, true),
                new AccountMeta(new PublicKey(vaultSignerAddress), false),
                new AccountMeta(TokenProgram.ProgramIdKey, false)
            };

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = EncodeSettleFundsInstructionData()
            };
        }

        /// <summary>
        /// Initializes an instruction to consume events of a list of open orders accounts in a given market on Serum.
        /// </summary>
        /// <param name="signer">The account to sign the transaction.</param>
        /// <param name="openOrdersAccounts">A list of <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="baseWallet">The public key of the base wallet or token account.</param>
        /// <param name="quoteWallet">The public key of the quote wallet or token account.</param>
        /// <param name="limit">The maximum number of events to consume.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction ConsumeEvents(Account signer, List<PublicKey> openOrdersAccounts,
            Market market, PublicKey baseWallet, PublicKey quoteWallet, ushort limit)
        {
            List<AccountMeta> keys = new() {new AccountMeta(signer, false)};
            openOrdersAccounts.ForEach(pk => keys.Add(new AccountMeta(pk, true)));
            keys.Add(new AccountMeta(market.OwnAddress, true));
            keys.Add(new AccountMeta(market.EventQueue, true));
            keys.Add(new AccountMeta(baseWallet, true));
            keys.Add(new AccountMeta(quoteWallet, true));

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = EncodeConsumeEventsInstructionData(limit)
            };
        }

        /// <summary>
        /// Initializes an instruction to cancel an order by <c>clientOrderId</c> in a given market on Serum.
        /// </summary>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="owner">The <see cref="Account"/> that owns the payer and the open orders account.</param>
        /// <param name="side">The side of the order.</param>
        /// <param name="orderId">The order's id, fetched from a <see cref="OpenOrdersAccount"/>.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderV2(Market market, PublicKey openOrdersAccount, PublicKey owner,
            Side side, BigInteger orderId)
        {
            List<AccountMeta> keys = new()
            {
                new AccountMeta(market.OwnAddress, false),
                new AccountMeta(market.Bids, true),
                new AccountMeta(market.Asks, true),
                new AccountMeta(openOrdersAccount, true),
                new AccountMeta(owner, false),
                new AccountMeta(market.EventQueue, true)
            };

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey,
                Keys = keys,
                Data = EncodeCancelOrderV2InstructionData(side, orderId)
            };
        }

        /// <summary>
        /// Initializes an instruction to cancel all orders by <c>clientId</c> in a given market on Serum.
        /// </summary>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="owner">The <see cref="Account"/> that owns the payer and the open orders account.</param>
        /// <param name="clientOrderId">The client's <c>orderId</c></param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderByClientIdV2(Market market, PublicKey openOrdersAccount,
            PublicKey owner, ulong clientOrderId)
        {
            List<AccountMeta> keys = new()
            {
                new AccountMeta(market.OwnAddress, false),
                new AccountMeta(market.Bids, true),
                new AccountMeta(market.Asks, true),
                new AccountMeta(openOrdersAccount, true),
                new AccountMeta(owner, false),
                new AccountMeta(market.EventQueue, true)
            };

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = EncodeCancelOrderByClientIdV2InstructionData(clientOrderId)
            };
        }


        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.ConsumeEvents"/> method.
        /// </summary>
        /// <param name="limit">The maximum number of events to consume.</param>
        /// <returns>The encoded data.</returns>
        private static byte[] EncodeConsumeEventsInstructionData(ushort limit)
        {
            byte[] data = new byte[7];
            data.WriteU32((uint)SerumProgramInstructions.ConsumeEvents, MethodOffset);
            data.WriteU16(limit, ConsumeEventsLimitOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.SettleFunds"/> method.
        /// </summary>
        /// <returns>The encoded data.</returns>
        private static byte[] EncodeSettleFundsInstructionData()
        {
            byte[] data = new byte[5];
            data.WriteU32((uint)SerumProgramInstructions.SettleFunds, MethodOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.NewOrderV3"/> method.
        /// </summary>
        /// <param name="order">The order instance to encode the data.</param>
        /// <returns>The encoded data.</returns>
        private static byte[] EncodeNewOrderV3InstructionData(Order order)
        {
            byte[] data = new byte[51];
            data.WriteU32((uint)SerumProgramInstructions.NewOrderV3, MethodOffset);
            data.WriteU32((uint)order.Side, NewOrderV3DataLayout.SideOffset);
            data.WriteU64(order.RawPrice, NewOrderV3DataLayout.PriceOffset);
            data.WriteU64(order.RawQuantity, NewOrderV3DataLayout.MaxBaseQuantityOffset);
            data.WriteU64(order.MaxQuoteQuantity, NewOrderV3DataLayout.MaxQuoteQuantity);
            data.WriteU32((uint)order.SelfTradeBehavior, NewOrderV3DataLayout.SelfTradeBehaviorOffset);
            data.WriteU32((uint)order.Type, NewOrderV3DataLayout.OrderTypeOffset);
            data.WriteU64(order.ClientId, NewOrderV3DataLayout.ClientIdOffset);
            data.WriteU16(NewOrderV3DataLayout.Limit, NewOrderV3DataLayout.LimitOffset);
            return data;
        }


        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.CancelOrderV2"/> method.
        /// </summary>
        /// <param name="side">The order's side.</param>
        /// <param name="clientOrderId">The client's order id.</param>
        /// <returns>The encoded data.</returns>
        private static byte[] EncodeCancelOrderV2InstructionData(Side side, BigInteger clientOrderId)
        {
            byte[] data = new byte[25];
            data.WriteU32((uint)SerumProgramInstructions.CancelOrderV2, MethodOffset);
            data.WriteU32((uint)side, CancelOrderV2SideOffset);
            data.WriteBigInt(clientOrderId, CancelOrderV2OrderIdOffset);
            return data;
        }

        /// <summary>
        /// Encode the <see cref="TransactionInstruction"/> data for the <see cref="SerumProgramInstructions.CancelOrderByClientIdV2"/> method.
        /// </summary>
        /// <param name="clientOrderId">The client's <c>orderId</c>.</param>
        /// <returns>The encoded data.</returns>
        private static byte[] EncodeCancelOrderByClientIdV2InstructionData(ulong clientOrderId)
        {
            byte[] data = new byte[13];
            data.WriteU32((uint)SerumProgramInstructions.CancelOrderByClientIdV2, MethodOffset);
            data.WriteU64(clientOrderId, CancelOrderByClientIdV2ClientIdOffset);
            return data;
        }

        /// <summary>
        /// Derive the vault signer address for the given market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns>The vault signer address.</returns>
        /// <exception cref="Exception">Throws exception when unable to derive the vault signer address.</exception>
        private static byte[] DeriveVaultSignerAddress(Market market)
        {
            byte[] vaultSignerAddress;
            try
            {
                vaultSignerAddress = AddressExtensions.CreateProgramAddress(
                    new List<byte[]> {market.OwnAddress.KeyBytes, BitConverter.GetBytes(market.VaultSignerNonce)},
                    ProgramIdKey.KeyBytes);
            }
            catch (Exception e)
            {
                return null;
            }
            return vaultSignerAddress;
        }
    }
}