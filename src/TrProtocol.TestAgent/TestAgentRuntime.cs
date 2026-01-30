namespace TrProtocol.TestAgent;

public static class TestAgentRuntime
{
    private static readonly object Gate = new();
    private static DirectionFilter _directionFilter = DirectionFilter.All;

    public static void ApplyConfig(TestAgentConfig config)
    {
        lock (Gate)
        {
            _directionFilter = ParseDirection(config.Filters.Direction);
        }

        PacketInspector.Configure(PacketInspectorOptions.FromConfig(config));
    }

    public static bool ShouldInspectDirection(bool isC2S)
    {
        DirectionFilter filter;
        lock (Gate)
        {
            filter = _directionFilter;
        }

        return filter switch
        {
            DirectionFilter.All => true,
            DirectionFilter.C2S => isC2S,
            DirectionFilter.S2C => !isC2S,
            _ => true,
        };
    }

    private static DirectionFilter ParseDirection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DirectionFilter.All;
        }
        return value.Trim().ToLowerInvariant() switch
        {
            "all" => DirectionFilter.All,
            "c2s" => DirectionFilter.C2S,
            "s2c" => DirectionFilter.S2C,
            _ => DirectionFilter.All,
        };
    }
}

