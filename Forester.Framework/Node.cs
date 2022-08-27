using Forester.Framework.Aggregates;
using Forester.Framework.EventStore;
using Forester.Framework.Policies;
using Forester.Framework.Projections;

namespace Forester.Framework
{
    public interface INode
    {
        void Handle<TCommand>(TCommand command) where TCommand : ICommand;
        TResult Query<TResult>(IQuery<TResult> query);
        IEnumerable<string> Synchronize(string remoteNode);
        IList<Exception> TriggerPolicyChecks();
    }

    internal class Node : INode
    {
        private readonly AggregateCatalog _aggregateCatalog;
        private readonly QueryHandlerCatalog _projectionCatalog;
        private readonly NodeCatalog _nodeCatalog;
        private readonly PolicyCatalog _policyCatalog;

        public IEventStoreClient EventStore { get; }

        public Node(
            AggregateCatalog aggregateCatalog, 
            IEventStoreClient eventStore, 
            QueryHandlerCatalog projectionCatalog,
            NodeCatalog nodeCatalog,
            PolicyCatalog policyCatalog)
        {
            _aggregateCatalog = aggregateCatalog;
            _projectionCatalog = projectionCatalog;
            _nodeCatalog = nodeCatalog;
            _policyCatalog = policyCatalog;
            EventStore = eventStore;
        }

        public void Handle<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (_aggregateCatalog.IsSupported(command))
            {
                _aggregateCatalog.Forward(command);
                return;
            }

            throw new NotImplementedException($"Command {command.GetType().Name} is not registered.");
        }

        public TResult Query<TResult>(IQuery<TResult> query)
        {
            if (_projectionCatalog.IsSupported<TResult>(query))
            {
                return _projectionCatalog.Forward<TResult>(query);
            }

            throw new NotImplementedException($"Query {query.GetType().Name} is not registered.");
        }

        public IList<Exception> TriggerPolicyChecks() 
        {
            var results = new List<Exception>();

            var commandsGenerated = _policyCatalog.Trigger();
            foreach (var command in commandsGenerated)
            {
                if (_aggregateCatalog.IsSupported(command))
                {
                    try
                    {
                        _aggregateCatalog.Forward(command);
                    } 
                    catch (Exception ex)
                    {
                        results.Add(ex);
                    }
                }
            }

            return results;
        }

        public IEnumerable<string> Synchronize(string remoteNode)
        {
            var remote = _nodeCatalog.AddressBook[remoteNode];
            return EventStore.Synchronize(remote);
        }

        public VersionVector GetCurrentStableVersion()
        {
            return EventStore.GetCurrentStableVersion();
        }
    }
}