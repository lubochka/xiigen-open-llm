using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeOptimizer;

// Composes all passes into a single analysis run over a file or directory.
public sealed class OptimizationPipeline
{
    private readonly IReadOnlyList<IOptimizationPass> _passes;
    private readonly ICodeReader                       _reader;
    private readonly IOptimizationReporter             _reporter;

    public OptimizationPipeline(
        IReadOnlyList<IOptimizationPass> passes,
        ICodeReader                      reader,
        IOptimizationReporter            reporter)
    {
        _passes   = passes;
        _reader   = reader;
        _reporter = reporter;
    }

    public OperationResult<OptimizationReport> AnalyzeSource(
        string sourceCode, string filePath, OptimizationContext context)
    {
        var sw = Stopwatch.StartNew();

        var syntaxTree  = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            "Analysis",
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)]);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var parseErrors = syntaxTree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.GetMessage())
            .ToList();

        var allFindings = new List<OptimizationFinding>();

        foreach (var pass in _passes)
        {
            var result = pass.Execute(syntaxTree, semanticModel, filePath, context);
            if (result.IsSuccess)
                allFindings.AddRange(result.Value!);
        }

        var report = new OptimizationReport
        {
            FilePath                     = filePath,
            Findings                     = allFindings.OrderBy(f => f.LineNumber).ToList(),
            AnalysisDurationMilliseconds = sw.ElapsedMilliseconds,
            ParsedSuccessfully           = parseErrors.Count == 0,
            ParseErrors                  = parseErrors
        };

        _reporter.ReportFileSummary(report);
        return OperationResult<OptimizationReport>.Success(report);
    }

    public OperationResult<OptimizationReport> AnalyzeFile(
        string filePath, OptimizationContext context)
    {
        var read = _reader.ReadSource(filePath);
        return read.IsSuccess
            ? AnalyzeSource(read.Value!, filePath, context)
            : read.MapError<OptimizationReport>();
    }

    public OperationResult<IReadOnlyList<OptimizationReport>> AnalyzeDirectory(
        string directoryPath, OptimizationContext context)
    {
        var files = _reader.ListSourceFiles(directoryPath);
        if (!files.IsSuccess)
            return files.MapError<IReadOnlyList<OptimizationReport>>();

        var reports = new List<OptimizationReport>();
        foreach (var f in files.Value!)
        {
            var r = AnalyzeFile(f, context);
            if (r.IsSuccess) reports.Add(r.Value!);
        }
        return OperationResult<IReadOnlyList<OptimizationReport>>.Success(reports);
    }
}
