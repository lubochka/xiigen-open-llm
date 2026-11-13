using ExperimentsV2;

namespace ExperimentsV2.LogicalFlow;

// This record carries every execution flag that a logical unit can see.
public sealed record LogicalExecutionFlags
{
    // This list names behaviors that improve routing and learning quality.
    public static IReadOnlyList<string> OptimizationFlagNames { get; } = Array.AsReadOnly([
        nameof(UseCompiledPredicateRepresentation),
        nameof(UseDomainScopedRetrieval),
        nameof(UseConfidenceFloor),
        nameof(UseLearnedUnitWhenAvailable),
        nameof(UseTraceEvidence),
        nameof(UseContextCarryForward),
        nameof(UseIdleLearningScheduling)
    ]);

    // This list names behaviors that protect correctness and user authority.
    public static IReadOnlyList<string> SafetyFlagNames { get; } = Array.AsReadOnly([
        nameof(UseArbiterAuthority),
        nameof(UseKnowledgeEdgeDetection),
        nameof(UseInternalFirstExecution),
        nameof(RequireExternalAiApproval),
        nameof(RejectArbiterContradictions)
    ]);

    // This list names flags that describe how an experiment is being run.
    public static IReadOnlyList<string> ExperimentControlFlagNames { get; } = Array.AsReadOnly([
        nameof(UseDeterministicFixtureBaseline),
        nameof(WriteEvidenceArtifacts),
        nameof(CompareAgainstOracle),
        nameof(FailOnMetricDrift)
    ]);

    // The unit should receive the compiled 22-predicate representation by default.
    public bool UseCompiledPredicateRepresentation { get; init; } = true;

    // KNN and memory lookup should search inside the predicted domain by default.
    public bool UseDomainScopedRetrieval { get; init; } = true;

    // Low-confidence matches should be rejected or routed to a gap path by default.
    public bool UseConfidenceFloor { get; init; } = true;

    // A learned unit may replace a fixture only after its conformance gate passes.
    public bool UseLearnedUnitWhenAvailable { get; init; } = true;

    // Evidence should be recorded for every accepted experiment or product path.
    public bool UseTraceEvidence { get; init; } = true;

    // Phase and sub-phase outputs should carry forward into downstream runs.
    public bool UseContextCarryForward { get; init; } = true;

    // Accepted feedback should be available to the idle learning scheduler.
    public bool UseIdleLearningScheduling { get; init; } = true;

    // Arbiter verdicts remain the hard acceptance authority.
    public bool UseArbiterAuthority { get; init; } = true;

    // Missing competence should create a named knowledge edge instead of invented output.
    public bool UseKnowledgeEdgeDetection { get; init; } = true;

    // The local model should be attempted before any external AI request.
    public bool UseInternalFirstExecution { get; init; } = true;

    // Any external AI request requires one explicit approval.
    public bool RequireExternalAiApproval { get; init; } = true;

    // Internal and external outputs that contradict arbiters must be rejected.
    public bool RejectArbiterContradictions { get; init; } = true;

    // Fixture baselines are off unless an experiment explicitly asks for the oracle fixture.
    public bool UseDeterministicFixtureBaseline { get; init; }

    // Evidence artifacts are written unless a narrow in-memory test disables writing.
    public bool WriteEvidenceArtifacts { get; init; } = true;

    // Replacement experiments should compare against the accepted oracle by default.
    public bool CompareAgainstOracle { get; init; } = true;

    // Metric drift should fail the replacement experiment by default.
    public bool FailOnMetricDrift { get; init; } = true;
}

// This record is the input envelope every logical unit receives.
public sealed record LogicalUnitRequest<TInput>(
    string RunId,
    string UnitId,
    string UnitRole,
    string DomainTag,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record is the output envelope every logical unit returns.
public sealed record LogicalUnitResult<TOutput>(
    string UnitId,
    string UnitRole,
    string DomainTag,
    TOutput Output,
    double Confidence,
    string[] PredicateIds,
    string[] DomainTags,
    string[] EvidenceIds,
    string[] Warnings);

// This interface is the smallest executable contract for a dynamic-flow cell.
public interface ILogicalUnit<TInput, TOutput>
{
    // ExecuteAsync is async from the first contract so learned and LLM-backed units do not need a second interface later.
    Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(LogicalUnitRequest<TInput> request, CancellationToken cancellationToken = default);
}
