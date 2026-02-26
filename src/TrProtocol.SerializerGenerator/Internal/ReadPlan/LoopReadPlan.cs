using TrProtocol.SerializerGenerator.Internal.Utilities;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal sealed record LoopReadPlanSegment(
    BlockNode TargetBlock,
    string SizeExpression,
    string MemberPath,
    string? LoopIndexExpression = null,
    bool UseLongExpression = false);

internal sealed class LoopReadPlan
{
    public List<LoopReadPlanSegment> Segments { get; } = [];
}
