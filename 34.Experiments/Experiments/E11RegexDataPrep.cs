// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Experiments;

// ─── E11: 8 compiled regex patterns vs Roslyn AST parse — time benchmark ───────
//
// Hypothesis: 8 compiled regex patterns scan C# files ≥ 10× faster than
// Roslyn.ParseText while still detecting ≥ 80% of structural elements.
//
// Baseline: Roslyn CSharpSyntaxTree.ParseText per file → count AST nodes.
// Enhanced: 8 compiled regex → count structure matches.
// Metric (primary): time_pct_saved = (roslyn_ms – regex_ms) / roslyn_ms × 100.
// Pass condition:   time_pct_saved >= 90% AND coverage_pct >= 80%.
public sealed class E11RegexDataPrep
{
    private static readonly Regex MethodStart = new(
        @"\b(public|private|protected|internal)\s+[\w<>\[\],\s]+\s+\w+\s*\(", RegexOptions.Compiled);
    private static readonly Regex IfBlock     = new(@"\bif\s*\(",             RegexOptions.Compiled);
    private static readonly Regex ForLoop     = new(@"\bfor(?:each)?\s*\(",   RegexOptions.Compiled);
    private static readonly Regex WhileLoop   = new(@"\bwhile\s*\(",          RegexOptions.Compiled);
    private static readonly Regex ReturnStmt  = new(@"\breturn\b",            RegexOptions.Compiled);
    private static readonly Regex ClassDecl   = new(@"\bclass\s+\w+",         RegexOptions.Compiled);
    private static readonly Regex CallSite    = new(@"\b\w+\.\w+\s*\(",       RegexOptions.Compiled);
    private static readonly Regex Assignment  = new(@"\b\w+\s*=[^=]",         RegexOptions.Compiled);

    private static readonly Regex[] AllPatterns =
    [
        MethodStart, IfBlock, ForLoop, WhileLoop,
        ReturnStmt, ClassDecl, CallSite, Assignment,
    ];

    public OperationResult<ExperimentResult> Run(
        IReadOnlyDictionary<string, string> fileTexts,
        int scale = 1)
    {
        if (fileTexts.Count == 0)
            return OperationResult<ExperimentResult>.Fail("No files for E11");

        var files = fileTexts.Values.ToList();

        // ── Baseline: Roslyn parse ────────────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();

        long roslynStructures = 0;
        foreach (string text in files)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            // Count structure nodes: methods, ifs, fors, whiles, returns, classes, calls, assignments
            roslynStructures += CountRoslynStructures(tree);
        }
        double baselineMs    = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: 8 compiled regex ───────────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t1          = System.Diagnostics.Stopwatch.GetTimestamp();

        long regexStructures = 0;
        foreach (string text in files)
        {
            foreach (var rx in AllPatterns)
                regexStructures += rx.Matches(text).Count;
        }
        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        double timePctSaved  = baselineMs > 0
            ? (baselineMs - enhancedMs) / baselineMs * 100.0
            : 0;
        double coveragePct   = roslynStructures > 0
            ? Math.Min(100, regexStructures * 100.0 / roslynStructures)
            : 0;

        // Primary metric: time_pct_saved (higher = better, like accuracy)
        // Pass when time_pct_saved >= 90% AND coverage_pct >= 80%.
        bool customPassed = timePctSaved >= 90.0 && coveragePct >= 80.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E11",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = timePctSaved / 100.0,  // normalised to 0–1 for shared infra
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = files.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = timePctSaved / 100.0,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = files.Count
            },
            Notes = $"files={files.Count}, " +
                    $"roslynMs={baselineMs:F1}ms, regexMs={enhancedMs:F1}ms, " +
                    $"timeSaved={timePctSaved:F1}%, " +
                    $"roslynStructures={roslynStructures:N0}, regexMatches={regexStructures:N0}, " +
                    $"coverage={coveragePct:F1}%, " +
                    $"pass={customPassed}(need 90%time AND 80%cov)"
        });
    }

    private static long CountRoslynStructures(
        Microsoft.CodeAnalysis.SyntaxTree tree)
    {
        var root = tree.GetRoot();
        long count = 0;
        foreach (var node in root.DescendantNodes())
        {
            var kind = node.Kind();
            switch (kind)
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.LocalFunctionStatement:
                case SyntaxKind.IfStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.WhileStatement:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                    count++;
                    break;
            }
        }
        return count;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
