namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal static class LoopReadPlanEmitter
{
    public static void Emit(LoopReadPlan plan, string typeName) {
        foreach (var segment in plan.Segments) {
            EmitSegment(segment, typeName);
        }
    }

    public static void EmitSingleSegment(
        TrProtocol.SerializerGenerator.Internal.Utilities.BlockNode targetBlock,
        string sizeExpression,
        string typeName,
        string memberPath,
        string? loopIndexExpression = null,
        bool useLongExpression = false) {
        Emit(
            LoopReadPlanBuilder.SingleSegment(
                targetBlock,
                sizeExpression,
                memberPath,
                loopIndexExpression,
                useLongExpression),
            typeName);
    }

    private static void EmitSegment(LoopReadPlanSegment segment, string typeName) {
        var expr = segment.UseLongExpression ? $"(long)({segment.SizeExpression})" : segment.SizeExpression;
        var debugLine = segment.LoopIndexExpression is null
            ? $"CommonCode.EnsureReadable(ptr_current, ptr_end, {expr}, nameof({typeName}), \"{segment.MemberPath}\");"
            : $"CommonCode.EnsureReadable(ptr_current, ptr_end, {expr}, nameof({typeName}), \"{segment.MemberPath}\", {segment.LoopIndexExpression});";
        var releaseLine = $"CommonCode.EnsureReadable(ptr_current, ptr_end, {expr});";

        segment.TargetBlock.WriteLine("#if DEBUG");
        segment.TargetBlock.WriteLine(debugLine);
        segment.TargetBlock.WriteLine("#else");
        segment.TargetBlock.WriteLine(releaseLine);
        segment.TargetBlock.WriteLine("#endif");
    }
}
