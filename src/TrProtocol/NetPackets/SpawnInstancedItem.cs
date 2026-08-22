using Microsoft.Xna.Framework;
using Terraria;
using TrProtocol.Attributes;
using TrProtocol.Models.Interfaces;

namespace TrProtocol.NetPackets;

public partial struct SpawnInstancedItem : INetPacket, IItemSlot
{
    public readonly MessageID Type => MessageID.SpawnInstancedItem;
    public short ItemSlot { get; set; }
    public Vector2 Position;
    public Vector2 Velocity;
    public short Stack;
    public byte Prefix;
    public BitsByte Flags;
    public short ItemType;

    [Condition(nameof(Flags), 2)]
    public bool Shimmered;

    [Condition(nameof(Flags), 2)]
    public float ShimmerTime;

    [Condition(nameof(Flags), 3)]
    public byte EnemyGrabDelayTime;

    [IgnoreSerialize]
    public NewItemOwnership Ownership {
        readonly get => (NewItemOwnership)(Flags.value & 3);
        set => Flags.value = (byte)((Flags.value & ~3) | ((byte)value & 3));
    }
}
