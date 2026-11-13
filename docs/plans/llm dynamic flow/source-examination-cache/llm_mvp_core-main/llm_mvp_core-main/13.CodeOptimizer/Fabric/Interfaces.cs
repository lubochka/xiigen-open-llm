namespace CodeOptimizer;

public interface ICodeReader
{
    OperationResult<string>                  ReadSource(string path);
    OperationResult<IReadOnlyList<string>>   ListSourceFiles(string directoryPath);
}

public interface ICodeWriter
{
    OperationResult<bool> WriteOptimized(string path, string optimizedCode, bool overwrite);
}

public interface IOptimizationReporter
{
    void ReportFinding(OptimizationFinding finding);
    void ReportFileSummary(OptimizationReport report);
}
