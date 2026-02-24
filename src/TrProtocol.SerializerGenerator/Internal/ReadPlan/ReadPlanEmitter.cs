using TrProtocol.SerializerGenerator.Internal.Utilities;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal static class ReadPlanEmitter
{
    public static void Emit(ReadPlan plan, string typeName) {
        foreach (var segment in plan.EnumerateSegments()) {
            EmitSegment(segment, typeName);
        }
    }

    private static void EmitSegment(ReadPlanSegment segment, string typeName) {
        var target = segment.FirstMember.DeserializationBlock;
        var memberPath = segment.MemberPath;
        var expr = segment.SizeExpression;

        target.Sources.Insert(0, new NewLineTextNode("#endif", target));
        target.Sources.Insert(0, new NewLineTextNode($"CommonCode.EnsureReadable(ptr_current, ptr_end, {expr});", target));
        target.Sources.Insert(0, new NewLineTextNode("#else", target));
        target.Sources.Insert(0, new NewLineTextNode($"CommonCode.EnsureReadable(ptr_current, ptr_end, {expr}, nameof({typeName}), \"{memberPath}\");", target));
        target.Sources.Insert(0, new NewLineTextNode("#if DEBUG", target));
    }
}
