namespace Forester.Framework.EventStore
{
    public interface ISerializerRegistry
    {
        ISerializer GetSerializer(string type);
    }

    public interface ISerializer<TEvent>
    {
        string SerializedTypeName();
        byte[] Serialize(TEvent @event);
        TEvent? Deserialize(byte[] data);
    }

    public interface ISerializer
    {
        byte[] Serialize(object @event);
        object Deserialize(byte[] data);
    }
}
