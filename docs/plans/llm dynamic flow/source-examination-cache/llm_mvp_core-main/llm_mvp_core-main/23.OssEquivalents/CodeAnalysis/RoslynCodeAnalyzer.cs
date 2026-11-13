using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OssEquivalents;

// Extracts structural information from C# source files using Roslyn.
// Produces names, signatures, documentation, and relationships — the same data ts-morph
// extracts from TypeScript files in the OSS plan.
public sealed class RoslynCodeAnalyzer
{
    // Analyze all .cs files under the given directory.
    // Returns one dictionary per symbol (class or method) found.
    public async Task<OperationResult<List<Dictionary<string, object>>>> AnalyzeDirectoryAsync(
        string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return OperationResult<List<Dictionary<string, object>>>.Fail(
                $"Directory not found: {directoryPath}", "DIR_NOT_FOUND");

        var files  = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
        var result = new List<Dictionary<string, object>>();

        foreach (var filePath in files)
        {
            string source = await File.ReadAllTextAsync(filePath);
            ExtractSymbols(filePath, source, result);
        }

        return OperationResult<List<Dictionary<string, object>>>.Ok(result);
    }

    // Parse one source file and add its symbols to the result list.
    private static void ExtractSymbols(
        string filePath, string source, List<Dictionary<string, object>> result)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        // Extract class declarations.
        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            result.Add(new Dictionary<string, object>
            {
                ["name"]          = cls.Identifier.Text,
                ["kind"]          = "Class",
                ["signature"]     = cls.Identifier.Text,
                ["documentation"] = ExtractXmlDoc(cls),
                ["filePath"]      = filePath,
                ["calledBy"]      = new List<string>()
            });
        }

        // Extract method declarations — collect their call targets for edge building.
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var callees = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(inv => inv.Expression.ToString().Split('.').Last())
                .Distinct()
                .ToList();

            string sig = $"{method.ReturnType} {method.Identifier.Text}({method.ParameterList})";
            result.Add(new Dictionary<string, object>
            {
                ["name"]          = method.Identifier.Text,
                ["kind"]          = "Method",
                ["signature"]     = sig,
                ["documentation"] = ExtractXmlDoc(method),
                ["filePath"]      = filePath,
                ["calledBy"]      = new List<string>(),   // populated by CallGraphBuilder
                ["calls"]         = callees
            });
        }
    }

    // Extract the text of XML documentation comments preceding a node.
    private static string ExtractXmlDoc(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        return string.Join(" ", node.GetLeadingTrivia()
            .Where(t => t.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia)
            .Select(t => t.ToString().TrimStart('/', ' '))).Trim();
    }
}
