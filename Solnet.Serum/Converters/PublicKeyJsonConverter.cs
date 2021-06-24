// unset

using Solnet.Wallet;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solnet.Serum.Converters
{
    /// <summary>
    /// Implements a json converter to deserialize public keys from a string.
    /// </summary>
    public class PublicKeyJsonConverter : JsonConverter<PublicKey>
    {
        /// <inheritdoc/>
        public override PublicKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string content = reader.GetString();

            return new PublicKey(content);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, PublicKey value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}