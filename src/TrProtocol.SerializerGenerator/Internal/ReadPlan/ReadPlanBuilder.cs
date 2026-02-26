using TrProtocol.SerializerGenerator.Internal.Conditions.Model;
using TrProtocol.SerializerGenerator.Internal.Conditions.Optimization;
using TrProtocol.SerializerGenerator.Internal.Models;

namespace TrProtocol.SerializerGenerator.Internal.ReadPlan;

internal static class ReadPlanBuilder
{
    public static ReadPlan Build(
        IEnumerable<ConditionBlock> blocks,
        IReadOnlyDictionary<SerializationExpandContext, ReadPlanMember> membersByContext) {
        var plan = new ReadPlan();

        foreach (var block in blocks) {
            var scope = BuildScope(block, membersByContext);
            plan.Root.Branches.Add(new ReadPlanBranch(block.Condition, null, scope));
        }

        return plan;
    }

    private static ReadPlanScope BuildScope(
        ConditionBlock block,
        IReadOnlyDictionary<SerializationExpandContext, ReadPlanMember> membersByContext) {
        var scope = new ReadPlanScope();
        var groups = BuildGroups(block, membersByContext);

        foreach (var group in groups) {
            if (group.Members.Count == 0) {
                continue;
            }

            if (group.Condition.IsEmpty) {
                AppendSegments(scope, group.Members);
            }
            else {
                var branchScope = new ReadPlanScope();
                AppendSegments(branchScope, group.Members);
                scope.Branches.Add(new ReadPlanBranch(group.Condition, group.ParentVar, branchScope));
            }
        }

        foreach (var nested in block.NestedBlocks) {
            var nestedScope = BuildScope(nested, membersByContext);
            scope.Branches.Add(new ReadPlanBranch(nested.Condition, null, nestedScope));
        }

        return scope;
    }

    private static List<MemberGroup> BuildGroups(
        ConditionBlock block,
        IReadOnlyDictionary<SerializationExpandContext, ReadPlanMember> membersByContext) {
        var groups = new List<MemberGroup>();

        for (int i = 0; i < block.Members.Count;) {
            var first = block.Members[i];
            var additional = first.AdditionalCondition;
            var additionalKey = additional.IsEmpty ? null : additional.GetNormalizedKey();
            var parentVar = additional.IsEmpty ? null : first.ParentVar;
            var members = new List<ReadPlanMember>();

            if (membersByContext.TryGetValue(first.Member, out var firstMember)) {
                members.Add(firstMember);
            }

            i++;
            for (; i < block.Members.Count; i++) {
                var next = block.Members[i];
                var nextAdditional = next.AdditionalCondition;
                var nextKey = nextAdditional.IsEmpty ? null : nextAdditional.GetNormalizedKey();
                var nextParentVar = nextAdditional.IsEmpty ? null : next.ParentVar;

                if (nextKey != additionalKey || nextParentVar != parentVar) {
                    break;
                }

                if (membersByContext.TryGetValue(next.Member, out var nextMember)) {
                    members.Add(nextMember);
                }
            }

            groups.Add(new MemberGroup(additional, parentVar, members));
        }

        return groups;
    }

    private static void AppendSegments(ReadPlanScope scope, IReadOnlyList<ReadPlanMember> members) {
        int i = 0;
        while (i < members.Count) {
            var member = members[i];
            if (member.NodeKind != ReadPlanNodeKind.Fixed || string.IsNullOrWhiteSpace(member.FixedSizeExpression)) {
                i++;
                continue;
            }

            int j = i + 1;
            var expressions = new List<string> { member.FixedSizeExpression! };
            while (j < members.Count) {
                var next = members[j];
                if (next.NodeKind != ReadPlanNodeKind.Fixed
                    || string.IsNullOrWhiteSpace(next.FixedSizeExpression)
                    || next.ParentVar != member.ParentVar) {
                    break;
                }

                expressions.Add(next.FixedSizeExpression!);
                j++;
            }

            scope.Segments.Add(new ReadPlanSegment(
                members.Skip(i).Take(j - i).ToArray(),
                string.Join(" + ", expressions),
                member.Member.MemberName));
            i = j;
        }
    }

    private sealed record MemberGroup(
        ConditionNode Condition,
        string? ParentVar,
        IReadOnlyList<ReadPlanMember> Members);
}
