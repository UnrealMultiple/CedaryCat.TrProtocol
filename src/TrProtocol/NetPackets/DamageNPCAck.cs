namespace TrProtocol.NetPackets;

public partial struct DamageNPCAck : INetPacket
{
    public readonly MessageID Type => MessageID.DamageNPCAck;
}
