using Forester.Framework.EventStore;

namespace Forester.Framework.Projections
{
    internal interface IProjectionQueryRehydrator<TProjection>
        where TProjection : IProjection
    {
        TResult RehydrateAndQuery<TQueryHandler, TQuery, TResult>(TQueryHandler handler, TQuery query)
            where TQuery : IQuery<TResult>
            where TQueryHandler : IQueryHandler<TQuery, TResult>, TProjection;
    }

    internal class ProjectionQueryRehydrator<TProjection> : IProjectionQueryRehydrator<TProjection>
        where TProjection : IProjection
    {
        private readonly IEventStoreClient _eventStore;

        public ProjectionQueryRehydrator(IEventStoreClient store)
        {
            _eventStore = store;
        }

        public TResult RehydrateAndQuery<TQueryHandler, TQuery, TResult>(TQueryHandler handler, TQuery query)
            where TQuery : IQuery<TResult>
            where TQueryHandler : IQueryHandler<TQuery, TResult>, TProjection
        {
            Rehydrate(handler, query);
            return handler.Query(query);
        }

        private void Rehydrate<TQuery>(TProjection projection, TQuery query)
            where TQuery : IQuery
        {
            var events = _eventStore
                .Query(new EventStoreQuery(query.Mode, projection.AcceptedEventTypes, query.AsAt, query.AsOf))
                .ToList();
            projection.Rehydrate(events);
        }
    }
}
