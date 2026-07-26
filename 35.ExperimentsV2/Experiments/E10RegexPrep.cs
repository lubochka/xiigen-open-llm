// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler;

namespace ExperimentsV2;

// E10: measures the Roslyn parsing speedup after regex normalization.
// Baseline: Roslyn.ParseText(rawSource)
// Enhanced: Roslyn.ParseText(RegexPreprocessor.Normalize(rawSource))
// Metric: parse time for the whole dataset in milliseconds.
public sealed class E10RegexPrep
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No files for E10");

        // Step 1: verify that normalization does not break syntax
        int syntaxErrors = 0;
        foreach (var (_, text) in files)
        {
            string normalized = RegexPreprocessor.Normalize(text);
            var tree = CSharpSyntaxTree.ParseText(normalized);
            bool hasErrors = tree.GetDiagnostics()
                .Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            if (hasErrors) syntaxErrors++;
        }

        if (syntaxErrors > 0)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Normalization broke syntax in {syntaxErrors} files — check RegexPreprocessor");

        // Step 2: measure baseline — Roslyn on raw files
        var t0 = Stopwatch();
        int baselineNodes = 0;
        foreach (var (_, text) in files)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            baselineNodes += tree.GetRoot().DescendantNodes().Count();
        }
        double baselineMs = ElapsedMs(t0);

        // Step 3: measure enhanced — regex + Roslyn
        var t1 = Stopwatch();
        int enhancedNodes   = 0;
        int totalCharsRaw   = 0;
        int totalCharsNorm  = 0;
        foreach (var (_, text) in files)
        {
            totalCharsRaw += text.Length;
            string normalized = RegexPreprocessor.Normalize(text);
            totalCharsNorm += normalized.Length;
            var tree = CSharpSyntaxTree.ParseText(normalized);
            enhancedNodes += tree.GetRoot().DescendantNodes().Count();
        }
        double enhancedMs = ElapsedMs(t1);

        double charReductionPct = totalCharsRaw > 0
            ? (1.0 - (double)totalCharsNorm / totalCharsRaw) * 100.0
            : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E10-REGEX-PREP",
            Tier              = tier,
            BaselinePrimary   = baselineMs,
            EnhancedPrimary   = enhancedMs,
            PrimaryMetricName = "parse_time_ms",
            LowerIsBetter     = true,
            ScalingSignal     = charReductionPct,
            ScalingSignalNote = "char_reduction_pct (what % of characters the regex removed)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"files={files.Count}, " +
                                $"charsRaw={totalCharsRaw:N0}, " +
                                $"charsNorm={totalCharsNorm:N0}, " +
                                $"reduction={charReductionPct:F1}%, " +
                                $"nodesBaseline={baselineNodes}, " +
                                $"nodesEnhanced={enhancedNodes}, " +
                                $"syntaxErrors={syntaxErrors}"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
