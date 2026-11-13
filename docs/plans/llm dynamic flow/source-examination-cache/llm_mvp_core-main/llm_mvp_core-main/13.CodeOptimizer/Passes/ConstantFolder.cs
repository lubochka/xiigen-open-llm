using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeOptimizer;

// Detects binary expressions consisting entirely of constant literals
// and suggests replacing them with the pre-computed result.
// Algorithm: ask Roslyn for the constant value of each BinaryExpression.
public sealed class ConstantFolder : IOptimizationPass
{
    public string PassName => "Constant Folding — replace constant expressions with their computed value";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree, SemanticModel semanticModel,
        string filePath, OptimizationContext context)
    {
        if (!context.EnableConstantFolding)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root     = syntaxTree.GetRoot();

        foreach (var binary in root.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            // Only flag expressions where both sides are literals (not variables)
            if (binary.Left  is not LiteralExpressionSyntax) continue;
            if (binary.Right is not LiteralExpressionSyntax) continue;

            var cv = semanticModel.GetConstantValue(binary);
            if (!cv.HasValue) continue;

            int    line        = binary.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            string original    = binary.ToString().Trim();
            string resultStr   = cv.Value?.ToString() ?? "null";
            string suggestion  = cv.Value is string ? $"\"{resultStr}\"" : resultStr;

            if (suggestion == original) continue;

            findings.Add(new OptimizationFinding
            {
                OptimizationType         = "Constant Folding",
                FilePath                 = filePath,
                LineNumber               = line,
                OriginalCode             = original,
                SuggestedReplacement     = suggestion,
                HumanReadableExplanation =
                    $"'{original}' always evaluates to {suggestion}. Replace with the literal.",
                ConfidenceScore          = 0.95
            });
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(
            findings.Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold).ToList());
    }
}
