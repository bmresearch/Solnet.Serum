using System;
using System.Collections.Generic;

namespace Solnet.Serum.Models
{
    public class EventQueue
    {
        public QueueHeader  Header; // The header of the event queue, which specifies item count etc.
        public IList<Event> Events; // The events in the queue.
        
        // Deserialize a span of bytes into an 'EventQueue' instance
        public static EventQueue Deserialize(ReadOnlySpan<byte> dataWithHeader)
        {
            int headerLen = QueueHeader.SerializedLength;
            QueueHeader header = QueueHeader.Deserialize(dataWithHeader[..headerLen]);
            ReadOnlySpan<byte> data = dataWithHeader.Slice(headerLen, dataWithHeader.Length - headerLen);

            int numElements = data.Length / Event.SerializedLength;
            List<Event> events = new (numElements);

            for (int i = 0; i < numElements; i++)
            {
                long idx = (header.Head + header.Count + numElements - 1 - i) % numElements;
                long evtOffset = idx * Event.SerializedLength;
                Event evt = Event.Deserialize(data.Slice((int) evtOffset, Event.SerializedLength));
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