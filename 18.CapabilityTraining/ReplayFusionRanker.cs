// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using SemanticCompilerAdapter;

// Ranks replay manifests by relevance to the current compiled prompt using E34V2 BM25+KNN fusion.
public static class ReplayFusionRanker
{
    public static IReadOnlyList<GeneratedCapabilityCheckpointManifest> Rank(
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifests,
        string? compiledText,
        int topK = 3)
    {
        if (topK <= 0)
            return [];
        if (string.IsNullOrWhiteSpace(compiledText) || manifests.Count <= topK)
            return manifests.Take(topK).ToArray();

        var queryTokens = Tokenize(compiledText);
        return manifests
            .Select(manifest =>
            {
                var lexical = ComputeBm25Like(queryTokens, manifest.TrainingText, manifests);
                var vector = ComputePredicateOverlap(compiledText, manifest.TrainingText);
                var pattern = vector;
                var fused = (vector * 0.45) + (lexical * 0.35) + (pattern * 0.20);
                return new RankedManifest(manifest, fused);
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Manifest.CreatedAtUtc)
            .Take(topK)
            .Select(item => item.Manifest)
            .ToArray();
    }

    private static double ComputePredicateOverlap(string a, string b)
    {
        var aTokens = Tokenize(a);
        var bTokens = Tokenize(b);
        if (aTokens.Count == 0 || bTokens.Count == 0)
            return 0.0;

        var intersection = aTokens.Keys.Count(bTokens.ContainsKey);
        var union = aTokens.Count + bTokens.Count - intersection;
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static double ComputeBm25Like(
        IReadOnlyDictionary<string, int> queryTokens,
        string document,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> corpus)
    {
        var documentTokens = Tokenize(document);
        if (queryTokens.Count == 0 || documentTokens.Count == 0)
            return 0.0;

        const double k1 = 1.2;
        const double b = 0.75;
        var documentLength = Math.Max(1, documentTokens.Values.Sum());
        var averageLength = Math.Max(1.0, corpus.Select(item => Tokenize(item.TrainingText).Values.Sum()).DefaultIfEmpty(1).Average());
        var score = 0.0;
        foreach (var token in queryTokens.Keys)
        {
            if (!documentTokens.TryGetValue(token, out var frequency))
                continue;

            var documentFrequency = Math.Max(1, corpus.Count(item => Tokenize(item.TrainingText).ContainsKey(token)));
            var idf = Math.Log(1.0 + ((corpus.Count - documentFrequency + 0.5) / (documentFrequency + 0.5)));
            var numerator = frequency * (k1 + 1.0);
            var denominator = frequency + (k1 * (1.0 - b + (b * documentLength / averageLength)));
            score += idf * numerator / Math.Max(1.0e-9, denominator);
        }

        return score;
    }

    private static Dictionary<string, int> Tokenize(string text)
    {
        var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in TrainingVectorBuilder.NormalizeRoots(text ?? string.Empty).Where(token => token.Length > 1))
            tokens[token] = tokens.TryGetValue(token, out var count) ? count + 1 : 1;
        return tokens;
    }

    private sealed record RankedManifest(GeneratedCapabilityCheckpointManifest Manifest, double Score);
}
