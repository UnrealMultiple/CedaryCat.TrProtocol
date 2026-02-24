using TrProtocol.SerializerGenerator.Internal.Generation;
using TrProtocol.SerializerGenerator.Internal.Utilities;

namespace TrProtocol.SerializerGenerator.Internal.Serialization.TypeSerializers;

/// <summary>
/// String serialization strategy.
/// Uses CommonCode.WriteString/ReadString.
/// </summary>
public class StringTypeStrategy : ITypeSerializerStrategy
{
    public bool StopPropagation => true;
    public bool CanHandle(TypeSerializerContext context) {
        return context.TypeStr is "string" or nameof(String);
    }

    public void GenerateSerialization(TypeSerializerContext context, BlockNode seriBlock, BlockNode deserBlock) {
        var m = context.Member;
        var memberAccess = context.MemberAccess;

        // Track nullable members.
        if (context.ParentVar is null && context.IsConditional && !context.RoundState.IsArrayRound && !context.RoundState.IsEnumRound) {
            context.MemberNullables.Add(m.MemberName);
        }

        seriBlock.WriteLine($"CommonCode.WriteString(ref ptr_current, {memberAccess});");
        seriBlock.WriteLine();

        GenerationHelpers.WriteDebugRelease(
            deserBlock,
            $"{memberAccess} = CommonCode.ReadString(ref ptr_current, ptr_end, nameof({context.Model.TypeName}), \"{m.MemberName}\");",
            $"{memberAccess} = CommonCode.ReadString(ref ptr_current, ptr_end);");
        deserBlock.WriteLine();
    }
}
