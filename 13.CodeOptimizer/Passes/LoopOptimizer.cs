// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeOptimizer;

// Detects two loop anti-patterns:
//  1. .Count / .Length evaluated in a for-loop condition on every iteration.
//  2. Method calls with zero arguments inside a loop body that do not
//     reference the loop iteration variable.
// Source algorithm: LLVM LICM.cpp (Loop Invariant Code Motion, Apache 2.0).
public sealed class LoopOptimizer : IOptimizationPass
{
    public string PassName => "Loop Optimization — hoist loop-invariant computations";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree, SemanticModel semanticModel,
        string filePath, OptimizationContext context)
    {
        if (!context.EnableLoopOptimization)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root     = syntaxTree.GetRoot();

        foreach (var forLoop in root.DescendantNodes().OfType<ForStatementSyntax>())
        {
            FindCountInCondition(forLoop, filePath, findings);
            FindInvariantCallsInBody(forLoop, forLoop.Statement, filePath, findings);
        }

        foreach (var wl in root.DescendantNodes().OfType<WhileStatementSyntax>())
            FindInvariantCallsInBody(wl, wl.Statement, filePath, findings);

        foreach (var fel in root.DescendantNodes().OfType<ForEachStatementSyntax>())
            FindInvariantCallsInBody(fel, fel.Statement, filePath, findings);

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(
            findings.Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold).ToList());
    }

    private static void FindCountInCondition(
        ForStatementSyntax forLoop, string filePath,
        List<OptimizationFinding> findings)
    {
        if (forLoop.Condition is null) return;

        foreach (var ma in forLoop.Condition.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>()
            .Where(m => m.Name.Identifier.Text is "Count" or "Length"))
        {
            int    line     = forLoop.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            string original = ma.ToString().Trim();
            findings.Add(new OptimizationFinding
            {
                OptimizationType         = "Loop Invariant — Count/Length in loop condition",
                FilePath                 = filePath,
                LineNumber               = line,
                OriginalCode             = $"i < {original}",
                SuggestedReplacement     = $"int count = {original};\n// use 'count' in for condition",
                HumanReadableExplanation =
                    $"'{original}' is re-evaluated on every iteration. Cache it before the loop.",
                ConfidenceScore          = 0.85
            });
        }
    }

    private static void FindInvariantCallsInBody(
        SyntaxNode loop, StatementSyntax body,
        string filePath, List<OptimizationFinding> findings)
    {
        var loopVars = GetLoopVars(loop);

        foreach (var call in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.ArgumentList.Arguments.Count > 0) continue;

            var usedIds = call.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(id => id.Identifier.Text)
                .ToHashSet();

            if (usedIds.Any(id => loopVars.Contains(id))) continue;

            int    line     = call.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            string original = call.ToString().Trim();
            findings.Add(new OptimizationFinding
            {
                OptimizationType         = "Loop Invariant — no-arg call inside loop",
                FilePath                 = filePath,
                LineNumber               = line,
                OriginalCode             = original,
                SuggestedReplacement     = $"// Hoist '{original}' before the loop",
                HumanReadableExplanation =
                    $"'{original}' does not depend on the loop variable. Consider hoisting it.",
                ConfidenceScore          = 0.70
            });
        }
    }

    private static HashSet<string> GetLoopVars(SyntaxNode loop)
    {
        if (loop is ForStatementSyntax fs)
            return fs.Declaration?.Variables.Select(v => v.Identifier.Text).ToHashSet()
                   ?? [];
        if (loop is ForEachStatementSyntax fe)
            return [fe.Identifier.Text];
        return [];
    }
}
