using TrProtocol.SerializerGenerator.Internal.Utilities;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal static class LoopReadPlanBuilder
{
    public static LoopReadPlan SingleSegment(
        BlockNode targetBlock,
        string sizeExpression,
        string memberPath,
        string? loopIndexExpression = null,
        bool useLongExpression = false) {
        var plan = new LoopReadPlan();
        plan.Segments.Add(new LoopReadPlanSegment(
            targetBlock,
            sizeExpression,
            memberPath,
            loopIndexExpression,
            useLongExpression));
        return plan;
    }
}
