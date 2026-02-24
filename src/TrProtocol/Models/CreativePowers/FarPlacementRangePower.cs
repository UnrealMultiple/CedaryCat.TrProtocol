namespace TrProtocol.Models.CreativePowers;

public partial class FarPlacementRangePower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.IncreasePlacementRange;
    public APerPlayerTogglePowerData Data;
    
    public override string ToString()
    {
        switch (Data.SubMessageType)
        {
            case Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.SubMessageType.SyncOnePlayer:
            {
                var state = Data.EnableState ? "ENABLED" : "DISABLED";
                return $"[Power: {PowerType}] Player[{Data.PlayerSlot}] -> {state}";
            }
            case Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.SubMessageType.SyncEveryone:
            {
                var enabledPlayers = new List<int>();
                for (var i = 0; i < 256; i++)
                {
                    if (Data.PerPlayerIsEnabled[i]) 
                        enabledPlayers.Add(i);
                }

                var players = enabledPlayers.Count > 0 
                    ? string.Join(", ", enabledPlayers) 
                    : "None";

                return $"[Power: {PowerType}] Global Sync | Enabled Players: [{players}]";
            }
            default:
                return $"[Power: {PowerType}] Unknown Message Type";
        }
    }
}
