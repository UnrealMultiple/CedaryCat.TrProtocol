using RealmNexus.Logging;
using Terraria.DataStructures;
using Terraria.GameContent;
using TrProtocol.NetPackets.Modules;

namespace RealmNexus.Core.Handlers;

public class PylonHandler(RealmClient client, ILogger logger) : PacketHandlerBase<NetTeleportPylonModule>(client, logger)
{
    private const byte maxPylon = 9;
    private readonly Point16[] activePylon = [.. new Point16[maxPylon].Select(x => new Point16(-1, -1))];

    protected override void HandleC2S(NetTeleportPylonModule packet, PacketInterceptArgs args)
    {
        if (packet.PylonPacketType == Terraria.GameContent.NetModules.NetTeleportPylonModule.SubPacketType.PylonWasAdded)
            activePylon[(int)packet.PylonType] = packet.Position;
        else
            activePylon[(int)packet.PylonType] = new Point16(-1, -1);
    }

    protected override void HandleS2C(NetTeleportPylonModule packet, PacketInterceptArgs args)
    {
        if (packet.PylonPacketType == Terraria.GameContent.NetModules.NetTeleportPylonModule.SubPacketType.PylonWasAdded)
            activePylon[(int)packet.PylonType] = packet.Position;
        else
            activePylon[(int)packet.PylonType] = new Point16(-1, -1);
    }

    public override void OnServerChanged()
    {
        for (short i = 0; i < maxPylon; ++i)
        {
            if (activePylon[i].X >= 0 && activePylon[i].Y >= 0)
            {
                _= Client.SendPacketToClientAsync(new NetTeleportPylonModule
                {
                    PylonPacketType = Terraria.GameContent.NetModules.NetTeleportPylonModule.SubPacketType.PylonWasRemoved,
                    PylonType = (TeleportPylonType)i,
                    Position = activePylon[i]
                });
                activePylon[i] = new Point16(-1, -1);
            }
        }
    }
}
