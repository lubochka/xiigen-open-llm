namespace SemanticCompiler;

// StackDetector identifies the technology stack from source text.
// E20: CRITICAL — must run BEFORE SemanticNormalizer (E15 destroys stack markers).
// Method: K-NN (K-Nearest Neighbours) on normalized marker frequency vector.
// Falls back to marker voting when training data is absent.
public sealed class StackDetector
{
    // Stack-specific tokens that survive E15 normalization
    private static readonly Dictionary<StackLabel, HashSet<string>> Markers = new()
    {
        [StackLabel.CSharp] = new(StringComparer.OrdinalIgnoreCase)
        {
            "task","ienumerable","cancellation","namespace","csharp","dotnet",
            "nullable","linq","delegate","struct","sealed","override",
            "partial","extern","unsafe","checked","stackalloc"
        },
        [StackLabel.TypeScript] = new(StringComparer.OrdinalIgnoreCase)
        {
            "promise","injectable","interface","export","readonly","enum",
            "typescript","nestjs","angular","rxjs","observable","decorator",
            "generics","union","intersection","keyof","typeof"
        },
        [StackLabel.Python] = new(StringComparer.OrdinalIgnoreCase)
        {
            "def","none","yield","tuple","dict","list","optional","typing",
            "python","django","flask","fastapi","pydantic","dataclass",
            "comprehension","generator","lambda","unpacking"
        },
    };

    private readonly List<(float[] Features, StackLabel Stack)> _train = [];
    private readonly int _k;

    public StackDetector(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(string Source, StackLabel Stack)> examples)
    {
        _train.Clear();
        foreach (var (src, stack) in examples)
            _train.Add((BuildFeatureVector(src), stack));
    }

    // Detect stack label from source text (raw, before normalization)
    public OperationResult<StackLabel> Detect(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return OperationResult<StackLabel>.Ok(StackLabel.Generic);

        if (_train.Count == 0)
            return OperationResult<StackLabel>.Ok(PredictByMarkers(source));

        var q = BuildFeatureVector(source);
        if (q.All(x => x == 0f))
            return OperationResult<StackLabel>.Ok(StackLabel.Generic);

        var best = _train
            .Where(t => t.Features.Any(x => x > 0f))
            .OrderBy(t => CosineDistance(q, t.Features))
            .Take(_k)
            .GroupBy(t => t.Stack)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return OperationResult<StackLabel>.Ok(best?.Key ?? StackLabel.Generic);
    }

    // Baseline: count markers per stack, return the stack with the most matches
    public static StackLabel PredictByMarkers(string source)
    {
        var words = source.ToLowerInvariant()
            .Split([' ', '\n', '\r', '\t', '.', '(', ')', '<', '>', '{', '}'],
                   StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var detectable = new[] { StackLabel.CSharp, StackLabel.TypeScript, StackLabel.Python };
        var scores = detectable.Select(s => Markers[s].Count(m => words.Contains(m))).ToArray();
        int maxScore = scores.Max();
        if (maxScore == 0) return StackLabel.Generic;
        return detectable[Array.IndexOf(scores, maxScore)];
    }

    private static float[] BuildFeatureVector(string source)
    {
        var words = source.ToLowerInvariant()
            .Split([' ', '\n', '\r', '\t', '.', '(', ')', '<', '>', '{', '}'],
                   StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        var detectable = new[] { StackLabel.CSharp, StackLabel.TypeScript, StackLabel.Python };
        return detectable.Select(s =>
            Markers[s].Count(m => words.Contains(m)) / (float)Markers[s].Count).ToArray();
    }

    private static double CosineDistance(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i];
        }
        double d = Math.Sqrt(na) * Math.Sqrt(nb);
        return d < 1e-9 ? 1.0 : 1.0 - dot / d;
    }
}
