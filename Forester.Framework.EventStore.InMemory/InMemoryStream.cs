using System.Diagnostics;

namespace Forester.Framework.EventStore.InMemory
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class InMemoryStream
    {
        public string StreamId { get; }
        public List<Guid> Events { get; }

        public VersionVector CurrentStreamVersion { get; private set; }
        public VersionVector LatestNodeVersion { get; private set; }

        public InMemoryStream(string streamId)
        {
            StreamId = streamId;
            Events = new List<Guid>();
            CurrentStreamVersion = new VersionVector();
            LatestNodeVersion = new VersionVector();
        }

        public void Append(CommittedEvent @event)
        {
            CurrentStreamVersion = @event.StreamVersion;
            LatestNodeVersion = @event.OccurredAtNodeVersion;
            Events.Add(@event.EventId);
        }

        public Guid? LastEventId() => Events.LastOrDefault();

        private string GetDebuggerDisplay()
        {
            return $"[StreamId: {StreamId}, CurrentVersion: {CurrentStreamVersion}, LatestNodeVersion: {LatestNodeVersion}, Events: {Events.Count}]";
        }
    }
}
