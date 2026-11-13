// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// MergeAssembler combines per-segment domain responses into a final answer.
// Each segment may have a different canonical Domain → assemble in dependency order.
public static class MergeAssembler
{
    public record SegmentResponse(
        PromptSegment Segment,
        SemanticCompiler29.Domain Domain,
        string        Response);

    // Merge ordered responses into one coherent output
    public static string Assemble(IReadOnlyList<SegmentResponse> responses)
    {
        if (responses.Count == 0) return string.Empty;
        if (responses.Count == 1) return responses[0].Response;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < responses.Count; i++)
        {
            var r = responses[i];
            if (responses.Count > 1)
                sb.AppendLine(FormatHeader(r.Domain, i + 1, responses.Count));
            sb.AppendLine(r.Response);
            if (i < responses.Count - 1)
                sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    // Merge with dependency ordering from DependencyGraph
    public static OperationResult<string> AssembleOrdered(
        IReadOnlyList<SegmentResponse> responses,
        DependencyGraph graph)
    {
        var segments = responses.Select(r => r.Segment).ToList();
        var sortResult = graph.Sort(segments);
        if (!sortResult.IsSuccess)
            return OperationResult<string>.Fail(sortResult.ErrorMessage, sortResult.ErrorCode);

        var ordered = new List<SegmentResponse>();
        foreach (var level in sortResult.Value!)
            foreach (var seg in level)
            {
                var match = responses.FirstOrDefault(r => r.Segment == seg);
                if (match != null) ordered.Add(match);
            }

        return OperationResult<string>.Ok(Assemble(ordered));
    }

    private static string FormatHeader(SemanticCompiler29.Domain domain, int index, int total)
    {
        string label = domain switch
        {
            SemanticCompiler29.Domain.CODE_Q  => "Explanation",
            SemanticCompiler29.Domain.BUG     => "Bug Fix",
            SemanticCompiler29.Domain.PLAN    => "Plan",
            SemanticCompiler29.Domain.ARCH    => "Architecture",
            SemanticCompiler29.Domain.REFACTOR=> "Refactoring",
            _                   => domain.ToString(),
        };
        return total > 1 ? $"## Part {index}/{total} — {label}" : $"## {label}";
    }
}
