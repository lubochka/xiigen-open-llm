using Microsoft.CodeAnalysis;

namespace CodeOptimizer;

public interface IOptimizationPass
{
    string PassName { get; }

    OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree         syntaxTree,
        SemanticModel      semanticModel,
        string             filePath,
        OptimizationContext context);
}
