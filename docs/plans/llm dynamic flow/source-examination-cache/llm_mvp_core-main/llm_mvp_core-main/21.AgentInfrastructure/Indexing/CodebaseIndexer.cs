using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AgentInfrastructure;

// ─── Symbol graph node ────────────────────────────────────────────────────────

// Represents one code symbol (class, method, field) extracted from a C# source file.
public sealed record SymbolNode(
    string Name,
    string Kind,        // "Class" | "Method" | "Field"
    string FilePath,
    int    LineNumber,
    IReadOnlyList<string> References); // names of symbols this one calls or uses

// ─── Symbol graph ─────────────────────────────────────────────────────────────

// In-memory graph of all symbols in a codebase and their relationships.
public sealed class SymbolGraph
{
    private readonly Dictionary<string, SymbolNode> _nodes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double>     _pageRank = new(StringComparer.Ordinal);

    public void AddNode(SymbolNode node) => _nodes[node.Name] = node;

    public IReadOnlyList<SymbolNode> AllNodes => _nodes.Values.ToList();

    // How many times is this symbol referenced by other symbols (in-degree).
    public int InDegree(string name)
        => _nodes.Values.Count(n => n.References.Contains(name));

    // Simplified PageRank score (pre-computed during index build).
    public double GetScore(string name)
        => _pageRank.TryGetValue(name, out var s) ? s : 0.0;

    // Run power-iteration PageRank on the symbol graph.
    // dampingFactor — probability of following a reference (standard value: 0.85).
    public void ComputePageRank(double dampingFactor = 0.85, int iterations = 20)
    {
        if (_nodes.Count == 0) return;

        double init = 1.0 / _nodes.Count;
        foreach (var name in _nodes.Keys)
            _pageRank[name] = init;

        for (int i = 0; i < iterations; i++)
        {
            var next = new Dictionary<string, double>(StringComparer.Ordinal);

            foreach (var node in _nodes.Values)
            {
                double sum = 0.0;
                // Accumulate rank from every node that references this one.
                foreach (var src in _nodes.Values)
                {
                    if (src.References.Contains(node.Name) && src.References.Count > 0)
                        sum += _pageRank[src.Name] / src.References.Count;
                }
                next[node.Name] = (1.0 - dampingFactor) / _nodes.Count + dampingFactor * sum;
            }

            foreach (var (k, v) in next)
                _pageRank[k] = v;
        }
    }
}

// ─── Stage 02 — Codebase indexer ─────────────────────────────────────────────

// Walks all .cs files in a directory, extracts symbols via Roslyn,
// builds a SymbolGraph, and computes PageRank scores so widely-called symbols rank higher.
public sealed class CodebaseIndexer
{
    private readonly ICodeReader    _codeReader;
    private readonly IStackDetector? _stackDetector;

    public CodebaseIndexer(ICodeReader codeReader, IStackDetector? stackDetector = null)
    {
        _codeReader     = codeReader;
        _stackDetector  = stackDetector;
    }

    public async Task<OperationResult<SymbolGraph>> IndexDirectoryAsync(
        string directoryPath,
        IProgress<int>? progressReporter = null)
    {
        var listResult = await _codeReader.ListFilesAsync(directoryPath, "*.cs");
        if (!listResult.IsSuccess)
            return OperationResult<SymbolGraph>.Fail(listResult.ErrorMessage, listResult.ErrorCode);

        var files = listResult.Value!;
        var graph  = new SymbolGraph();
        int processed = 0;

        foreach (var filePath in files)
        {
            var readResult = await _codeReader.ReadFileAsync(filePath);
            if (!readResult.IsSuccess) continue; // skip unreadable files

            // E20 Scenario B: detect stack BEFORE any normalization — preserves 72% more markers
            _stackDetector?.Detect(readResult.Value!);

            ExtractSymbols(filePath, readResult.Value!, graph);
            processed++;
            progressReporter?.Report(processed);
        }

        graph.ComputePageRank();
        return OperationResult<SymbolGraph>.Ok(graph);
    }

    // Parse one C# source file with Roslyn and add its symbols to the graph.
    private static void ExtractSymbols(string filePath, string source, SymbolGraph graph)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        // Extract class declarations.
        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            // References = methods declared by this class (not calls made inside them).
            // Bug fix: DescendantNodes().OfType<InvocationExpressionSyntax>() collected
            // call targets from method bodies, inflating out-degree and corrupting PageRank.
            var methods = cls.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(m => m.Identifier.Text)
                .ToList();

            graph.AddNode(new SymbolNode(
                cls.Identifier.Text,
                "Class",
                filePath,
                cls.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                methods));
        }

        // Extract method declarations.
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var calls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(inv => inv.Expression.ToString().Split('.').Last())
                .Distinct()
                .ToList();

            graph.AddNode(new SymbolNode(
                method.Identifier.Text,
                "Method",
                filePath,
                method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                calls));
        }
    }
}
