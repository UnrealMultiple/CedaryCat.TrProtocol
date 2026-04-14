using RealmNexus.Logging;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class ProjectileHandler(RealmClient client, ILogger logger) : PacketHandlerBase<SyncProjectile>(client, logger)
{
    private const short MaxProjectile = 1000;
    private readonly short[] _projOwner = [.. Enumerable.Repeat((short)-1, MaxProjectile)];

    protected override void HandleC2S(SyncProjectile sync, PacketInterceptArgs args)
    {
        _projOwner[sync.ProjSlot] = sync.PlayerSlot;
    }

    public override void OnS2C(PacketInterceptArgs args)
    {
        if (args.Packet is KillProjectile kill && _projOwner[kill.ProjSlot] == kill.PlayerSlot)
            _projOwner[kill.ProjSlot] = -1;
    }

    public override void OnServerChanging()
    {
        for (short i = 0; i < MaxProjectile; ++i)
        {
            if (_projOwner[i] != -1)
            {
                // 通知客户端移除弹幕
                _ = Client.SendPacketToClientAsync(new KillProjectile
                {
                    PlayerSlot = (byte)_projOwner[i],
                    ProjSlot = i
                });
                _projOwner[i] = -1;
            }
        }
    }
}
