using Newtonsoft.Json;

namespace Dimensions.Models;

public class Config
{
    [JsonProperty("listenPort")]
    public ushort ListenPort { get; set; } = 7654;
    [JsonProperty("sendDimensionPacket")]
    public bool SendDimensionPacket { get; set; } = false;
    [JsonProperty("protocolVersion")]
    public string ProtocolVersion { get; set; } = "Terraria319";
    [JsonProperty("servers")]
    public Server[] Servers { get; set; } = [];

    private Dictionary<string, Server> _serverCache;

    public Server GetServer(string name)
    {
        _serverCache ??= Servers.ToDictionary(s => s.Name!, s => s);
        return _serverCache.TryGetValue(name, out var val) ? val : null;
    }
}