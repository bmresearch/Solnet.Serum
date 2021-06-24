// unset

using Solnet.Serum.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solnet.Serum
{
    /// <summary>
    /// Specifies functionality for the Serum Client.
    /// </summary>
    public interface ISerumClient
    {
        /// <summary>
        /// Gets the available markets in the Serum DEX. This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which may return a list with the market's info.</returns>
        Task<IList<MarketInfo>> GetMarketsAsync();
        
        /// <summary>
        /// Gets the available markets in the Serum DEX.
        /// </summary>
        /// <returns>A list with the market's info.</returns>
        IList<MarketInfo> GetMarkets();
        
        /// <summary>
        /// Gets the available tokens in the Serum DEX. This is an asynchronous operation.
        /// </summary>
        /// <returns>A task which may return a list with the token's info.</returns>
        Task<IList<TokenInfo>> GetTokensAsync();
        
        /// <summary>
        /// Gets the available tokens in the Serum DEX.
        /// </summary>
        /// <returns>A list with the token's info.</returns>
        IList<TokenInfo> GetTokens();
        
        /// <summary>
        /// Gets the account data associated with the given market address in the Serum DEX.
        /// This is an asynchronous operation.
        /// </summary>
        /// <param name="address">The public key of the market account.</param>
        /// <returns>A task which may return the market's account data.</returns>
        Task<Market> GetMarketAsync(string address);
        
        /// <summary>
        /// Gets the account data associated with the given market address in the Serum DEX.
        /// </summary>
        /// <param name="address">The public key of the market account.</param>
        /// <returns>The market's account data.</returns>
        Market GetMarket(string address);
    }
}