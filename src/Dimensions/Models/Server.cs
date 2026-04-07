using Newtonsoft.Json;

namespace Dimensions.Models;

public class Server
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("serverIP")]
    public string ServerIP { get; set; } = "127.0.0.1";
    [JsonProperty("serverPort")]
    public ushort ServerPort { get; set; } = 7777;
}