using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TrProtocol.NetPackets;

namespace Dimensions.Core;

public class ConnectionHandler : ClientHandler
{
    public enum ClientState
    {
        New,
        ReusedConnect1,
        ReusedConnect2,
        Connected,
    }

    private Point16 spawnPosition;

    // once client received world data from server, set it to true and request tile data
    private ClientState state = ClientState.New;

    public override void OnS2CPacket(PacketReceiveArgs args)
    {
        if (args.Packet is WorldData data)
        {
            if (state != ClientState.ReusedConnect1) return;
            Parent.SendServer(new RequestTileData
            {
                Position = new Point(data.SpawnX, data.SpawnY)
            });
            spawnPosition = new Point16(data.SpawnX, data.SpawnY);
            state = ClientState.ReusedConnect2;
        }
        else if (args.Packet is StartPlaying)
        {
            if (state != ClientState.ReusedConnect2) return;
            var spawn = new SpawnPlayer
            {
                PlayerSlot = Parent.syncPlayer.PlayerSlot,
                Context = PlayerSpawnContext.SpawningIntoWorld,
                DeathsPVE = 0,
                DeathsPVP = 0,
                Position = spawnPosition,
                Timer = 0,
                Team = 0
            };
            Parent.SendClient(spawn);
            Parent.SendServer(spawn);
            state = ClientState.Connected;
        }
        else if (args.Packet is LoadPlayer)
        {
            if (!string.IsNullOrEmpty(Parent.syncPlayer.Name))
            {
                Parent.SendServer(Parent.syncPlayer);
            }
        }
    }

    public override void OnC2SPacket(PacketReceiveArgs args)
    {
        if (args.Packet is SyncPlayer sync)
        {
            if (!string.IsNullOrEmpty(Parent.syncPlayer.Name) && Parent.syncPlayer.Name != sync.Name)
            {
                Parent.Disconnect("禁止修改名字");
                args.Handled = true;
            }
            else
            {
                Parent.syncPlayer = sync;
                Parent.Name = sync.Name;
            }
        }
    }

    public override void OnCleaning()
    {
        state = ClientState.ReusedConnect1;
    }
}