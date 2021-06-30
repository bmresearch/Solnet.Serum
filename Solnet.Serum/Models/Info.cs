using Solnet.Serum.Converters;
using Solnet.Wallet;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Solnet.Serum.Models
{
    // Represents information about a market in Serum.
    [DebuggerDisplay("Market = {Address.Key}")]
    public class MarketInfo
    {
        public bool   Deprecated { get; set; }   // Whether the market has been deprecated or not.
        public string Name       { get; set; }   // The name of the market.
       
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey Address { get; set; }   // The public key of the market account.
        
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey ProgramId { get; set; } // The public key of the program id.
    }
    
    /// Represents information about a token mint in Serum.
    [DebuggerDisplay("Mint = {Address.Key}")]
    public class TokenMintInfo
    {
        public string Name { get; set; }        // The name of the token mint.
        [JsonConverter(typeof(PublicKeyJsonConverter))]
        public PublicKey Address { get; set; }  // The public key of the token mint.
    }
}