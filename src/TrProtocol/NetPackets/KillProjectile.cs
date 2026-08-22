using TrProtocol.Models.Interfaces;

using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace TrProtocol.NetPackets;

public partial struct KillProjectile : INetPacket
{
    public readonly MessageID Type => MessageID.KillProjectile;
    public ProjectileKey Key;
    public Vector2 FinalPosition;
}
