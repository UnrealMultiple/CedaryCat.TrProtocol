namespace TrProtocol.Models.CreativePowers;

public partial class StartNoonImmediately : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.SetNoon;
    public override string ToString()
    {
        return $"[Power: {PowerType}]";
    }
}
