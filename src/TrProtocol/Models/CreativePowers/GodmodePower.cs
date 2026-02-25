namespace TrProtocol.Models.CreativePowers;

public partial class GodmodePower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.Godmode;
    public APerPlayerTogglePowerData Data;
    
    public override string ToString()
    {
        switch (Data.SubMessageType)
        {
            case Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.SubMessageType.SyncOnePlayer:
            {
                var state = Data.EnableState ? "ON" : "OFF";
                return $"[Power: {PowerType}] Player[{Data.PlayerSlot}] -> Godmode: {state}";
            }
            case Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.SubMessageType.SyncEveryone:
            {
                var godList = new List<int>();
                for (var i = 0; i < 256; i++) if (Data.PerPlayerIsEnabled[i]) godList.Add(i);

                return $"[Power: {PowerType}] Global Sync | Gods Active: [{string.Join(", ", godList)}]";
            }
            default:
                return $"[Power: {PowerType}] SyncType: {Data.SubMessageType}";
        }
    }
}