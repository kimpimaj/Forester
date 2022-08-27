namespace Forester.Framework.Aggregates
{
    public interface IEventHandler<TEventPayload>
    {
        void When(TEventPayload @event);
    }

}