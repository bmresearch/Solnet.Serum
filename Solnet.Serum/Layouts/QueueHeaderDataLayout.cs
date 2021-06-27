using Solnet.Serum.Models;

namespace Solnet.Serum.Layouts
{
    /// <summary>
    /// Represents the layout of the <see cref="QueueHeader"/> data structure.
    /// </summary>
    internal class QueueHeaderDataLayout
    {
        /// <summary>
        /// The size of the data for a queue header structure.
        /// </summary>
        internal const int QueueHeaderSpanLength = 37;

        /// <summary>
        /// The number of bytes of the padding at the beginning of the queue header structure.
        /// </summary>
        internal const int StartPadding = 5;

        /// <summary>
        /// The number of bytes of the padding at the end of the queue header structure.
        /// </summary>
        internal const int EndPadding = 4;

        /// <summary>
        /// The offset at which the value of the queue's head begins.
        /// </summary>
        internal const int HeadOffset = 8;

        /// <summary>
        /// The offset at which the value of the queue's count begins.
        /// </summary>
        internal const int CountOffset = 16;

        /// <summary>
        /// The offset at which the value of the queue's next sequence number begins.
        /// </summary>
        internal const int NextSequenceNumberOffset = 24;
    }
}