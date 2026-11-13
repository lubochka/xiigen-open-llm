namespace SemanticCompiler;

// MergeAssembler combines per-segment domain responses into a final answer.
// Each segment may have a different DomainLabel → assemble in dependency order.
public static class MergeAssembler
{
    public record SegmentResponse(
        PromptSegment Segment,
        DomainLabel   Domain,
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

    private static string FormatHeader(DomainLabel domain, int index, int total)
    {
        string label = domain switch
        {
            DomainLabel.CODE_Q  => "Explanation",
            DomainLabel.BUG     => "Bug Fix",
            DomainLabel.PLAN    => "Plan",
            DomainLabel.ARCH    => "Architecture",
            DomainLabel.REFACTOR=> "Refactoring",
            _                   => domain.ToString(),
        };
        return total > 1 ? $"## Part {index}/{total} — {label}" : $"## {label}";
    }
}
