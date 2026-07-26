// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using System.Text.RegularExpressions;

// Extracts expression prefix/completion pairs from complete expression rows.
public static class ExpressionCorpusBuilder
{
    public static IReadOnlyList<(string Partial, string Completion)> ExtractPairs(
        IReadOnlyList<string> completeExpressions)
    {
        var pairs = new List<(string Partial, string Completion)>();
        foreach (var expression in completeExpressions)
        {
            if (string.IsNullOrWhiteSpace(expression))
                continue;

            var separator = expression.LastIndexOf('=');
            if (separator < 0)
                continue;

            var left = expression[..separator].Trim();
            if (string.IsNullOrWhiteSpace(left))
                continue;

            var partial = expression[..(separator + 1)].Trim();
            var completion = expression[(separator + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(completion))
                continue;

            pairs.Add((partial, completion));
        }

        return pairs;
    }
}

// Exact learned-memory lookup for expression completion. This class owns all expression-domain logic.
public sealed class ExpressionCompletionCore
{
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex OperatorPad = new(@"\s*([+\-*/=])\s*", RegexOptions.Compiled);
    private static readonly Regex LeadingStars = new(@"^\*+", RegexOptions.Compiled);
    private static readonly Regex TrailingStars = new(@"\*+$", RegexOptions.Compiled);

    private readonly IReadOnlyDictionary<string, string> _lookup;

    private ExpressionCompletionCore(IReadOnlyDictionary<string, string> lookup)
    {
        _lookup = lookup;
    }

    public int Count => _lookup.Count;

    public static ExpressionCompletionCore? Train(IReadOnlyList<string> completeExpressions)
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(completeExpressions);
        if (pairs.Count == 0)
            return null;

        var lookup = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (partial, completion) in pairs)
            lookup[Normalise(partial)] = completion;

        return new ExpressionCompletionCore(lookup);
    }

    public static string Normalise(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var lookupPartial = ExtractLookupPartial(value);
        var spacedOperators = OperatorPad.Replace(lookupPartial, " $1 ");
        var collapsed = WhitespaceRun.Replace(spacedOperators.Trim(), " ");
        return collapsed.ToLowerInvariant();
    }

    private static string ExtractLookupPartial(string value)
    {
        var candidate = value.Trim();
        candidate = candidate.Trim('[', ']', '(', ')', '{', '}').Trim();
        candidate = LeadingStars.Replace(candidate, string.Empty).Trim();
        candidate = TrailingStars.Replace(candidate, string.Empty).Trim();

        var separator = candidate.LastIndexOf('=');
        return separator >= 0
            ? candidate[..(separator + 1)].Trim()
            : candidate;
    }

    public (bool Matched, string? Completion) Lookup(string partial)
    {
        if (string.IsNullOrWhiteSpace(partial))
            return (false, null);

        var key = Normalise(partial);
        return _lookup.TryGetValue(key, out var completion)
            ? (true, completion)
            : (false, null);
    }
}
