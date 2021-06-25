using Solnet.Serum.Layouts;
using Solnet.Serum.Models.Flags;
using System;
using System.Buffers.Binary;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents the header of either an <see cref="EventQueue"/> or a <see cref="RequestQueue"/>.
    /// </summary>
    public class QueueHeader
    {
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
            if (data.Length != QueueHeaderLayout.QueueHeaderDataSize)
                return null;

            ReadOnlySpan<byte> padLessData = data.Slice(
                MarketDataLayout.StartPadding,
                data.Length - (QueueHeaderLayout.StartPadding + QueueHeaderLayout.EndPadding));

            AccountFlags flags = AccountFlags.Deserialize(padLessData[..8]);

            QueueHeader header = new()
            {
                Flags = flags,
                Head = BinaryPrimitives.ReadUInt32LittleEndian(
                    padLessData.Slice(QueueHeaderLayout.HeadOffset, 4)),
                Count = BinaryPrimitives.ReadUInt32LittleEndian(
                    padLessData.Slice(QueueHeaderLayout.CountOffset, 4)),
                NextSequenceNumber = BinaryPrimitives.ReadUInt32LittleEndian(
                    padLessData.Slice(QueueHeaderLayout.NextSequenceNumberOffset, 4))
            };

            return header;
        }
        
    }
}