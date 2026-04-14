using System.Reflection;
using System.Text;
using TrProtocol.Attributes;
using TrProtocol.Interfaces;

namespace TrProtocol;

[GenerateGlobalID]
[PolymorphicBase(typeof(MessageID), nameof(Type))]
public partial interface INetPacket : IAutoSerializable
{
    public abstract MessageID Type { get; }
    public string? ToString() {
        return $"{{{Type}}}";
    }

    // 不换行的属性打印
    public string ToStringInline()
    {
        var type = GetType();
        var sb = new StringBuilder();
        sb.Append($"[{type.Name}] {{ ");
        
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        var fields = type.GetFields(flags)
            .Where(f => !f.Name.Contains("k__BackingField"));

        var properties = type.GetProperties(flags)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
        
        var members = fields
            .Select<FieldInfo, (string name, object value)>(f => (f.Name, f.GetValue(this)!))
            .Concat(properties.Select<PropertyInfo, (string name, object value)>(p => (p.Name, p.GetValue(this)!)))
            .Where(m => m.name != "Type" && m.name != "ModuleType");

        bool first = true;
        foreach (var (name, value) in members)
        {
            if (!first) sb.Append(", ");
            first = false;
            
            var formattedValue = value switch
            {
                null => "null",
                byte[] bytes => $"byte[{bytes.Length}]",
                Array arr => $"{arr.GetType().GetElementType()?.Name}[{arr.Length}]",
                string s => $"\"{s}\"",
                bool b => b.ToString().ToLower(),
                _ => value.ToString()
            } ?? string.Empty;

            sb.Append($"{name}={formattedValue}");
        }

        sb.Append(" }");
        return sb.ToString();
    }

    public string Describe(int indentLevel = 0)
    {
        var type = GetType();
        var indent = new string(' ', indentLevel * 4);
        var subIndent = new string(' ', (indentLevel + 1) * 4);

        StringBuilder sb = new();
        sb.AppendLine($"[{indent}{type.Name}] {{");
        
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        var fields = type.GetFields(flags)
            .Where(f => !f.Name.Contains("k__BackingField"));

        var properties = type.GetProperties(flags)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
        
        IEnumerable<(string name, object value)> members = fields
            .Select<FieldInfo, (string, object)>(f => new(f.Name, f.GetValue(this)!))
            .Concat(properties.Select<PropertyInfo, (string, object)>(p => new(p.Name, p.GetValue(this)!)));

        foreach (var member in members)
        {
            if (member.name == "Type") continue;
            if (member.name == "ModuleType") continue; 
            
            var formattedValue = member.value switch
            {
                null => "null",
                byte[] bytes => $"byte[{bytes.Length}] {{ {BitConverter.ToString(bytes).Replace("-", " ")} }}",
                Array arr => $"{arr.GetType().GetElementType()?.Name}[{arr.Length}] {{ {string.Join(", ", arr.Cast<object>().Select(o => o?.ToString() ?? "null"))} }}", 
                string s => $"\"{s}\"",
                bool b => b.ToString().ToLower(),
                _ => member.value.ToString()
            } ?? string.Empty;

            sb.AppendLine($"{subIndent}{member.name} = {formattedValue}");
        }

        sb.Append($"{indent}}}");
        return sb.ToString();
    }
}
