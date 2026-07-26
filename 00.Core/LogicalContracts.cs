// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;

// This record carries every execution flag that a logical unit, flow, or orchestrator can see.
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
        nameof(UseIdleLearningScheduling),
        nameof(UseE35DependencyDetection),
        nameof(UseSkillRetriever),
        nameof(UseReplayFusionRanking)
    ]);

    // This list names behaviors that protect correctness and user authority.
    public static IReadOnlyList<string> SafetyFlagNames { get; } = Array.AsReadOnly([
        nameof(UseArbiterAuthority),
        nameof(UseKnowledgeEdgeDetection),
        nameof(UseInternalFirstExecution),
        nameof(RequireExternalAiApproval),
        nameof(RejectArbiterContradictions)
    ]);

    // This list names flags that describe how an experiment or proof run is being executed.
    public static IReadOnlyList<string> ExperimentControlFlagNames { get; } = Array.AsReadOnly([
        nameof(UseDeterministicFixtureBaseline),
        nameof(WriteEvidenceArtifacts),
        nameof(CompareAgainstOracle),
        nameof(FailOnMetricDrift)
    ]);

    public bool UseCompiledPredicateRepresentation { get; init; } = true;
    public bool UseDomainScopedRetrieval { get; init; } = true;
    public bool UseConfidenceFloor { get; init; } = true;
    public bool UseLearnedUnitWhenAvailable { get; init; } = true;
    public bool UseTraceEvidence { get; init; } = true;
    public bool UseContextCarryForward { get; init; } = true;
    public bool UseIdleLearningScheduling { get; init; } = true;
    public bool UseE35DependencyDetection { get; init; }
    public bool UseSkillRetriever { get; init; }
    public bool UseReplayFusionRanking { get; init; }
    public bool UseArbiterAuthority { get; init; } = true;
    public bool UseKnowledgeEdgeDetection { get; init; } = true;
    public bool UseInternalFirstExecution { get; init; } = true;
    public bool RequireExternalAiApproval { get; init; } = true;
    public bool RejectArbiterContradictions { get; init; } = true;
    public bool UseDeterministicFixtureBaseline { get; init; }
    public bool WriteEvidenceArtifacts { get; init; } = true;
    public bool CompareAgainstOracle { get; init; } = true;
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
    string[] Warnings)
{
    // TransitionSignal is optional. Null means this unit requests no follow-up transition.
    // Keep it off the positional constructor so existing unit result call sites stay stable.
    public PhaseTransitionSignal? TransitionSignal { get; init; }
}

// This record carries an advisory request from a flow node to re-enter orchestration after the current branch completes.
public sealed record PhaseTransitionSignal(
    Domain SuggestedDomain,
    string TransitionPrompt,
    string SourceNodeId,
    string SourceSubPhaseId,
    string[] EvidenceIds);

// This interface is the smallest executable contract for a dynamic-flow cell.
public interface ILogicalUnit<TInput, TOutput>
{
    Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default);
}

// This record carries a request to execute one logical flow.
public sealed record LogicalFlowRequest<TInput>(
    string RunId,
    string FlowId,
    string ScenarioId,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record carries a runtime-neutral summary of a flow or experiment run.
public sealed record LogicalFlowSummary(
    string SummaryId,
    int Tier,
    double BaselinePrimary,
    double EnhancedPrimary,
    string PrimaryMetricName,
    bool LowerIsBetter,
    bool Passed,
    bool ShouldScaleUp,
    string NextAction,
    double ScalingSignal,
    string ScalingSignalNote,
    string Notes,
    double BaselineTimeMs,
    double EnhancedTimeMs);

// This record carries the normalized result of a logical flow execution.
public sealed record LogicalFlowResult(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalFlowSummary Summary,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings)
{
    // TransitionSignals carries advisory post-branch transition requests emitted by flow nodes.
    public PhaseTransitionSignal[] TransitionSignals { get; init; } = [];

    // EdgeTargetHandoff is inference-only. Null means this flow produced no learned edge target handoff.
    // It stays off the positional constructor so all existing flow-result call sites remain source compatible.
    public DynamicGraphEdgeTargetHandoff? EdgeTargetHandoff { get; init; }
}

// This record carries one completed branch as compact predicate-level context for downstream branches.
public sealed record LogicalContextFragment(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    string? PhaseId,
    string? SubPhaseId,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings);

// This record carries every upstream branch fragment that a downstream branch may inherit.
public sealed record LogicalContextPacket(
    string RunId,
    string[] SourceFlowIds,
    string[] SourceScenarioIds,
    LogicalContextFragment[] Fragments,
    DateTimeOffset CreatedAtUtc);

// This interface is the executable contract for a topology of logical units.
public interface ILogicalFlow<TInput>
{
    Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<TInput> request,
        CancellationToken cancellationToken = default);
}

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
    LogicalFlowResult FlowResult)
{
    public LogicalFlowResult[] BranchResults { get; init; } = [];
    public string[] Warnings { get; init; } = [];
    public LogicalContextPacket? OutputContextPacket { get; init; }
}

// This record binds one route or scenario to one executable logical flow.
public sealed record LogicalFlowBinding(string FlowId, ILogicalFlow<string> Flow)
{
    public string ScenarioId { get; init; } = "";
}

// This interface is the assistant-level coordinator over logical flows.
public interface ILogicalOrchestrator
{
    Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default);
}

// Trust level classification used by the E40V2 trust scorer.
public enum TrustLevel { Low, Medium, High }

// Input features for E40V2 trust scoring.
public sealed record SourceFeatures(
    bool IsInternal,
    bool HasCIPassing,
    bool HasTests,
    int UpdateDaysAgo,
    int PopularityScore,
    bool IsOfficialDocs,
    bool DomainMatch);

// Output of E40V2 trust scoring.
public sealed record TrustScorerOutput(double Score, TrustLevel Level);

// Shared trust scoring contract consumed by project 21 without reflecting into project 16.
public interface ITrustScorer
{
    OperationResult<TrustScorerOutput> Score(SourceFeatures features);
}
