namespace TrProtocol.Models.CreativePowers;

public partial class StopBiomeSpreadPower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.BiomeSpreadFreeze;
    public ASharedTogglePowerData Data;
    
    public override string ToString()
    {
        var status = Data.EnableState ? "LOCKED (No Spread)" : "ACTIVE (Natural Spread)";
        return $"[Power: {PowerType}] Biome Evolution -> {status}";
    }
}
