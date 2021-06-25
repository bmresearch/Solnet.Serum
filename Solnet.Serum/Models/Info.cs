using Solnet.Serum.Converters;
using Solnet.Wallet;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents information about a market in Serum.
    /// </summary>
    [DebuggerDisplay("Market = {Address.Key}")]
    public class MarketInfo
    {
        /// <summary>
        /// Whether the market has been deprecated or not.
        /// </summary>
        public bool Deprecated { get; set; }

        /// <summary>
        /// The name of the market.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The public key of the market account.
        /// </summary>
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey Address { get; set; }
        
        /// <summary>
        /// The public key of the program id.
        /// </summary>
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey ProgramId { get; set; }
    }
    
    /// <summary>
    /// Represents information about a market in Serum.
    /// </summary>
    [DebuggerDisplay("Mint = {Address.Key}")]
    public class TokenInfo
    {
        /// <summary>
        /// The name of the token.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The public key of the token mint.
        /// </summary>
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey Address { get; set; }
    }

}