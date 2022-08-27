using Forester.Framework.EventStore;

namespace Forester.Framework.EventStore.InMemory
{
    public interface IClock
    {
        DateTime Now();
    }

    public class DefaultClock : IClock
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }

    public class InMemoryEventStoreAdapter : IEventStoreClient, IEventStoreSynchronizerClient
    {
        private static readonly Dictionary<string, IEventStoreSynchronizerClient> _clients = new Dictionary<string, IEventStoreSynchronizerClient>();

        private readonly InMemoryEventStore _eventStore;
        private List<string> _knownNodes;

        public string Node { get; }

        public InMemoryEventStoreAdapter(string name, IClock clock)
        {
            _eventStore = new InMemoryEventStore(name, clock);
            _clients.Add(name, this);
            Node = name;
        }

        public InMemoryEventStoreAdapter(string name)
        {
            _eventStore = new InMemoryEventStore(name, new DefaultClock());
            _clients.Add(name, this);
        }

        public bool Append(string streamId, IList<UncommittedEvent> events)
        {
            return _eventStore.Append(streamId, events);
        }

        public VersionVector GetCurrentNodeVersion()
        {
            return _eventStore.GetCurrentNodeVersion();
        }

        public IEnumerable<CommittedEvent> Read(string streamId)
        {
            return _eventStore.Read(streamId);
        }

        public IEnumerable<CommittedEvent> ReadNewerAndConcurrentTo(VersionVector timestamp)
        {
            return _eventStore.ReadNewerAndConcurrentTo(timestamp);
        }

        public void Replicate(IEnumerable<CommittedEvent> missingFromLocal)
        {
            _eventStore.Replicate(missingFromLocal);
        }

        public void Setup(List<string> knownNodes)
        {
            _knownNodes = knownNodes;
            // InMemory does not need any setup
        }

        public VersionMatrix GetCurrentNodeVersionMatrix()
        {
            return _eventStore.GetCurrentNodeVersionMatrix();
        }

        public void SynchronizeVersionMatrix(VersionMatrix versionMatrix, string localNode, string remoteNode)
        {
            _eventStore.SynchronizeVersionMatrix(versionMatrix, localNode, remoteNode);
        }

        public VersionVector GetCurrentStableVersion()
        {
            return _eventStore.GetCurrentNodeVersionMatrix().Stable(_knownNodes);
        }

        public IEnumerable<CommittedEvent> ReadLatest(EventStoreLatestQuery query)
        {
            return _eventStore.Read()
                .Where(e => query.EventTypes.Contains(e.Type));
        }

        public IEnumerable<string> Synchronize(string remoteAddress)
        {
            var synchronizer = new InMemoryNodeSynchronizer();
            var remote = _clients[remoteAddress];

            return synchronizer.Synchronize(this, remote);
        }

        public IEnumerable<CommittedEvent> Query(EventStore.EventStoreQuery query)
        {
            var results = _eventStore.Read()
                .Where(e => query.EventTypes.Contains(e.Type))
                .Where(e => e.RecordedAt <= query.AsOf && e.OccurredAt <= query.AsAt);

            if (query.Mode == Projections.ProjectionMode.Stable)
            {
                var stable = GetCurrentStableVersion();
                return results.Where(e => e.OccurredAtNodeVersion <= stable);
            }
            else
            {
                return results;
            }
        }

        public IEnumerable<CommittedEvent> ReadLatest(EventStore.EventStoreLatestQuery query)
        {
            var stable = GetCurrentStableVersion();

            return _eventStore.Read()
                .Where(e => query.EventTypes.Contains(e.Type));
        }

        public IEnumerable<CommittedEvent> ReadStable(EventStore.EventStoreStableQuery query)
        {
            var stable = GetCurrentStableVersion();

            return _eventStore.Read()
                .Where(e => query.EventTypes.Contains(e.Type))
                .Where(e => e.OccurredAtNodeVersion <= stable);
        }
    }
}
