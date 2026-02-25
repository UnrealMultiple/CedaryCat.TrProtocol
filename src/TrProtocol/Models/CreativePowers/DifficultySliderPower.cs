namespace TrProtocol.Models.CreativePowers;

public partial class DifficultySliderPower : CreativePower
{
    public sealed override CreativePowerTypes PowerType => CreativePowerTypes.WorldDifficulty;
    public ASharedSliderPowerData Data;
    
    public override string ToString()
    {
        return $"[Power: {PowerType}] {Data.SliderState * 100:F0}% Difficulty";
    }
}
