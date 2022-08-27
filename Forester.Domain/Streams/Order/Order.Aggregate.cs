using Forester.Framework.Aggregates;
using Forester.Framework.Aggregates.DynamicallyOwned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Domain.Streams.Order
{
    public class OrderAggregateRepository : IAggregateRepository<OrderAggregate>
    {
        private readonly string _nodeId;

        public OrderAggregateRepository(string nodeId)
        {
            _nodeId = nodeId;
        }

        public OrderAggregate Get(string streamId)
        {
            return new OrderAggregate(streamId, _nodeId);
        }
    }

    public class OrderAggregate : DynamicallyOwnedAggregate<OrderIssuedEvent>,
        ICommandHandler<IssueOrderCommand>,
        ICommandHandler<AssignToHarvesterCommand>,
        ICommandHandler<ReportOrderSiteHarvestedCommand>,
        ICommandHandler<CompleteOrderCommand>,
        IEventHandler<OrderIssuedEvent>,
        IEventHandler<SiteHarvestedEvent>,
        IEventHandler<AssignedToHarvesterEvent>,
        IEventHandler<OrderCompletedEvent>
    {
        public enum OrderState
        {
            None = 0,
            Issued = 1,
            InHarvest = 2,
            Harvested = 3,
            Completed = 4
        }

        private OrderState _state = OrderState.None;

        public OrderAggregate(string streamId, string node) : base(streamId, node)
        {

        }

        public void Handle(IssueOrderCommand command)
        {
            if (_state != OrderState.None)
                throw new Exception("Order already issued");

            RaiseCreationEvent(this, new OrderIssuedEvent(command.OrderId, command.SiteId, command.SalesPerson));
        }

        public void Handle(AssignToHarvesterCommand command)
        {
            if (_state == OrderState.None)
                throw new Exception("Order is not yet issued");

            if (_state == OrderState.Harvested)
                throw new Exception("Order already harvested");

            if (_state == OrderState.Completed)
                throw new Exception("Order already completed");

            RaiseEvent(this, new AssignedToHarvesterEvent(command.Harvester));
            base.Handle(new TransferOwnershipCommand(command.StreamId, command.Harvester));
        }

        public void Handle(ReportOrderSiteHarvestedCommand command)
        {
            if (_state != OrderState.InHarvest)
                throw new Exception("Order is not in harvest");

            RaiseEvent(this, new SiteHarvestedEvent(command.SalesPerson, command.OrderId));
            base.Handle(new TransferOwnershipCommand(StreamId, command.SalesPerson));
        }

        public void Handle(CompleteOrderCommand command)
        {
            if (_state != OrderState.Harvested)
                throw new Exception("Order is not harvested");

            RaiseEvent(this, new OrderCompletedEvent(command.OrderId, command.Report));
        }


        public void When(OrderIssuedEvent @event)
        {
            _state = OrderState.Issued;
        }

        public void When(AssignedToHarvesterEvent @event)
        {
            _state = OrderState.InHarvest;
        }

        public void When(SiteHarvestedEvent @event)
        {
            _state = OrderState.Harvested;
        }

        public void When(OrderCompletedEvent @event)
        {
            _state = OrderState.Completed;
        }

    }
}
