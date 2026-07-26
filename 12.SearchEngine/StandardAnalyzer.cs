// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.RegularExpressions;

namespace SearchEngine;

// Tokenises, lowercases, removes stop-words, and Porter-stems input text.
public sealed class StandardAnalyzer : IAnalyzer
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","and","or","but","in","on","at","to","for","of","with",
        "is","it","its","be","are","was","were","has","have","had","not","this",
        "that","by","from","as","if","do","no","so","up","he","she","we","they",
        "i","my","you","your","we","us","our"
    };

    private static readonly Regex TokenPattern = new(@"[a-zA-Z0-9]+", RegexOptions.Compiled);
    private readonly PorterStemmer _stemmer = new();

    public IReadOnlyList<AnalyzedToken> Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var tokens = new List<AnalyzedToken>();
        int position = 0;

        foreach (Match m in TokenPattern.Matches(text))
        {
            string lower = m.Value.ToLowerInvariant();
            if (StopWords.Contains(lower)) continue;
            string term = _stemmer.Stem(lower);
            if (term.Length == 0) continue;
            tokens.Add(new AnalyzedToken(term, position++));
        }

        return tokens;
    }
}
