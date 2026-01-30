namespace TrProtocol.TestAgent;

public readonly record struct PacketInspectorOptions(
    bool RoundTripEnabled,
    string RoundTripDumpMode,
    int RoundTripContextLines,
    int RoundTripFullDumpThresholdBytes,
    bool ShowOk,
    bool ShowParseIssues,
    bool ShowRoundTripIssues)
{
    public bool RoundTripDumpFull =>
        string.Equals(RoundTripDumpMode, "full", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(RoundTripDumpMode, "all", StringComparison.OrdinalIgnoreCase);

    public static PacketInspectorOptions FromConfig(TestAgentConfig config)
    {
        string dump = string.IsNullOrWhiteSpace(config.RoundTrip.Dump) ? "window" : config.RoundTrip.Dump.Trim();
        int contextLines = Math.Clamp(config.RoundTrip.ContextLines, 0, 100);
        int thresholdBytes = Math.Clamp(config.RoundTrip.FullDumpThresholdBytes, 0, 8 * 1024 * 1024);
        return new PacketInspectorOptions(
            RoundTripEnabled: config.RoundTrip.Enabled,
            RoundTripDumpMode: dump,
            RoundTripContextLines: contextLines,
            RoundTripFullDumpThresholdBytes: thresholdBytes,
            ShowOk: config.Filters.ShowOk,
            ShowParseIssues: config.Filters.ShowParseIssues,
            ShowRoundTripIssues: config.Filters.ShowRoundTripIssues);
    }
}
