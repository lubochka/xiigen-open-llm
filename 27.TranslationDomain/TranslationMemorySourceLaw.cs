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

    // Version the opt-in English source polarity field separately: the legacy copular policy
    // identity above stays distinguishable, because the legacy decision is still the default.
    internal const string EnglishSourcePolarityFieldVersion = "en-source-polarity-field-v2";

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

    // Opt-in overload: the English source polarity field runs BEFORE the unchanged legacy rule.
    // The flag is last and defaults to false, so every existing caller keeps the legacy decision.
    internal static SourceCompatibility Compare(
        string? querySource,
        string? candidateSource,
        bool enableEnglishPolarityField = false)
    {
        // Not opted in: return exactly the legacy two-argument decision, unchanged.
        if (!enableEnglishPolarityField)
            return Compare(querySource, candidateSource);

        // The polarity sign is read ONLY from the English source side of both texts.
        // It must never be applied to a Russian memory target: "не" and "никогда" are absent
        // from the word lists below, so any Russian text would be classified as positive.
        var querySign = EnglishSourcePolarityField.ClassifyEnglishSource(querySource);
        var candidateSign = EnglishSourcePolarityField.ClassifyEnglishSource(candidateSource);

        // Two or more negation signals need a contextual reference: no veto, legacy decides.
        // Without this branch a double negation would start blocking lawful candidates.
        if (querySign == EnglishSourcePolaritySign.Ambiguous ||
            candidateSign == EnglishSourcePolaritySign.Ambiguous)
            return Compare(querySource, candidateSource);

        // A different sign for the same action is a contradiction: the candidate is not admitted.
        // Always-negation and plain negation count as DIFFERENT signs on purpose.
        if (querySign != candidateSign)
            return SourceCompatibility.Contradiction;

        // The sign agrees, so nothing else changes: the unchanged legacy rule decides.
        return Compare(querySource, candidateSource);
    }

    // The four possible polarity signs of one English source utterance.
    private enum EnglishSourcePolaritySign
    {
        // No negation signal at all.
        Positive,
        // Plain negation: not / cannot / can't / no.
        NegationSimple,
        // Always-negation: never / nothing / nobody / nowhere / none.
        NegationAlways,
        // Two or more negation signals: a double negation needs a contextual reference.
        Ambiguous
    }

    // Closed, source-side-only polarity classifier. It is private to this law on purpose:
    // nothing outside may reuse it as a general theory of negation or on a target text.
    private static class EnglishSourcePolarityField
    {
        // Word split keeps an apostrophe INSIDE a word so "can't" stays one token.
        private static readonly Regex WordRegex = new(@"[\p{L}\p{N}']+", RegexOptions.Compiled);

        // Contracted negative forms are expanded to their full spelling before counting.
        // Both the apostrophe and the bare spelling are listed, because real input carries both.
        // Having an entry here is not corpus qualification of that entry: only four phrases are proven.
        private static readonly Dictionary<string, string> Contractions = new(StringComparer.OrdinalIgnoreCase)
        {
            // "cant" and "can't" mean "cannot" — this is the exact case the transfer closes.
            ["cant"] = "cannot", ["can't"] = "cannot",
            // "dont" and "don't" mean "do not".
            ["dont"] = "do not", ["don't"] = "do not",
            // "doesnt" and "doesn't" mean "does not".
            ["doesnt"] = "does not", ["doesn't"] = "does not",
            // "didnt" and "didn't" mean "did not".
            ["didnt"] = "did not", ["didn't"] = "did not",
            // "isnt" and "isn't" mean "is not".
            ["isnt"] = "is not", ["isn't"] = "is not",
            // "arent" and "aren't" mean "are not".
            ["arent"] = "are not", ["aren't"] = "are not",
            // "wasnt" and "wasn't" mean "was not".
            ["wasnt"] = "was not", ["wasn't"] = "was not",
            // "werent" and "weren't" mean "were not".
            ["werent"] = "were not", ["weren't"] = "were not",
            // "wont" and "won't" mean "will not".
            ["wont"] = "will not", ["won't"] = "will not",
            // "havent" and "haven't" mean "have not".
            ["havent"] = "have not", ["haven't"] = "have not",
            // "hasnt" and "hasn't" mean "has not".
            ["hasnt"] = "has not", ["hasn't"] = "has not",
            // "hadnt" and "hadn't" mean "had not".
            ["hadnt"] = "had not", ["hadn't"] = "had not",
            // "couldnt" and "couldn't" mean "could not".
            ["couldnt"] = "could not", ["couldn't"] = "could not",
            // "shouldnt" and "shouldn't" mean "should not".
            ["shouldnt"] = "should not", ["shouldn't"] = "should not",
            // "wouldnt" and "wouldn't" mean "would not".
            ["wouldnt"] = "would not", ["wouldn't"] = "would not"
        };

        // Always-negation signals: a plain "not" is not an acceptable substitute for them.
        private static readonly HashSet<string> AlwaysNegationWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // "never", "nothing", "nobody", "nowhere" and "none" negate the whole habit, not one act.
            "never", "nothing", "nobody", "nowhere", "none"
        };

        // Plain negation signals.
        private static readonly HashSet<string> SimpleNegationWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // "not", "cannot" and "no" negate one concrete statement.
            "not", "cannot", "no"
        };

        // ClassifyEnglishSource computes the polarity sign of ONE English source text.
        internal static EnglishSourcePolaritySign ClassifyEnglishSource(string? englishSourceText)
        {
            // A typographic apostrophe is folded to the plain one, otherwise "can’t" is not recognized.
            var normalized = (englishSourceText ?? string.Empty).Replace('’', '\'');

            // Words are split out and contractions expanded into their full forms.
            var words = new List<string>();
            foreach (Match match in WordRegex.Matches(normalized))
            {
                // One raw word of the source text.
                var word = match.Value;
                // A known contraction contributes its expanded words instead of itself.
                if (Contractions.TryGetValue(word, out var expanded))
                    words.AddRange(expanded.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                else
                    words.Add(word);
            }

            // Each kind of signal is counted separately.
            var always = words.Count(word => AlwaysNegationWords.Contains(word));
            var simple = words.Count(word => SimpleNegationWords.Contains(word));
            var total = always + simple;

            // No signal at all means a positive utterance.
            if (total == 0) return EnglishSourcePolaritySign.Positive;
            // Two or more signals mean a double negation, which this field does not decide.
            if (total > 1) return EnglishSourcePolaritySign.Ambiguous;
            // Exactly one signal decides which kind of negation this is.
            return always == 1
                ? EnglishSourcePolaritySign.NegationAlways
                : EnglishSourcePolaritySign.NegationSimple;
        }
    }

    // Unknown is distinct from agreement and cannot authorize a hard veto.
    internal enum SourceCompatibility { Unknown, Compatible, Contradiction }
}
