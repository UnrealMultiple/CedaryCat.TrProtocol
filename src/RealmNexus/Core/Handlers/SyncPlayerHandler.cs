using RealmNexus.Logging;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class SyncPlayerHandler(RealmClient client, ILogger logger) : PacketHandlerBase<SyncPlayer>(client, logger)
{
    private SyncPlayer? _savedSyncPlayer;

    protected override void HandleC2S(SyncPlayer packet, PacketInterceptArgs args)
    {
        if (_savedSyncPlayer.HasValue && !string.IsNullOrEmpty(_savedSyncPlayer.Value.Name) 
            && _savedSyncPlayer.Value.Name != packet.Name)
        {
            Logger.LogWarning("SyncPlayer", $"[{Client.Endpoint}] 禁止修改名字: {_savedSyncPlayer.Value.Name} -> {packet.Name}");
            _ = Client.DisconnectAsync("禁止修改名字");
            args.Handled = true;
            return;
        }

        _savedSyncPlayer = packet;
        
        if (!string.IsNullOrEmpty(packet.Name))
        {
            Client.PlayerName = packet.Name;
        }
    }

    public override void OnServerChanging()
    {
        Logger.LogInfo("SyncPlayer", $"玩家 {_savedSyncPlayer?.Name} 切换服务器");
        Client.PlayerName = "Unknown";
    }

    public SyncPlayer? GetSyncPlayer() => _savedSyncPlayer;
    public bool HasSyncPlayer => _savedSyncPlayer.HasValue;
    public string PlayerName => _savedSyncPlayer?.Name ?? "Unknown";
    public byte PlayerSlot => _savedSyncPlayer?.PlayerSlot ?? 0;
}
