using Forester.Framework.Aggregates;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Domain.Streams.LogLocations
{
    public interface ILogLocationsEventHandler :
        IEventHandler<BunchPickedByForwarderEvent>,
        IEventHandler<BunchGroundedByForwarderEvent>,
        IEventHandler<BunchLoadedToTruckEvent>,
        IEventHandler<StoredToWarehouseEvent>
    { }

    [ProtoContract(SkipConstructor = true)]
    public record Location(
        [property: ProtoMember(1)] int Latitude,
        [property: ProtoMember(2)] int Longitude);

    [ProtoContract(SkipConstructor = true)]
    public record BunchPickedByForwarderEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] Location From,
        [property: ProtoMember(3)] string By);

    [ProtoContract(SkipConstructor = true)]
    public record BunchGroundedByForwarderEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] string By,
        [property: ProtoMember(3)] Location To);

    [ProtoContract(SkipConstructor = true)]
    public record BunchLoadedToTruckEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] Location From,
        [property: ProtoMember(3)] string TruckLoadId);


    [ProtoContract(SkipConstructor = true)]
    public record StoredToWarehouseEvent(
        [property: ProtoMember(1)] string StreamId,
        [property: ProtoMember(2)] string TruckLoadId,
        [property: ProtoMember(3)] string WareHouseId);
}
