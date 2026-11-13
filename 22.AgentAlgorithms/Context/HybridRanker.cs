// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Production source for E34V2: BM25 lexical evidence fused with KNN/vector evidence.
public sealed class HybridRanker
{
    private const double K1 = 1.5;
    private const double B = 0.75;

    private readonly double _bm25Weight;
    private readonly double _knnWeight;
    private readonly double _patternWeight;

    public HybridRanker(double bm25Weight = 0.45, double knnWeight = 0.35, double patternWeight = 0.20)
    {
        var total = Math.Max(0.0001, bm25Weight + knnWeight + patternWeight);
        _bm25Weight = bm25Weight / total;
        _knnWeight = knnWeight / total;
        _patternWeight = patternWeight / total;
    }

    public IReadOnlyList<HybridRankResult> Rank(string query, IReadOnlyList<HybridRankCandidate> candidates)
    {
        if (candidates.Count == 0)
            return [];

        var queryTerms = Tokenize(query);
        var queryPattern = ClassifyPattern(queryTerms);
        var candidateTerms = candidates
            .Select(candidate => (candidate, Terms: Tokenize(candidate.Text)))
            .ToArray();
        var docFrequency = BuildDocumentFrequency(candidateTerms.Select(row => row.Terms));
        var averageLength = Math.Max(1.0, candidateTerms.Average(row => row.Terms.Length));
        var bm25Scores = candidateTerms
            .Select(row => ComputeBm25(queryTerms, row.Terms, docFrequency, candidates.Count, averageLength))
            .ToArray();
        var maxBm25 = Math.Max(1e-9, bm25Scores.Max());
        var maxKnn = Math.Max(1e-9, candidates.Max(candidate => Math.Max(0.0, candidate.KnnScore)));

        return candidates
            .Select((candidate, index) =>
            {
                var normalizedBm25 = bm25Scores[index] / maxBm25;
                var normalizedKnn = Math.Max(0.0, candidate.KnnScore) / maxKnn;
                var patternScore = PatternScore(queryPattern, ClassifyPattern(candidateTerms[index].Terms));
                var fused = (_bm25Weight * normalizedBm25) + (_knnWeight * normalizedKnn) + (_patternWeight * patternScore);
                return new HybridRankResult(candidate.Id, normalizedBm25, normalizedKnn, fused);
            })
            .OrderByDescending(result => result.FusedScore)
            .ThenBy(result => result.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public static double ScoreOne(string query, string document, double knnScore = 0.0)
    {
        return new HybridRanker().Rank(query, [new HybridRankCandidate("candidate", document, knnScore)])
            .FirstOrDefault()?.FusedScore ?? 0.0;
    }

    private static Dictionary<string, int> BuildDocumentFrequency(IEnumerable<string[]> documents)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            foreach (var term in document.Distinct(StringComparer.Ordinal))
                result[term] = result.GetValueOrDefault(term) + 1;
        }

        return result;
    }

    private static double ComputeBm25(
        string[] queryTerms,
        string[] documentTerms,
        IReadOnlyDictionary<string, int> docFrequency,
        int documentCount,
        double averageLength)
    {
        if (queryTerms.Length == 0 || documentTerms.Length == 0)
            return 0.0;

        var termFrequency = documentTerms
            .GroupBy(term => term, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var length = documentTerms.Length;
        var score = 0.0;
        foreach (var term in queryTerms.Distinct(StringComparer.Ordinal))
        {
            if (!termFrequency.TryGetValue(term, out var frequency))
                continue;

            var df = docFrequency.TryGetValue(term, out var value) ? value : 0;
            var idf = Math.Log(((documentCount - df + 0.5) / (df + 0.5)) + 1.0);
            var norm = K1 * (1.0 - B + B * length / averageLength);
            score += idf * (frequency * (K1 + 1.0)) / (frequency + norm);
        }

        return score;
    }

    private static string[] Tokenize(string text)
    {
        return System.Text.RegularExpressions.Regex
            .Split((text ?? string.Empty).ToLowerInvariant(), @"[^a-z]+")
            .Where(word => word.Length >= 2 && !E15Stopwords.Contains(word))
            .Select(E15Stem)
            .Where(word => word.Length >= 2)
            .ToArray();
    }

    private static string E15Stem(string word)
    {
        string[] suffixes =
        [
            "isation",
            "ization",
            "nesses",
            "ments",
            "tions",
            "ings",
            "ness",
            "ment",
            "tion",
            "ing",
            "ful",
            "ous",
            "ive",
            "ed",
            "er",
            "ly"
        ];

        foreach (var suffix in suffixes)
            if (word.Length > suffix.Length + 3 &&
                word.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return word[..^suffix.Length];

        return word;
    }

    private static double PatternScore(string queryPattern, string documentPattern)
    {
        if (queryPattern == "UNKNOWN" || documentPattern == "UNKNOWN")
            return 0.0;

        return string.Equals(queryPattern, documentPattern, StringComparison.Ordinal)
            ? 1.0
            : 0.5;
    }

    private static string ClassifyPattern(string[] tokens)
    {
        if (tokens.Length == 0)
            return "UNKNOWN";

        var tags = tokens.Select(InferPos).ToArray();
        var firstVerb = Array.IndexOf(tags, Verb);
        var firstNoun = tags.Select((tag, index) => (tag, index))
            .FirstOrDefault(item => item.tag == Noun || item.tag == Pron)
            .index;

        if (firstVerb < 0)
            return "SVO";

        var verbFirst = firstVerb == 0 || (firstVerb == 1 && tags[0] == Structure);
        var nounBeforeVerb = firstNoun >= 0 && firstNoun < firstVerb;
        var verbLast = tags[^1] == Verb;

        if (verbFirst && !nounBeforeVerb) return "IMPERATIVE";
        if (verbLast && nounBeforeVerb) return "SOV";
        if (verbFirst && nounBeforeVerb) return "VSO";
        return "SVO";
    }

    private static int InferPos(string token)
    {
        if (E25ActionWords.Contains(token))
            return Verb;
        if (E25StructureWords.Contains(token))
            return Pron;
        return Noun;
    }

    private const int Pron = 3;
    private const int Noun = 7;
    private const int Verb = 8;
    private const int Structure = 10;

    private static readonly HashSet<string> E15Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","of","in","on","at","by","for","with","about","from","to",
        "into","through","is","are","was","were","be","been","being",
        "have","has","had","do","does","did","can","could","will","would",
        "shall","should","may","might","must","need","used","let","get","got",
        "i","we","it","its","and","or","but","not","no","as","if","so",
        "then","else","this","that","which","who","when","where","how","why"
    };

    private static readonly HashSet<string> E25ActionWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "add","fix","create","update","delete","validate","check","return","throw","send",
        "log","fetch","process","handle","run","build","set","remove","find","refactor",
        "implement","explain","show","list","plan","design"
    };

    private static readonly HashSet<string> E25StructureWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "this","that","which","who","where","when","how"
    };
}

public sealed record HybridRankCandidate(string Id, string Text, double KnnScore);

public sealed record HybridRankResult(string Id, double Bm25Score, double KnnScore, double FusedScore);
