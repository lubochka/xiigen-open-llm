namespace ExperimentsV2.LogicalFlow;

// This record carries a user-level request into the logical orchestrator.
public sealed record LogicalOrchestratorRequest(
    string RunId,
    string ScenarioId,
    string UserRequest,
    LogicalExecutionFlags? Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record carries the assistant-level result returned by the logical orchestrator.
public sealed record LogicalOrchestratorResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    LogicalFlowResult FlowResult);

// This record binds one scenario id to one executable logical flow without introducing a registry yet.
public sealed record LogicalFlowBinding(string FlowId, ILogicalFlow<string> Flow);

// This interface is the assistant-level coordinator over logical flows.
public interface ILogicalOrchestrator
{
    // RunAsync selects a logical flow, applies flags, and returns one normalized assistant result.
    Task<OperationResult<LogicalOrchestratorResult>> RunAsync(LogicalOrchestratorRequest request, CancellationToken cancellationToken = default);
}
