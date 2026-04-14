using RealmNexus.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealmNexus.Models;

public class Config
{
    [JsonPropertyName("port")]
    public int Port { get; set; } = 7654;

    [JsonPropertyName("log_level")]
    [JsonConverter(typeof(LogLevelJsonConverter))]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [JsonPropertyName("servers")]
    public Server[] Servers { get; set; }

    public const string Filename = "config.json";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Server GetServer(string name) => Servers.FirstOrDefault(s => s.Name == name);

    public static Config Instance
    {
        get
        {
            return field ??= Read();
        }
        set
        {
            field = value;
        }
    }

    public static Config Read()
    {
        if (File.Exists(Filename))
        {
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(Filename), JsonSerializerOptions);
        }
        var cfg = new Config();
        cfg.SetDefault();
        File.WriteAllText(Filename, JsonSerializer.Serialize(cfg, JsonSerializerOptions));
        return cfg;
    }

    private void SetDefault()
    {
        Servers =
        [
            new()
            {
                Host = "127.0.0.1",
                Port = 7777,
                Name = "a"
            },
            new()
            {
                Host = "127.0.0.1",
                Port = 7778,
                Name = "b"
            }
        ];
    }
}
