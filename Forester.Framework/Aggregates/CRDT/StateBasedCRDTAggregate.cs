using Forester.Framework.Aggregates;
using Forester.Framework.EventStore;

namespace Forester.Framework.Aggregates.CRDT
{
    public interface IStateBasedCRDTState<TState> where TState : class, IStateBasedCRDTState<TState>
    {
        TState Merge(TState state);
        TState When(object @event);
    }

    public record StateBasedCRDTEvent<TState>(TState State) where TState : class;

    public abstract class StateBasedCRDTAggregate<TState> : AggregateBase where TState : class, IStateBasedCRDTState<TState>, new()
    {
        protected TState? State { get; set; }

        protected StateBasedCRDTAggregate(string streamId) : base(streamId)
        {

        }

        public override void Rehydrate(IList<CommittedEvent> events)
        {
            var state = State ?? throw new ArgumentException("State not set before rehydration");

            foreach (var commitEvent in events)
            {
                state = State.When(commitEvent);
            }
        }

        private void RecursiveRehydration(IList<CommittedEvent> events)
        {
            var forwardConnections = new Dictionary<Guid, List<Guid>>();
            var pairWise = events.Zip(events.Skip(1), (first, second) => (first, second));

            foreach (var pair in pairWise)
            {
                if (pair.second.StreamVersion.ComparedTo(pair.first.StreamVersion) == VersionVector.Comparison.IsNewer)
                {
                    if (!forwardConnections.ContainsKey(pair.first.EventId))
                        forwardConnections.Add(pair.first.EventId, new List<Guid>());
                    forwardConnections[pair.first.EventId].Add(pair.second.EventId);
                }
            }
        }

        private void BuildEventGraph(IList<CommittedEvent> events)
        {
            var eventIndex = events.ToDictionary(e => e.EventId, e => e);

            // First read stream with sliding window of 2. 
            var pairWise = events.Zip(events.Skip(1), (first, second) => (first, second));
            // Concurrency happens when the two version vectors are concurrent. This does not yet define where branching started, as first is last event in the local concurrent branch, and second is the first event in the remote branch
            var concurrencyPoints = pairWise.Where(p => p.first.StreamVersion.ComparedTo(p.second.StreamVersion) == VersionVector.Comparison.AreConcurrent);
            // For every branching event from remote find first concurrent local.
            var branches = concurrencyPoints.Select(p => (local: events.First(e => e.StreamVersion.ComparedTo(p.second.StreamVersion) == VersionVector.Comparison.AreConcurrent), remote: p.second));
            // For every branching point find first (if exists) event that is not concurrent to either of them.
            var branchCloses = branches.Select(p => (p.local, p.remote, closing: events.FirstOrDefault(e => ThatClosesBranch(e, p.local, p.remote))));

            var nextBranch = branchCloses.FirstOrDefault();

            var enumerator = events.GetEnumerator();

            foreach (var @event in events)
            {

            }
        }
        /*
        private TState Travel(TState state, IList<CommittedEvent> upcomingEvents)
        {
            if (upcomingEvents.Count == 0)
                return state;

            var head = upcomingEvents.First();
            state = state.When(head);

            if (upcomingEvents.Count == 1)
                return state;

            var tail = upcomingEvents.Skip(1);
            var next = tail.First();

            var comparison = head.StreamVersion.ComparedTo(next.StreamVersion);

            switch (comparison)
            {
                case VersionVector.Comparison.AreSame:
                    throw new InvalidOperationException("No two versions should ever happen within a stream.");
                case VersionVector.Comparison.AreConcurrent:

            }

            if (head.StreamVersion.ComparedTo(next.StreamVersion) == VersionVector.Comparison.AreConcurrent)
            {

            } 
            else
            {

            }
        }*/

        protected void Handle(Func<TState, TState> action)
        {

        }

        private bool ThatClosesBranch(CommittedEvent toCompare, CommittedEvent local, CommittedEvent remote)
        {
            return toCompare.StreamVersion.ComparedTo(local.StreamVersion) == VersionVector.Comparison.IsNewer
                && toCompare.StreamVersion.ComparedTo(remote.StreamVersion) == VersionVector.Comparison.IsNewer;
        }
    }

    internal class ListTraveler<T>
    {
        private readonly IReadOnlyList<T> _list;
        private int position = 0;

        public ListTraveler(IReadOnlyList<T> list)
        {
            _list = list;
        }

        public T Next()
        {
            position++;
            return _list[position];
        }
    }

}