using RealmNexus.Logging;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class NpcHandler(RealmClient client, ILogger logger) : PacketHandlerBase<SyncNPC>(client, logger)
{
    private const short MaxNPC = 200;
    private readonly bool[] _activeNpc = new bool[MaxNPC];

    protected override void HandleS2C(SyncNPC npc, PacketInterceptArgs args)
    {
        _activeNpc[npc.NPCSlot] = npc.ShortHP > 0 || npc.PrettyShortHP > 0 || npc.HP > 0 || npc.Bit1[7];
    }

    public override void OnServerChanging()
    {
        for (short i = 0; i < MaxNPC; ++i)
        {
            if (_activeNpc[i])
            {
                // 通知客户端移除 NPC
                _ = Client.SendPacketToClientAsync(new SyncNPC
                {
                    NPCSlot = i,
                    Bit3 = 1,
                    ExtraData = []
                });
                _activeNpc[i] = false;
            }
        }
    }
}
