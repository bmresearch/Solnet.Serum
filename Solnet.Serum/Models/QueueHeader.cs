using Solnet.Programs.Utilities;
using Solnet.Serum.Layouts;
using Solnet.Serum.Models.Flags;
using System;
using System.Buffers.Binary;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the header of either an <see cref="EventQueue"/>.
    /// </summary>
    public class QueueHeader
    {
        #region Layout
        
        /// <summary>
        /// Represents the layout of the <see cref="QueueHeader"/> data structure.
        /// </summary>
        internal class Layout
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

        #endregion
        
        /// <summary>
        /// The flags which define this queue account.
        /// </summary>
        public AccountFlags Flags;

        /// <summary>
        /// The _
        /// </summary>
        public uint Head;

        /// <summary>
        /// The number of _
        /// </summary>
        public uint Count;

        /// <summary>
        /// The value which defines the next sequence number.
        /// </summary>
        public uint NextSequenceNumber;

        /// <summary>
        /// Deserialize a span of bytes into a <see cref="Market"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Market structure.</returns>
        public static QueueHeader Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length != Layout.QueueHeaderSpanLength)
                return null;

            ReadOnlySpan<byte> padLessData = data.Slice(
                Market.Layout.StartPadding,
                data.Length - (Layout.StartPadding + Layout.EndPadding));

            AccountFlags flags = AccountFlags.Deserialize(padLessData[..8]);

            QueueHeader header = new()
            {
                Flags = flags,
                Head = padLessData.GetU32(Layout.HeadOffset),
                Count = padLessData.GetU32(Layout.CountOffset),
                NextSequenceNumber = padLessData.GetU32(Layout.NextSequenceNumberOffset)
            };

            return header;
        }
        
    }
}