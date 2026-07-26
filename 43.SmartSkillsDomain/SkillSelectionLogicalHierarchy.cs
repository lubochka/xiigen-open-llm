// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class SkillSelectionLogicalOrchestrator(SkillSelectionLogicalFlow flow) : ILogicalOrchestrator
{
    public const string RequiredMetadataKey = "skill_selection_request";

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Metadata.TryGetValue(RequiredMetadataKey, out var value) || value is not SkillSelectionRequest skillRequest)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                $"SkillSelection logical orchestrator requires metadata '{RequiredMetadataKey}' containing a SkillSelectionRequest.",
                "SKILL_SELECTION_REQUEST_METADATA_REQUIRED");
        }

        var flowRequest = new LogicalFlowRequest<SkillSelectionRequest>(
            RunId: request.RunId,
            FlowId: SkillSelectionLogicalFlow.FlowId,
            ScenarioId: request.ScenarioId,
            Input: skillRequest,
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

public sealed class SkillSelectionLogicalFlow(SkillSelectionMachine machine) : ILogicalFlow<SkillSelectionRequest>
{
    public const string FlowId = "skill-selection-logical-hierarchy";

    public static IReadOnlyList<string> ExpectedSemanticUnitIds => SmartSkillsSemanticUnitCatalog.ExpectedDomainIds;

    private readonly IReadOnlyList<ILogicalUnit<SkillSelectionRequest, SkillSelectionDecision>> _units =
    [
        new SkillSelectionSemanticUnitInventoryUnit(machine),
        new SkillSelectionCheckpointBoundaryUnit(machine),
        new SkillSelectionDecisionPathUnit(machine),
        new SkillSelectionAuthorityBoundaryUnit(machine)
    ];
    private readonly ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport> _candidateOrderLeakageGuardUnit =
        new CandidateOrderLeakageGuardUnit(
            machine.ExportCandidateOrderLeakageGuardState,
            machine.ProbeWithoutCandidatePositionAuthority);

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<SkillSelectionRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Flow cancelled.", "CANCELLED");

        var evidence = new List<string>
        {
            "SKILL_SELECTION_LOGICAL_FLOW_SOURCE_BACKED",
            "SKILL_SELECTION_DECIDE_DELEGATED_TO_MACHINE",
            "SKILL_SELECTION_11_UNITS_CHECKPOINT_BOUNDARY",
            "NO_ADDITIONAL_DETERMINISTIC_SEMANTIC_SHORTCUT",
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        var warnings = new List<string>
        {
            "CM26D_LOGICAL_HIERARCHY_ADAPTER_ONLY",
            "NO_TRAINING_OR_SEMANTIC_QUALITY_GATE_RUN",
            "SS13_ASSET_LIFECYCLE_IS_NOT_FULL_SEMANTIC_QUALITY"
        };

        SkillSelectionDecision? finalDecision = null;
        foreach (var unit in _units)
        {
            var unitResult = await unit.ExecuteAsync(new LogicalUnitRequest<SkillSelectionRequest>(
                RunId: request.RunId,
                UnitId: GetUnitId(unit),
                UnitRole: GetUnitRole(unit),
                DomainTag: SkillSelectionMachine.DomainTag,
                Input: request.Input,
                Flags: request.Flags,
                Metadata: request.Metadata), cancellationToken);
            if (!unitResult.IsSuccess)
                return unitResult.MapError<LogicalFlowResult>();

            finalDecision = unitResult.Value!.Output;
            evidence.AddRange(unitResult.Value.EvidenceIds);
            warnings.AddRange(unitResult.Value.Warnings);
        }
        var leakageGuardResult = await _candidateOrderLeakageGuardUnit.ExecuteAsync(new LogicalUnitRequest<SkillSelectionRequest>(
            RunId: request.RunId,
            UnitId: CandidateOrderLeakageGuardUnit.UnitId,
            UnitRole: CandidateOrderLeakageGuardUnit.Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Input: request.Input,
            Flags: request.Flags,
            Metadata: request.Metadata), cancellationToken);
        if (!leakageGuardResult.IsSuccess)
            return leakageGuardResult.MapError<LogicalFlowResult>();

        var leakageReport = leakageGuardResult.Value!.Output;
        evidence.AddRange(leakageGuardResult.Value.EvidenceIds);
        warnings.AddRange(leakageGuardResult.Value.Warnings);

        var summary = new LogicalFlowSummary(
            SummaryId: $"{request.RunId}-skill-selection-logical-summary",
            Tier: 0,
            BaselinePrimary: 0.0,
            EnhancedPrimary: 0.0,
            PrimaryMetricName: "semantic_quality_claimed",
            LowerIsBetter: false,
            Passed: false,
            ShouldScaleUp: false,
            NextAction: "Run SkillSelection trained semantic quality gates before any semantic completion claim.",
            ScalingSignal: 0.0,
            ScalingSignalNote: "CM-26D source hierarchy evidence only.",
            Notes: "Logical hierarchy delegates to SkillSelectionMachine.DecideFromTrainedState and does not add ranking authority.",
            BaselineTimeMs: 0.0,
            EnhancedTimeMs: 0.0);

        var result = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: request.ScenarioId,
            OverallVerdict: finalDecision?.RequiresSecondPass == true
                ? "SKILL_SELECTION_REQUIRES_SECOND_PASS_OR_NOT_READY"
                : "SKILL_SELECTION_DECISION_SOURCE_HIERARCHY_ONLY",
            FinalHumanResult: finalDecision?.DecisionText ?? "",
            Summary: summary,
            Metrics: new Dictionary<string, string>
            {
                ["logical_unit_count"] = _units.Count.ToString(),
                ["semantic_unit_count"] = ExpectedSemanticUnitIds.Count.ToString(),
                ["semantic_quality_claimed"] = bool.FalseString,
                ["training_run"] = bool.FalseString,
                ["candidate_position_scaffold_final_authority"] = bool.FalseString,
                ["candidate_order_guard_unit_count"] = "1",
                ["candidate_order_permutation_count"] = leakageReport.PermutationCount.ToString(),
                ["candidate_order_sensitivity_detected"] = leakageReport.OrderSensitivityDetected.ToString()
            },
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray());

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    private static string GetUnitId(ILogicalUnit<SkillSelectionRequest, SkillSelectionDecision> unit)
        => unit switch
        {
            SkillSelectionSemanticUnitInventoryUnit => SkillSelectionSemanticUnitInventoryUnit.UnitId,
            SkillSelectionCheckpointBoundaryUnit => SkillSelectionCheckpointBoundaryUnit.UnitId,
            SkillSelectionDecisionPathUnit => SkillSelectionDecisionPathUnit.UnitId,
            SkillSelectionAuthorityBoundaryUnit => SkillSelectionAuthorityBoundaryUnit.UnitId,
            _ => "skill_selection_unknown"
        };

    private static string GetUnitRole(ILogicalUnit<SkillSelectionRequest, SkillSelectionDecision> unit)
        => unit switch
        {
            SkillSelectionSemanticUnitInventoryUnit => SkillSelectionSemanticUnitInventoryUnit.Role,
            SkillSelectionCheckpointBoundaryUnit => SkillSelectionCheckpointBoundaryUnit.Role,
            SkillSelectionDecisionPathUnit => SkillSelectionDecisionPathUnit.Role,
            SkillSelectionAuthorityBoundaryUnit => SkillSelectionAuthorityBoundaryUnit.Role,
            _ => "unknown"
        };
}

public sealed class SkillSelectionSemanticUnitInventoryUnit(SkillSelectionMachine machine)
    : SkillSelectionLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "skill_selection_semantic_unit_inventory_unit";
    public const string Role = "eleven_semantic_unit_inventory";

    protected override IReadOnlyList<string> AdditionalEvidence()
        => SmartSkillsSemanticUnitCatalog.ExpectedDomainIds
            .Select(unitId => $"SKILL_SELECTION_SEMANTIC_UNIT:{unitId}")
            .Append("SKILL_SELECTION_11_SEMANTIC_UNITS_CATALOG_SOURCE_BACKED")
            .ToArray();
}

public sealed class SkillSelectionCheckpointBoundaryUnit(SkillSelectionMachine machine)
    : SkillSelectionLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "skill_selection_checkpoint_boundary_unit";
    public const string Role = "checkpoint_backed_semantic_unit_boundary";

    protected override IReadOnlyList<string> AdditionalEvidence()
    {
        var checkpoint = Machine.ExportCheckpoint();
        return
        [
            SkillSelectionMachine.PromptUnderstandingCheckpointEvidence,
            SkillSelectionMachine.TaskUnderstandingCheckpointEvidence,
            SkillSelectionMachine.TaskTypeCheckpointEvidence,
            SkillSelectionMachine.RoutingCheckpointEvidence,
            SkillSelectionMachine.SkillCandidatesCheckpointEvidence,
            SkillSelectionMachine.SkillSelectionCheckpointEvidence,
            SkillSelectionMachine.PromptContextCheckpointEvidence,
            SkillSelectionMachine.ResultGapCheckpointEvidence,
            SkillSelectionMachine.OutputValidationCheckpointEvidence,
            SkillSelectionMachine.TaskCompletionCheckpointEvidence,
            SkillSelectionMachine.IntermediateChecksCheckpointEvidence,
            $"checkpoint_semantic_unit_count={CountCheckpointSections(checkpoint)}",
            "SS13_ASSET_LIFECYCLE_BOUNDARY_NOT_SEMANTIC_QUALITY_CLAIM"
        ];
    }

    private static int CountCheckpointSections(SkillSelectionCheckpoint checkpoint)
        =>
        new[]
        {
            checkpoint.PromptUnderstanding.UnitId,
            checkpoint.TaskUnderstanding.UnitId,
            checkpoint.TaskType.UnitId,
            checkpoint.Routing.UnitId,
            checkpoint.SkillCandidates.UnitId,
            checkpoint.SkillSelection.UnitId,
            checkpoint.PromptContext.UnitId,
            checkpoint.ResultGap.UnitId,
            checkpoint.OutputValidation.UnitId,
            checkpoint.TaskCompletion.UnitId,
            checkpoint.IntermediateChecks.UnitId
        }.Distinct(StringComparer.Ordinal).Count();
}

public sealed class SkillSelectionDecisionPathUnit(SkillSelectionMachine machine)
    : SkillSelectionLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "skill_selection_decision_path_unit";
    public const string Role = "machine_decide_from_trained_state_boundary";

    protected override IReadOnlyList<string> AdditionalEvidence()
        =>
        [
            "SKILL_SELECTION_MACHINE_DECIDE_FROM_TRAINED_STATE_DELEGATED",
            "NO_HOST_SIDE_RERANKING",
            "NO_NEW_KEYWORD_CLASSIFIER"
        ];
}

public sealed class SkillSelectionAuthorityBoundaryUnit(SkillSelectionMachine machine)
    : SkillSelectionLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "skill_selection_authority_boundary_unit";
    public const string Role = "candidate_position_scaffold_not_final_authority";

    protected override IReadOnlyList<string> AdditionalEvidence()
        =>
        [
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            CandidateOrderLeakageGuardUnit.DiagnosticsEvidence,
            "CANDIDATE_POSITION_SCAFFOLD_CLASSIFIED_AS_NON_FINAL_AUTHORITY",
            "STATIC_VALIDATORS_AND_JSON_REPORTS_NOT_TRAINED_AUTHORITY"
        ];
}

public sealed class CandidateOrderLeakageGuardUnit : ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport>
{
    public const string UnitId = "candidate_order_leakage_guard";
    public const string Role = "candidate_position_diagnostics_provenance_only";
    public const string DiagnosticsEvidence = "CANDIDATE_ORDER_LEAKAGE_GUARD_DIAGNOSTICS_ONLY";

    private readonly Func<CandidateOrderLeakageGuardState> _stateAccessor;
    private readonly Func<SkillSelectionRequest, SkillSelectionDecision> _decisionProbe;

    public CandidateOrderLeakageGuardUnit(
        CandidateOrderLeakageGuardState state,
        Func<SkillSelectionRequest, SkillSelectionDecision> decisionProbe)
        : this(() => state, decisionProbe)
    {
    }

    public CandidateOrderLeakageGuardUnit(
        Func<CandidateOrderLeakageGuardState> stateAccessor,
        Func<SkillSelectionRequest, SkillSelectionDecision> decisionProbe)
    {
        _stateAccessor = stateAccessor;
        _decisionProbe = decisionProbe;
    }

    public CandidateOrderLeakageReport Evaluate(SkillSelectionRequest request)
    {
        var state = _stateAccessor();
        var candidatePositionWeights = state.CandidatePositionWeights
            .OrderBy(item => item.Key)
            .ToDictionary(item => item.Key, item => item.Value);
        var profileCount = candidatePositionWeights.Count;
        var weightTotal = candidatePositionWeights.Values.Sum();
        var coveredPositions = Enumerable
            .Range(0, request.AvailableSkillIds.Count)
            .Count(candidatePositionWeights.ContainsKey);
        var blockedFeatures = new[]
        {
            "candidate_index_skill_suggestion",
            "candidate_position_weight_skill_suggestion",
            "candidate_list_order_skill_authority"
        };
        var permutationProbes = BuildPermutationProbes(request);
        var selectionInvariant = permutationProbes
            .All(probe => probe.SelectedSkillIds.SequenceEqual(
                permutationProbes[0].SelectedSkillIds,
                StringComparer.Ordinal)
                && probe.RequiresSecondPass == permutationProbes[0].RequiresSecondPass
                && Math.Abs(probe.Confidence - permutationProbes[0].Confidence) < 0.0000001);
        var orderSensitivityDetected = !selectionInvariant;
        var evidence = new[]
        {
            DiagnosticsEvidence,
            SkillSelectionMachine.CandidatePositionScaffoldEvidence,
            "candidate_position_role=diagnostics_provenance_only",
            "candidate_order_guard_status=selection_authority_blocked",
            "candidate_order_permutation_selection_authority=false",
            "candidate_order_probe_source=semantic_units_without_candidate_position",
            $"candidate_order_permutation_count={permutationProbes.Count}",
            $"candidate_order_permutation_selection_invariant={selectionInvariant}",
            $"candidate_order_sensitivity_detected={orderSensitivityDetected}",
            $"candidate_position_profile_count={profileCount}",
            $"candidate_position_weight_total={weightTotal}",
            $"candidate_order_request_candidate_count={request.AvailableSkillIds.Count}",
            $"candidate_position_request_profile_coverage={coveredPositions}"
        };

        return new CandidateOrderLeakageReport(
            UnitId: UnitId,
            SemanticQualityClaimed: false,
            CanSelectSkill: false,
            OrderSensitivityDetected: orderSensitivityDetected,
            PermutationSelectionInvariant: selectionInvariant,
            PermutationCount: permutationProbes.Count,
            CandidatePositionProfileCount: profileCount,
            CandidatePositionWeightTotal: weightTotal,
            RequestCandidateCount: request.AvailableSkillIds.Count,
            CoveredRequestPositions: coveredPositions,
            PermutationProbes: permutationProbes,
            BlockedFeatureIds: blockedFeatures,
            EvidenceFeatures: evidence);
    }

    private IReadOnlyList<CandidateOrderPermutationProbe> BuildPermutationProbes(SkillSelectionRequest request)
    {
        var currentPositions = Enumerable.Range(0, request.AvailableSkillIds.Count).ToArray();
        var reversedPositions = currentPositions.Reverse().ToArray();
        return
        [
            BuildProbe(
                request,
                "candidate_order_probe_current",
                "current",
                currentPositions,
                request.AvailableSkillIds.ToArray()),
            BuildProbe(
                request,
                "candidate_order_probe_reversed",
                "reversed",
                reversedPositions,
                request.AvailableSkillIds.Reverse().ToArray())
        ];
    }

    private CandidateOrderPermutationProbe BuildProbe(
        SkillSelectionRequest request,
        string probeId,
        string orderLabel,
        IReadOnlyList<int> sourceCandidatePositions,
        IReadOnlyList<string> availableSkillIds)
    {
        var probeRequest = request with
        {
            AvailableSkillIds = availableSkillIds
        };
        var decision = _decisionProbe(probeRequest);

        return new CandidateOrderPermutationProbe(
            ProbeId: probeId,
            OrderLabel: orderLabel,
            CandidateCount: sourceCandidatePositions.Count,
            SourceCandidatePositions: sourceCandidatePositions,
            SelectedSkillIds: decision.SelectedSkillIds,
            RequiresSecondPass: decision.RequiresSecondPass,
            Confidence: decision.Confidence);
    }

    public Task<OperationResult<LogicalUnitResult<CandidateOrderLeakageReport>>> ExecuteAsync(
        LogicalUnitRequest<SkillSelectionRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<CandidateOrderLeakageReport>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var report = Evaluate(request.Input);
        var result = new LogicalUnitResult<CandidateOrderLeakageReport>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: report,
            Confidence: 0.0,
            PredicateIds: report.BlockedFeatureIds.ToArray(),
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: report.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "CANDIDATE_POSITION_STATE_IS_DIAGNOSTICS_ONLY",
                "NO_SKILL_ID_MAY_BE_SELECTED_FROM_CANDIDATE_INDEX"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<CandidateOrderLeakageReport>>.Ok(result));
    }
}

public sealed class PromptUnderstandingSkillUnit
    : ILogicalUnit<PromptUnderstandingSkillInput, PromptUnderstandingSkillEvidence>
{
    public const string UnitId = "prompt_understanding_skill_unit";
    public const string Role = "baseline_prompt_feature_to_skill_support";
    public const string UnitExecutedEvidence = "PROMPT_UNDERSTANDING_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "PROMPT_UNDERSTANDING_BASELINE_COUNTER_ONLY";

    public PromptUnderstandingSkillEvidence Evaluate(PromptUnderstandingSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.PromptFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();
        var provenanceCount = profileBySkill.TryGetValue(scored.SkillId, out var selectedProfile)
            ? selectedProfile.SourcePairIds.Count
            : 0;

        return new PromptUnderstandingSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.PromptUnderstandingCheckpointEvidence,
                "prompt_understanding:semantic_quality_claimed=false",
                "prompt_understanding:decision_features=prompt_feature_weights_only",
                $"prompt_understanding:learned_signal_score={scored.Score}",
                $"prompt_understanding:provenance_pair_count={provenanceCount}",
                $"prompt_understanding:vocabulary_size={state.PromptFeatureVocabulary.Count}"
            ],
            LearnedSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.50 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "prompt_understanding_skill_unit selected from typed baseline prompt-feature state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<PromptUnderstandingSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<PromptUnderstandingSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<PromptUnderstandingSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<PromptUnderstandingSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "PROMPT_UNDERSTANDING_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_OR_SOURCE_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<PromptUnderstandingSkillEvidence>>.Ok(result));
    }

    private static PromptUnderstandingSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "prompt_understanding:semantic_quality_claimed=false",
                "prompt_understanding:no_baseline_decision"
            ],
            LearnedSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "PromptUnderstanding baseline unit found no supported skill decision.");
}

public sealed class TaskUnderstandingSkillUnit
    : ILogicalUnit<TaskUnderstandingSkillInput, TaskUnderstandingSkillEvidence>
{
    public const string UnitId = "task_understanding_skill_unit";
    public const string Role = "baseline_task_boundary_feature_to_skill_support";
    public const string UnitExecutedEvidence = "TASK_UNDERSTANDING_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "TASK_UNDERSTANDING_BASELINE_COUNTER_ONLY";

    public TaskUnderstandingSkillEvidence Evaluate(TaskUnderstandingSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.TaskFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();
        var provenanceCount = profileBySkill.TryGetValue(scored.SkillId, out var selectedProfile)
            ? selectedProfile.SourcePairIds.Count
            : 0;

        return new TaskUnderstandingSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.TaskUnderstandingCheckpointEvidence,
                "task_understanding:semantic_quality_claimed=false",
                "task_understanding:decision_features=task_boundary_feature_weights_only",
                $"task_understanding:learned_task_signal_score={scored.Score}",
                $"task_understanding:provenance_pair_count={provenanceCount}",
                $"task_understanding:vocabulary_size={state.TaskFeatureVocabulary.Count}"
            ],
            LearnedTaskSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.52 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "task_understanding_skill_unit selected from typed baseline task-boundary state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<TaskUnderstandingSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<TaskUnderstandingSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskUnderstandingSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<TaskUnderstandingSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "TASK_UNDERSTANDING_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_OR_CANDIDATE_ORDER_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskUnderstandingSkillEvidence>>.Ok(result));
    }

    private static TaskUnderstandingSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "task_understanding:semantic_quality_claimed=false",
                "task_understanding:no_baseline_decision"
            ],
            LearnedTaskSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "TaskUnderstanding baseline unit found no supported skill decision.");
}

public sealed class TaskTypeSkillUnit
    : ILogicalUnit<TaskTypeSkillInput, TaskTypeSkillEvidence>
{
    public const string UnitId = "task_type_skill_unit";
    public const string Role = "baseline_task_type_feature_to_skill_support";
    public const string UnitExecutedEvidence = "TASK_TYPE_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "TASK_TYPE_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<TaskTypeKnnExample> FindNearestTaskTypeExamples(
        IReadOnlyList<string> taskTypeFeatures,
        TaskTypeSkillUnitState state,
        int k)
    {
        var requestFeatures = taskTypeFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.TaskTypeFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.TaskTypeFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public TaskTypeSupport ComputeTaskTypeSupport(
        IReadOnlyList<string> taskTypeFeatures,
        string skillId,
        TaskTypeSkillUnitState state,
        int k)
    {
        var nearest = FindNearestTaskTypeExamples(taskTypeFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.TaskTypeFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(taskTypeFeatures.Contains);

        return new TaskTypeSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public TaskTypeSkillEvidence Evaluate(TaskTypeSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestTaskTypeExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeTaskTypeSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.TaskTypeFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new TaskTypeSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.TaskTypeCheckpointEvidence,
                "task_type:semantic_quality_claimed=false",
                "task_type:decision_features=task_type_feature_weights_only",
                "task_type:knn_role=support_evidence_only",
                $"task_type:learned_task_type_signal_score={scored.Score}",
                $"task_type:nearest_example_count={nearest.Count}",
                $"task_type:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"task_type:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"task_type:vocabulary_size={state.TaskTypeFeatureVocabulary.Count}",
                $"task_type:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedTaskTypeSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.54 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "task_type_skill_unit selected from typed baseline task-type state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<TaskTypeSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<TaskTypeSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskTypeSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<TaskTypeSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "TASK_TYPE_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskTypeSkillEvidence>>.Ok(result));
    }

    private static TaskTypeSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "task_type:semantic_quality_claimed=false",
                "task_type:no_baseline_decision"
            ],
            LearnedTaskTypeSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "TaskType baseline unit found no supported skill decision.");
}

public sealed class RoutingSkillUnit
    : ILogicalUnit<RoutingSkillInput, RoutingSkillEvidence>
{
    public const string UnitId = "routing_skill_unit";
    public const string Role = "baseline_routing_feature_to_skill_support";
    public const string UnitExecutedEvidence = "ROUTING_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "ROUTING_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<RoutingKnnExample> FindNearestRoutingExamples(
        IReadOnlyList<string> routingFeatures,
        RoutingSkillUnitState state,
        int k)
    {
        var requestFeatures = routingFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.RoutingFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.RoutingFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public RoutingSupport ComputeRoutingSupport(
        IReadOnlyList<string> routingFeatures,
        string skillId,
        RoutingSkillUnitState state,
        int k)
    {
        var nearest = FindNearestRoutingExamples(routingFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.RoutingFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(routingFeatures.Contains);

        return new RoutingSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public RoutingSkillEvidence Evaluate(RoutingSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestRoutingExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeRoutingSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.RoutingFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new RoutingSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.RoutingCheckpointEvidence,
                "routing:semantic_quality_claimed=false",
                "routing:decision_features=routing_feature_weights_only",
                "routing:knn_role=support_evidence_only",
                $"routing:learned_routing_signal_score={scored.Score}",
                $"routing:nearest_example_count={nearest.Count}",
                $"routing:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"routing:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"routing:vocabulary_size={state.RoutingFeatureVocabulary.Count}",
                $"routing:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedRoutingSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.56 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "routing_skill_unit selected from typed baseline routing state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<RoutingSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<RoutingSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<RoutingSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<RoutingSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "ROUTING_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<RoutingSkillEvidence>>.Ok(result));
    }

    private static RoutingSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "routing:semantic_quality_claimed=false",
                "routing:no_baseline_decision"
            ],
            LearnedRoutingSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Routing baseline unit found no supported skill decision.");
}

public sealed class PromptContextSkillSelectionUnit
    : ILogicalUnit<PromptContextSkillInput, PromptContextSkillEvidence>
{
    public const string UnitId = "prompt_context_skill_selection_unit";
    public const string Role = "baseline_prompt_context_feature_to_skill_support";
    public const string UnitExecutedEvidence = "PROMPT_CONTEXT_SKILL_SELECTION_UNIT_EXECUTED";
    public const string BaselineEvidence = "PROMPT_CONTEXT_BASELINE_COUNTER_ONLY";
    public const int SelectionScoreFloor = 6;
    public const int StrongSelectionScoreFloor = 10;

    public PromptContextSkillEvidence Evaluate(PromptContextSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptContextSignals(request)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                PromptScore = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals
                        .Where(SkillSelectionMachine.IsPromptContextPromptSignal)
                        .Sum(signal => profile.PromptFeatureWeights.GetValueOrDefault(signal))
                    : 0,
                ContextScore = profileBySkill.TryGetValue(skillId, out var contextProfile)
                    ? requestSignals
                        .Where(SkillSelectionMachine.IsPromptContextContextSignal)
                        .Sum(signal => contextProfile.ContextFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .Select(item => new
            {
                item.SkillId,
                item.PromptScore,
                item.ContextScore,
                Score = item.PromptScore + item.ContextScore * 2
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .ToArray();

        if (scored.Length == 0 || scored[0].Score <= 0 || scored[0].ContextScore <= 0)
            return NoDecision();

        var bestScore = scored[0].Score;
        var selected = scored
            .Where(item => item.Score >= SelectionScoreFloor && item.ContextScore > 0)
            .Where(item => item.PromptScore >= 0 || item.Score >= StrongSelectionScoreFloor)
            .Select(item => item.SkillId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (selected.Length == 0)
        {
            selected = scored
                .Where(item => item.Score == bestScore && item.ContextScore > 0)
                .Where(item => item.PromptScore >= 0 || item.Score >= StrongSelectionScoreFloor)
                .Select(item => item.SkillId)
                .Order(StringComparer.Ordinal)
                .ToArray();
        }
        if (selected.Length == 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !selected.Contains(skillId, StringComparer.Ordinal))
            .ToArray();
        var sourceProfileCount = selected
            .Select(skillId => profileBySkill.TryGetValue(skillId, out var selectedProfile)
                ? selectedProfile.SourcePairIds.Count
                : 0)
            .DefaultIfEmpty(0)
            .Sum();

        return new PromptContextSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: selected,
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.PromptContextCheckpointEvidence,
                "prompt_context:semantic_quality_claimed=false",
                "prompt_context:decision_features=separated_prompt_and_context_feature_weights",
                $"prompt_context:prompt_score={scored[0].PromptScore}",
                $"prompt_context:context_score={scored[0].ContextScore}",
                $"prompt_context:total_score={bestScore}",
                $"prompt_context:source_profile_count={sourceProfileCount}",
                $"prompt_context:prompt_vocabulary_size={state.PromptFeatureVocabulary.Count}",
                $"prompt_context:context_vocabulary_size={state.ContextFeatureVocabulary.Count}"
            ],
            PromptScore: scored[0].PromptScore,
            ContextScore: scored[0].ContextScore,
            TotalScore: bestScore,
            Confidence: Math.Min(0.95, 0.57 + bestScore * 0.05),
            RequiresSecondPass: false,
            DecisionText: "prompt_context_skill_selection_unit selected from typed baseline prompt/context state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<PromptContextSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<PromptContextSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<PromptContextSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<PromptContextSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "PROMPT_CONTEXT_IS_BASELINE_COUNTER_STATE",
                "PROMPT_AND_CONTEXT_FEATURE_WEIGHTS_ARE_SEPARATED",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<PromptContextSkillEvidence>>.Ok(result));
    }

    private static PromptContextSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "prompt_context:semantic_quality_claimed=false",
                "prompt_context:no_baseline_decision"
            ],
            PromptScore: 0,
            ContextScore: 0,
            TotalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "PromptContext baseline unit found no supported skill decision.");
}

public sealed class SkillCandidatesSkillUnit
    : ILogicalUnit<SkillCandidatesSkillInput, SkillCandidatesSkillEvidence>
{
    public const string UnitId = "skill_candidates_skill_unit";
    public const string Role = "baseline_skill_candidate_feature_to_skill_support";
    public const string UnitExecutedEvidence = "SKILL_CANDIDATES_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "SKILL_CANDIDATES_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<SkillCandidatesKnnExample> FindNearestSkillCandidateExamples(
        IReadOnlyList<string> candidateFeatures,
        SkillCandidatesSkillUnitState state,
        int k)
    {
        var requestFeatures = candidateFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.CandidateFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.CandidateFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public SkillCandidatesSupport ComputeSkillCandidateSupport(
        IReadOnlyList<string> candidateFeatures,
        string skillId,
        SkillCandidatesSkillUnitState state,
        int k)
    {
        var nearest = FindNearestSkillCandidateExamples(candidateFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.CandidateFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(candidateFeatures.Contains);

        return new SkillCandidatesSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public SkillCandidatesSkillEvidence Evaluate(SkillCandidatesSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestSkillCandidateExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeSkillCandidateSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.CandidateFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new SkillCandidatesSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.SkillCandidatesCheckpointEvidence,
                "skill_candidates:semantic_quality_claimed=false",
                "skill_candidates:decision_features=candidate_feature_weights_only",
                "skill_candidates:knn_role=support_evidence_only",
                $"skill_candidates:learned_candidate_signal_score={scored.Score}",
                $"skill_candidates:nearest_example_count={nearest.Count}",
                $"skill_candidates:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"skill_candidates:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"skill_candidates:vocabulary_size={state.CandidateFeatureVocabulary.Count}",
                $"skill_candidates:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedCandidateSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.58 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "skill_candidates_skill_unit selected from typed baseline candidate-feature state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<SkillCandidatesSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<SkillCandidatesSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<SkillCandidatesSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<SkillCandidatesSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "SKILL_CANDIDATES_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<SkillCandidatesSkillEvidence>>.Ok(result));
    }

    private static SkillCandidatesSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "skill_candidates:semantic_quality_claimed=false",
                "skill_candidates:no_baseline_decision"
            ],
            LearnedCandidateSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "SkillCandidates baseline unit found no supported skill decision.");
}

public sealed class SkillSelectionSkillUnit
    : ILogicalUnit<SkillSelectionSkillInput, SkillSelectionSkillEvidence>
{
    public const string UnitId = "skill_selection_skill_unit";
    public const string Role = "baseline_final_selection_feature_to_skill_support";
    public const string UnitExecutedEvidence = "SKILL_SELECTION_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "SKILL_SELECTION_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<SkillSelectionKnnExample> FindNearestSkillSelectionExamples(
        IReadOnlyList<string> finalSelectionFeatures,
        SkillSelectionSkillUnitState state,
        int k)
    {
        var requestFeatures = finalSelectionFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.FinalSelectionFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.FinalSelectionFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public SkillSelectionSupport ComputeSkillSelectionSupport(
        IReadOnlyList<string> finalSelectionFeatures,
        string skillId,
        SkillSelectionSkillUnitState state,
        int k)
    {
        var nearest = FindNearestSkillSelectionExamples(finalSelectionFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.FinalSelectionFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(finalSelectionFeatures.Contains);

        return new SkillSelectionSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public SkillSelectionSkillEvidence Evaluate(SkillSelectionSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestSkillSelectionExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeSkillSelectionSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.FinalSelectionFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .ToArray();
        var selected = scored
            .Where(item => item.Score > 0)
            .Select(item => item.SkillId)
            .ToArray();

        if (selected.Length == 0)
            return NoDecision();

        var selectedSet = selected.ToHashSet(StringComparer.Ordinal);
        var rejected = request.AvailableSkillIds
            .Where(skillId => !selectedSet.Contains(skillId))
            .ToArray();
        var maxScore = scored[0].Score;
        var positiveScoreSum = scored
            .Where(item => item.Score > 0)
            .Sum(item => item.Score);
        var selectedSupportScore = selected
            .Sum(skillId => selectedSupport.GetValueOrDefault(skillId)?.SupportScore ?? 0.0);
        var selectedMatchedFeatureCount = selected
            .Sum(skillId => selectedSupport.GetValueOrDefault(skillId)?.MatchedFeatureCount ?? 0);

        return new SkillSelectionSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: selected,
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.SkillSelectionCheckpointEvidence,
                "skill_selection:semantic_quality_claimed=false",
                "skill_selection:decision_features=final_selection_feature_weights_only",
                "skill_selection:knn_role=support_evidence_only",
                $"skill_selection:learned_final_selection_signal_score={maxScore}",
                $"skill_selection:positive_selected_skill_count={selected.Length}",
                $"skill_selection:positive_final_selection_signal_score_sum={positiveScoreSum}",
                $"skill_selection:nearest_example_count={nearest.Count}",
                $"skill_selection:support_score={selectedSupportScore:0.###}",
                $"skill_selection:support_matched_feature_count={selectedMatchedFeatureCount}",
                $"skill_selection:vocabulary_size={state.FinalSelectionFeatureVocabulary.Count}",
                $"skill_selection:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedFinalSelectionSignalScore: maxScore,
            Confidence: Math.Min(0.95, 0.60 + maxScore * 0.05),
            RequiresSecondPass: false,
            DecisionText: "skill_selection_skill_unit selected from typed baseline final-selection state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<SkillSelectionSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<SkillSelectionSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<SkillSelectionSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<SkillSelectionSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "SKILL_SELECTION_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<SkillSelectionSkillEvidence>>.Ok(result));
    }

    private static SkillSelectionSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "skill_selection:semantic_quality_claimed=false",
                "skill_selection:no_baseline_decision"
            ],
            LearnedFinalSelectionSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "SkillSelection baseline unit found no supported final skill decision.");
}

public sealed class ResultGapSkillUnit
    : ILogicalUnit<ResultGapSkillInput, ResultGapSkillEvidence>
{
    public const string UnitId = "result_gap_skill_unit";
    public const string Role = "baseline_result_gap_feature_to_skill_support";
    public const string UnitExecutedEvidence = "RESULT_GAP_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "RESULT_GAP_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<ResultGapKnnExample> FindNearestResultGapExamples(
        IReadOnlyList<string> resultGapFeatures,
        ResultGapSkillUnitState state,
        int k)
    {
        var requestFeatures = resultGapFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.ResultGapFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.ResultGapFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public ResultGapSupport ComputeResultGapSupport(
        IReadOnlyList<string> resultGapFeatures,
        string skillId,
        ResultGapSkillUnitState state,
        int k)
    {
        var nearest = FindNearestResultGapExamples(resultGapFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.ResultGapFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(resultGapFeatures.Contains);

        return new ResultGapSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public ResultGapSkillEvidence Evaluate(ResultGapSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestResultGapExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeResultGapSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.ResultGapFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new ResultGapSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.ResultGapCheckpointEvidence,
                "result_gap:semantic_quality_claimed=false",
                "result_gap:decision_features=result_gap_feature_weights_only",
                "result_gap:knn_role=support_evidence_only",
                $"result_gap:learned_result_gap_signal_score={scored.Score}",
                $"result_gap:nearest_example_count={nearest.Count}",
                $"result_gap:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"result_gap:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"result_gap:vocabulary_size={state.ResultGapFeatureVocabulary.Count}",
                $"result_gap:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedResultGapSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.59 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "result_gap_skill_unit selected from typed baseline result-gap state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<ResultGapSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<ResultGapSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<ResultGapSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<ResultGapSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "RESULT_GAP_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<ResultGapSkillEvidence>>.Ok(result));
    }

    private static ResultGapSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "result_gap:semantic_quality_claimed=false",
                "result_gap:no_baseline_decision"
            ],
            LearnedResultGapSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "ResultGap baseline unit found no supported result-gap skill decision.");
}

public sealed class OutputValidationSkillUnit
    : ILogicalUnit<OutputValidationSkillInput, OutputValidationSkillEvidence>
{
    public const string UnitId = "output_validation_skill_unit";
    public const string Role = "baseline_output_validation_feature_to_skill_support";
    public const string UnitExecutedEvidence = "OUTPUT_VALIDATION_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "OUTPUT_VALIDATION_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<OutputValidationKnnExample> FindNearestOutputValidationExamples(
        IReadOnlyList<string> validationFeatures,
        OutputValidationSkillUnitState state,
        int k)
    {
        var requestFeatures = validationFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.ValidationFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.ValidationFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public OutputValidationSupport ComputeOutputValidationSupport(
        IReadOnlyList<string> validationFeatures,
        string skillId,
        OutputValidationSkillUnitState state,
        int k)
    {
        var nearest = FindNearestOutputValidationExamples(validationFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.ValidationFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(validationFeatures.Contains);

        return new OutputValidationSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public OutputValidationSkillEvidence Evaluate(OutputValidationSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestOutputValidationExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeOutputValidationSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.ValidationFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new OutputValidationSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.OutputValidationCheckpointEvidence,
                "output_validation:semantic_quality_claimed=false",
                "output_validation:decision_features=validation_feature_weights_only",
                "output_validation:knn_role=support_evidence_only",
                $"output_validation:learned_output_validation_signal_score={scored.Score}",
                $"output_validation:nearest_example_count={nearest.Count}",
                $"output_validation:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"output_validation:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"output_validation:vocabulary_size={state.ValidationFeatureVocabulary.Count}",
                $"output_validation:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedOutputValidationSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.58 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "output_validation_skill_unit selected from typed baseline validation state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<OutputValidationSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<OutputValidationSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<OutputValidationSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<OutputValidationSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "OUTPUT_VALIDATION_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<OutputValidationSkillEvidence>>.Ok(result));
    }

    private static OutputValidationSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "output_validation:semantic_quality_claimed=false",
                "output_validation:no_baseline_decision"
            ],
            LearnedOutputValidationSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "OutputValidation baseline unit found no supported validation skill decision.");
}

public sealed class TaskCompletionSkillUnit
    : ILogicalUnit<TaskCompletionSkillInput, TaskCompletionSkillEvidence>
{
    public const string UnitId = "task_completion_skill_unit";
    public const string Role = "baseline_task_completion_feature_to_skill_support";
    public const string UnitExecutedEvidence = "TASK_COMPLETION_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "TASK_COMPLETION_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<TaskCompletionKnnExample> FindNearestTaskCompletionExamples(
        IReadOnlyList<string> completionFeatures,
        TaskCompletionSkillUnitState state,
        int k)
    {
        var requestFeatures = completionFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.CompletionFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.CompletionFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public TaskCompletionSupport ComputeTaskCompletionSupport(
        IReadOnlyList<string> completionFeatures,
        string skillId,
        TaskCompletionSkillUnitState state,
        int k)
    {
        var nearest = FindNearestTaskCompletionExamples(completionFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.CompletionFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(completionFeatures.Contains);

        return new TaskCompletionSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public TaskCompletionSkillEvidence Evaluate(TaskCompletionSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestTaskCompletionExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeTaskCompletionSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.CompletionFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new TaskCompletionSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.TaskCompletionCheckpointEvidence,
                "task_completion:semantic_quality_claimed=false",
                "task_completion:decision_features=completion_feature_weights_only",
                "task_completion:knn_role=support_evidence_only",
                $"task_completion:learned_task_completion_signal_score={scored.Score}",
                $"task_completion:nearest_example_count={nearest.Count}",
                $"task_completion:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"task_completion:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"task_completion:vocabulary_size={state.CompletionFeatureVocabulary.Count}",
                $"task_completion:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedTaskCompletionSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.58 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "task_completion_skill_unit selected from typed baseline completion state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<TaskCompletionSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<TaskCompletionSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskCompletionSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<TaskCompletionSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "TASK_COMPLETION_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskCompletionSkillEvidence>>.Ok(result));
    }

    private static TaskCompletionSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "task_completion:semantic_quality_claimed=false",
                "task_completion:no_baseline_decision"
            ],
            LearnedTaskCompletionSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "TaskCompletion baseline unit found no supported completion skill decision.");
}

public sealed class IntermediateChecksSkillUnit
    : ILogicalUnit<IntermediateChecksSkillInput, IntermediateChecksSkillEvidence>
{
    public const string UnitId = "intermediate_checks_skill_unit";
    public const string Role = "baseline_intermediate_check_feature_to_skill_support";
    public const string UnitExecutedEvidence = "INTERMEDIATE_CHECKS_SKILL_UNIT_EXECUTED";
    public const string BaselineEvidence = "INTERMEDIATE_CHECKS_BASELINE_COUNTER_ONLY";

    public IReadOnlyList<IntermediateChecksKnnExample> FindNearestCheckExamples(
        IReadOnlyList<string> checkFeatures,
        IntermediateChecksSkillUnitState state,
        int k)
    {
        var requestFeatures = checkFeatures.ToHashSet(StringComparer.Ordinal);
        if (requestFeatures.Count == 0 || k <= 0)
            return [];

        return state.KnnMemory
            .Select(example => new
            {
                Example = example,
                MatchedFeatureCount = example.CheckFeatures.Count(requestFeatures.Contains),
                UnionFeatureCount = example.CheckFeatures.Concat(requestFeatures).Distinct(StringComparer.Ordinal).Count()
            })
            .Where(item => item.MatchedFeatureCount > 0)
            .OrderByDescending(item => (double)item.MatchedFeatureCount / Math.Max(1, item.UnionFeatureCount))
            .ThenByDescending(item => item.MatchedFeatureCount)
            .ThenBy(item => item.Example.ChosenSkillId, StringComparer.Ordinal)
            .Take(k)
            .Select(item => item.Example)
            .ToArray();
    }

    public IntermediateChecksSupport ComputeCheckSupport(
        IReadOnlyList<string> checkFeatures,
        string skillId,
        IntermediateChecksSkillUnitState state,
        int k)
    {
        var nearest = FindNearestCheckExamples(checkFeatures, state, k)
            .Where(example => string.Equals(example.ChosenSkillId, skillId, StringComparison.Ordinal))
            .ToArray();
        var matchedFeatureCount = nearest
            .SelectMany(example => example.CheckFeatures)
            .Distinct(StringComparer.Ordinal)
            .Count(checkFeatures.Contains);

        return new IntermediateChecksSupport(
            SkillId: skillId,
            SupportScore: nearest.Sum(example => example.Weight),
            MatchedFeatureCount: matchedFeatureCount,
            NearestExampleCount: nearest.Length);
    }

    public IntermediateChecksSkillEvidence Evaluate(IntermediateChecksSkillInput input)
    {
        var request = input.Request;
        var state = input.State;
        if (state.SkillProfiles.Count == 0 || request.AvailableSkillIds.Count == 0)
            return NoDecision();

        var requestSignals = SkillSelectionMachine.ExtractPromptUnderstandingSignals(request.Prompt)
            .ToHashSet(StringComparer.Ordinal);
        if (requestSignals.Count == 0)
            return NoDecision();

        var nearest = FindNearestCheckExamples(requestSignals.ToArray(), state, 3);
        var selectedSupport = request.AvailableSkillIds
            .Select(skillId => ComputeCheckSupport(requestSignals.ToArray(), skillId, state, 3))
            .ToDictionary(support => support.SkillId, StringComparer.Ordinal);
        var profileBySkill = state.SkillProfiles.ToDictionary(profile => profile.SkillId, StringComparer.Ordinal);
        var scored = request.AvailableSkillIds
            .Select(skillId => new
            {
                SkillId = skillId,
                Score = profileBySkill.TryGetValue(skillId, out var profile)
                    ? requestSignals.Sum(signal => profile.CheckFeatureWeights.GetValueOrDefault(signal))
                    : 0
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .First();

        if (scored.Score <= 0)
            return NoDecision();

        var rejected = request.AvailableSkillIds
            .Where(skillId => !string.Equals(skillId, scored.SkillId, StringComparison.Ordinal))
            .ToArray();

        return new IntermediateChecksSkillEvidence(
            UnitId: UnitId,
            HasDecision: true,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [scored.SkillId],
            RejectedSkillIds: rejected,
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                SkillSelectionMachine.IntermediateChecksCheckpointEvidence,
                "intermediate_checks:semantic_quality_claimed=false",
                "intermediate_checks:decision_features=check_feature_weights_only",
                "intermediate_checks:knn_role=support_evidence_only",
                $"intermediate_checks:learned_check_signal_score={scored.Score}",
                $"intermediate_checks:nearest_example_count={nearest.Count}",
                $"intermediate_checks:support_score={selectedSupport.GetValueOrDefault(scored.SkillId)?.SupportScore ?? 0.0:0.###}",
                $"intermediate_checks:support_matched_feature_count={selectedSupport.GetValueOrDefault(scored.SkillId)?.MatchedFeatureCount ?? 0}",
                $"intermediate_checks:vocabulary_size={state.CheckFeatureVocabulary.Count}",
                $"intermediate_checks:skill_profile_count={state.SkillProfiles.Count}"
            ],
            LearnedCheckSignalScore: scored.Score,
            Confidence: Math.Min(0.95, 0.58 + scored.Score * 0.05),
            RequiresSecondPass: false,
            DecisionText: "intermediate_checks_skill_unit selected from typed baseline check-feature state; semantic_quality_claimed=false.");
    }

    public Task<OperationResult<LogicalUnitResult<IntermediateChecksSkillEvidence>>> ExecuteAsync(
        LogicalUnitRequest<IntermediateChecksSkillInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<LogicalUnitResult<IntermediateChecksSkillEvidence>>.Fail(
                    "Unit cancelled.",
                    "CANCELLED"));
        }

        var evidence = Evaluate(request.Input);
        var result = new LogicalUnitResult<IntermediateChecksSkillEvidence>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: evidence,
            Confidence: evidence.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.EvidenceFeatures.ToArray(),
            Warnings:
            [
                "INTERMEDIATE_CHECKS_IS_BASELINE_COUNTER_STATE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_EXACT_PROMPT_SOURCE_PATH_CANDIDATE_ORDER_OR_PAIR_ID_FEATURE_AUTHORITY"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<IntermediateChecksSkillEvidence>>.Ok(result));
    }

    private static IntermediateChecksSkillEvidence NoDecision()
        => new(
            UnitId: UnitId,
            HasDecision: false,
            SemanticQualityClaimed: false,
            BaselineCounterOnly: true,
            SelectedSkillIds: [],
            RejectedSkillIds: [],
            EvidenceFeatures:
            [
                UnitExecutedEvidence,
                BaselineEvidence,
                "intermediate_checks:semantic_quality_claimed=false",
                "intermediate_checks:no_baseline_decision"
            ],
            LearnedCheckSignalScore: 0,
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "IntermediateChecks baseline unit found no supported skill decision.");
}

public abstract class SkillSelectionLogicalUnitBase(
    SkillSelectionMachine machine,
    string unitId,
    string unitRole) : ILogicalUnit<SkillSelectionRequest, SkillSelectionDecision>
{
    protected SkillSelectionMachine Machine { get; } = machine;

    public Task<OperationResult<LogicalUnitResult<SkillSelectionDecision>>> ExecuteAsync(
        LogicalUnitRequest<SkillSelectionRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<SkillSelectionDecision>>.Fail("Unit cancelled.", "CANCELLED"));

        var decided = Machine.DecideFromTrainedState(request.Input);
        if (!decided.IsSuccess)
            return Task.FromResult(decided.MapError<LogicalUnitResult<SkillSelectionDecision>>());

        var evidence = new List<string>
        {
            "SKILL_SELECTION_LOGICAL_UNIT_SOURCE_BACKED",
            $"SKILL_SELECTION_UNIT_ROLE:{unitRole}",
            SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        evidence.AddRange(decided.Value!.EvidenceFeatures);
        evidence.AddRange(AdditionalEvidence());

        var result = new LogicalUnitResult<SkillSelectionDecision>(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: SkillSelectionMachine.DomainTag,
            Output: decided.Value,
            Confidence: decided.Value.Confidence,
            PredicateIds: [],
            DomainTags: [SkillSelectionMachine.DomainTag],
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings:
            [
                "SKILL_SELECTION_LOGICAL_UNIT_DELEGATES_DECIDE_FROM_TRAINED_STATE",
                "NO_ADDITIONAL_SKILL_RANKING_OR_STATIC_ROUTE_TABLE"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<SkillSelectionDecision>>.Ok(result));
    }

    protected virtual IReadOnlyList<string> AdditionalEvidence() => [];
}
