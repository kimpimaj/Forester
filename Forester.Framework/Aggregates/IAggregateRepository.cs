namespace Forester.Framework.Aggregates
{
    public interface IAggregateRepository
    {
        IAggregate Get(string streamId);
    }

    public interface IAggregateRepository<out TAggregate> : IAggregateRepository
        where TAggregate : IAggregate
    {
        new TAggregate Get(string streamId);
        IAggregate IAggregateRepository.Get(string streamId) => Get(streamId);
    }
}