using Forester.Framework.Aggregates;
using Forester.Framework.Aggregates.DynamicallyOwned;
using Forester.Framework.EventStore;
using Forester.Framework.Policies;
using Forester.Framework.Projections;

namespace Forester.Framework
{
    public interface INodeBuilder
    {
        INodeBuilder AddThisNode(string name, string address);
        INodeBuilder AddKnownNode(string name, string address);
        INodeBuilder UseEventstore(Func<INodeContext, string, IEventStoreClient> getEventStore);
        INodeBuilder RegisterSimpleAggregate<TAggregate>(
            CreateRepository<TAggregate> repository,
            Action<IAggregateCommandRegisterer<TAggregate>> registerCommands
        ) where TAggregate : IAggregate;

        INodeBuilder RegisterDynamicallyOwnedAggregate<TAggregate>(
            CreateRepository<TAggregate> repository,
            Action<IAggregateCommandRegisterer<TAggregate>> registerCommands
        ) where TAggregate : IDynamicallyOwnerAggregate, IAggregate;

        INodeBuilder RegisterSerializer<TEvent>(ISerializer<TEvent> serializer);
        INodeBuilder RegisterProjectionQueryHandler<TProjectionQueryHandler, TQuery, TResult>()
            where TProjectionQueryHandler : IProjectionQueryHandler<TQuery, TResult>, new()
            where TQuery : IQuery<TResult>;

        INodeBuilder RegisterPolicy<TProjectionQueryHandler>()
            where TProjectionQueryHandler : IPolicy, new();
        INode Build();
    }

    public interface INodeContext
    {
        string NodeName { get; }
        ISerializerRegistry SerializerRegistry { get; }
    }

    internal class NodeCatalog
    {
        public Dictionary<string, string> AddressBook { get; }
        public Dictionary<string, IEventStoreClient> EventStoreClients { get; }

        public NodeCatalog()
        {
            AddressBook = new();
            EventStoreClients = new();
        }
    }

    internal class NodeContext : INodeContext
    {
        public string NodeName { get; internal set; }
        public List<string> Nodes { get; internal set; } = new List<string>();
        public IEventStoreClient EventStore { get; internal set; }
        public AggregateCatalog AggregateCatalog { get; }
        public QueryHandlerCatalog ProjectionCatalog { get; }
        public SerializerRegistry SerializerRegistry { get; }
        public NodeCatalog NodeCatalog { get; }
        public PolicyCatalog PolicyCatalog { get; }

        ISerializerRegistry INodeContext.SerializerRegistry => SerializerRegistry;

        public NodeContext(
            AggregateCatalog aggregateCatalog, 
            QueryHandlerCatalog projectionCatalog, 
            SerializerRegistry serializerRegistry, 
            PolicyCatalog policyCatalog)
        {
            AggregateCatalog = aggregateCatalog;
            ProjectionCatalog = projectionCatalog;
            SerializerRegistry = serializerRegistry;
            PolicyCatalog = policyCatalog;
            NodeCatalog = new();
        }
    }

    public class NodeBuilder : INodeBuilder
    {
        private NodeContext _nodeContext;

        private NodeBuilder()
        {
            _nodeContext = new NodeContext(
                new AggregateCatalog(),
                new QueryHandlerCatalog(),
                new SerializerRegistry(),
                new PolicyCatalog());
        }

        public static INodeBuilder Builder()
        {
            return new NodeBuilder();
        }

        public INodeBuilder AddThisNode(string name, string address)
        {
            _nodeContext.NodeName = name;
            _nodeContext.Nodes.Add(name);
            _nodeContext.NodeCatalog.AddressBook.Add(name, address);
            return this;
        }

        public INodeBuilder AddKnownNode(string name, string address)
        {
            _nodeContext.Nodes.Add(name);
            _nodeContext.NodeCatalog.AddressBook.Add(name, address);
            return this;
        }

        public INodeBuilder UseEventstore(Func<INodeContext, string, IEventStoreClient> getEventStore)
        {
            var thisNode = _nodeContext.NodeName;
            var address = _nodeContext.NodeCatalog.AddressBook[thisNode];

            _nodeContext.EventStore = getEventStore(_nodeContext, address);

            return this;
        }

        public INodeBuilder RegisterSimpleAggregate<TAggregate>(CreateRepository<TAggregate> repository, Action<IAggregateCommandRegisterer<TAggregate>> registerCommands)
            where TAggregate : IAggregate
        {
            _nodeContext.AggregateCatalog.RegisterAggregate<TAggregate>(
                repository,
                registerCommands,
                new SimpleRehydrator<TAggregate>(_nodeContext.EventStore),
                _nodeContext);
            return this;
        }

        public INodeBuilder RegisterDynamicallyOwnedAggregate<TAggregate>(
            CreateRepository<TAggregate> repository,
            Action<IAggregateCommandRegisterer<TAggregate>> registerCommands) where TAggregate : IDynamicallyOwnerAggregate, IAggregate
        {
            _nodeContext.AggregateCatalog.RegisterAggregate<TAggregate>(
                repository,
                registry => {
                    registry.RegisterCommand<TransferOwnershipCommand, TAggregate>();
                    registerCommands(registry);
                },
                new SimpleRehydrator<TAggregate>(_nodeContext.EventStore),
                _nodeContext);
            return this;
        }

        public INode Build()
        {
            _nodeContext.EventStore.Setup(_nodeContext.Nodes);
            return new Node(
                _nodeContext.AggregateCatalog, 
                _nodeContext.EventStore, 
                _nodeContext.ProjectionCatalog, 
                _nodeContext.NodeCatalog,
                _nodeContext.PolicyCatalog);
        }

        public INodeBuilder RegisterSerializer<TEvent>(ISerializer<TEvent> serializer)
        {
            _nodeContext.SerializerRegistry.Register(serializer);
            return this;
        }

        public INodeBuilder RegisterProjectionQueryHandler<TProjectionQueryHandler, TQuery, TResult>()
            where TProjectionQueryHandler : IProjectionQueryHandler<TQuery, TResult>, new ()
            where TQuery : IQuery<TResult>
        {
            _nodeContext.ProjectionCatalog.RegisterProjectionQueryHandler<TProjectionQueryHandler, TQuery, TResult>(new ProjectionQueryRehydrator<TProjectionQueryHandler>(_nodeContext.EventStore));
            return this;
        }

        public INodeBuilder RegisterPolicy<TProjectionQueryHandler>()
            where TProjectionQueryHandler : IPolicy, new()
        {
            _nodeContext.PolicyCatalog.RegisterPolicy<TProjectionQueryHandler>(new PolicyRehydrator(_nodeContext.EventStore));
            return this;
        }
    }
}