using Microsoft.Xna.Framework;
using RealmNexus.Logging;
using Terraria;
using Terraria.DataStructures;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class ConnectionHandler(RealmClient client, ILogger logger, SyncPlayerHandler syncPlayerHandler, ClientHelloHandler clientHelloHandler) 
    : PacketHandlerBase(client, logger)
{
    public enum ClientState
    {
        New,
        ReusedConnect1,
        ReusedConnect2,
        Connected,
    }

    private ClientState _state = ClientState.New;
    private Point16 _spawnPosition;
    private readonly SyncPlayerHandler _syncPlayerHandler = syncPlayerHandler;
    private readonly ClientHelloHandler _clientHelloHandler = clientHelloHandler;

    public ClientState State => _state;

    public void SetState(ClientState state)
    {
        _state = state;
    }

    public override void OnS2C(PacketInterceptArgs args)
    {
        switch (args.Packet)
        {
            case WorldData data:
                HandleWorldData(data);
                break;

            case StartPlaying:
                HandleStartPlaying();
                break;

            case LoadPlayer:
                HandleLoadPlayer();
                break;
        }
    }

    private void HandleWorldData(WorldData data)
    {
        if (_state != ClientState.ReusedConnect1) return;

        _spawnPosition = new Point16(data.SpawnX, data.SpawnY);

        var requestTileData = new RequestTileData
        {
            Position = new Point(data.SpawnX, data.SpawnY)
        };
        _ = Client.SendPacketToServerAsync(requestTileData);

        _state = ClientState.ReusedConnect2;
    }

    private void HandleStartPlaying()
    {
        if (_state != ClientState.ReusedConnect2) return;

        var spawn = new SpawnPlayer
        {
            PlayerSlot = _syncPlayerHandler.PlayerSlot,
            Context = PlayerSpawnContext.SpawningIntoWorld,
            DeathsPVE = 0,
            DeathsPVP = 0,
            Position = _spawnPosition,
            Timer = 0,
            Team = 0
        };
        _ = Client.SendPacketToServerAsync(spawn);

        _state = ClientState.Connected;
    }

    private void HandleLoadPlayer()
    {
        if (_syncPlayerHandler.HasSyncPlayer)
        {
            var player = _syncPlayerHandler.GetSyncPlayer();
            if (player.HasValue)
            {
                _ = Client.SendPacketToServerAsync(player.Value);
            }
        }
    }
    

    public override void OnServerChanging()
    {
        _state = ClientState.ReusedConnect1;
    }
}
