using Forester.Framework.Aggregates;
using Forester.Framework.EventStore;

namespace Forester.Framework.Policies
{
    internal interface IPolicyRehydrator
    {
        IList<ICommand> RehydrateAndTrigger<TPolicy>(TPolicy handler, PolicyTrigger query)
            where TPolicy : IPolicy;
    }

    internal class PolicyRehydrator : IPolicyRehydrator
    {
        private readonly IEventStoreClient _eventStore;

        public PolicyRehydrator(IEventStoreClient store)
        {
            _eventStore = store;
        }

        public IList<ICommand> RehydrateAndTrigger<TPolicy>(TPolicy handler, PolicyTrigger query)
            where TPolicy : IPolicy
        {
            Rehydrate(handler, query);
            return handler.Trigger(query);
        }

        private void Rehydrate(IPolicy projection, PolicyTrigger query)
        {
            var events = _eventStore
                .Query(new EventStoreQuery(query.Mode, projection.AcceptedEventTypes, query.AsAt, query.AsOf))
                .ToList();
            projection.Rehydrate(events);
        }
    }
}
