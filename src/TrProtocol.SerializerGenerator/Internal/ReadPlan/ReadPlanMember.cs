using TrProtocol.SerializerGenerator.Internal.Conditions.Model;
using TrProtocol.SerializerGenerator.Internal.Models;
using TrProtocol.SerializerGenerator.Internal.Utilities;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal enum ReadPlanNodeKind
{
    Fixed,
    Variable
}

internal sealed record ReadPlanMember(
    SerializationExpandContext Member,
    ConditionNode Condition,
    string? ParentVar,
    ReadPlanNodeKind NodeKind,
    string? FixedSizeExpression,
    BlockNode SerializationBlock,
    BlockNode DeserializationBlock);

internal sealed record ReadPlanSegment(
    IReadOnlyList<ReadPlanMember> Members,
    string SizeExpression,
    string MemberPath,
    string? LoopIndexExpression = null)
{
    public ReadPlanMember FirstMember => Members[0];
}

internal sealed class ReadPlanScope
{
    public List<ReadPlanSegment> Segments { get; } = [];
    public List<ReadPlanBranch> Branches { get; } = [];
}

internal sealed class ReadPlanBranch(
    ConditionNode condition,
    string? parentVar,
    ReadPlanScope scope)
{
    public ConditionNode Condition { get; } = condition;
    public string? ParentVar { get; } = parentVar;
    public ReadPlanScope Scope { get; } = scope;
}

internal sealed class ReadPlan
{
    public ReadPlanScope Root { get; } = new();

    public IEnumerable<ReadPlanSegment> EnumerateSegments() {
        foreach (var segment in EnumerateSegments(Root)) {
            yield return segment;
        }
    }

    private static IEnumerable<ReadPlanSegment> EnumerateSegments(ReadPlanScope scope) {
        foreach (var segment in scope.Segments) {
            yield return segment;
        }

        foreach (var branch in scope.Branches) {
            foreach (var segment in EnumerateSegments(branch.Scope)) {
                yield return segment;
            }
        }
    }
}
