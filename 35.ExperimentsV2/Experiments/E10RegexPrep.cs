// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler;

namespace ExperimentsV2;

// E10: измеряет ускорение Roslyn-разбора после регекс-нормализации.
// Baseline: Roslyn.ParseText(rawSource)
// Enhanced: Roslyn.ParseText(RegexPreprocessor.Normalize(rawSource))
// Метрика: время разбора всего датасета в миллисекундах.
public sealed class E10RegexPrep
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("Нет файлов для E10");

        // Шаг 1: проверить что нормализация не ломает синтаксис
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
                $"Нормализация сломала синтаксис в {syntaxErrors} файлах — проверь RegexPreprocessor");

        // Шаг 2: измерить baseline — Roslyn на сырых файлах
        var t0 = Stopwatch();
        int baselineNodes = 0;
        foreach (var (_, text) in files)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            baselineNodes += tree.GetRoot().DescendantNodes().Count();
        }
        double baselineMs = ElapsedMs(t0);

        // Шаг 3: измерить enhanced — регекс + Roslyn
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
            ScalingSignalNote = "char_reduction_pct (сколько % символов убрал регекс)",
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
