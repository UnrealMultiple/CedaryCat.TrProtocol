using RealmNexus.Logging;
using RealmNexus.Packets;

namespace RealmNexus.Core;

public abstract class CustomPacketHandlerBase<T>(RealmClient client, ILogger logger) : PacketHandlerBase(client, logger)
    where T : ICustomPacket
{
    public override void OnC2S(PacketInterceptArgs args)
    {
        if (args.CustomPacket is T packet)
        {
            HandleC2S(packet, args);
        }
    }

    public override void OnS2C(PacketInterceptArgs args)
    {
        if (args.CustomPacket is T packet)
        {
            HandleS2C(packet, args);
        }
    }

    protected virtual void HandleC2S(T packet, PacketInterceptArgs args) { }
    protected virtual void HandleS2C(T packet, PacketInterceptArgs args) { }
}
