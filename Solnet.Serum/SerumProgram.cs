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
    /// For more information see:
    /// https://github.com/project-serum/serum-dex/
    /// https://github.com/project-serum/serum-dex/blob/master/dex/src/instruction.rs
    /// </remarks>
    /// </summary>
    public class SerumProgram
    {
        /// <summary>
        /// The Serum V3 Program key.
        /// </summary>
        public static readonly PublicKey ProgramIdKey = new("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin");

        /// <summary>
        /// The public key of the Serum token mint.
        /// </summary>
        public static readonly PublicKey SerumTokenMintKey = new("SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt");

        /// <summary>
        /// The public key of the MegaSerum token mint.
        /// </summary>
        public static readonly PublicKey MegaSerumTokenMintKey = new("MSRMcoVyrFxnSgo5uXwone5SKcGhT1KEJMFEkMEWf9L");

        /// <summary>
        /// The public key of the authority for disabling markets.
        /// </summary>
        public static readonly PublicKey DisableAuthorityKey = new("5ZVJgwWxMsqXxRMYHXqMwH2hd4myX5Ef4Au2iUsuNQ7V");

        /// <summary>
        /// The public key of the fee sweeper.
        /// </summary>
        public static readonly PublicKey FeeSweeperKey = new("DeqYsmBd9BnrbgUwQjVH4sQWK71dEgE6eoZFw3Rp4ftE");
        
        /// <summary>
        /// The program's name.
        /// </summary>
        private const string ProgramName = "Serum Program";

        /// <summary>
        /// Initializes an instruction to create a new Order on Serum v3.
        /// </summary>
        /// <param name="market">The <see cref="Market"/> that we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="orderPayer">The <see cref="PublicKey"/> of the token account funding the order.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the account that owns the payer and the open orders account.</param>
        /// <param name="order">The <see cref="Order"/> to place.</param>
        /// <param name="serumFeeDiscount">The public key of the SRM wallet for fee discount.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction NewOrderV3(Market market, PublicKey openOrdersAccount, 
            PublicKey orderPayer, PublicKey openOrdersAccountOwner, Order order, PublicKey serumFeeDiscount = null)
            => NewOrderV3(market.OwnAddress, openOrdersAccount, market.RequestQueue, market.EventQueue, market.Bids,
                market.Asks, orderPayer, openOrdersAccountOwner, market.BaseVault, market.QuoteVault,
                TokenProgram.ProgramIdKey, SysVars.RentKey, ProgramIdKey, order.Side, order.RawPrice,
                order.RawQuantity, order.Type, order.ClientOrderId, order.SelfTradeBehavior,
                ushort.MaxValue, order.MaxQuoteQuantity, serumFeeDiscount);

        /// <summary>
        /// Initializes an instruction to create a New Order on Serum v3.
        /// </summary>
        /// <param name="market">The <see cref="PublicKey"/> of the <see cref="Market"/></param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="requestQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s request queue.</param>
        /// <param name="eventQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s event queue.</param>
        /// <param name="marketBids">The <see cref="PublicKey"/> of the <see cref="Market"/>'s bids account.</param>
        /// <param name="marketAsks">The <see cref="PublicKey"/> of the <see cref="Market"/>'s asks account.</param>
        /// <param name="orderPayer">The <see cref="PublicKey"/> of the token account funding the order.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the account that owns the payer and the open orders account.</param>
        /// <param name="coinVault">The <see cref="PublicKey"/> of the <see cref="Market"/>'s coin vault.</param>
        /// <param name="pcVault">The <see cref="PublicKey"/> of the <see cref="Market"/>'s price coin vault.</param>
        /// <param name="splTokenProgramId">The <see cref="PublicKey"/> of the SPL Token Program associated with the market.</param>
        /// <param name="rentSysVarId">The <see cref="PublicKey"/> of the Rent SysVar.</param>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="side">The <see cref="Side"/> of the order.</param>
        /// <param name="limitPrice">The limit price for the order.</param>
        /// <param name="maxCoinQty">The maximum amount of coins to receive.</param>
        /// <param name="orderType">The <see cref="OrderType"/>.</param>
        /// <param name="clientOrderId">The client's <see cref="Order"/> Id.</param>
        /// <param name="selfTradeBehaviorType">The <see cref="SelfTradeBehavior"/> associated with the order.</param>
        /// <param name="limit">The maximum number of iterations of the Serum order matching loop.</param>
        /// <param name="maxNativePcQtyIncludingFees">The maximum quantity of price coin, including fees, for the order.</param>
        /// <param name="serumFeeDiscount">The public key of the SRM wallet for fee discount.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction NewOrderV3(
            PublicKey market, PublicKey openOrdersAccount, PublicKey requestQueue, PublicKey eventQueue,
            PublicKey marketBids, PublicKey marketAsks, PublicKey orderPayer, PublicKey openOrdersAccountOwner,
            PublicKey coinVault, PublicKey pcVault, PublicKey splTokenProgramId, PublicKey rentSysVarId,
            PublicKey programId, Side side, ulong limitPrice, ulong maxCoinQty, OrderType orderType,
            ulong clientOrderId, SelfTradeBehavior selfTradeBehaviorType, ushort limit,
            ulong maxNativePcQtyIncludingFees, PublicKey serumFeeDiscount = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(market, false),
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.Writable(requestQueue, false),
                AccountMeta.Writable(eventQueue, false),
                AccountMeta.Writable(marketBids, false),
                AccountMeta.Writable(marketAsks, false),
                AccountMeta.Writable(orderPayer, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, true),
                AccountMeta.Writable(coinVault, false),
                AccountMeta.Writable(pcVault, false),
                AccountMeta.ReadOnly(splTokenProgramId, false),
                AccountMeta.ReadOnly(rentSysVarId, false)
            };
            if (serumFeeDiscount != null)
            {
                keys.Add(AccountMeta.Writable(serumFeeDiscount, false));
            }

            return new TransactionInstruction
            {
                ProgramId = programId,
                Keys = keys,
                Data = SerumProgramData.EncodeNewOrderV3Data(side, limitPrice, maxCoinQty,
                    orderType, clientOrderId, selfTradeBehaviorType, maxNativePcQtyIncludingFees, limit)
            };
        }

        /// <summary>
        /// Initializes an instruction to settle funds on Serum.
        /// </summary>
        /// <param name="market">The market we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="owner">The <see cref="PublicKey"/> of the account that owns the payer and the open orders account.</param>
        /// <param name="baseWallet">The <see cref="PublicKey"/> of the coin wallet or token account.</param>
        /// <param name="quoteWallet">The <see cref="PublicKey"/> of the quote wallet or token account.</param>
        /// <param name="referrerPcWallet">The <see cref="PublicKey"/> of the quote wallet or token account.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction SettleFunds(Market market, PublicKey openOrdersAccount, PublicKey owner,
            PublicKey baseWallet, PublicKey quoteWallet, PublicKey referrerPcWallet = null)
        {
            byte[] vaultSignerAddress = SerumProgramData.DeriveVaultSignerAddress(market);

            if (vaultSignerAddress == null)
                return null;

            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(market.OwnAddress, false),
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.ReadOnly(owner, true),
                AccountMeta.Writable(market.BaseVault, false),
                AccountMeta.Writable(market.QuoteVault, false),
                AccountMeta.Writable(baseWallet, false),
                AccountMeta.Writable(quoteWallet, false),
                AccountMeta.ReadOnly(new PublicKey(vaultSignerAddress), false),
                AccountMeta.ReadOnly(TokenProgram.ProgramIdKey, false)
            };
            
            if (referrerPcWallet != null)
                keys.Add(AccountMeta.Writable(referrerPcWallet, false));

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = SerumProgramData.EncodeSettleFundsData()
            };
        }

        /// <summary>
        /// Initializes an instruction to consume events of a list of open orders accounts in a given market on Serum.
        /// </summary>
        /// <param name="openOrdersAccounts">A list of <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="market">The <see cref="Market"/> we are trading on.</param>
        /// <param name="coinAccount">The <see cref="PublicKey"/> of the coin account.</param>
        /// <param name="pcAccount">The <see cref="PublicKey"/> of the price coin account.</param>
        /// <param name="limit">The maximum number of events to consume.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction ConsumeEvents(List<PublicKey> openOrdersAccounts,
            Market market, PublicKey coinAccount, PublicKey pcAccount, ushort limit)
            => ConsumeEvents(openOrdersAccounts, market.OwnAddress, market.EventQueue, coinAccount, pcAccount, limit);

        /// <summary>
        /// Initializes an instruction to consume events of a list of open orders accounts in a given market on Serum.
        /// </summary>
        /// <param name="openOrdersAccounts">A list of <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the <see cref="Market"/> we are consuming events on.</param>
        /// <param name="eventQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s event queue.</param>
        /// <param name="coinAccount">The <see cref="PublicKey"/> of the coin account.</param>
        /// <param name="pcAccount">The <see cref="PublicKey"/> of the price coin account.</param>
        /// <param name="limit">The maximum number of events to consume.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction ConsumeEvents(List<PublicKey> openOrdersAccounts,
            PublicKey market, PublicKey eventQueue, PublicKey coinAccount, PublicKey pcAccount, ushort limit)
        {
            List<AccountMeta> keys = new();
            openOrdersAccounts.ForEach(pubKey => keys.Add(AccountMeta.Writable(pubKey, false)));
            keys.Add(AccountMeta.Writable(market, false));
            keys.Add(AccountMeta.Writable(eventQueue, false));
            keys.Add(AccountMeta.Writable(coinAccount, false));
            keys.Add(AccountMeta.Writable(pcAccount, false));

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey, Keys = keys, Data = SerumProgramData.EncodeConsumeEventsData(limit)
            };
        }

        /// <summary>
        /// Initializes an instruction to cancel an order by <c>orderId</c> in a given market on Serum.
        /// </summary>
        /// <param name="market">The <see cref="Market"/> we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="Account"/> that owns the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="side">The side of the order.</param>
        /// <param name="orderId">The order's id, fetched from a <see cref="OpenOrdersAccount"/>.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderV2(
            Market market, PublicKey openOrdersAccount, PublicKey openOrdersAccountOwner, Side side, BigInteger orderId) 
            => CancelOrderV2(ProgramIdKey, market.OwnAddress, market.Bids, market.Asks, openOrdersAccount,
                openOrdersAccountOwner, market.EventQueue, side, orderId);
        
        /// <summary>
        /// Initializes an instruction to cancel orders by <c>orderId</c> in a given market on Serum.
        /// </summary>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <param name="marketBids">The <see cref="PublicKey"/> of the <see cref="Market"/>'s bids account.</param>
        /// <param name="marketAsks">The <see cref="PublicKey"/> of the <see cref="Market"/>'s asks account.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the account that owns the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="eventQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s event queue.</param>
        /// <param name="side">The side of the order.</param>
        /// <param name="orderId">The client's <c>orderId</c></param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderV2(PublicKey programId, PublicKey market,
            PublicKey marketBids, PublicKey marketAsks, PublicKey openOrdersAccount, PublicKey openOrdersAccountOwner,
            PublicKey eventQueue, Side side, BigInteger orderId)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(market, false),
                AccountMeta.Writable(marketBids, false),
                AccountMeta.Writable(marketAsks, false),
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, true),
                AccountMeta.Writable(eventQueue, false)
            };

            return new TransactionInstruction
            {
                ProgramId = programId, Keys = keys, Data = SerumProgramData.EncodeCancelOrderV2Data(side, orderId)
            };
        }

        /// <summary>
        /// Initializes an instruction to cancel orders by <c>clientId</c> in a given market on Serum.
        /// </summary>
        /// <param name="market">The <see cref="Market"/> we are trading on.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the that owns the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="clientOrderId">The client's <c>orderId</c></param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderByClientIdV2(Market market, PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner, ulong clientOrderId)
            => CancelOrderByClientIdV2(ProgramIdKey, market.OwnAddress, market.Bids, market.Asks, openOrdersAccount,
                openOrdersAccountOwner, market.EventQueue, clientOrderId);
        
        /// <summary>
        /// Initializes an instruction to cancel orders by <c>clientId</c> in a given market on Serum.
        /// </summary>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <param name="marketBids">The <see cref="PublicKey"/> of the <see cref="Market"/>'s bids account.</param>
        /// <param name="marketAsks">The <see cref="PublicKey"/> of the <see cref="Market"/>'s asks account.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> associated with this <see cref="Account"/> and <see cref="Market"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the account that owns the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="eventQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s event queue.</param>
        /// <param name="clientOrderId">The client's <c>orderId</c></param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CancelOrderByClientIdV2(PublicKey programId, PublicKey market,
            PublicKey marketBids, PublicKey marketAsks, PublicKey openOrdersAccount, PublicKey openOrdersAccountOwner,
            PublicKey eventQueue, ulong clientOrderId)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(market, false),
                AccountMeta.Writable(marketBids, false),
                AccountMeta.Writable(marketAsks, false),
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, true),
                AccountMeta.Writable(eventQueue, false)
            };

            return new TransactionInstruction
            {
                ProgramId = programId,
                Keys = keys,
                Data = SerumProgramData.EncodeCancelOrderByClientIdV2Data(clientOrderId)
            };
        }
        
        /// <summary>
        /// Initializes an instruction to close an open orders account for a given Serum market.
        /// </summary>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <param name="destination">The <see cref="PublicKey"/> of the destination account.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CloseOpenOrders(PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner, PublicKey destination, PublicKey market)
            => CloseOpenOrders(ProgramIdKey, openOrdersAccount, 
                openOrdersAccountOwner, destination, market);

        /// <summary>
        /// Initializes an instruction to close an open orders account for a given Serum market.
        /// </summary>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <param name="destination">The <see cref="PublicKey"/> of the destination account.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CloseOpenOrders(PublicKey programId, PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner, PublicKey destination, PublicKey market)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, true),
                AccountMeta.Writable(destination, false),
                AccountMeta.ReadOnly(market, false),
            };

            return new TransactionInstruction
            {
                ProgramId = programId,
                Data = SerumProgramData.EncodeCloseOpenOrdersData(),
                Keys = keys
            };
        }

        /// <summary>
        /// Initializes an instruction to initialize an open orders account for a given Serum market.
        /// </summary>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <param name="marketAuthority">The <see cref="PublicKey"/> of the market authority.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction InitOpenOrders(PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner, PublicKey market, PublicKey marketAuthority = null)
            => InitOpenOrders(ProgramIdKey, openOrdersAccount, openOrdersAccountOwner, market, marketAuthority);

        /// <summary>
        /// Initializes an instruction to initialize an open orders account for a given Serum market.
        /// </summary>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <param name="marketAuthority">The <see cref="PublicKey"/> of the market authority.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction InitOpenOrders(PublicKey programId, PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner, PublicKey market, PublicKey marketAuthority = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(openOrdersAccount, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, true),
                AccountMeta.ReadOnly(market, false),
                AccountMeta.ReadOnly(SysVars.RentKey, false)
            };
            
            if (marketAuthority != null)
                keys.Add(AccountMeta.ReadOnly(marketAuthority, true));

            return new TransactionInstruction
            {
                ProgramId = programId,
                Data = SerumProgramData.EncodeInitOpenOrdersData(),
                Keys = keys
            };
        }

        /// <summary>
        /// Initializes an instruction to remove all orders for a given open orders account from the order book.
        /// </summary>
        /// <param name="market">The <see cref="Market"/> we are trading on.</param>
        /// <param name="pruneAuthority">The <see cref="PublicKey"/> of the prune authority.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Prune(Market market, PublicKey pruneAuthority, PublicKey openOrdersAccount,
            PublicKey openOrdersAccountOwner) => Prune(ProgramIdKey, market.OwnAddress, market.Bids,
            market.Asks, pruneAuthority, openOrdersAccount, openOrdersAccountOwner, market.EventQueue, ushort.MaxValue);

        /// <summary>
        /// Initializes an instruction to remove all orders for a given open orders account from the order book.
        /// </summary>
        /// <param name="programId">The <see cref="PublicKey"/> of the Serum Program associated with this market.</param>
        /// <param name="market">The <see cref="PublicKey"/> of the market.</param>
        /// <param name="marketBids">The <see cref="PublicKey"/> of the <see cref="Market"/>'s bids account.</param>
        /// <param name="marketAsks">The <see cref="PublicKey"/> of the <see cref="Market"/>'s asks account.</param>
        /// <param name="pruneAuthority">The <see cref="PublicKey"/> of the <see cref="Market"/>'s pruning authority.</param>
        /// <param name="openOrdersAccount">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/>.</param>
        /// <param name="openOrdersAccountOwner">The <see cref="PublicKey"/> of the <see cref="OpenOrdersAccount"/> owner.</param>
        /// <param name="eventQueue">The <see cref="PublicKey"/> of the <see cref="Market"/>'s event queue.</param>
        /// <param name="limit">The maximum number of iterations of the Serum order matching loop.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Prune(PublicKey programId, PublicKey market, PublicKey marketBids,
            PublicKey marketAsks, PublicKey pruneAuthority, PublicKey openOrdersAccount, PublicKey openOrdersAccountOwner,
            PublicKey eventQueue, ushort limit)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(market, false),
                AccountMeta.Writable(marketBids, false),
                AccountMeta.Writable(marketAsks, false),
                AccountMeta.ReadOnly(pruneAuthority, true),
                AccountMeta.ReadOnly(openOrdersAccount, false),
                AccountMeta.ReadOnly(openOrdersAccountOwner, false),
                AccountMeta.Writable(eventQueue, false),
            };

            return new TransactionInstruction
            {
                ProgramId = programId, 
                Data = SerumProgramData.EncodePruneData(limit),
                Keys = keys
            };
        }


        /// <summary>
        /// Decodes an instruction created by the System Program.
        /// </summary>
        /// <param name="data">The instruction data to decode.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        /// <returns>A decoded instruction.</returns>
        public static DecodedInstruction Decode(ReadOnlySpan<byte> data, IList<PublicKey> keys, byte[] keyIndices)
        {
            uint instruction = data.GetU32(SerumProgramLayouts.MethodOffset);
            SerumProgramInstructions.Values instructionValue =
                (SerumProgramInstructions.Values) Enum.Parse(typeof(SerumProgramInstructions.Values), instruction.ToString());
            
            DecodedInstruction decodedInstruction = new()
            {
                PublicKey = ProgramIdKey,
                InstructionName = SerumProgramInstructions.Names[instructionValue],
                ProgramName = ProgramName,
                Values = new Dictionary<string, object>(),
                InnerInstructions = new List<DecodedInstruction>()
            };

            switch (instructionValue)
            {
                    case SerumProgramInstructions.Values.InitOpenOrders:
                        SerumProgramData.DecodeInitOpenOrders(decodedInstruction, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.ConsumeEvents:
                        SerumProgramData.DecodeConsumeEvents(decodedInstruction, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.SettleFunds:
                        SerumProgramData.DecodeSettleFunds(decodedInstruction, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.NewOrderV3:
                        SerumProgramData.DecodeNewOrderV3(decodedInstruction, data, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.CancelOrderV2:
                        SerumProgramData.DecodeCancelOrderV2(decodedInstruction, data, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.CancelOrderByClientIdV2:
                        SerumProgramData.DecodeCancelOrderByClientIdV2(decodedInstruction, data, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.CloseOpenOrders:
                        SerumProgramData.DecodeCloseOpenOrders(decodedInstruction, keys, keyIndices);
                        break;
                    case SerumProgramInstructions.Values.Prune:
                        SerumProgramData.DecodePrune(decodedInstruction, keys, keyIndices);
                        break;
            }

            return decodedInstruction;
        }
    }
}