namespace Forester.Framework.EventStore
{
    public record EventStoreLatestQuery(List<string> EventTypes)
    {

    }


    public class EventStoreLocalQuery
    {
        public EventStoreLocalQuery(VersionVector asAt, VersionVector asOf)
        {
            AsAt = asAt;
            AsOf = asOf;
        }

        public VersionVector AsAt { get; }
        public VersionVector AsOf { get; }
    }

    public record EventStoreStableQuery(List<string> nodes) { }

    public interface IEventStoreReader
    {
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
        VersionVector GetCurrentNodeVersion();
        void Replicate(IEnumerable<CommittedEvent> missingFromLocal);
        VersionMatrix GetCurrentNodeVersionMatrix();
        void SynchronizeVersionMatrix(VersionMatrix localVersionMatrix);
    }

    public interface IEventStoreClient :
        IEventStoreAggregateClient,
        IEventStoreSynchronizerClient,
        IEventStoreReader
    {
        void Setup();
        VersionVector GetCurrentStableVersion(List<string> nodes);
    }
}