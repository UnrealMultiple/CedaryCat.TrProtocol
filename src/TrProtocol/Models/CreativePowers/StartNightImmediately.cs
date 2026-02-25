namespace TrProtocol.Models.CreativePowers;

public partial class StartNightImmediately : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.SetDusk;
    
    public override string ToString()
    {
        return $"[Power: {PowerType}]";
    }
}
