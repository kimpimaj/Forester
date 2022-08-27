namespace Forester.Framework.Aggregates
{
    internal interface IRehydrator { }
    internal interface IRehydrator<TAggregate> : IRehydrator where TAggregate : IAggregate
    {
        void RehydrateAndExecute<TAggregateHandler, TCommand>(TAggregateHandler aggregate, TCommand command)
            where TCommand : ICommand
            where TAggregateHandler : TAggregate, ICommandHandler<TCommand>;
    }
}