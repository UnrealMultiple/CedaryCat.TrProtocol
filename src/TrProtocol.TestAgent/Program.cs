using System.Text.Json;
using TrProtocol.TestAgent;

Console.WriteLine("TrProtocol Test Agent v1.0");

static string? GetArgValue(string[] args, string key) {
    for (int i = 0; i < args.Length - 1; i++) {
        if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase)) {
            return args[i + 1];
        }
    }
    return null;
}

string configPath = GetArgValue(args, "--config")
    ?? Path.Combine(Directory.GetCurrentDirectory(), "testagent.json");

if (!File.Exists(configPath)) {
    string exeConfig = Path.Combine(AppContext.BaseDirectory, "testagent.json");
    if (File.Exists(exeConfig)) {
        configPath = exeConfig;
    }
}

TestAgentConfig config;
try {
    if (File.Exists(configPath)) {
        string json = await File.ReadAllTextAsync(configPath);
        config = JsonSerializer.Deserialize<TestAgentConfig>(json, TestAgentConfig.JsonOptions) ?? new TestAgentConfig();
    }
    else {
        config = new TestAgentConfig();
        string json = JsonSerializer.Serialize(config, TestAgentConfig.JsonOptions);
        await File.WriteAllTextAsync(configPath, json);
        Console.WriteLine($"[Config] Created default config at: {configPath}");
    }
}
catch (Exception ex) {
    Console.WriteLine($"[Config] Failed to read '{configPath}': {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine("[Config] Falling back to defaults.");
    config = new TestAgentConfig();
}

TestAgentRuntime.ApplyConfig(config);

Console.WriteLine($"[Config] File: {configPath}");
Console.WriteLine($"[Config] Proxy: Listen {config.ListenPort} -> {config.TargetHost}:{config.TargetPort}");
Console.WriteLine($"[Config] RoundTrip: {(config.RoundTrip.Enabled ? "ENABLED" : "disabled")} (dump={config.RoundTrip.Dump})");
Console.WriteLine($"[Config] Filters: Dir={config.Filters.Direction} OK={(config.Filters.ShowOk ? "on" : "off")} Parse={(config.Filters.ShowParseIssues ? "on" : "off")} RT-Issues={(config.Filters.ShowRoundTripIssues ? "on" : "off")}");

var proxy = new ProxyServer(config.ListenPort, config.TargetHost, config.TargetPort);
await proxy.StartAsync();
