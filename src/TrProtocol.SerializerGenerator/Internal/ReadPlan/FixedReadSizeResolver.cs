using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TrProtocol.Interfaces;
using TrProtocol.SerializerGenerator.Internal.Extensions;
using TrProtocol.SerializerGenerator.Internal.Serialization.TypeSerializers;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal static class FixedReadSizeResolver
{
    public static string? TryGetFixedReadSizeExpression(TypeSerializerContext context) {
        if (Int7BitEncodedStrategy.Is7BitEncoded(context)) {
            return null;
        }

        if (!context.RoundState.IsArrayRound && context.Member.MemberType is ArrayTypeSyntax) {
            return null;
        }

        var serializeAsType = GetSerializeAsType(context);
        if (serializeAsType is not null) {
            return GetPrimitiveFixedSizeExpression(serializeAsType.GetPredifinedName());
        }

        return TryGetFixedReadSizeExpressionForType(context.MemberTypeSym, context.TypeStr);
    }

    public static string? TryGetFixedReadSizeExpressionForType(ITypeSymbol typeSym, string typeStr) {
        if (typeSym.AllInterfaces.Any(i => i.Name == nameof(IPackedSerializable))) {
            return $"sizeof({typeStr})";
        }

        if (typeSym.AllInterfaces.Any(i => i.Name == nameof(IBinarySerializable))) {
            return null;
        }

        var viewInterface = typeSym.AllInterfaces.FirstOrDefault(i => i.Name == nameof(ISerializableView<>));
        if (viewInterface is not null) {
            var viewType = viewInterface.TypeArguments[0].GetFullTypeName();
            return $"sizeof({viewType})";
        }

        if (typeSym is INamedTypeSymbol { EnumUnderlyingType: not null } enumSym) {
            return GetPrimitiveFixedSizeExpression(enumSym.EnumUnderlyingType!.GetPredifinedName());
        }

        return GetPrimitiveFixedSizeExpression(typeStr);
    }

    private static INamedTypeSymbol? GetSerializeAsType(TypeSerializerContext context) {
        var typeAsAttr = context.FieldMemberSym?.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "SerializeAsAttribute");
        typeAsAttr ??= context.PropMemberSym?.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "SerializeAsAttribute");

        if (typeAsAttr?.ConstructorArguments.Length > 0) {
            return typeAsAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
        }

        return null;
    }

    private static string? GetPrimitiveFixedSizeExpression(string typeStr) => typeStr switch {
        "bool" or nameof(Boolean) => "sizeof(byte)",
        "byte" or nameof(Byte) => "sizeof(byte)",
        "sbyte" or nameof(SByte) => "sizeof(sbyte)",
        "short" or nameof(Int16) => "sizeof(short)",
        "ushort" or nameof(UInt16) => "sizeof(ushort)",
        "char" or nameof(Char) => "sizeof(char)",
        "int" or nameof(Int32) => "sizeof(int)",
        "uint" or nameof(UInt32) => "sizeof(uint)",
        "float" or nameof(Single) => "sizeof(float)",
        "long" or nameof(Int64) => "sizeof(long)",
        "ulong" or nameof(UInt64) => "sizeof(ulong)",
        "double" or nameof(Double) => "sizeof(double)",
        "decimal" or nameof(Decimal) => "sizeof(decimal)",
        _ => null,
    };
}
