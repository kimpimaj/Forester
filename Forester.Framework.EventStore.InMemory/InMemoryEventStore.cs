namespace Forester.Framework.EventStore.InMemory
{
    internal class InMemoryEventStore
    {
        private readonly string _name;
        private readonly IClock _clock;
        private readonly Dictionary<Guid, CommittedEvent> _events;
        private readonly Dictionary<string, InMemoryStream> _aggregateStreams;
        private readonly List<Guid> _eventsInRecordedOrder;

        private VersionVector _nodeVersion;
        private VersionMatrix _systemVersion;

        public InMemoryEventStore(string name, IClock clock)
        {
            _name = name;
            _clock = clock;
            _events = new();
            _aggregateStreams = new();
            _eventsInRecordedOrder = new();

            _nodeVersion = new();
            _systemVersion = new();
        }

        #region Read

        public IEnumerable<CommittedEvent> Read(string streamId)
        {
            if (!_aggregateStreams.ContainsKey(streamId))
                return new List<CommittedEvent>();

            return _aggregateStreams[streamId]
                .Events
                .Select(e => _events[e]);
        }

        public IEnumerable<CommittedEvent> ReadNewerAndConcurrentTo(VersionVector timestamp)
        {
            return _eventsInRecordedOrder
                .Select(e => _events[e])
                .Where(e =>
                {
                    var comparison = e.OccurredAtNodeVersion.ComparedTo(timestamp);
                    return comparison != VersionVector.Comparison.IsOlder && comparison != VersionVector.Comparison.AreSame;
                });
        }

        internal IEnumerable<CommittedEvent> Read(DateTime asAt, DateTime asOf)
        {
            return _eventsInRecordedOrder
                .Select(e => _events[e])
                .Where(e => e.OccurredAt <= asAt && e.RecordedAt <= asOf);
        }

        internal IEnumerable<CommittedEvent> Read()
        {
            return _eventsInRecordedOrder
                .Select(e => _events[e]);
        }

        #endregion

        #region Write

        public bool Append(string streamId, IList<UncommittedEvent> events)
        {
            var stream = GetOrCreateStream(streamId);

            foreach (var ev in events)
            {
                var nextStreamVersion = stream.CurrentStreamVersion.Next(_name);
                var nextSystemVersion = NextSystemVersion();
                var completedEvent = new CommittedEvent(Guid.NewGuid(), ev.EventType, ev.Payload, streamId, _name, nextStreamVersion, nextSystemVersion, _clock.Now());

                Save(stream, completedEvent);
            }

            return true;
        }

        public void Replicate(IEnumerable<CommittedEvent> replicatedEvents)
        {
            foreach (var replicatedEvent in replicatedEvents)
            {
                var nextNodeVersion = ReplicatedVersion(replicatedEvent.OccurredAtNodeVersion);
                var toStore = replicatedEvent.AsRecordedAt(nextNodeVersion, _clock.Now());

                Save(GetOrCreateStream(toStore.StreamId), toStore);
            }
        }

        #endregion

        #region Versioning

        internal VersionMatrix GetCurrentNodeVersionMatrix() => _systemVersion.Copy();

        public VersionVector GetCurrentNodeVersion() => _nodeVersion.Copy();

        internal void SynchronizeVersionMatrix(VersionMatrix versionMatrix, string localNode, string remoteNode)
        {
            // Could use Ceil instead Sync, because assumption is that events fed during synchronization merges version vectors.
            _systemVersion = _systemVersion.Sync(versionMatrix, localNode, remoteNode);
        }

        private VersionVector NextSystemVersion()
        {
            _nodeVersion = _nodeVersion.Next(_name);
            _systemVersion = _systemVersion.Update(_name, _nodeVersion);
            return _nodeVersion;
        }

        private VersionVector ReplicatedVersion(VersionVector baseline)
        {
            _nodeVersion = _nodeVersion.Ceil(baseline);
            _systemVersion = _systemVersion.Update(_name, _nodeVersion);
            return _nodeVersion;
        }

        #endregion

        #region Helpers

        private InMemoryStream GetOrCreateStream(string streamId)
        {
            if (!_aggregateStreams.ContainsKey(streamId))
                _aggregateStreams[streamId] = new InMemoryStream(streamId);

            return _aggregateStreams[streamId];
        }

        private void Save(InMemoryStream stream, CommittedEvent @event)
        {
            stream.Append(@event);
            _eventsInRecordedOrder.Add(@event.EventId);
            _events[@event.EventId] = @event;
        }

        #endregion
    }
}
