// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public sealed class OrchestrationReasoningLearningMachineAdapter : ILearningMachine<OrchestrationReasoningDomain>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private const int LogicalCheckpointSchemaVersion = 2;

    private readonly OrchestrationReasoningMachine _machine;
    private readonly string? _checkpointPath;
    private readonly string? _manifestPath;
    private readonly string _sourceManifestRef;
    private readonly List<string> _acceptedRefIds;
    private readonly List<OrchestrationReasoningLearnedPattern> _trainedPatterns = [];
    private OrchestrationReasoningAdmittedLearnedState? _trainedAdmittedState;
    private double _lastEvaluationAccuracy;

    public OrchestrationReasoningLearningMachineAdapter(
        OrchestrationReasoningMachine? machine = null,
        string? checkpointPath = null,
        string? manifestPath = null,
        string sourceManifestRef = "artifacts/orchestration-reasoning-corpus-summary.json",
        IReadOnlyList<string>? acceptedRefIds = null)
    {
        _machine = machine ?? new OrchestrationReasoningMachine();
        _checkpointPath = checkpointPath;
        _manifestPath = manifestPath;
        _sourceManifestRef = sourceManifestRef;
        _acceptedRefIds = acceptedRefIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList() ?? [];
    }

    public async Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<OrchestrationReasoningDomain> batch,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        if (ct.IsCancellationRequested)
            return OperationResult<LearningBatchResult>.Fail("Training cancelled.", "CANCELLED");
        if (!string.Equals(batch.DomainTag, OrchestrationReasoningMachine.DomainTag, StringComparison.Ordinal))
            return OperationResult<LearningBatchResult>.Fail(
                "OrchestrationReasoning training batch domain tag mismatch.",
                "ORCHESTRATION_REASONING_TRAINING_DOMAIN_MISMATCH");
        if (batch.TrainingItems is null || batch.TrainingItems.Count == 0)
            return OperationResult<LearningBatchResult>.Fail(
                "OrchestrationReasoning training requires at least one learned pattern item.",
                "ORCHESTRATION_REASONING_TRAINING_ITEMS_REQUIRED");

        var beforePatternCount = _trainedPatterns.Count;
        var importedPatternIds = new HashSet<string>(_trainedPatterns.Select(pattern => pattern.PatternId), StringComparer.Ordinal);
        var importedPatterns = new List<OrchestrationReasoningLearnedPattern>();
        var admittedStates = new List<OrchestrationReasoningAdmittedLearnedState>();
        foreach (var item in batch.TrainingItems)
        {
            var parsed = ParseAdmittedState(item.ChosenResponse);
            if (!parsed.IsSuccess)
                return OperationResult<LearningBatchResult>.Fail(parsed.ErrorMessage, parsed.ErrorCode);

            admittedStates.Add(parsed.Value!);
            foreach (var pattern in parsed.Value!.State.LearnedPatterns!)
            {
                if (string.IsNullOrWhiteSpace(pattern.PatternId))
                    return OperationResult<LearningBatchResult>.Fail(
                        "OrchestrationReasoning learned pattern id is required.",
                        "ORCHESTRATION_REASONING_TRAINING_PATTERN_ID_REQUIRED");
                if (!importedPatternIds.Add(pattern.PatternId))
                    return OperationResult<LearningBatchResult>.Fail(
                        $"OrchestrationReasoning learned pattern '{pattern.PatternId}' is duplicated.",
                        "ORCHESTRATION_REASONING_TRAINING_DUPLICATE_PATTERN");

                importedPatterns.Add(pattern);
            }

            if (!string.IsNullOrWhiteSpace(item.ItemId))
                _acceptedRefIds.Add(item.ItemId);
        }

        _trainedPatterns.AddRange(importedPatterns);
        foreach (var sourceId in batch.SourceIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            _acceptedRefIds.Add(sourceId);

        var normalizedAcceptedRefs = _acceptedRefIds
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        _acceptedRefIds.Clear();
        _acceptedRefIds.AddRange(normalizedAcceptedRefs);

        var mergedAdmittedState = _trainedAdmittedState;
        foreach (var admittedState in admittedStates)
        {
            if (mergedAdmittedState is null)
            {
                mergedAdmittedState = admittedState;
                continue;
            }

            var merged = OrchestrationReasoningSourceAdmission.Merge(
                mergedAdmittedState,
                admittedState,
                $"orchestration-reasoning-logical-state:{batch.BatchId}",
                $"common://orchestration-reasoning/logical-adapter/{batch.BatchId}",
                "common-orchestration-reasoning-source-admitted");
            if (!merged.IsSuccess)
                return merged.MapError<LearningBatchResult>();
            mergedAdmittedState = merged.Value;
        }

        if (mergedAdmittedState is null)
            return OperationResult<LearningBatchResult>.Fail(
                "OrchestrationReasoning training requires admitted learned state evidence.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode);
        _trainedAdmittedState = mergedAdmittedState;
        var imported = _machine.ImportAdmittedState(mergedAdmittedState);
        if (!imported.IsSuccess)
            return imported.MapError<LearningBatchResult>();

        await Task.CompletedTask;
        stopwatch.Stop();
        var afterPatternCount = _trainedPatterns.Count;
        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: importedPatterns.Count,
            AccuracyBefore: _lastEvaluationAccuracy,
            AccuracyAfter: afterPatternCount > beforePatternCount ? _lastEvaluationAccuracy : _lastEvaluationAccuracy,
            RewardMargin: 0.0,
            CpuMs: stopwatch.ElapsedMilliseconds,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024,
            ElapsedMs: stopwatch.ElapsedMilliseconds,
            PromotedUnitIds: []);

        return OperationResult<LearningBatchResult>.Ok(result);
    }

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<OrchestrationReasoningDomain> evalSet,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail("Evaluation cancelled.", "CANCELLED"));
        if (!string.Equals(evalSet.DomainTag, OrchestrationReasoningMachine.DomainTag, StringComparison.Ordinal))
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(
                "OrchestrationReasoning evaluation set domain tag mismatch.",
                "ORCHESTRATION_REASONING_EVALUATION_DOMAIN_MISMATCH"));

        if (evalSet.EvaluationItems is null || evalSet.EvaluationItems.Count == 0)
            return Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(new LearningEvaluationResult(
                EvaluationId: evalSet.EvaluationId,
                RowsEvaluated: 0,
                Accuracy: 0.0,
                RewardMargin: 0.0,
                PassesGate: false,
                GateNote: "OrchestrationReasoning evaluation set is empty; no semantic quality is claimed.")));

        var correct = 0;
        foreach (var item in evalSet.EvaluationItems)
        {
            var parsed = ParseRequest(item.Prompt);
            if (!parsed.IsSuccess)
                return Task.FromResult(OperationResult<LearningEvaluationResult>.Fail(parsed.ErrorMessage, parsed.ErrorCode));

            var planned = _machine.Plan(parsed.Value!);
            if (!planned.IsSuccess)
                return Task.FromResult(planned.MapError<LearningEvaluationResult>());

            var expected = item.ExpectedResponse?.Trim() ?? string.Empty;
            if (string.Equals(planned.Value!.ScenarioPlan.ScenarioId, expected, StringComparison.Ordinal) ||
                planned.Value.ContractEvidence.Contains(expected, StringComparer.Ordinal))
            {
                correct++;
            }
        }

        _lastEvaluationAccuracy = correct / (double)evalSet.EvaluationItems.Count;
        return Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: _lastEvaluationAccuracy,
            RewardMargin: _lastEvaluationAccuracy,
            PassesGate: _lastEvaluationAccuracy >= 0.8,
            GateNote: "OrchestrationReasoning evaluation uses learned guide-state planning; semantic quality remains unclaimed until broader gates approve it.")));
    }

    public async Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_checkpointPath) || string.IsNullOrWhiteSpace(_manifestPath))
        {
            return OperationResult<LearningMachineExport>.Fail(
                "OrchestrationReasoning logical adapter export requires checkpoint and manifest paths.",
                "ORCHESTRATION_REASONING_LOGICAL_EXPORT_PATHS_REQUIRED");
        }

        var export = await _machine.SaveCheckpointAsync(
            _checkpointPath,
            _manifestPath,
            _trainedAdmittedState?.State.ManifestId ?? _sourceManifestRef,
            _trainedAdmittedState?.Receipt.PairIds ?? _acceptedRefIds,
            ct);
        if (!export.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(export.ErrorMessage, export.ErrorCode);

        return OperationResult<LearningMachineExport>.Ok(new LearningMachineExport(
            MachineId: export.Value!.MachineId,
            DomainTag: export.Value.DomainTag,
            Version: export.Value.Version,
            CheckpointRef: export.Value.CheckpointRef,
            ManifestRef: export.Value.ManifestRef,
            AccuracyAtExport: _lastEvaluationAccuracy,
            ExportedAtUtc: export.Value.ExportedAtUtc));
    }

    public async Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");
        if (export is null)
            return OperationResult<bool>.Fail("Export is required.", "ORCHESTRATION_REASONING_EXPORT_REQUIRED");
        if (!string.Equals(export.MachineId, OrchestrationReasoningMachine.MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "ORCHESTRATION_REASONING_MACHINE_ID_MISMATCH");
        if (!string.Equals(export.DomainTag, OrchestrationReasoningMachine.DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Domain tag mismatch.", "ORCHESTRATION_REASONING_DOMAIN_TAG_MISMATCH");

        var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(export.CheckpointRef, export.ManifestRef, ct);
        if (!loaded.IsSuccess)
            return OperationResult<bool>.Fail(loaded.ErrorMessage, loaded.ErrorCode);

        var admitted = loaded.Value!.Machine.GetVerifiedAdmittedState();
        return admitted is null
            ? OperationResult<bool>.Fail(
                "Loaded checkpoint did not produce runtime admission authority.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode)
            : _machine.ImportAdmittedState(admitted);
    }

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
        => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
            new OrchestrationReasoningLogicalOrchestrator(new OrchestrationReasoningLogicalFlow(_machine))));

    private static OperationResult<OrchestrationReasoningAdmittedLearnedState> ParseAdmittedState(
        string chosenResponse)
    {
        if (string.IsNullOrWhiteSpace(chosenResponse))
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "OrchestrationReasoning training item chosen response must contain an admitted learned-state envelope.",
                "ORCHESTRATION_REASONING_TRAINING_CHOSEN_RESPONSE_REQUIRED");

        return OrchestrationReasoningSourceAdmission.ResolveRuntimeReference(chosenResponse);
    }

    private static OperationResult<OrchestrationReasoningRequest> ParseRequest(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<OrchestrationReasoningRequest>.Fail(
                "OrchestrationReasoning evaluation prompt must contain request JSON.",
                "ORCHESTRATION_REASONING_EVALUATION_REQUEST_REQUIRED");

        try
        {
            var request = JsonSerializer.Deserialize<OrchestrationReasoningRequest>(prompt, JsonOptions);
            if (request is null)
                return OperationResult<OrchestrationReasoningRequest>.Fail(
                    "OrchestrationReasoning evaluation request JSON parsed to null.",
                    "ORCHESTRATION_REASONING_EVALUATION_REQUEST_PARSE_FAILED");

            return OperationResult<OrchestrationReasoningRequest>.Ok(request);
        }
        catch (JsonException ex)
        {
            return OperationResult<OrchestrationReasoningRequest>.Fail(
                $"OrchestrationReasoning evaluation prompt is not valid request JSON: {ex.Message}",
                "ORCHESTRATION_REASONING_EVALUATION_REQUEST_JSON_INVALID");
        }
    }

}

public sealed class OrchestrationReasoningLogicalOrchestrator(OrchestrationReasoningLogicalFlow flow) : ILogicalOrchestrator
{
    public const string RequiredMetadataKey = "orchestration_reasoning_request";

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Metadata.TryGetValue(RequiredMetadataKey, out var value) || value is not OrchestrationReasoningRequest reasoningRequest)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                $"OrchestrationReasoning logical orchestrator requires metadata '{RequiredMetadataKey}' containing an OrchestrationReasoningRequest.",
                "ORCHESTRATION_REASONING_REQUEST_METADATA_REQUIRED");
        }

        var flowRequest = new LogicalFlowRequest<OrchestrationReasoningRequest>(
            FlowId: OrchestrationReasoningLogicalFlow.FlowId,
            ScenarioId: request.ScenarioId,
            RunId: request.RunId,
            Input: reasoningRequest,
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

public sealed class OrchestrationReasoningLogicalFlow(OrchestrationReasoningMachine machine) : ILogicalFlow<OrchestrationReasoningRequest>
{
    public const string FlowId = "orchestration-reasoning-logical-hierarchy";

    private readonly IReadOnlyList<ILogicalUnit<OrchestrationReasoningRequest, OrchestrationReasoningResponse>> _units =
    [
        new OrchestrationReasoningScenarioPlanningUnit(machine),
        new OrchestrationReasoningGateFamilyUnit(machine),
        new OrchestrationReasoningRiskPolicyUnit(machine),
        new OrchestrationReasoningAuthorityBoundaryUnit(machine)
    ];

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<OrchestrationReasoningRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Flow cancelled.", "CANCELLED");

        var evidence = new List<string>
        {
            "ORCHESTRATION_REASONING_LOGICAL_FLOW_SOURCE_BACKED",
            "ORCHESTRATION_REASONING_PLAN_DELEGATED_TO_MACHINE",
            "NO_ADDITIONAL_DETERMINISTIC_SEMANTIC_SHORTCUT",
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        var warnings = new List<string>
        {
            "CM26B_LOGICAL_HIERARCHY_ADAPTER_ONLY",
            "NO_TRAINING_OR_SEMANTIC_QUALITY_GATE_RUN"
        };

        OrchestrationReasoningResponse? finalResponse = null;
        foreach (var unit in _units)
        {
            var unitResult = await unit.ExecuteAsync(new LogicalUnitRequest<OrchestrationReasoningRequest>(
                RunId: request.RunId,
                UnitId: GetUnitId(unit),
                UnitRole: GetUnitRole(unit),
                DomainTag: OrchestrationReasoningMachine.DomainTag,
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
            SummaryId: $"{request.RunId}-orchestration-reasoning-logical-summary",
            Tier: 0,
            BaselinePrimary: 0.0,
            EnhancedPrimary: 0.0,
            PrimaryMetricName: "semantic_quality_claimed",
            LowerIsBetter: false,
            Passed: false,
            ShouldScaleUp: false,
            NextAction: "Run OrchestrationReasoning trainable semantic quality gates before any semantic completion claim.",
            ScalingSignal: 0.0,
            ScalingSignalNote: "CM-26B source hierarchy evidence only.",
            Notes: "Logical hierarchy delegates to OrchestrationReasoningMachine.Plan and does not add semantic authority.",
            BaselineTimeMs: 0.0,
            EnhancedTimeMs: 0.0);

        var result = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: request.ScenarioId,
            OverallVerdict: finalResponse?.ContractStatus ?? OrchestrationReasoningMachine.NotTrainedStatus,
            FinalHumanResult: finalResponse?.ScenarioPlan.ScenarioId ?? OrchestrationReasoningMachine.NoInferenceScenarioId,
            Summary: summary,
            Metrics: new Dictionary<string, string>
            {
                ["logical_unit_count"] = _units.Count.ToString(),
                ["semantic_quality_claimed"] = bool.FalseString,
                ["training_run"] = bool.FalseString
            },
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray());

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    private static string GetUnitId(ILogicalUnit<OrchestrationReasoningRequest, OrchestrationReasoningResponse> unit)
        => unit switch
        {
            OrchestrationReasoningScenarioPlanningUnit => OrchestrationReasoningScenarioPlanningUnit.UnitId,
            OrchestrationReasoningGateFamilyUnit => OrchestrationReasoningGateFamilyUnit.UnitId,
            OrchestrationReasoningRiskPolicyUnit => OrchestrationReasoningRiskPolicyUnit.UnitId,
            OrchestrationReasoningAuthorityBoundaryUnit => OrchestrationReasoningAuthorityBoundaryUnit.UnitId,
            _ => "orchestration_reasoning_unknown"
        };

    private static string GetUnitRole(ILogicalUnit<OrchestrationReasoningRequest, OrchestrationReasoningResponse> unit)
        => unit switch
        {
            OrchestrationReasoningScenarioPlanningUnit => OrchestrationReasoningScenarioPlanningUnit.Role,
            OrchestrationReasoningGateFamilyUnit => OrchestrationReasoningGateFamilyUnit.Role,
            OrchestrationReasoningRiskPolicyUnit => OrchestrationReasoningRiskPolicyUnit.Role,
            OrchestrationReasoningAuthorityBoundaryUnit => OrchestrationReasoningAuthorityBoundaryUnit.Role,
            _ => "unknown"
        };
}

public sealed class OrchestrationReasoningScenarioPlanningUnit(OrchestrationReasoningMachine machine)
    : OrchestrationReasoningLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "orchestration_reasoning_scenario_planning_unit";
    public const string Role = "scenario_planning";
}

public sealed class OrchestrationReasoningGateFamilyUnit(OrchestrationReasoningMachine machine)
    : OrchestrationReasoningLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "orchestration_reasoning_gate_family_unit";
    public const string Role = "gate_family_planning";
}

public sealed class OrchestrationReasoningRiskPolicyUnit(OrchestrationReasoningMachine machine)
    : OrchestrationReasoningLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "orchestration_reasoning_risk_policy_unit";
    public const string Role = "risk_policy_review";
}

public sealed class OrchestrationReasoningAuthorityBoundaryUnit(OrchestrationReasoningMachine machine)
    : OrchestrationReasoningLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "orchestration_reasoning_authority_boundary_unit";
    public const string Role = "authority_boundary_check";
}

public abstract class OrchestrationReasoningLogicalUnitBase(
    OrchestrationReasoningMachine machine,
    string unitId,
    string unitRole) : ILogicalUnit<OrchestrationReasoningRequest, OrchestrationReasoningResponse>
{
    public Task<OperationResult<LogicalUnitResult<OrchestrationReasoningResponse>>> ExecuteAsync(
        LogicalUnitRequest<OrchestrationReasoningRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<OrchestrationReasoningResponse>>.Fail("Unit cancelled.", "CANCELLED"));

        var planned = machine.Plan(request.Input);
        if (!planned.IsSuccess)
            return Task.FromResult(planned.MapError<LogicalUnitResult<OrchestrationReasoningResponse>>());

        var result = new LogicalUnitResult<OrchestrationReasoningResponse>(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: OrchestrationReasoningMachine.DomainTag,
            Output: planned.Value!,
            Confidence: 0.0,
            PredicateIds: [],
            DomainTags: [OrchestrationReasoningMachine.DomainTag],
            EvidenceIds:
            [
                "ORCHESTRATION_REASONING_LOGICAL_UNIT_SOURCE_BACKED",
                $"ORCHESTRATION_REASONING_UNIT_ROLE:{unitRole}",
                planned.Value!.ContractStatus,
                .. planned.Value.ContractEvidence
            ],
            Warnings:
            [
                "ORCHESTRATION_REASONING_LOGICAL_UNIT_DELEGATES_PLAN",
                "NO_ADDITIONAL_SCENARIO_RANKING_OR_TEMPLATE_SELECTION"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<OrchestrationReasoningResponse>>.Ok(result));
    }
}
