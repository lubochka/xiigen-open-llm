namespace OrchestratorModel;

using AgentInfrastructure;

// Task domains — align with Domain enum from SemanticCompiler29 (D-04).
// CODE_Q=CodeQuestion, BUG=BugFix, PLAN=Planning, ARCH=Architecture, REFACTOR=Refactoring.
public enum TaskDomain { CODE_Q, BUG, PLAN, ARCH, REFACTOR }

// Classifies a task description into one of five domains.
// Implemented via ISemanticCompiler29.Compile() — Domain from PipelineOutput.
// Confidence < 0.6 (CompressionRatio < 12) → signal for 17.DynamicDomains.
public interface IEpisodeClassifier
{
    OperationResult<TaskDomain> Classify(string taskDescription);
    OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription);
}

// Breaks a task into ordered sub-episodes.
// E32 +11%: PLAN → 4, ARCH → 3, REFACTOR → 3, BUG → 2, CODE_Q → 1.
public interface IEpisodeDecomposer
{
    OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>> Decompose(
        string taskDescription, TaskDomain domain);
}

// Four stop signals for decomposition (E33).
public interface IStopSignalDetector
{
    OperationResult<bool> ShouldStop(string subTaskDescription);
}

// Launches a sub-agent for one episode.
public interface IAgentLauncher
{
    Task<OperationResult<string>> LaunchAsync(AgentAlgorithms.SubTask task, string context);
}

// Merges multi-agent results into one coherent output.
// Uses ITemplateBeautifier (Fabric First — declared in Fabric/Interfaces.cs).
public interface IResultMerger
{
    OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results);
}

// Stores and recalls episode history.
// InMemoryEpisodeMemory is a stub until 14.ContinuousLearning.
public interface IEpisodeMemory
{
    OperationResult<bool> Store(
        string taskDescription, IReadOnlyList<AgentAlgorithms.SubTask> decomposition);
    OperationResult<IReadOnlyList<string>> Recall(string taskDescription, int topK = 3);
}

// Runs 16 quality checks before delivering a result (E39 +20.2%).
public interface IQualityOrchestrator
{
    OperationResult<QualityReport> Check(
        string taskDescription, string result, string outputType);
}

// Quality report returned by IQualityOrchestrator.
// Passed=false when any check fails; FailedChecks lists the check codes.
public sealed record QualityReport(bool Passed, IReadOnlyList<string> FailedChecks);
