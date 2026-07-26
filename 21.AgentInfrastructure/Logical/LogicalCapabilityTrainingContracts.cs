// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using SemanticCompilerAdapter;
using SemanticCompiler29;

// This request is emitted when a logical run reaches one or more knowledge edges.
public sealed record LogicalCapabilityTrainingRequest(
    string RunId,
    LogicalOrchestratorRequest OriginalRequest,
    LogicalFlowResult[] KnowledgeEdgeBranches,
    ILogicalTopologyRegistry Topology,
    InMemoryLogicalFlowRegistry Flows,
    InMemoryLogicalUnitRegistry Units)
{
    // Domain identifies the D-04 business memory bucket that should receive this teaching signal.
    public Domain Domain { get; init; } = Domain.CODE_Q;

    // CompiledPrompt is the project-23 semantic representation used by the tokenizer,
    // DPO prompt side, and learned-unit retrieval text. Null callers compile on demand.
    public SemanticTeachingPromptRepresentation? CompiledPrompt { get; init; }

    // CompiledText is the canonical prompt string used by older plan contracts.
    // When supplied, it wins over raw text as the DPO prompt-side representation.
    public string? CompiledText { get; init; }
}

// This result reports what capability was produced from the knowledge-edge evidence.
public sealed record LogicalCapabilityTrainingResult(
    bool CapabilityRegistered,
    string[] RegisteredFlowIds,
    string[] RegisteredSubPhaseIds,
    string[] RegisteredUnitIds,
    double TrainingTimeMs,
    string[] EvidenceIds,
    string Notes)
{
    // Domain is the D-04 business-memory bucket that received this teaching run.
    public string Domain { get; init; } = string.Empty;

    // BatchedBranchCount records how many routed branches were taught by the shared domain DPO step.
    public int BatchedBranchCount { get; init; }

    // BatchedDpoStepCount records optimizer updates, not preference examples.
    public int BatchedDpoStepCount { get; init; }

    // Per-branch timing shows whether slow teaching is caused by stack loading, DPO, persistence, wrapping, or registry work.
    public LogicalCapabilityTrainingBranchTiming[] BranchTimings { get; init; } = [];
}

// This result item measures one taught sub-phase branch inside a capability-training run.
public sealed record LogicalCapabilityTrainingBranchTiming(
    string SubPhaseId,
    string UnitId,
    string? ParentUnitId,
    int TrainingStepCount,
    bool UsedWarmCache,
    double BuildStackMs,
    double ReferenceModelMs,
    double DpoMs,
    double SaveArtifactsMs,
    double WrapUnitMs,
    double SaveManifestMs,
    double RegisterUnitMs,
    double RegisterFlowMs,
    double TotalMs);

// This interface is the bridge from detected knowledge edge to learned/promoted runtime capability.
public interface ILogicalCapabilityTrainer
{
    // TrainAndRegisterAsync may add flows or learned units to the runtime registries, then the orchestrator can retry.
    Task<AgentInfrastructure.OperationResult<LogicalCapabilityTrainingResult>> TrainAndRegisterAsync(
        LogicalCapabilityTrainingRequest request,
        CancellationToken cancellationToken = default);
}
