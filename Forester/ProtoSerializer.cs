using Forester.Framework.Aggregates.DynamicallyOwned;
using Forester.Framework.EventStore;
using ProtoBuf;

namespace Forester
{
    public class ProtoSerializer
    {
        protected TEvent Deserialize<TEvent>(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<TEvent>(stream);
            }
        }

        protected byte[] Serialize<TEvent>(TEvent @event)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, @event);
                return stream.ToArray();
            }
        }
    }

    public class GenericEventSerializer<TEvent> : ProtoSerializer,
        ISerializer<TEvent>
    {
        public string SerializedTypeName() => typeof(TEvent).Name;
        public TEvent? Deserialize(byte[] data) => Deserialize<TEvent>(data);
        public byte[] Serialize(TEvent @event) => base.Serialize(@event);
    }


    public class OwnershipTransferredEventSerializer : ISerializer<OwnershipTransferredEvent>
    {
        [ProtoContract(SkipConstructor = true)]
        public record OwnershipTransferredEventDto(
            [property: ProtoMember(1)] string StreamId,
            [property: ProtoMember(2)] string NewOwner,
            [property: ProtoMember(3)] string PreviousOwner);

        private readonly GenericEventSerializer<OwnershipTransferredEventDto> _serializer = new GenericEventSerializer<OwnershipTransferredEventDto>();

        public OwnershipTransferredEvent? Deserialize(byte[] data)
        {
            var dto = _serializer.Deserialize(data);
            return new OwnershipTransferredEvent(dto.StreamId, dto.NewOwner, dto.PreviousOwner);
        }

        public byte[] Serialize(OwnershipTransferredEvent @event)
        {
            var dto = new OwnershipTransferredEventDto(@event.StreamId, @event.NewOwner, @event.PreviousOwner);
            return _serializer.Serialize(dto);
        }

        public string SerializedTypeName()
        {
            return typeof(OwnershipTransferredEvent).Name;
        }
    }
}
