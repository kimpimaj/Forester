namespace Forester.Framework.Aggregates
{
    public delegate IAggregateRepository<TAggregate> CreateRepository<TAggregate>(INodeContext context)
        where TAggregate : IAggregate;

    internal interface IAggregateCommandRegistry
    {
        void Forward<TCommand>(TCommand command) where TCommand : ICommand;
    }

    public interface IAggregateCommandRegisterer<TAggregate>
        where TAggregate : IAggregate
    {
        IAggregateCommandRegisterer<TAggregate> RegisterCommand<TCommand, TCmdAggregate>()
            where TCmdAggregate : TAggregate, ICommandHandler<TCommand>
            where TCommand : ICommand;
    }

    internal class CommandRegistry<TAggregate> : IAggregateCommandRegisterer<TAggregate>,
        IAggregateCommandRegistry
        where TAggregate : IAggregate
    {
        public IReadOnlyList<Type> RegisteredCommands => _commandToForwarderMap.Keys.ToList();

        private readonly CreateRepository<TAggregate> _createRepository;
        private readonly IRehydrator<TAggregate> _rehydrator;
        private readonly INodeContext _context;
        private readonly Dictionary<Type, Action<ICommand>> _commandToForwarderMap = new();

        public CommandRegistry(
            CreateRepository<TAggregate> repository,
            IRehydrator<TAggregate> rehydrator,
            INodeContext context)
        {
            _createRepository = repository;
            _rehydrator = rehydrator;
            _context = context;
        }

        public IAggregateCommandRegisterer<TAggregate> RegisterCommand<TCommand, TCmdAggregate>()
            where TCommand : ICommand
            where TCmdAggregate : ICommandHandler<TCommand>, TAggregate
        {
            Action<ICommand> forwarder = (command) =>
            {
                var repository = _createRepository(_context);
                var aggregate = (TCmdAggregate)repository.Get(command.StreamId);
                _rehydrator.RehydrateAndExecute(aggregate, (TCommand)command);
            };

            _commandToForwarderMap.Add(typeof(TCommand), forwarder);

            return this;
        }

        public void Forward<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            _commandToForwarderMap[command.GetType()](command);
        }
    }

    internal class AggregateCatalog
    {
        private readonly Dictionary<Type, IAggregateCommandRegistry> _aggregateToRegistryMap = new Dictionary<Type, IAggregateCommandRegistry>();
        private readonly Dictionary<Type, Type> _commandToAggregateMap = new Dictionary<Type, Type>();

        public void RegisterAggregate<TAggregate>(
            CreateRepository<TAggregate> repository,
            Action<IAggregateCommandRegisterer<TAggregate>> registerCommands,
            IRehydrator<TAggregate> _rehydrator,
            INodeContext context)
            where TAggregate : IAggregate
        {
            var commandRegistry = new CommandRegistry<TAggregate>(repository, _rehydrator, context);
            registerCommands(commandRegistry);

            _aggregateToRegistryMap.Add(typeof(TAggregate), commandRegistry);
            foreach (var registeredCommand in commandRegistry.RegisteredCommands)
            {
                _commandToAggregateMap.Add(registeredCommand, typeof(TAggregate));
            }
        }

        public bool IsSupported<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            return _commandToAggregateMap.ContainsKey(command.GetType());
        }

        public void Forward<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var aggregateType = _commandToAggregateMap[command.GetType()];
            var registry = _aggregateToRegistryMap[aggregateType];
            registry.Forward(command);
        }
    }
}