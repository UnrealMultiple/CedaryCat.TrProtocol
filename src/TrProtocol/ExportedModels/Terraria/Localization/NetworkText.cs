using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TrProtocol;
using TrProtocol.Attributes;
using TrProtocol.Interfaces;

namespace Terraria.Localization;
public partial class NetworkText : IAutoSerializable
{
    public enum Mode : byte
    {
        Literal,
        Formattable,
        LocalizationKey
    }

    public Mode _mode;
    public string _text;

    [ConditionNotEqual(nameof(_mode), 0)]
    [LengthPrefixedArray(typeof(byte))]
    public NetworkText[]? _substitutions = [];

    public NetworkText(string text, Mode mode) {
        _text = text;
        _mode = mode;
    }
    public NetworkText() {
        _text = string.Empty;
    }
    
    public override string ToString()
    {
        switch (_mode)
        {
            case Mode.Literal:
                return _text;
            case Mode.Formattable:
            case Mode.LocalizationKey:
            {
                var text2 = _text;
                var substitutions = _substitutions;
                return string.Format(text2, substitutions);
            }
            default:
                return _text;
        } 
    }
}
