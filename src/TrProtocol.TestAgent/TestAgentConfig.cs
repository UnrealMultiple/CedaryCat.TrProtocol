using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrProtocol.TestAgent;

public sealed class TestAgentConfig
{
    public int ListenPort { get; set; } = 7654;
    public string TargetHost { get; set; } = "127.0.0.1";
    public int TargetPort { get; set; } = 7777;

    public RoundTripConfig RoundTrip { get; set; } = new();
    public FiltersConfig Filters { get; set; } = new();

    public sealed class RoundTripConfig
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// "window" (default) or "full"
        /// </summary>
        public string Dump { get; set; } = "window";

        public int ContextLines { get; set; } = 3;

        public int FullDumpThresholdBytes { get; set; } = 256;
    }

    public sealed class FiltersConfig
    {
        /// <summary>
        /// "all" (default), "c2s", or "s2c"
        /// </summary>
        public string Direction { get; set; } = "all";

        /// <summary>
        /// Print parse OK lines (normally quiet).
        /// </summary>
        public bool ShowOk { get; set; } = false;

        /// <summary>
        /// Show parse-time issues: critical fail / unknown packet / under-read / over-read.
        /// </summary>
        public bool ShowParseIssues { get; set; } = true;

        /// <summary>
        /// Show roundtrip issues: under-write / over-write / mismatch.
        /// </summary>
        public bool ShowRoundTripIssues { get; set; } = true;
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
