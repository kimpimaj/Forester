using Forester.Domain.Streams.Order;
using Forester.Framework.EventStore;
using Forester.Framework.Projections;
using System.Text;

namespace Forester.Domain.Streams.GetCompletedOrders
{
    public record GetIssuedOrdersQuery() : IQuery<GetIssuedOrdersQueryResult>
    {
        public ProjectionMode Mode { get; init; } = ProjectionMode.Latest;
        public DateTime AsAt { get; init; }
        public DateTime AsOf { get; init; }
    }

    public record IssuedOrder(string OrderId, string Issuer, DateTime occurredAt, DateTime recordedAt);
    public record GetIssuedOrdersQueryResult(List<IssuedOrder> IssuedOrders)
    {
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(nameof(GetCompletedOrdersQueryResult) + " {");

            foreach (var issuedOrder in IssuedOrders)
            {
                sb.AppendLine(issuedOrder.ToString());
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    public class IssuedOrdersProjection :
        IProjectionQueryHandler<GetIssuedOrdersQuery, GetIssuedOrdersQueryResult>
    {
        public List<string> AcceptedEventTypes { get; } = new List<string>()
        {
            typeof(OrderIssuedEvent).Name
        };

        private readonly List<IssuedOrder> _issuedOrders = new List<IssuedOrder>();

        public void Rehydrate(IEnumerable<CommittedEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event.Payload is OrderIssuedEvent e)
                {
                    _issuedOrders.Add(new IssuedOrder(e.OrderId, e.SalesPerson, @event.OccurredAt, @event.RecordedAt));
                }
            }
        }

        public GetIssuedOrdersQueryResult Query(GetIssuedOrdersQuery query)
        {
            return new GetIssuedOrdersQueryResult(_issuedOrders);
        }
    }
}
