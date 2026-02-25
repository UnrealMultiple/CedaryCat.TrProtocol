namespace TrProtocol.Models.CreativePowers;

public partial class FreezeWindDirectionAndStrength : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.WindFreeze;
    public ASharedTogglePowerData Data;
    
    public override string ToString()
    {
        string status = Data.EnableState ? "FROZEN (Static Wind)" : "DYNAMIC (Normal Wind)";

        return $"[Power: {PowerType}] {status}";
    }
}
