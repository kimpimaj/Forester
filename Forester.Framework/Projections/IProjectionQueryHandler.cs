using Forester.Framework.EventStore;

namespace Forester.Framework.Projections
{
    public interface IProjection
    {
        List<string> AcceptedEventTypes { get; }
        void Rehydrate(IEnumerable<CommittedEvent> events);
    }

    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        TResult Query(TQuery query);
    }

    public interface IProjectionQueryHandler<TQuery, TResult> : 
            IQueryHandler<TQuery, TResult>,
            IProjection
        where TQuery : IQuery<TResult>
    {

    }
}
