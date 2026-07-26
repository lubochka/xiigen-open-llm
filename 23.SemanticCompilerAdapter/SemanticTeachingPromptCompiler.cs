// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using System.Text.RegularExpressions;
using SemanticCompiler29;

// Canonical project-23 representation for prompts before they enter teaching or learned-unit lookup.
public sealed class SemanticTeachingPromptCompiler
{
    public const string CanonicalPredicateSeed =
        "SECURITY AUTHORIZATION API_CONTRACT ARCHITECTURE IMPLEMENT TEST QA UI DOCUMENT LEARNING";

    private readonly DevelopmentPromptCompiler _compiler;

    public SemanticTeachingPromptCompiler(DevelopmentPromptCompiler? compiler = null)
    {
        _compiler = compiler ?? new DevelopmentPromptCompiler();
    }

    public OperationResult<SemanticTeachingPromptRepresentation> Compile(string prompt)
    {
        var compiled = _compiler.Compile(prompt);
        if (!compiled.IsSuccess)
            return compiled.MapError<SemanticTeachingPromptRepresentation>();

        var canonicalText = BuildCanonicalPromptText(compiled.Value!);
        var tokenizerSeed = BuildTokenizerSeedText(compiled.Value!, canonicalText);
        return OperationResult<SemanticTeachingPromptRepresentation>.Ok(new SemanticTeachingPromptRepresentation(
            prompt?.Trim() ?? string.Empty,
            compiled.Value!,
            canonicalText,
            tokenizerSeed,
            TrainingVectorBuilder.Build(compiled.Value!)));
    }

    public static string BuildCanonicalPromptText(DevelopmentPromptCompilation compiled)
    {
        return string.Join(" ",
            string.Join(" ", compiled.DevelopmentPredicateIds),
            compiled.NormalizedText,
            string.Join(" ", compiled.PipelinePredicates),
            string.Join(" ", ExtractSubPhaseHints(compiled.Text).Select(subPhaseId => $"explicit_sub_phase:{subPhaseId}")),
            $"semantic_domain:{compiled.Domain}",
            $"semantic_stack:{compiled.StackLanguage}")
            .Trim();
    }

    public static string BuildTokenizerSeedText(DevelopmentPromptCompilation compiled, string? canonicalText = null)
    {
        return string.Join(" ",
            CanonicalPredicateSeed,
            canonicalText ?? BuildCanonicalPromptText(compiled),
            string.Join(" ", compiled.Segments.SelectMany(segment => segment.DevelopmentPredicateIds)),
            string.Join(" ", compiled.Segments.Select(segment => segment.Text)),
            "capability generated learned flow selected rejected")
            .Trim();
    }

    private static string[] ExtractSubPhaseHints(string text)
    {
        return Regex.Matches(text ?? string.Empty, @"\bsub[-\s]*phase\s+(?<id>\d+\.\d+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            .Select(match => match.Groups["id"].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
    }
}

public sealed record SemanticTeachingPromptRepresentation(
    string RawText,
    DevelopmentPromptCompilation Compilation,
    string CanonicalText,
    string TokenizerSeedText,
    float[] TeachingVector);
