namespace TrProtocol.Attributes;

/// <summary>
/// Serializes a one-dimensional array with an inline numeric length prefix.
/// </summary>
/// <remarks>
/// <para>
/// Interpreted by <c>TrProtocol.SerializerGenerator</c> for one-dimensional arrays. The generator writes
/// <c>(LengthType)array.Length</c> before element data, and reads the same prefix during deserialization to allocate
/// the target array.
/// </para>
/// <para>
/// Supported length prefix types are integral non-floating primitives up to <see cref="uint"/>:
/// <c>byte</c>, <c>sbyte</c>, <c>short</c>, <c>ushort</c>, <c>int</c>, <c>uint</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class LengthPrefixedArrayAttribute(Type lengthType) : Attribute
{
    public Type LengthType { get; } = lengthType;
}
