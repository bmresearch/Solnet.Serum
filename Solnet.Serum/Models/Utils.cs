// unset

using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Utilities specific to Serum data encoding or decoding.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Check if a certain bit in a span of bytes is set.
        /// </summary>
        /// <param name="data">The data to check for the bit being set.</param>
        /// <param name="offset">The offset at which to check if the bit it set.</param>
        /// <returns>True if it is set, otherwise false.</returns>
        public static bool CheckBit(this ReadOnlySpan<byte> data, int offset)
        {
            int posByte = offset / 8;
            int posBit = offset % 8;
            byte valByte = data[posByte];
            int valInt = valByte >> posBit & 1;
            return valInt == 1;
        }
    }
}