// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace CodeOptimizer;

// Detects two categories of dead code:
//  1. Statements that follow an unconditional return/throw/break/continue
//     (unreachable-after-jump).
//  2. Assignments to local variables whose values are never read
//     (detected via Roslyn DataFlow analysis).
public sealed class DeadCodeEliminator : IOptimizationPass
{
    public string PassName => "Dead Code Elimination — unreachable statements and dead assignments";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree, SemanticModel semanticModel,
        string filePath, OptimizationContext context)
    {
        if (!context.EnableDeadCodeDetection)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root     = syntaxTree.GetRoot();

        // ── Pass 1: statements after unconditional jumps ──────────────────

        foreach (var block in root.DescendantNodes().OfType<BlockSyntax>())
        {
            bool afterJump = false;
            foreach (var stmt in block.Statements)
            {
                if (afterJump && stmt is not LocalFunctionStatementSyntax)
                {
                    int line = stmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    findings.Add(new OptimizationFinding
                    {
                        OptimizationType         = "Dead Code — unreachable after jump",
                        FilePath                 = filePath,
                        LineNumber               = line,
                        OriginalCode             = Truncate(stmt.ToString().Trim()),
                        SuggestedReplacement     = null,
                        HumanReadableExplanation = $"Line {line} is unreachable (follows return/throw/break/continue).",
                        ConfidenceScore          = 0.92
                    });
                }

                afterJump = stmt is ReturnStatementSyntax
                         || stmt is ThrowStatementSyntax
                         || stmt is BreakStatementSyntax
                         || stmt is ContinueStatementSyntax;
            }
        }

        // ── Pass 2: dead assignments (Roslyn DataFlow) ────────────────────

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.Body is null) continue;
            try
            {
                var df = semanticModel.AnalyzeDataFlow(method.Body);
                if (df is null || !df.Succeeded) continue;

                // Symbols assigned inside but whose value is never read inside or outside
                var deadStores = df.WrittenInside
                    .Except(df.ReadInside)
                    .Except(df.ReadOutside)
                    .Except(df.AlwaysAssigned.Where(s => df.ReadOutside.Contains(s)));

                foreach (var sym in deadStores)
                {
                    // Find the assignment expression for accurate line number
                    foreach (var assign in method.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>())
                    {
                        var assignedSym = semanticModel.GetSymbolInfo(assign.Left).Symbol;
                        if (!SymbolEqualityComparer.Default.Equals(assignedSym, sym)) continue;

                        int line = assign.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        findings.Add(new OptimizationFinding
                        {
                            OptimizationType         = "Dead Assignment — value never read",
                            FilePath                 = filePath,
                            LineNumber               = line,
                            OriginalCode             = Truncate(assign.ToString().Trim()),
                            SuggestedReplacement     = null,
                            HumanReadableExplanation =
                                $"Variable '{sym.Name}' is assigned but its value is never used.",
                            ConfidenceScore          = 0.80
                        });
                        break;
                    }
                }
            }
            catch { /* skip methods with complex control flow */ }
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(
            findings.Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold).ToList());
    }

    private static string Truncate(string s) =>
        s.Length > 100 ? s[..100] + "..." : s;
}
