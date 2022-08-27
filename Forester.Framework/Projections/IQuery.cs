namespace Forester.Framework.Projections
{
    public interface IQuery
    {
        ProjectionMode Mode { get; }
        DateTime AsAt { get; init; }
        DateTime AsOf { get; init; }
    }

    public interface IQuery<TResult> : IQuery
    {

    }
}
