using ExperimentsV2;

namespace ExperimentsV2.LogicalFlow;

// This record carries a request to execute one logical flow.
public sealed record LogicalFlowRequest<TInput>(
    string RunId,
    string FlowId,
    string ScenarioId,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record carries the normalized result of a logical flow execution.
public sealed record LogicalFlowResult(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    ExperimentV2Result Summary,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings);

// This interface is the executable contract for a topology of logical units.
public interface ILogicalFlow<TInput>
{
    // ExecuteAsync runs the flow behind one stable interface so deterministic and learned flows share a caller path.
    Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<TInput> request, CancellationToken cancellationToken = default);
}
