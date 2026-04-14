using System.Text.Json.Serialization;

namespace RealmNexus.Models;

public sealed class Server
{
    [JsonPropertyName("name")]
    public required string Name { get; init; } = "a";

    [JsonPropertyName("host")]
    public required string Host { get; init; } = "127.0.0.1";

    [JsonPropertyName("port")]
    public required int Port { get; init; } = 7777;
}
