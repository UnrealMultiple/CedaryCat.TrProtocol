using TrProtocol.Models.Interfaces;

namespace TrProtocol.NetPackets;

public partial struct StrikeNPC : INetPacket
{
    public readonly MessageID Type => MessageID.StrikeNPC;
    public byte NPCSlot;
    public byte NPCGeneration;
    public short Damage;
    public float Knockback;
    public byte HitDirection;
    public bool Crit;
}
