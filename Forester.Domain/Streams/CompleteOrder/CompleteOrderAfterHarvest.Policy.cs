using Forester.Domain.Streams.GetCompletedOrders;
using Forester.Domain.Streams.Log;
using Forester.Domain.Streams.LogLocations;
using Forester.Domain.Streams.Order;
using Forester.Framework.Aggregates;
using Forester.Framework.EventStore;
using Forester.Framework.Policies;

namespace Forester.Domain.Streams.CompleteOrder
{
    public class CompleteOrderAfterHarvestPolicy : IPolicy
    {
        private readonly LogMovementTracker _logMovementTracker = new();
        private readonly TreeTracker _treeTracker = new();
        private readonly Dictionary<string, (string site, bool isHarvested, bool isCompleted)> _orders = new();

        public List<string> AcceptedEventTypes { get; } = new List<string>()
        {
            typeof(OrderIssuedEvent).Name,
            typeof(TreeIdentifiedEvent).Name,
            typeof(LogCutToLengthEvent).Name,
            typeof(BunchPickedByForwarderEvent).Name,
            typeof(BunchGroundedByForwarderEvent).Name,
            typeof(BunchLoadedToTruckEvent).Name,
            typeof(StoredToWarehouseEvent).Name,
            typeof(SiteHarvestedEvent).Name,
            typeof(OrderCompletedEvent).Name,
        };

        public IList<ICommand> Trigger(PolicyTrigger query)
        {
            return _orders
                .Where(kvp => kvp.Value.isHarvested)
                .Where(kvp => !kvp.Value.isCompleted)
                .Select(kvp => CompleteOrder(kvp.Key))
                .Select(cmd => (ICommand)cmd)
                .ToList();
        }

        private CompleteOrderCommand CompleteOrder(string orderId)
        {
            var site = _orders[orderId].site;

            var trees = _treeTracker.SiteToTrees.ContainsKey(site) 
                ? _treeTracker.SiteToTrees[site] 
                : new List<string>();
            var logs = trees.SelectMany(tree => _treeTracker.TreesToLogs[tree]);

            var movements = logs.Select(log =>
                (
                    log,
                    tree: _treeTracker.LogsToTrees[log],
                    movements: _logMovementTracker.LogMovements[log]
                )
            ).ToList();

            var reportBuilder = new System.Text.StringBuilder();

            reportBuilder.AppendLine($"Total of {movements.Count} logs:");

            foreach (var log in movements)
            {
                reportBuilder.AppendLine($"- Log {log.log} (Cut from {log.tree}):");
                foreach (var move in log.movements)
                {
                    reportBuilder.AppendLine($"-- {move}");
                }
            }

            return new CompleteOrderCommand(orderId, reportBuilder.ToString());
        }

        public void Rehydrate(IEnumerable<CommittedEvent> events)
        {
            events.OrderBy(e => e.OccurredAt)
                .ToList()
                .ForEach(Rehydrate);

        }

        private void Rehydrate(CommittedEvent @event)
        {
            switch (@event.Payload)
            {
                case OrderIssuedEvent e:
                    Handle(e);
                    return;
                case SiteHarvestedEvent e:
                    Handle(e);
                    return;
                case TreeIdentifiedEvent e:
                    Handle(e);
                    return;
                case LogCutToLengthEvent e:
                    Handle(e);
                    return;
                case BunchPickedByForwarderEvent e:
                    Handle(e);
                    return;
                case BunchGroundedByForwarderEvent e:
                    Handle(e);
                    return;
                case BunchLoadedToTruckEvent e:
                    Handle(e);
                    return;
                case StoredToWarehouseEvent e:
                    Handle(e);
                    return;
                case OrderCompletedEvent e:
                    Handle(e);
                    return;
                default:
                    return;
            }
        }

        private void Handle(OrderCompletedEvent e)
        {
            _orders[e.OrderId] = (_orders[e.OrderId].site, true, true);
        }

        private void Handle(OrderIssuedEvent e)
        {
            _orders.Add(e.OrderId, (e.SiteId, false, false));
        }

        private void Handle(SiteHarvestedEvent e)
        {
            _orders[e.OrderId] = (_orders[e.OrderId].site, true, false);
        }

        private void Handle(TreeIdentifiedEvent e)
        {
            _treeTracker.Identify(e.TreeId, e.SiteId);
        }

        private void Handle(BunchLoadedToTruckEvent e)
        {
            _logMovementTracker.LoadToTruck(e.From.Latitude, e.From.Longitude, e.TruckLoadId);
        }

        private void Handle(StoredToWarehouseEvent e)
        {
            _logMovementTracker.UnloadToWarehouse(e.TruckLoadId, e.WareHouseId);
        }

        private void Handle(BunchGroundedByForwarderEvent e)
        {
            _logMovementTracker.Ground(e.To.Latitude, e.To.Longitude, e.By);
        }

        private void Handle(BunchPickedByForwarderEvent e)
        {
            _logMovementTracker.Pick(e.From.Latitude, e.From.Longitude, e.By);
        }

        private void Handle(LogCutToLengthEvent e)
        {
            _treeTracker.CutToLength(e.TreeId, e.LogId);
            _logMovementTracker.CutToLength(e.LogId, e.DroppedTo.Latitude, e.DroppedTo.Longitude);
        }

    }
}
