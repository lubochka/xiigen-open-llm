using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeOptimizer;

// Finds expressions that are computed more than once inside the same method
// and suggests extracting them into a local variable.
// Simplified GVN (Global Value Numbering) operating on text-normalized AST subexpressions.
public sealed class CommonSubexpressionEliminator : IOptimizationPass
{
    public string PassName => "Common Subexpression Elimination — compute repeated expressions once";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree, SemanticModel semanticModel,
        string filePath, OptimizationContext context)
    {
        if (!context.EnableCommonSubexpressionElimination)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root     = syntaxTree.GetRoot();

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var seen = new Dictionary<string, List<int>>();

            var candidates = method.DescendantNodes().OfType<ExpressionSyntax>()
                .Where(e => e is InvocationExpressionSyntax
                          || e is BinaryExpressionSyntax
                          || e is MemberAccessExpressionSyntax)
                .Where(e => e is not LiteralExpressionSyntax)
                .Where(e => e is not IdentifierNameSyntax);

            foreach (var expr in candidates)
            {
                string key  = Normalize(expr.ToString());
                int    line = expr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                if (!seen.ContainsKey(key)) seen[key] = [];
                seen[key].Add(line);
            }

            foreach (var (key, lines) in seen)
            {
                if (lines.Count < 2) continue;
                string varName = MakeVarName(key);
                findings.Add(new OptimizationFinding
                {
                    OptimizationType         = "Common Subexpression",
                    FilePath                 = filePath,
                    LineNumber               = lines[0],
                    OriginalCode             = key,
                    SuggestedReplacement     = $"var {varName} = {key};",
                    HumanReadableExplanation =
                        $"'{key}' is computed {lines.Count} times (lines {string.Join(", ", lines)}). " +
                        $"Cache the result in a local variable.",
                    ConfidenceScore          = 0.65
                });
            }
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(
            findings.Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold).ToList());
    }

    private static string Normalize(string s) =>
        string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string MakeVarName(string expr)
    {
        var word = expr.Split('.', '(', ')', ' ', '+', '-', '*', '/')
                       .LastOrDefault(w => w.Length > 0 && char.IsLetter(w[0])) ?? "cached";
        return char.ToLower(word[0]) + word[1..] + "Result";
    }
}
