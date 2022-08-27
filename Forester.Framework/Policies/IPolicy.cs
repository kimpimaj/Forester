using Forester.Framework.Aggregates;
using Forester.Framework.EventStore;

namespace Forester.Framework.Policies
{
    public interface IPolicy
    {
        List<string> AcceptedEventTypes { get; }
        void Rehydrate(IEnumerable<CommittedEvent> events);
        IList<ICommand> Trigger(PolicyTrigger trigger);
    }
}
