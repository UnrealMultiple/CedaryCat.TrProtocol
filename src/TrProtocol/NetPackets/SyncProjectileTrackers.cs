using TrProtocol.Attributes;
using TrProtocol.Models.Interfaces;
using Terraria.DataStructures;

namespace TrProtocol.NetPackets;

public partial struct SyncProjectileTrackers : INetPacket, IPlayerSlot
{
    public readonly MessageID Type => MessageID.SyncProjectileTrackers;
    public byte PlayerSlot { get; set; }

    public TrackedProjectileReference PiggyBankProjectile;
    public TrackedProjectileReference VoidLensChestProjectile;
}
