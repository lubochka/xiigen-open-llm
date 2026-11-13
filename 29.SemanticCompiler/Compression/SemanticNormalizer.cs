// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// SemanticNormalizer: stemming and stop-word removal.
// E15: +9.7% — reduces vocabulary noise before predicate extraction.
// Runs AFTER StackDetector (E20 Rule A): stack markers already captured before this stage.
public sealed class SemanticNormalizer : BaseService, ISemanticNormalizer29
{
    // Standard constructor: inject storage and logger without constructing them here.
    public SemanticNormalizer(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Common English stop words that carry no semantic value in code analysis.
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","of","in","on","at","by","for","with","about","from","to",
        "into","through","is","are","was","were","be","been","being",
        "have","has","had","do","does","did","can","could","will","would",
        "shall","should","may","might","must","need","used","let","get","got",
        "i","we","it","its","and","or","but","not","no","as","if","so",
        "then","else","this","that","which","who","when","where","how","why",
    };

    // Normalize text: split on non-alpha, lowercase, remove stop words, stem.
    public string[] Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        return System.Text.RegularExpressions.Regex
            .Split(text.ToLowerInvariant(), @"[^a-z]+")
            .Where(w => w.Length >= 2 && !Stopwords.Contains(w))
            .Select(Stem)
            .Where(w => w.Length >= 2)
            .ToArray();
    }

    // Dictionary-free suffix stemmer (Rule 10 — no external ML frameworks).
    // Strips common English suffixes while preserving the root meaning.
    private static string Stem(string word)
    {
        string[] suffixes = ["isation","ization","nesses","ments","tions","ings",
                             "ness","ment","tion","ing","ful","ous","ive","ed","er","ly"];
        foreach (var s in suffixes)
            if (word.Length > s.Length + 3
                && word.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                return word[..^s.Length];
        return word;
    }
}
