namespace Forester.Framework.EventStore.InMemory
{
    public interface INodeSynchronizer
    {
        IEnumerable<string> Synchronize(IEventStoreSynchronizerClient local, IEventStoreSynchronizerClient remote);
    }

    public class InMemoryNodeSynchronizer : INodeSynchronizer
    {
        public IEnumerable<string> Synchronize(IEventStoreSynchronizerClient local, IEventStoreSynchronizerClient remote)
        {
            var localVersion = local.GetCurrentNodeVersion();
            var remoteVersion = remote.GetCurrentNodeVersion();

            var missingFromRemote = local.ReadNewerAndConcurrentTo(remoteVersion).ToList();
            var missingFromLocal = remote.ReadNewerAndConcurrentTo(localVersion).ToList();

            local.Replicate(missingFromLocal);
            remote.Replicate(missingFromRemote);

            var localVersionMatrix = local.GetCurrentNodeVersionMatrix();
            var remoteVersionMatrix = remote.GetCurrentNodeVersionMatrix();

            local.SynchronizeVersionMatrix(remoteVersionMatrix, local.Node, remote.Node);
            remote.SynchronizeVersionMatrix(localVersionMatrix, local.Node, remote.Node);

            var conflictingStreams = GetConflictingStreams(
                GroupByStream(missingFromLocal), 
                GroupByStream(missingFromRemote));

            return conflictingStreams;
        }

        private IEnumerable<string> GetConflictingStreams(Dictionary<string, List<CommittedEvent>> local, Dictionary<string, List<CommittedEvent>> remote)
        {
            var localStreams = new HashSet<string>(local.Keys);
            var remoteStreams = new HashSet<string>(remote.Keys);

            var intersection = localStreams.Intersect(remoteStreams).ToHashSet();
            return intersection;
        }

        private Dictionary<string, List<CommittedEvent>> GroupByStream(IEnumerable<CommittedEvent> events)
        {
            return events.Aggregate(
                new Dictionary<string, List<CommittedEvent>>(),
                (streams, ev) =>
                {
                    if (!streams.ContainsKey(ev.StreamId))
                        streams.Add(ev.StreamId, new List<CommittedEvent>());
                    streams[ev.StreamId].Add(ev);
                    return streams;
                });
        }
    }
}
