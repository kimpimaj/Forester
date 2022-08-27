using Forester.Framework.EventStore;

namespace Forester.Framework.Aggregates
{
    internal class SimpleRehydrator<TAggregate> : IRehydrator<TAggregate>
        where TAggregate : IAggregate
    {
        private readonly IEventStoreClient _eventStore;

        public SimpleRehydrator(IEventStoreClient store)
        {
            _eventStore = store;
        }

        public void RehydrateAndExecute<TAggregateHandler, TCommand>(TAggregateHandler aggregate, TCommand command)
            where TCommand : ICommand
            where TAggregateHandler : TAggregate, ICommandHandler<TCommand>
        {
            Rehydrate(aggregate);
            aggregate.Handle(command);
            Save(aggregate);
        }

        private void Rehydrate(TAggregate aggregate)
        {
            var events = _eventStore
                .Read(aggregate.StreamId)
                //.Select(e => e.Payload)
                .ToList();

            aggregate.Rehydrate(events);
        }

        private void Save(TAggregate aggregate)
        {
            var transformedEvents = aggregate
                .UncommittedEvents
                .ToList();

            if (_eventStore.Append(aggregate.StreamId, transformedEvents))
            {
                aggregate.Commit();
            }
        }
    }
}