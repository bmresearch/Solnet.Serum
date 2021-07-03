using Solnet.Serum.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Represents an Event Queue in Serum.
    /// </summary>
    public class EventQueue
    {
        /// <summary>
        /// The header of the event queue, which specifies item count etc.
        /// </summary>
        public QueueHeader Header;

        /// <summary>
        /// The events in the queue.
        /// </summary>
        public IList<Event> Events;
        
        /// <summary>
        /// Deserialize a span of bytes into an <see cref="EventQueue"/> instance.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <returns>The Event Queue structure.</returns>
        public static EventQueue Deserialize(ReadOnlySpan<byte> data)
        {
            QueueHeader header = QueueHeader.Deserialize(data[..QueueHeaderDataLayout.QueueHeaderSpanLength]);

            ReadOnlySpan<byte> headLessData = data.Slice(
                QueueHeaderDataLayout.QueueHeaderSpanLength, 
                data.Length - QueueHeaderDataLayout.QueueHeaderSpanLength);

            int numElements = headLessData.Length / EventDataLayout.EventSpanLength;
            List<Event> events = new (numElements);

            for (int i = 0; i < numElements; i++)
            {
                long idx = (header.Head + header.Count + numElements - 1 - i) % numElements;
                long evtOffset = idx * EventDataLayout.EventSpanLength;

                Event evt = Event.Deserialize(
                    headLessData.Slice((int) evtOffset, EventDataLayout.EventSpanLength));
                events.Add(evt);
            }
            return new EventQueue
            {
                Header = header,
                Events = events
            };
        }

        /// <summary>
        /// Deserialize a span of bytes into an <see cref="EventQueue"/> instance that contains only the new events
        /// since the given sequence number.
        /// </summary>
        /// <param name="data">The data to deserialize into the structure.</param>
        /// <param name="lastSequenceNumber">The previous queue's next sequence number.</param>
        /// <returns>The Event Queue structure.</returns>
        public static EventQueue DeserializeSince(ReadOnlySpan<byte> data, long lastSequenceNumber = 0)
        {
            QueueHeader header = QueueHeader.Deserialize(data[..QueueHeaderDataLayout.QueueHeaderSpanLength]);

            ReadOnlySpan<byte> headLessData = data.Slice(
                QueueHeaderDataLayout.QueueHeaderSpanLength, 
                data.Length - QueueHeaderDataLayout.QueueHeaderSpanLength);

            int numElements = headLessData.Length / EventDataLayout.EventSpanLength;

            // Calculate number of missed events
            // Account for u32 & ring buffer overflows
            const long modulo = 0x100000000;
            long missedEvents = (header.NextSequenceNumber - lastSequenceNumber + modulo) % modulo;

            if (missedEvents > numElements)
            {
                missedEvents = numElements - 1;
            }

            long startSequence = (header.NextSequenceNumber - missedEvents + modulo) % numElements;
            
            // Define boundary indexes in ring buffer [start;end]
            long endIdx = (header.Head + header.Count) % numElements;
            long startIdx = (endIdx - missedEvents + numElements) % numElements;
            
            List<Event> events = new ();
            
            for (int i = 0; i < missedEvents; i++)
            {
                long idx = (startIdx + i) % numElements;
                long evtOffset = idx * EventDataLayout.EventSpanLength;

                Event evt = Event.Deserialize(
                    headLessData.Slice((int) evtOffset, EventDataLayout.EventSpanLength));
                evt.SequenceNumber = (startSequence + i) % modulo;
                events.Add(evt);
            }
            
            return new EventQueue
            {
                Header = header,
                Events = events
            };
        }
    }
}