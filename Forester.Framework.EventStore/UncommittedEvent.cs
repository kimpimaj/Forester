namespace Forester.Framework.EventStore
{
    public class UncommittedEvent
    {
        public UncommittedEvent(string eventType, object payload)
        {
            this.EventType = eventType;
            this.Payload = payload;
        }

        public string EventType { get; }
        public object Payload { get; }
    }
}