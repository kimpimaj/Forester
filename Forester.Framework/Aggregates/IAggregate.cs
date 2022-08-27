using Forester.Framework.EventStore;

namespace Forester.Framework.Aggregates
{

    public interface IAggregate
    {
        /// <summary>
        /// Unique identifier for the stream this aggregate represents.
        /// </summary>
        string StreamId { get; }

        /// <summary>
        /// Contains events that are not yet saved to Event Store, meaning 
        /// events that are raised during a command execution.
        /// </summary>
        IList<UncommittedEvent> UncommittedEvents { get; }

        /// <summary>
        /// Tell aggregate to commit uncommitted events.
        /// </summary>
        void Commit();

        /// <summary>
        /// Play given events to build up desired state.
        /// </summary>
        void Rehydrate(IList<CommittedEvent> events);

        //VersionVector Version { get; }
    }

    public interface ICreationEvent
    {
        string StreamId { get; }
    }

    public abstract class AggregateBase : IAggregate
    {
        public string StreamId { get; protected internal set; }
        public VersionVector StreamVersion { get; private set; }

        protected AggregateBase(string streamId)
        {
            StreamId = streamId;
            StreamVersion = new VersionVector();
        }

        public IList<UncommittedEvent> UncommittedEvents { get; } = new List<UncommittedEvent>();

        public void Commit()
        {
            UncommittedEvents.Clear();
        }

        public virtual void Rehydrate(IList<CommittedEvent> events)
        {
            foreach (var @event in events)
            {
                StreamVersion = @event.StreamVersion;
                Rehydrate(@event.Payload);
            }
        }

        //protected abstract bool Rehydrate(object @event);
        protected virtual void Rehydrate(object @event)
        {
            var eventType = @event.GetType();
            var handlerType = typeof(IEventHandler<>)
                .MakeGenericType(eventType);

            var method = handlerType.GetMethod("When");

            if (GetType().GetInterfaces().Contains(handlerType) && method != null)
            {
                method.Invoke(this, new[] { @event });
            }
        }

        protected void RaiseEvent<TEventPayload>(IEventHandler<TEventPayload> handler, TEventPayload @event)
        {
            RaiseEventImpl(handler, @event);
        }

        internal virtual void RaiseEventImpl<TEventPayload>(IEventHandler<TEventPayload> handler, TEventPayload @event)
        {
            StreamVersion = StreamVersion.Next(StreamId);
            UncommittedEvents.Add(new UncommittedEvent(typeof(TEventPayload).Name, @event));
            handler.When(@event);
        }
    }

}