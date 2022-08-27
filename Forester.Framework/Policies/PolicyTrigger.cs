using Forester.Framework.Projections;

namespace Forester.Framework.Policies
{
    public record PolicyTrigger
    {
        public ProjectionMode Mode => ProjectionMode.Stable;

        public DateTime AsAt => DateTime.UtcNow.AddYears(10);

        public DateTime AsOf => DateTime.UtcNow.AddYears(10);
    }
}
