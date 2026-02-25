namespace TrProtocol.Models.CreativePowers;

public partial class ModifyTimeRate : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.TimeSpeed;
    public ASharedSliderPowerData Data;
    
    public override string ToString()
    {
        return $"[Power: {PowerType}] {Data.SliderState:F1}x";
    }
}
