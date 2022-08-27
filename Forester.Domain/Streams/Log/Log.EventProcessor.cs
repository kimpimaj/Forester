using Forester.Framework.Aggregates;
using Forester.Framework.Aggregates.Simple;

namespace Forester.Domain.Streams.Log
{
    public class LogEventProcessorRepository : IAggregateRepository<LogEventProcessor>
    {
        public LogEventProcessor Get(string streamId)
        {
            return new LogEventProcessor(streamId);
        }
    }

    public class LogEventProcessor : SimpleAggregate<TreeIdentifiedEvent>,
        ILogCommandHandler,
        ILogEventHander
    {
        public LogEventProcessor(string streamId) : base(streamId)
        {

        }

        public void Handle(IdentifyTreeCommand command)
        {
            RaiseCreationEvent(this, new TreeIdentifiedEvent(command.StreamId, command.SiteId, new Location(command.Latitude, command.Longitude), command.Species));
        }

        public void Handle(CutDownTreeCommand command)
        {
            RaiseEvent(this, new TreeCutDownEvent(command.StreamId, command.By));
        }

        public void Handle(CutLogToLengthCommand command)
        {
            RaiseEvent(this, new LogCutToLengthEvent(command.StreamId, command.LogId, new Location(command.Latitude, command.Longitude), command.CutLength, command.Volume));
        }

        public void When(TreeIdentifiedEvent @event)
        {
            // This event processor is stateless
        }

        public void When(TreeCutDownEvent @event)
        {
            // This event processor is stateless
        }

        public void When(LogCutToLengthEvent @event)
        {
            // This event processor is stateless
        }

    }
}
