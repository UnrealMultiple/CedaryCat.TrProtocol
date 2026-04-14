using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealmNexus.Logging;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public class LogLevelJsonConverter : JsonConverter<LogLevel>
{
    public override LogLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return value?.ToLowerInvariant() switch
            {
                "debug" => LogLevel.Debug,
                "info" => LogLevel.Info,
                "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                _ => LogLevel.Info
            };
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            return (LogLevel)reader.GetInt32();
        }
        
        return LogLevel.Info;
    }

    public override void Write(Utf8JsonWriter writer, LogLevel value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}
