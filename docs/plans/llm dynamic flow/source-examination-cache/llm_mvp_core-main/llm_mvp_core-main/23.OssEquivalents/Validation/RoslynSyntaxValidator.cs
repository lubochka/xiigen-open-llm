using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

namespace OssEquivalents;

// Finds C# syntax errors in string-valued fields of an LLM JSON response.
// Allows StructuredLlmClient to include precise error messages in retry prompts
// instead of generic "try again" instructions — avoiding an extra LLM round-trip.
// E10: StripNoise applied conditionally — only when noiseRatio > 0.25 (fixes Tier 2 regression).
public sealed class RoslynSyntaxValidator
{
    private static readonly Regex _lineComment  = new(@"//[^\n]*",             RegexOptions.Compiled);
    private static readonly Regex _blockComment = new(@"/\*.*?\*/",            RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex _strLiteral   = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);
    private static readonly Regex _whitespace   = new(@"\s+",                  RegexOptions.Compiled);

    // Return a list of "fieldKey: first_error_message" strings for every string field
    // in the response that looks like C# code but fails Roslyn's syntax check.
    public IReadOnlyList<string> FindInvalidCodeFields(Dictionary<string, object> llmResponse)
    {
        var invalid = new List<string>();

        foreach (var (key, value) in llmResponse)
        {
            if (value is not string str) continue;
            if (!LooksLikeCSharp(str))   continue;

            // E10 regression fix: only strip noise when the code is actually noisy
            double noiseRatio = EstimateNoiseRatio(str);
            string normalized = noiseRatio > 0.25 ? StripNoise(str) : str;

            var errors = CSharpSyntaxTree.ParseText(normalized)
                .GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (errors.Count > 0)
                invalid.Add($"{key}: {errors[0].GetMessage()}");
        }

        return invalid;
    }

    // Heuristic: string contains braces and at least one C# visibility keyword.
    private static bool LooksLikeCSharp(string s)
        => s.Contains('{') && s.Contains('}') &&
           (s.Contains("public ")  || s.Contains("private ") ||
            s.Contains("class ")   || s.Contains("return "));

    // Fraction of characters that are comments or string literals.
    internal static double EstimateNoiseRatio(string source)
    {
        if (source.Length == 0) return 0.0;
        int noise = _lineComment.Matches(source).Sum(m => m.Length)
                  + _blockComment.Matches(source).Sum(m => m.Length)
                  + _strLiteral.Matches(source).Sum(m => m.Length);
        return (double)noise / source.Length;
    }

    // Remove comments and string literals before Roslyn parse (E10: -21% chars, +31% speed).
    private static string StripNoise(string source)
    {
        source = _lineComment.Replace(source, " ");
        source = _blockComment.Replace(source, " ");
        source = _strLiteral.Replace(source, "\"\"");
        return  _whitespace.Replace(source, " ");
    }
}
