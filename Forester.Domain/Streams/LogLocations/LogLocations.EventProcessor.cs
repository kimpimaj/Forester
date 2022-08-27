using Forester.Framework.Aggregates;

namespace Forester.Domain.Streams.LogLocations
{
    public class LogLocationsEventProcessorRepository : IAggregateRepository<LogLocationsEventProcessor>
    {
        public LogLocationsEventProcessor Get(string streamId)
        {
            return new LogLocationsEventProcessor(streamId);
        }
    }

    public class LogLocationsEventProcessor : AggregateBase,
        ILogLocationsCommandHandler,
        ILogLocationsEventHandler
    {
        public LogLocationsEventProcessor(string streamId) : base(streamId)
        {

        }

        public void Handle(PickBunchCommand c)
        {
            RaiseEvent(this, new BunchPickedByForwarderEvent(c.StreamId, new Location(c.FromLatitude, c.FromLongitude), c.By));
        }

        public void Handle(GroundBunchCommand c)
        {
            RaiseEvent(this, new BunchGroundedByForwarderEvent(c.StreamId, c.By, new Location(c.FromLatitude, c.FromLongitude)));
        }

        public void Handle(LoadBunchToTruckCommand c)
        {
            RaiseEvent(this, new BunchLoadedToTruckEvent(c.StreamId, new Location(c.FromLatitude, c.FromLongitude), c.TruckLoadId));
        }

        public void Handle(StoreToWarehouseCommand c)
        {
            RaiseEvent(this, new StoredToWarehouseEvent(c.StreamId, c.TruckLoadId, c.WarehouseSlot));
        }

        public void When(BunchPickedByForwarderEvent @event)
        {
            // This event processor is stateless
        }

        public void When(BunchGroundedByForwarderEvent @event)
        {
            // This event processor is stateless
        }

        public void When(BunchLoadedToTruckEvent @event)
        {
            // This event processor is stateless
        }

        public void When(StoredToWarehouseEvent @event)
        {
            // This event processor is stateless
        }
    }
}
