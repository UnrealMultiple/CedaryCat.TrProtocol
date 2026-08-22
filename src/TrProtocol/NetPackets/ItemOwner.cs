using Microsoft.Xna.Framework;
using TrProtocol.Models.Interfaces;
using TrProtocol.Attributes;

namespace TrProtocol.NetPackets;

public partial struct ItemOwner : INetPacket, IItemSlot, IOtherPlayerSlot
{
    public readonly MessageID Type => MessageID.ItemOwner;
    public short ItemSlot { get; set; }
    public byte OtherPlayerSlot { get; set; }

    [Int7BitEncoded]
    public int TimeToKeepReservation;

    public byte GrabDelayPlayer;

    [Int7BitEncoded]
    public int GrabDelayTime;

    public Vector2 Position;
}
