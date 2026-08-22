using TrProtocol.Models.Interfaces;

namespace TrProtocol.NetPackets;

public partial struct Dodge : INetPacket, IPlayerSlot
{
    public readonly MessageID Type => MessageID.SyncDodge;
    public byte PlayerSlot { get; set; }
    public byte DodgeType;
}
