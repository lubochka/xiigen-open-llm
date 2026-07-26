// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ResultGapDomain;

public sealed class ResultGapLearningMachineAdapter : ILearningMachine<ResultGapDomain>
{
    public const string MachineId = "ResultGapMachine";
    public const string DomainTag = "ResultGapDomain";

    private readonly ResultGapMachine _bootstrapMachine;
    private readonly SkillSelectionMachine _skillSelectionMachine;

    public ResultGapLearningMachineAdapter(
        ResultGapMachine? bootstrapMachine = null,
        SkillSelectionMachine? skillSelectionMachine = null)
    {
        _bootstrapMachine = bootstrapMachine ?? new ResultGapMachine();
        _skillSelectionMachine = skillSelectionMachine ?? new SkillSelectionMachine();
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<ResultGapDomain> batch,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningBatchResult>.Fail(
            "ResultGap logical hierarchy adapter does not train semantic behavior in CM-26C.",
            "RESULT_GAP_TRAINING_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER"));

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<ResultGapDomain> evalSet,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: 0.0,
            RewardMargin: 0.0,
            PassesGate: false,
            GateNote: "ResultGap logical hierarchy adapter exposes source hierarchy only; semantic quality remains unclaimed.")));

    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningMachineExport>.Fail(
            "ResultGap logical adapter does not own a standalone export path; checkpoint-backed result_gap state is owned by SkillSelectionMachine.",
            "RESULT_GAP_STANDALONE_EXPORT_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER"));

    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<bool>.Fail(
            "ResultGap logical adapter import is not standalone; use the SkillSelectionMachine checkpoint result_gap section for common semantic state and user-local ResultGap adaptive package for personal state.",
            "RESULT_GAP_STANDALONE_IMPORT_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER"));

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
        => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
            new ResultGapLogicalOrchestrator(new ResultGapLogicalFlow(_bootstrapMachine, _skillSelectionMachine))));
}

public sealed class ResultGapLogicalOrchestrator(ResultGapLogicalFlow flow) : ILogicalOrchestrator
{
    public const string RequiredMetadataKey = "result_gap_request";

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Metadata.TryGetValue(RequiredMetadataKey, out var value) || value is not ResultGapRequest resultGapRequest)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                $"ResultGap logical orchestrator requires metadata '{RequiredMetadataKey}' containing a ResultGapRequest.",
                "RESULT_GAP_REQUEST_METADATA_REQUIRED");
        }

        var flowRequest = new LogicalFlowRequest<ResultGapRequest>(
            FlowId: ResultGapLogicalFlow.FlowId,
            ScenarioId: request.ScenarioId,
            RunId: request.RunId,
            Input: resultGapRequest,
            Flags: request.Flags ?? new LogicalExecutionFlags(),
            Metadata: request.Metadata);
        var flowResult = await flow.ExecuteAsync(flowRequest, cancellationToken);
        if (!flowResult.IsSuccess)
            return flowResult.MapError<LogicalOrchestratorResult>();

        var result = new LogicalOrchestratorResult(
            ScenarioId: request.ScenarioId,
            SelectedFlowId: flowResult.Value!.FlowId,
            OverallVerdict: flowResult.Value.OverallVerdict,
            FinalHumanResult: flowResult.Value.FinalHumanResult,
            AppliedFlags: request.Flags ?? new LogicalExecutionFlags(),
            FlowResult: flowResult.Value)
        {
            Warnings = flowResult.Value.Warnings
        };

        return OperationResult<LogicalOrchestratorResult>.Ok(result);
    }
}

public sealed class ResultGapLogicalFlow(
    ResultGapMachine bootstrapMachine,
    SkillSelectionMachine skillSelectionMachine) : ILogicalFlow<ResultGapRequest>
{
    public const string FlowId = "result-gap-logical-hierarchy";

    private readonly IReadOnlyList<ILogicalUnit<ResultGapRequest, ResultGapResult>> _units =
    [
        new ResultGapBootstrapFixtureUnit(bootstrapMachine),
        new ResultGapCheckpointSemanticBoundaryUnit(bootstrapMachine, skillSelectionMachine),
        new ResultGapAdaptiveSignalBoundaryUnit(bootstrapMachine),
        new ResultGapAuthorityBoundaryUnit(bootstrapMachine)
    ];

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<ResultGapRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Flow cancelled.", "CANCELLED");

        var evidence = new List<string>
        {
            "RESULT_GAP_LOGICAL_FLOW_SOURCE_BACKED",
            "RESULT_GAP_BOOTSTRAP_FIXTURE_SEPARATED_FROM_TRAINED_AUTHORITY",
            SkillSelectionMachine.ResultGapCheckpointEvidence,
            "RESULT_GAP_USER_LOCAL_ADAPTIVE_EVIDENCE_SEPARATE",
            "NO_ADDITIONAL_DETERMINISTIC_SEMANTIC_SHORTCUT",
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        var warnings = new List<string>
        {
            "CM26C_LOGICAL_HIERARCHY_ADAPTER_ONLY",
            "BOOTSTRAP_RESULT_GAP_FIXTURE_IS_CONTRACT_SUPPORT_NOT_TRAINED_AUTHORITY",
            "NO_TRAINING_OR_SEMANTIC_QUALITY_GATE_RUN"
        };

        ResultGapResult? finalResponse = null;
        foreach (var unit in _units)
        {
            var unitResult = await unit.ExecuteAsync(new LogicalUnitRequest<ResultGapRequest>(
                RunId: request.RunId,
                UnitId: GetUnitId(unit),
                UnitRole: GetUnitRole(unit),
                DomainTag: ResultGapLearningMachineAdapter.DomainTag,
                Input: request.Input,
                Flags: request.Flags,
                Metadata: request.Metadata), cancellationToken);
            if (!unitResult.IsSuccess)
                return unitResult.MapError<LogicalFlowResult>();

            finalResponse = unitResult.Value!.Output;
            evidence.AddRange(unitResult.Value.EvidenceIds);
            warnings.AddRange(unitResult.Value.Warnings);
        }

        var summary = new LogicalFlowSummary(
            SummaryId: $"{request.RunId}-result-gap-logical-summary",
            Tier: 0,
            BaselinePrimary: 0.0,
            EnhancedPrimary: 0.0,
            PrimaryMetricName: "semantic_quality_claimed",
            LowerIsBetter: false,
            Passed: false,
            ShouldScaleUp: false,
            NextAction: "Run ResultGap trained semantic and adaptive gates before any semantic completion claim.",
            ScalingSignal: 0.0,
            ScalingSignalNote: "CM-26C source hierarchy evidence only.",
            Notes: "Logical hierarchy delegates to ResultGapMachine.Evaluate while recording that bootstrap exact comparison is fixture support only.",
            BaselineTimeMs: 0.0,
            EnhancedTimeMs: 0.0);

        var result = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: request.ScenarioId,
            OverallVerdict: finalResponse?.SkillGapDetected == true
                ? "RESULT_GAP_DETECTED_BOOTSTRAP_FIXTURE_CONTRACT_SUPPORT"
                : "NO_RESULT_GAP_DETECTED_BOOTSTRAP_FIXTURE_CONTRACT_SUPPORT",
            FinalHumanResult: finalResponse?.MissingSkillId ?? "",
            Summary: summary,
            Metrics: new Dictionary<string, string>
            {
                ["logical_unit_count"] = _units.Count.ToString(),
                ["semantic_quality_claimed"] = bool.FalseString,
                ["training_run"] = bool.FalseString,
                ["bootstrap_fixture_final_authority"] = bool.FalseString,
                ["skill_selection_result_gap_checkpoint_unit"] = SkillSelectionMachine.ResultGapUnitId
            },
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray());

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    private static string GetUnitId(ILogicalUnit<ResultGapRequest, ResultGapResult> unit)
        => unit switch
        {
            ResultGapBootstrapFixtureUnit => ResultGapBootstrapFixtureUnit.UnitId,
            ResultGapCheckpointSemanticBoundaryUnit => ResultGapCheckpointSemanticBoundaryUnit.UnitId,
            ResultGapAdaptiveSignalBoundaryUnit => ResultGapAdaptiveSignalBoundaryUnit.UnitId,
            ResultGapAuthorityBoundaryUnit => ResultGapAuthorityBoundaryUnit.UnitId,
            _ => "result_gap_unknown"
        };

    private static string GetUnitRole(ILogicalUnit<ResultGapRequest, ResultGapResult> unit)
        => unit switch
        {
            ResultGapBootstrapFixtureUnit => ResultGapBootstrapFixtureUnit.Role,
            ResultGapCheckpointSemanticBoundaryUnit => ResultGapCheckpointSemanticBoundaryUnit.Role,
            ResultGapAdaptiveSignalBoundaryUnit => ResultGapAdaptiveSignalBoundaryUnit.Role,
            ResultGapAuthorityBoundaryUnit => ResultGapAuthorityBoundaryUnit.Role,
            _ => "unknown"
        };
}

public sealed class ResultGapBootstrapFixtureUnit(ResultGapMachine machine)
    : ResultGapLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "result_gap_bootstrap_fixture_unit";
    public const string Role = "bootstrap_fixture_gap_detection";
}

public sealed class ResultGapCheckpointSemanticBoundaryUnit(
    ResultGapMachine machine,
    SkillSelectionMachine skillSelectionMachine)
    : ResultGapLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "result_gap_checkpoint_semantic_boundary_unit";
    public const string Role = "checkpoint_backed_result_gap_unit_boundary";

    protected override IReadOnlyList<string> AdditionalEvidence()
        =>
        [
            SkillSelectionMachine.ResultGapCheckpointEvidence,
            $"{SkillSelectionMachine.ResultGapUnitId}:checkpoint_profile_count={skillSelectionMachine.ExportCheckpoint().ResultGap.SkillProfiles.Count}",
            "SKILL_SELECTION_MACHINE_OWNS_CHECKPOINT_BACKED_RESULT_GAP_SEMANTIC_UNIT"
        ];
}

public sealed class ResultGapAdaptiveSignalBoundaryUnit(ResultGapMachine machine)
    : ResultGapLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "result_gap_adaptive_signal_boundary_unit";
    public const string Role = "user_local_adaptive_signal_boundary";

    protected override IReadOnlyList<string> AdditionalEvidence()
        =>
        [
            "AD_01_RESULT_GAP_SIGNAL_WRITER",
            "AD_02_RESULT_GAP_ADAPTIVE_TRAINER",
            "AD_09A_RESULT_GAP_BACKUP_RESTORE_SHARE_EVIDENCE"
        ];
}

public sealed class ResultGapAuthorityBoundaryUnit(ResultGapMachine machine)
    : ResultGapLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "result_gap_authority_boundary_unit";
    public const string Role = "bootstrap_not_trained_authority_check";

    protected override IReadOnlyList<string> AdditionalEvidence()
        =>
        [
            "BOOTSTRAP_EXACT_EXPECTED_SKILL_COMPARISON_IS_NOT_FINAL_SEMANTIC_AUTHORITY",
            "NO_REGEX_BM25_STATIC_LOOKUP_ADDED_AS_RESULT_GAP_AUTHORITY"
        ];
}

public abstract class ResultGapLogicalUnitBase(
    ResultGapMachine machine,
    string unitId,
    string unitRole) : ILogicalUnit<ResultGapRequest, ResultGapResult>
{
    public Task<OperationResult<LogicalUnitResult<ResultGapResult>>> ExecuteAsync(
        LogicalUnitRequest<ResultGapRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<ResultGapResult>>.Fail("Unit cancelled.", "CANCELLED"));

        var evaluated = machine.Evaluate(request.Input);
        var evidence = new List<string>
        {
            "RESULT_GAP_LOGICAL_UNIT_SOURCE_BACKED",
            $"RESULT_GAP_UNIT_ROLE:{unitRole}",
            evaluated.EvidenceMode
        };
        evidence.AddRange(evaluated.EvidenceFeatures);
        evidence.AddRange(AdditionalEvidence());

        var result = new LogicalUnitResult<ResultGapResult>(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: ResultGapLearningMachineAdapter.DomainTag,
            Output: evaluated,
            Confidence: 0.0,
            PredicateIds: [],
            DomainTags: [ResultGapLearningMachineAdapter.DomainTag],
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings:
            [
                "RESULT_GAP_LOGICAL_UNIT_DELEGATES_BOOTSTRAP_EVALUATE",
                "BOOTSTRAP_FIXTURE_NOT_TRAINED_SEMANTIC_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<ResultGapResult>>.Ok(result));
    }

    protected virtual IReadOnlyList<string> AdditionalEvidence() => [];
}
