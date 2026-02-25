namespace TrProtocol.Models.CreativePowers;

public partial class ModifyWindDirectionAndStrength : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.WindStrength;
    public ASharedSliderPowerData Data;
    public override string ToString()
    {
        return $"[Power: {PowerType}] {Data.SliderState:P1}";
    }
}
