namespace Forester.Framework.EventStore
{
    public class CommittedEvent
    {
        public Guid EventId { get; set; }
        public string StreamId { get; }
        public string Issuer { get; }
        public object Payload { get; }
        public string Type { get; }

        public VersionVector StreamVersion { get; }
        public VersionVector OccurredAtNodeVersion { get; }
        public VersionVector RecordedAtNodeVersion { get; }

        public DateTime OccurredAt { get; }
        public DateTime RecordedAt { get; }

        public CommittedEvent(Guid eventId, string type, object payload, string streamId, string issuer, VersionVector streamVersion, VersionVector occurredAtNodeVersion, DateTime occurredAt)
        {
            Payload = payload;
            EventId = eventId;
            Type = type;
            StreamId = streamId;
            Issuer = issuer;
            StreamVersion = streamVersion;
            OccurredAtNodeVersion = occurredAtNodeVersion;
            RecordedAtNodeVersion = occurredAtNodeVersion;
            OccurredAt = occurredAt;
            RecordedAt = occurredAt;
        }

        public CommittedEvent(Guid eventId, string type, object payload, string streamId, string issuer, VersionVector streamVersion, VersionVector occurredAtNodeVersion, VersionVector recordedAtNodeVersion, DateTime occurredAt, DateTime recordedAt)
        {
            Payload = payload;
            EventId = eventId;
            Type = type;
            StreamId = streamId;
            Issuer = issuer;
            StreamVersion = streamVersion;
            OccurredAtNodeVersion = occurredAtNodeVersion;
            RecordedAtNodeVersion = recordedAtNodeVersion;
            OccurredAt = occurredAt;
            RecordedAt = recordedAt;
        }

        public CommittedEvent AsRecordedAt(VersionVector recordedAtNodeVersion, DateTime recordedAt)
        {
            return new CommittedEvent(EventId, Type, Payload, StreamId, Issuer, StreamVersion, OccurredAtNodeVersion, recordedAtNodeVersion, OccurredAt, recordedAt);
        }
    }
}