using RealmNexus.Logging;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class ItemHandler(RealmClient client, ILogger logger) : PacketHandlerBase<SyncItem>(client, logger)
{
    private const short MaxItem = 401;
    private readonly bool[] _activeItem = new bool[MaxItem];

    protected override void HandleS2C(SyncItem sync, PacketInterceptArgs args)
    {
        _activeItem[sync.ItemSlot] = sync.ItemType != 0;
    }

    public override void OnServerChanging()
    {
        for (short i = 0; i < MaxItem; ++i)
        {
            if (_activeItem[i])
            {
                // 通知客户端移除物品
                _ = Client.SendPacketToClientAsync(new SyncItem
                {
                    ItemSlot = i,
                    ItemType = 0
                });
                _activeItem[i] = false;
            }
        }
    }
}
