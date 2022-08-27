using Forester.Framework.Aggregates;

namespace Forester.Domain.Streams.LogLocations
{
    public interface ILogLocationsCommandHandler :
        ICommandHandler<PickBunchCommand>,
        ICommandHandler<GroundBunchCommand>,
        ICommandHandler<LoadBunchToTruckCommand>,
        ICommandHandler<StoreToWarehouseCommand>
    { }

    public record PickBunchCommand(string StreamId, string By, int FromLatitude, int FromLongitude) : ICommand;

    public record GroundBunchCommand(string StreamId, string By, int FromLatitude, int FromLongitude) : ICommand;

    public record LoadBunchToTruckCommand(string StreamId, string TruckLoadId, int FromLatitude, int FromLongitude) : ICommand;

    public record StoreToWarehouseCommand(string StreamId, string TruckLoadId, string WarehouseSlot) : ICommand;
}
