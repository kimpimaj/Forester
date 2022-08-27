using Forester.Framework.Aggregates;

namespace Forester.Framework.Aggregates.CRDT
{
    public abstract class OperationBasedCRDTAggregate : AggregateBase
    {
        protected OperationBasedCRDTAggregate(string streamId) : base(streamId)
        {

        }
    }
}