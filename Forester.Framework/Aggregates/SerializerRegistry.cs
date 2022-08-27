using Forester.Framework.EventStore;

namespace Forester.Framework.Aggregates
{
    public class SerializerRegistry : ISerializerRegistry
    {
        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        public ISerializer GetSerializer(string type)
        {
            return _serializers[type];
        }

        public void Register<TEvent>(ISerializer<TEvent> serializer)
        {
            _serializers[serializer.SerializedTypeName()] = new SerializerDecorator<TEvent>(serializer);
        }
    }

    internal class SerializerDecorator<TEvent> : ISerializer
    {
        private readonly ISerializer<TEvent> _serializer;

        public SerializerDecorator(ISerializer<TEvent> serializer)
        {
            _serializer = serializer;
        }

        public byte[] Serialize(object @event)
        {
            return _serializer.Serialize((TEvent)@event);
        }

        public object Deserialize(byte[] data)
        {
            return _serializer.Deserialize(data);
        }
    }
}
