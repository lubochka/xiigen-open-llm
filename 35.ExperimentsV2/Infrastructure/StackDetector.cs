// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// StackDetector identifies the technology stack from stack-specific tokens
// that survive E15 normalization.
public sealed class StackDetector
{
    public const int CSHARP     = 0;
    public const int TYPESCRIPT = 1;
    public const int PYTHON     = 2;
    public const int GENERIC    = 3;

    public static readonly string[] StackNames =
        ["CSharp", "TypeScript", "Python", "Generic"];

    // Stack-specific markers that E15 does NOT remove
    private static readonly Dictionary<int, HashSet<string>> Markers = new()
    {
        [CSHARP] = new(StringComparer.OrdinalIgnoreCase)
        {
            "task","ienumerable","cancellation","namespace","csharp","dotnet",
            "nullable","linq","delegate","struct","sealed","override",
            "partial","extern","unsafe","checked","stackalloc"
        },
        [TYPESCRIPT] = new(StringComparer.OrdinalIgnoreCase)
        {
            "promise","injectable","interface","export","readonly","enum",
            "typescript","nestjs","angular","rxjs","observable","decorator",
            "generics","union","intersection","keyof","typeof"
        },
        [PYTHON] = new(StringComparer.OrdinalIgnoreCase)
        {
            "def","none","yield","tuple","dict","list","optional","typing",
            "python","django","flask","fastapi","pydantic","dataclass",
            "comprehension","generator","lambda","unpacking"
        },
    };

    private readonly List<(float[] Features, int Stack)> _train = new();
    private readonly int _k;

    public StackDetector(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(string NormalizedText, int Stack)> examples)
    {
        _train.Clear();
        foreach (var (text, stack) in examples)
            _train.Add((BuildFeatureVector(text), stack));
    }

    // Enhanced: K-NN on marker frequency feature vector
    public int Predict(string normalizedText)
    {
        if (_train.Count == 0) return GENERIC;
        var q = BuildFeatureVector(normalizedText);
        // Zero vector → no stack markers detected → GENERIC (cosine distance is
        // undefined for the zero vector; the baseline correctly returns GENERIC here).
        if (q.All(x => x == 0f)) return GENERIC;
        return _train
            .Where(t => t.Features.Any(x => x > 0f))  // skip GENERIC training examples
            .OrderBy(t => CosineDistance(q, t.Features))
            .Take(_k)
            .GroupBy(t => t.Stack)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: count markers per stack, take maximum
    public static int PredictByMarkers(string normalizedText)
    {
        var words = normalizedText.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var scores = new int[4];
        for (int s = 0; s < 3; s++)
            scores[s] = Markers[s].Count(m => words.Contains(m));

        int maxScore = scores[..3].Max();
        if (maxScore == 0) return GENERIC;
        return Array.IndexOf(scores[..3], maxScore);
    }

    // Generate synthetic code examples for each stack
    public static List<(string Code, int Stack)> GenerateSyntheticExamples(int seed, int perStack)
    {
        var rng     = new Random(seed);
        var result  = new List<(string, int)>();
        var methods = new[] { "GetUser", "ValidateToken", "ProcessPayment", "BuildFilter" };
        var types   = new[] { "User", "Token", "Payment", "Result" };

        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"namespace App {{ sealed class {t}Service {{ async Task<{t}?> {m}(IEnumerable<int> ids, CancellationToken ct) {{ if (ids == null) return null; return await _repo.GetAsync(ids, ct); }} }} }}", CSHARP));
        }
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"@Injectable() export class {t}Service {{ async {m}(ids: readonly string[]): Promise<{t} | undefined> {{ if (!ids) return undefined; return await this.repo.findOne(ids); }} }}", TYPESCRIPT));
        }
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"from typing import Optional, List\nclass {t}Service:\n    async def {m}(self, ids: List[int]) -> Optional[{t}]:\n        if not ids:\n            return None\n        return await self.repo.get(ids)", PYTHON));
        }
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"class {t}Service {{ method {m}(ids) {{ if ids == null return null result = repo.get(ids) return result }} }}", GENERIC));
        }
        // Shuffle so that the 80/20 split gets a balanced mix of all four stacks,
        // not just the last batch (which would be all GENERIC with ordered data).
        return result.OrderBy(_ => rng.Next()).ToList();
    }

    // Normalized marker frequencies per stack (3-element vector)
    private static float[] BuildFeatureVector(string normalizedText)
    {
        var words  = normalizedText.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        var result = new float[3];
        for (int s = 0; s < 3; s++)
            result[s] = Markers[s].Count(m => words.Contains(m)) / (float)Markers[s].Count;
        return result;
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
