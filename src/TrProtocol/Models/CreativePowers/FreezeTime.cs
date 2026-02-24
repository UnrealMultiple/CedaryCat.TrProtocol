namespace TrProtocol.Models.CreativePowers;

public partial class FreezeTime : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.FreezeTime;
    public ASharedTogglePowerData Data;
    
    public override string ToString()
    {
        string status = Data.EnableState ? "FROZEN (Time Paused)" : "NORMAL (Time Flowing)";

        return $"[Power: {PowerType}] {status}";
    }
}
