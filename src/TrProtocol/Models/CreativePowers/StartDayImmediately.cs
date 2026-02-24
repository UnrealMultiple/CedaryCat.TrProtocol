namespace TrProtocol.Models.CreativePowers;

public partial class StartDayImmediately : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.SetDawn;
    
    public override string ToString()
    {
        return $"[Power: {PowerType}]";
    }
}
