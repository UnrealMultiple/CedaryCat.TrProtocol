namespace TrProtocol.Models.CreativePowers;

public partial class FreezeRainPower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.RainFreeze;
    public ASharedTogglePowerData Data;
    
    public override string ToString()
    {
        string state = Data.EnableState ? "FROZEN (ON)" : "NORMAL (OFF)";

        return $"[Power: {PowerType}] World Weather -> {state}";
    }
}
