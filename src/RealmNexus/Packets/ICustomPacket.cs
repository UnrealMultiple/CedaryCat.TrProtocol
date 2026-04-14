using TrProtocol;
using TrProtocol.Interfaces;

namespace RealmNexus.Packets;

public interface ICustomPacket : IBinarySerializable
{
    MessageID Type { get; }
}

public static class CustomPacketRegistry
{
    private static readonly Dictionary<MessageID, Func<ICustomPacket>> _factories = [];

    public static void Register<T>(MessageID type) where T : ICustomPacket, new()
    {
        _factories[type] = () => new T();
    }

    public static bool TryCreate(MessageID type, out ICustomPacket packet)
    {
        if (_factories.TryGetValue(type, out var factory))
        {
            packet = factory();
            return true;
        }
        packet = null!;
        return false;
    }

    public static bool IsCustomPacket(MessageID type)
    {
        return _factories.ContainsKey(type);
    }

    public static bool IsCustomPacket(byte typeId)
    {
        return Enum.IsDefined(typeof(MessageID), typeId) &&
               _factories.ContainsKey((MessageID)typeId);
    }
}
