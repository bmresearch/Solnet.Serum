namespace Solnet.Serum.Models.Flags
{
    /// <summary>
    /// Represents bitmask flags for various types of accounts within Serum.
    /// </summary>
    public class Flag
    {
        /// <summary>
        /// The bitmask for the account flags.
        /// </summary>
        protected readonly byte Bitmask;

        /// <summary>
        /// Initialize the flags with the given mask.
        /// </summary>
        /// <param name="mask">The mask to use.</param>
        protected Flag(byte mask)
        {
            Bitmask = mask;
        }
    }
}