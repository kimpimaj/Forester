using Forester.Framework.Aggregates;

namespace Forester.Domain.Streams.Order
{
    public record IssueOrderCommand(string OrderId, string SiteId, string SalesPerson) : ICommand
    {
        public string StreamId => OrderId;
    }

    public record AssignToHarvesterCommand(string OrderId, string Harvester) : ICommand
    {
        public string StreamId => OrderId;
    }

    public record ReportOrderSiteHarvestedCommand(string OrderId, string SalesPerson) : ICommand
    {
        public string StreamId => OrderId;
    }

    public record CompleteOrderCommand(string OrderId, string Report) : ICommand
    {
        public string StreamId => OrderId;
    }
}
