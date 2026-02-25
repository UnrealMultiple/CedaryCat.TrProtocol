namespace TrProtocol.Models.CreativePowers;

public partial class ModifyRainPower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.RainStrength;
    public ASharedSliderPowerData Data;
    
    public override string ToString()
    {

        return $"[Power: {PowerType}] {Data.SliderState:P1}";
    }
}
