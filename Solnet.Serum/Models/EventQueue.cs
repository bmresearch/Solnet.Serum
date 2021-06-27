using Solnet.Serum.Layouts;
using Solnet.Wallet;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

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
        /// Deserialize a span of bytes into a <see cref="EventQueue"/> instance.
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
    }
}