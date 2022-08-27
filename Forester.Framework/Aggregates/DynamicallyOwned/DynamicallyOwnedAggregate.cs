using Forester.Framework.EventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Framework.Aggregates.DynamicallyOwned
{
    public record TransferOwnershipCommand(string StreamId, string NewOwner) : ICommand { }
    public record OwnershipTransferredEvent(string StreamId, string NewOwner, string PreviousOwner) { }

    public interface IDynamicallyOwnedCreationEvent : ICreationEvent
    {
        string InitialOwner { get; }
    }

    public interface IDynamicallyOwnerAggregate :
        ICommandHandler<TransferOwnershipCommand>
    {
    }

    public abstract class DynamicallyOwnedAggregate<TCreationEvent> : AggregateBase,
        IDynamicallyOwnerAggregate,
        IEventHandler<OwnershipTransferredEvent>
        where TCreationEvent : IDynamicallyOwnedCreationEvent
    {
        private bool _created;
        protected string NodeName { get; }
        protected string CurrentOwner { get; private set; }

        protected DynamicallyOwnedAggregate(string streamId, string node) : base(streamId)
        {
            NodeName = node;
        }

        public virtual void Handle(TransferOwnershipCommand command)
        {
            if (NodeName != CurrentOwner)
            {
                throw new InvalidOperationException("Only current owner is allowed to move ownership");
            }

            RaiseEvent(this, new OwnershipTransferredEvent(StreamId, command.NewOwner, CurrentOwner));
        }

        public void When(OwnershipTransferredEvent @event)
        {
            CurrentOwner = @event.NewOwner;
        }

        private void When(TCreationEvent @event)
        {
            CurrentOwner = @event.InitialOwner;
        }

        protected override void Rehydrate(object @event)
        {
            if (@event is TCreationEvent e)
            {
                When(e);
            }

            base.Rehydrate(@event);
        }

        protected void RaiseCreationEvent<TEventPayload>(IEventHandler<TEventPayload> handler, TEventPayload @event)
            where TEventPayload : TCreationEvent
        {
            if (_created == true)
            {
                throw new InvalidOperationException("Only one creation event is allowed");
            }

            StreamId = @event.StreamId;
            CurrentOwner = @event.InitialOwner;
            RaiseEventImpl(handler, @event);
        }

        internal override void RaiseEventImpl<TEventPayload>(IEventHandler<TEventPayload> handler, TEventPayload @event)
        {
            if (CurrentOwner != NodeName)
            {
                throw new InvalidOperationException("Only current owner is allowed to raise events");
            }

            base.RaiseEventImpl(handler, @event);
        }
    }
}
