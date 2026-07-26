// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeOptimizer;

// Replaces multiplication by a power-of-two constant with a left-shift.
// x * 2 → (x << 1), x * 4 → (x << 2), etc.
// Source algorithm: LLVM StrengthReductionPass.cpp (Apache 2.0).
public sealed class StrengthReducer : IOptimizationPass
{
    public string PassName => "Strength Reduction — replace x*2^n with x<<n";

    private static readonly Dictionary<long, int> PowTwoShift = new()
    {
        {2,1},{4,2},{8,3},{16,4},{32,5},{64,6},{128,7},{256,8},{512,9},{1024,10}
    };

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree, SemanticModel semanticModel,
        string filePath, OptimizationContext context)
    {
        if (!context.EnableStrengthReduction)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root     = syntaxTree.GetRoot();

        foreach (var mult in root.DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Where(e => e.IsKind(SyntaxKind.MultiplyExpression)))
        {
            var rcv = semanticModel.GetConstantValue(mult.Right);
            if (!rcv.HasValue) continue;

            long? multiplier = rcv.Value switch
            {
                int  i => i,
                long l => l,
                _      => null
            };
            if (multiplier is null || !PowTwoShift.TryGetValue(multiplier.Value, out int shift))
                continue;

            int    line       = mult.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            string left       = mult.Left.ToString().Trim();
            string original   = mult.ToString().Trim();
            string suggestion = $"({left} << {shift})";

            findings.Add(new OptimizationFinding
            {
                OptimizationType         = "Strength Reduction",
                FilePath                 = filePath,
                LineNumber               = line,
                OriginalCode             = original,
                SuggestedReplacement     = suggestion,
                HumanReadableExplanation =
                    $"'{original}' can be replaced by '{suggestion}' — bit-shift is faster than multiplication.",
                ConfidenceScore          = 0.80
            });
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(
            findings.Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold).ToList());
    }
}
