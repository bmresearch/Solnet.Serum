using Solnet.Serum.Models;
using Solnet.Serum.Models.Flags;

namespace Solnet.Serum
{
    /// <summary>
    /// Represents the possible errors thrown by the <see cref="SerumProgram"/>.
    /// </summary>
    public enum SerumProgramError
    {
        /// <summary>
        /// An error thrown when the <see cref="Market"/> has invalid <see cref="AccountFlags"/>.
        /// </summary>
        InvalidMarketFlags = 0,
        
        /// <summary>
        /// An error thrown when the ask <see cref="OrderBookSide"/> has invalid <see cref="AccountFlags"/>.
        /// </summary>
        InvalidAskFlags = 1,
        
        /// <summary>
        /// An error thrown when the bid <see cref="OrderBookSide"/> has invalid <see cref="AccountFlags"/>.
        /// </summary>
        InvalidBidFlags = 2,
        
        /// <summary>
        /// An error thrown when a queue has an invalid length.
        /// </summary>
        InvalidQueueLength = 3,
        
        /// <summary>
        /// An error thrown when an owner account hasn't been provided.
        /// </summary>
        OwnerAccountNotProvided = 4,

        /// <summary>
        /// An error thrown when there was a failure to consume the <see cref="EventQueue"/>.
        /// </summary>
        ConsumeEventsQueueFailure = 5,
        
        /// <summary>
        /// An error thrown when a base vault is invalid.
        /// </summary>
        WrongCoinVault = 6,
        
        /// <summary>
        /// An error thrown when a quote vault is invalid.
        /// </summary>
        WrongPcVault = 7,
        
        /// <summary>
        /// An error thrown when a base mint is invalid.
        /// </summary>
        WrongCoinMint = 8,
        
        /// <summary>
        /// An error thrown when a quote mint is invalid.
        /// </summary>
        WrongPcMint = 9,

        /// <summary>
        /// An error thrown when the base vault program id is invalid.
        /// </summary>
        CoinVaultProgramId = 10,
        
        /// <summary>
        /// An error thrown when the quote vault program id is invalid.
        /// </summary>
        PcVaultProgramId = 11,
        
        /// <summary>
        /// An error thrown when the base mint program id is invalid.
        /// </summary>
        CoinMintProgramId = 12,
        
        /// <summary>
        /// An error thrown when the quote mint program id is invalid.
        /// </summary>
        PcMintProgramId = 13,

        /// <summary>
        /// An error thrown when the base mint size is invalid.
        /// </summary>
        WrongCoinMintSize = 14,
        
        /// <summary>
        /// An error thrown when the quote mint size is invalid.
        /// </summary>
        WrongPcMintSize = 15,
        
        /// <summary>
        /// An error thrown when the base vault size is invalid.
        /// </summary>
        WrongCoinVaultSize = 16,
        
        /// <summary>
        /// An error thrown when the quote vault size is invalid.
        /// </summary>
        WrongPcVaultSize = 17,

        /// <summary>
        /// An error thrown when a vault is uninitialized.
        /// </summary>
        UninitializedVault = 18,
        
        /// <summary>
        /// An error thrown when a mint is uninitialized.
        /// </summary>
        UninitializedMint = 19,

        /// <summary>
        /// An error thrown when the base mint is uninitialized.
        /// </summary>
        CoinMintUninitialized = 20,
        
        /// <summary>
        /// An error thrown when the quote mint is uninitialized.
        /// </summary>
        PcMintUninitialized = 21,
        
        /// <summary>
        /// An error thrown when the a mint is invalid.
        /// </summary>
        WrongMint = 22,
        
        /// <summary>
        /// An error thrown when the a mint is invalid.
        /// </summary>
        WrongVaultOwner = 23,
        
        /// <summary>
        /// An error thrown when a vault has a delegate.
        /// </summary>
        VaultHasDelegate = 24,

        /// <summary>
        /// An error thrown when a structure has already been initialized.
        /// </summary>
        AlreadyInitialized = 25,
        
        /// <summary>
        /// An error thrown when a structure has already been initialized.
        /// </summary>
        WrongAccountDataAlignment = 26,
        
        /// <summary>
        /// An error thrown when an account's data padding has an invalid length.
        /// </summary>
        WrongAccountDataPaddingLength = 27,
        
        /// <summary>
        /// An error thrown when an account's data has an invalid head padding.
        /// </summary>
        WrongAccountHeadPadding = 28,
        
        /// <summary>
        /// An error thrown when an account's data has an invalid tail padding.
        /// </summary>
        WrongAccountTailPadding = 29,

        /// <summary>
        /// An error thrown when the request queue is empty.
        /// </summary>
        RequestQueueEmpty = 30,
        
        /// <summary>
        /// An error thrown when the <see cref="EventQueue"/> length is too small.
        /// </summary>
        EventQueueTooSmall = 31,
        
        /// <summary>
        /// An error thrown when the <see cref="Slab"/> length is too small.
        /// </summary>
        SlabTooSmall = 32,
        
        /// <summary>
        /// An error thrown when the vault signer nonce is invalid.
        /// </summary>
        BadVaultSignerNonce = 33,
        
        /// <summary>
        /// An error thrown when the funds are insufficient.
        /// </summary>
        InsufficientFunds = 34,

        /// <summary>
        /// An error thrown when the token account program id is invalid.
        /// </summary>
        SplAccountProgramId = 35,
        
        /// <summary>
        /// An error thrown when the token account data length is invalid.
        /// </summary>
        SplAccountLen = 36,
        
        /// <summary>
        /// An error thrown when the fee discount account owner is invalid.
        /// </summary>
        WrongFeeDiscountAccountOwner = 37,
        
        /// <summary>
        /// An error thrown when the fee discount mint is invalid.
        /// </summary>
        WrongFeeDiscountMint = 38,

        /// <summary>
        /// An error thrown when the base payer's program id is invalid.
        /// </summary>
        CoinPayerProgramId = 39,
        
        /// <summary>
        /// An error thrown when the quote payer's program id is invalid.
        /// </summary>
        PcPayerProgramId = 40,
        
        /// <summary>
        /// An error thrown when the given client id wasn't found.
        /// </summary>
        ClientIdNotFound = 41,

        /// <summary>
        /// An error thrown when the open orders account has too many open orders.
        /// </summary>
        TooManyOpenOrders = 42,

        /// <summary>
        /// Scammed again.
        /// </summary>
        FakeErrorSoWeDontChangeNumbers = 43,

        /// <summary>
        /// Unused.
        /// </summary>
        BorrowError = 44,

        /// <summary>
        /// An error thrown when the open orders account isn't owned by the given owner account.
        /// </summary>
        WrongOrdersAccount = 45,
        
        /// <summary>
        /// An error thrown when the bids account for the given market is invalid.
        /// </summary>
        WrongBidsAccount = 46,
        
        /// <summary>
        /// An error thrown when the asks account for the given market is invalid.
        /// </summary>
        WrongAsksAccount = 47,
        
        /// <summary>
        /// An error thrown when the request queue account for the given market is invalid.
        /// </summary>
        WrongRequestQueueAccount = 48,
        
        /// <summary>
        /// An error thrown when the event queue account for the given market is invalid.
        /// </summary>
        WrongEventQueueAccount = 49,

        /// <summary>
        /// An error thrown when the request queue is full.
        /// </summary>
        RequestQueueFull = 50,
        
        /// <summary>
        /// An error thrown when the event queue is full.
        /// </summary>
        EventQueueFull = 51,
        
        /// <summary>
        /// An error thrown when the market is disabled.
        /// </summary>
        MarketIsDisabled = 52,
        
        /// <summary>
        /// An error thrown when a signer is wrong.
        /// </summary>
        WrongSigner = 53,
        
        /// <summary>
        /// An error thrown when a signer is wrong.
        /// </summary>
        TransferFailed = 54,
        
        /// <summary>
        /// An error thrown when the rent SysVar is invalid.
        /// </summary>
        ClientOrderIdIsZero = 55,

        /// <summary>
        /// An error thrown when the rent SysVar is invalid.
        /// </summary>
        WrongRentSysVarAccount = 56,
        
        /// <summary>
        /// An error thrown when the rent SysVar was not provided.
        /// </summary>
        RentNotProvided = 57,
        
        /// <summary>
        /// An error thrown when the open orders account is not rent exempt.
        /// </summary>
        OrdersNotRentExempt = 58,
        
        /// <summary>
        /// An error thrown when the order wasn't found.
        /// </summary>
        OrderNotFound = 59,
        
        /// <summary>
        /// An error thrown when the matching order is not the user's.
        /// </summary>
        OrderNotYours = 60,

        /// <summary>
        /// An error thrown when the order would self trade and the self trade behavior specifies otherwise.
        /// </summary>
        WouldSelfTrade = 61,
        
        /// <summary>
        /// An error thrown when the open orders authority is invalid.
        /// </summary>
        InvalidOpenOrdersAuthority = 62,

        /// <summary>
        /// An unknown error.
        /// </summary>
        Unknown = 1000,

        /// <summary>
        /// This error contains the line number in the lower 16 bits and the source file id in the upper 8 bits.
        /// </summary>
        AssertionError = 1001,
    }
}