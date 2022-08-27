using Forester.Domain.Streams.Order;
using Forester.Framework.EventStore;
using Forester.Framework.Projections;
using System.Text;

namespace Forester.Domain.Streams.GetCompletedOrders
{
    public record GetCompletedOrdersQuery() : IQuery<GetCompletedOrdersQueryResult>
    {
        public ProjectionMode Mode { get; init; } = ProjectionMode.Stable;
        public DateTime AsAt { get; init; }
        public DateTime AsOf { get; init; }
    }

    public record CompletedOrder(string OrderId, string Report);
    public record GetCompletedOrdersQueryResult(List<CompletedOrder> CompletedOrders)
    {
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(nameof(GetCompletedOrdersQueryResult) + " {");

            foreach (var completedOrder in CompletedOrders)
            {
                sb.AppendLine(completedOrder.ToString());
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    public class CompletedOrdersProjection :
        IProjectionQueryHandler<GetCompletedOrdersQuery, GetCompletedOrdersQueryResult>
    {
        public List<string> AcceptedEventTypes { get; } = new List<string>()
        {
            typeof(OrderCompletedEvent).Name
        };

        private readonly List<CompletedOrder> _completedOrders = new List<CompletedOrder>();

        public void Rehydrate(IEnumerable<CommittedEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event.Payload is OrderCompletedEvent e)
                {
                    _completedOrders.Add(new CompletedOrder(e.OrderId, e.Report));
                }
            }
        }

        public GetCompletedOrdersQueryResult Query(GetCompletedOrdersQuery query)
        {
            return new GetCompletedOrdersQueryResult(_completedOrders);
        }
    }
}
