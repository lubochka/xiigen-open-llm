namespace ExperimentsV2;

// PromptSplitter detects segment boundaries inside a natural-language prompt
// by comparing the op-family profiles of a sliding window on each side of a
// candidate boundary. A cosine distance above _changeThreshold signals a
// domain shift — i.e. the user switched from one task to another.
public sealed class PromptSplitter
{
    private readonly double _changeThreshold;
    private readonly int    _windowSize;

    public PromptSplitter(double changeThreshold = 0.4, int windowSize = 2)
    {
        _changeThreshold = changeThreshold;
        _windowSize      = windowSize;
    }

    // Split prompt into segments. Always succeeds — returns 1 segment on error.
    public OperationResult<IReadOnlyList<PromptSegment>> Split(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<IReadOnlyList<PromptSegment>>.Ok(
                new[] { new PromptSegment(prompt, 0, 0) });

        var words = SemanticNormalizer.Normalize(prompt);
        if (words.Count < _windowSize * 2 + 1)
            return OperationResult<IReadOnlyList<PromptSegment>>.Ok(
                new[] { new PromptSegment(prompt, 0, Math.Max(0, words.Count - 1)) });

        var ops = FamilyCollapser.Collapse(words);

        var boundaries = new List<int>();
        for (int i = _windowSize; i < ops.Count - _windowSize; i++)
        {
            var left  = GetOpProfile(ops, Math.Max(0, i - _windowSize), i);
            var right = GetOpProfile(ops, i, Math.Min(ops.Count, i + _windowSize));

            // Only fire if both sides have at least one mapped op — avoids
            // trivial boundaries where one side has no signal.
            if (left.Count == 0 || right.Count == 0) continue;

            double dist = CosineDist(left, right);
            if (dist > _changeThreshold)
                boundaries.Add(i);
        }

        var clean = MergeClose(boundaries, minGap: 2);

        // Build segment texts from the raw prompt words
        var rawWords  = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var allBounds = new[] { 0 }.Concat(clean).Append(rawWords.Length).ToArray();
        var segments  = new List<PromptSegment>();

        for (int i = 0; i < allBounds.Length - 1; i++)
        {
            int wStart = allBounds[i];
            int wEnd   = Math.Min(allBounds[i + 1], rawWords.Length);
            if (wStart >= wEnd) continue;
            segments.Add(new PromptSegment(
                string.Join(" ", rawWords[wStart..wEnd]), wStart, wEnd - 1));
        }

        if (segments.Count == 0)
            segments.Add(new PromptSegment(prompt, 0, rawWords.Length - 1));

        return OperationResult<IReadOnlyList<PromptSegment>>.Ok(segments.AsReadOnly());
    }

    // Only count tokens that mapped to a semantic op (end with "_OP").
    private static Dictionary<string, double> GetOpProfile(
        List<string> ops, int start, int end)
    {
        var profile = new Dictionary<string, double>();
        for (int i = start; i < end; i++)
            if (ops[i].EndsWith("_OP", StringComparison.Ordinal))
                profile[ops[i]] = profile.GetValueOrDefault(ops[i], 0) + 1;
        double total = profile.Values.Sum();
        if (total > 0)
            foreach (var k in profile.Keys.ToList()) profile[k] /= total;
        return profile;
    }

    private static double CosineDist(
        Dictionary<string, double> a, Dictionary<string, double> b)
    {
        var allKeys = a.Keys.Union(b.Keys);
        double dot = allKeys.Sum(k =>
            a.GetValueOrDefault(k, 0) * b.GetValueOrDefault(k, 0));
        double na = Math.Sqrt(a.Values.Sum(v => v * v));
        double nb = Math.Sqrt(b.Values.Sum(v => v * v));
        return na < 1e-9 || nb < 1e-9 ? 1.0 : 1.0 - dot / (na * nb);
    }

    private static List<int> MergeClose(List<int> bs, int minGap)
    {
        if (bs.Count < 2) return bs;
        var result = new List<int> { bs[0] };
        for (int i = 1; i < bs.Count; i++)
            if (bs[i] - result[^1] >= minGap)
                result.Add(bs[i]);
        return result;
    }
}

public record PromptSegment(string Text, int WordStart, int WordEnd);
