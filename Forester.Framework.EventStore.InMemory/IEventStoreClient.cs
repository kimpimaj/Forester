using Forester.Framework.Projections;

namespace Forester.Framework.EventStore.InMemory
{
    public record EventStoreLatestQuery(List<string> EventTypes)
    {

    }

    public record EventStoreQuery(ProjectionMode Mode, List<string> EventTypes, DateTime AsAt, DateTime AsOf);

    public class EventStoreLocalQuery
    {
        public EventStoreLocalQuery(DateTime asAt, DateTime asOf)
        {
            AsAt = asAt;
            AsOf = asOf;
        }

        public DateTime AsAt { get; }
        public DateTime AsOf { get; }
    }

    public record EventStoreStableQuery(List<string> EventTypes) { }

    public interface IEventStoreProjectionClient
    {
        IEnumerable<CommittedEvent> Query(EventStoreQuery query);
        IEnumerable<CommittedEvent> ReadLatest(EventStoreLatestQuery query);
        IEnumerable<CommittedEvent> ReadStable(EventStoreStableQuery query);
    }

    public interface IEventStoreAggregateClient
    {
        IEnumerable<CommittedEvent> Read(string streamId);
        bool Append(string streamId, IList<UncommittedEvent> events);
    }

    public interface IEventStoreSynchronizerClient
    {
        IEnumerable<CommittedEvent> ReadNewerAndConcurrentTo(VersionVector timestamp);
        void Replicate(IEnumerable<CommittedEvent> missingFromLocal);
        void SynchronizeVersionMatrix(VersionMatrix localVersionMatrix, string localNode, string remoteNode);
        VersionVector GetCurrentNodeVersion();
        VersionMatrix GetCurrentNodeVersionMatrix();
        IEnumerable<string> Synchronize(string remoteAddress);
        string Node { get; }
    }

    public interface IEventStoreClient2 :
        IEventStoreAggregateClient,
        IEventStoreSynchronizerClient,
        IEventStoreProjectionClient
    {
        void Setup(List<string> knownNodes);
        VersionVector GetCurrentStableVersion();
    }
}