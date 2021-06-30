// unset

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Open Order in a Serum Open Orders Account.
    /// </summary>
    public class OpenOrder
    {
        public int OrderIndex;

        public long ClientId;

        public byte[] ClientOrderId;

        public long Price;

        public bool IsFreeSlot;

        public bool IsBid;
    }
}