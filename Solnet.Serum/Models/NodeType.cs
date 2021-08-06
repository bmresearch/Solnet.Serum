namespace Solnet.Serum.Models
{
    /// <summary>
    /// Specifies the <see cref="SlabNode"/> types on the <see cref="Slab"/>.
    /// </summary>
    public enum NodeType : byte
    {
        /// <summary>
        /// Represents an uninitialized node on the slab.
        /// </summary>
        Uninitialized = 0,
        
        /// <summary>
        /// Represents an inner node on the slab.
        /// </summary>
        InnerNode = 1,
        
        /// <summary>
        /// Represents a leaf node on the slab.
        /// </summary>
        LeafNode = 2,
        
        /// <summary>
        /// Represents a free node on the slab.
        /// </summary>
        FreeNode = 3,
        
        /// <summary>
        /// Represents the last free node of the slab.
        /// </summary>
        LastFreeNode = 4,
    }
}