using System;
using Solnet.Serum.Shared;

namespace Solnet.Serum.Models
{
    public class QueueHeader
    {
        public const int SerializedLength = 37;
        public const int PadBytesAtStart  = 5;   // Number of padding bytes at the start
        public const int PadBytesAtEnd    = 4;   // Number of padding bytes at the end

        public static class Layout
        {
            public const int AccountFlags =  0;
            public const int Head         =  8;
            public const int Count        = 16;
            public const int NextSeqNum   = 24;
        }

        public AccountFlags Flags;       // The flags which define this queue account
        public uint         Head;        // The head of the queue
        public uint         Count;       // The number of items in the queue
        public uint         NextSeqNum;  // The next sequence number

        public static QueueHeader Deserialize(ReadOnlySpan<byte> dataWithPadding)
        {
            if (dataWithPadding.Length != SerializedLength) { return null; }
            int lengthWithoutPadding = dataWithPadding.Length - (PadBytesAtStart + PadBytesAtEnd);    
            ReadOnlySpan<byte> data  = dataWithPadding.Slice(PadBytesAtStart, lengthWithoutPadding);

            QueueHeader header = new()
            {
                Flags      = data.GetU64(Layout.AccountFlags),
                Head       = data.GetU32(Layout.Head),
                Count      = data.GetU32(Layout.Count),
                NextSeqNum = data.GetU32(Layout.NextSeqNum)
            };

            return header;
        }
        
    }
}