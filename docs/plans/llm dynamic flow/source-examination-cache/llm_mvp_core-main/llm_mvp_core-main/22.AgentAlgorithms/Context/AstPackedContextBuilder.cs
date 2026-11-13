using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentAlgorithms;

// Compresses a C# source file into a compact AST summary for inclusion in LLM context.
// Each method becomes one line: "fn ReturnType Name(params) [BodyNodeKinds]"
//
// Processing order (v2.0, per E06/E10/E11 results):
//   1. StripNoise  — strip comments + string literals (-21% chars, E10 Regex)
//   2. Roslyn parse — runs 31% faster on cleaned text (E10: +31.2%)
//   3. AST summary — 7.8× fewer tokens than raw source (E06 Scale 3: 93.5% accuracy)
// Same token budget → 7.8× more methods described → LLM sees whole project, not first 50 files.
public sealed class AstPackedContextBuilder
{
    private static readonly Regex _lineComment  = new(@"//[^\n]*",             RegexOptions.Compiled);
    private static readonly Regex _blockComment = new(@"/\*.*?\*/",            RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex _strLiteral   = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);
    private static readonly Regex _whitespace   = new(@"\s+",                  RegexOptions.Compiled);

    // Compress csharpSource to a one-method-per-line AST summary.
    public string Pack(string csharpSource, string filePath)
    {
        // Step 1: strip noise conditionally — only when file is noisy enough to justify the
        // overhead (E10 regression fix: Regex slows clean code by 31% at Tier 2).
        double noiseRatio = EstimateNoiseRatio(csharpSource);
        string normalized = noiseRatio > 0.25 ? StripNoise(csharpSource) : csharpSource;

        // Step 2: parse cleaned source
        var root = CSharpSyntaxTree.ParseText(normalized).GetRoot();
        var sb   = new StringBuilder();

        sb.AppendLine($"// {Path.GetFileName(filePath)}");

        // Step 3: one line per method
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            string sig = $"{method.ReturnType} {method.Identifier}" +
                         $"({string.Join(",", method.ParameterList.Parameters.Select(p => p.Type))})";

            var bodyKinds = method.Body?
                .DescendantNodes()
                .Select(n => Abbreviate(n.Kind().ToString()))
                .Distinct()
                .ToList() ?? [];

            sb.AppendLine($"  fn {sig} [{string.Join(" ", bodyKinds)}]");
        }

        return sb.ToString();
    }

    // Ratio of original char count to packed char count (higher = more compression).
    public double CompressionRatio(string original, string packed)
        => packed.Length > 0 ? (double)original.Length / packed.Length : 1.0;

    // Fraction of characters that are comments or string literals.
    // Used to decide whether StripNoise is worth the overhead (threshold: 0.25).
    public static double EstimateNoiseRatio(string source)
    {
        if (source.Length == 0) return 0.0;
        int noise = _lineComment.Matches(source).Sum(m => m.Length)
                  + _blockComment.Matches(source).Sum(m => m.Length)
                  + _strLiteral.Matches(source).Sum(m => m.Length);
        return (double)noise / source.Length;
    }

    // Remove comments and string literals before Roslyn parse (E10: -21% chars, +31% speed).
    internal static string StripNoise(string source)
    {
        source = _lineComment.Replace(source, " ");
        source = _blockComment.Replace(source, " ");
        source = _strLiteral.Replace(source, "\"\"");
        return  _whitespace.Replace(source, " ");
    }

    private static string Abbreviate(string kind) => kind switch
    {
        "MethodDeclaration"          => "MDecl",
        "LocalFunctionStatement"     => "LocalFn",
        "IfStatement"                => "If",
        "ReturnStatement"            => "Ret",
        "ForStatement"               => "For",
        "ForEachStatement"           => "ForEach",
        "WhileStatement"             => "While",
        "InvocationExpression"       => "Call",
        "SimpleAssignmentExpression" => "Assign",
        "ThrowStatement"             => "Throw",
        "TryStatement"               => "Try",
        "SwitchStatement"            => "Switch",
        _ => kind.Length > 6 ? kind[..6] : kind,
    };
}
