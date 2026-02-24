namespace TrProtocol.Attributes;

/// <summary>
/// Includes a non-public member in source-generated serialization.
/// </summary>
/// <remarks>
/// Interpreted by <c>TrProtocol.SerializerGenerator</c>. Use this on private/protected/internal fields or properties
/// when they should still participate in generated read/write code.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class IncludeSerializeAttribute : Attribute { }
