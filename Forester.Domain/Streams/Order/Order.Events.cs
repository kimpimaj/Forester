using Forester.Framework.Aggregates.DynamicallyOwned;
using ProtoBuf;

namespace Forester.Domain.Streams.Order
{
    [ProtoContract(SkipConstructor = true)]
    public record OrderIssuedEvent(
        [property: ProtoMember(1)] string OrderId,
        [property: ProtoMember(2)] string SiteId,
        [property: ProtoMember(3)] string SalesPerson) : IDynamicallyOwnedCreationEvent
    {
        public string StreamId => OrderId;

        public string InitialOwner => SalesPerson;
    }

    [ProtoContract(SkipConstructor = true)]
    public record AssignedToHarvesterEvent(
        [property: ProtoMember(1)] string Harvester);

    [ProtoContract(SkipConstructor = true)]
    public record SiteHarvestedEvent(
        [property: ProtoMember(1)] string SalesPerson,
        [property: ProtoMember(2)] string OrderId);

    [ProtoContract(SkipConstructor = true)]
    public record OrderCompletedEvent(
        [property: ProtoMember(1)] string OrderId,
        [property: ProtoMember(2)] string Report);
}
