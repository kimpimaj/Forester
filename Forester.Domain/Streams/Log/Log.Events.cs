using Forester.Framework.Aggregates;
using ProtoBuf;

namespace Forester.Domain.Streams.Log
{
    public interface ILogEventHander :
        IEventHandler<TreeIdentifiedEvent>,
        IEventHandler<TreeCutDownEvent>,
        IEventHandler<LogCutToLengthEvent>
    { }

    [ProtoContract(SkipConstructor = true)]
    public record Location(
        [property: ProtoMember(1)] int Latitude,
        [property: ProtoMember(2)] int Longitude);

    [ProtoContract(SkipConstructor = true)]
    public record TreeIdentifiedEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] string SiteId,
        [property: ProtoMember(3)] Location At,
        [property: ProtoMember(4)] string Species) : ICreationEvent
    {
        public string TreeId => StreamId;
    }

    [ProtoContract(SkipConstructor = true)]
    public record TreeCutDownEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] string By) : ICreationEvent
    {
        public string TreeId => StreamId;
    }

    [ProtoContract(SkipConstructor = true)]
    public record LogCutToLengthEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] string LogId,
        [property: ProtoMember(3)] Location DroppedTo,
        [property: ProtoMember(4)] int CutLength,
        [property: ProtoMember(5)] int Volume)
    {
        public string TreeId => StreamId;
    }
}
