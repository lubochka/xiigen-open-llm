using Microsoft.CodeAnalysis.CSharp;

namespace AgentAlgorithms;

// ITokenBudgetCalculator using AST node count for C# (E11: AST tokens 5.2× more info-dense).
// CSharpCode: Roslyn AST node count — one AST unit describes a full syntactic element.
// Documentation: text.Length / 5 — BPE vocabulary covers prose more efficiently.
// NaturalLanguage / Json: text.Length / 4 — standard approximation.
public sealed class AstTokenBudgetCalculator : ITokenBudgetCalculator
{
    private const double CSharpBytesPerAstNode = 20.0;

    // Simple formula-based estimate (one AST node per 20 characters, E11).
    // Use this for budget calculations. For actual Roslyn node count use EstimateAstTokens.
    public int Estimate(string content, ContentType type) => type switch
    {
        ContentType.CSharpCode      => (int)Math.Ceiling(content.Length / CSharpBytesPerAstNode),
        ContentType.Documentation   => (int)Math.Ceiling(content.Length / 5.0),
        ContentType.SkillDocument   => (int)Math.Ceiling(content.Length / 4.0),
        ContentType.Json            => (int)Math.Ceiling(content.Length / 4.0),
        _                           => (int)Math.Ceiling(content.Length / 4.0),
    };

    // Kept for backward compatibility with existing tests that call EstimateTokens directly.
    public int EstimateTokens(string content, ContentType type) => type switch
    {
        ContentType.CSharpCode    => EstimateAstTokens(content),
        ContentType.Documentation => (int)Math.Ceiling(content.Length / 5.0),
        ContentType.Json          => (int)Math.Ceiling(content.Length / 4.0),
        _                         => (int)Math.Ceiling(content.Length / 4.0),
    };

    public int EstimateAstTokens(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource)) return 0;
        try
        {
            return CSharpSyntaxTree.ParseText(csharpSource)
                .GetRoot()
                .DescendantNodes()
                .Count();
        }
        catch
        {
            return (int)Math.Ceiling(csharpSource.Length / 4.0);
        }
    }
}
