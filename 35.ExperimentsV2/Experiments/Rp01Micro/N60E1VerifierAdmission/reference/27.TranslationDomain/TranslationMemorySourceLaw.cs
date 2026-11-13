// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.RegularExpressions;

// This stateless admission constraint recognizes only isolated, unambiguous English copular clauses.
// It never generates a translation or replaces the trained adapter's final quality verifier.
internal static class TranslationMemorySourceLaw
{
    // Version the executable policy separately from the unchanged stored source/target schema.
    internal const string PolicyVersion = "en-simple-copular-polarity-v1";

    // Closed noun/adjective sets avoid guessing part-of-speech, referents, quotation, or negation scope.
    // Articles may differ because Russian memory targets do not encode English articles.
    private static readonly Regex SimpleClause = new(
        @"\A(?:the|a|an)[ \t]+(?<subject>box|door|file|room|house|window|container|document|folder|account|connection)[ \t]+(?<copula>is|was)[ \t]+(?<negative>not[ \t]+)?(?<predicate>empty|full|open|closed|ready|available|valid|invalid|enabled|disabled|safe|unsafe|null)[.!]?\z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Candidate-conditioned compatibility is UNKNOWN outside the exact supported clause/role/tense.
    internal static SourceCompatibility Compare(string? querySource, string? candidateSource)
    {
        // Keep the raw source until after scope recognition; destructive token normalization loses "not".
        var query = SimpleClause.Match((querySource ?? string.Empty).Trim());
        var candidate = SimpleClause.Match((candidateSource ?? string.Empty).Trim());
        if (!query.Success || !candidate.Success)
            return SourceCompatibility.Unknown;

        // Different subjects, predicates, or tenses are outside this one narrow contradiction proof.
        foreach (var role in new[] { "subject", "copula", "predicate" })
            if (!string.Equals(query.Groups[role].Value, candidate.Groups[role].Value, StringComparison.OrdinalIgnoreCase))
                return SourceCompatibility.Unknown;

        // Similarity cannot compensate for opposite polarity of the same supported source proposition.
        return query.Groups["negative"].Success == candidate.Groups["negative"].Success
            ? SourceCompatibility.Compatible
            : SourceCompatibility.Contradiction;
    }

    // Unknown is distinct from agreement and cannot authorize a hard veto.
    internal enum SourceCompatibility { Unknown, Compatible, Contradiction }
}
