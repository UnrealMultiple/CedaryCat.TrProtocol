using RealmNexus.Logging;
using TrProtocol;

namespace RealmNexus.Core;

public abstract class PacketHandlerBase(RealmClient client, ILogger logger) : IPacketHandler
{
    protected readonly RealmClient Client = client;
    protected readonly ILogger Logger = logger;

    public virtual void OnC2S(PacketInterceptArgs args) { }
    public virtual void OnS2C(PacketInterceptArgs args) { }

    public virtual void OnConnected() { }

    public virtual void OnDisconnected() { }

    public virtual void OnServerChanging() { }

    public virtual void OnServerChanged() { }
}

public abstract class PacketHandlerBase<T>(RealmClient client, ILogger logger) : PacketHandlerBase(client, logger) where T : INetPacket
{
    public override void OnC2S(PacketInterceptArgs args)
    {
        if (args.Packet is T packet)
        {
            HandleC2S(packet, args);
        }
    }

    public override void OnS2C(PacketInterceptArgs args)
    {
        if (args.Packet is T packet)
        {
            HandleS2C(packet, args);
        }
    }

    protected virtual void HandleC2S(T packet, PacketInterceptArgs args) { }
    protected virtual void HandleS2C(T packet, PacketInterceptArgs args) { }
}
