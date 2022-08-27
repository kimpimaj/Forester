using Forester.Framework.Aggregates;

namespace Forester.Framework.Aggregates.Simple
{
    public abstract class SimpleAggregate<TCreationEvent> : AggregateBase
        where TCreationEvent : ICreationEvent
    {
        private bool _created;

        protected SimpleAggregate(string streamId) : base(streamId)
        {
        }

        protected void RaiseCreationEvent<TEventPayload>(IEventHandler<TEventPayload> handler, TEventPayload @event)
            where TEventPayload : TCreationEvent
        {
            if (_created == true)
            {
                throw new InvalidOperationException("Only one creation event is allowed");
            }

            StreamId = @event.StreamId;
            RaiseEventImpl(handler, @event);
        }
    }
}