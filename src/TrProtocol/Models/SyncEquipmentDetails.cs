using Terraria;
using TrProtocol.Interfaces;

namespace TrProtocol.Models;

public struct SyncEquipmentDetails : IPackedSerializable
{
    public BitsByte packedValue;
    public bool Favorited {
        get => packedValue[0];
        set => packedValue[0] = value;
    }
    public bool IndicateBlockedSlot {
        get => packedValue[1];
        set => packedValue[1] = value;
    }
    
    public override string ToString()
    {
        if (packedValue == 0) return "[Normal]";

        var states = new List<string>();
        if (Favorited) states.Add("Favorited");
        if (IndicateBlockedSlot) states.Add("Blocked");

        return $"[{string.Join(", ", states)}]";
    }
}
